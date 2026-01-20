// PlayerBookController.cs
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBookController : MonoBehaviour
{
    [Header("书本设置")]
    public GameObject bookObject;          // 书本的GameObject
    public BookPro bookProScript;          // BookPro脚本（可选）

    [Header("按键设置")]
    public Key openBookKey = Key.E;        // 打开书本按键
    public Key closeBookKey = Key.Escape;  // 关闭书本按键

    [Header("功能设置")]
    public bool toggleWithE = true;        // 按E键切换打开/关闭
    public bool startClosed = true;        // 游戏开始时书本关闭
    public bool pauseWhileOpen = false;    // 打开书本时暂停游戏

    [Header("摄像机控制")]
    public bool lockCameraWhenOpen = false; // 打开书本时锁定摄像机
    private PlayerController playerController; // 玩家控制器引用

    // 状态变量
    private bool isBookOpen = false;
    private float lastMessageTime = 0f;
    private string currentMessage = "";

    void Start()
    {
        // 初始化书本状态
        if (bookObject != null)
        {
            bookObject.SetActive(!startClosed);
            isBookOpen = !startClosed;

            // 设置书本交互
            if (bookProScript != null)
            {
                bookProScript.interactable = !startClosed;
            }
        }

        // 获取玩家控制器
        playerController = GetComponent<PlayerController>();
    }
    
    
    void Update()
    {
        if (Keyboard.current == null) return;

        // 检测E键按下 - 打开/切换书本
        if (Keyboard.current[openBookKey].wasPressedThisFrame)
        {
            if (toggleWithE)
            {
                ToggleBook();
            }
            else
            {
                OpenBook();
            }
        }

        // 检测ESC键按下 - 关闭书本
        if (Keyboard.current[closeBookKey].wasPressedThisFrame && isBookOpen)
        {
            CloseBook();
        }
    }
    
    public void ToggleBook()
    {
        if (isBookOpen)
        {
            CloseBook();
        }
        else
        {
            OpenBook();
        }
    }
    
    public void OpenBook()
    {
        if (isBookOpen || bookObject == null) return;

        // 激活书本
        bookObject.SetActive(true);
        isBookOpen = true;

        // 启用书本交互
        if (bookProScript != null)
        {
            bookProScript.interactable = true;
        }

    }
    
    public void CloseBook()
    {
        if (!isBookOpen || bookObject == null) return;

        // 关闭书本
        bookObject.SetActive(false);
        isBookOpen = false;

        // 禁用书本交互
        if (bookProScript != null)
        {
            bookProScript.interactable = false;
        }
    }
    
}