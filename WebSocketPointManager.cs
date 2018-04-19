using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class WebSocketPointManager : MonoBehaviour
{
    private WebSocket ws;
    private SocketData sendData;
    public GameObject players;
    public GameObject player;
    private GameObject field;
    public GameObject anotherPlayerPrefab;
    private bool isHost;
    private Dictionary<String, GameObject> playerInstansMap = new Dictionary<string, GameObject>();
    private Dictionary<String, Transform> playerTransformMap = new Dictionary<string, Transform>();
    // ----------------------------------------------------------------------
    // ゲーム開始時の処理.
    // ----------------------------------------------------------------------
    private IEnumerator Start()
    {
        sendData = new SocketData();
        //Debug.Log(anotherPlayer);

        ws = new WebSocket(new Uri("ws://153.126.204.61:8002"));

        field = GameObject.FindGameObjectsWithTag("Field")[0];

        isHost = PlayerPrefs.GetString("isHost", "False").Equals("True");
        if (isHost)
        {
            Destroy(player);
        }

        yield return StartCoroutine(ws.Connect());
        yield return 0;
    }
    // ----------------------------------------------------------------------
    // ゲーム終了時の処理.
    // ----------------------------------------------------------------------
    private void OnDestroy()
    {
        // 接続を切る.
        ws.Close();
    }
    // ----------------------------------------------------------------------
    // サーバーに要求を送信する処理.
    // ----------------------------------------------------------------------
    private void Send(SocketData data)
    {
        string json = JsonUtility.ToJson(data);
        //Debug.Log(json);
        ws.SendString(json);
    }
    private void Send(SocketDataListWrapper data)
    {
        string json = JsonUtility.ToJson(data);
        //Debug.Log(json);
        ws.SendString(json);
    }
    // ----------------------------------------------------------------------
    // 常時呼ばれる処理.
    // ----------------------------------------------------------------------
    private void Update()
    {
        if (isHost)
        {
            SendHostInfo();
            RecieveHostInfo();
        }
        else
        {
            SendClientInfo();
            RecieveClientInfo();
        }
    }
    // ----------------------------------------------------------------------
    // クライアントの状態に応じてサーバーに要求を送信する処理.
    // ----------------------------------------------------------------------
    private void SendClientInfo()
    {
        if (sendData.playerId == null)
        {
            Login();
        }
        else
        {
            SetPlayerAction();
        }
    }
    // ----------------------------------------------------------------------
    // サーバーからの要求を受け取る処理.
    // ----------------------------------------------------------------------
    private void RecieveClientInfo()
    {
        string jsonString = ws.RecvString();
        while (jsonString != null)
        {
            SocketDataListWrapper recieveDataListWrapper = JsonUtility.FromJson<SocketDataListWrapper>(jsonString);
            switch (recieveDataListWrapper.eventName)
            {
                case "login":
                    SocketData recieveData = JsonUtility.FromJson<SocketData>(jsonString);
                    LoginCallback(recieveData);
                    break;
                case "setPlayerAction":
                    SetPlayerActionCallback(recieveDataListWrapper);
                    break;
            }
            jsonString = ws.RecvString();
        }
        //Debug.Log(jsonString);
    }
    // ----------------------------------------------------------------------
    // ログイン処理.
    // ----------------------------------------------------------------------
    private void Login()
    {
        sendData.eventName = "login";
        Send(sendData);
    }
    // ----------------------------------------------------------------------
    // ログインのコールバック処理.
    // ----------------------------------------------------------------------
    private void LoginCallback(SocketData recieveData)
    {
        if (sendData.playerId == null)
        {
            //Debug.Log("LoginCallback");
            //Debug.Log(recieveData.positionY);

            sendData = recieveData;
            sendData.eventName = "loginSuccess";
            Send(sendData);

            player.GetComponent<Renderer>().material.color = new Color(recieveData.playerColorR, recieveData.playerColorG, recieveData.playerColorB, 1f);
            player.transform.position = new Vector3(recieveData.positionX, recieveData.positionY, recieveData.positionZ);
            player.name = recieveData.playerId;
            player.SetActive(true);
            playerInstansMap.Add(sendData.playerId, player);
        }
    }
    // ----------------------------------------------------------------------
    // 操作情報を送信する処理.
    // ----------------------------------------------------------------------
    private void SetPlayerAction()
    {
        float x = Input.GetAxis("Horizontal");
        if (x == 0) x = Input.acceleration.x;
        float z = Input.GetAxis("Vertical");
        if (z == 0) z = Input.acceleration.y;

        sendData.eventName = "setPlayerAction";
        sendData.horizontal = x;
        sendData.vertical = z;
        Send(sendData);
    }
    // ----------------------------------------------------------------------
    // ゲーム情報取得のコールバック処理.
    // ----------------------------------------------------------------------
    private void SetPlayerActionCallback(SocketDataListWrapper recieveData)
    {
        SetFieldInfo(recieveData);
        SetPlayerInfo(recieveData);
    }
    // ----------------------------------------------------------------------
    // マップ情報を元に描画する処理.
    // ----------------------------------------------------------------------
    private void SetFieldInfo(SocketDataListWrapper recieveData)
    {
        //if (recieveData.fieldUpdate)
        //{
        //    field.transform.localScale = new Vector3(recieveData.fieldX, recieveData.fieldY, recieveData.fieldZ);
        //}
    }
    // ----------------------------------------------------------------------
    // プレイヤー情報を元に描画する処理.
    // ----------------------------------------------------------------------
    private void SetPlayerInfo(SocketDataListWrapper recieveData)
    {
        if (recieveData.playerList != null)
        {
            SocketData playerInfo = null;
            Transform transform = null;
            int size = recieveData.playerList.Count;
            for (int i = 0; i < size; i++) // ループをやめる
            {
                playerInfo = recieveData.playerList[i];
                transform = GetPlayerTransformById(playerInfo);
                if (transform)
                {
                    transform.position = new Vector3(playerInfo.positionX, playerInfo.positionY, playerInfo.positionZ);
                }
            }
        }
    }
    // ----------------------------------------------------------------------
    // ユーザー生成処理.
    // ----------------------------------------------------------------------
    private void CreatePlayer(ref GameObject instans, SocketData playerInfo)
    {
        Debug.Log("createPlayer");
        instans = Instantiate(anotherPlayerPrefab, new Vector3(playerInfo.positionX, playerInfo.positionY, playerInfo.positionZ), Quaternion.identity);
        instans.GetComponent<Renderer>().material.color = new Color(playerInfo.playerColorR, playerInfo.playerColorG, playerInfo.playerColorB, 1f);
        instans.name = playerInfo.playerId;
        instans.transform.parent = players.transform;
    }
    // ----------------------------------------------------------------------
    // ホスト情報を送信する処理.
    // ----------------------------------------------------------------------
    private void SendHostInfo()
    {
        GameObject[] playerList = GameObject.FindGameObjectsWithTag("Player");
        List<SocketData> playerDataList = new List<SocketData>();
        if (playerList.Length > 0)
        {
            for (int i = 0; i < playerList.Length; i++)
            {
                GameObject player = playerList[i];
                SocketData playerData = new SocketData();
                playerData.playerId = player.name;
                playerData.positionX = player.transform.position.x;
                playerData.positionY = player.transform.position.y;
                playerData.positionZ = player.transform.position.z;
                playerDataList.Add(playerData);
            }
        }
        SocketDataListWrapper sendData = new SocketDataListWrapper();
        sendData.playerList = playerDataList;
        sendData.eventName = "sendHostInfo";
        Send(sendData);
    }
    // ----------------------------------------------------------------------
    // ホスト用の情報を受信する処理.
    // ----------------------------------------------------------------------
    private void RecieveHostInfo()
    {
        string jsonString = ws.RecvString();
        //Debug.Log(jsonString);
        while (jsonString != null)
        {
            SocketDataListWrapper recieveData = JsonUtility.FromJson<SocketDataListWrapper>(jsonString);
            switch (recieveData.eventName)
            {
                case "sendHostInfo":
                    SendHostInfoCallback(recieveData);
                    break;
            }
            jsonString = ws.RecvString();
        }
    }
    // ----------------------------------------------------------------------
    // サーバーの情報に基づいてホストで描画する処理.
    // ----------------------------------------------------------------------
    private void SendHostInfoCallback(SocketDataListWrapper recieveData)
    {
        //Debug.Log("SendHostInfoCallback");
        //Debug.Log(recieveData);
        SetFieldInfo(recieveData);
        CalcActions(recieveData);
    }
    // ----------------------------------------------------------------------
    // クライアントの操作状態に基づいた動きを計算する処理.
    // ----------------------------------------------------------------------
    private void CalcActions(SocketDataListWrapper recieveData)
    {
        if (recieveData.playerList != null)
        {
            SocketData playerInfo = null;
            GameObject instans = null;
            int size = recieveData.playerList.Count;
            for (int i = 0; i < size; i++) // ループをやめる
            {
                playerInfo = recieveData.playerList[i];
                instans = GetPlayerById(playerInfo);
                if (instans)
                {
                    Rigidbody rigidbody = instans.GetComponent<Rigidbody>();
                    rigidbody.AddForce(playerInfo.horizontal * 5, 0, playerInfo.vertical * 5);
                }
            }
        }
    }
    // ----------------------------------------------------------------------
    // プレイヤーIDに基づいて、GameObjectを取得.
    // ----------------------------------------------------------------------
    private GameObject GetPlayerById(SocketData playerInfo)
    {
        GameObject instans;
        try
        {
            instans = playerInstansMap[playerInfo.playerId];
        }
        catch (KeyNotFoundException e)
        {
            instans = null;
        }

        // 削除フラグがONの場合、消す
        if (playerInfo.isDelete)
        {
            if (instans != null)
            {
                Destroy(instans);
            }
            return null;
        }

        // インスタンスがない場合、インスタンスを生成
        if (instans == null)
        {
            CreatePlayer(ref instans, playerInfo);
            if (isHost)
            {
                // 重力ON
                Rigidbody rigidbody = instans.GetComponent<Rigidbody>();
                rigidbody.useGravity = true;
                // 衝突判定ON
                SphereCollider collider = instans.GetComponent<SphereCollider>();
                collider.enabled = true;

            }
            playerInstansMap.Add(playerInfo.playerId, instans);
        }
        return instans;
    }
    // ----------------------------------------------------------------------
    // プレイヤーIDに基づいて、Transformを取得.
    // ----------------------------------------------------------------------
    private Transform GetPlayerTransformById(SocketData playerInfo) {
        Transform transform;
        try
        {
            transform = playerTransformMap[playerInfo.playerId];
        }
        catch (KeyNotFoundException e)
        {
            transform = null;
        }
        if (transform == null)
        {
            GameObject instans = GetPlayerById(playerInfo);
            if (instans != null)
            {
                transform = instans.transform;
                playerTransformMap.Add(playerInfo.playerId, transform);
            }
        }
        return transform;
    }
}
