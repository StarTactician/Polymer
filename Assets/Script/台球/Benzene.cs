using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Benzene : Charge
{
    [Tooltip("连接口可视预制体，需挂有 SpriteRenderer")]
    public GameObject portVisualPrefab;

    [Tooltip("六边形半径，用于定位生成")]
    public float portRadius = 1.92f;

    private List<Transform> edgePorts = new List<Transform>();
    private List<SpriteRenderer> edgePortSprites = new List<SpriteRenderer>();

    private int connectedPortID = -1;
    private bool isFirstConnected = false;

    protected override void Start()
    {
        base.Start();
        GenerateEdgePorts();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canConnect || CurrntConnectNum >= MaxConnectNum)
            return;

        if (collision.gameObject.TryGetComponent<Charge>(out Charge otherCharge))
        {
            if (!isFirstConnected)
            {
                connectedPortID = GetClosestPortIndex(otherCharge.transform.position);
                isFirstConnected = true;
                HighlightPorts(connectedPortID);
            }
            else
            {
                int targetPortID = GetClosestPortIndex(otherCharge.transform.position);
                int expectedPortID = (connectedPortID + 3) % 6;

                if (targetPortID != expectedPortID)
                {
                    Debug.LogWarning($"❌ 连接失败！目标聚合位置为 {expectedPortID + 1} 号口，当前靠近 {targetPortID + 1} 号口");
                    LevelManager.GetInst().TriggerDefeat("连接方向错误！必须对位连接苯环");
                    return;
                }
            }

            ConnectionManager.GetInst().HandleCollision(this, otherCharge);
        }
    }

    private void GenerateEdgePorts()
    {
        edgePorts.Clear();
        edgePortSprites.Clear();

        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60 * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * portRadius;
            Vector3 worldPos = transform.position + offset;

            GameObject portObj = Instantiate(portVisualPrefab, worldPos, Quaternion.identity, this.transform);
            portObj.name = $"Port_{i + 1}";

            edgePorts.Add(portObj.transform);

            SpriteRenderer sr = portObj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white;
                edgePortSprites.Add(sr);
            }
            else
            {
                Debug.LogWarning($"Port {i + 1} 没有找到 SpriteRenderer");
                edgePortSprites.Add(null);
            }
        }
    }

    private int GetClosestPortIndex(Vector3 targetPos)
    {
        int closestIndex = 0;
        float minDist = float.MaxValue;

        for (int i = 0; i < edgePorts.Count; i++)
        {
            float dist = Vector3.Distance(edgePorts[i].position, targetPos);
            if (dist < minDist)
            {
                minDist = dist;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private void HighlightPorts(int connectedID)
    {
        for (int i = 0; i < edgePortSprites.Count; i++)
        {
            if (edgePortSprites[i] == null) continue;

            if (i == connectedID || i == (connectedID + 3) % 6)
                edgePortSprites[i].color = Color.green;
            else
                edgePortSprites[i].color = Color.red;
        }

        // 如果苯环已完成连接（两口对位连接）
        if (CurrntConnectNum == MaxConnectNum && connectedID >= 0)
        {
            Debug.Log("✅ 苯环连接完毕！");
            StartCoroutine(HidePortVisualsAfterDelay(1f));
        }
    }
    private IEnumerator HidePortVisualsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (var sprite in edgePortSprites)
        {
            if (sprite != null)
            {
                sprite.enabled = false;
            }
        }
    }

}
