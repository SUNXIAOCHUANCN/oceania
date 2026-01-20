using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class IslandPageController : MonoBehaviour
{
    [Header("手动绑定的物种展示（固定5个）")]
    [SerializeField] private SpeciesDisplay[] manualSpeciesDisplays = new SpeciesDisplay[5];

    [Header("手动绑定的秘密展示（固定1个）")]
    [SerializeField] private SecretDisplay manualSecretDisplay;

    [Header("手动绑定的信物展示（固定1个）")]
    [SerializeField] private TokenDisplay manualTokenDisplay;

    [Header("默认显示内容")]
    [SerializeField] private Sprite defaultSpeciesSprite;
    [SerializeField] private string lockedSpeciesNamePrefix = "???";
    [SerializeField] private string lockedSpeciesDescription = "???";

    private ProgressTableManager currentManager;

    public void UpdatePageContent(ProgressTableManager manager)
    {
        if (manager == null)
        {
            Debug.LogError("IslandPageController: 传入的ProgressTableManager为空");
            return;
        }

        currentManager = manager;

        // 更新物种展示
        UpdateSpeciesDisplays();

        // 更新秘密展示
        UpdateSecretDisplay();

        // 更新信物展示
        UpdateTokenDisplay();
    }

    private void UpdateSpeciesDisplays()
    {
        if (currentManager == null) return;

        // 获取绑定的物种列表
        List<SpeciesScriptableObject> boundSpecies = currentManager.GetBoundSpecies();
        if (boundSpecies == null || boundSpecies.Count == 0)
        {
            Debug.LogWarning("IslandPageController: 没有绑定的物种");
            // 隐藏所有手动绑定的显示项
            for (int i = 0; i < manualSpeciesDisplays.Length; i++)
            {
                if (manualSpeciesDisplays[i] != null)
                {
                    manualSpeciesDisplays[i].gameObject.SetActive(false);
                }
            }
            return;
        }

        // 更新手动绑定的物种展示项
        for (int i = 0; i < manualSpeciesDisplays.Length; i++)
        {
            SpeciesDisplay display = manualSpeciesDisplays[i];
            if (display == null)
            {
                Debug.LogWarning($"IslandPageController: manualSpeciesDisplays[{i}] 未绑定");
                continue;
            }

            if (i < boundSpecies.Count)
            {
                // 有对应的物种数据
                SpeciesScriptableObject species = boundSpecies[i];
                display.Initialize(species, defaultSpeciesSprite, lockedSpeciesNamePrefix, lockedSpeciesDescription);
                display.gameObject.SetActive(true);
            }
            else
            {
                // 没有对应的物种数据，隐藏该显示项
                display.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateSecretDisplay()
    {
        SecretScriptableObject secret = currentManager?.GetBoundSecret();
        
        if (manualSecretDisplay == null)
        {
            Debug.LogError("IslandPageController: manualSecretDisplay 未绑定");
            return;
        }

        if (secret == null)
        {
            // 没有秘密，隐藏秘密显示项
            manualSecretDisplay.gameObject.SetActive(false);
            return;
        }

        // 初始化秘密显示项
        manualSecretDisplay.Initialize(secret);
        manualSecretDisplay.gameObject.SetActive(true);
    }

    private void UpdateTokenDisplay()
    {
        TokenScriptableObject token = currentManager?.GetBoundToken();
        
        if (manualTokenDisplay == null)
        {
            Debug.LogError("IslandPageController: manualTokenDisplay 未绑定");
            return;
        }

        if (token == null)
        {
            // 没有信物，隐藏信物显示项
            manualTokenDisplay.gameObject.SetActive(false);
            return;
        }

        // 初始化信物显示项
        manualTokenDisplay.Initialize(token, currentManager);
        manualTokenDisplay.gameObject.SetActive(true);
    }

    public void RefreshDisplay()
    {
        if (currentManager != null)
        {
            UpdatePageContent(currentManager);
        }
    }
}