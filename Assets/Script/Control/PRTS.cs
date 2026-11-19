using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PRTS
{
    private static PRTS Inst;
    public static PRTS GetInst()
    {
        if (Inst == null)
        {
            Inst = new PRTS();
            Inst.LoadFromFile();
        }
        return Inst;
    }

    private Dictionary<int, PRTSDate> prtsDataDict = new Dictionary<int, PRTSDate>();
    private string savePath => Application.persistentDataPath + "/PRTSData.json";

    // 用于记录当前关卡的操作数据
    private List<PosForceDate> currentSessionSteps = new List<PosForceDate>();//当前的非记录在JSON里面的数据
    public List<PosForceDate> GetCurrentSessionSteps() => currentSessionSteps;

    public Dictionary<int, PRTSDate> GetPRTSData() => prtsDataDict;

    public void ResetSession()
    {
        currentSessionSteps.Clear();
        Debug.Log("PRTS: 当前操作记录已清空");
    }

    public void RemberPos(Vector3 pos, float radius,int Type)
    {
        
            PosForceDate step = new PosForceDate
            {
                type = Type,
                x = pos.x,
                y = pos.y,
                Force = radius
            };
            currentSessionSteps.Add(step);
            Debug.Log($"PRTS记录第{currentSessionSteps.Count}步：位置({pos.x}, {pos.y})，力度{radius}");
        
    }


    // 当通关时调用：检查是否有记录，或是否需要提示覆盖
    public void OnVictoryCheckOrPrompt(int level)
    {
        if (!prtsDataDict.ContainsKey(level))
        {
            prtsDataDict[level] = new PRTSDate
            {
                Level = level,
                StepList = new List<PosForceDate>(currentSessionSteps)
            };
            SaveToFile();
            Debug.Log($"PRTS首次保存关卡 {level} 操作，共 {currentSessionSteps.Count} 步");
        }
        else
        {

            // 弹窗提示是否覆盖（调用UIManager）
            OverridePRTSPanel panel = new OverridePRTSPanel();
            panel.Setup(level, new List<PosForceDate>(currentSessionSteps));
            UIManager.GetInst().Push(panel);
        }
    }

    public void OverwriteLevelSteps(int level, List<PosForceDate> steps)
    {
        if (steps == null || steps.Count == 0)
        {
            Debug.Log("步骤数据为空，覆盖失败");
            return;
        }
        prtsDataDict[level] = new PRTSDate
        {
            Level = level,
            StepList = new List<PosForceDate>(steps)
        };
        Debug.Log($"PRTS创建了新的关卡 {level} 数据并保存");
        SaveToFile();
    }



    private void SaveToFile()
    {
        PRTSDateList list = new PRTSDateList();
        list.LevelPosDate = new List<PRTSDate>(prtsDataDict.Values);
        string json = JsonUtility.ToJson(list, true);
        File.WriteAllText(savePath, json);
        Debug.Log("PRTS数据保存成功");
    }

    private void LoadFromFile()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            PRTSDateList list = JsonUtility.FromJson<PRTSDateList>(json);
            prtsDataDict.Clear();
            foreach (var entry in list.LevelPosDate)
            {
                prtsDataDict[entry.Level] = entry;
            }
            Debug.Log("读取PRTS成功，共有记录关卡: " + prtsDataDict.Count);
        }
    }
}

[System.Serializable]
public class PRTSDateList
{
    public List<PRTSDate> LevelPosDate = new List<PRTSDate>();
}

[System.Serializable]
public class PRTSDate
{
    public int Level;
    public List<PosForceDate> StepList = new List<PosForceDate>();
}

[System.Serializable]
public class PosForceDate
{
    public int type;
    public float x;
    public float y;
    public float Force;
}
