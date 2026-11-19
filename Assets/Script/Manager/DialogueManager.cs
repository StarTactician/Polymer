using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class DialogueManager
{
    private static DialogueManager Inst;
    private Dictionary<string, Dialogue> dialogueMap = new Dictionary<string, Dialogue>();
    private DialogueStateList stateList;
    private string stateSavePath = Application.persistentDataPath + "/DialogueStates.json";


    public static DialogueManager GetInst()
    {
        if (Inst == null)
        {
            Debug.LogError("未找到DialogueManager实例");
            return Inst;
        }
        return Inst;
    }
    public DialogueManager()
    {
        Inst = this;
        LoadAllDialogues();
    }

    private void LoadAllDialogues()
    {
        if (System.IO.File.Exists(stateSavePath))
        {
            string json = System.IO.File.ReadAllText(stateSavePath);
            stateList = JsonUtility.FromJson<DialogueStateList>(json);
            Debug.Log("✅ 加载已存在的对话状态列表！");
        }
        else
        {
            Debug.Log("⚠️ 未找到对话状态存档，正在扫描 Resources/Dialogues/ 下的所有 JSON 文件");

            stateList = new DialogueStateList();

            // 加载 Resources/Dialogues 下的所有 TextAsset（注意：必须都在 Resources 文件夹中）
            TextAsset[] allDialogueFiles = Resources.LoadAll<TextAsset>("Dialogues");

            foreach (TextAsset file in allDialogueFiles)
            {
                Dialogue dialogue = JsonUtility.FromJson<Dialogue>(file.text);
                if (dialogue != null && !string.IsNullOrEmpty(dialogue.name))
                {
                    stateList.dialogueStateList.Add(new DialogueState
                    {
                        stateName = dialogue.name,
                        hasShown = false
                    });
                }
            }

            SaveStateList();
        }
    }
    public void TryTriggerDialogue(string dialogueName)
    {
        DialogueState state = stateList.dialogueStateList.Find(s => s.stateName == dialogueName);
        if (state == null)
        {
            Debug.LogWarning("⚠️ 未在状态表中找到该对话：" + dialogueName);
            return;
        }

        if (!state.hasShown)
        {
            StartDialogue(dialogueName); // 播放对话
            state.hasShown = true;
            SaveStateList();
        }
        else
        {
            Debug.Log("💬 对话已播放过：" + dialogueName);
        }
    }
    public void OnDialogueFinished(string dialogueName)
    {
        DialogueState state = stateList.dialogueStateList.Find(s => s.stateName == dialogueName);
        if (state != null && !state.hasShown)
        {
            state.hasShown = true;
            SaveStateList();
            Debug.Log("📌 已标记对话为已完成：" + dialogueName);
        }
    }


    public void StartDialogue(string Name)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Dialogues/" + Name);
        if (jsonFile == null)
        {
            Debug.LogWarning("未找到对话文件：" + Name);
            return;
        }

        //Dialogue dialogue = JsonUtility.FromJson<Dialogue>(jsonFile.text);
        DialogueWindow dialogueWindow = new DialogueWindow();
        UIManager.GetInst().Push(dialogueWindow, true);
        dialogueWindow.LoadDialogue(Name);
    }
    private void SaveStateList()
    {
        string json = JsonUtility.ToJson(stateList, true);
        System.IO.File.WriteAllText(stateSavePath, json);
    }

}
