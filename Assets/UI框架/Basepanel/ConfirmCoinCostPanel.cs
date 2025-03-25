using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmCoinCostPanel : BasePanel
{
    private static string name = "ConfirmCoinCostPanel";
    private static string path = "Panel/ConfirmCoinCostPanel";
    public static readonly UIType uitype = new UIType(path, name);
    private TextMeshProUGUI coinNum;
    public int Num;
    public ConfirmCoinCostPanel() : base(uitype)
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
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Cancel").onClick.AddListener(Back);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Confirm").onClick.AddListener(Confirm);
        coinNum = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "costNum");
        CoinNum(Num);
    }
    void Back()
    {
        UIManager.GetInst().PopOnceOnlyUsePanel();
    }
    void Confirm()
    {
        //判断钱够不够
        if(SaveManager.CurrentPU< Num)
        {
            //弹出不够窗口
            UIManager.GetInst().PopOnceOnlyUsePanel();
            TipsJumpWindow jumpWindow = new TipsJumpWindow();
            jumpWindow.Text = "当前聚合基元不够，无法开始研发工作!";
            UIManager.GetInst().PushTips(jumpWindow,2.5f);
        }
        else
        {
            SaveManager.GetInst().ResourcesChange(-Num,"PU");
            //TestScene testScene = new TestScene();
            //SceneControl.GetInst().LoadScene(testScene.SceneName, testScene);
            //SaveManager.GetInst().CoinChange(-SaveManager.CurrentLevel * 40);
            SceneControl.GetInst().LoadLevel(SaveManager.CurrentLevel);
        }
    }
    void CoinNum(int num)
    {
        coinNum.text = "进入关卡将花费 "+num.ToString();
    }
}
