using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System;

using Photon.Pun;
using Photon.Realtime;

[Serializable]
public struct MainBackgroundClips
{
    public AudioClip[] mainBackgroundClips;
}

public class OptionSettingManager : MonoBehaviourPunCallbacks, IPunObservable
{
    private static OptionSettingManager instance;
    public static OptionSettingManager GetInstance()
    {
        if (!instance)
        {
            instance = GameObject.FindObjectOfType<OptionSettingManager>();
            if (!instance)
                Debug.LogError("There needs to be one active MyClass script on a GameObject in your scene.");
        }

        return instance;
    }
    private PhotonView PV;
    public AudioSource MainBackgroundAudio;
    public float MainBackgroundVolumeValue;
    public float MainBackgroundAudioAdjustTime;

    public Slider backgroundAudioSlider;
    public Slider effectAudioSlider;

    public int MainAudioIndex = 0;

    public List<MainBackgroundClips> mainBackgroundClipList = new List<MainBackgroundClips>();

    public Sound[] sounds;

    public AudioMixer MasterAudioMixer;
    public AudioMixerGroup EffectAudioMixer;


    [SerializeField] Transform backgroundVolIcons;
    [SerializeField] Transform effectVolIcons;

    [SerializeField] GameObject CustomizePanelBtn;
    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip[0];

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = EffectAudioMixer;
        }
    }
    public void ActiveCustomPanelOpenBtn(bool _active)
    {
        CustomizePanelBtn.SetActive(_active);
    }
    private void SoundPlay(string name, bool stop)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("야 그런거 없어");
            return;
        }
        else
        {
            if (stop)
            {
                s.source.Stop();
            }
            else
            {
                s.source.clip = s.clip[UnityEngine.Random.Range(0, s.clip.Length)];
                s.source.Play();
            }
        }
    }
    public void Play(string name, bool rpc, bool stop = false)
    {
        if (rpc)
            PV.RPC("PlayAudioRPC", RpcTarget.All, new object[] { name, stop });
        else
        {
            SoundPlay(name, stop);
        }
    }

    [PunRPC]
    public void PlayAudioRPC(string name, bool stop)
    {
        SoundPlay(name, stop);
    }

    private void Start()
    {
        float tmpVal = 0;
        MasterAudioMixer.GetFloat("masterVol", out tmpVal);
        MasterAudioMixer.GetFloat("backgroundVol", out tmpVal);
        backgroundAudioSlider.value = tmpVal;
        MasterAudioMixer.GetFloat("effectVol", out tmpVal);
        effectAudioSlider.value = tmpVal;
    }

    public void PlayBackgroundAudio(bool random)
    {
        if (PhotonNetwork.CurrentRoom != null)
            if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["IsGameStarted"])
            {
                if (random)
                {
                    MainAudioIndex = UnityEngine.Random.Range(0, 4);
                }
                MainBackgroundAudio.clip = mainBackgroundClipList[(int)PhotonNetwork.CurrentRoom.CustomProperties["MapIndex"]].mainBackgroundClips[MainAudioIndex % mainBackgroundClipList[(int)PhotonNetwork.CurrentRoom.CustomProperties["MapIndex"]].mainBackgroundClips.Length];
            }
            else
            {
                MainBackgroundAudio.clip = mainBackgroundClipList[4].mainBackgroundClips[0];
            }
        MainBackgroundAudio.Play();
    }

    public void SetMasterVolume(float val)
    {
        MasterAudioMixer.SetFloat("masterVol", val);
    }
    public void SetBackgroundVolume(float val)
    {
        MasterAudioMixer.SetFloat("backgroundVol", val);

        for(int i=1;i< backgroundVolIcons.childCount; i++)
        {
            backgroundVolIcons.GetChild(i).gameObject.SetActive((float)(i)/(backgroundVolIcons.childCount-1) <= (80+val) / 90f);
        }
    }
    public void SetEffectVolume(float val)
    {
        MasterAudioMixer.SetFloat("effectVol", val);
        for (int i = 1; i < effectVolIcons.childCount; i++)
        {
            effectVolIcons.GetChild(i).gameObject.SetActive((float)(i) / (effectVolIcons.childCount - 1) <= (80 + val) / 90f);
        }
    }

    public void SetBackgroundVolume(float val, float Time)
    {
        MainBackgroundAudioAdjustTime = Time;
        MainBackgroundVolumeValue = val;
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}