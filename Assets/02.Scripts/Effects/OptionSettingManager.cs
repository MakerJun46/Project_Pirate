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

    public Slider masterAudioSlider;
    public Slider backgroundAudioSlider;
    public Slider effectAudioSlider;
    //public float EffectAudioVolumeMul;

    public int MainAudioIndex = 0;
    //public AudioClip[] mainBackgroundClips0;
    //public AudioClip[] mainBackgroundClips1;
    //public AudioClip[] mainBackgroundClips2;
    //public AudioClip[] mainBackgroundClips3;
    //public AudioClip[] mainBackgroundClips4;

    public List<MainBackgroundClips> mainBackgroundClipList = new List<MainBackgroundClips>();

    public Sound[] sounds;

    public AudioMixer MasterAudioMixer;
    public AudioMixerGroup EffectAudioMixer;

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
        masterAudioSlider.value = tmpVal;
        MasterAudioMixer.GetFloat("backgroundVol", out tmpVal);
        backgroundAudioSlider.value = tmpVal;
        MasterAudioMixer.GetFloat("effectVol", out tmpVal);
        effectAudioSlider.value = tmpVal;

        /*
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            if (!options.Contains(option))
                options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        
        resolutionDropdown.AddOptions(options);
        //resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.value = 0;
        resolutionDropdown.RefreshShownValue();
        */
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            MainAudioIndex++;
            PlayBackgroundAudio(false);
        }

        /*
        if (MainBackgroundAudioAdjustTime > 0)
        {
            MainBackgroundAudioAdjustTime -= Time.deltaTime;
            MainBackgroundAudio.volume = Mathf.Lerp(MainBackgroundAudio.volume, MainBackgroundVolumeValue, Time.deltaTime);
        }
        else
        {
            MainBackgroundAudio.volume = backgroundAudioSlider.value;
            //MainBackgroundAudio.volume = Mathf.Lerp(MainBackgroundAudio.volume, 0.1f* backgroundAudioSlider.value, Time.deltaTime);
        }
        */
    }

    public void SetMasterVolume(float val)
    {
        MasterAudioMixer.SetFloat("masterVol", val);
    }
    public void SetBackgroundVolume(float val)
    {
        MasterAudioMixer.SetFloat("backgroundVol", val);
    }
    public void SetEffectVolume(float val)
    {
        MasterAudioMixer.SetFloat("effectVol", val);
    }

    public void SetBackgroundVolume(float val, float Time)
    {
        MainBackgroundAudioAdjustTime = Time;
        MainBackgroundVolumeValue = val;
    }

    /*
    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }
    */

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}