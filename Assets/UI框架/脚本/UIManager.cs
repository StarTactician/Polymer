using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager
{
    /// <summary>
    /// 将BasePanel和他们对应的具体游戏物体一一对应起来的关键
    /// </summary>
    public Dictionary<string, GameObject> UI_Dict;
    public Dictionary<string, BasePanel> Windows_Dict; // 用于存储可以覆盖在其他面板之上的窗口
    /// <summary>
    /// 当前场景的UI栈，只用栈顶的UI元素会被显示,window将不会加入其中
    /// </summary>
    public Stack<BasePanel> UI_Stack;   
    public GameObject CurrentSceneCanvas;
    private GameObject tipsWindowContainer;
    private static UIManager Inst;
    public static bool IsPause { get; private set; }
    public static UIManager GetInst()
    {
        if (Inst == null)
        {
            Debug.LogError("未找到UIManager实例");
            return Inst;
        }
        return Inst;
    }
    public UIManager()
    {
        Inst = this;
        UI_Stack = new Stack<BasePanel>();
        UI_Dict = new Dictionary<string, GameObject>();
        Windows_Dict = new Dictionary<string, BasePanel>();
    }
    public static void Pause(bool pause)
    {
        IsPause = pause;
        Time.timeScale = pause ? 0f : 1f;
    }
    /// <summary>
    /// 在需要实例化对应信息的UI面板时被调用
    /// </summary>
    /// <param name="ui_info">传递UI界面的名字与存储路径的脚本</param>
    /// <returns></returns>
    private GameObject InstantiateUIObject(UIType ui_info,bool isWindow = false)//
    {
        if(!isWindow)
        {
            if (UI_Dict.ContainsKey(ui_info.Name))
            {
                return UI_Dict[ui_info.Name];
            }
            if (CurrentSceneCanvas == null)
                CurrentSceneCanvas = UIFunction.Inst.FindCanvas();
            //根据UIType的信息实例化对应的Panel.gameobject
            GameObject TheUI = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(ui_info.Path), CurrentSceneCanvas.transform);
            return TheUI;
        }
        else
        {
            GameObject TheWindow = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(ui_info.Path), CurrentSceneCanvas.transform);
            return TheWindow;
        }
    }
    /// <summary>
    /// 需要显示某个UI界面时调用
    /// </summary>
    /// <param name="basePanel"></param>
    /// <param name="isWindow">是不是窗口</param>
    public void Push(BasePanel basePanel, bool isWindow = false)
    {
        if (isWindow)// 如果是覆盖面板，直接添加到Windouws列表中并显示
        {   //更新记录：把basePanel.uiType.Name换成 basePanel.WindowObject.name防止同脚本撞车
            if (basePanel.PanelObj==null )//如果这个游戏物体的名字（不是basePanel的名字）不在这个字典里）则是首次推入这个窗口，直接实例化这个窗口
            {
                //|| !Windows_Dict.ContainsKey(basePanel.PanelObj.name
                GameObject ui_Obj = InstantiateUIObject(basePanel.uiType,true);
                basePanel.PanelObj = ui_Obj;//将窗口脚本与窗口游戏物体绑定
                //UI_Dict.Add(basePanel.WindowObject.name, ui_Obj);
                Windows_Dict.Add(basePanel.PanelObj.name, basePanel);
                basePanel.OnStart();
            }
            else//不是首次推入，则直接启用已经加入了的窗口
            {
                //basePanel.PanelObj = Windows_Dict[basePanel.PanelObj.name].PanelObj;
                basePanel.PanelObj.SetActive(true);
                basePanel.PanelObj.transform.SetAsLastSibling(); // 将窗口移动到层级最顶端
                basePanel.OnEnable();
            }
            
        }
        else
        {
            if (UI_Stack.Count > 0)//不止一个就禁用掉上一个
            {
                UI_Stack.Peek().OnDisable();
                UI_Dict[UI_Stack.Peek().uiType.Name].SetActive(false);
            }
            // 如果字典中没有该面板，则创建一个新的实例
            if (!UI_Dict.ContainsKey(basePanel.uiType.Name))
            {
                GameObject ui_Obj = InstantiateUIObject(basePanel.uiType);
                UI_Dict.Add(basePanel.uiType.Name, ui_Obj);
                basePanel.PanelObj = ui_Obj;
                basePanel.OnStart(); // 初始化面板
            }
            else
            {
                // 如果面板已存在，则获取它并启用
                basePanel.PanelObj = UI_Dict[basePanel.uiType.Name];
                basePanel.PanelObj.transform.SetAsLastSibling(); // 将UI移动到层级最顶端
                //UI_Stack.Push(basePanel);//待测试
                basePanel.PanelObj.SetActive(true);
                basePanel.OnEnable(); // 启用面板
            }
            // 将面板推入栈中
            if (UI_Stack.Count == 0 || UI_Stack.Peek().uiType.Name != basePanel.uiType.Name)
            {
                UI_Stack.Push(basePanel);
                Debug.Log(basePanel+"已入栈！");
            }
            
        }
    }
    /// <summary>
    /// 一段时间后会自动消失的窗口
    /// </summary>
    /// <param name="basePanel"></param>
    /// <param name="time">出现多长时间后消失，默认时长1.2s</param>
    public void PushTips(BasePanel basePanel,float time=1.2f)
    {
        if (tipsWindowContainer == null)
            tipsWindowContainer = InstantiateUIObject(TipsWindowContainer.uitype, true);
        tipsWindowContainer.transform.SetAsLastSibling();
        GameObject ui_Obj = InstantiateUIObject(basePanel.uiType,true);
        basePanel.PanelObj = ui_Obj;
        basePanel.OnStart();
        ui_Obj.transform.SetParent(tipsWindowContainer.transform, false);
        UIFunction.Inst.StartDelayMethod(basePanel.OnDestroy, time-0.4f);//执行完协程需要0.3s
        GameObject.Destroy(ui_Obj, time);        
    }
    /// <summary>
    /// 需要关闭当前UI界面时调用
    /// </summary>
    /// <param name="changeScene">是否切换场景，切换场景则关闭当前场景的所有UI</param>
    public void Pop(bool changeScene)//
    {
        if (changeScene)
        {
            // 销毁当前场景的所有面板
            while (UI_Stack.Count > 0)
            {
                BasePanel topPanel = UI_Stack.Peek();
                //topPanel.OnDisable();
                topPanel.OnDestroy();
                GameObject.Destroy(UI_Dict[topPanel.uiType.Name]);
                UI_Dict.Remove(topPanel.uiType.Name);
                UI_Stack.Pop();
                Debug.Log(topPanel + "已出栈！");
            }
            PopAllWindow();
        }
        else
        {
            if (UI_Stack.Count > 0)
            {
                BasePanel topPanel = UI_Stack.Peek();
                topPanel.OnDisable();
                UI_Dict[topPanel.uiType.Name].SetActive(false);
                UI_Stack.Pop();
                Debug.Log(topPanel + "已出栈！");
            }
            // 启用栈中的下一个面板
            if (UI_Stack.Count > 0)
            {
                BasePanel nextPanel = UI_Stack.Peek();
                nextPanel.PanelObj.SetActive(true);
                nextPanel.OnEnable();
            }
        }
    }
    /// <summary>
    /// 弹出一次性的面板，复用性低的，如得钱与花钱面板
    /// </summary>
    public void PopOnceOnlyUsePanel()
    {
        if (UI_Stack.Count > 0)
        {
            BasePanel topPanel = UI_Stack.Peek();
            topPanel.OnDestroy();
            GameObject.Destroy(UI_Dict[topPanel.uiType.Name]);
            UI_Dict.Remove(topPanel.uiType.Name);
            UI_Stack.Pop();
            Debug.Log(topPanel + "已出栈并被销毁！");
        }
        // 启用栈中的下一个面板
        if (UI_Stack.Count > 0)
        {
            BasePanel nextPanel = UI_Stack.Peek();
            nextPanel.PanelObj.SetActive(true);
            nextPanel.OnEnable();
        }
    }
    /// <summary>
    /// 关闭某个窗口的逻辑
    /// </summary>
    /// <param name="Window">具体窗口</param>
    /// <param name="isOnceOnlyUse">这个窗口是不是一次性的，比如显示特点数值的，以及复用性很低的</param>
    public void PopWindow(BasePanel Window,bool isOnceOnlyUse)
    {
        if (Windows_Dict.ContainsKey(Window.PanelObj.name))
        {
            if(isOnceOnlyUse)
            {
                //Window.OnDisable();
                Window.OnDestroy();
                GameObject.Destroy(Window.PanelObj);
                //UI_Dict.Remove(Window.uiType.Name);
                Windows_Dict.Remove(Window.PanelObj.name);
            }
            else
            {
                Window.OnDisable();
                Window.PanelObj.SetActive(false);
            }
        }
        else
            Debug.Log("Windows_List没有包含"+ Window);
    }
    public void PopAllWindow()
    {
        if (Windows_Dict.Count > 0) // 销毁所有窗口
        {
            List<BasePanel> windowsList = new List<BasePanel>(Windows_Dict.Values); // 复制字典的值到List
            foreach (BasePanel window in windowsList)
            {
                PopWindow(window, true);
            }
        }
    }
}
