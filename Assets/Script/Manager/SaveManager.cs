using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;
/// <summary>
/// 管理储存读取的脚本，内部方法在关卡过关，委托完成
/// </summary>

public partial class SaveManager
{
    private static SaveManager Inst;
    public static int CurrentLevel=1;
    public static int LastesLevel=1;
    public bool AutoDO = true;
    //public static int LevelCost;//关卡消耗聚合基元的数量
    [Tooltip("关卡总数")]
    public int MaxLevel = 12;//一共的关卡总数
    public int PlayerProgress ;
    public string PlayerName;
    public static int CurrentCoin { get; private set; }
    public static int CurrentPU { get; private set; }
    public static int CurrentFinishedTaskNum { get; set; }
    public float TheSetMusicVolum = 1;
    public float TheSetEffectVolum = 1;
    public int TaskDepartment = 1;


    public static int SaveRefresh { get; private set; }//任务刷新次数
    public string LastChangeTime;//上次刷新的时间
    public static bool IsFirstEnter;

    private string starRecordFilePath = Application.persistentDataPath + "/LevelStarRecord.json";
    private LevelStarRecordList levelStarRecordList = new LevelStarRecordList();

    //public Dictionary<string, bool> TechAndBuildingTree { get; private set; }
    public static SaveManager GetInst()
    {
        if (Inst == null)
        {
            Debug.LogError("未找到SaveManager实例");
            return Inst;
        }
        return Inst;
    }
    public SaveManager()
    {
        Inst = this;
        //TechAndBuildingTree = new Dictionary<string, bool>();
    }
    public void OnStart()
    {
        IsFirstEnter = LoadPlayerDate();
        UpdateProgress();
        InitLevelStarRecords();

    }
    public void InitLevelStarRecords()
    {
        if (File.Exists(starRecordFilePath))
        {
            string json = File.ReadAllText(starRecordFilePath);
            levelStarRecordList = JsonUtility.FromJson<LevelStarRecordList>(json);
            Debug.Log("读取已有星级记录文件成功！");
        }
        else
        {
            Debug.Log("未找到星级记录文件，自动初始化中...");

            for (int i = 1; i <= MaxLevel; i++)
            {
                levelStarRecordList.records.Add(new LevelStarRecord
                {
                    levelNumber = i,
                    starCount = 0
                });
            }

            SaveLevelStarRecords();
        }
    }
    private void SaveLevelStarRecords()
    {
        string json = JsonUtility.ToJson(levelStarRecordList, true);
        File.WriteAllText(starRecordFilePath, json);
    }

    /// <summary>
    /// 更新金币后会自动保存游戏
    /// </summary>
    /// <param name="num">要在当前金钱基础上增加或减少的金钱</param>
    public void ResourcesChange(int num,string type="coin")
    {
        if (type == "coin"|| type == "EU")
        {
            CurrentCoin += num;
            AchievementManager.GetInst().CheckAchievProgress("coin");
        }
            
        else if (type == "PU")
        {
            CurrentPU += num;
            AchievementManager.GetInst().CheckAchievProgress("PU");
        }
            
        else return;
        
        //保存当前金币数量
        SavePlayerDate();
    }
    public void AddFinishedTaskNum()
    {
        ++CurrentFinishedTaskNum;
        AchievementManager.GetInst().CheckAchievProgress("task");
        SavePlayerDate();
    }
    
    public int ChangeRefreshChance(int num=0)
    {
        SaveRefresh += num;
        SavePlayerDate();
        return SaveRefresh;
    }
    /// <summary>
    /// 用于计算关卡将消耗多少聚合基元
    /// </summary>
    /// <returns></returns>
    public static int CountLevelCost(int level)
    {
        if (level < LastesLevel)
            return ((level - 1) * 5 + 20)/5;
        else
            return (level - 1) * 5 + 20;
    }
    // 检查当前时间是否需要重置刷新次数
    public void CheckAndResetRefreshTimes()
    {
        DateTime now = DateTime.Now;
        DateTime todayMidnight = now.Date; // 今天的 00:00

        if (string.IsNullOrEmpty(LastChangeTime))
        {
            Debug.Log("LastChangeTime似乎无法被正确读取");        
            Debug.Log(LastChangeTime);
            return;
        }

        DateTime lastRefershTime = DateTime.Parse(LastChangeTime);

        // 如果上次刷新记录早于今天的 00:00，并且当前时间是今天之后的
        if (lastRefershTime < todayMidnight && now >= todayMidnight)
        {
            SaveRefresh = 2;
            LastChangeTime = now.ToString("o");
            SavePlayerDate();
            Debug.Log("刷新次数已重置为 2！（每日 00:00 触发）");
        }
        else
        {
            Debug.Log("当前不满足每日刷新时间（00:00），未重置刷新次数。");
        }
    }

