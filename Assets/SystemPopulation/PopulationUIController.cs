using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Security.Cryptography;

public class PopulationUIController : CanvasController
{
    [Header("UI References")]
    [SerializeField] private GameObject nowPopulationPanel;
    [SerializeField] private GameObject newPopulationPanel;
    [SerializeField] private Button closePopualtionButton;
    [SerializeField] private Transform RecruitedPersonList;
    [SerializeField] private Transform UnrecruitedPersonList;

    private const string NOW_POPULATION_PREFAB_PATH = "UIprefabs/Population/now_population";
    private const string NEW_POPULATION_PREFAB_PATH = "UIprefabs/Population/new_population";
    /// <summary>
    /// 重写ShowCanvas以添加调试信息
    /// </summary>
    public override void ShowCanvas()
    {
        Debug.Log($"PopulationUIController.ShowCanvas() called, current active state: {gameObject.activeSelf}");
        base.ShowCanvas();
        Debug.Log($"PopulationUIController.ShowCanvas() completed, new active state: {gameObject.activeSelf}");
    }

    void Start()
    {
        // 初始化UI
        InitializeUI();

        // 在开始时不显示UI
        // HideCanvas();
    }

    private void InitializeUI()
    {
        InitializeNowpopulationPanel();
        InitializeNewpopulationPanel();    
    }

    private void InitializeNowpopulationPanel()
    {
        foreach (Transform child in RecruitedPersonList)
        {
            Destroy(child.gameObject);
        }

        GameObject NowpopulationPrefab = Resources.Load<GameObject>(NOW_POPULATION_PREFAB_PATH);
        if(NowpopulationPrefab == null)
        {
            Debug.LogError($"无法加载现有人口预制体：{NOW_POPULATION_PREFAB_PATH}");
            return;
        }

        List<PersonScriptableObject> persons = PersonsLoader.Instance.GetRecruitedPersons();

        foreach (PersonScriptableObject person in persons)
        {
            GameObject recruitedPerson = Instantiate(NowpopulationPrefab, RecruitedPersonList);
            recruitedPerson.name = person.personName;

            Transform nameText = recruitedPerson.transform.Find("Name");
            if(nameText != null && nameText.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI Name))
            {
                Name.text = person.personName;
            }

            Transform jobText = recruitedPerson.transform.Find("Profession");
            if(jobText != null && jobText.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI Profession))
            {
                Profession.text = person.profession.ToString();
            }

            Transform avatarImage = recruitedPerson.transform.Find("touxiang");
            if (avatarImage != null && avatarImage.TryGetComponent<Image>(out Image avatar))
            {
                avatar.sprite = person.avatar;
            }
        }
    }

    private void InitializeNewpopulationPanel()
    {
        foreach (Transform child in UnrecruitedPersonList)
        {
            Destroy(child.gameObject);
        }

        GameObject NewpopulationPrefab = Resources.Load<GameObject>(NEW_POPULATION_PREFAB_PATH);
        if(NewpopulationPrefab == null)
        {
            Debug.LogError($"无法加载现有人口预制体：{NEW_POPULATION_PREFAB_PATH}");
            return;
        }

        List<PersonScriptableObject> persons = PersonsLoader.Instance.GetUnrecruitedPersons();

        foreach (PersonScriptableObject person in persons)
        {
            GameObject unrecruitedPerson = Instantiate(NewpopulationPrefab, UnrecruitedPersonList);
            unrecruitedPerson.name = person.personName;

            Transform nameText = unrecruitedPerson.transform.Find("Name");
            if(nameText != null && nameText.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI Name))
            {
                Name.text = person.personName;
            }

            Transform jobText = unrecruitedPerson.transform.Find("Profession");
            if(jobText != null && jobText.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI Profession))
            {
                Profession.text = person.profession.ToString();
            }

            Transform effectText = unrecruitedPerson.transform.Find("Effect");
            if(effectText != null && effectText.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI Effect))
            {
                Effect.text = person.professionDescription;
            }

            Transform avatarImage = unrecruitedPerson.transform.Find("touxiang");
            if (avatarImage != null && avatarImage.TryGetComponent<Image>(out Image avatar))
            {
                avatar.sprite = person.avatar;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private void HidePopulationUI()
    {
        HideCanvas();
    }
    private void OnClosePopulationButtonClicked()
    {
        HidePopulationUI();

        if(CursorManager.Instance != null)
        {
            CursorManager.Instance.RegisterInteractionPanel(false);
        }
    }
}
