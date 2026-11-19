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
    private List<Achievement> PUType = new List<Achievement>();
    private List<Achievement> missionType = new List<Achievement>();
    private List<Achievement> levelType = new List<Achievement>();
    private List<Achievement> starType = new List<Achievement>();
    private List<Achievement> techType = new List<Achievement>();
    private List<Achievement> buildingType = new List<Achievement>();
    //private Dictionary<string, Achievement> singleType = new Dictionary<string, Achievement>();
    private string AchievementLibraryFilePath = Application.persistentDataPath + "/AchievementLibrary.json";
    private Queue<string> unlockAchievQueue = new Queue<string>();
    private bool isProcessingQueue = false;

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
        if (achievementList == null || achievementList.Achievements == null )
        {
            LoadAchievementLibrary();
            TypeAchiev();
            Debug.Log("场景已切换，重新初始化数据");
        }
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
            case "PU":
                achievementsToCheck = PUType;
                break;
            case "mainlineLevel":
                achievementsToCheck = levelType;
                break;
            case "star":
                achievementsToCheck = starType;
                break;
            case "tech":
                achievementsToCheck = techType;
                break;
            case "building":
                achievementsToCheck = buildingType;
                break;
            default:
                achievementsToCheck = null;
                break;
        }
        if (achievementsToCheck == null)
        {
            Debug.LogWarning($"⚠️ 没有找到 {type} 类型的成就");
            return;
        }
        Debug.Log("开始检查"+type+"类成就");
        foreach (Achievement achiev in achievementsToCheck)
        {

            if (!achiev.IsUnlock) // 只检查未解锁的成就
            {
                bool canUnlock = true; // 需要所有条件都满足
                if (achievementsToCheck == starType)
                {
                    int star = GetProgressByType("star", achiev.UnlockType.targetValue);//这里targetValue为目标关卡
                    Debug.Log("开始检查第" + achiev.UnlockType.targetValue + "关三星成就");
                    canUnlock = star == 3;
                }
                else if (achievementsToCheck == techType || achievementsToCheck == buildingType)
                {
                    canUnlock = GetProgressByType(achiev.UnlockType.type, achiev.UnlockType.targetValue) == 1;
                }
                else if (achievementsToCheck != starType|| achievementsToCheck != techType || achievementsToCheck != buildingType)
                {
                    int progress = GetProgressByType(achiev.UnlockType.type); // 获取当前进度
                    if (progress < achiev.UnlockType.targetValue)
                    {
                        Debug.Log("当前成就" + achiev.AchieveName + "的值还未达标，" + progress + "<" + achiev.UnlockType.targetValue);
                        canUnlock = false;
                        break;
                    }
                }
                
                

                if (canUnlock)
                {
                    Achievement unlockAchiev = achievementList.Achievements.Find(a => a.AchieveID == achiev.AchieveID);
                    if (unlockAchiev != null)
                    {
                        unlockAchiev.IsUnlock = true;
                        unlockAchiev.hasCheck = 1;//解锁的时候把hasChecked设置为1
                        SaveAchievementLibrary();
                        unlockAchievQueue.Enqueue(unlockAchiev.AchieveName);
                        /*AcivementSildeWindow sildeWindow = new AcivementSildeWindow();
                        sildeWindow.AchiveName = unlockAchiev.AchieveName;
                        UIManager.GetInst().Push(sildeWindow, true);*/
                        Debug.Log("🏆 解锁成就: " + unlockAchiev.AchieveName);//弹出成就解锁的window

                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ 成就 {achiev.AchieveName} 未找到");
                    }
                }
            }
            else Debug.Log(achiev.AchieveName+"已解锁");
        }
        if (!isProcessingQueue && unlockAchievQueue.Count > 0)
        {
            UIFunction.Inst.StartCoroutine(ProcessUnlockQueue());
        }
    }
    private IEnumerator ProcessUnlockQueue()
    {
        isProcessingQueue = true;

        while (unlockAchievQueue.Count > 0)
        {
            string achievName = unlockAchievQueue.Dequeue();

            AcivementSildeWindow sildeWindow = new AcivementSildeWindow();
            sildeWindow.AchiveName = achievName;
            UIManager.GetInst().Push(sildeWindow, true);

            // 等待弹窗生命周期结束再继续（比如弹窗总时长 2秒+0.2秒余量）
            yield return new WaitForSeconds(3.5f);
        }

        isProcessingQueue = false;
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
        PUType.Clear();
        levelType.Clear();
        starType.Clear();
        techType.Clear();
        buildingType.Clear();
        foreach (Achievement achiev in achievementList.Achievements)
        {
            string achievType = achiev.UnlockType.type; // 获取当前条件类型

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
            else if (achievType == "PU")
            {
                if (!missionType.Contains(achiev))
                {
                    PUType.Add(achiev);
                }
            }
            else if (achievType == "mainlineLevel")
            {
                if (!missionType.Contains(achiev))
                {
                    levelType.Add(achiev);
                }
            }
            else if (achievType == "star")
            {
                if (!starType.Contains(achiev))
                {
                    starType.Add(achiev);
                }
            }
            else if (achievType == "tech")
            {
                if (!techType.Contains(achiev))
                {
                    techType.Add(achiev);
                }
            }
            else if (achievType == "building")
            {
                if (!buildingType.Contains(achiev))
                {
                    buildingType.Add(achiev);
                }
            }
            else Debug.Log("未知的成就类型"+ achiev.UnlockType.type);
        }
        Debug.Log("成就分类完毕"+"金币类数量:"+coinType.Count+ "任务类数量:" + missionType.Count + "PU类数量:" + PUType.Count + "主线通关类数量:" + levelType.Count + "三星通关类数量:" + starType.Count);
    }

    /// <summary>
    /// 获取解锁类型的进度数据
    /// </summary>
    /// <param name="key"></param>

    private int GetProgressByType(string key,int level=0)
    {
        switch (key)
        {
            case "task": return SaveManager.CurrentFinishedTaskNum;//如果是任务型，则获取当前任务完成数量
            case "coin": return SaveManager.CurrentCoin;//如果是资源型，则获取当前资源数量
            case "PU": return SaveManager.CurrentPU;
            case "mainlineLevel": return SaveManager.LastesLevel-1;
            case "star": return SaveManager.GetInst().GetLevelStar(level);
            case "tech": return  TechTreeManager.IsTechnologyUnlocked(level)?1:0;
            case "building": return BuildingManager.GetInst().IsUnlock(level) ? 1 : 0;
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

