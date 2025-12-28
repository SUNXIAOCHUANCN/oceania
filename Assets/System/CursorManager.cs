using UnityEngine;

public class CursorManager : MonoBehaviour
{
    // 初始化时默认隐藏
    void Start()
    {
        SetCursorState(false);
    }

    /// <summary>
    /// 外部接口：设置鼠标状态
    /// </summary>
    /// <param name="isVisible">true=显示鼠标(此时不能转视角), false=隐藏鼠标(锁定并可以转视角)</param>
    public void SetCursorState(bool isVisible)
    {
        if (isVisible)
        {
            // 显示鼠标，允许自由移动
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // 隐藏鼠标，锁定在屏幕中心
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}