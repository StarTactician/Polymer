using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionQueuePanel : BasePanel
{
    private static string name = "MissionQueuePanel";
    private static string path = "Panel/MissionQueuePanel";

    public static readonly UIType uitype = new UIType(path, name);
    public List<Button> QueueMissionsButton;
    public List<Button> MissionsBlock;
    Dictionary<int, Button> QueueButtonAndTaskID_Dict = new Dictionary<int, Button>();//将任务按钮的序号和按钮本身一一对应起来
    Dictionary<int, Button> MissionsBlock_Dict = new Dictionary<int, Button>();//将添加任务的按钮的序号和按钮本身一一对应起来
    public TextMeshProUGUI Title;
    Button GetRewards;
    Image GetRewardsImage;
    Slider progressBar; // 任务进度条
    TextMeshProUGUI progressText; // 任务进度百分比文本
    
    MissionDate currentMission = MissionManager.GetInst().currentTask;
    Coroutine progressCoroutine;
    
    public MissionQueuePanel() : base(uitype)
    {

    }


    public override void OnDestroy()
    {
        base.OnDestroy();
        if (progressCoroutine != null)
        {
            UIFunction.Inst.StopTheCoroutine(progressCoroutine);
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        UIFunction.Inst.StartFade(0.2f, this, false);
        if (progressCoroutine != null)
        {
            UIFunction.Inst.StopTheCoroutine(progressCoroutine);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        UIFunction.Inst.StartFade(0.2f, this, true);
        Title = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Title");
        progressBar = UIFunction.Inst.GetCompoentInObjsChild<Slider>(PanelObj, "ProgressBar");
        progressText = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "ProgressText");
        QueueMissionsButton = UIFunction.Inst.GetComponentsInObjsChild<Button>(PanelObj, "SelectionMission");
        MissionsBlock = UIFunction.Inst.GetComponentsInObjsChild<Button>(PanelObj, "MissionBlock");
        GetRewardsImage= UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "GetRewards");
        GetRewards = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "GetRewards");
        for (int index = 0; index < QueueMissionsButton.Count; index++)
        {
            // 如果字典中没有当前任务索引的按钮，则添加
            if (!QueueButtonAndTaskID_Dict.ContainsKey(index))
                QueueButtonAndTaskID_Dict.Add(index, QueueMissionsButton[index]);
        }
        for (int index = 0; index < MissionsBlock.Count; index++)
        {
            if (!MissionsBlock_Dict.ContainsKey(index))
                MissionsBlock_Dict.Add(index, MissionsBlock[index]);
        }
        UpdateQueueimage();
        ShowProgress();
        progressCoroutine = UIFunction.Inst.StartCoroutineWithInterval(ShowProgress, 15f);
    }

    public override void OnStart()
    {
        base.OnStart();
        currentMission = MissionManager.GetInst().currentTask;
        UIFunction.Inst.StartFade(0.2f, this, true);
        Title = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "Title");
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Back").onClick.AddListener(Back);
        GetRewardsImage = UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "GetRewards");
        GetRewards =UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "GetRewards");
        GetRewards.onClick.AddListener(Finish);
        if (GetRewards == null)
        {
            Debug.LogError("GetRewards is not set or has been destroyed!");
        }
        progressBar = UIFunction.Inst.GetCompoentInObjsChild<Slider>(PanelObj, "ProgressBar");
        progressText = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "ProgressText");
        QueueMissionsButton = UIFunction.Inst.GetComponentsInObjsChild<Button>(PanelObj, "SelectionMission");
        MissionsBlock = UIFunction.Inst.GetComponentsInObjsChild<Button>(PanelObj, "MissionBlock");
        foreach (Button button in MissionsBlock)
        {
            button.onClick.AddListener(Back);
        }
        foreach(Button button1 in QueueMissionsButton)
        {
            button1.onClick.AddListener(Select);
        }
        ShowProgress();
        // 启动协程，每分钟更新一次进度条
        progressCoroutine = UIFunction.Inst.StartCoroutineWithInterval(ShowProgress, 15f);
        //将序号与按钮一一对应
        for (int index = 0; index < QueueMissionsButton.Count; index++)
        {
            // 如果字典中没有当前任务索引的按钮，则添加
            if (!QueueButtonAndTaskID_Dict.ContainsKey(index))
                QueueButtonAndTaskID_Dict.Add(index, QueueMissionsButton[index]);           
        }
        for (int index = 0; index < MissionsBlock.Count; index++)
        {
            if (!MissionsBlock_Dict.ContainsKey(index))
                MissionsBlock_Dict.Add(index, MissionsBlock[index]);
        }
            
        //如果有已选择的任务，则读取并一一对应
        UpdateQueueimage();
    }
    void UpdateQueueimage()
    {
        //QueueButtonAndTaskID_Dict.Clear();
        foreach (var button in QueueButtonAndTaskID_Dict)
            UIFunction.Inst.SetCanvasGroupVise(button.Value.gameObject, false);
        foreach (var block in MissionsBlock_Dict)
            UIFunction.Inst.SetCanvasGroupVise(block.Value.gameObject, true);
        //button.Value.gameObject.SetActive(false);// 禁用所有任务按钮

        //Debug.Log(MissionManager.GetInst().taskQueue.Count);
        if (MissionManager.GetInst().taskQueue.Count == 0)       
            return;
        
        else
        {
            for (int i = 0; i < MissionManager.GetInst().taskQueue.Count; i++)            
                GetMissionInfo(i);           
        }
            
    }
    void Select()//点击查看详情任务，也就是打开任务窗口
    {
        Button clickedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        if (clickedButton != null)
        {
            UIFunction.Inst.SetCanvasGroupVise(Title.gameObject, false);
            UIFunction.Inst.SetCanvasGroupVise(clickedButton.gameObject, false);
            QueueDetailWindow queueDetailWindow = new QueueDetailWindow();
            queueDetailWindow.task = new List<MissionDate>(MissionManager.GetInst().taskQueue)[int.Parse(clickedButton.gameObject.name)];//很脑缠的办法
            queueDetailWindow.viewPanel = this;
            queueDetailWindow.ClickButton = clickedButton.gameObject;//在退出时会重新显示预览界面
            UIManager.GetInst().Push(queueDetailWindow, true);
        }
        else
        {
            Debug.LogWarning("无法找到被点击的按钮");
        }
    }
    void Back()
    {
        UIManager.GetInst().Pop(false);
    }

    void GetMissionInfo(int index)
    {

        // 如果任务队列不为空，处理任务与按钮映射
        List<MissionDate> taskQueueList = new List<MissionDate>(MissionManager.GetInst().taskQueue);

        // 检查索引是否在任务队列的范围内
        if (index >= 0 && index < taskQueueList.Count)
        {
            MissionDate task = taskQueueList[index]; // 获取当前索引的任务信息

            // 设置任务按钮状态和信息
            UIFunction.Inst.SetCanvasGroupVise(QueueButtonAndTaskID_Dict[index].gameObject, true);
            UIFunction.Inst.SetCanvasGroupVise(MissionsBlock_Dict[index].gameObject, false); // 禁用对应的MissionsBlock按钮
            switch (task.rarity.key)
            {
                case "Common":
                    UIFunction.Inst.GetComponentInObj<Image>(QueueButtonAndTaskID_Dict[index].gameObject).sprite = Resources.Load<Sprite>("IMG/UIEdgeWhite");
                    break;
                case "Rare":
                    UIFunction.Inst.GetComponentInObj<Image>(QueueButtonAndTaskID_Dict[index].gameObject).sprite = Resources.Load<Sprite>("IMG/UIEdgeBlue");
                    break;
                case "VeryRare":
                    UIFunction.Inst.GetComponentInObj<Image>(QueueButtonAndTaskID_Dict[index].gameObject).sprite = Resources.Load<Sprite>("IMG/UIEdgePurple");
                    break;
                case "SuperRare":
                    UIFunction.Inst.GetComponentInObj<Image>(QueueButtonAndTaskID_Dict[index].gameObject).sprite = Resources.Load<Sprite>("IMG/UIEdgeGloden");
                    break;
            }
            Debug.Log(index + "任务ID" + task.id + "稀有度为" + task.rarity.key);
            switch (task.type)
            {
                case "材料研发":
                    UIFunction.Inst.GetCompoentInObjsChild<Image>(QueueButtonAndTaskID_Dict[index].gameObject, "Image").sprite = Resources.Load<Sprite>("IMG/Rewards/MaterialResearch");
                    break;
                case "基地升级":
                    UIFunction.Inst.GetCompoentInObjsChild<Image>(QueueButtonAndTaskID_Dict[index].gameObject, "Image").sprite = Resources.Load<Sprite>("IMG/Rewards/BaseUpgrade");
                    break;
                case "订单接取":
                    UIFunction.Inst.GetCompoentInObjsChild<Image>(QueueButtonAndTaskID_Dict[index].gameObject, "Image").sprite = Resources.Load<Sprite>("IMG/Rewards/MaterialTrade");
                    break;
                case "工厂革新":
                    UIFunction.Inst.GetCompoentInObjsChild<Image>(QueueButtonAndTaskID_Dict[index].gameObject, "Image").sprite = Resources.Load<Sprite>("IMG/Rewards/BaseUpgrade");
                    break;
                case "任务部":
                    UIFunction.Inst.GetCompoentInObjsChild<Image>(QueueButtonAndTaskID_Dict[index].gameObject, "Image").sprite = Resources.Load<Sprite>("IMG/Rewards/TaskDepartMentUpgrade");
                    break;
            }
            UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(QueueButtonAndTaskID_Dict[index].gameObject, "Title").text = task.type;
            UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(QueueButtonAndTaskID_Dict[index].gameObject, "Rank").text = UIFunction.Inst.RomanMap[task.level];
            UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(QueueButtonAndTaskID_Dict[index].gameObject, "Time").text = System.TimeSpan.FromSeconds(task.duration).ToString();

            // 奖励图案设置
            GameObject reward = UIFunction.Inst.GetChild(QueueButtonAndTaskID_Dict[index].gameObject, "Reward");
            List<Image> rewardImage = UIFunction.Inst.GetComponentsInObj<Image>(reward);

            bool rewardCoinShown = false, newBuildingShown = false, newTechShown = false, rewardPUShown = false;

            foreach (Image image in rewardImage)
            {
                if (!rewardCoinShown && task.rewardCoin > 0)
                {
                    image.sprite = Resources.Load<Sprite>("IMG/Rewards/Coin");
                    rewardCoinShown = true;
                }
                else if (!newBuildingShown && !string.IsNullOrEmpty(task.newBuliding))
                {
                    image.sprite = Resources.Load<Sprite>("IMG/Rewards/Building");
                    newBuildingShown = true;
                }
                else if (!newTechShown && !string.IsNullOrEmpty(task.newTech))
                {
                    image.sprite = Resources.Load<Sprite>("IMG/Rewards/Tech");
                    newTechShown = true;
                }
                else if (!rewardPUShown && task.rewardPU > 0)
                {
                    image.sprite = Resources.Load<Sprite>("IMG/Rewards/PU");
                    rewardPUShown = true;
                }
                else
                {
                    image.gameObject.SetActive(false);  // 禁用不需要的奖励图案
                }
            }
        }
        else
        {
            Debug.LogWarning("不应该存在的情况");
            // 如果索引超出范围，禁用相关按钮
            if (QueueButtonAndTaskID_Dict.ContainsKey(index))
                UIFunction.Inst.SetCanvasGroupVise(QueueButtonAndTaskID_Dict[index].gameObject, false);
            //QueueButtonAndTaskID_Dict[index].gameObject.SetActive(false);

            if (MissionsBlock_Dict.ContainsKey(index))
                UIFunction.Inst.SetCanvasGroupVise(MissionsBlock_Dict[index].gameObject, true);
                //MissionsBlock_Dict[index].gameObject.SetActive(true);
        }
    }


    public void ShowProgress()
    {
        MissionManager.GetInst().LoadTaskQueue();
        currentMission = MissionManager.GetInst().currentTask;
        Debug.Log("Progress被调用");

        if (GetRewards == null )
            GetRewards = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "GetRewards");
        SetGetRewardsImageVisible();
        if (currentMission == null)
        {
            progressBar.value = 0;
            progressText.text = "等待添加任务";
            return;
        }
    
        if (currentMission != null && currentMission.duration > 0)
        {
            // 获取剩余时间
            int remainingTime = MissionManager.GetInst().GetRemainingTime(currentMission);

            // 计算剩余时间与总时间的比例
            float progressRatio = (float)(currentMission.duration - remainingTime) / currentMission.duration;

            // 确保比例在 0 到 1 之间
            progressRatio = Mathf.Clamp01(progressRatio);

            // 设置进度条的值
            progressBar.value = progressRatio;

            // 显示任务完成的百分比
            int percentage = Mathf.RoundToInt(progressRatio * 100);
            progressText.text = $"{percentage}% 完成";
        }
        else
        {
            progressBar.value = 0;
            progressText.text = "0% 完成";
        }
    }


    void Finish()
    {
        //Debug.Log(MissionManager.GetInst().taskQueue.Peek().id);
        MissionManager.GetInst().GetCompleteReward();
        UpdateQueueimage();
        //领取完成后禁用完成按钮
        SetGetRewardsImageVisible();
    }

    private void SetGetRewardsImageVisible()
    {
        if (MissionManager.GetInst().taskQueue.Count == 0 || MissionManager.GetInst().taskQueue.Peek() == null || !MissionManager.GetInst().taskQueue.Peek().IsComplete)
        {
            UIFunction.Inst.SetCanvasGroupVise(GetRewards.gameObject, false);
            /*GetRewards.interactable = false;
            Color currentColor = GetRewardsImage.color;
            currentColor.a = 0;
            GetRewardsImage.color = currentColor;*/
        }
        else if (MissionManager.GetInst().taskQueue.Peek() != null && MissionManager.GetInst().taskQueue.Peek().IsComplete)// 启用完成按钮
        {
            UIFunction.Inst.SetCanvasGroupVise(GetRewards.gameObject, true);
            /*GetRewards.interactable = true;
            Color currentColor = GetRewardsImage.color;
            currentColor.a = 255;
            GetRewardsImage.color = currentColor;
            //GetRewards.gameObject.SetActive(true);*/
        }
    }

    
}
