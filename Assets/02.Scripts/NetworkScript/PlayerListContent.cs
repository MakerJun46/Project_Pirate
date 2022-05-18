using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;

public class PlayerListContent : MonoBehaviour, IPunObservable 
{
    [SerializeField] private Text nameTxt;
    [SerializeField] private Image PlayerListImg;
    [SerializeField] private Image PlayerProfileImg;
    [SerializeField] private Color[] ReadyColors;
    [SerializeField] private Toggle readyToggle;
    [SerializeField] private Button KickBtn;

    public Photon.Realtime.Player myPlayer;
    private LobbyManager lobbyManager;

    private void Start()
    {
        myPlayer = GetComponent<PhotonView>().Owner;
        lobbyManager = FindObjectOfType<LobbyManager>();
        if (PhotonNetwork.IsMasterClient && myPlayer != PhotonNetwork.LocalPlayer)
        {
            KickBtn.onClick.AddListener(() => lobbyManager.KickPlayer(myPlayer.UserId));
        }
        else
        {
            KickBtn.gameObject.SetActive(false);
        }
        GetComponent<PhotonView>().RPC("doEnable", RpcTarget.AllBuffered);
    }


    [PunRPC]
    public void doEnable()
    {
        if (lobbyManager == null)
            lobbyManager = FindObjectOfType<LobbyManager>();

        transform.SetParent(lobbyManager.PlayerListContainer);
    }

    private void Update()
    {
        this.transform.localScale = Vector3.one;
        if (myPlayer != null)
        {
            nameTxt.text = myPlayer.NickName;

            if ((string)myPlayer.CustomProperties["Ready"] == "0") {
                readyToggle.isOn = false;
                PlayerListImg.color = ReadyColors[0];
            }
            else {
                readyToggle.isOn = true;
                PlayerListImg.color = ReadyColors[1];
            }
            
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.Destroy(GetComponent<PhotonView>());
        }

        if (lobbyManager.myPlayerListContent)
        {
            if (lobbyManager.myPlayerListContent.gameObject != this.gameObject)
            {
                if (GetComponent<PhotonView>().IsMine)
                    PhotonNetwork.Destroy(GetComponent<PhotonView>());
            }
        }

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            if (GetComponent<PhotonView>().Owner == PhotonNetwork.PlayerList[i])
            {
                string tmp = (string)PhotonNetwork.PlayerList[i].CustomProperties["ProfileIndex"];
                int profileIndex = int.Parse(tmp);
                PlayerProfileImg.sprite = RoomData.GetInstance().playerSprite[profileIndex];
                break;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }

}
