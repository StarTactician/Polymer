using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueWindow : BasePanel
{
    private static string name = "DialogueWindow";
    private static string path = "Panel/DialogueWindow";
    public static readonly UIType uitype = new UIType(path, name);

    private Queue<DialogueLine> dialogueQueue = new Queue<DialogueLine>();
    private TextMeshProUGUI nameText;
    private TextMeshProUGUI dialogueText;
    private Image portraitImage;
    private Button continueButton;
    private Button skipButton;
    private Coroutine typingCoroutine;
    private string currentFullText = "";

    public DialogueWindow() : base(uitype) { }

    public override void OnStart()
    {
        base.OnStart();
        nameText = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "NameText");
        dialogueText = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "DialogueText");
        portraitImage = UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "Portrait");
        continueButton = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "ContinueButton");
        skipButton = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "SkipButton");

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        if (skipButton != null)
            skipButton.onClick.AddListener(() => UIManager.GetInst().PopWindow(this, true));
    }

    public override void OnEnable()
    {
        base.OnEnable();
        UIFunction.Inst.StartFade(0.2f, this, true);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        UIFunction.Inst.StartFade(0.2f, this, false);
    }

    public void LoadDialogue(string dialogueID)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("Dialogues/" + dialogueID);
        if (jsonFile == null)
        {
            Debug.LogError("未找到对话文件: " + dialogueID);
            return;
        }

        Dialogue dialogue = JsonUtility.FromJson<Dialogue>(jsonFile.text);

        dialogueQueue.Clear();
        foreach (DialogueLine line in dialogue.dialogueLines)
        {
            dialogueQueue.Enqueue(line);
        }

        DisplayNextSentence();
    }

    private void OnContinueClicked()
    {
        // 如果正在打字，先跳过打字，直接显示整句
        if (typingCoroutine != null)
        {
            UIFunction.Inst.StopCoroutine(typingCoroutine);
            dialogueText.text = currentFullText;
            typingCoroutine = null;
            return;
        }

        // 否则进入下一句
        DisplayNextSentence();
    }

    private void DisplayNextSentence()
    {
        if (dialogueQueue.Count == 0)
        {
            UIManager.GetInst().PopWindow(this, true);
            return;
        }

        DialogueLine currentLine = dialogueQueue.Dequeue();
        nameText.text = currentLine.speaker;
        currentFullText = currentLine.text;

        if (!string.IsNullOrEmpty(currentLine.portrait))
        {
            Sprite portraitSprite = Resources.Load<Sprite>("IMG/Character/" + currentLine.portrait);
            if (portraitSprite != null)
            {
                portraitImage.sprite = portraitSprite;
                if (currentLine.fadePortrait)
                    UIFunction.Inst.StartFade(0.2f, portraitImage, true);
            }
        }

        // 使用打字机效果显示文本
        if (typingCoroutine != null)
            UIFunction.Inst.StopCoroutine(typingCoroutine);

        typingCoroutine = UIFunction.Inst.StartTypewriter(dialogueText, currentFullText, 0.03f, UIFunction.Inst);
    }
}


