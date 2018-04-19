using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour
{
    public static GameObject player; // 玉のオブジェクト
    private Vector3 offset = new Vector3(0f, 10f, -10f);

    void LateUpdate()
    {
        if (player)
        {
            transform.position = player.transform.position + offset;
        }
    }
}