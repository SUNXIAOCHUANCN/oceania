using UnityEngine;

public class ExploreSystemTester : MonoBehaviour
{
    [Header("测试用的管理器")]
    public ProgressTableManager testManager;

    [Header("测试数据")]
    public string testSpeciesName;
    public string testClueName;

    public void TestUnlockSpecies()
    {
        if (testManager != null)
        {
            testManager.UnlockSpecies(testSpeciesName);
            Debug.Log($"尝试解锁物种: {testSpeciesName}");
        }
    }

    public void TestUnlockClue()
    {
        if (testManager != null)
        {
            testManager.UnlockClue(testClueName);
            Debug.Log($"尝试解锁线索: {testClueName}");
        }
    }

    public void TestUnlockSecret()
    {
        if (testManager != null)
        {
            testManager.UnlockSecret();
            Debug.Log("尝试解锁秘密");
        }
    }

    public void TestUnlockToken()
    {
        if (testManager != null)
        {
            testManager.UnlockToken();
            Debug.Log("尝试解锁信物");
        }
    }
}