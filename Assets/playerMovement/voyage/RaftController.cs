using UnityEngine;

public class RaftController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f; // 船移动速度
    public float rotationSpeed = 2f; // 船旋转速度
    public float maxSpeed = 10f; // 最大速度
    public float acceleration = 5f; // 加速度
    public float deceleration = 8f; // 减速度
    
    [Header("物理设置")]
    public Rigidbody raftRigidbody; // 船的Rigidbody组件

    [Header("玩家站位")]
    public Transform standPoint; // 角色被绑定时的默认站位
    
    private Vector3 targetPosition; // 目标位置
    private bool hasTarget = false; // 是否有目标
    private Vector3 currentVelocity; // 当前速度
    private Vector3 inputDirection; // 输入方向（用于WASD控制）
    private bool useDirectControl = false; // 是否使用直接控制（WASD）
    
    void Start()
    {
        // 如果没有指定Rigidbody，尝试获取
        if (raftRigidbody == null)
        {
            raftRigidbody = GetComponent<Rigidbody>();
        }
        
        // 如果还是没有，创建一个
        if (raftRigidbody == null)
        {
            raftRigidbody = gameObject.AddComponent<Rigidbody>();
            raftRigidbody.useGravity = false; // 船不需要重力
            raftRigidbody.linearDamping = 2f; // 添加阻力，让船自然减速
            raftRigidbody.angularDamping = 5f; // 角阻力
        }
        
        // 确保船是Kinematic或者有合适的物理设置
        raftRigidbody.isKinematic = false;
        raftRigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
    }
    
    void FixedUpdate()
    {
        if (useDirectControl)
        {
            // 直接控制模式（WASD）
            MoveWithDirectInput();
        }
        else if (hasTarget)
        {
            // 目标位置模式（鼠标点击）
            MoveTowardsTarget();
        }
        else
        {
            // 没有目标时，自然减速
            Decelerate();
        }
    }
    
    // 设置目标位置（鼠标点击位置或WASD控制）
    public void SetTargetPosition(Vector3 target)
    {
        targetPosition = target;
        targetPosition.y = transform.position.y; // 保持Y轴不变（水面高度）
        hasTarget = true;
    }
    
    // 清除目标
    public void ClearTarget()
    {
        hasTarget = false;
        useDirectControl = false;
        inputDirection = Vector3.zero;
    }

    // 设置直接控制方向（用于WASD控制）
    public void SetDirectControlDirection(Vector3 direction)
    {
        inputDirection = direction;
        useDirectControl = true;
        hasTarget = false; // 清除目标模式
    }
    
    // 朝目标移动
    private void MoveTowardsTarget()
    {
        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0; // 只考虑水平方向
        float distance = direction.magnitude;
        
        // 如果已经到达目标附近，停止
        if (distance < 0.5f)
        {
            hasTarget = false;
            Decelerate();
            return;
        }
        
        direction.Normalize();
        
        // 计算目标速度
        Vector3 targetVelocity = direction * moveSpeed;
        
        // 平滑加速到目标速度
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        
        // 限制最大速度
        if (currentVelocity.magnitude > maxSpeed)
        {
            currentVelocity = currentVelocity.normalized * maxSpeed;
        }
        
        // 应用速度
        if (raftRigidbody != null)
        {
            raftRigidbody.linearVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        }
        else
        {
            transform.position += currentVelocity * Time.fixedDeltaTime;
        }
        
        // 让船朝向移动方向
        if (currentVelocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
    
    // 直接控制移动（WASD模式）
    private void MoveWithDirectInput()
    {
        if (inputDirection.magnitude < 0.1f)
        {
            // 没有输入，减速
            Decelerate();
            return;
        }

        // 计算目标速度
        Vector3 targetVelocity = inputDirection.normalized * moveSpeed;
        
        // 平滑加速到目标速度
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        
        // 限制最大速度
        if (currentVelocity.magnitude > maxSpeed)
        {
            currentVelocity = currentVelocity.normalized * maxSpeed;
        }
        
        // 应用速度
        if (raftRigidbody != null)
        {
            raftRigidbody.linearVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        }
        else
        {
            transform.position += currentVelocity * Time.fixedDeltaTime;
        }
        
        // 让船朝向移动方向
        if (currentVelocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    // 减速
    private void Decelerate()
    {
        currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, deceleration * Time.fixedDeltaTime);
        
        if (raftRigidbody != null)
        {
            raftRigidbody.linearVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
        }
        else
        {
            transform.position += currentVelocity * Time.fixedDeltaTime;
        }
        
        // 如果速度很小，直接停止
        if (currentVelocity.magnitude < 0.01f)
        {
            currentVelocity = Vector3.zero;
            if (raftRigidbody != null)
            {
                raftRigidbody.linearVelocity = Vector3.zero;
            }
        }
    }
    
    // 检查玩家是否在船上（通过检测玩家位置是否在船的碰撞范围内）
    public bool IsPlayerOnRaft(Transform playerTransform, float checkRadius = 2f)
    {
        if (playerTransform == null) return false;
        
        Vector3 playerPos = playerTransform.position;
        Vector3 raftPos = transform.position;
        
        // 计算水平距离
        float horizontalDistance = Vector3.Distance(
            new Vector3(playerPos.x, 0, playerPos.z),
            new Vector3(raftPos.x, 0, raftPos.z)
        );
        
        // 检查垂直距离（玩家应该在船的上方或附近）
        float verticalDistance = Mathf.Abs(playerPos.y - raftPos.y);
        
        // 如果水平距离在检查范围内，且垂直距离合理（玩家在船上）
        bool isOnRaft = horizontalDistance <= checkRadius && verticalDistance <= 3f;
        
        // 调试输出（每60帧一次，只在接近时输出）
        if (horizontalDistance <= checkRadius * 2f && Time.frameCount % 60 == 0)
        {
            Debug.Log($"IsPlayerOnRaft检查 [{name}]: 水平距离={horizontalDistance:F2}/{checkRadius}, 垂直距离={verticalDistance:F2}/3, 结果={isOnRaft}");
        }
        
        return isOnRaft;
    }
}

