using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AcivementSildeWindow : BasePanel
{
    private static string name = "AcivementSildeWindow";
    private static string path = "Panel/AcivementSildeWindow";
    public static readonly UIType uitype = new UIType(path, name);
    public TextMeshProUGUI NameText;
    public string AchiveName;
    public RectTransform SlideRect;  // 用于移动的目标对象
    public float SlideDistance = 1000f;  // 滑入滑出距离
    public float SlideDuration = 0.3f;  // 滑动时间
    public float StayDuration = 2f;     // 停留时间

    private Coroutine moveCoroutine;
    public AcivementSildeWindow() : base(uitype)
    {

    }
    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        UIFunction.Inst.StartFade(0.2f, this, false);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        UIFunction.Inst.StartFade(0.2f, this, true);
    }

    public override void OnStart()
    {
        base.OnStart();
        UIFunction.Inst.StartFade(0.2f, this, true);

        NameText = UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(PanelObj, "NameText");
        NameText.text = "获得成就 " + AchiveName;

        // 开始滑入协程（通过 UIFunction 实现）
        UIFunction.Inst.StartCoroutine(SlideSequence());
    }

    private IEnumerator SlideSequence()
    {
        // 滑入：从右向左
        yield return UIFunction.Inst.MoveObject(PanelObj, Vector2.left, SlideDistance, SlideDuration, this);

        // 停留
        yield return new WaitForSeconds(StayDuration);

        // 滑出：从左向右（回到初始位置）
        yield return UIFunction.Inst.MoveObject(PanelObj, Vector2.right, SlideDistance, SlideDuration, this);

        yield return new WaitForSeconds(0.5f);
        // 自动关闭窗口
        UIManager.GetInst().PopWindow(this, true);
    }



    /*void MoveToLeft(Button button)
    {
        moveCoroutine = UIFunction.Inst.MoveObject(button.gameObject, Vector2.left, 100, 0.2f, this);
    }
    void MoveToRight(Button button)
    {
        moveCoroutine = UIFunction.Inst.MoveObject(button.gameObject, Vector2.right, 100, 0.2f, this);
    }*/
}
