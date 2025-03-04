using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefeatedPanel : BasePanel
{
    private static string name = "DefeatedPanel";
    private static string path = "Panel/DefeatedPanel";
    public static readonly UIType uitype = new UIType(path, name);
    public DefeatedPanel() : base(uitype)
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
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "BackToMenu").onClick.AddListener(BackToMenu);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Restart").onClick.AddListener(Restart);
    }
    void BackToMenu()
    {
        UIManager.Pause(false);
        StartScene startScene = new StartScene();
        SceneControl.GetInst().LoadScene(startScene.SceneName, startScene);
    }
    void Restart()
    {
        SceneControl.GetInst().LoadScene();
    }
   
}
