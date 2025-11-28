using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("移动与控制器设置")]
    public float speed = 5f;
    private Vector2 move;          // 移动输入（WASD 或手柄左摇杆）
    private Vector2 mouseLook;     // 鼠标输入（用于瞄准）

    public Vector2 Move => move;
    private Vector3 rotationTarget;
    public bool isPC = true;
    private CharacterController characterController;
    private Vector3 velocity;
    private float gravity = -9.8f;



    [Header("动画器设置")]
    public Animator playerAnimator;

    // 使用 Player Input 的 Input Actions
    private PlayerInput playerInput;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        characterController = GetComponent<CharacterController>();

        if (GetComponent<Animator>() != null)
            playerAnimator = GetComponent<Animator>();
        else
            playerAnimator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // 获取输入（通过 Player Input 组件）
        move = playerInput.actions.FindAction("Move").ReadValue<Vector2>();
        mouseLook = playerInput.actions.FindAction("MouseLook").ReadValue<Vector2>();

        // 处理重力
        if (characterController.isGrounded && velocity.y < 0)
            velocity.y = 0f;

        // 鼠标瞄准射线
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(mouseLook);
        if (Physics.Raycast(ray, out hit))
        {
            rotationTarget = hit.point;
        }

        MovePlayerWithLook();

        velocity.y += gravity * Time.deltaTime;
    }

    public void MovePlayerWithLook()
    {
        
            var lookPos = rotationTarget - transform.position;
            lookPos.y = 0;
            if (lookPos != Vector3.zero)
            {
                var rotation = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
            }


        Vector3 movement = new Vector3(move.x, 0f, move.y);
        float moveSpeed = movement.magnitude;
        playerAnimator?.SetFloat("MoveSpeed", moveSpeed);

        Vector3 moveVector = movement * speed * Time.deltaTime;
        moveVector.y = velocity.y * Time.deltaTime;
        characterController.Move(moveVector);
    }
}