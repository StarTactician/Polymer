using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class AchievementManager
{
    private static AchievementManager Inst;
    private AchievementList achievementList;
    private List<Achievement> coinType = new List<Achievement>();
    private List<Achievement> missionType = new List<Achievement>();
    private Dictionary<string, Achievement> singleType = new Dictionary<string, Achievement>();
    private string AchievementLibraryFilePath = Application.persistentDataPath + "/AchievementLibrary.json";

    public static AchievementManager GetInst()
    {
        if (Inst == null)
        {
            Inst = new AchievementManager();
        }
        return Inst;
    }
    public AchievementManager()
    {
        Inst = this;
        LoadAchievementLibrary();
        TypeAchiev();
    }
    /// <summary>
    /// 读取成就库的逻辑
    /// </summary>
    private void LoadAchievementLibrary()
    {
        if (File.Exists(AchievementLibraryFilePath))
        {
            string json = File.ReadAllText(AchievementLibraryFilePath);
            achievementList = JsonUtility.FromJson<AchievementList>(json);
            if (achievementList == null || achievementList.achievements == null)
            {
                Debug.LogError("JSON 解析失败：成就列表为空。");
                return;
            }
            if(achievementList.achievements.Count==0)
            {
                Debug.LogWarning("成就库为空。");
            }
            Debug.Log("成就库加载完成！");
        }
        else
        {
            Debug.LogWarning("成就库文件不存在，正在加载初始成就库...");
            TextAsset defaultLibrary = Resources.Load<TextAsset>("AchievementLibrary");
            achievementList = JsonUtility.FromJson<AchievementList>(defaultLibrary.text);
            if (achievementList != null && achievementList.achievements != null)
            {
                SaveAchievementLibrary(); // 保存默认成就库到目标路径
                Debug.Log("初始成就库已保存！");
            }
        }
    }
    /// <summary>
    /// 保存成就库，也就是保存成就完成情况的逻辑
    /// </summary>
    public void SaveAchievementLibrary()
    {
        string json = JsonUtility.ToJson(achievementList, true);
        File.WriteAllText(AchievementLibraryFilePath, json);
        Debug.Log("成就库保存成功！");
    }

    /// <summary>
    /// 根据ID获取科技的信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Achievement GetAchiev(int id)
    {
        Achievement achiev = Inst.achievementList.achievements.Find(t => t.id == id);//实际对象引用，学到了
        if (achiev != null)
        {
            return achiev;
        }
        Debug.Log("错误的ID编号" + id);
        return null;
    }
    public int AmountUncheckedCount()
    {
        return achievementList.achievements.Sum(tech => tech.hasCheck);
    }
    /// <summary>
    /// 按照成就解锁所需数据种类来查看当前类型中是否有成就可以解锁，如果可以，解锁成就并保存
    /// </summary>
    /// <param name="type"></param>
    public void CheckAchievProgress(string type)//解锁的时候还需要把hasChecked设置为1
    {
        List<Achievement> achievementsToCheck=null ;
        switch (type)
        {
            case "coin":
                achievementsToCheck = coinType;
                break;
            case "task":
                achievementsToCheck = missionType;
                break;
            default:
                
                break;
        }
        if (achievementsToCheck == null)
        {
            Debug.LogWarning($"⚠️ 没有找到 {type} 类型的成就");
            return;
        }
        foreach (Achievement achiev in achievementsToCheck)
        {
            if (!achiev.isUnlocked) // 只检查未解锁的成就
            {
                bool canUnlock = true; // 需要所有条件都满足

                foreach (AchievementCondition condition in achiev.conditions)
                {
                    int progress = GetProgressByType(condition.key); // 获取当前进度
                    if (progress < condition.targetValue)
                    {
                        canUnlock = false; // 只要有一个条件不满足，就不能解锁
                        break;
                    }
                }

                if (canUnlock)
                {
                    Achievement unlockAchiev = achievementList.achievements.Find(a => a.id == achiev.id);
                    if(unlockAchiev!=null)
                    {
                        unlockAchiev.isUnlocked =true;
                        unlockAchiev.hasCheck = 1;//解锁的时候把hasChecked设置为1
                        Debug.Log("🏆 解锁成就: "+ unlockAchiev.name);
                        SaveAchievementLibrary();
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ 成就 {achiev.name} 未找到");
                    }
                    //UnlockAchievement(achiev);
                }
            }
        }
        

    }
    /// <summary>
    /// 根据成就ID查询成就是否解锁
    /// </summary>
    /// <param name="techID">所查询的科技ID/param>
    /// <returns></returns>
    public static bool IsTechnologyUnlocked(int techID)
    {
        Achievement achievement= Inst.achievementList.achievements.Find(t => t.id == techID);
        if (achievement != null)
        {
            return achievement.isUnlocked;
        }
        Debug.LogWarning($"未找到科技：{techID}");
        return false;
    }
    /// <summary>
    /// 将成就按解锁所需来进行分类
    /// </summary>
    private void TypeAchiev()
    {
        coinType.Clear(); // 清空List和字典，防止重复存储（尽管在初始化时被调用）
        foreach (Achievement achiev in achievementList.achievements)
        {
            string achievType = achiev.type.type; // 获取成就的解锁数据类型
            switch (achievType)
            {
                case ("coin"):
                    // 存入List
                    coinType.Add(achiev);
                    break;
                case ("task"):
                    missionType.Add(achiev);
                    break;
                default:
                    //存入字典
                    singleType.Add(achievType, achiev);
                    Debug.Log("未规范使用的成就解锁条件，请确保是否存在拼写错误或是特殊条件类型");
                    break;
            }
        }

    }
    /// <summary>
    /// 获取对应类型的具体数据
    /// </summary>
    /// <param name="key"></param>

    private int GetProgressByType(string key)
    {
        switch (key)
        {
            case "task": return SaveManager.CurrentFinishedTaskNum;
            case "coin": return SaveManager.CurrentCoin;
            default:
                Debug.LogWarning($"未知的成就条件: {key}");
                return 0;
        }
    }
    /*
    public void UpdateAchievementProgress(AchievementList achievementLibrary, string conditionKey, int increment)
    {
        foreach (Achievement achievement in achievementLibrary.achievements)
        {
            if (achievement.isUnlocked)
                continue;

            foreach (AchievementCondition condition in achievement.conditions)
            {
                if (condition.key == conditionKey)
                {
                    condition.currentValue += increment;
                    if (condition.currentValue >= condition.targetValue)
                    {
                        condition.currentValue = condition.targetValue;
                        achievement.isUnlocked = true;
                        Debug.Log($"成就解锁：{achievement.name}");
                        //成就是否有奖励还有待考量  
                    }
                }
            }
        }
    }
    */
}

