using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class TechTreeManager 
{
    private static TechTreeManager Inst;
    private string techTreeFilePath = Application.persistentDataPath + "/TechnologyTree.json";
    private TechnologyList technologyList=null;
    public static TechTreeManager GetInst()
    {
        if (Inst == null)
        {
            Inst = new TechTreeManager();
        }
        return Inst;
    }
    public TechTreeManager()
    {
        Inst = this;
    }
    public void OnStart()
    {
        LoadTechTree();
    }
    /// <summary>
    /// 初始化并加载科技树
    /// </summary>
    public void LoadTechTree()
    {
        if (File.Exists(techTreeFilePath))
        {
            string json = File.ReadAllText(techTreeFilePath);
            technologyList = JsonUtility.FromJson<TechnologyList>(json);
            if (technologyList == null || technologyList.Technologies == null)
            {
                Debug.LogError("科技树列表为空，请检查存档文件或者默认科技树文件。");
                return;
            }
            if (technologyList.Technologies.Count == 0)
            {
                Debug.LogWarning("科技树内容为空。");
            }
            Debug.Log("科技树加载成功！");
        }
        else
        {
            Debug.LogWarning("科技树文件不存在，加载默认科技树...");
            // 从 Resources 文件夹加载默认科技树
            TextAsset resourcesTechTreeJson = Resources.Load<TextAsset>("TechTree");
            if (resourcesTechTreeJson != null)
            {
                technologyList = JsonUtility.FromJson<TechnologyList>(resourcesTechTreeJson.text);
                SaveTechTree();
                Debug.Log("默认科技树加载成功！");
            }
            else
            {
                Debug.LogError("无法加载默认科技树！");
                technologyList = new TechnologyList(); // 创建空的科技树以防报错
            }
        }
    }

    // 保存科技树
    public void SaveTechTree()
    {
        string json = JsonUtility.ToJson(technologyList, true);
        File.WriteAllText(techTreeFilePath, json);
        Debug.Log("科技树保存成功！");
    }

    /// <summary>
    /// 解锁科技时调用的函数
    /// </summary>
    /// <param name="techName"></param>
    public void UnlockTechnology(string techName)//解锁的时候还需要把hasChecked设置为1
    {
        Technology tech = technologyList.Technologies.Find(t => t.TechName == techName);
        if (tech != null && !tech.IsUnlock)
        {
            tech.IsUnlock = true;
            tech.hasCheck = 1;
            Debug.Log($"科技 {techName} 已解锁！");
            SaveTechTree();

            // 触发科技解锁事件（可选）
            //OnTechnologyUnlocked?.Invoke(techName);
        }
        else if (tech == null)
        {
            Debug.LogWarning($"未找到科技：{techName}");
        }
    }
    public void UnlockTechnology(int techID)//解锁的时候还需要把hasChecked设置为1
    {
        Technology tech = technologyList.Technologies.Find(t => t.TechID == techID);
        if (tech != null && !tech.IsUnlock)
        {
            tech.IsUnlock = true;
            tech.hasCheck = 1;
            Debug.Log($"科技 {techID} 已解锁！");
            SaveTechTree();
            AchievementManager.GetInst().CheckAchievProgress("tech");
            // 查看成就处科技类能否解锁
        }
        else if (tech == null)
        {
            Debug.LogWarning($"未找到科技：{techID}");
        }
    }

    /// <summary>
    /// 根据任务ID查询科技是否解锁
    /// </summary>
    /// <param name="techID">所查询的科技ID/param>
    /// <returns></returns>
    public static bool IsTechnologyUnlocked(int techID)
    {
        Technology tech = Inst.technologyList.Technologies.Find(t => t.TechID == techID);
        if (tech != null)
        {
            return tech.IsUnlock;
        }
        Debug.LogWarning($"未找到科技：{techID}");
        return false;
    }
    /// <summary>
    /// 获取未查看的新解锁科技
    /// </summary>
    /// <returns></returns>
    public int AmountUncheckedCount()
    {
        if (technologyList == null || technologyList.Technologies == null)
        {
            OnStart();
            Debug.Log("场景已切换，重新初始化数据");
        }
            
        return technologyList.Technologies.Sum(tech => tech.hasCheck);
    }
    /// <summary>
    /// 获取科技的进度值
    /// </summary>
    /// <returns></returns>
    public int TotalTechProgress()
    {
        if (technologyList == null || technologyList.Technologies == null)
        {
            OnStart();
            Debug.Log("场景已切换，重新初始化数据");
        }
        return technologyList.Technologies.Where(t => t.IsUnlock && t.TechLevel >= 4).Sum(t => t.TechLevel * 100);
    }

    /// <summary>
    /// 查看已解锁的科技数量
    /// </summary>
    /// <returns></returns>
    public int AmountUmlockCount()
    {
        return technologyList.Technologies.Count(t => t.IsUnlock);
    }
    /// <summary>
    /// 根据任务名称查询科技是否解锁
    /// </summary>
    /// <param name="techName">所查询的科技名称</param>
    /// <returns></returns>
    public static bool IsTechnologyUnlocked(string techName)
    {
        if(Inst.technologyList == null  )
        {
            Inst.LoadTechTree();
            Debug.LogWarning("科技树列表还不存在，先加载科技树，再查找科技");
        }

        Technology tech = Inst.technologyList.Technologies.Find(t => t.TechName == techName);
        if (tech != null)
        {
            return tech.IsUnlock;
        }
        Debug.LogWarning($"未找到科技：{techName}");
        return false;
    }
    /// <summary>
    /// 根据ID获取科技的信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Technology GetTech(int id)
    {
        Technology tech = Inst.technologyList.Technologies.Find(t => t.TechID == id);
        if (tech != null)
        {
            return tech;
        }
        Debug.Log("错误的ID编号"+id);
        return null;
    }
    /// <summary>
    /// 根据ID获取科技的信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Technology GetTech(string name)
    {
        Technology tech = Inst.technologyList.Technologies.Find(t => t.TechName == name);
        if (tech != null)
        {
            return tech;
        }
        Debug.Log("错误的科技名称"+name);
        return null;
    }
}
