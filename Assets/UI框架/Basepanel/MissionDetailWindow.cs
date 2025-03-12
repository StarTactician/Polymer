using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionDetailWindow : BasePanel
{
    private static string name = "MissionDetailWindow";
    private static string path = "Panel/MissionDetailWindow";
    public static readonly UIType uitype = new UIType(path, name);
    public GameObject ClickButton;
    public string MissionID;
    public MissionViewPanel viewPanel;
    private MissionDate task;
    public MissionDetailWindow() : base(uitype)
    {

    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        UIFunction.Inst.SetCanvasGroupVise(ClickButton,true);
        UIFunction.Inst.SetCanvasGroupVise(viewPanel.Title.gameObject,true);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        UIFunction.Inst.SetCanvasGroupVise(ClickButton,true);
        UIFunction.Inst.SetCanvasGroupVise(viewPanel.Title.gameObject, true);

    }

    public override void OnEnable()
    {
        base.OnEnable();
        //UIFunction.Inst.StartFade(0.2f, this, true);
    }

    public override void OnStart()
    {
        base.OnStart();
        UIFunction.Inst.StartScale(0.2f, PanelObj.GetComponent<RectTransform>(), new Vector3(1.25f, 1.25f, 0.2f));
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Back").onClick.AddListener(Back);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "AddToQueue").onClick.AddListener(AddToQueue);         
        GetMissionInfo();

    }
    void Back()
    {
        UIManager.GetInst().PopWindow(this,true);//false
    }
    void GetMissionInfo()
    {
        task = MissionManager.GetInst().GetTaskById(MissionID);
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Title").text = task.type;
        switch (task.type)
        {
            case "材料研发":
                UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "Image").sprite = Resources.Load<Sprite>("IMG/Rewards/MaterialResearch");
                break;
            case "基地升级":
                UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "Image").sprite = Resources.Load<Sprite>("IMG/Rewards/BaseUpgrade");
                break;
            case "订单接取":
                UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "Image").sprite = Resources.Load<Sprite>("IMG/Rewards/MaterialTrade");
                break;
            case "工厂革新":
                UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "Image").sprite = Resources.Load<Sprite>("IMG/Rewards/BaseUpgrade");
                break;
            case "任务部":
                UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "Image").sprite = Resources.Load<Sprite>("IMG/Rewards/TaskDepartMentUpgrade");
                break;
        }

        switch (task.rarity.key)
        {
            case "Common":
                UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "1").sprite = Resources.Load<Sprite>("IMG/UIEdgeWhite");
                break;
            case "Rare":
                UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "1").sprite = Resources.Load<Sprite>("IMG/UIEdgeBlue");
                break;
            case "VeryRare":
                UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "1").sprite = Resources.Load<Sprite>("IMG/UIEdgePurple");
                break;
            case "SuperRare":
                UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "1").sprite = Resources.Load<Sprite>("IMG/UIEdgeGloden");
                break;
            default:
                break;
        }
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Rank").text = UIFunction.Inst.RomanMap[task.level];
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "CoinCost").text = task.cost.ToString();
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Time").text = System.TimeSpan.FromSeconds(task.duration).ToString();
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "DescribeText").text = task.description;
        //UIFunction.Inst.GetCompoentInCanvasChild<Image>(WindowObject, "Image").sprite = Resources.Load<Sprite>("IMG/PolymerImg/" + missionDate.type);
        GameObject reward = UIFunction.Inst.GetChild(PanelObj, "Reward");
        List<Image> rewardImage = UIFunction.Inst.GetComponentsInObj<Image>(reward);
        // 创建布尔列表来跟踪每种奖励是否已经展示
        bool rewardCoinShown = false;
        bool newBuildingShown = false;
        bool newTechShown = false;
        bool upgradeTaskDepartmentShown = false;
        foreach (Image image in rewardImage)
        {
            // 依次检查每种奖励，优先显示金币
            if (!rewardCoinShown && task.rewardCoin > 0)
            {
                image.sprite = Resources.Load<Sprite>("IMG/Rewards/Coin");
                rewardCoinShown = true; // 标记金币奖励已展示
            }
            else if (!newBuildingShown && !string.IsNullOrEmpty(task.newBuliding))
            {
                image.sprite = Resources.Load<Sprite>("IMG/Rewards/Building");
                newBuildingShown = true; // 标记新建筑奖励已展示
            }
            else if (!newTechShown && !string.IsNullOrEmpty(task.newTech))
            {
                image.sprite = Resources.Load<Sprite>("IMG/Rewards/Tech");
                newTechShown = true; // 标记新科技奖励已展示
            }
            else if (!upgradeTaskDepartmentShown && task.upgradeTaskDepartment)
            {
                image.sprite = Resources.Load<Sprite>("IMG/Rewards/Upgrade");
                upgradeTaskDepartmentShown = true; // 标记任务部门升级奖励已展示
            }
            else
            {
                // 如果没有可展示的奖励，则禁用该图像对象
                image.gameObject.SetActive(false);
            }
        }

    }
    
    void AddToQueue()
    {
        if (!(MissionManager.GetInst().taskQueue.Count >= MissionManager.GetInst().MaxQueueNum))
        {
            viewPanel.Refresh();           
        }
        MissionManager.GetInst().AddTaskToQueue(task);  
        UIManager.GetInst().PopWindow(this, true);
    }
}
