using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class CGVideoPlayer : MonoBehaviour
{
    [Header("Video Components")]
    public VideoPlayer videoPlayer;
    public RawImage rawImage;
    public RenderTexture renderTexture;

    [Header("UI Components")]
    public GameObject skipButton; // 可选：跳过按钮

    [Header("面板控制")]
    public GameObject cgPanel;       // 当前 CG 面板
    public GameObject startPanel;    // CG 结束后切换的主界面面板

    private bool hasStarted = false;

    void Start()
    {
        // 初始化绑定
        if (renderTexture != null)
        {
            videoPlayer.targetTexture = renderTexture;
            rawImage.texture = renderTexture;
        }

        // 设置路径并准备播放
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "CG.mp4");
        videoPlayer.url = videoPath;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Prepare();
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        if (!hasStarted)
        {
            hasStarted = true;
            rawImage.color = Color.white; // 防止 RawImage 是透明的
            videoPlayer.Play();
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        CloseCGAndEnterStart();
    }

    public void SkipCG()
    {
        videoPlayer.Stop();
        CloseCGAndEnterStart();
    }

    private void CloseCGAndEnterStart()
    {
        if (cgPanel != null) cgPanel.SetActive(false);
        if (startPanel != null) startPanel.SetActive(true);
    }
}
