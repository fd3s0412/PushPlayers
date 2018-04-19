using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SocketDataListWrapper {

    public string eventName;

    [SerializeField]
    public List<SocketData> playerList = new List<SocketData>();
}
