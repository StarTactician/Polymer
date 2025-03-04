using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // 负责按下ESC后弹出的选项画面等，这个主线和无尽是通用的，所以单独拿出来
    public GameObject PauseUI;
    public static bool IsPause;//为true时，玩家将无法执行操作
    public GameObject SettingImg;//设置界面
    private void Start()
    {
        Time.timeScale = 1;
    }


}
