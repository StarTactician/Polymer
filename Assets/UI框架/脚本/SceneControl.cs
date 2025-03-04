using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControl
{
    private static SceneControl Inst;
    public Dictionary<string, BaseScene> Scene_Dict;
    

    public static SceneControl GetInst()
    {        
        if (Inst == null)
        {           
            Debug.LogError("未找到SceneManager实例");
            return Inst;
        }
        //Debug.Log("SceneManager被实例化！");
        return Inst;
    }
    public SceneControl()
    {
        Inst = this;
        Scene_Dict = new Dictionary<string, BaseScene>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    /// <summary>
    /// 加载指定场景时调用，一个类型的人可以有很多，同一个类型的人总会有一个共同点
    /// </summary>
    /// <param name="scene_name">目标场景的名称（相当于具体人名）</param>
    /// <param name="baseScene">目标场景的BaseScene脚本（相当于一个类型） </param>
    public void LoadScene(string scene_name,BaseScene baseScene)
    {
        if(!Scene_Dict.ContainsKey(scene_name))
        {
            Scene_Dict.Add(scene_name, baseScene);
        }
        //在加载指定场景前先退出当前的场景
        if (Scene_Dict.ContainsKey(SceneManager.GetActiveScene().name))
        {
            Scene_Dict[SceneManager.GetActiveScene().name].ExitScene();
        }
        else
            Debug.LogWarning("Scene_Dict字典中不包含"+ SceneManager.GetActiveScene().name);
        UIManager.GetInst().Pop(true);
        SceneManager.LoadScene(scene_name);
    }
    /// <summary>
    /// 重开当前关卡时调用
    /// </summary>
    public void LoadScene()
    {
        UIManager.GetInst().Pop(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    /// <summary>
    /// 当场景加载完成后调用，确保只有在场景加载完全后再调用EnterScene方法，防止出现空引用
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Scene_Dict.ContainsKey(scene.name))
        {
            Scene_Dict[scene.name].EnterScene();
        }
        else
        {
            Debug.LogWarning("Scene_Dict字典中不包含" + scene.name);
        }
    }
    /// <summary>
    ///加载对应关卡，选关确认后，以及过关后确定进入下一关的时候被调用
    /// </summary>
    /// /// <param name="level">要加载的指定关卡</param>
    public void LoadLevel(int level)
    {
        //SceneControl.GetInst().LoadScene(level.ToString(), );
        BaseScene scene = LevelSceneFactory.CreateScene(level);
        if (scene != null)
        {
            LoadScene(level.ToString(), scene);
            Debug.Log(level.ToString()+ scene);
        }
            
        else
            Debug.LogError("场景加载失败，关卡编号无效: " + level);
    }
    // 取消订阅事件，防止内存泄漏
    public void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}

