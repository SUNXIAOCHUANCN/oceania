using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class IslandPageController : MonoBehaviour
{
    [Header("物种UI元素 - 最多5个")]
    public SpeciesDisplayItem speciesSlot1;
    public SpeciesDisplayItem speciesSlot2;
    public SpeciesDisplayItem speciesSlot3;
    public SpeciesDisplayItem speciesSlot4;
    public SpeciesDisplayItem speciesSlot5;

    [Header("秘密UI元素")]
    public SecretDisplayItem secretSlot;

    [Header("信物UI元素")]
    public TokenDisplayItem tokenSlot;

    private ProgressTableManager manager;

    public void UpdatePageContent(ProgressTableManager manager)
    {
        this.manager = manager;
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        if (manager == null) return;

        // 显示物种
        DisplaySpecies();

        // 显示秘密
        DisplaySecret();

        // 显示信物
        DisplayToken();
    }

    private void DisplaySpecies()
    {
        var speciesList = manager.GetBoundSpecies();
        
        // 分别更新每个物种槽位
        UpdateSpeciesSlot(speciesSlot1, GetSpeciesByIndex(speciesList, 0));
        UpdateSpeciesSlot(speciesSlot2, GetSpeciesByIndex(speciesList, 1));
        UpdateSpeciesSlot(speciesSlot3, GetSpeciesByIndex(speciesList, 2));
        UpdateSpeciesSlot(speciesSlot4, GetSpeciesByIndex(speciesList, 3));
        UpdateSpeciesSlot(speciesSlot5, GetSpeciesByIndex(speciesList, 4));
    }

    private SpeciesScriptableObject GetSpeciesByIndex(List<SpeciesScriptableObject> speciesList, int index)
    {
        if (index >= 0 && index < speciesList.Count)
        {
            return speciesList[index];
        }
        return null;
    }

    private void UpdateSpeciesSlot(SpeciesDisplayItem slot, SpeciesScriptableObject species)
    {
        if (slot != null)
        {
            if (species != null)
            {
                bool isUnlocked = manager.IsSpeciesUnlocked(species.speciesName);
                slot.UpdateDisplay(species, isUnlocked);
            }
            else
            {
                // 如果没有物种，则显示空白或隐藏
                slot.gameObject.SetActive(false);
            }
        }
    }

    private void DisplaySecret()
    {
        var secret = manager.GetBoundSecret();
        if (secretSlot != null)
        {
            if (secret != null)
            {
                bool isUnlocked = manager.IsSecretUnlocked();
                secretSlot.UpdateDisplay(secret, isUnlocked);
            }
            else
            {
                secretSlot.gameObject.SetActive(false);
            }
        }
    }

    private void DisplayToken()
    {
        var token = manager.GetBoundToken();
        if (tokenSlot != null)
        {
            if (token != null)
            {
                bool isUnlocked = manager.IsTokenUnlocked();
                tokenSlot.UpdateDisplay(token, isUnlocked);
            }
            else
            {
                tokenSlot.gameObject.SetActive(false);
            }
        }
    }
}