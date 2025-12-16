using System.Collections.Generic;
using UnityEngine;
public class VariantManager : MonoBehaviour
{
    public static VariantManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    // 将变种注册到 ResourceManager.knownResources（如果 ResourceManager 存在）
    public bool RegisterVariant(VariantScriptableObject variant)
    {
        if (variant == null)
        {
            Debug.LogWarning("RegisterVariant: variant 为 null");
            return false;
        }
        if (ResourceManager.Instance == null)
        {
            Debug.LogWarning("ResourceManager 未就绪，无法注册变种。");
            return false;
        }
        bool ok = ResourceManager.Instance.RegisterResource(variant);
        if (ok) 
        {
            Debug.Log($"已注册变种资源: {variant.resourceName}");
            variant.unlocked = true;
        }

        else Debug.LogWarning($"注册变种失败（可能已存在）: {variant.resourceName}");
        return ok;
    }

    // 驯化野生变种（运行时创建一个驯化版变种并注册）
    // 仅当场上有至少一个出海状态且具有 canBringBackVariant 的人口时允许驯化
    public bool TameWildVariant(VariantScriptableObject wildVariant)
    {
        if (wildVariant == null) return false;
        if (wildVariant.variantType != VariantType.Wild)
        {
            Debug.LogWarning("TameWildVariant: 目标不是野生变种");
            return false;
        }
        if (PopulationManager.Instance == null)
        {
            Debug.LogWarning("PopulationManager 未就绪，无法检查驯化师。");
            return false;
        }

        // 查找任意可用的驯化师
        bool hasTamer = false;
        foreach (var p in PopulationManager.Instance.allPeople)
        {
            if (p == null) continue;
            if (p.CanBringBackVariant() && p.currentStatus == Person.Status.Sailing)
            {
                hasTamer = true; 
                break;
            }
        }
        if (!hasTamer)
        {
            Debug.LogWarning("没有可用的驯化师，无法进行驯化。");
            return false;
        }

        bool registered = RegisterVariant(wildVariant);
        if (registered)
        {
            Debug.Log($"已成功驯化并注册变种: {wildVariant.resourceName}");
            return true;
        }
        Debug.LogWarning("驯化失败：注册变种失败");
        return false;
    }
}
