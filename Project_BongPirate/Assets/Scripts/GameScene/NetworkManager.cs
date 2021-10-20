using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject DisconnetPanel;
    public GameObject RespawnPanel;
    public PhotonView PV;
    public Camera MainCamera;

    private void Awake()
    {
        Screen.SetResolution(960, 540, false);
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    private void Start()
    {
        Connect();
    }


    /// <summary>
    /// ��Ʈ��ũ ������ ���� �õ�
    /// </summary>
    public void Connect() => PhotonNetwork.ConnectUsingSettings();


    /// <summary>
    /// �÷��̾ ��Ʈ��ũ�� ������ ������ ȣ��
    /// </summary>
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("NickName");
        PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 4 }, null);
        Debug.Log("Conneted to Master");
    }

    /// <summary>
    /// photon ������ ��(Room)�� �շ��� ������ ȣ��
    /// </summary>
    public override void OnJoinedRoom()
    {
        DisconnetPanel.SetActive(false);
        Spawn();
    }

    /// <summary>
    /// photon ��Ʈ��ũ�� ������ ������ ������ ȣ��
    /// </summary>
    /// <param name="cause"></param>
    public override void OnDisconnected(DisconnectCause cause)
    {
        DisconnetPanel.SetActive(true);
        RespawnPanel.SetActive(false);
    }

    /// <summary>
    /// �÷��̾� �� ����
    /// </summary>
    public void Spawn()
    {
        if(PhotonNetwork.IsMasterClient) // ���� ���� ������ ���� ����
        {
            GameObject go = PhotonNetwork.Instantiate("Raft", new Vector3(100, 0, 100), Quaternion.Euler(0, 90, 0));
        }
        else
        {
            GameObject go = PhotonNetwork.Instantiate("Ship", new Vector3(-100, 0, -100), Quaternion.Euler(0, 90, 0));
        }
        RespawnPanel.SetActive(false);
    }


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected) // ESC �Է� �� ���� ������
        {
            PhotonNetwork.Disconnect();
        }
    }

}
