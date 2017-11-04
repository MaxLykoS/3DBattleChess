﻿using System.Collections.Generic;
using UnityEngine;

//Author: MaxLykoS
//UpdateTime: 2017/10/21

public class Transporter : Vehicle, ITransport
{

    public static int Price = 5000;

	void Start () 
    {
		
	}

    public override void SetEnemy()
    {
        GetComponent<MeshRenderer>().material = Resources.Load<Material>("Sprites/Unit/Materials/TransporterEnemy");
        base.SetEnemy();
    }

    public override void SetFriendly()
    {
        GetComponent<MeshRenderer>().material = Resources.Load<Material>("Sprites/Unit/Materials/TransporterFriendly");
        base.SetFriendly();
    }

    protected override List<Point> SetAttackRange()
    {
        virtualRange = new List<Point>();
        return virtualRange;
    }

    public Unit PayLoad
    {
        get;
        set;
    }

    public bool Load(Unit u)
    {
        if (u is Men)
        {
            PayLoad = u;

            Debug.Log(u.gridType);

            u.StopHighLight();
            u.StopShowAttackRange();

            GridContainer.Instance.UnitDic.Remove(u.gridID);
            u.gridID = null;
            Destroy(u.gameObject);

            return true;
        }
        else
        {
            Debug.Log(u.gridType+"上不了车！");
            return false;
        }
    }

    public bool UnLoad(Point pos)
    {
        if (!this.CheckCouldMoveTo(GridContainer.Instance.TerrainDic[pos]))
            return false;

        PayLoad.gridID = pos;
        GridContainer.Instance.AddUnit(PayLoad);

        PayLoad = null;

        StopMoveable();
        SetMovedToken();

        return true;
    }
}
