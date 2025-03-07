using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Inst;
    [Tooltip("主菜单歌单")]  public AudioClip[] musiclists;       // 音乐播放列表数组，每个关卡对应一个歌单
    [Tooltip("主线关歌单")] public AudioClip[] musicists1;
    [Tooltip("无尽关歌单")] public AudioClip[] musicists2;
    [Tooltip("胜利结算音乐")] public AudioClip[] musicists3;
    [Tooltip("失败结算音乐")] public AudioClip[] musicists4;
    public AudioClip[] currentPlaylist;       // 当前关卡的音乐列表
    private AudioClip[] lastPlaylist;       // 上一次的音乐列表
    private AudioSource audioSource;
    public float MusicVolum=1; 
    public float EffectVolum=1;
    public bool skip;
    /// <summary>
    /// 通知播放音效的物体更改自身的音量
    /// </summary>
    public static event System.Action<float> OnVolumeChanged;
    private int lastPlayedIndex = -1; // 用于记录上一次播放的曲目索引
    private void OnEnable()
    {
        if (Inst == null)
            Inst = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
    private void Awake()
    {
        
        BaseScene.OnEnterScene += EnterScene;
    }
    private void OnDestroy()
    {
        // 取消订阅进入场景的事件
        BaseScene.OnEnterScene -= EnterScene;
    }

    private void EnterScene()
    {
        // 更新歌单
        Updatelist(BaseScene.CurrentType);
    }
    void Start()
    {
        // 初始化音乐播放器
        audioSource = GetComponent<AudioSource>();
        //MusicVolum = SaveManager.GetInst().TheSetMusicVolum;
        //EffectVolum = SaveManager.GetInst().TheSetEffectVolum;
        audioSource.volume = MusicVolum;
       
        Updatelist(0);
        PlayMusic();
    }

    void FixedUpdate()
    {        
        /*if (skip)
        {
            PlayMusic();
            skip = false;
        }*/
        if (audioSource.isPlaying)
        {
            return;           
        }
        else// 如果没有播放音乐，则随机播放一首
            PlayMusic();
    }
    public void SetMusicVolume(float newVolume)
    {
        MusicVolum = newVolume;
        audioSource.volume = MusicVolum;
    }
    /// <summary>
    /// 设置中的音效大小滚动条改变时调用
    /// </summary>
    /// <param name="newVolume">由滚动条处获取的音量大小</param>
    public void SetEffectVolume(float newVolume)
    {
        EffectVolum = newVolume;
        
        OnVolumeChanged?.Invoke(EffectVolum);// 广播事件，通知所有订阅者音量改变
    }
    /// <summary>
    /// 进入新关卡时调用，比如主线关卡无尽关卡，用于同步音效值,此时没有输入值
    /// </summary>
    public void SetEffectVolume()
    {
        OnVolumeChanged?.Invoke(EffectVolum);// 广播事件，通知所有订阅者音量改变
    }

    /// <summary>
    /// 根据关卡更新当前的播放列表
    /// </summary>
    /// <param name="type">当前播放的歌单，主界面0，主线关1，无尽关2，胜利结算3，失败结算4，被各个场景控制界面状态的脚本调用</param>
    public void Updatelist( int type)
    {
        
        switch (type)
        {
            case (0):
                currentPlaylist = musiclists;
                break;
            case (1):
                currentPlaylist = musicists1;
                break;
            case (2):
                currentPlaylist = musicists2;
                break;
            case (3):
                currentPlaylist = musicists3;
                break;
            case (4):
                currentPlaylist = musicists4;
                break;
            default:
                break;
        }
        if(currentPlaylist!=lastPlaylist)
            PlayMusic();
    }
    /// <summary>
    /// 随机播放当前关卡中的音乐
    /// </summary>
    void PlayMusic()
    {
        lastPlaylist = currentPlaylist;
        if (currentPlaylist == null || currentPlaylist.Length == 0)
        {
            return;
        }
        if (currentPlaylist != null && currentPlaylist.Length > 0)
        {
            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, currentPlaylist.Length);
            } while (randomIndex == lastPlayedIndex && currentPlaylist.Length > 1);

            AudioClip randomClip = currentPlaylist[randomIndex];
            lastPlayedIndex = randomIndex;

            // 设置音频剪辑并播放
            audioSource.clip = randomClip;
            audioSource.Play();
        }

    }
    /*void PlayMusic(bool soloLoop)
    {
        audioSource.loop = soloLoop;
        audioSource.Play();
        
    }*/
}
