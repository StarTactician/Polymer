using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class DialogueManager
{
    private Dictionary<string, Dialogue> dialogueMap = new Dictionary<string, Dialogue>();

    public DialogueManager()
    {
        LoadAllDialogues();
    }

    private void LoadAllDialogues()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Dialogues/AllDialogues");
        DialogueCollection collection = JsonUtility.FromJson<DialogueCollection>(jsonFile.text);

        foreach (Dialogue dialogue in collection.dialogues)
        {
            dialogueMap[dialogue.name] = dialogue;
        }
    }

    public void StartDialogue(string id)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Dialogues/" + id);
        if (jsonFile == null)
        {
            Debug.LogWarning("未找到对话文件：" + id);
            return;
        }

        Dialogue dialogue = JsonUtility.FromJson<Dialogue>(jsonFile.text);
        DialogueWindow dialogueWindow = new DialogueWindow();
        UIManager.GetInst().Push(dialogueWindow, true);
        dialogueWindow.LoadDialogue(id);
    }
}
