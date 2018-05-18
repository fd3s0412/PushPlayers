using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class WebSocketPointManager : MonoBehaviour
{
    private WebSocket ws;
    private string playerId;
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
        playerInfoMap = new Dictionary<string, SocketData>();
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
    private void Send(string eventName, SocketDataListWrapper data)
    {
        data.eventName = eventName;
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
        }
        else
        {
            SendClientInfo();
        }
        RecieveInfo();
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
                Transform trans = player.transform;
                Rigidbody rigid = player.GetComponent<Rigidbody>();
                playerData.positionX = trans.position.x;
                playerData.positionY = trans.position.y;
                playerData.positionZ = trans.position.z;
                playerData.velocityX = rigid.velocity.x;
                playerData.velocityY = rigid.velocity.y;
                playerData.velocityZ = rigid.velocity.z;
                playerDataList.Add(playerData);
            }
        }
        SocketDataListWrapper sendData = new SocketDataListWrapper();
        sendData.playerList = playerDataList;
        Send("sendHostInfo", sendData);
    }
    // ----------------------------------------------------------------------
    // クライアントの状態に応じてサーバーに要求を送信する処理.
    // ----------------------------------------------------------------------
    private void SendClientInfo()
    {
        if (playerId == null)
        {
            Send("login", new SocketData());
        }
        else
        {
            SendPlayerAction();
        }
    }
    // ----------------------------------------------------------------------
    // サーバーからの要求を受け取る処理.
    // ----------------------------------------------------------------------
    private void RecieveInfo()
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
                case "sendHostInfo":
                    SetPlayerInfo(recieveDataListWrapper);
                    break;
            }
            jsonString = ws.RecvString();
        }
        //Debug.Log(jsonString);
    }
    // ----------------------------------------------------------------------
    // ログインのコールバック処理.
    // ----------------------------------------------------------------------
    private void LoginCallback(SocketData playerInfo)
    {
        if (playerId == null)
        {
            playerId = playerInfo.playerId;
            Send("loginSuccess", playerInfo);
            Debug.Log("loginSuccess : " + playerId);

            GameObject instans = CreatePlayer(playerInfo);
            playerInfoMap.Add(playerInfo.playerId, playerInfo);

            CameraManager.player = instans;
        }
    }
    // ----------------------------------------------------------------------
    // 操作情報を送信する処理.
    // ----------------------------------------------------------------------
    private void SendPlayerAction()
    {
        float x = Input.GetAxis("Horizontal");
        if (x == 0) x = Input.acceleration.x;
        float z = Input.GetAxis("Vertical");
        if (z == 0) z = Input.acceleration.y;

        SocketData sendData = new SocketData();
        sendData.playerId = playerId;
        sendData.horizontal = x;
        sendData.vertical = z;
        Send("setPlayerAction", sendData);
    }
    // ----------------------------------------------------------------------
    // ホストで計算したプレイヤー情報などをキャッシュする処理.
    // ----------------------------------------------------------------------
    private void SetPlayerInfo(SocketDataListWrapper listWrapper)
    {
        if (listWrapper.playerList != null)
        {
            int size = listWrapper.playerList.Count;
            for (int i = 0; i < size; i++)
            {
                SocketData playerInfo = listWrapper.playerList[i];
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
}
