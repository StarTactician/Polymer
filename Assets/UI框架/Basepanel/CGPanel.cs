using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class CGPanel : BasePanel
{
    private static string name = "CGPanel";
    private static string path = "Panel/CGPanel";
    public RawImage rawImage; // °ó¶¨UIÉÏµÄRawImage
    public VideoPlayer videoPlayer;
    public static readonly UIType uitype = new UIType(path, name);
    public CGPanel() : base(uitype)
    {

    }
    public override void OnStart()
    {
        if (GameRoot.GetInst().SecondaryCamera.activeSelf)
            GameRoot.GetInst().SecondaryCamera.SetActive(false);
        rawImage =UIFunction.Inst.GetCompoentInObjsChild<RawImage>(PanelObj,"CG");
        videoPlayer = UIFunction.Inst.GetCompoentInObjsChild<VideoPlayer>(PanelObj,"CG");
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Skip").onClick.AddListener(SkipCG);
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "CG.mp4");
        videoPlayer.url = videoPath;
        videoPlayer.Prepare();

        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.loopPointReached += OnCGFinished;
    }

    void OnPrepared(VideoPlayer vp)
    {
        //rawImage.color = Color.black; // Ô¤·ÀÉÁ°×
        rawImage.texture = vp.targetTexture;
        AudioManager.Inst.Pause();
        videoPlayer.Play();
    }

    void OnCGFinished(VideoPlayer vp)
    {
        UIManager.GetInst().Pop(false);
        UIManager.GetInst().Push(new StartPanel());
        DialogueManager.GetInst().StartDialogue("IntroFirstTime");
        AudioManager.Inst.Play();

    }

    public void SkipCG()
    {
        videoPlayer.Stop();
        OnCGFinished(videoPlayer);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        UIFunction.Inst.StartFade(0.2f, this, false);

    }
}
