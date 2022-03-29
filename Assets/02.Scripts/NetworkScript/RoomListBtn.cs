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
        LobbyManager networkController = FindObjectOfType<LobbyManager>();
        networkController.roomName = titleTxt.text;
        networkController.JoinRoom();
    }
}
