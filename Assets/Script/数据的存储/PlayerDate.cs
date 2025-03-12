
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
    public int coin=2000; // 当前金币数量
    public int PU = 0;//当前碳链分子,(聚合基元)数量
    public int progress;//玩家当前的进度
    public int finishedMissionNum;//已完成的任务数量
    public int taskDepartmentLevel=1; // 任务部门等级
    public int RefreshChance = 2; // 剩下的刷新次数
    public float musicVolume; // 音乐音量
    public float effectVolume; // 音效音量
    public string lastResetTime; // 上次重置的时间
    public string playerName;
}
#region 成就科技类
/// <summary>
/// 存储成就解锁情况的信息
/// </summary>
[System.Serializable]
public class Achievement
{
    public int AchieveID; // 成就的唯一标识符
    public string AchieveName; // 成就名称
    public string Description; // 成就描述
    public string lockedInfo;//锁定描述   
    public bool IsUnlock; // 是否已解锁
    public int hasCheck;//玩家是否已查看(int方便计数0已查看或未解锁，1未查看)   
    public AciveType RewardType; // 成就奖励（奖励类型与奖励数量）
    public List<AciveType> UnlockType =new List<AciveType>();//成就解锁所需要的数据种类，主要用于分拣
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
public class Technology
{
    public int TechID;//科技的ID
    public string TechName; // 科技的名称
    public string TechCNName; // 科技的中文名称
    public bool IsUnlock;   // 是否已解锁
    public string Prerequisites;  // 前置科技名称
    public string Description;  // 科技描述
    public string UnlockWay;  // 科技解锁方式的描述
    public int TechLevel;//科技的等级，控制UI界面科技的颜色
    public int hasCheck;//玩家是否已查看(int方便计数0已查看或未解锁，1未查看)
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
    public int id;
    public int income;//收入大于0则是生产类建筑
    public string incomeType;//收入类型
    public string name;
    public Position position;
    public Scale scale;
    public Rotation rotation;
    public bool isUnlocked;   
    public bool hasNPC;
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
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class Rotation
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class BuildingList
{
    public List<BuildingData> buildings;
}
#endregion
