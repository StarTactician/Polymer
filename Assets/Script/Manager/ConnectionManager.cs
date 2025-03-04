using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectionManager 
{
    private static ConnectionManager Inst;
    public static ConnectionManager GetInst()
    {
        if (Inst == null)
        {
            Debug.LogError("未找到ConnectionManager实例");
            return Inst;
        }
        return Inst;
    }

    public List<List<Charge>> ConnectedCharges ;
    public ConnectionManager()
    {
        Inst = this;
        ConnectedCharges = new List<List<Charge>>();
    }
    public void HandleCollision(Charge charge1, Charge charge2)
    {
        if (charge1 == null || charge2 == null)
        {
            Debug.LogWarning("传入的电荷为空，无法处理连接");
            return;
        }

        // 检查是否已经连接成对
        if (ConnectedCharges != null && ConnectedCharges.Count > 0)
        {
            foreach (var chargeList in ConnectedCharges)
            {
                if (chargeList.Contains(charge1) && chargeList.Contains(charge2))
                {
                    // 已经连接，直接返回
                    return;
                }
            }
        }

        // 如果可以连接，添加到列表中
        if (charge1.canConnect && charge2.canConnect)
        {
            List<Charge> newConnection = new List<Charge> { charge1, charge2 };
            ConnectedCharges.Add(newConnection);

            // 铰链连接自己与对方
            DistanceJoint2D joint = charge1.gameObject.AddComponent<DistanceJoint2D>();
            joint.connectedBody = charge2.gameObject.GetComponent<Rigidbody2D>();
            joint.autoConfigureDistance = false;
            joint.distance = Vector2.Distance(charge1.transform.position, charge2.transform.position);
            joint.breakForce = Mathf.Infinity;


            if (!charge1.neighbour.Contains(charge2))
                charge1.neighbour.Add(charge2);
            if (!charge2.neighbour.Contains(charge1))
                charge2.neighbour.Add(charge1);
            charge1.CurrntConnectNum++;
            charge2.CurrntConnectNum++;

            if (charge1.CurrntConnectNum >= charge1.MaxConnectNum)
                charge1.canConnect = false;
            if (charge2.CurrntConnectNum >= charge2.MaxConnectNum)
                charge2.canConnect = false;
            LevelManager.GetInst().VictoryCheck();
        }

        // 处理电荷中和逻辑
        HandleChargeNeutralization(charge1, charge2);
    }

    private void HandleChargeNeutralization(Charge charge1, Charge charge2)
    {
        int totalCharge = charge1.ChargeNum + charge2.ChargeNum;

        if (charge1.ChargeNum * charge2.ChargeNum < 0) // 异种电荷中和
        {
            charge1.ChargeNum = totalCharge / 2;
            charge2.ChargeNum = totalCharge / 2;

            if (totalCharge % 2 != 0)
            {
                if (totalCharge > 0)
                {
                    charge1.ChargeNum += 1;
                }
                else
                {
                    charge2.ChargeNum -= 1;
                }
            }
        }
        else if (charge1.ChargeNum * charge2.ChargeNum == 0) // 同种电荷平摊
        {
            charge1.ChargeNum = totalCharge / 2;
            charge2.ChargeNum = totalCharge / 2;

            if (totalCharge % 2 != 0)
            {
                charge1.ChargeNum += 1;
            }
        }

        charge1.changeColor();
        charge2.changeColor();
    }
    public void Reset()
    {
        ConnectedCharges.Clear();
    }
}
