using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetTouchedVector
{
    public enum Mode
    {
        LayerMaskDetection, // 模式1：识别LayerMask并获取射线打到其上的坐标
        ObjectDetection // 模式2：获取射线打到的第一个游戏物体的信息
    }

    [Tooltip("选择获取模式：LayerMask检测或物体检测")]
    public Mode currentMode = Mode.LayerMaskDetection;

    [Tooltip("用于射线检测的背景图层，仅在LayerMaskDetection模式下有效")]
    public LayerMask touchedLayer= LayerMask.GetMask("BackGround");

    [Tooltip("检测射线的最大距离")]
    public float maxRayDistance = 20f;

    // 获取的触摸点或点击位置
    public Vector3 touchedPosition;

    // 获取的射线打到的物体信息（仅在ObjectDetection模式下有效）
    public GameObject touchedObject;

    void ProcessTouch(Vector3 inputPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(inputPosition);
        RaycastHit hit;

        switch (currentMode)
        {
            case Mode.LayerMaskDetection:
                if (Physics.Raycast(ray, out hit, maxRayDistance, touchedLayer))
                {
                    touchedPosition = hit.point;
                    touchedObject = null;
                }
                break;

            case Mode.ObjectDetection:
                if (Physics.Raycast(ray, out hit, maxRayDistance))
                {
                    touchedPosition = hit.point;
                    touchedObject = hit.collider.gameObject;
                }
                break;
        }
    }

    // 提供获取位置的方法，供其他脚本调用
    public Vector3 GetTouchedPosition(Vector3 inputPosition)
    {
        currentMode = Mode.LayerMaskDetection;
        ProcessTouch(inputPosition);
        return touchedPosition;
    }

    // 提供获取被触摸物体的方法，仅在ObjectDetection模式下有效
    public GameObject GetTouchedObject(Vector3 inputPosition)
    {
        currentMode = Mode.ObjectDetection;
        ProcessTouch(inputPosition);
        return touchedObject;
    }
}
