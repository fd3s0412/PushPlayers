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
    private GameObject field;
    public GameObject anotherPlayerPrefab;
    public static bool isHost;
    public static Dictionary<string, SocketData> playerInfoMap = new Dictionary<string, SocketData>();
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
    private void Send(string eventName, SocketData data)
    {
        data.eventName = eventName;
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
        Send("login", sendData);
    }
    // ----------------------------------------------------------------------
    // ログインのコールバック処理.
    // ----------------------------------------------------------------------
    private void LoginCallback(SocketData playerInfo)
    {
        if (sendData.playerId == null)
        {
            //Debug.Log("LoginCallback");
            //Debug.Log(recieveData.positionY);

            sendData = playerInfo;
            Send("loginSuccess", sendData);

            GameObject instans = CreatePlayer(playerInfo);
            playerInfoMap.Add(playerInfo.playerId, playerInfo);

            CameraManager.player = instans;
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

        sendData.horizontal = x;
        sendData.vertical = z;
        Send("setPlayerAction", sendData);
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
            int size = recieveData.playerList.Count;
            for (int i = 0; i < size; i++)
            {
                SocketData playerInfo = recieveData.playerList[i];
                if (playerInfoMap.ContainsKey(playerInfo.playerId))
                {
                    playerInfoMap[playerInfo.playerId] = playerInfo;
                } else
                {
                    playerInfoMap.Add(playerInfo.playerId, playerInfo);
                    if (!playerInfo.isDelete) CreatePlayer(playerInfo);
                }
            }
        }
    }
    // ----------------------------------------------------------------------
    // ユーザー生成処理.
    // ----------------------------------------------------------------------
    private GameObject CreatePlayer(SocketData playerInfo)
    {
        Debug.Log("createPlayer : " + playerInfo.playerId);
        GameObject instans = Instantiate(anotherPlayerPrefab, new Vector3(playerInfo.positionX, playerInfo.positionY, playerInfo.positionZ), Quaternion.identity);
        instans.GetComponent<Renderer>().material.color = new Color(playerInfo.playerColorR, playerInfo.playerColorG, playerInfo.playerColorB, 1f);
        instans.name = playerInfo.playerId;
        instans.transform.parent = players.transform;
        return instans;
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
        SetFieldInfo(recieveData);
        SetPlayerInfo(recieveData);
    }
}
