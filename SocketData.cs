using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SocketData
{
    // Javascriptの命名規則に合わせる
    public string playerId;
    
	public string eventName;

    public bool isDelete;

	public float playerColorR;
    public float playerColorG;
    public float playerColorB;
    
	public float positionX;
    public float positionY;
    public float positionZ;

    public float velocityX;
    public float velocityY;
    public float velocityZ;

    public float horizontal;
	public float vertical;
}
