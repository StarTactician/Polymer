using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 左上角的暂停按钮
/// </summary>
public class PauseButtonPanel : BasePanel
{
    private static string name = "PauseButtonPanel";
    private static string path = "Panel/PauseButtonPanel";
    public static readonly UIType uitype = new UIType(path, name);
    private Image image;
    private bool showImage=false;
    public PauseButtonPanel() : base(uitype)
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
        image = UIFunction.Inst.GetCompoentInObjsChild<Image>(PanelObj, "Image");
        Sprite sprite= Resources.Load<Sprite>("IMG/PolymerImg/" + SaveManager.CurrentLevel);
        if (sprite != null)
            image.sprite = sprite;
        else
        {
            Debug.Log("第" + SaveManager.CurrentLevel + "关的分子图标还没有创建");
            image.sprite = Resources.Load<Sprite>("IMG/LittleUI/NotFound");
        }
            
        UIFunction.Inst.SetCanvasGroupVise(image.gameObject,false);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Pause").onClick.AddListener(Pause);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Help").onClick.AddListener(Help);
    }

    void Pause()
    {
        UIManager.Pause(true);
        UIManager.GetInst().Push(new PausePanel());
    }
    void Help()
    {
        showImage = !showImage;
        if(showImage)
            UIFunction.Inst.SetCanvasGroupVise(image.gameObject, true);
        else
            UIFunction.Inst.SetCanvasGroupVise(image.gameObject, false);
    }

}