    public void UnlockTechByLastesLevel()//解锁编号从1到LastesLevel-1的所有科技
    {
        for (int i = 1; i < LastesLevel; i++) // 注意：不包含 LastesLevel
        {
            Technology tech = TechTreeManager.GetInst().GetTech(i);
            if (tech != null && !tech.IsUnlock)
            {
                TechTreeManager.GetInst().UnlockTechnology(tech.TechName);
            }
        }
    }
    /// <summary>
    /// 过关的时候调用，如果当前关卡是最远关卡，并且不是最后一关，则更新最远关卡(不负责加载场景，只负责结算画面)
    /// </summary>
    /// <param name="FarstLevel">当前关卡是不是最远关卡</param>
    public void PassLevel()
    {
        int levelReward = 100;//每关奖励的乘数
        int firstReward = 200 * CurrentLevel;//新通关的额外奖励数
        //1星0.75倍率，3星1.25倍率
        int randomCoin = (int)((0.75 + 0.25 * (LevelManager.GetInst().starNumJudge()-1)) * UnityEngine.Random.Range(CurrentLevel * levelReward * 0.8f, CurrentLevel * levelReward * 1.2f));
        //int testCoin= (int)((0.75f + 0.25 * LevelManager.GetInst().starNumJudge()) * Random.Range( 60 * 0.85f, 60 * 1.1f));
        if (CurrentLevel >= LastesLevel)//如果是新通关
        {
            /*if (CurrentLevel == 1 && LastesLevel == 0)
                LastesLevel = 2;*/
            
            LastesLevel++;//同时解锁编号LastesLevel-1的科技
            //也要加入主线关卡类成就的检查
            if (LastesLevel != MaxLevel+1)//并且不是最后一关
            {
                
                //如果是第一次通关，获得额外金币
                ResourcesChange(100 * CurrentLevel+ (int)(UnityEngine.Random.Range(1.0f,1.25f) * randomCoin));//金币结算，根据当前关数和星数增加一定随机范围的金币
                UIManager.GetInst().Push(new VectoryPanel());//显示胜利UI
                GetCoinPanel coinPanel = new GetCoinPanel();
                coinPanel.CoinNumber = (firstReward + levelReward * CurrentLevel + randomCoin);
                //AchievementManager.GetInst().CheckAchievProgress("mainlineLevel");//检查关卡类成就
                UIManager.GetInst().Push(coinPanel);//显示获取金币UI
                Debug.Log("是新通关");
            }                                                    
            else//是最后一关
            {               
                ResourcesChange(randomCoin);
                UIManager.GetInst().Push(new VectoryPanel());//显示最后一关胜利UI，没有前往下一关的选项
                GetCoinPanel coinPanel = new GetCoinPanel();
                coinPanel.CoinNumber = (randomCoin);
                UIManager.GetInst().Push(coinPanel);//显示获取金币UI
                Debug.Log("是最后一关");
            }
            UpdateProgress();

        }
        else//不是新通关，旧关，CurrentLevel < LastesLevel
        {
            ResourcesChange(randomCoin);
            UIManager.GetInst().Push(new VectoryPanel());//显示胜利UI
            GetCoinPanel coinPanel = new GetCoinPanel();
            coinPanel.CoinNumber = (randomCoin);
            UIManager.GetInst().Push(coinPanel);//显示获取金币UI
            Debug.Log("不是新通关，旧关");
        }

        AchievementManager.GetInst().CheckAchievProgress("mainlineLevel");
        AchievementManager.GetInst().CheckAchievProgress("star");
        //在UpdateLevelStar处进行星数成就判断
        UnlockTechByLastesLevel();
    }
    public void UpdateProgress()
    {
        // 主线关卡进度
        int levelProgress = (LastesLevel - 1) * 250;

        // 解锁科技数量（通过 TechTreeManager 访问,不包含主线关卡的科技）
  
        int techProgress = TechTreeManager.GetInst().TotalTechProgress();

        // 解锁建筑数量（通过 BuildingManager 访问）
        int buildingProgress = BuildingManager.GetInst().TotalBuildingProgress();

        // 总进度值
        int totalProgress = levelProgress + techProgress + buildingProgress;
        totalProgress = Mathf.Clamp(totalProgress, 0, 10000);

        PlayerProgress = totalProgress;
        if (PlayerProgress >= 3000 && PlayerProgress < 6000 )
        {
            BuildingManager.GetInst().UnlockBuilding("CommandStation_Level2");
            TaskDepartment = 2;
        }
            
        if(PlayerProgress >=6000 && PlayerProgress < 10000 )
        {
            BuildingManager.GetInst().UnlockBuilding("CommandStation_Level3");
            TaskDepartment = 3;
        }
        if(PlayerProgress >=10000)
        {
            BuildingManager.GetInst().UnlockBuilding("CommandStation_Level4");
        }
        Debug.Log($"✅ 游戏进度更新成功，当前进度：{PlayerProgress} / 10000");
    }
    /// <summary>
    /// 提供用于更新星级的方法（在通关后调用）
    /// </summary>
    /// <param name="level"></param>
    /// <param name="star"></param>
    public void UpdateLevelStar( int star)
    {
        InitLevelStarRecords();
        LevelStarRecord existing = levelStarRecordList.records.Find(r => r.levelNumber == SaveManager.CurrentLevel);
        if (existing != null)
        {
            if (star > existing.starCount)
            {
                existing.starCount = star;
                SaveLevelStarRecords();
                AchievementManager.GetInst().CheckAchievProgress("star");
                Debug.Log($"更新关卡 {SaveManager.CurrentLevel} 星级为 {star} 星！");
            }
        }
        else
        {
            // 如果意外未找到该关卡，补充进去
            levelStarRecordList.records.Add(new LevelStarRecord { levelNumber = SaveManager.CurrentLevel, starCount = star });
            SaveLevelStarRecords();
        }
    }
    /// <summary>
    /// 获取某关卡的星数
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public int GetLevelStar(int level)
    {
        InitLevelStarRecords();
        LevelStarRecord record = levelStarRecordList.records.Find(r => r.levelNumber == level);
        return record != null ? record.starCount : 0;
    }

}
