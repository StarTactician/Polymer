using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VectoryPanel : BasePanel
{
    private static string name = "VectoryPanel";
    private static string path = "Panel/VectoryPanel";
    private RawImage star1;
    private RawImage star2;
    private RawImage star3;
    public static readonly UIType uitype = new UIType(path, name);
    public VectoryPanel() : base(uitype)
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
        ShowStar(LevelManager.GetInst().starNumJudge());//自身start启用了之后马上会被禁用出现金币数量UI，所以跳星星的操作放进OnEnable
    }

    public override void OnStart()
    {
        base.OnStart();
        UIFunction.Inst.StartFade(0.2f, this, true);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "BackToMenu").onClick.AddListener(BackToMenu);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "NextLevel").onClick.AddListener(NextLevel);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Restart").onClick.AddListener(Restart);
        star1 = UIFunction.Inst.GetCompoentInObjsChild<RawImage>(PanelObj, "star1");
        star2 = UIFunction.Inst.GetCompoentInObjsChild<RawImage>(PanelObj, "star2");
        star3 = UIFunction.Inst.GetCompoentInObjsChild<RawImage>(PanelObj, "star3");       
    }
    void BackToMenu()
    {
        UIManager.Pause(false);
        StartScene startScene = new StartScene();
        SceneControl.GetInst().LoadScene(startScene.SceneName, startScene);
    }
    void NextLevel()
    {
        ++SaveManager.CurrentLevel;
        ConfirmCoinCostPanel coinCostPanel = new ConfirmCoinCostPanel();
        coinCostPanel.Num = SaveManager.CountLevelCost();
        UIManager.GetInst().Push(coinCostPanel);
    }
    void Restart()
    {
        ConfirmCoinCostPanel coinCostPanel = new ConfirmCoinCostPanel();
        coinCostPanel.Num = SaveManager.CountLevelCost();
        UIManager.GetInst().Push(coinCostPanel);
    }
    void ShowStar(int starnum)
    {
        if (star1 == null || star2 == null || star3 == null)
        {
            Debug.LogError("One or more star RawImage references are null. Please check the UI setup.");
            return;
        }
        switch (starnum)
        {
            case (1):
                UIFunction.Inst.StartFade(1f, star1, true);
                UIFunction.Inst.StartScale(1f, star1.gameObject.GetComponent<RectTransform>(), new Vector3(0.6376f, 0.6376f,1));                
                return;
            case (2):
                UIFunction.Inst.StartFade(1f, star1, true);
                UIFunction.Inst.StartScale(1f, star1.gameObject.GetComponent<RectTransform>(), new Vector3(0.6376f, 0.6376f, 1));
                UIFunction.Inst.StartFade(1f, star2, true,0.8f);
                UIFunction.Inst.StartScale(1f, star2.gameObject.GetComponent<RectTransform>(), new Vector3(0.6376f, 0.6376f, 1),0.8f);
                return;
            case (3):
                UIFunction.Inst.StartFade(1f, star1, true);
                UIFunction.Inst.StartScale(1f, star1.gameObject.GetComponent<RectTransform>(), new Vector3(0.6376f, 0.6376f, 1));
                UIFunction.Inst.StartFade(1f, star2, true,0.8f);
                UIFunction.Inst.StartScale(1f, star2.gameObject.GetComponent<RectTransform>(), new Vector3(0.6376f, 0.6376f, 1),0.8f);
                UIFunction.Inst.StartFade(1f, star3, true,1.6f);
                UIFunction.Inst.StartScale(1f, star3.gameObject.GetComponent<RectTransform>(), new Vector3(0.6376f, 0.6376f, 1),1.6f);
                return;
            default:
                return;
        }
    }
}
