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
    private float raftCameraDistance;//航海时的摄像机距离
    public float raftMinDistance = 4f;
    public float raftMaxDistance = 20f;

    // 摄像机距离
    public float cameraDistance = 4f; // 摄像机距离
    public float minDistance = 2f;    // 最近距离 (拉到最近)
    public float maxDistance = 10f;   // 最远距离 (拉到最远)
    public float zoomSpeed = 0.5f;    // 缩放灵敏度
    private float previousCameraDistance;
    [Header("光标控制")]
    public CursorManager cursorManager;
    private bool isCursorVisible = false;

    [Header("碰撞检测设置")]
    public LayerMask raftLayer = -1; // 可以设置raft所在的Layer，默认检测所有层
    public float collisionCheckRadius = 0.6f; // 碰撞检测半径，略大于CharacterController的半径

    [Header("上船控制设置")]
    public float onRaftCheckRadius = 5f; // 检测玩家是否在船上的半径（默认改为5，更容易检测）
    public float switchToRaftControlDelay = 2f; // 上船后多少秒切换到船控制模式
    public bool enableDebugLogs = true; // 是否启用调试日志

    [Header("船只交互设置")]
    public Vector3 raftStandOffset = new Vector3(0f, 0.5f, 0f);
    public float dismountPushDistance = 2f;
    public float dismountUpOffset = 0.5f;
    
    [Header("输入设置")]
    private playerInputActions inputActions;

    [Header("航海颠簸效果（驾驶时）")]
    public bool enableSeaBobbing = true;
    public float bobFrequency = 1.2f;          // 上下起伏频率
    public float bobAmplitude = 0.06f;         // 上下起伏幅度（米）
    public float swayFrequency = 0.9f;         // 左右/前后轻微漂移频率
    public float swayAmplitude = 0.04f;        // 漂移幅度（米）
    public float rollAmplitudeDeg = 1.5f;      // 左右横滚（度）
    public float pitchAmplitudeDeg = 1.0f;     // 前后俯仰（度）
    public float speedInfluence = 0.08f;       // 船速对颠簸强度的影响
    public float maxIntensity = 2.0f;          // 强度上限

    private PlayerInput playerInput;
    private RaftController currentRaft; // 当前所在的船
    private float timeOnRaft = 0f; // 在船上的时间
    private bool isControllingRaft = false; // 是否正在控制船
    private Transform originalParent;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        characterController = GetComponent<CharacterController>();
        
        // 初始化输入动作
        if (inputActions == null)
        {
            inputActions = new playerInputActions();
        }
        //interactionAction = inputActions.player.Interaction;
        
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

        originalParent = transform.parent;

        // 锁定并隐藏鼠标光标（FPS/TPS游戏的标准操作）
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        
    }

    
    private void OnEnable()
    {
        // 启用 Interaction 动作并绑定回调
        /*
        if (interactionAction != null)
        {
            interactionAction.Enable();
            interactionAction.performed += OnInteractionPerformed;
        }
        */
    }

    private void OnDisable()
    {
        /*
        // 禁用 Interaction 动作并解绑回调
        if (interactionAction != null)
        {
            interactionAction.performed -= OnInteractionPerformed;
            interactionAction.Disable();
        }
        */
    }

    /// <summary>
    /// Interaction Action 回调 - 处理船只交互（上船/下船）
    /// </summary>
    private void OnInteractionPerformed(InputAction.CallbackContext context)
    {
        if (currentRaft != null)
        {
            HandleDismountInput();
        }
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
        bool godownRaft=playerInput.actions.FindAction("Interaction").triggered;

        //摄像机距离
        float screenInput = playerInput.actions.FindAction("Zoom").ReadValue<Vector2>().y;
        if (screenInput != 0)
        {
            if (isControllingRaft)
            {
                raftCameraDistance -= screenInput * zoomSpeed * 0.1f;
                raftCameraDistance = Mathf.Clamp(raftCameraDistance, raftMinDistance, raftMaxDistance);
            }
            else
            {
                cameraDistance -= screenInput * zoomSpeed * 0.1f;
                cameraDistance = Mathf.Clamp(cameraDistance, minDistance, maxDistance);
            }
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

        // 检测玩家是否在船上
        CheckIfOnRaft();

        // 检测下船输入（F键）
        //bool interactTriggered = playerInput.actions.FindAction("Interaction").triggered;
        if (currentRaft != null && godownRaft)
        {
            Debug.Log("下船输入检测到");
            HandleDismountInput();
        }
        else
        {
            Debug.Log("玩家按下了F键，但是没有检测到船只");
        }

        // 如果玩家在船上，处理船的控制
        if (currentRaft != null)
        {
            UpdateRaftControl();
        }
        else
        {
            // 不在船上时重置状态
            timeOnRaft = 0f;
            isControllingRaft = false;
        }

        bool controllerActive = characterController != null && characterController.enabled;

        if (controllerActive)
        {
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
            

            // 如果玩家在船上且还未进入船控制模式，允许玩家自由移动
            if (currentRaft != null && !isControllingRaft)
            {
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
                    
                    // 碰撞检测：在移动前检查是否会与raft碰撞
                    Vector3 moveVector = moveDirection * currentSpeed * Time.deltaTime;
                    
                    // 使用SphereCast检测碰撞（从当前位置向前检测）
                    Vector3 capsuleCenter = transform.position + characterController.center;
                    Vector3 capsuleBottom = capsuleCenter - Vector3.up * (characterController.height / 2f - characterController.radius);
                    Vector3 capsuleTop = capsuleCenter + Vector3.up * (characterController.height / 2f - characterController.radius);
                    
                    RaycastHit hit;
                    float moveDistance = moveVector.magnitude;
                    // 在最后添加 QueryTriggerInteraction.Ignore
                    if (Physics.CapsuleCast(capsuleBottom, capsuleTop, characterController.radius, moveDirection.normalized, out hit, moveDistance + 0.1f, raftLayer, QueryTriggerInteraction.Ignore))
                    {
                        // 如果检测到raft，计算可以移动的距离
                        float allowedDistance = hit.distance - characterController.radius - 0.1f;
                        if (allowedDistance > 0.01f)
                        {
                            // 可以部分移动
                            characterController.Move(moveDirection.normalized * allowedDistance);
                        }
                        // 如果距离太近，则不移动（被阻挡）
                    }
                    else
                    {
                        // 没有碰撞，正常移动
                        characterController.Move(moveVector);
                    }
                }
            }
            else if (currentRaft == null)
            {
                // 不在船上，正常移动角色 
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
                    
                    // 碰撞检测：在移动前检查是否会与raft碰撞
                    Vector3 moveVector = moveDirection * currentSpeed * Time.deltaTime;
                    
                    // 使用SphereCast检测碰撞（从当前位置向前检测）
                    Vector3 capsuleCenter = transform.position + characterController.center;
                    Vector3 capsuleBottom = capsuleCenter - Vector3.up * (characterController.height / 2f - characterController.radius);
                    Vector3 capsuleTop = capsuleCenter + Vector3.up * (characterController.height / 2f - characterController.radius);
                    
                    RaycastHit hit;
                    float moveDistance = moveVector.magnitude;
                    // 在最后添加 QueryTriggerInteraction.Ignore
                    if (Physics.CapsuleCast(capsuleBottom, capsuleTop, characterController.radius, moveDirection.normalized, out hit, moveDistance + 0.1f, raftLayer, QueryTriggerInteraction.Ignore))
                    {
                        // 如果检测到raft，计算可以移动的距离
                        float allowedDistance = hit.distance - characterController.radius - 0.1f;
                        if (allowedDistance > 0.01f)
                        {
                            // 可以部分移动
                            characterController.Move(moveDirection.normalized * allowedDistance);
                        }
                        // 如果距离太近，则不移动（被阻挡）
                    }
                    else
                    {
                        // 没有碰撞，正常移动
                        characterController.Move(moveVector);
                    }
                }
            }
            
            // 重力处理
            // 如果不在船控制模式，正常处理重力
            if (currentRaft == null || !isControllingRaft)
            {
                velocity.y += gravity * Time.deltaTime;
                characterController.Move(velocity * Time.deltaTime);
            }
            else
            {
                // 在船控制模式下，只应用少量重力确保玩家贴船，不应用完整重力
                velocity.y = -2f; // 给一点向下的力确保贴船
                characterController.Move(velocity * Time.deltaTime);
            }
        }
        else
        {
            // 在船控制模式下禁用 CharacterController，保持静止
            velocity = Vector3.zero;
        }
        
        // 动画参数
        float inputMagnitude = move.magnitude;
        if(playerAnimator != null)
        {
            // 如果在船控制模式，输入幅度应该为0（因为玩家不移动）
            if (isControllingRaft)
            {
                inputMagnitude = 0f;
            }
            playerAnimator.SetFloat("speed", inputMagnitude);
            playerAnimator.SetBool("isRunning", isRunning && !isControllingRaft);
            bool groundedForAnim = characterController != null && characterController.enabled && characterController.isGrounded;
            playerAnimator.SetBool("isGrounded", groundedForAnim);
        }
    }

    // 使用 LateUpdate 处理摄像机，确保在角色移动完成后再移动摄像机，避免抖动
    void LateUpdate()
    {
        if (cameraObject != null)
        {
            if(!isCursorVisible)
            {
                float targetDistance = isControllingRaft ? raftCameraDistance : cameraDistance;
                rotateCamera(mouseLook, sensitivity, targetDistance);
            }

            ApplySeaBobbingToCamera();
            
        }
    }

    private void ApplySeaBobbingToCamera()
    {
        if (!enableSeaBobbing) return;
        if (!isControllingRaft || currentRaft == null) return;
        if (cameraObject == null) return;

        float speed = 0f;
        Rigidbody rb = currentRaft.raftRigidbody != null ? currentRaft.raftRigidbody : currentRaft.GetComponent<Rigidbody>();
        if (rb != null)
        {
            speed = rb.linearVelocity.magnitude;
        }

        float intensity = Mathf.Clamp(1f + speed * speedInfluence, 0f, maxIntensity);
        float t = Time.time;

        // 用 Perlin + 正弦做更自然的海浪感
        float n1 = Mathf.PerlinNoise(t * 0.37f, 0.13f) * 2f - 1f;
        float n2 = Mathf.PerlinNoise(0.21f, t * 0.41f) * 2f - 1f;

        float bob = Mathf.Sin(t * bobFrequency * Mathf.PI * 2f) * bobAmplitude * intensity + n1 * (bobAmplitude * 0.35f) * intensity;
        float swayX = Mathf.Sin(t * swayFrequency * Mathf.PI * 2f + 1.3f) * swayAmplitude * intensity;
        float swayZ = Mathf.Cos(t * (swayFrequency * 0.8f) * Mathf.PI * 2f + 0.7f) * swayAmplitude * 0.6f * intensity;

        // 以 rotateCamera 计算后的姿态为基准，叠加世界空间位移与姿态
        var camTransform = cameraObject.transform;
        Vector3 basePos = camTransform.position;
        Quaternion baseRot = camTransform.rotation;

        Vector3 offset = camTransform.right * swayX + Vector3.up * bob + camTransform.forward * swayZ;
        camTransform.position = basePos + offset;

        float roll = n2 * rollAmplitudeDeg * intensity;
        float pitch = n1 * pitchAmplitudeDeg * intensity;
        camTransform.rotation = baseRot * Quaternion.Euler(pitch, 0f, -roll);
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

    // CharacterController碰撞回调，用于处理与移动物体的碰撞
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // 如果碰撞到raft，确保玩家被推回
        // 这个方法会在CharacterController与碰撞体接触时自动调用
        Rigidbody body = hit.collider.attachedRigidbody;
        
        // 如果raft有Rigidbody且不是Kinematic，需要特殊处理
        if (body != null && !body.isKinematic)
        {
            // 计算推回力
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            body.linearVelocity = pushDir * 5f; // 给raft一个推力，防止玩家被推入raft内部
        }
    }

    // 检测玩家是否在船上
    void CheckIfOnRaft()
    {
        // 查找附近的船 - 使用更宽松的检测方式
        LayerMask checkLayer = raftLayer != -1 ? raftLayer : ~0;
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, onRaftCheckRadius, checkLayer);
        
        // 调试：每60帧输出一次检测信息
        if (enableDebugLogs && Time.frameCount % 60 == 0)
        {
            Debug.Log($"检测附近物体: 找到 {nearbyColliders.Length} 个碰撞体，检测半径: {onRaftCheckRadius}, 玩家位置: {transform.position}");
            foreach (Collider col in nearbyColliders)
            {
                RaftController raft = col.GetComponent<RaftController>();
                if (raft == null) raft = col.GetComponentInParent<RaftController>();
                Debug.Log($"  - {col.name}, 距离: {Vector3.Distance(transform.position, col.transform.position):F2}, 有RaftController: {raft != null}");
            }
        }
        
        RaftController foundRaft = null;
        float closestDistance = float.MaxValue;
        
        // 方法1: 通过碰撞体查找
        foreach (Collider col in nearbyColliders)
        {
            RaftController raft = col.GetComponent<RaftController>();
            if (raft == null)
            {
                // 尝试在父对象上查找
                raft = col.GetComponentInParent<RaftController>();
            }
            
            if (raft != null)
            {
                // 检查玩家是否真的在船上
                if (raft.IsPlayerOnRaft(transform, onRaftCheckRadius))
                {
                    float distance = Vector3.Distance(transform.position, raft.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        foundRaft = raft;
                    }
                }
            }
        }
        
        // 方法2: 如果方法1没找到，直接查找所有RaftController
        if (foundRaft == null)
        {
            RaftController[] allRafts = FindObjectsOfType<RaftController>();
            foreach (RaftController raft in allRafts)
            {
                if (raft.IsPlayerOnRaft(transform, onRaftCheckRadius))
                {
                    float distance = Vector3.Distance(transform.position, raft.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        foundRaft = raft;
                    }
                }
            }
        }
        
        // 更新当前所在的船
        if (foundRaft != null && foundRaft != currentRaft)
        {
            currentRaft = foundRaft;
            timeOnRaft = 0f; // 重置在船上的时间
            isControllingRaft = false; // 重置控制状态
            Debug.Log($"玩家上船了！船名: {currentRaft.name}, 距离: {closestDistance:F2}");
        }
        else if (foundRaft == null && currentRaft != null)
        {
            // 检查是否还在之前的船上
            if (!currentRaft.IsPlayerOnRaft(transform, onRaftCheckRadius))
            {
                Debug.Log("玩家下船了！");
                DetachFromRaft();
            }
        }
    }

    // 更新船的控制状态
    void UpdateRaftControl()
    {
        if (currentRaft == null)
        {
            Debug.LogWarning("UpdateRaftControl: currentRaft is null!");
            return;
        }

        // 累计在船上的时间
        timeOnRaft += Time.deltaTime;

        // 如果超过延迟时间，切换到船控制模式
        if (!isControllingRaft && timeOnRaft >= switchToRaftControlDelay)
        {
            isControllingRaft = true;
            AttachPlayerToRaft();
            Debug.Log($"切换到船控制模式！使用WASD控制船移动。船名: {currentRaft.name}, Rigidbody: {currentRaft.GetComponent<Rigidbody>() != null}");
        }

        // 如果已经切换到船控制模式，使用WASD控制船
        if (isControllingRaft)
        {
            ControlRaftWithWASD();
        }
        else
        {
            // 调试：显示倒计时
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"等待切换到船控制模式: {timeOnRaft:F1} / {switchToRaftControlDelay:F1} 秒");
            }
        }
    }

    // 使用WASD控制船
    void ControlRaftWithWASD()
    {
        if (currentRaft == null)
        {
            Debug.LogWarning("ControlRaftWithWASD: currentRaft is null!");
            return;
        }

        // 获取WASD输入（move已经在Update中获取了）
        Vector2 input = move;
        
        if (input.magnitude > 0.1f)
        {
            // 计算移动方向（基于摄像机方向）
            Transform camTransform = cameraObject.transform;
            if (camTransform == null)
            {
                Debug.LogError("ControlRaftWithWASD: camTransform is null!");
                return;
            }
            
            Vector3 camForward = camTransform.forward;
            Vector3 camRight = camTransform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();
            
            // 计算目标移动方向
            Vector3 moveDirection = camForward * input.y + camRight * input.x;
            moveDirection.Normalize();
            
            // 使用直接控制模式，设置移动方向
            currentRaft.SetDirectControlDirection(moveDirection);
            
            // 调试输出（每60帧输出一次，避免刷屏）
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"控制船移动: 输入 {input}, 方向 {moveDirection}, 船速度 {currentRaft.GetComponent<Rigidbody>()?.linearVelocity.magnitude ?? 0:F2}");
            }
        }
        else
        {
            // 没有输入时，清除控制，让船自然减速
            currentRaft.ClearTarget();
        }
    }

    void AttachPlayerToRaft()
    {
        if (currentRaft == null)
        {
            return;
        }

        if (characterController != null && characterController.enabled)
        {
            characterController.enabled = false;
        }

        transform.SetParent(currentRaft.transform, true);

        previousCameraDistance = cameraDistance;

        Vector3 targetPosition = currentRaft.transform.position + raftStandOffset;
        if (currentRaft.standPoint != null)
        {
            targetPosition = currentRaft.standPoint.position;
        }

        transform.position = targetPosition;
        Vector3 lookDirection = currentRaft.transform.forward;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        velocity = Vector3.zero;

        if (cameraObject != null)
        {
            raftCameraDistance = 20f; // 上船后距离设为10f（更靠后）
            cameraPitch = 15f;    // 上船后抬头15度（视角更高）
            cameraPitch = Mathf.Clamp(cameraPitch, minPitch, maxPitch);
            // 强制刷新相机位置
            rotateCamera(Vector2.zero, sensitivity, raftCameraDistance);
        }
    }

    void DetachFromRaft()
    {
        if (transform.parent == currentRaft?.transform)
        {
            transform.SetParent(originalParent, true);
        }

        if (characterController != null && !characterController.enabled)
        {
            characterController.enabled = true;
        }

        currentRaft?.ClearTarget();
        currentRaft = null;
        timeOnRaft = 0f;
        isControllingRaft = false;
        velocity = Vector3.zero;
        cameraDistance = previousCameraDistance;
    }

    void HandleDismountInput()
    {
        if (currentRaft == null)
        {
            Debug.LogWarning("HandleDismountInput: currentRaft is null!");
            return;
        }

        if (!isControllingRaft)
        {            
            // 情况1：刚上船但未进入控制模式
            // → 强制进入船控制模式
            isControllingRaft = true;
            timeOnRaft = switchToRaftControlDelay;
            AttachPlayerToRaft();
            if (enableDebugLogs) Debug.Log($"[PlayerController] 按F键，开始控制船: {currentRaft.name}");
        }
        else
        {
            // 情况2：已经在控制船，尝试下船
            // → 检查是否可以下船（必须靠近岛屿）

            // 检查航海系统是否存在
            if (VoyageSystemManager.Instance == null)
            {
                if (enableDebugLogs) Debug.LogWarning("[PlayerController] VoyageSystemManager不存在，执行简单下船");
                PerformDismount();
                return;
            }

            // 检查是否是当前控制的船
            if (VoyageSystemManager.Instance.CurrentRaft != currentRaft)
            {
                if (enableDebugLogs) Debug.LogWarning("[PlayerController] 当前船不是航海系统记录的船，执行简单下船");
                PerformDismount();
                return;
            }

            // 检查是否可以下船（靠近岛屿）
            if (!VoyageSystemManager.Instance.CanLeaveRaft())
            {
                if (enableDebugLogs) Debug.Log("[PlayerController] 不能在海上下船，必须靠近岛屿！");
                return;
            }

            // 所有检查通过，执行下船
            VoyageSystemManager.Instance.LeaveRaft();
            if (enableDebugLogs) Debug.Log("[PlayerController] 通过VoyageSystemManager下船");
        }
    }

    void PerformDismount()
    {
        if (currentRaft == null)
        {
            return;
        }

        Transform raftTransform = currentRaft.transform;
        Vector3 exitDirection = raftTransform.right;
        if (exitDirection.sqrMagnitude < 0.001f)
        {
            exitDirection = raftTransform.forward;
        }

        Vector3 exitPosition = raftTransform.position + exitDirection.normalized * dismountPushDistance + Vector3.up * dismountUpOffset;

        DetachFromRaft();
        transform.position = exitPosition;
        if (PlayerModeManager.Instance != null)
        {
            PlayerModeManager.Instance.OnLeaveRaft();
        }
    }

    /// <summary>
    /// 供外部（航海触发器）调用：强制让玩家上指定的船并立即进入控制模式
    /// </summary>
    public void ForceBoardRaft(RaftController raft)
    {
        if (raft == null) return;

        currentRaft = raft;
        timeOnRaft = switchToRaftControlDelay;
        isControllingRaft = true;
        AttachPlayerToRaft();
        if (PlayerModeManager.Instance != null)
        {
            PlayerModeManager.Instance.OnBoardRaft();
        }
        Debug.Log($"ForceBoardRaft: 已上船并进入控制模式 -> {raft.name}");
    }
    
    /// <summary>
    /// 检查玩家是否在船上
    /// </summary>
    public bool IsOnRaft()
    {
        return currentRaft != null;
    }
    
    /// <summary>
    /// 检查玩家是否正在控制船
    /// </summary>
    public bool IsControllingRaft()
    {
        return isControllingRaft && currentRaft != null;
    }

    /// <summary>
    /// 强制下船（供外部系统调用）
    /// </summary>
    public void ForceLeaveRaft()
    {
        if (currentRaft == null)
        {
            Debug.LogWarning("[PlayerController] ForceLeaveRaft: 玩家不在船上，无法下船");
            return;
        }

        // 调用原有的下船逻辑
        PerformDismount();
    }
}
