using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerStateManager : MonoBehaviour
{
    private const string SAVE_FILE_NAME = "player_state.dat";
    private const int CURRENT_SAVE_VERSION = 1;
    
    public static PlayerStateManager Instance { get; private set; }
    
    private PlayerController playerController;
    private PersonManager personManager;
    private PlayerLocationState currentLocation;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // 获取必要的引用
        playerController = FindObjectOfType<PlayerController>();
        personManager = PersonManager.Instance;
    }
    
    /// <summary>
    /// 保存玩家状态
    /// </summary>
    public void SavePlayerState()
    {
        PlayerStateData saveData = new PlayerStateData();
        
        // 保存玩家位置和旋转
        if (playerController != null)
        {
            Transform playerTransform = playerController.transform;

            // 检查玩家是否在船上
            saveData.isOnRaft = playerController.IsOnRaft();

            if (!saveData.isOnRaft)
            {
                // 不在船上，正常保存位置
                saveData.playerPosition = playerTransform.position;
                saveData.playerRotation = playerTransform.rotation;
            }
            else
            {
                // 在船上时，保存特殊标记（zero表示需要重生到安全点）
                saveData.playerPosition = Vector3.zero;
                saveData.playerRotation = Quaternion.identity;
                Debug.Log("[PlayerStateManager] 玩家在船上退出，保存重生标记");
            }
        }
        
        saveData.currentIsland = currentLocation;
        
        // 保存队友信息
        if (personManager != null)
        {
            List<PersonScriptableObject> allPersons = personManager.GetAllPersons();
            foreach (var person in allPersons)
            {
                if (person.recruited) // 只保存已招募的人员
                {
                    TeamMemberData memberData = new TeamMemberData(
                        person.personName,
                        person.profession.ToString(),
                        person.recruited,
                        person.status.ToString()
                    );
                    saveData.teamMembers.Add(memberData);
                }
            }
        }
        
        // 序列化并保存
        string json = JsonUtility.ToJson(saveData, true);
        string fullPath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        
        File.WriteAllText(fullPath, json);
        Debug.Log($"玩家状态已保存到: {fullPath}");
    }
    
    /// <summary>
    /// 加载玩家状态
    /// </summary>
    public bool LoadPlayerState()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        
        if (!File.Exists(fullPath))
        {
            Debug.Log("未找到玩家状态存档，使用初始状态");
            return false;
        }
        
        try
        {
            string json = File.ReadAllText(fullPath);
            PlayerStateData saveData = JsonUtility.FromJson<PlayerStateData>(json);
            
            if (saveData == null)
            {
                Debug.LogError("反序列化玩家状态数据失败。JSON可能已损坏或为空。");
                return false;
            }
            
            // 处理不同版本的存档
            if (saveData.saveVersion < CURRENT_SAVE_VERSION)
            {
                Debug.Log($"升级存档版本: {saveData.saveVersion} -> {CURRENT_SAVE_VERSION}");
                // 这里可以添加版本升级逻辑
            }
            
            currentLocation = saveData.currentIsland;
            
            if (playerController != null)
            {
                Transform playerTransform = playerController.transform;
                playerTransform.position = saveData.playerPosition;
                playerTransform.rotation = saveData.playerRotation;
            }
            
            // 加载队友信息
            if (personManager != null && saveData.teamMembers != null)
            {
                foreach (var memberData in saveData.teamMembers)
                {
                    PersonScriptableObject person = personManager.FindPersonByName(memberData.personName);
                    if (person != null)
                    {
                        // 更新人员招募状态
                        person.recruited = memberData.isRecruited;
                        
                        // 更新人员状态
                        if (Enum.TryParse(memberData.status, out PersonStatus status))
                        {
                            person.status = status;
                        }
                        
                        // 通知PersonsLoader更新状态
                        PersonsLoader.Instance?.UpdatePersonRecruitment(person, memberData.isRecruited);
                        PersonsLoader.Instance?.UpdatePersonStatus(person, status);
                    }
                }
            }
            
            // 如果玩家在船上，需要特殊处理
            if (saveData.isOnRaft)
            {
                // 传送到安全点（因为船会被销毁）
                if (VoyageSystemManager.Instance != null)
                {
                    Transform respawnPoint = VoyageSystemManager.Instance.GetSafeRespawnPoint();
                    if (respawnPoint != null && playerController != null)
                    {
                        playerController.transform.position = respawnPoint.position;
                        playerController.transform.rotation = respawnPoint.rotation;
                        Debug.Log("[PlayerStateManager] 玩家之前在船上，已传送到安全点");
                    }
                    else if (playerController != null)
                    {
                        // 如果没有配置安全点，保持当前位置
                        Debug.LogWarning("[PlayerStateManager] VoyageSystemManager的安全点未配置，玩家位置未重置");
                    }
                }
                else if (playerController != null)
                {
                    // 如果VoyageSystemManager不存在，保持原位置不变
                    Debug.LogWarning("[PlayerStateManager] VoyageSystemManager不存在，玩家位置未重置");
                }

                // 清空VoyageSystemManager的船只状态
                VoyageSystemManager.Instance?.ClearCurrentRaft();
            }
            else
            {
                // 不在船上，正常恢复位置
                if (playerController != null)
                {
                    Transform playerTransform = playerController.transform;
                    playerTransform.position = saveData.playerPosition;
                    playerTransform.rotation = saveData.playerRotation;
                }
            }
            
            Debug.Log("成功加载玩家状态");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"加载玩家状态失败: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 检查存档是否存在
    /// </summary>
    public bool SaveExists()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        return File.Exists(fullPath);
    }
    
    /// <summary>
    /// 删除存档文件
    /// </summary>
    public void DeleteSaveFile()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            Debug.Log($"玩家状态存档已删除: {fullPath}");
        }
        else
        {
            Debug.Log("没有找到玩家状态存档文件，无需删除");
        }
    }
    
    public PlayerLocationState CurrentLocation => currentLocation;
    
    public void SetCurrentLocation(PlayerLocationState location)
    {
        currentLocation = location;
    }
    
    /// <summary>
    /// 获取玩家当前所在的船
    /// </summary>
    private RaftController GetPlayerCurrentRaft()
    {
        if (playerController == null) return null;
        
        // 通过反射获取私有字段currentRaft
        var field = typeof(PlayerController).GetField("currentRaft", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            return (RaftController)field.GetValue(playerController);
        }
        
        return null;
    }
    
    /// <summary>
    /// 根据名称查找船只
    /// </summary>
    private RaftController FindRaftByName(string name)
    {
        RaftController[] allRafts = FindObjectsOfType<RaftController>();
        foreach (var raft in allRafts)
        {
            if (raft.name == name)
            {
                return raft;
            }
        }
        return null;
    }
}
