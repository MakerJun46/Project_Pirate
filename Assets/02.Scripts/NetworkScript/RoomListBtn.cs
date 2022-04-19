using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListBtn : MonoBehaviour
{
    [SerializeField] private Text titleTxt;
    [SerializeField] Text memberCountTxt;
    private string roomName;
    private int playerCount;

    public void InitializeRoomLIstInfo(string _roomName, int _playerCount, int _maxPalyers)
    {
        roomName = _roomName;
        playerCount = _playerCount;
        titleTxt.text = _roomName;
        memberCountTxt.text = "[" + _playerCount + "/" + _maxPalyers + "]";
    }
    public void SetSelectedRoomTitle()
    {
        LobbyManager networkController = FindObjectOfType<LobbyManager>();
        networkController.roomName = titleTxt.text;
        networkController.JoinRoom();
    }
    public bool TitleEqualTo(string _title)
    {
        return roomName == _title;
    }
}
