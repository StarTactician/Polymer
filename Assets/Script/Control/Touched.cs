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
    private bool onTouching;
    public int TwoStarStep;//达成二星所要求不高于的步数
    public int ThreeStarStep;//达成三星所要求不高于的步数
    [Tooltip("每关的答案，由Touched提供（节点，边）")]
    public List<int> Answear;
    private Vector3 touchPosition;
    public List<Charge> affectedCharges = new List<Charge>();
    private GetTouchedVector getTouchedVector;

    private void Awake()
    {
        LevelManager.GetInst().touched = this;
    }
    private void Start()
    {
        LevelManager.GetInst().touched = this;
        getTouchedVector = GameRoot.GetInst().GetTouchedVector_Root;
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(UIFunction.Inst.FindCanvas(), "StepText").text = "当前步数: 0";
    }
    void Update()
    {
        if (UIManager.IsPause)
            return;
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            touchPosition = getTouchedVector.GetTouchedPosition(Input.mousePosition);
            OnTouchBegin();
        }
        else if ((Input.GetMouseButton(0) && onTouching) || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary)))
        {
            touchPosition = getTouchedVector.GetTouchedPosition(Input.mousePosition);
            OnTouching();
        }
        else if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled)))
        {
            OnTouchEnd();
        }
    }
    #region 触碰开始到结束执行的操作
    void OnTouchBegin()
    {
        onTouching = true;
        Circle.SetActive(true);
        radius = minRadius;
        expandTime = 0f;
        affectedCharges.Clear();
        Circle.transform.position = touchPosition;
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
        Circle.transform.position = touchPosition;
        DetectBalls();
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
        UIFunction.Inst.GetCompoentInObjsChild<TextMeshProUGUI>(UIFunction.Inst.FindCanvas(), "StepText").text="当前步数: "+ ++Step;
        onTouching = false;
        Circle.SetActive(false);
        DetectBalls();
        BounceBall();
    }
    #endregion
    #region 显示隐藏箭头的操作
    void CreateArrow(Charge charge)
    {
        GameObject arrow = charge.transform.Find("Arrow").gameObject;
        if (!arrow.activeSelf)
            arrow.SetActive(true);

        Vector3 dir = Type * charge.ChargeNum * (charge.transform.position - touchPosition).normalized;
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
                Vector3 direction = (ball.transform.position - touchPosition).normalized;
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
}
