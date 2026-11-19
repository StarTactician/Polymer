using System;
using System.Collections.Generic;

[System.Serializable]
public class MissionDate
{
    public string id;// 任务的唯一标识符
    public string type;     // 任务的类型（例如：材料研发、基地建设等）
    public string description; // 任务的描述信息
    public int cost;        // 任务需要投入的资源数量
    public int level;         // 任务的等级或难度
    public int duration;      // 任务的持续时间（以秒为单位）
    public int rewardCoin; //任务的奖励1金币
    public int rewardPU; //任务的奖励1金币
    public string newBuliding;//任务的奖励2解锁的工厂，合成方式，科技线等
    public string newTech;//任务的奖励3解锁的工厂，合成方式，科技线等
    public bool IsComplete;
    //public bool CanFinishOnce;//只能完成一次的任务，完成后将条件里的设置为true
    public string startTime; //任务开始的时间
    public Rarity rarity;          // 任务的稀有度
    public List<Condition> conditions = new List<Condition>(); // 条件集合，用于控制任务的刷新

    /*public enum Rarity
    {
        Common,     // 普通
        Rare,       // 稀有
        VeryRare,   // 非常稀有
        SuperRare   // 超级稀有
    }*/
    public MissionDate Clone()
    {
        return new MissionDate
        {
            id = this.id,
            type = this.type,
            description = this.description,
            cost = this.cost,
            level = this.level,
            duration = this.duration,
            rewardCoin = this.rewardCoin,
            rewardPU=this.rewardPU,
            newBuliding = this.newBuliding,
            newTech = this.newTech,
            IsComplete = this.IsComplete,
            //CanFinishOnce=this.CanFinishOnce,
            startTime = this.startTime,
            rarity = this.rarity,
            //conditions = new List<Condition>(this.conditions) // 深拷贝条件
        };
    }
}
[System.Serializable]
public class Rarity
{
    public string key; // 稀有度名称，例如 "Common", "Rare"
}
[System.Serializable]
public class Condition
{
    public string key;
    public bool canNotShowOut;//只能完成一次的任务
}
[System.Serializable]
public class MissionList
{
    public List<MissionDate> tasks;  // 包含多个任务的列表
    
}
[Serializable]
public class TaskIdList
{
    public List<string> taskIds;
}


