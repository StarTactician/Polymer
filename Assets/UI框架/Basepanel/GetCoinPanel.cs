using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GetCoinPanel : BasePanel
{
    private static string name = "GetCoinPanel";
    private static string path = "Panel/GetCoinPanel";
    public static readonly UIType uitype = new UIType(path, name);
    //private TextMeshProUGUI coinNum;
    //private TextMeshProUGUI bulidName;
    //private TextMeshProUGUI techName;
    public int CoinNumber;
    public string Name1;
    public string Name2;
    public bool rewardCoinShown = false;//在调用这个Panel的脚本中设置
    public bool rewardPuShown = false;
    public bool newBuildingShown = false;
    public bool newTechShown = false;
    public bool upgradeTaskDepartmentShown = false;

    public GetCoinPanel() : base(uitype)
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
        if (CoinNumber != 0)
            rewardCoinShown = true;
    }

    public override void OnStart()
    {
        base.OnStart();
        UIFunction.Inst.StartFade(0.2f, this, true);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Back").onClick.AddListener(Back);
        /*coinNum = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "coinNum");
        bulidName = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Name1");
        techName = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Name2");*/
        if (CoinNumber != 0)
            rewardCoinShown = true;
        DisplayRewards();
    }

    void Back()
    {
        UIManager.GetInst().PopOnceOnlyUsePanel();
    }

    void DisplayRewards()
    {
        GameObject reward = UIFunction.Inst.GetChild(PanelObj, "Reward");        
        List<Image> rewardImage = UIFunction.Inst.GetComponentsInObj<Image>(reward);

        /*UIFunction.Inst.SetCanvasGroupVise(coinNum.gameObject, false);
        UIFunction.Inst.SetCanvasGroupVise(bulidName.gameObject, false);
        UIFunction.Inst.SetCanvasGroupVise(techName.gameObject, false);*/
        foreach (Image image in rewardImage)
        {
            //RectTransform rectTransform = image.GetComponent<RectTransform>();

            if (rewardCoinShown)
            {
                image.sprite = Resources.Load<Sprite>("IMG/Rewards/Coin");
                //coinNum设置为可见
                /*UIFunction.Inst.SetCanvasGroupVise(coinNum.gameObject, true);
                coinNum.text = CoinNumber.ToString();*/
                UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(image.gameObject, "Text").text= CoinNumber.ToString();
                rewardCoinShown = false; // 标记金币奖励已展示
            }
            else if(rewardPuShown)
            {
                image.sprite = Resources.Load<Sprite>("IMG/Rewards/PU");
                UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(image.gameObject, "Text").text = CoinNumber.ToString();
                rewardPuShown = false; // 标记新建筑奖励已展示
            }
            else if (newBuildingShown)
            {
                image.sprite = Resources.Load<Sprite>("IMG/Rewards/Building");
                UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(image.gameObject, "Text").text = Name1;
                /*UIFunction.Inst.SetCanvasGroupVise(bulidName.gameObject, true);
                bulidName.text = Name1;
                Debug.Log(MissionManager.GetInst().taskQueue.Peek().newBuliding+Name1);*/
                
                newBuildingShown = false; // 标记新建筑奖励已展示
            }
            else if (newTechShown)
            {
                image.sprite = Resources.Load<Sprite>("IMG/Rewards/Tech");
                /*UIFunction.Inst.SetCanvasGroupVise(bulidName.gameObject, true);
                techName.text = Name2;
                Debug.Log(MissionManager.GetInst().taskQueue.Peek().newTech + Name2);*/
                UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(image.gameObject, "Text").text = Name2;
                newTechShown = false; // 标记新科技奖励已展示
            }
            else if (upgradeTaskDepartmentShown)
            {
                image.sprite = Resources.Load<Sprite>("IMG/Rewards/Upgrade");
                upgradeTaskDepartmentShown = false; // 标记任务部门升级奖励已展示
            }
            else
            {
                // 如果没有可展示的奖励，则禁用该图像对象
                image.gameObject.SetActive(false);                
                continue;
            }
        }
    }
}
