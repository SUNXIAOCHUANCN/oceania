using System.Collections.Generic;
using UnityEngine;

public class PersonManager : MonoBehaviour
{
    private static PersonManager _instance;
    
    // 存储所有人员的列表
    private List<PersonScriptableObject> _allPersons = new List<PersonScriptableObject>();
    
    public static PersonManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PersonManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("PersonManager");
                    _instance = obj.AddComponent<PersonManager>();
                    DontDestroyOnLoad(obj);
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 安全地订阅人员加载完成事件
        PersonsLoader loader = PersonsLoader.Instance;
        if (loader != null)
        {
            loader.OnPersonsLoaded += OnPersonsLoaded;
        }
        
        // 从PersonsLoader加载所有人员
        LoadAllPersons();
    }

    private void OnPersonsLoaded()
    {
        // 当人员加载完成后，重新加载一次数据
        LoadAllPersons();
        Debug.Log("PersonManager: 人员数据加载完成");
    }

    /// <summary>
    /// 从PersonsLoader加载所有人员数据
    /// </summary>
    private void LoadAllPersons()
    {
        PersonsLoader loader = PersonsLoader.Instance;
        if (loader != null)
        {
            _allPersons = loader.GetAllPersons();
            Debug.Log($"从PersonsLoader成功加载 {_allPersons.Count} 个人员资源");
        }
        else
        {
            Debug.LogError("无法找到PersonsLoader实例");
        }
    }

    /// <summary>
    /// 招募人员
    /// </summary>
    /// <param name="person">要招募的人员</param>
    public void RecruitPerson(PersonScriptableObject person)
    {
        if (person != null)
        {
            // 更新人员招募状态
            person.recruited = true;
            
            // 通知PersonsLoader更新状态
            PersonsLoader.Instance?.UpdatePersonRecruitment(person, true);
            
            Debug.Log($"招募人员: {person.personName}");
        }
    }

    /// <summary>
    /// 解散人员
    /// </summary>
    /// <param name="person">要解散的人员</param>
    public void DismissPerson(PersonScriptableObject person)
    {
        if (person != null)
        {
            // 检查资源是否足够支付遣散费用（可选）
            // 这里可以添加遣散费用的逻辑
            
            // 更新人员招募状态
            person.recruited = false;
            
            // 通知PersonsLoader更新状态
            PersonsLoader.Instance?.UpdatePersonRecruitment(person, false);
            
            Debug.Log($"解散人员: {person.personName}");
        }
    }

    /// <summary>
    /// 改变人员状态
    /// </summary>
    /// <param name="person">要改变状态的人员</param>
    /// <param name="newStatus">新的状态</param>
    public void ChangePersonStatus(PersonScriptableObject person, PersonStatus newStatus)
    {
        if (person != null)
        {
            // 更新人员状态
            person.status = newStatus;
            
            // 通知PersonsLoader更新状态
            PersonsLoader.Instance?.UpdatePersonStatus(person, newStatus);
            
            Debug.Log($"更改人员 {person.personName} 的状态为: {newStatus}");
        }
    }

    /// <summary>
    /// 获取所有人员列表
    /// </summary>
    /// <returns>所有人员列表</returns>
    public List<PersonScriptableObject> GetAllPersons()
    {
        return new List<PersonScriptableObject>(_allPersons);
    }

    /// <summary>
    /// 获取已招募人员列表
    /// </summary>
    /// <returns>已招募人员列表</returns>
    public List<PersonScriptableObject> GetRecruitedPersons()
    {
        List<PersonScriptableObject> recruitedPersons = new List<PersonScriptableObject>();
        foreach (var person in _allPersons)
        {
            if (person.recruited)
            {
                recruitedPersons.Add(person);
            }
        }
        return recruitedPersons;
    }

    /// <summary>
    /// 根据名称查找人员
    /// </summary>
    /// <param name="personName">人员名称</param>
    /// <returns>找到的人员，如果未找到则返回null</returns>
    public PersonScriptableObject FindPersonByName(string personName)
    {
        foreach (var person in _allPersons)
        {
            if (person.personName == personName)
            {
                return person;
            }
        }
        return null;
    }

    /// <summary>
    /// 检查资源是否足够招募人员
    /// </summary>
    /// <param name="person">要招募的人员</param>
    /// <returns>是否足够</returns>
    public bool CanAffordToRecruit(PersonScriptableObject person)
    {
        if (person == null || ResourceManager.Instance == null) return false;
        
        // 检查是否有足够的资源来招募人员（这里可以定义招募成本）
        // 暂时返回true，可以根据实际需求添加招募成本
        return true;
    }

    /// <summary>
    /// 检查当前资源是否足够维持所有已招募人员
    /// </summary>
    public void CheckResourceConsumption()
    {
        if (ResourceManager.Instance == null) return;
        
        var recruitedPersons = GetRecruitedPersons();
        float totalCropConsumption = 0;
        float totalAniConsumption = 0;
        float totalMatConsumption = 0;
        
        foreach (var person in recruitedPersons)
        {
            totalCropConsumption += person.monthlyCropConsumption;
            totalAniConsumption += person.monthlyAniConsumption;
            totalMatConsumption += person.monthlyMatConsumption;
        }
        
        // 这里可以实现资源消耗逻辑，例如每月消耗资源
        Debug.Log($"已招募人员总数: {recruitedPersons.Count}, 每月总消耗 - Crop: {totalCropConsumption}, Ani: {totalAniConsumption}, Mat: {totalMatConsumption}");
    }

    /// <summary>
    /// 保存人员状态到PersonsLoader
    /// </summary>
    private void SavePersonStates()
    {
        // 人员状态已经通过PersonsLoader的Update方法实时更新
        // 这里可以添加额外的保存逻辑，如保存到PlayerPrefs
        SaveToPlayerPrefs();
    }

    /// <summary>
    /// 将人员状态保存到PlayerPrefs
    /// </summary>
    private void SaveToPlayerPrefs()
    {
        // 保存每个人员的招募状态和状态
        for (int i = 0; i < _allPersons.Count; i++)
        {
            PlayerPrefs.SetInt($"Person_{i}_Recruited", _allPersons[i].recruited ? 1 : 0);
            PlayerPrefs.SetString($"Person_{i}_Status", _allPersons[i].status.ToString());
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 从PlayerPrefs加载人员状态
    /// </summary>
    private void LoadFromPlayerPrefs()
    {
        for (int i = 0; i < _allPersons.Count; i++)
        {
            if (PlayerPrefs.HasKey($"Person_{i}_Recruited"))
            {
                _allPersons[i].recruited = PlayerPrefs.GetInt($"Person_{i}_Recruited") == 1;
            }
            if (PlayerPrefs.HasKey($"Person_{i}_Status"))
            {
                string statusStr = PlayerPrefs.GetString($"Person_{i}_Status");
                if (System.Enum.TryParse(statusStr, out PersonStatus status))
                {
                    _allPersons[i].status = status;
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        SavePersonStates();
    }

    private void OnDestroy()
    {
        SavePersonStates();
        // 取消订阅，防止内存泄漏
        PersonsLoader loader = PersonsLoader.Instance;
        if (loader != null)
        {
            loader.OnPersonsLoaded -= OnPersonsLoaded;
        }
    }
}