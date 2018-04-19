using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPointManager : WebSocketPointManager
{
    private Transform trans;
    private Rigidbody rigid;

    private void Awake()
    {
        trans = transform;
        rigid = GetComponent<Rigidbody>();

        if (isHost)
        {
            // 重力ON
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.useGravity = true;
            // 衝突判定ON
            SphereCollider collider = GetComponent<SphereCollider>();
            collider.enabled = true;

        }
    }

    void Update()
    {
        try
        {
            SocketData playerInfo = playerInfoMap[this.gameObject.name];

            // 削除フラグがONの場合、消す
            if (playerInfo.isDelete)
            {
                Destroy(this.gameObject);
            }

            if (isHost)
            {
                rigid.AddForce(playerInfo.horizontal * 5, 0, playerInfo.vertical * 5);
            }
            else
            {
                trans.position = new Vector3(playerInfo.positionX, playerInfo.positionY, playerInfo.positionZ);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
}
