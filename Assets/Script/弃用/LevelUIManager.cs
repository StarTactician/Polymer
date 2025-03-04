using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class LevelUIManager : MonoBehaviour
{
    // 负责在主线关卡中的UI互动，如更换电场正负，道具的使用，步数的显示，结算画面的管理等（不负责按下ESC后弹出的界面）
    public static LevelUIManager levelUIInst;
    public Text StepSaver;
    private MenuManager menu;
    public RawImage Target;//当前需要合成的分子式图片
    public GameObject[] Star;
    public GameObject VectoryUI;
    public GameObject FailedUI;
    //private int finalStep;

    private void Awake()
    {
        if (levelUIInst != null)
        {
            Destroy(gameObject);
        }
        levelUIInst = this;
        menu = GetComponent<MenuManager>();
        menu.PauseUI.SetActive(false);
    }
    

    public void updateStep(int step)
    {
        //finalStep = step;
        StepSaver.text = ("当前步数:"+step);
    }
    public void VectorySettlement(int starnum)//胜利结算时调用
    {
        MenuManager.IsPause = true;
        VectoryUI.SetActive(true);
        switch(starnum)
        {
            case (2):
                Star[0].SetActive(true);
                Star[1].SetActive(true);
                Star[2].SetActive(true);
                break;
            case (1):
                Star[0].SetActive(true);
                Star[1].SetActive(true);
                break;
            case (0):
                Star[0].SetActive(true);
                break;
            default:
                break;
        }                     
    }
}
