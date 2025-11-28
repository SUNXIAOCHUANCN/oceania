using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float speed = 5f;
    public float runSpeed=10f;
    public float jumpForce = 5f;
    private Vector2 move;
    private Vector2 mouseLook;
    public Vector2 Move => move;
    
    // Character Controller 变量
    private CharacterController characterController;
    private Vector3 velocity;
    private float gravity = -9.8f; // 注意：重力通常设为负数

    [Header("动画设置")]
    private Animator playerAnimator;

    [Header("摄像机设置")]
    private GameObject cameraObject;
    public float sensitivity = 0.5f; // 鼠标灵敏度
    
    public float minPitch=-60f;
    public float maxPitch=90f;
    // 角色的距离
    
    //记录摄像机当前的欧拉角
    private float cameraYaw = 0f;   // 水平旋转角度 (Y轴)
    private float cameraPitch = 0f; // 垂直旋转角度 (X轴)

    // 摄像机距离
    public float cameraDistance = 4f; // 摄像机距离
    public float minDistance = 2f;    // 最近距离 (拉到最近)
    public float maxDistance = 10f;   // 最远距离 (拉到最远)
    public float zoomSpeed = 0.5f;    // 缩放灵敏度
    [Header("光标控制")]
    public CursorManager cursorManager;
    private bool isCursorVisible = false;

    private PlayerInput playerInput;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        
        // 查找子物体 Camera (建议确保名字匹配)
        Transform camTrans = transform.Find("Camera");
        if (camTrans != null)
            cameraObject = camTrans.gameObject;
        else
            Debug.LogError("未找到名为 'Camera' 的子物体，请检查层级！");

        if (GetComponent<Animator>() != null)
        {
            playerAnimator = GetComponent<Animator>();
        }
        else
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }

        // 初始化摄像机角度（防止一开始摄像机跳变）
        if (cameraObject != null)
        {
            cameraYaw = transform.eulerAngles.y;
        }

        // 锁定并隐藏鼠标光标（FPS/TPS游戏的标准操作）
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. 获取输入
        move = playerInput.actions.FindAction("Move").ReadValue<Vector2>();
        mouseLook = playerInput.actions.FindAction("MouseLook").ReadValue<Vector2>();
        bool isSprintPressed=playerInput.actions.FindAction("Sprint").IsPressed();
        bool isRunning=isSprintPressed&&move.magnitude>0.01f;
        float currentSpeed=isRunning?runSpeed:speed;
        bool jumpTriggered=playerInput.actions.FindAction("Jump").triggered;
        bool isAltPressed = playerInput.actions.FindAction("UnlockCursor").IsPressed();

        //摄像机距离
        float screenInput=playerInput.actions.FindAction("Zoom").ReadValue<Vector2>().y;
        if(screenInput != 0)
        {
            cameraDistance -= screenInput * zoomSpeed*0.1f;
            cameraDistance = Mathf.Clamp(cameraDistance, minDistance, maxDistance);
        }

        //光标控制
        if (isAltPressed != isCursorVisible)
        {
            isCursorVisible = isAltPressed;
            
            // 调用外部接口
            if (cursorManager != null)
            {
                cursorManager.SetCursorState(isCursorVisible);
            }
        }

        // 2. 角色移动逻辑
        if (characterController.isGrounded)
        {
            if (velocity.y < 0)
            {
                velocity.y = -2f; // 给一点向下的力确保贴地
            }
            if (jumpTriggered)
            {
                velocity.y=jumpForce;
                if(playerAnimator != null)
                {
                    playerAnimator.SetTrigger("jump");
                }
            }
        }
        else
        {
            Debug.Log("In Air");
        }
        

        // 移动角色 
        Transform camTransform= cameraObject.transform;
        Vector3 camForward = camTransform.forward;
        Vector3 camRight = camTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();
        Vector3 moveDirection = camForward * move.y + camRight * move.x;
        if (moveDirection.magnitude > 0.1f)
        {
            //让角色身体转向移动的方向，这样看起来更自然
            transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * 10f);
            // 移动角色
            characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        }
        // 重力
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
        float inputMagnitude=move.magnitude;
        if(playerAnimator != null)
        {
            playerAnimator.SetFloat("speed",inputMagnitude);
            playerAnimator.SetBool("isRunning",isRunning);
            playerAnimator.SetBool("isGrounded",characterController.isGrounded);
        }
        //应用重力
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    // 使用 LateUpdate 处理摄像机，确保在角色移动完成后再移动摄像机，避免抖动
    void LateUpdate()
    {
        if (cameraObject != null)
        {
            if(!isCursorVisible)
            {
                rotateCamera(mouseLook, sensitivity, cameraDistance);
            }
            
        }
    }

    // ---------------------- 补全的核心函数 ----------------------
    void rotateCamera(Vector2 rotateVector, float sensitivity, float distance)
    {
        // 1. 定义旋转中心：角色位置向上偏移一点（大概在头部或肩膀位置）
        Vector3 rotateCenter = this.transform.position + Vector3.up * 1.5f;

        // 2. 累加鼠标输入到角度变量
        // rotateVector.x 是鼠标左右移动 -> 影响 Yaw (绕Y轴转)
        // rotateVector.y 是鼠标上下移动 -> 影响 Pitch (绕X轴转)
        
        cameraYaw += rotateVector.x * sensitivity;
        cameraPitch -= rotateVector.y * sensitivity; // 减去y值，通常鼠标向上推是想看上方（视角下压）还是看下方取决于习惯

        // 3. 限制垂直角度（Clamp），防止摄像机翻转到底部或顶部
        // -80度(看天) 到 80度(看地)
        cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);

        // 4. 计算旋转的四元数
        // 顺序：先绕X轴转(Pitch)，再绕Y轴转(Yaw)
        Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);

        // 5. 计算摄像机位置
        // 原理：位置 = 中心点 - (旋转方向 * 距离)
        // 这样摄像机就会始终保持在中心点后方 'distance' 米处，并随着 rotation 旋转
        Vector3 position = rotateCenter - (rotation * Vector3.forward * distance);

        // 6. 应用到摄像机对象
        cameraObject.transform.rotation = rotation;
        cameraObject.transform.position = position;
        
        // 可选：如果你希望角色身体始终跟随镜头的水平方向旋转（像魔兽世界右键或绝地求生），取消下面这行的注释
        // transform.rotation = Quaternion.Euler(0, cameraYaw, 0); 
    }
}