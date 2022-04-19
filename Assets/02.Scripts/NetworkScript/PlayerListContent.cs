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
        this.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

        if (myPlayer != null)
        {
            nameTxt.text = "["+ myPlayer.ActorNumber+ "]" + myPlayer.NickName;

            if ((string)myPlayer.CustomProperties["Ready"] == "0")
                readyToggle.isOn = false;
            else
                readyToggle.isOn = true;
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
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
