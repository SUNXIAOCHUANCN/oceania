using UnityEngine;
using UnityEditor;
using System.IO;

public class TempClearSaves : EditorWindow
{
    [MenuItem("Tools/临时调试/删除所有存档")]
    public static void DeleteAllSaves()
    {
        string basePath = Application.persistentDataPath;
        
        string[] files = new string[]
        {
            "player_state.dat",
            "farm_save.dat",
            "forest_save.dat",
            "ranch_save.dat",
            "explore_save.dat",
            "population_save.dat"
        };
        
        int deletedCount = 0;
        foreach (string file in files)
        {
            string path = Path.Combine(basePath, file);
            if (File.Exists(path))
            {
                File.Delete(path);
                deletedCount++;
                Debug.Log($"已删除: {file}");
            }
        }
        
        // 清除PlayerPrefs
        PlayerPrefs.DeleteKey("GlobalTimeSystem_TotalElapsedTime");
        PlayerPrefs.DeleteKey("GlobalTimeSystem_FirstStart");
        PlayerPrefs.Save();
        
        EditorUtility.DisplayDialog("完成", $"已删除 {deletedCount} 个存档文件", "确定");
    }
}
