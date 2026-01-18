using UnityEngine;

public class FarmTrigger : MonoBehaviour
{
    [Header("Button Canvas Controller")]
    [SerializeField] private ButtonCanvaController buttonCanvasController;
    
    [Header("Trigger Settings")]
    [SerializeField] private bool showOnEnter = true;
    [SerializeField] private bool hideOnExit = true;
    
    [Header("Target Tag (Optional)")]
    [SerializeField] private string targetTag = "Player";
    
    private void Start()
    {
        // 确保Collider是触发器
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        // 如果没有指定ButtonCanvasController，尝试自动查找
        if (buttonCanvasController == null)
        {
            buttonCanvasController = FindObjectOfType<ButtonCanvaController>();
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // 检查进入触发器的对象是否是我们期望的物体（比如玩家）
        if ((targetTag == "" || other.CompareTag(targetTag)) && showOnEnter && buttonCanvasController != null)
        {
            // 显示农场按钮
            buttonCanvasController.ShowButton("farm");
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // 当物体离开触发器时，隐藏所有按钮
        if ((targetTag == "" || other.CompareTag(targetTag)) && hideOnExit && buttonCanvasController != null)
        {
            buttonCanvasController.HideAllButtons();
        }
    }
}