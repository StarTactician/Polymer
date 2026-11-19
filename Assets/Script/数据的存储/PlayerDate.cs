
using System;
using System.Collections.Generic;
/// <summary>
/// 存储玩家与基地情况的信息
/// </summary>
[System.Serializable]
public class PlayerData
{
    public int currentLevel; // 当前关卡
    public int farthestLevel; // 最远解锁关卡
    public int coin=5000; // 当前金币数量
    public int PU = 200;//当前碳链分子,(聚合基元)数量
    public int progress;//玩家当前的进度，最大值为10000
    public int finishedMissionNum;//已完成的任务数量
    //public int taskDepartmentLevel=1; // 任务部门等级
    public int RefreshChance = 2; // 剩下的刷新次数
    public float musicVolume; // 音乐音量
    public float effectVolume; // 音效音量
    public string lastResetTime; // 上次重置的时间
    public string playerName;
    public bool AutoDo;
}
#region 成就科技类
/// <summary>
/// 存储成就解锁情况的信息
/// </summary>
[System.Serializable]
public class Achievement
{
    public int AchieveID; // 成就的唯一标识符1
    public string AchieveName; // 成就名称2
    public string Description; // 成就描述
    public string lockedInfo;//锁定描述   
    public bool IsUnlock; // 是否已解锁3
    public int hasCheck;//玩家是否已查看(int方便计数0已查看或未解锁，1未查看)4   
    public AciveType RewardType; // 成就奖励（奖励类型与奖励数量）
    public AciveType UnlockType ;//成就解锁所需要的数据种类，主要用于分拣
}
[System.Serializable]
public class AciveType
{
    public string type; // 解锁需要的数据种类，便于数据整理，比如当金币增加时，遍历type为coin的成就
    public int targetValue;//所要求的数量
}
[System.Serializable]
public class AchievementList
{
    public List<Achievement> Achievements;
}
[System.Serializable]
public class DialogueState
{
    public string stateName;
    public bool hasShown;
}
[System.Serializable]
public class DialogueStateList
{
    public List<DialogueState> dialogueStateList = new List<DialogueState>();
}
[System.Serializable]
public class Technology
{
    public int TechID;//科技的ID1
    public string TechName; // 科技的名称2
    public string TechCNName; // 科技的中文名称3
    public bool IsUnlock;   // 是否已解锁4
    //public string Prerequisites;  // 前置科技名称
    public string Description;  // 科技描述
    public string UnlockWay;  // 科技解锁方式的描述
    public int TechLevel;//科技的等级，控制UI界面科技的颜色
    public int hasCheck;//玩家是否已查看(int方便计数0已查看或未解锁，1未查看)5
}

[System.Serializable]
public class TechnologyList
{
    public List<Technology> Technologies = new List<Technology>();
}
#endregion
# region 建筑类
[System.Serializable]
public class BuildingData
{
    public int id;//1
    public string name;//2
    public string textName;//中文名3
    public string upgradeBy;//4由哪一个建筑升级而来，防止出现同一个建筑的不同等级出现在一起。这里的值是一级建筑的名字，如果该建筑是一级或者没有升级则为空。用于使已解锁的高级建筑替代低级建筑出现
    public string description;
    public int progressValue; // 解锁该建筑可获得的进度值，初始建筑默认为 0
    public int level;//建筑的等级，2.3级的建筑需要有upgradeBy值以使用位置值。
    public int type;//0指挥中心类，1工厂类，2进度推进类
    public int unlockReward;//解锁后的进度推进值
    public int income;//
    public string incomeType;//收入类型
    public int producInterval;//生产间隔
    public int maxReserves;//最大储量
    public int currentReserves;//当前储量
    public int hasCheck;//新解锁未查看的建筑5
    public string nextGenerateTime;//上次生产的时间  6
    public Position position;//7
    public Scale scale;//8
    public Rotation rotation;//9
    public bool isUnlocked;//true解锁false未解锁10
                           
    public bool hasNPC;//实验性功能
    public List<Position> npcPath;
}

[System.Serializable]
public class Position
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class Scale
{
    public float x=1;
    public float y=1;
    public float z=1;
}

[System.Serializable]
public class Rotation
{
    public float x=0;
    public float y=0;
    public float z=0;
}

[System.Serializable]
public class BuildingList
{
    public List<BuildingData> buildings;
}
#endregion
[System.Serializable]
public class LevelStarRecord
{
    public int levelNumber;
    public int starCount;
}

[System.Serializable]
public class LevelStarRecordList
{
    public List<LevelStarRecord> records = new List<LevelStarRecord>();
}

