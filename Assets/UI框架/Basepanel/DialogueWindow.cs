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
    private string currentDialogueName;
    private bool isTyping = false;

    public DialogueWindow() : base(uitype) { }

    public override void OnStart()
    {
        base.OnStart();
        nameText = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "NameText");
        dialogueText = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "DialogueText");
        portraitImage = UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "Portrait");
        continueButton = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Continue");
        skipButton = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Skip");

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);

        if (skipButton != null)
            skipButton.onClick.AddListener(() =>
            {
                if (isTyping)
                {
                    UIFunction.Inst.StopCoroutine(typingCoroutine);
                    dialogueText.text = currentFullText;
                    typingCoroutine = null;
                    isTyping = false;
                }
                else
                {
                    UIManager.GetInst().PopWindow(this, true);
                }
            });
        UIManager.Pause(true);

    }

    public override void OnEnable()
    {
        base.OnEnable();
        UIFunction.Inst.StartFade(0.2f, this, true);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        DialogueManager.GetInst().OnDialogueFinished(currentDialogueName);
        UIManager.Pause(false);
    }

    public override void OnDestroy()
    {       
        base.OnDestroy();
        DialogueManager.GetInst().OnDialogueFinished(currentDialogueName);
        UIManager.Pause(false);
    }

    public void LoadDialogue(string dialogueName)
    {
        currentDialogueName = dialogueName;
        TextAsset jsonFile = Resources.Load<TextAsset>("Dialogues/" + dialogueName);
        if (jsonFile == null)
        {
            Debug.LogError("未找到对话文件: " + dialogueName);
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
        if (isTyping)
        {
            UIFunction.Inst.StopCoroutine(typingCoroutine);
            dialogueText.text = currentFullText;
            typingCoroutine = null;
            isTyping = false;
            return;
        }

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
            Sprite portraitSprite = Resources.Load<Sprite>("IMG/People/" + currentLine.portrait);
            if (portraitSprite != null)
            {
                portraitImage.sprite = portraitSprite;
                if (currentLine.fadePortrait)
                    UIFunction.Inst.StartFade(0.2f, portraitImage, true);
            }
        }

        if (typingCoroutine != null)
            UIFunction.Inst.StopCoroutine(typingCoroutine);

        isTyping = true;
        typingCoroutine = UIFunction.Inst.StartTypewriter(dialogueText, currentFullText, 0.03f, UIFunction.Inst, () =>
        {
            typingCoroutine = null;
            isTyping = false;
        });
    }
}


