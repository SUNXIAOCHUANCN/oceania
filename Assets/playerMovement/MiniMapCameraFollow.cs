using UnityEngine;

public class MiniMapCameraFollow : MonoBehaviour
{
    public Transform playerTransform;
    public float fixedYPosition = 80f;
    
    void Start()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("MiniMapCameraFollow: 未找到Player标签的游戏对象，请手动分配playerTransform。");
            }
        }
    }
    
    void LateUpdate()
    {
        if (playerTransform != null)
        {
            Vector3 playerPos = playerTransform.position;
            transform.position = new Vector3(playerPos.x, fixedYPosition, playerPos.z);
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}