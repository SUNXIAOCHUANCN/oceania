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

        // 绑定关闭按钮事件
        if (closePopualtionButton != null)
        {
            closePopualtionButton.onClick.AddListener(OnClosePopulationButtonClicked);
        }

        // 在开始时不显示UI
        // HideCanvas();
    }

    private void InitializeUI()
    {
        UpdateNowpopulationPanel();
        UpdateNewpopulationPanel();    
    }

    private void UpdateNowpopulationPanel()
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
            
            // 添加解雇按钮事件
            Transform dismissButtonTransform = recruitedPerson.transform.Find("Delete");
            if (dismissButtonTransform != null && dismissButtonTransform.TryGetComponent<Button>(out Button dismissButton))
            {
                // 使用闭包传递person参数
                dismissButton.onClick.AddListener(() => OnDismissPersonClicked(person));
            }
        }
    }

    private void UpdateNewpopulationPanel()
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
            
            // 添加雇佣按钮事件（避免闭包捕获循环变量问题）
            PersonScriptableObject currentPerson = person;
            Transform recruitButtonTransform = unrecruitedPerson.transform.Find("Recruit");
            if (recruitButtonTransform == null)
            {
                recruitButtonTransform = unrecruitedPerson.transform.Find("RecruitButton");
            }
            if (recruitButtonTransform == null)
            {
                recruitButtonTransform = unrecruitedPerson.transform.Find("AddButton");
            }
            if (recruitButtonTransform == null)
            {
                recruitButtonTransform = unrecruitedPerson.transform.Find("HireButton");
            }
            if (recruitButtonTransform == null)
            {
                recruitButtonTransform = unrecruitedPerson.transform.Find("JoinButton");
            }
            
            if (recruitButtonTransform != null && recruitButtonTransform.TryGetComponent<Button>(out Button recruitButton))
            {
                recruitButton.onClick.AddListener(() => OnRecruitPersonClicked(currentPerson));
                Debug.Log($"为人员{currentPerson.personName}绑定雇佣按钮");
            }
            else
            {
                Debug.LogError($"人员{currentPerson.personName}：未找到雇佣按钮，请检查prefab结构");
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDismissPersonClicked(PersonScriptableObject person)
    {
        Debug.Log($"解雇按钮触发: {person.personName}");
        if (person != null)
        {
            // 调用PersonManager的解雇方法
            PersonManager.Instance.DismissPerson(person);
            Debug.Log($"已解雇：{person.personName}");
            // 刷新UI显示
            InitializeUI();
        }
    }

    private void OnRecruitPersonClicked(PersonScriptableObject person)
    {
        Debug.Log($"雇佣按钮触发: {person.personName}");
        if (person != null)
        {
            // 调用PersonManager的招募方法
            PersonManager.Instance.RecruitPerson(person);
            Debug.Log($"已雇佣：{person.personName}");
            // 刷新UI显示
            InitializeUI();
        }
    }

    private void OnClosePopulationButtonClicked()
    {
        HideCanvas();

        if(CursorManager.Instance != null)
        {
            CursorManager.Instance.RegisterInteractionPanel(false);
        }
    }
}
