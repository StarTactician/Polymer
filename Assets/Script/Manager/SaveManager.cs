using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
/// <summary>
/// 管理储存读取的脚本，内部方法在关卡过关，委托完成
/// </summary>

public partial class SaveManager
{
    private static SaveManager Inst;
    public static int CurrentLevel;
    public static int LastesLevel;
    //public static int LevelCost;//关卡消耗聚合基元的数量
    public int MaxLevel = 3;//一共的关卡总数
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
        LoadPlayerDate();
     
    }
/// <summary>
/// 更新金币后会自动保存游戏
/// </summary>
/// <param name="num">要在当前金钱基础上增加或减少的金钱</param>
    public void ResourcesChange(int num,string type="EU")
    {
        if(type=="EU")
        CurrentCoin += num;
        else if(type=="PU")
        CurrentPU += num;
        //保存当前金币数量
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
    public static int CountLevelCost()
    {
        return (CurrentLevel-1)*5+20;
    }
    // 检查当前时间是否需要重置刷新次数
    public void CheckAndResetRefreshTimes()
    {
        DateTime now = DateTime.Now;
        DateTime lastRefershTime;
        if (!string.IsNullOrEmpty(LastChangeTime))
        {
            lastRefershTime = DateTime.Parse(LastChangeTime);
        }
        else
        {
            lastRefershTime = now;
        }
        
        // 如果当前时间的日期不同于上次重置时间的日期，或者当前时间超过 12 点
        if (string.IsNullOrEmpty(LastChangeTime) || lastRefershTime.Date != now.Date || (now.TimeOfDay.Hours >= 12 && lastRefershTime.TimeOfDay.Hours < 12))
        {
            SaveRefresh = 2; // 重置刷新次数
            LastChangeTime = now.ToString("o"); // 更新重置时间
            SavePlayerDate(); // 保存数据
            //Console.WriteLine("刷新次数已重置为 2！");
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
        if (CurrentLevel == LastesLevel)//如果是新通关
        {
            if (LastesLevel != MaxLevel)//并且不是最后一关
            {
                LastesLevel++;
                //如果是第一次通关，获得额外金币
                ResourcesChange(100 * CurrentLevel+ (int)(UnityEngine.Random.Range(1.0f,1.25f) * randomCoin));//金币结算，根据当前关数和星数增加一定随机范围的金币
                UIManager.GetInst().Push(new VectoryPanel());//显示胜利UI
                GetCoinPanel coinPanel = new GetCoinPanel();
                coinPanel.CoinNumber = (firstReward + levelReward * CurrentLevel + randomCoin);
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

        }
        else//不是新通关，旧关，CurrentLevel ！= LastesLevel
        {
            ResourcesChange(randomCoin);
            UIManager.GetInst().Push(new VectoryPanel());//显示胜利UI
            GetCoinPanel coinPanel = new GetCoinPanel();
            coinPanel.CoinNumber = (randomCoin);
            UIManager.GetInst().Push(coinPanel);//显示获取金币UI
            Debug.Log("不是新通关，旧关");
        }                
    }

}
