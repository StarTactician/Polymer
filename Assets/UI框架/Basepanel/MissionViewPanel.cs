using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionViewPanel : BasePanel
{
    private static string name = "MissionViewPanel";
    private static string path = "Panel/MissionViewPanel";

    public static readonly UIType uitype = new UIType(path, name);
    public List<Button> UnselectionMissions;
    public Dictionary<Button, string > UnselectionTaskID_Dict=new Dictionary<Button, string>();//任务刷新时，将任务信息添加进去
    public TextMeshProUGUI Title;
    private Vector2 queueButtonTransform;
    //RectTransform missionQueueTrans;
    private Coroutine moveCoroutine;
    Button missionQueue;
    public MissionViewPanel() : base(uitype)
    {

    }


    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        //MoveBack(missionQueue);
        UIFunction.Inst.StartFade(0.2f, this, false);
        
    }

    public override void OnEnable()
    {
        base.OnEnable();
        //UIFunction.Inst.GetCompoentInCanvasChild<Button>(PanelObj, "Back").onClick.AddListener(Back);
        //UIFunction.Inst.GetCompoentInCanvasChild<Button>(PanelObj, "Refresh").onClick.AddListener(Refresh);
        UnselectionMissions = UIFunction.Inst.GetComponentsInObjsChild<Button>(PanelObj, "UnselectionMission");
        missionQueue = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "MissionQueue");
        SaveManager.GetInst().CheckAndResetRefreshTimes();
        Title = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Title");
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "RefreshChance").text = "今日刷新次数：" + SaveManager.SaveRefresh;
        MoveBack(missionQueue);
        UIFunction.Inst.StartFade(0.2f, this, true);
    }

    public override void OnStart()
    {
        base.OnStart();
        
        UIFunction.Inst.StartFade(0.2f, this, true);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Back").onClick.AddListener(Back);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Refresh").onClick.AddListener(HandleRefresh);
        SaveManager.GetInst().CheckAndResetRefreshTimes();
        Title = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Title");
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "RefreshChance").text = "今日刷新次数："+SaveManager.SaveRefresh;
        UnselectionMissions = UIFunction.Inst.GetComponentsInObjsChild<Button>(PanelObj, "UnselectionMission");

        missionQueue=UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "MissionQueue");
        missionQueue.onClick.AddListener(MissionQueue);
        queueButtonTransform = UIFunction.Inst.GetComponentInObj<RectTransform>(missionQueue.gameObject).anchoredPosition;

        EventTrigger trigger = missionQueue.gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerEnter;
        pointerDownEntry.callback.AddListener((data) => { MoveTORight(missionQueue); });
        trigger.triggers.Add(pointerDownEntry);
        EventTrigger.Entry pointerExitEntry = new EventTrigger.Entry();
        pointerExitEntry.eventID = EventTriggerType.PointerExit;
        pointerExitEntry.callback.AddListener((data) => { MoveBack(missionQueue); });
        trigger.triggers.Add(pointerExitEntry);
               
        //Debug.Log("当前的可备选任务有" + UnselectionMissions.Count);

        //如果有已刷新出来的任务，则读取并一一对应，不需要重新刷新，否则则调用刷新方法，并且特殊任务不能重复
        MissionManager.GetInst().LoadUnselectionTasksFromJson();//加载待选任务
        UnselectionTaskID_Dict.Clear();
        foreach (Button button in UnselectionMissions)
        {
            UIFunction.Inst.SetCanvasGroupVise(button.gameObject,false); // 初始时禁用所有按钮
            //刷新的任务和UI任务选项一一对应
            switch (MissionManager.GetInst().UnselectionTasks.Count==0)
            {
                case (true)://刷新任务池然后将其一一对应
                    Refresh();
                    break;
                case (false)://将现有的一一对应                   
                    for (int i = 0; i < MissionManager.GetInst().UnselectionTasks.Count; i++)
                    {
                        GetMissionInfo(i);
                    }
                    break;

            }
            button.onClick.AddListener(Select);
        }
    }
    void Back()
    {
        UIManager.GetInst().Pop(false);
    }
    private void HandleRefresh()
    {
        if(SaveManager.SaveRefresh<=0)
        {
            UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "RefreshChance").text = "今日刷新次数：" + 0;
            TipsJumpWindow jumpWindow = new TipsJumpWindow();
            jumpWindow.Text = "今日刷新次数已用尽！";
            UIManager.GetInst().PushTips(jumpWindow);
            return;
        }
        else
        {
            SaveManager.GetInst().ChangeRefreshChance(-1);
            Refresh();
            UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "RefreshChance").text = "今日刷新次数：" + SaveManager.SaveRefresh;
        }
    }
    public void Refresh()
    {
        UnselectionTaskID_Dict.Clear();
        Debug.Log("待选任务列表已刷新！");
        RefreshUnselectionTask();
        for (int i = 0; i < MissionManager.GetInst().UnselectionTasks.Count; i++)
        {
            if (MissionManager.GetInst().UnselectionTasks != null && MissionManager.GetInst().UnselectionTasks.Count > 0)
                GetMissionInfo(i);
            else
                Debug.Log(MissionManager.GetInst().UnselectionTasks+"有问题");
        }
    }
    void Select()//点击查看详情任务，也就是打开任务窗口
    {
        Button clickedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        if (clickedButton != null)
        {
            UIFunction.Inst.SetCanvasGroupVise(Title.gameObject, false);
            UIFunction.Inst.SetCanvasGroupVise(clickedButton.gameObject,false);
            //PanelObj.SetActive(false);
            MissionDetailWindow missionDetailWindow = new MissionDetailWindow();
            missionDetailWindow.MissionID = UnselectionTaskID_Dict[clickedButton];
            missionDetailWindow.viewPanel = this;
            //missionDetailWindow.missionDate = MissionManager.GetInst().UnselectionTasks[int.Parse(clickedButton.gameObject.name)];
            missionDetailWindow.ClickButton = clickedButton.gameObject;//在退出时会重新显示预览界面
            UIManager.GetInst().Push(missionDetailWindow,true);
        }
        else
        {
            Debug.LogWarning("无法找到被点击的按钮");
        }
    }
    void GetMissionInfo(int index)//将任务与任务面板一一对应
    {
        if (MissionManager.GetInst().UnselectionTasks[index] == null)
        {
            Debug.LogWarning($"UnselectionTasks 中的任务在索引 {index} 处为空");
            return;
        }

        string id = MissionManager.GetInst().UnselectionTasks[index].id;
        MissionDate task = MissionManager.GetInst().GetTaskById(id);// 获取任务的信息   
        if (!UnselectionTaskID_Dict.ContainsKey(UnselectionMissions[index]))
            UnselectionTaskID_Dict.Add(UnselectionMissions[index], id);

        UIFunction.Inst.SetCanvasGroupVise(UnselectionMissions[index].gameObject,true);

        switch (task.type)
        {
            case "材料研发":
                UIFunction.Inst.GetCompoentInObjsChild<Image>(UnselectionMissions[index].gameObject, "Image").sprite = Resources.Load<Sprite>("IMG/Rewards/MaterialResearch");
                break;
            case "基地升级":
                UIFunction.Inst.GetCompoentInObjsChild<Image>(UnselectionMissions[index].gameObject, "Image").sprite = Resources.Load<Sprite>("IMG/Rewards/BaseUpgrade");
                break;
            case "订单接取":
                UIFunction.Inst.GetCompoentInObjsChild<Image>(UnselectionMissions[index].gameObject, "Image").sprite = Resources.Load<Sprite>("IMG/Rewards/MaterialTrade");
                break;
            case "工厂革新":
                UIFunction.Inst.GetCompoentInObjsChild<Image>(UnselectionMissions[index].gameObject, "Image").sprite = Resources.Load<Sprite>("IMG/Rewards/BaseUpgrade");
                break;
            case "任务部":
                UIFunction.Inst.GetCompoentInObjsChild<Image>(UnselectionMissions[index].gameObject, "Image").sprite = Resources.Load<Sprite>("IMG/Rewards/TaskDepartMentUpgrade");
                break;
        }

        switch (task.rarity.key)
        {
            case "Common":
                UIFunction.Inst.GetComponentInObj<Image>(UnselectionMissions[index].gameObject).sprite = Resources.Load<Sprite>("IMG/UIEdgeWhite");
                Debug.Log(index + "任务ID" + task.id + "稀有度为" + task.rarity.key);
                break;
            case "Rare":
                UIFunction.Inst.GetComponentInObj<Image>(UnselectionMissions[index].gameObject).sprite = Resources.Load<Sprite>("IMG/UIEdgeBlue");
                Debug.Log(index + "任务ID" + task.id + "稀有度为" + task.rarity.key);
                break;
            case "VeryRare":
                UIFunction.Inst.GetComponentInObj<Image>(UnselectionMissions[index].gameObject).sprite = Resources.Load<Sprite>("IMG/UIEdgePurple");
                Debug.Log(index + "任务ID" + task.id + "稀有度为" + task.rarity.key);
                break;
            case "SuperRare":
                UIFunction.Inst.GetComponentInObj<Image>(UnselectionMissions[index].gameObject).sprite = Resources.Load<Sprite>("IMG/UIEdgeGloden");
                Debug.Log(index + "任务ID" + task.id + "稀有度为" + task.rarity.key);
                break;
            default:
                break;
        }
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(UnselectionMissions[index].gameObject, "Rank").text = UIFunction.Inst.RomanMap[task.level];
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(UnselectionMissions[index].gameObject, "Title").text = task.type;
        
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(UnselectionMissions[index].gameObject, "Time").text = System.TimeSpan.FromSeconds(task.duration).ToString();
        GameObject reward = UIFunction.Inst.GetChild(UnselectionMissions[index].gameObject, "Reward");
        List<Image> rewardImage = UIFunction.Inst.GetComponentsInObj<Image>(reward);
       
        //Debug.Log(rewardImage.Count);
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
    void RefreshUnselectionTask()//刷新任务池的逻辑
    {
        foreach (MissionDate task in MissionManager.GetInst().taskPool)
        {
            MissionManager.GetInst().RefreshTaskPool(task);
        }            
        switch (SaveManager.GetInst().TaskDepartment)//根据任务部门的等级决定一次可以刷新几个备选任务
        {
            case (1)://1个备选任务
                UIFunction.Inst.SetCanvasGroupVise(UnselectionMissions[0].gameObject,true);
                MissionManager.GetInst().GenerateTaskList(1);
                break;
            case (2)://3个备选任务
                UIFunction.Inst.SetCanvasGroupVise(UnselectionMissions[0].gameObject, true);
                UIFunction.Inst.SetCanvasGroupVise(UnselectionMissions[1].gameObject, true);
                UIFunction.Inst.SetCanvasGroupVise(UnselectionMissions[2].gameObject, true);
                MissionManager.GetInst().GenerateTaskList(3);
                break;
            case (3)://5个备选任务
                foreach(Button button in UnselectionMissions)
                {
                    UIFunction.Inst.SetCanvasGroupVise(button.gameObject,true);
                }
                MissionManager.GetInst().GenerateTaskList(5);
                break;
        }

    }
    void MoveTORight(Button button)
    {
        moveCoroutine=UIFunction.Inst.MoveObject(button.gameObject, Vector2.right, 100,0.2f,this);
    }
    void MoveBack(Button button)
    {
        if(IsMoveing)
            UIFunction.Inst.StopTheCoroutine(moveCoroutine);
        //Debug.Log(queueButtonTransform+ UIFunction.Inst.GetCompoentInObj<RectTransform>(button.gameObject).anchoredPosition);
        if(button.gameObject!=null)
            UIFunction.Inst.GetComponentInObj<RectTransform>(button.gameObject).anchoredPosition = queueButtonTransform;
        IsMoveing = false;
    }
    void MissionQueue()//查看当前任务队列，推入任务队列的Panel
    {
        //MissionQueuePanel missionQueuePanel = new MissionQueuePanel();
        UIManager.GetInst().Push(new MissionQueuePanel());
    }
}
