using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class MissionManager
{
    private const int CarbonFiberTechnologyId = 25;
    private const int LevelTwoLandingPadId = 10;
    private static MissionManager Inst;
    public int CompeletMission;

    public List<MissionDate> taskPool = new List<MissionDate>();
    private List<MissionDate> commonTasks = new List<MissionDate>();
    private List<MissionDate> rareTasks = new List<MissionDate>();
    private List<MissionDate> veryRareTasks = new List<MissionDate>();
    private List<MissionDate> superRareTasks = new List<MissionDate>();

    private Dictionary<string, MissionDate> taskMap = new Dictionary<string, MissionDate>();//进入游戏时将任务内容与任务ID一一对应

    public List<MissionDate> UnselectionTasks = new List<MissionDate>();//当前待选的任务

    public Queue<MissionDate> taskQueue = new Queue<MissionDate>(); // 排队中的任务，目前还没有设置最大队列长度，当前准备设置最大为5
    public MissionDate currentTask = null; // 正在进行的任务
    public int MaxQueueNum = 5;
    private string missionQueueFilePath = Application.persistentDataPath + "/CurrentMissionQueue.json";
    private string unselectionTasksFilePath = Application.persistentDataPath + "/UnselectionTasks.json";
    private bool isFirstLoadInSession = true; // 用于判断是否是本次游戏的第一次调用LoadQueue函数
    public static MissionManager GetInst()
    {
        if (Inst == null)
        {
            Debug.LogError("MissionManager");
            return Inst;
        }
        return Inst;
        //加载任务队列和待选任务队列
    }
    public MissionManager()
    {
        Inst = this;
        LoadLibraryFromJson();
    }             
    #region 任务的出现
    /// <summary>
    /// 加载任务库，并把任务根据稀有度分配到对应的任务池中。在需要更新待选任务时被调用
    /// </summary>
    private void LoadLibraryFromJson()
    {
        TextAsset jsonText = Resources.Load<TextAsset>("MissionLibrary");
        if (jsonText != null)
        {
            MissionList taskList = JsonUtility.FromJson<MissionList>(jsonText.text);//将Resources文件夹中的数据读取出了存储进taskList
            // 检查解析后的任务列表是否为空
            if (taskList == null || taskList.tasks == null)
            {
                Debug.LogError("JSON 解析失败：任务列表为空。");
                return;
            }
            taskPool = taskList.tasks;//赋值给游戏中
            // 检查任务池是否为空
            if (taskPool.Count == 0)
            {
                Debug.LogWarning("任务池为空，可能是 JSON 文件中没有任务条目。");
            }

            foreach (MissionDate task in taskList.tasks)// 根据稀有度将任务添加到不同的池中
            {
                //Debug.Log($"任务 {task.id} 条件：{task.conditions.Keys}, 值：{task.conditions.Values}");
                if (string.IsNullOrEmpty(task.id))
                {
                    Debug.LogWarning("任务的 ID 为空，跳过此任务。");
                    continue;
                }
                else
                {
                    taskMap[task.id] = task;
                }
                RefreshTaskPool(task);
            }
            Debug.Log("任务库已成功加载，任务数量：" + taskPool.Count+"普通任务"+ commonTasks.Count+"稀有任务"+rareTasks.Count+"非常稀有任务"+veryRareTasks.Count+"极其稀有任务"+superRareTasks.Count);
        }
        else
        {
            Debug.LogError("未能加载任务库：找不到 MissionLibrary 文件");
        }        
    }
   
    /// <summary>
    /// 刷新任务池的
    /// </summary>
    /// <param name="task"></param>
    public void RefreshTaskPool(MissionDate task)
    {
        // 如果该任务已在当前任务队列中，则不添加到刷新池中
        if (taskQueue.Any(t => t.id == task.id))
        {
            if (isFirstLoadInSession)
                Debug.Log($"任务 {task.id} 已在队列中，跳过刷新。");
            return;
        }
        bool canShowOut = ConditionsJudge(task);
        Debug.Log($"任务 {task.id}可刷新="+canShowOut);
        if (canShowOut)
        {
            switch (task.rarity.key)
            {
                case "Common":
                    commonTasks.Add(task);
                    break;
                case "Rare":
                    rareTasks.Add(task);
                    break;
                case "VeryRare":
                    veryRareTasks.Add(task);
                    break;
                case "SuperRare":
                    superRareTasks.Add(task);
                    break;
                default:
                    Debug.LogWarning($"未知稀有度: {task.rarity.key}");
                    break;
            }
            /*switch (task.rarity)
            {
                case MissionDate.Rarity.Common:
                    commonTasks.Add(task);
                    break;
                case MissionDate.Rarity.Rare:
                    rareTasks.Add(task);
                    break;
                case MissionDate.Rarity.VeryRare:
                    veryRareTasks.Add(task);
                    break;
                case MissionDate.Rarity.SuperRare:
                    superRareTasks.Add(task);
                    break;
                default:
                    Debug.Log("任务稀有度无法判断！");
                    break;
            }*/

        }
        
    }
    /// <summary>
    /// 根据任务的ID返回任务相关的信息
    /// </summary>
    /// <param name="taskId">任务的ID</param>
    /// <returns></returns>
    public MissionDate GetTaskById(string taskId)
    {
        if (string.IsNullOrEmpty(taskId))
        {
            Debug.LogError("无法根据空的任务 ID 查找任务。");
            return null;
        }
        if (taskMap.TryGetValue(taskId, out MissionDate task))
        {
            return task;
        }
        Debug.LogError("任务 ID 无效：" + taskId);
        return null;
    }
    /// <summary>
    /// 生成指定数量任务的外部调用函数
    /// </summary>
    /// <param name="num"></param>
    public void GenerateTaskList(int num)
    {
        UnselectionTasks.Clear();
        for (int i = 0; i < num; i++)
        {
            UnselectionTasks.Add(GenerateRandomTask());
        }
        Debug.Log("任务个数"+UnselectionTasks.Count);
        SaveUnselectionTasks();
    }
    
    #endregion
    #region 任务的选择
    /// <summary>
    /// 指定任务加入队列
    /// </summary>
    /// <param name="mission">指定的任务</param>
    public void AddTaskToQueue(MissionDate mission)//其实这样的话完全可以传入任务ID
    {
        if (mission == null)
        {
            Debug.LogError("无法将任务加入队列，任务无效");
            return;
        }
        MissionDate onlyMission = mission.Clone();//涉及到唯一的属性，时间类，使用它
        // 检查当前任务队列是否已达到最大限制
        //int totalTasks = taskQueue.Count;
        if (taskQueue.Count >= MaxQueueNum)
        {
            TipsJumpWindow jumpWindow = new TipsJumpWindow();
            jumpWindow.Text = "任务队列已满，无法添加新任务!";
            UIManager.GetInst().PushTips(jumpWindow);
            Debug.LogWarning("任务队列已满，无法添加新任务");
            return;
        }
        if (mission.cost > 0 && SaveManager.CurrentCoin < mission.cost)
        {
            TipsJumpWindow jumpWindow = new TipsJumpWindow();
            jumpWindow.Text = "资金不够!";
            UIManager.GetInst().PushTips(jumpWindow);
            Debug.LogWarning("资金不够");
            return;
        }
        else
            SaveManager.GetInst().ResourcesChange(-mission.cost);
        // 将任务加入队列
        if(taskQueue.Count==0|| GetNextIncompleteTask()==null)//如果队列为空或者没有未完成的任务则
            onlyMission.startTime = DateTime.Now.ToString("o");
        else if(string.IsNullOrEmpty(onlyMission.startTime))// 如果队列中有未完成的任务，将任务的开始时间设置为上一个未完成任务的结束时间
        {
            // 计算下一个任务的开始时间，应该是上一个未完成任务的结束时间
            DateTime lastTaskEndTime = DateTime.Parse(GetLastIncompleteTask().startTime).AddSeconds(GetLastIncompleteTask().duration);
            onlyMission.startTime = lastTaskEndTime.ToString("o");
        }
        //MissionDate taskToAdd = mission.Clone();
        TipsJumpWindow jumpWindow1 = new TipsJumpWindow();
        jumpWindow1.Text = "任务已添加!";
        UIManager.GetInst().PushTips(jumpWindow1);
        taskQueue.Enqueue(onlyMission);
        SaveTaskQueue();//添加任务后保存任务队列
        Debug.Log($"任务 {onlyMission.id} 已加入任务队列");

    }
    #endregion
    #region 任务的排队与结束
    /// <summary>
    /// 检查任务是否完成，点击领取任务奖励后调用
    /// </summary>
    public void GetCompleteReward()
    {
        if (taskQueue.Peek() != null && taskQueue.Peek().IsComplete)
        {
            FinishMission(); // 任务完成，处理奖励和下一个任务
            //currentTask = GetNextIncompleteTask(); // 获取下一个未完成的任务
            SaveTaskQueue(); // 保存任务队列
        }
        else
        {
            Debug.LogWarning("当前任务未完成或当前没有任务");
        }
    }
    /// <summary>
    /// 获取指定任务的剩余时间（int）时调用
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    public int GetRemainingTime(MissionDate task)
    {
        DateTime startTime = DateTime.Parse(task.startTime); // 解析任务的开始时间
        TimeSpan elapsedTime = DateTime.Now - startTime;    // 当前时间与开始时间的差值
        int remainingTime = task.duration - (int)elapsedTime.TotalSeconds; // 剩余时间

        return remainingTime > 0 ? remainingTime : 0; // 确保剩余时间不为负
    }
    #endregion
    #region 任务的保存与加载

    /// <summary>
    /// 进入游戏时调用,获取当前已完成了几个任务
    /// </summary>
    public int LoadTaskQueue()
    {
        CompeletMission = 0;//每次调用重置
        if (!File.Exists(missionQueueFilePath))//如果没有被创建这个路径的文件，则创建一个
        {
            SaveTaskQueue();//创建已选择的任务队列
            //SaveTasksToFile(missionQueueFilePath, new List<MissionDate>(taskQueue));
            return 0;
        }
        if (isFirstLoadInSession)//确保只加载读取一次
        {
            // 第一次调用，加载任务队列并同步到 taskQueue
            string jsonString = File.ReadAllText(missionQueueFilePath);
            MissionList savedTaskList = JsonUtility.FromJson<MissionList>(jsonString);

            foreach (MissionDate task in savedTaskList.tasks)
            {
                taskQueue.Enqueue(task);
                if (!task.IsComplete)
                {
                    currentTask = task; // 设置 currentTask 为第一个未完成的任务
                }
                Debug.Log("任务已从 JSON 文件中加载，当前任务：" + task.id+ currentTask);
            }
            
            isFirstLoadInSession = false; // 更新为非首次调用
            Debug.Log("任务队列已从 JSON 文件中加载，任务数量：" + taskQueue.Count);
        }
        // 根据检查结果更新已完成任务数量
        foreach (MissionDate task in taskQueue)
        {
            CheckRemainingTime(task, out task.IsComplete);
            if (task.IsComplete)
            {
                ++CompeletMission;
            }
        }
        currentTask = GetNextIncompleteTask();
        Debug.Log("已完成任务数量"+ CompeletMission);
        return CompeletMission; // 返回已完成的任务数量
    }


    /// <summary>
    /// 任务完成时，领取奖励后调用,退出游戏时调用
    /// </summary>
    public void SaveTaskQueue()
    {
        if (taskQueue.Count == 0)
        {
            Debug.Log("当前没有任务队列需要保存。");           
        }
        List<MissionDate> taskList = new List<MissionDate>(taskQueue);
        SaveTasksToFile(missionQueueFilePath, taskList);
    }
    /// <summary>
    /// 保存待选任务
    /// </summary>
    public void SaveUnselectionTasks()
    {
        if (UnselectionTasks.Count == 0)
        {
            Debug.Log("当前没有待选任务需要保存。");
            return;
        }

        // 创建一个只包含任务ID的列表
        List<string> unselectionTaskIds = UnselectionTasks.Select(task => task.id).ToList();

        // 序列化任务ID列表
        string jsonString = JsonUtility.ToJson(new TaskIdList { taskIds = unselectionTaskIds }, true);

        // 保存到文件
        File.WriteAllText(unselectionTasksFilePath, jsonString);
        Debug.Log("待选任务ID已保存到 JSON 文件，任务数量: " + unselectionTaskIds.Count);
    }

    /// <summary>
    /// 加载待选任务
    /// </summary>
    public void LoadUnselectionTasksFromJson()
    {
        string filePath = Path.Combine(unselectionTasksFilePath);
        if (File.Exists(filePath))
        {
            try
            {
                string json = File.ReadAllText(filePath);
                TaskIdList loadedTaskIdList = JsonUtility.FromJson<TaskIdList>(json);
                if (loadedTaskIdList != null && loadedTaskIdList.taskIds != null)
                {
                    UnselectionTasks.Clear();

                    // 根据任务ID从任务库中加载完整的任务
                    foreach (string taskId in loadedTaskIdList.taskIds)
                    {
                        MissionDate task = GetTaskById(taskId);
                        if (task != null)
                        {
                            UnselectionTasks.Add(task);
                        }
                        else
                        {
                            Debug.LogWarning($"无法找到任务ID为 {taskId} 的任务，请确保任务库已正确加载。");
                        }
                    }

                    Debug.Log("待选任务成功加载，任务数量: " + UnselectionTasks.Count + "任务ID: " + string.Join(", ", loadedTaskIdList.taskIds));
                }
                else
                {
                    Debug.LogWarning("无法正确解析待选任务 JSON 文件。");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("加载待选任务时发生错误: " + ex.Message);
            }
        }
        else
        {
            Debug.LogWarning("待选任务 JSON 文件不存在: " + filePath);
        }
    }

    #endregion

    #region 私有的函数语句
    /// <summary>
    /// 需要生成任务时调用，也就是在任务待选列表需要更新时调用
    /// </summary>
    /// <returns></returns>
    private MissionDate GenerateRandomTask()
    {
        // 定义每个稀有度池的抽取概率
        //float commonProbability = 0.5f;    // 50%
        float rareProbability = 0.4f;      // 40%
        float veryRareProbability = 0.09f; // 9%
        float superRareProbability = 0.01f;// 1%

        // 生成一个0到1之间的随机数，确定稀有度池

        float randomValue = UnityEngine.Random.value;

        if (randomValue < superRareProbability&& superRareTasks.Count>0)
        {
            return GetRandomTaskFromPool(superRareTasks);
        }
        else if (randomValue < superRareProbability + veryRareProbability && veryRareTasks.Count > 0)
        {
            return GetRandomTaskFromPool(veryRareTasks);
        }
        else if (randomValue < superRareProbability + veryRareProbability + rareProbability && rareTasks.Count > 0)
        {
            return GetRandomTaskFromPool(rareTasks);
        }
        else if(commonTasks.Count > 0)
        {
            return GetRandomTaskFromPool(commonTasks);
        }
        Debug.LogError("所有的任务池都为空");
        return null;
    }
    /// <summary>
    /// 从指定的任务池中随机选择一个任务
    /// </summary>
    /// <param name="taskList">指定的任务池</param>
    /// <returns></returns>
    private MissionDate GetRandomTaskFromPool(List<MissionDate> taskList)
    {
        if (taskList.Count == 0)
        {
            Debug.LogWarning("任务池为空，无法选择任务");
            return null;
        }
        int randomIndex = UnityEngine.Random.Range(0, taskList.Count);
        return taskList[randomIndex];
    }
    /// <summary>
    /// 判断当前的任务的刷新条件是否满足，不满足的不会添加到对应池子里
    /// </summary>
    /// <param name="task"></param>
    /// <returns></returns>
    private bool ConditionsJudge(MissionDate task)
    {
        if (task == null)
        {
            Debug.LogError("任务为空，无法检查条件。");
            return false;
        }
        if (task.conditions == null || task.conditions.Count == 0)
        {
            
            Debug.LogWarning("任务条件为空，将视为条件满足。任务ID: " + task.id);
            return true;  // 如果没有定义条件，则默认为满足条件
        }
        foreach (Condition condition in task.conditions)
        {
            
            switch (condition.key)//不能刷新的情况
            {
                case "Progress2":
                    if (condition.canNotShowOut || SaveManager.GetInst().TaskDepartment <2)
                        return false;
                    break;
                case "Progress3":
                    if (condition.canNotShowOut || SaveManager.GetInst().TaskDepartment <3)
                        return false;
                    break;
                case "hasBuiltFactory":
                    if (condition.canNotShowOut || ! BuildingManager.GetInst().IsUnlock("LunarFactory_Level1"))//返回false的情况，也就是所需的科技建筑未解锁
                        return false;
                    break;
                case "hasBuiltMiningBase":
                    if (condition.canNotShowOut || ! BuildingManager.GetInst().IsUnlock("LunarMiningBase"))
                        return false;
                    break;
                case "LandingPadLevel1":
                case "LandingPad_Level1":
                    BuildingManager buildings = BuildingManager.GetInst();
                    if (condition.canNotShowOut || buildings.IsUnlock(LevelTwoLandingPadId) || buildings.IsUnlock("LandingPad_Level2"))
                        return false;
                    break;
                case "LivingStationLevel1":
                    if (condition.canNotShowOut || BuildingManager.GetInst().IsUnlock("LivingStationLevel2"))
                        return false;
                    break;
                case "ResearchCenterLevel1":
                    if (condition.canNotShowOut || BuildingManager.GetInst().IsUnlock("ResearchCenterLevel2"))
                        return false;                   
                    break;                
                case "SolarPanelLevel1":
                    if (condition.canNotShowOut || BuildingManager.GetInst().IsUnlock("SolarPanelLevel2"))
                        return false;                   
                    break;                
                case "notHasBuiltMiningBase":
                    if (condition.canNotShowOut || BuildingManager.GetInst().IsUnlock("LunarMiningBase"))
                        return false;                   
                    break;                
                case "notHasBuiltFactory":
                    if (condition.canNotShowOut || BuildingManager.GetInst().IsUnlock("LunarFactory_Level1"))
                        return false;                   
                    break;
                case "notUnlockBiosphere":
                    if (condition.canNotShowOut || BuildingManager.GetInst().IsUnlock("LunarBiosphere"))
                        return false;                   
                    break;                
                case "notUnlockCarbonFiberComposite":
                    if (condition.canNotShowOut || TechTreeManager.IsTechnologyUnlocked(CarbonFiberTechnologyId))
                        return false;                   
                    break;


                // 可以根据需要扩展更多的条件
                default:
                   //if (isFirstLoadInSession)
                   //    Debug.LogWarning("未知的条件：" + condition.key);
                    return false;
                    
            }
        }
        return true;
        
    }
    private void FinishMission()
    {
        if (taskQueue.Peek() !=null&& taskQueue.Peek().IsComplete)
        {
            
            Debug.Log($"任务 {taskQueue.Peek().id} 已完成");
            // 调用奖励处理代码
            GetCoinPanel rewardPanel = new GetCoinPanel();
            if (taskQueue.Peek().rewardCoin > 0)
            {
                rewardPanel.CoinNumber = (int)(UnityEngine.Random.Range(0.9f, 1.2f) * taskQueue.Peek().rewardCoin);
                SaveManager.GetInst().ResourcesChange(rewardPanel.CoinNumber);
            }
            if (taskQueue.Peek().rewardPU > 0)
            {
                rewardPanel.rewardPuShown = true;
                rewardPanel.PUNumber = taskQueue.Peek().rewardPU;
                SaveManager.GetInst().ResourcesChange(rewardPanel.PUNumber,"PU");
            }
            //PU
            if (!string.IsNullOrEmpty(taskQueue.Peek().newBuliding))//在建筑，科技管理器中解锁对应的
            {
                BuildingManager buildings = BuildingManager.GetInst();
                if (taskQueue.Peek().id == "T009") buildings.UnlockBuilding(LevelTwoLandingPadId, "LandingPad_Level2");
                else buildings.UnlockBuilding(taskQueue.Peek().newBuliding);
                rewardPanel.newBuildingShown = true;
                rewardPanel.Name1 = taskQueue.Peek().newBuliding;
            }
            if (taskQueue.Peek().id == "T016")
            {
                TechTreeManager techTree = TechTreeManager.GetInst();
                Technology technology = techTree.GetTech(CarbonFiberTechnologyId);
                if (technology != null)
                {
                    techTree.UnlockTechnology(CarbonFiberTechnologyId);
                    rewardPanel.newTechShown = true;
                    rewardPanel.Name2 = technology.TechCNName;
                }
            }
            else if (!string.IsNullOrEmpty(taskQueue.Peek().newTech))
            {
                TechTreeManager.GetInst().UnlockTechnology(taskQueue.Peek().newTech);
                rewardPanel.newTechShown = true;
                rewardPanel.Name2 = taskQueue.Peek().newTech;
            }
            /*if (taskQueue.Peek().upgradeTaskDepartment)
            {
                if (SaveManager.GetInst().TaskDepartment < 3)
                    SaveManager.GetInst().TaskDepartment++;
                rewardPanel.upgradeTaskDepartmentShown = true;
            }*/
            SaveManager.GetInst().AddFinishedTaskNum();
            UIManager.GetInst().Push(rewardPanel);
            taskQueue.Dequeue();//
            //++SaveManager.CurrentFinishedTaskNum;
            //AchievementManager.GetInst().CheckAchievProgress("task");
            //taskQueue.Peek() = null; // 将 currentTask 重置为 null，等待下一个任务
            SaveManager.GetInst().SavePlayerDate();
        }
        else Debug.LogError("非法的调用时间");
    }
    private void SaveTasksToFile(string filePath, List<MissionDate> tasks)
    {
        string jsonString = JsonUtility.ToJson(new MissionList() { tasks = tasks }, true);
        File.WriteAllText(filePath, jsonString);
        Debug.Log($"{filePath} 已保存。");
    }
    private bool CheckRemainingTime(MissionDate task, out bool isCompleted)
    {
        if (task == null)
        {
            isCompleted = false;
            return false;
        }

        int remainingTime = GetRemainingTime(task); // 获取剩余时间

        if (remainingTime <= 0)
        {
            isCompleted = true;
            task.IsComplete = true; // 任务已完成但奖励未领取
            SaveTaskQueue(); // 保存任务完成情况
        }
        else
        {
            isCompleted = false;
        }
        return true; // 处理成功
    }

    /// <summary>
    /// 获取队列中下一个未完成的任务，如果所有任务都已完成，则返回空
    /// </summary>
    /// <returns></returns>
    private MissionDate GetNextIncompleteTask()
    {
        foreach (MissionDate task in taskQueue)
        {
            if (!task.IsComplete)
            {
                return task.Clone();
            }
        }
        // 如果所有任务都已完成，返回队列中最后一个任务
        //return taskQueue.Count > 0 ? taskQueue.Last() : null;
        return null;
    }
    private MissionDate GetLastIncompleteTask()
    {
        // 使用 Reverse() 遍历任务队列从后向前查找未完成任务
        foreach (MissionDate task in taskQueue.Reverse())
        {
            if (!task.IsComplete)
            {
                return task.Clone();
            }
        }
        return null; // 如果所有任务都已完成，返回空
    }
    #endregion
}
