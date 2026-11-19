using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Touched : MonoBehaviour
{// 负责主线模式中各种操作实现的，比如使用道具，切换道具等
    [Tooltip("匀强电场的正负,1为正，-1为负,默认为1")]
    public int Type = 1;
    public int Step;
    [Tooltip("按下按键时生成的圆圈")]
    public GameObject Circle;
    [SerializeField]
    [Tooltip("最大能影响的半径")]
    private float maxRadius;
    [SerializeField]
    [Tooltip("圈刚生成时的半径")]
    private float minRadius;
    [SerializeField]
    [Tooltip("圈扩大到最大所需时间")]
    private float needTime;
    [SerializeField]
    [Tooltip("弹开的力")]
    private float bounceForce;
    private float radius;
    private float chargeRate;
    private float expandTime;
    [SerializeField]
    private bool onTouching;
    public int TwoStarStep;//达成二星所要求不高于的步数
    public int ThreeStarStep;//达成三星所要求不高于的步数
    [Tooltip("每关的答案，由Touched提供（节点，边）")]
    public List<int> Answear;
    private Vector3 touchPosition;
    public List<Charge> affectedCharges = new List<Charge>();
    private GetTouchedVector getTouchedVector;

    public bool HasChangeCircle;
    public bool ISONBUTTON;

    private Item draggingItem = null;            // 当前正在拖的道具
    private Vector3 dragOffset = Vector3.zero;   // 防止道具“跳到手指中心”
    private Vector3 dragStartPos = Vector3.zero; // 未命中目标时可回退
    [SerializeField]
    [Tooltip("道具放下时，检测目标电荷的半径")]
    private float itemDropDetectRadius = 0.25f;
    public GameObject Red;
    public GameObject Blue;
    //
    public bool inAssistMode = false;
    private int assistStepIndex = 0;
    private List<PosForceDate> currentAssistSteps;//记忆在JSON的数据
    public List<PosForceDate> currentSessionSteps = new List<PosForceDate>(); // 本次记录数据
    private Coroutine releaseAnimRoutine = null;
    SpriteRenderer sr;
    private Coroutine resetColorCoroutine;
    //
    private void Awake()
    {
        LevelManager.GetInst().touched = this;
    }
    private void Start()
    {
        LevelManager.GetInst().touched = this;
        getTouchedVector = GameRoot.GetInst().GetTouchedVector_Root;
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(UIFunction.Inst.FindCanvas("UIScreen"), "StepText").text = "当前步数: 0";

        UIManager.Pause(false);
        var prtsList = PRTS.GetInst().GetPRTSData();
        if (SaveManager.GetInst().AutoDO && prtsList.ContainsKey(SaveManager.CurrentLevel))
        {
            inAssistMode = true;
            assistStepIndex = 0;
            currentAssistSteps = prtsList[SaveManager.CurrentLevel].StepList;
        }
    }
    private Vector3 GetInputScreenPos()
    {
        if (Input.touchCount > 0) return Input.GetTouch(0).position;
        return Input.mousePosition;
    }

    void Update()
    {
        if (UIManager.IsPause)
            return;

        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            ISONBUTTON = true;
            return;
        }
        else ISONBUTTON = false;
        if (getTouchedVector.IsDraggingItem)
        {
            HandleItemDragging();
            return;
        }
        Vector3 screenPos = GetInputScreenPos();

        bool isBegin =
            Input.GetMouseButtonDown(0) ||
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

        bool isHold =
            (Input.GetMouseButton(0) && onTouching) ||
            (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary));

        bool isEnd =
            Input.GetMouseButtonUp(0) ||
            (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled));

        if (isBegin)
        {
            // [新增] 自适应：优先Item，否则背景
            GameObject hitItemObj;
            Vector3 hitWorldPos;
            bool hitItem = getTouchedVector.AutoDetectBegin(screenPos, out hitItemObj, out hitWorldPos);

            if (hitItem)
            {
                // [新增] 进入拖道具状态（不触发OnTouchBegin）
                BeginDragItem(hitItemObj, hitWorldPos);
                return;
            }

            // [修改] 未命中Item → 原版逻辑
            touchPosition = hitWorldPos; // 用AutoDetectBegin返回的世界坐标
            OnTouchBegin();
        }
        else if (isHold)
        {
            touchPosition = getTouchedVector.GetTouchedPosition(screenPos);
            OnTouching();
        }
        else if (isEnd)
        {
            OnTouchEnd();
        }
    }

    private void BeginDragItem(GameObject itemObj, Vector3 hitWorldPos)
    {
        Item item = itemObj.GetComponent<Item>();
        if (item == null || !item.CanUse())
        {
            getTouchedVector.ChangeDraggingState(false);
            return;
        }

        draggingItem = item;
        dragStartPos = draggingItem.transform.position;

        // 用背景坐标做拖动平面：避免道具跟手时Z乱跳
        Vector3 planePos = getTouchedVector.GetTouchedPosition(GetInputScreenPos());

        dragOffset = draggingItem.transform.position - planePos;

        // [新增] 切换拖动状态
        getTouchedVector.ChangeDraggingState(true);

        // [新增] 避免原版电场正在触摸中
        onTouching = false;
        Circle.SetActive(false);

        draggingItem.OnDragBegin();
    }

    // =========================
    // [新增] 拖道具每帧执行
    // =========================
    private void HandleItemDragging()
    {
        if (draggingItem == null)
        {
            getTouchedVector.ChangeDraggingState(false);
            return;
        }

        Vector3 screenPos = GetInputScreenPos();
        Vector3 planePos = getTouchedVector.GetTouchedPosition(screenPos);

        // 1) 跟随移动
        draggingItem.transform.position = planePos + dragOffset;
        draggingItem.OnDragging();

        // 2) 抬手：尝试施加到Charge上
        bool isEnd =
            Input.GetMouseButtonUp(0) ||
            (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled));

        if (!isEnd) return;

        bool applied = TryApplyItemToCharge(draggingItem);

        if (!applied)
        {
            // [新增] 没命中目标：回退到原位置（你也可以选择不回退）
            draggingItem.transform.position = dragStartPos;
            draggingItem.OnDragCancel();
        }

        draggingItem.OnDragEnd(applied);

        // 3) 结束拖动
        draggingItem = null;
        getTouchedVector.ChangeDraggingState(false);
    }

    // =========================
    // [新增] 检测并对Charge生效（只在拖道具抬手时调用）
    // =========================
    private bool TryApplyItemToCharge(Item item)
    {
        Vector2 p = item.transform.position;

        // 你球/电荷是2D Collider，所以用Physics2D
        Collider2D[] hits = Physics2D.OverlapCircleAll(p, itemDropDetectRadius);
        foreach (var h in hits)
        {
            if (h.TryGetComponent<Charge>(out Charge charge))
            {
                // 让Item自己决定怎么生效（可扩展的关键点）
                return item.TryApplyToCharge(charge);
            }
        }

        return false;
    }

    #region 触碰开始到结束执行的操作
    void OnTouchBegin()
    {
        onTouching = true;
        Circle.SetActive(true);

        if (sr == null)
            sr = Circle.GetComponentInChildren<SpriteRenderer>();
        if (resetColorCoroutine != null)
            StopCoroutine(resetColorCoroutine);
        resetColorCoroutine = StartCoroutine(ResetColorNextFrame());

        radius = minRadius;
        expandTime = 0f;
        affectedCharges.Clear();
        Circle.transform.position = touchPosition;
        if (inAssistMode && assistStepIndex < currentAssistSteps.Count)
        {
            var assistData = currentAssistSteps[assistStepIndex];
            StartCoroutine(LerpToAssistPosition(assistData));
        }
    }

    void OnTouching()
    {
        if (expandTime < needTime)
        {
            expandTime += Time.deltaTime;
            chargeRate = expandTime / needTime;
            radius = Mathf.Lerp(minRadius, maxRadius, chargeRate);
        }
        VisibleCircle();
        
        DetectBalls();
        if (!inAssistMode)
            Circle.transform.position = touchPosition;
        if (affectedCharges.Count > 0)
        {
            foreach (Charge charge in affectedCharges)
            {
                if (charge.ChargeNum != 0)
                    CreateArrow(charge);
            }
        }
    }

    void OnTouchEnd()
    {
        //LevelUIManager.levelUIInst.updateStep(++Step);
    DetectBalls();
        if(affectedCharges.Count!=0)
        {
            UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(UIFunction.Inst.FindCanvas("UIScreen"), "StepText").text = "当前步数: " + ++Step;
            PRTS.GetInst().RemberPos(touchPosition, radius, Type); // 记录玩家操作
            BounceBall();           

            if (inAssistMode&&assistStepIndex < currentAssistSteps.Count)
                assistStepIndex++; // 进入下一步
        }
        else if(inAssistMode||(SaveManager.GetInst().GetLevelStar(SaveManager.CurrentLevel) == 3 && LevelManager.GetInst().touched.Step <= LevelManager.GetInst().touched.ThreeStarStep))
            ExitAssistMode();
    
    onTouching = false;
        //Circle.SetActive(false);

        if (releaseAnimRoutine != null)
        {
            StopCoroutine(releaseAnimRoutine);
            releaseAnimRoutine = null;
        }

        releaseAnimRoutine = StartCoroutine(ReleaseAnimEffect(radius));
    }
    #endregion
    private void ExitAssistMode()
    {
        inAssistMode = false;
        assistStepIndex = 0;
        currentAssistSteps = null;
    }

    #region 显示隐藏箭头的操作
    void CreateArrow(Charge charge)
    {
        GameObject arrow = charge.transform.Find("Arrow").gameObject;
        if (!arrow.activeSelf)
            arrow.SetActive(true);

        //Vector3 dir = Type * charge.ChargeNum * (charge.transform.position - touchPosition).normalized;
        Vector3 dir = Type * charge.ChargeNum * (charge.transform.position - Circle.transform.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        arrow.transform.position = charge.transform.position;
        arrow.transform.rotation = Quaternion.Euler(0, 0, angle);
        float distance = Mathf.Abs(charge.ChargeNum * 0.6f * chargeRate);
        arrow.transform.localScale = new Vector3(distance, arrow.transform.localScale.y, arrow.transform.localScale.z);
    }

    void DisableArrow(Charge charge)
    {
        if (charge.transform.Find("Arrow").gameObject)
        {
            charge.transform.Find("Arrow").gameObject.SetActive(false);
        }
        else
        {
            Debug.Log(charge.gameObject.name + "该电荷未携带箭头子物体");
        }
    }
    #endregion
    void DetectBalls()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(touchPosition, radius);
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<Rigidbody2D>(out Rigidbody2D rigidball) && hitCollider.TryGetComponent<Charge>(out Charge charge))
            {
                float dis = Mathf.Abs(Vector3.Distance(Circle.transform.position, charge.transform.position));
                if (!affectedCharges.Contains(charge))
                    affectedCharges.Add(charge);
                if (dis > radius && affectedCharges.Contains(charge))
                {
                    affectedCharges.Remove(charge);
                    DisableArrow(charge);
                }
            }
        }
    }

    void BounceBall()
    {
        foreach (Charge ball in affectedCharges)
        {
            if (ball.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                //Vector3 direction = (ball.transform.position - touchPosition).normalized;
                Vector3 direction = (ball.transform.position - Circle.transform.position).normalized;

                float force = ball.ChargeNum * bounceForce * chargeRate * Type;
                rb.AddForce(direction * force);
                DisableArrow(ball);
            }
        }
        affectedCharges.Clear();
    }

    void VisibleCircle()
    {
        Circle.transform.localScale = new Vector3(radius, radius, 1);
    }
    public void ChangeCircleType()
    {
        HasChangeCircle = true;
        UIFunction.Inst.SetCanvasGroupVise(Red, Type == -1);
        UIFunction.Inst.SetCanvasGroupVise(Blue, Type == 1);
        SpriteRenderer circle = UIFunction.Inst.GetCompoentInObjsChild<SpriteRenderer>(Circle, "匀强电场");
        circle.color = (Type == -1) ? Color.red : Color.blue;
        Type *= -1;
        
    }
    IEnumerator LerpToAssistPosition(PosForceDate assistData)
    {
        Vector3 startPos = touchPosition;
        Vector3 targetPos = new Vector3(assistData.x, assistData.y, touchPosition.z); // Z保持当前值
        float lerpDuration = 0.8f;
        float elapsed = 0f;

        while (elapsed < lerpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lerpDuration;
            touchPosition = Vector3.Lerp(startPos, targetPos, t);
            Circle.transform.position = touchPosition;

            // 允许提前结束，避免卡死（如果中途释放或操作）自行判断
            yield return null;
        }

        // 微调精度
        if (Vector2.Distance(new Vector2(touchPosition.x, touchPosition.y), new Vector2(assistData.x, assistData.y)) < 0.02f)
        {
            touchPosition.x = assistData.x;
            touchPosition.y = assistData.y;
        }

        radius = assistData.Force;
    }
    private IEnumerator ReleaseAnimEffect(float CircleBaseScale, float duration = 0.2f)
    {
        float elapsed = 0f;
        float startScale = 0.8f;
        float endScale = 1.5f;

        Vector3 baseScale = Vector3.one * CircleBaseScale; // 比如 Vector3.one * 1
        Circle.transform.localScale = baseScale * startScale;
        

        if (sr == null)
        {
            sr = Circle.GetComponentInChildren<SpriteRenderer>();
        }

        Color originalColor = sr.color;
        originalColor.a = 1;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 缩放
            float scaleFactor = Mathf.Lerp(startScale, endScale, t);
            Circle.transform.localScale = baseScale * scaleFactor;

            // 透明度变化
            Color newColor = originalColor;
            newColor.a = Mathf.Lerp(1f, 0f, t);
            sr.color = newColor;

            yield return null;
        }
        originalColor.a = 1;
        sr.color = originalColor;
        Circle.SetActive(false);
        releaseAnimRoutine = null;
    }
    IEnumerator ResetColorNextFrame()
    {
        yield return null; // 等待一帧，确保SpriteRenderer激活生效

        if (sr != null)
        {
            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
    }
    /*public void SmoothTimeStop(float duration = 0.5f)
    {
        StartCoroutine(SlowDownTime(duration));
    }

    private IEnumerator SlowDownTime(float duration)
    {
        float start = Time.timeScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // 注意用 unscaledDeltaTime，不受 TimeScale 影响
            Time.timeScale = Mathf.Lerp(start, 0f, elapsed / duration);
            yield return null;
        }
        UIManager.Pause(true);
        //Time.timeScale = 0f; // 保证最终为0
    }*/

}
