using System.Collections.Generic;
using UnityEngine;

public class PersonsLoader : MonoBehaviour
{
    private static PersonsLoader _instance;
    
    private List<PersonScriptableObject> _persons = new List<PersonScriptableObject>();

    public System.Action OnPersonsLoaded;

    public static PersonsLoader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PersonsLoader>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("PersonsLoader");
                    _instance = obj.AddComponent<PersonsLoader>();
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
        
        LoadPersons();
    }

    private void LoadPersons()
    {
        // 从Resources目录加载所有PersonScriptableObject
        PersonScriptableObject[] allPersons = Resources.LoadAll<PersonScriptableObject>("ScriptableObjects/persons");
        
        if (allPersons.Length == 0)
        {
            Debug.LogWarning("未找到人员资源，请确保资源放在Resources/ScriptableObjects/persons目录下");
        }
        else
        {
            Debug.Log($"成功加载 {allPersons.Length} 个人员资源");
        }
        
        foreach (var person in allPersons)
        {
            _persons.Add(person);
        }
        OnPersonsLoaded?.Invoke();
    }

    /// <summary>
    /// 返回所有已招募的人员
    /// </summary>
    public List<PersonScriptableObject> GetRecruitedPersons()
    {
        List<PersonScriptableObject> recruitedPersons = new List<PersonScriptableObject>();
        foreach (var person in _persons)
        {
            if (person.recruited)
            {
                recruitedPersons.Add(person);
            }
        }
        return recruitedPersons;
    }
    public List<PersonScriptableObject> GetUnrecruitedPersons()
    {
        List<PersonScriptableObject> unrecruitedPersons = new List<PersonScriptableObject>();
        foreach (var person in _persons)
        {
            if (!person.recruited)
            {
                unrecruitedPersons.Add(person);
            }
        }
        return unrecruitedPersons;
    }

    /// <summary>
    /// 获取所有人员
    /// </summary>
    /// <returns>所有人员列表</returns>
    public List<PersonScriptableObject> GetAllPersons()
    {
        return new List<PersonScriptableObject>(_persons);
    }

    /// <summary>
    /// 更新人员招募状态
    /// </summary>
    /// <param name="person">要更新的人员</param>
    /// <param name="isRecruited">招募状态</param>
    public void UpdatePersonRecruitment(PersonScriptableObject person, bool isRecruited)
    {
        if (person != null)
        {
            person.recruited = isRecruited;
        }
    }

    /// <summary>
    /// 更新人员状态
    /// </summary>
    /// <param name="person">要更新的人员</param>
    /// <param name="newStatus">新状态</param>
    public void UpdatePersonStatus(PersonScriptableObject person, PersonStatus newStatus)
    {
        if (person != null)
        {
            person.status = newStatus;
        }
    }
}