[System.Serializable]
public class Dialogue
{
    public string name; // 本段对话的唯一ID，比如“IntroFirstTime”
    public DialogueLine[] dialogueLines; // 每句对话内容
}

[System.Serializable]
public class DialogueLine
{
    public string speaker;        // 说话人名称
    public string text;           // 台词正文
    public string portrait;       // 使用的立绘资源名（可为空）
    public bool fadePortrait = true; // 是否对立绘使用淡入动画
}


[System.Serializable]
public class DialogueCollection
{
    public Dialogue[] dialogues;
}
