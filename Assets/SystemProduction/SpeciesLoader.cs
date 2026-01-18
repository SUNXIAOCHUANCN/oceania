using System.Collections.Generic;
using UnityEngine;

public class SpeciesLoader : MonoBehaviour
{
    private static SpeciesLoader _instance;
    
    private List<SpeciesScriptableObject> _cropSpecies = new List<SpeciesScriptableObject>();
    private List<SpeciesScriptableObject> _aniSpecies = new List<SpeciesScriptableObject>();
    private List<SpeciesScriptableObject> _matSpecies = new List<SpeciesScriptableObject>();

    public static SpeciesLoader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SpeciesLoader>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("SpeciesLoader");
                    _instance = obj.AddComponent<SpeciesLoader>();
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
        
        LoadSpecies();
    }

    private void LoadSpecies()
    {
        // 从Resources目录加载所有SpeciesScriptableObject
        SpeciesScriptableObject[] allSpecies = Resources.LoadAll<SpeciesScriptableObject>("ScriptableObjects/Species");
        
        if (allSpecies.Length == 0)
        {
            Debug.LogWarning("未找到物种资源，请确保资源放在Resources/ScriptableObjects/Species目录下");
        }
        else
        {
            Debug.Log($"成功加载 {allSpecies.Length} 个物种资源");
        }
        
        foreach (var species in allSpecies)
        {
            Debug.Log($"正在加载物种: {species.speciesName} (类型: {species.speciesType}, 解锁: {species.unlocked})");
            switch (species.speciesType)
            {
                case SpeciesType.Crop:
                    _cropSpecies.Add(species);
                    Debug.Log($"  ✓ 已添加作物物种: {species.speciesName}");
                    break;
                case SpeciesType.Ani:
                    _aniSpecies.Add(species);
                    Debug.Log($"  ✓ 已添加动物物种: {species.speciesName}");
                    break;
                case SpeciesType.Mat:
                    _matSpecies.Add(species);
                    Debug.Log($"  ✓ 已添加材料物种: {species.speciesName}");
                    break;
            }
        }
        
        Debug.Log($"物种加载完成 - 作物: {_cropSpecies.Count}, 动物: {_aniSpecies.Count}, 材料: {_matSpecies.Count}");
    }

    /// <summary>
    /// 返回所有已解锁的作物物种
    /// </summary>
    public List<SpeciesScriptableObject> GetUnlockedCropSpecies()
    {
        List<SpeciesScriptableObject> unlockedCrops = new List<SpeciesScriptableObject>();
        foreach (var crop in _cropSpecies)
        {
            if (crop.unlocked)
            {
                unlockedCrops.Add(crop);
            }
        }
        Debug.Log($"获取作物物种 - 总数: {_cropSpecies.Count}, 已解锁: {unlockedCrops.Count}");
        return unlockedCrops;
    }

    /// <summary>
    /// 返回所有已解锁的动物物种
    /// </summary>
    public List<SpeciesScriptableObject> GetUnlockedAniSpecies()
    {
        List<SpeciesScriptableObject> unlockedAnis = new List<SpeciesScriptableObject>();
        foreach (var ani in _aniSpecies)
        {
            if (ani.unlocked)
            {
                unlockedAnis.Add(ani);
            }
        }
        Debug.Log($"获取动物物种 - 总数: {_aniSpecies.Count}, 已解锁: {unlockedAnis.Count}");
        return unlockedAnis;
    }

    /// <summary>
    /// 返回所有已解锁的材料物种
    /// </summary>
    public List<SpeciesScriptableObject> GetUnlockedMatSpecies()
    {
        List<SpeciesScriptableObject> unlockedMats = new List<SpeciesScriptableObject>();
        foreach (var mat in _matSpecies)
        {
            if (mat.unlocked)
            {
                unlockedMats.Add(mat);
            }
        }
        Debug.Log($"获取材料物种 - 总数: {_matSpecies.Count}, 已解锁: {unlockedMats.Count}");
        return unlockedMats;
    }
}