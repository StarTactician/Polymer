using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetTouchedVector
{


    public bool IsDraggingItem { get; private set; }
    public void ChangeDraggingState(bool state)
    {
        IsDraggingItem = state;
        Debug.Log("当前道具拖动状态为"+state);
    }



    [Tooltip("用于射线检测的背景图层，仅在LayerMaskDetection模式下有效")]
    public LayerMask touchedLayer= LayerMask.GetMask("BackGround");

    [Tooltip("检测射线的最大距离")]
    public float maxRayDistance = 20f;

    // 获取的触摸点或点击位置
    public Vector3 touchedPosition;

    // 获取的射线打到的物体信息（仅在ObjectDetection模式下有效）
    public GameObject touchedObject;



    // =========================
    // [修改] 只负责获取背景世界坐标（不再切Mode）
    // =========================
    public Vector3 GetTouchedPosition(Vector3 inputScreenPosition)
    {
        UpdateTouchedPositionByBackground(inputScreenPosition);
        return touchedPosition;
    }

    // =========================
    // [修改] 获取“屏幕位置下的物体”
    // 说明：你的Charge/Ball是2D Collider，所以这里优先Physics2D；
    //      如果你道具/场景里有3D Collider，也提供3D兜底。
    // =========================
    public GameObject GetTouchedObject(Vector3 inputScreenPosition)
    {
        UpdateTouchedObject(inputScreenPosition);
        return touchedObject;
    }

    // =========================
    // [新增] 按下那一帧的“自适应检测”
    // 规则：优先检测 Item；否则当作点到背景（返回背景世界坐标）
    // 返回：true=点到Item；false=点到背景
    // =========================
    public bool AutoDetectBegin(Vector3 inputScreenPosition, out GameObject hitItemObj, out Vector3 hitWorldPos)
    {
        // 1) 先看是不是点到 Item（优先）
        UpdateTouchedObject(inputScreenPosition);
        if (touchedObject != null && touchedObject.GetComponent<Item>() != null)
        {
            hitItemObj = touchedObject;
            hitWorldPos = touchedPosition; // touchedPosition在UpdateTouchedObject里也会被更新
            return true;
        }

        // 2) 否则按背景处理，返回背景的世界坐标
        UpdateTouchedPositionByBackground(inputScreenPosition);
        hitItemObj = null;
        hitWorldPos = touchedPosition;
        return false;
    }

    // -------------------------
    // [新增] 背景取点：保持你原先的 Physics.Raycast + LayerMask 思路
    // -------------------------
    private void UpdateTouchedPositionByBackground(Vector3 inputScreenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(inputScreenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxRayDistance, touchedLayer))
        {
            touchedPosition = hit.point;
        }
        else
        {
            // [新增] 兜底：避免没打到背景时坐标不更新（否则会用到上一次的touchedPosition）
            var wp = Camera.main.ScreenToWorldPoint(new Vector3(inputScreenPosition.x, inputScreenPosition.y, 0f));
            touchedPosition = wp;
        }
    }

    // -------------------------
    // [新增] 物体检测：优先2D（命中Charge/Ball/2D道具），再兜底3D
    // touchedPosition 同步更新成命中点（拖拽时可直接用）
    // -------------------------
    private void UpdateTouchedObject(Vector3 inputScreenPosition)
    {
        touchedObject = null;

        // 2D Raycast：你的球/电荷是CircleCollider2D（Ball.cs/Charge.cs）
        Vector3 wp = Camera.main.ScreenToWorldPoint(inputScreenPosition);
        Vector2 origin = new Vector2(wp.x, wp.y);

        RaycastHit2D hit2D = Physics2D.Raycast(origin, Vector2.zero);
        if (hit2D.collider != null)
        {
            touchedObject = hit2D.collider.gameObject;
            touchedPosition = hit2D.point;
            return;
        }

        // 3D 兜底（如果你有3D collider的道具/物体）
        Ray ray3D = Camera.main.ScreenPointToRay(inputScreenPosition);
        RaycastHit hit3D;
        if (Physics.Raycast(ray3D, out hit3D, maxRayDistance))
        {
            touchedObject = hit3D.collider.gameObject;
            touchedPosition = hit3D.point;
        }
        else
        {
            touchedPosition = wp;
        }
    }
}
