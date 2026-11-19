using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : BasePanel
{
    private static string name = "SettingPanel";
    private static string path = "Panel/SettingPanel";
    public static readonly UIType uitype = new UIType(path, name);
    Slider musicSlider;
    Slider effectSlider;
    Button AutoDO;
    GameObject Go;
    public SettingPanel() : base(uitype)
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
        musicSlider = UIFunction.Inst.GetCompoentInObjsChild<Slider>(PanelObj, "MusicVolumSetting");
        effectSlider = UIFunction.Inst.GetCompoentInObjsChild<Slider>(PanelObj, "EffectVolumSetting");
        AutoDO = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "AutoDO");
        Go = UIFunction.Inst.GetChild(AutoDO.gameObject, "go");
        musicSlider.value = AudioManager.Inst.MusicVolum;
        effectSlider.value = AudioManager.Inst.EffectVolum;
        
        changeGo();
    }

    public override void OnStart()
    {
        base.OnStart();        
        UIFunction.Inst.StartFade(0.2f, this, true);
        UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "Back").onClick.AddListener(Back);
        musicSlider = UIFunction.Inst.GetCompoentInObjsChild<Slider>(PanelObj, "MusicVolumSetting");
        effectSlider = UIFunction.Inst.GetCompoentInObjsChild<Slider>(PanelObj, "EffectVolumSetting");
        musicSlider.onValueChanged.AddListener(SetMusicVolum);
        effectSlider.onValueChanged.AddListener(SetEffectVolum);

        AutoDO = UIFunction.Inst.GetCompoentInObjsChild<Button>(PanelObj, "AutoDO");
        Go = UIFunction.Inst.GetChild(AutoDO.gameObject, "go");
        //同步滚动条音量值
        musicSlider.value = AudioManager.Inst.MusicVolum;
        effectSlider.value = AudioManager.Inst.EffectVolum;
        AutoDO.onClick.AddListener(AutoDo);
        changeGo();
    }
    void AutoDo()
    {
        SaveManager.GetInst().AutoDO = !SaveManager.GetInst().AutoDO;
        changeGo();
    }
    void changeGo()
    {
        UIFunction.Inst.SetCanvasGroupVise(Go, SaveManager.GetInst().AutoDO);
    }
    void Back()
    {
        UIManager.GetInst().Pop(false);
        SaveManager.GetInst().SavePlayerDate();
    }
    void SetMusicVolum(float volum)
    {
        AudioManager.Inst.SetMusicVolume(volum);
    }
    void SetEffectVolum(float volum)
    {
        AudioManager.Inst.SetEffectVolume(volum);
    }
}
