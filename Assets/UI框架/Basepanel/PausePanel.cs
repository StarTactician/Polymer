using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PausePanel : BasePanel
{
    private static string name = "PausePanel";
    private static string path = "Panel/PausePanel";
    public static readonly UIType uitype = new UIType(path, name);
    public PausePanel() : base(uitype)
    {

    }


    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        UIFunction.Inst.StartFade(0.2f, this, false);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        UIFunction.Inst.StartFade(0.2f, this, true);
    }

    public override void OnStart()
    {
        base.OnStart();
        UIFunction.Inst.StartFade(0.2f, this, true);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Resume").onClick.AddListener(Resume);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Restart").onClick.AddListener(Restart);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "BackToMenu").onClick.AddListener(BackToMenu);
    }

    void Resume()
    {
        UIManager.GetInst().Pop(false);
        UIManager.Pause(false);
    }
    void Restart()
    {
        UIManager.Pause(false);
        SceneControl.GetInst().LoadScene();
    }void BackToMenu()
    {
        UIManager.Pause(false);
        StartScene startScene = new StartScene();
        SceneControl.GetInst().LoadScene(startScene.SceneName,startScene);
    }

}
