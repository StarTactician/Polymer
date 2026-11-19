using UnityEngine;

public class Item : MonoBehaviour
{
    // 通用道具属性
    [Tooltip("道具的剩余使用次数")]
    public int useCount;

    // 是否正在拖动道具
    protected bool isBeingDragged;

    private Vector3 offset;

    GetTouchedVector touchedVector;
    public virtual void OnDragBegin() { }
    public virtual void OnDragging() { }
    public virtual void OnDragCancel() { }
    public virtual void OnDragEnd(bool applied) { }
    public bool CanUse()
    {
        return useCount > 0 && isActiveAndEnabled;
    }
    // =========================
    // 核心：对Charge生效（由子类实现）
    // 返回：true=成功施加并消耗次数；false=未生效
    // =========================
    public virtual bool TryApplyToCharge(Charge charge)
    {
        // 默认不生效
        return false;
    }
    private void Start()
    {
        touchedVector = GameRoot.GetInst().GetTouchedVector_Root;
    }
    // 需要在子类中实现道具的应用逻辑
    protected virtual void ApplyEffectToObject() { }
    protected void ConsumeOnce()
    {
        useCount = Mathf.Max(0, useCount - 1);
        if (useCount == 0)
        {
            // 次数用完：直接禁用
            gameObject.SetActive(false);
        }
    }

    // 获取鼠标世界坐标
    protected Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

    // 鼠标按下时开始拖动
    private void OnMouseDown()
    {
        if (useCount > 0)
        {
            isBeingDragged = true;
            offset = transform.position - GetMouseWorldPosition();

            // 禁用 Touched 的触摸事件
            if (touchedVector != null) touchedVector.ChangeDraggingState(true);
        }
    }

    // 拖动过程中更新道具位置
    private void OnMouseDrag()
    {
        if (isBeingDragged)
        {
            transform.position = GetMouseWorldPosition() + offset;
        }
    }

    // 鼠标抬起时，应用道具效果
    private void OnMouseUp()
    {
        if (isBeingDragged)
        {
            isBeingDragged = false;

            // 应用道具效果
            ApplyEffectToObject();

            // 恢复 Touched 的触摸事件
            if (touchedVector != null) touchedVector.ChangeDraggingState(false);
        }
    }
}
