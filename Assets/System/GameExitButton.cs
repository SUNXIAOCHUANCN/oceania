using UnityEngine;

public class GameExitButton : MonoBehaviour
{
    /// <summary>
    /// 退出游戏
    /// 注意：GlobalSaveManager的OnApplicationQuit会自动保存所有数据
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("退出游戏...");

        #if UNITY_EDITOR
            // 在编辑器模式下停止播放
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // 在构建版本中退出应用
            Application.Quit();
        #endif
    }

    /// <summary>
    /// 手动保存并退出游戏
    /// </summary>
    public void SaveAndQuit()
    {
        Debug.Log("保存并退出游戏...");

        // 手动调用保存以确保数据保存（双保险）
        if (GlobalSaveManager.Instance != null)
        {
            GlobalSaveManager.Instance.SaveAllData();
        }

        // 延迟一帧后退出，确保保存完成
        Invoke(nameof(DoQuit), 0.2f);
    }

    private void DoQuit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
