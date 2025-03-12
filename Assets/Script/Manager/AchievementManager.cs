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
    //private Dictionary<string, Achievement> singleType = new Dictionary<string, Achievement>();
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
        Debug.Log("成就数量："+GetAchivesNum());
    }
    public int GetAchivesNum()
    {
        return achievementList.Achievements.Count();
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
            if (achievementList == null || achievementList.Achievements == null)
            {
                Debug.LogError("JSON 解析失败：成就列表为空。");
                return;
            }
            if(achievementList.Achievements.Count==0)
            {
                Debug.LogWarning("成就库为空。");
            }
            Debug.Log("成就库加载完成！");
        }
        else
        {
            Debug.LogWarning("成就库文件不存在，正在加载初始成就库...");
            TextAsset resourcesAchiveLibrary = Resources.Load<TextAsset>("AchievementLibrary");
            
            if (resourcesAchiveLibrary != null)
            {
                achievementList = JsonUtility.FromJson<AchievementList>(resourcesAchiveLibrary.text);
                SaveAchievementLibrary(); // 保存默认成就库到目标路径
                Debug.Log("初始成就库已保存！");
            }
            else
            {
                Debug.LogError("无法加载默认科技树！");
                achievementList = new AchievementList(); // 创建空的成就库以防报错
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
        Achievement achiev = Inst.achievementList.Achievements.Find(t => t.AchieveID == id);//实际对象引用
        if (achiev != null)
        {
            return achiev;
        }
        Debug.Log("错误的ID编号" + id);
        return null;
    }
    public int AmountUncheckedCount()
    {
        return achievementList.Achievements.Sum(tech => tech.hasCheck);
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
            if (!achiev.IsUnlock) // 只检查未解锁的成就
            {
                bool canUnlock = true; // 需要所有条件都满足

                foreach (AciveType condition in achiev.UnlockType)
                {
                    int progress = GetProgressByType(condition.type); // 获取当前进度
                    if (progress < condition.targetValue)
                    {
                        canUnlock = false; // 只要有一个条件不满足，就不能解锁
                        break;
                    }
                }

                if (canUnlock)
                {
                    Achievement unlockAchiev = achievementList.Achievements.Find(a => a.AchieveID == achiev.AchieveID);
                    if(unlockAchiev!=null)
                    {
                        unlockAchiev.IsUnlock =true;
                        unlockAchiev.hasCheck = 1;//解锁的时候把hasChecked设置为1
                        Debug.Log("🏆 解锁成就: "+ unlockAchiev.AchieveName);//弹出成就解锁的window
                        SaveAchievementLibrary();
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ 成就 {achiev.AchieveName} 未找到");
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
    public static bool IsAchiveUnlocked(int techID)
    {
        Achievement achievement= Inst.achievementList.Achievements.Find(t => t.AchieveID == techID);
        if (achievement != null)
        {
            return achievement.IsUnlock;
        }
        Debug.LogWarning($"未找到科技：{techID}");
        return false;
    }
    /// <summary>
    /// 将成就按解锁所需来进行分类
    /// </summary>
    private void TypeAchiev()
    {
        coinType.Clear();
        missionType.Clear();

        foreach (Achievement achiev in achievementList.Achievements)
        {
            foreach (AciveType condition in achiev.UnlockType) // 遍历所有解锁条件
            {
                string achievType = condition.type; // 获取当前条件类型

                if (achievType == "coin")
                {
                    if (!coinType.Contains(achiev))
                    {
                        coinType.Add(achiev);
                    }
                }
                else if (achievType == "task")
                {
                    if (!missionType.Contains(achiev))
                    {
                        missionType.Add(achiev);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 获取解锁类型的进度数据
    /// </summary>
    /// <param name="key"></param>

    private int GetProgressByType(string key)
    {
        switch (key)
        {
            case "task": return SaveManager.CurrentFinishedTaskNum;//如果是任务型，则获取当前任务完成数量
            case "coin": return SaveManager.CurrentCoin;//如果是资源型，则获取当前资源数量
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

