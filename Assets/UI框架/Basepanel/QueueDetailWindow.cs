using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QueueDetailWindow : BasePanel
{
    private static string name = "QueueDetailWindow";
    private static string path = "Panel/QueueDetailWindow";
    public static readonly UIType uitype = new UIType(path, name);
    public GameObject ClickButton;
    //public string MissionID;
    public MissionQueuePanel viewPanel;
    public MissionDate task;
    TextMeshProUGUI completeState;
    Coroutine changeTimeCoroutine;
    public QueueDetailWindow() : base(uitype)
    {

    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        UIFunction.Inst.SetCanvasGroupVise(ClickButton, true);
        UIFunction.Inst.SetCanvasGroupVise(viewPanel.Title.gameObject, true);
        if (changeTimeCoroutine != null)
        {
            UIFunction.Inst.StopTheCoroutine(changeTimeCoroutine);
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        UIFunction.Inst.SetCanvasGroupVise(ClickButton, true);
        UIFunction.Inst.SetCanvasGroupVise(viewPanel.Title.gameObject, true);
        if (changeTimeCoroutine != null)
        {
            UIFunction.Inst.StopTheCoroutine(changeTimeCoroutine);
        }

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
        completeState = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "CompleteState");        
        GetMissionInfo();
        ChangeCostTime();//task在GetMissionInfo后才被赋值；2改，task在推入时被赋值，通过ID查找的任务没有完成时间的细节
        changeTimeCoroutine = UIFunction.Inst.StartCoroutineWithInterval(ChangeCostTime, 1f);

    }
    void Back()
    {
        UIManager.GetInst().PopWindow(this, true);//false
    }
    void ChangeCostTime()
    {
        if (task.IsComplete)
        {
            UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Time").text = "00:00:00";
            completeState.text = "任务已完成！";
            return;
        }
        DateTime startTime = DateTime.Parse(task.startTime);
        DateTime now = DateTime.Now;
        DateTime endTime = startTime.AddSeconds(task.duration);

        // 如果当前时间已经超过了任务结束时间，说明任务已经完成
        if (now >= endTime)
        {
            //task.IsComComplete = true;//有待考虑有没有必要在这里更新完成状态
            viewPanel.ShowProgress();//在这里同步
            UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Time").text = "00:00:00";
            completeState.text = "任务已完成！";
            return;
        }
        int remainingSeconds = (int)(endTime - now).TotalSeconds;

        // 转换为 TimeSpan 格式，并返回格式化字符串
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Time").text = TimeSpan.FromSeconds(remainingSeconds).ToString(@"hh\:mm\:ss");
        completeState.text = "任务进行中...";
        /*if(task==MissionManager.GetInst().taskQueue.Peek())
        {
            
        }
        else
        {
            UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Time").text = "等待任务开始...";
            completeState.text = "任务排队中...";
        }*/
    }
    void GetMissionInfo()
    {
        
        //task = MissionManager.GetInst().GetTaskById(MissionID);
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
        //UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Time").text = System.TimeSpan.FromSeconds(task.duration).ToString();
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "DescribeText").text = task.description;
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
}