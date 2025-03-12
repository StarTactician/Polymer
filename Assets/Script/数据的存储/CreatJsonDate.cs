using System;
using System.Collections;
using UnityEngine;
using System.IO;
using UnityEditor;

public partial class SaveManager
{
    public void SavePlayerDate()
    {
        Debug.Log("正在保存玩家数据...");

        // 创建玩家数据对象
        PlayerData playerData = new PlayerData
        {
            currentLevel = CurrentLevel,
            farthestLevel = LastesLevel,
            coin = CurrentCoin,
            finishedMissionNum = CurrentFinishedTaskNum,
            taskDepartmentLevel = TaskDepartment,
            RefreshChance = SaveRefresh,
            lastResetTime = LastChangeTime,
            musicVolume = AudioManager.Inst.MusicVolum,
            effectVolume = AudioManager.Inst.EffectVolum
        };

        // 序列化为 JSON
        string json = JsonUtility.ToJson(playerData, true);

        // 写入文件
        string filePath = Path.Combine(Application.persistentDataPath, "PlayerData.json");
        File.WriteAllText(filePath, json);

        Debug.Log($"玩家数据已保存至 {filePath}");
    }
    public bool LoadPlayerDate()//bool值用于判断玩家是否第一次进入游戏
    {
        string filePath = Path.Combine(Application.persistentDataPath, "PlayerData.json");

        if (File.Exists(filePath))
        {
            Debug.Log("正在加载玩家数据...");

            // 读取 JSON
            string json = File.ReadAllText(filePath);

            // 反序列化
            PlayerData playerData = JsonUtility.FromJson<PlayerData>(json);

            // 同步数据
            CurrentLevel = playerData.currentLevel;
            LastesLevel = playerData.farthestLevel;
            CurrentCoin = playerData.coin;
            CurrentFinishedTaskNum = playerData.finishedMissionNum;
            TaskDepartment = playerData.taskDepartmentLevel;
            SaveRefresh = playerData.RefreshChance;
            LastChangeTime = playerData.lastResetTime;
            if (!AudioManager.Inst)
                Debug.Log("AudioManager不在");
            if(playerData==null)
                Debug.Log("playerData不在");
            AudioManager.Inst.MusicVolum = playerData.musicVolume;
            AudioManager.Inst.EffectVolum = playerData.effectVolume;

            Debug.Log("玩家数据加载完成！");
            return true;
        }
        else
        {
            Debug.LogWarning("玩家数据文件不存在，创建默认数据...");
            CurrentCoin = 2000;//开局默认两千
            SaveRefresh = 2;
            TaskDepartment = 1;
            SavePlayerDate(); // 如果没有文件，则创建默认数据文件
            return false;
        }
    }

    /*public bool LoadPlayerByJson()//读取JSON数据,在游戏进入时调用(需要LevelManager被实例化后使用)
    {
        if (File.Exists(Application.persistentDataPath + "/PlayerDate.json"))
        {
            StreamReader sr = new StreamReader(Application.persistentDataPath + "/PlayerDate.json");//从文件夹中的JsonDate中读取
            string JsonString = sr.ReadToEnd();
            sr.Close();
            PlayerDate save = JsonUtility.FromJson<PlayerDate>(JsonString);
            //读取所有被保存的数据,最远的关卡，科研完成情况，金币多少等
            LastesLevel = save.TheFarthestLevel;
            AudioManager.Inst.MusicVolum = save.MusicVolum;
            AudioManager.Inst.EffectVolum = save.EffectVolum;
            CurrentCoin = save.Coin;
            TaskDepartment = save.TaskDepartmentLevel;
            return true;
        }
        else
        {
            Debug.Log("读取Json失败");
            SavePlayerByJson();
            return false;
        }
    }
    
    public void SavePlayerByJson()//储存JSON数据，在过关（主线与无尽），完成委托结算资金与科技线后以及退出游戏时调用
    {

        Debug.Log("正在保存");
        PlayerDate save = CreateSavePlayerDate();
        string JsonString = JsonUtility.ToJson(save, true);
        StreamWriter sw = new StreamWriter(Application.persistentDataPath + "/PlayerDate.json");//把暂存数据写入
        sw.Write(JsonString);
        sw.Close();
    }

    PlayerDate CreateSavePlayerDate()//创建一个新的 JsonDate 对象,然后将当前游戏中的内容同步进JSON里
    {
        PlayerDate save = new PlayerDate();
        save.CurrentLevel = CurrentLevel;
        save.TheFarthestLevel = LastesLevel;
        save.MusicVolum = AudioManager.Inst.MusicVolum;
        save.EffectVolum = AudioManager.Inst.EffectVolum;
        save.Coin = CurrentCoin;
        save.TaskDepartmentLevel = TaskDepartment;
        return save;
    }*/
}

