using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListBtn : MonoBehaviour
{
    public Text titleTxt;
    public Text memberCountTxt;
    public string roomName;

    public int playerCount;
    public void SetSelectedRoomTitle()
    {
        FindObjectOfType<NetworkController>().roomName = titleTxt.text;
        FindObjectOfType<NetworkController>().JoinRoom();
    }
}
