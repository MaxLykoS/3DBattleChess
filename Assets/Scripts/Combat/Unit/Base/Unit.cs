﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: MaxLykoS
//UpdateTime: 2017/11/6

public interface ITransport
{
    Unit PayLoad { set; get; }
    bool Load(Unit u);
    bool UnLoad(Point pos);

}
public abstract class Unit : Grid {
    public ArmorType armorType;
    public AttackType attackType;

    public List<Point> RangeList
    {
        get
        {
            if (virtualRange == null)
                virtualRange=SetAttackRange();
            if(rangeList==null)
                GetAttackPoint();
            return rangeList;
        }
    }  //实际的范围
    protected List<Point> rangeList; //实际的范围
    protected List<Point> virtualRange;   //设定的范围

    public float HP;
    private Transform HPDisplay;
    public int Oil;
    public float FirePower;
    [HideInInspector]
    public bool IsMoveable
    {
        get { return isMoveable; }
    }
    protected bool isMoveable = true;

    #region 设置阵营
    //设置阵营，编辑模式下启用
    public override void SetNeutral()
    {
        throw new System.Exception("Unit grid can not be Neutral!");
    }
    #endregion

    #region 高亮
    /// <summary>
    /// 高亮物体
    /// </summary>
    public override void SetHighLight()
    {
        bHighLight = true;
        StartCoroutine(IHightLight());
    }
    protected override IEnumerator IHightLight()
    {
        while (bHighLight)
        {
            transform.Rotate(0, 1, 0);
            yield return new WaitForSeconds(0.01f);
        }
        transform.localEulerAngles = new Vector3(0, -90, 0);
        yield return 0;
    }
    /// <summary>
    /// 停止高亮
    /// </summary>
    public override void StopHighLight()
    {
        bHighLight = false;
    }
    #endregion

    /// <summary>
    /// 当物体被加载后执行
    /// </summary>
    public override void OnInstatiate()
    {
        HPDisplay = Instantiate(Resources.Load<GameObject>("Prefabs/Cursor/HPDisplay"), transform).transform;
        HPDisplay.position = transform.position+new Vector3(0,2,0);
    }

    public abstract bool CheckCouldMoveTo(TerrainBase tb);

    #region 移动
    private Transform movedToken = null;
    /// <summary>
    /// 移动到指定位置
    /// </summary>
    /// <param name="targetPos"></param>
    public void UnitMoveToTargetPos(Point targetPos, PathNav.OnMoveEnd OnMoveEndMethod)
    {
        StartCoroutine(IUnitMoveToTargetPos(targetPos, OnMoveEndMethod));
    }
    protected IEnumerator IUnitMoveToTargetPos(Point targetPos, PathNav.OnMoveEnd OnMoveEndMethod)
    {
        StopMoveable();

        GridContainer.Instance.ExchangeUnitData(this, targetPos);     //先交换数据，再移动

        PathNav.CurrentMovingUnit = GridContainer.Instance.UnitDic[targetPos];

        float Height = GridContainer.Instance.TerrainDic[targetPos].transform.position.y + 1;

        #region 单位移动动画
        #region 上天3个单位
        int loopTimes = 0;
        int maxLoopTimes = 0;
        maxLoopTimes = (int)(0.1f * 3f * 100f);
        while (loopTimes <= maxLoopTimes && PathNav.bMoving)
        {
            transform.Translate(Vector3.up * 0.1f);
            loopTimes++;
            yield return new WaitForSeconds(0.01f);
        }
        #endregion

        #region 平移
        loopTimes = 0;
        int xBetween = Convert.ToInt32(Math.Abs(targetPos.X - transform.position.x));
        int zBetween = Convert.ToInt32(Math.Abs(targetPos.X - transform.position.x) + Math.Abs(targetPos.Z - transform.position.z));
        int nodesBetween = xBetween > zBetween ? xBetween : zBetween;   //0.5s  0.1f
        maxLoopTimes = (int)(0.1f * (float)nodesBetween * 100f);
        while (loopTimes <= maxLoopTimes && PathNav.bMoving)
        {
            Vector3 position = new Vector3(transform.position.x, 0, transform.position.z);
            transform.position = new Vector3(Mathf.MoveTowards(position.x, targetPos.X, 0.1f),
                transform.position.y,
                Mathf.MoveTowards(position.z, targetPos.Z, 0.1f));
            loopTimes++;
            yield return new WaitForSeconds(0.01f);
        }
        #endregion

        #region 下地
        loopTimes = 0;
        maxLoopTimes = (int)(0.1f * (transform.position.y - Height) * 100f);
        while (loopTimes != maxLoopTimes && PathNav.bMoving)
        {
            transform.Translate(Vector3.down * 0.1f);
            loopTimes++;
            yield return new WaitForSeconds(0.01f);
        }
        #endregion
        #endregion

        transform.position = new Vector3(targetPos.X, Height, targetPos.Z);   //落地时校准坐标，避免误差

        OnMoveEndMethod();//移动完毕后的回调函数

        yield return 0;
    }
    /// <summary>
    /// 设置单位可移动并销毁之前的已移动标志
    /// </summary>
    public void SetMoveableDestroyToken()
    {
        isMoveable = true;

        if (movedToken != null)
        {
            Destroy(movedToken.gameObject);
            movedToken = null;
        }
    }
    /// <summary>
    /// 让单位不可移动
    /// </summary>
    public void StopMoveable()
    {
        isMoveable = false;
    }
    /// <summary>
    /// 设置已移动标志
    /// </summary>
    public void SetMovedToken()
    {
        if (movedToken != null)
        {
            return;
        }
        movedToken = Instantiate(Resources.Load<GameObject>("Prefabs/Cursor/MovedToken")).transform;
        movedToken.SetParent(gameObject.transform);
        Vector3 vec3 = transform.position;
        vec3.y += 1.1f;
        movedToken.position = vec3;
    }
    /// <summary>
    /// 展示移动范围
    /// </summary>
    public void ShowMoveRange()
    {
        PathNav.ShowUnitMoveRange(this);
    }
    #endregion

    #region 攻击
    /// <summary>
    /// 还击
    /// </summary>
    /// <param name="targetUnit"></param>
    public void AttackPassive(Unit targetUnit)
    {
        float firePower;

        GetDamageNum(out firePower, attackType, targetUnit.armorType);
        firePower *= 0.6f;

        targetUnit.BeAttacked(firePower);
    }

    /// <summary>
    /// 主动攻击
    /// </summary>
    /// <param name="targetUnit"></param>
    public void AttackInitiative(Unit targetUnit)
    {
        float firePower;   //firePower实际伤害

        GetDamageNum(out firePower, attackType, targetUnit.armorType);

        targetUnit.BeAttacked(firePower);
    }

    /// <summary>
    /// 根据护甲和攻击类型通过不同的计算公式计算伤害量
    /// </summary>
    /// <param name="firePower"></param>
    /// <param name="self"></param>
    /// <param name="target"></param>
    void GetDamageNum(out float firePower,AttackType self, ArmorType target)
    {
        firePower = FirePower;
        switch (attackType)
        {
            case AttackType.Bullet:
                {
                    switch (target)
                    {
                        case ArmorType.Men:
                            firePower *= 3f;
                            break;
                        case ArmorType.Plane:
                            firePower *= 3;
                            break;
                        case ArmorType.Ship:
                            firePower *= 0.2f;
                            break;
                        case ArmorType.Vehicle:
                            firePower *= 0.6f;
                            break;
                    }
                    break;
                }
            case AttackType.Explosion:
                {
                    switch (target)
                    {
                        case ArmorType.Men:
                            firePower *= 0.9f;
                            break;
                        case ArmorType.Plane:
                            firePower *= 2;
                            break;
                        case ArmorType.Ship:
                            firePower *= 2.5f;
                            break;
                        case ArmorType.Vehicle:
                            firePower *= 2.2f;
                            break;
                    }
                }
                break;
            case AttackType.NoWeapon:
                firePower = 0;break;
            default: throw new Exception("未知攻击类型");
        }
        firePower *= (HP / 100f);
    }

    /// <summary>
    /// 重写本方法来设定单位的攻击范围
    /// </summary>
    /// <returns></returns>
    protected virtual List<Point> SetAttackRange()
    {
        virtualRange = new List<Point>();
        virtualRange.Add(new Point(0, 0));
        virtualRange.Add(new Point(0, 1));
        virtualRange.Add(new Point(1, 0));
        virtualRange.Add(new Point(0, -1));
        virtualRange.Add(new Point(-1, 0));
        return virtualRange;
    }

    /// <summary>
    /// 从重写的SetAttackRange方法推算实际攻击范围
    /// </summary>
    private void GetAttackPoint()
    {
        rangeList = new List<Point>();
        int x = gridID.X;
        int z = gridID.Z;
        for (int i = 0; i < virtualRange.Count; i++)
        {
            Point p = virtualRange[i];
            TerrainBase tb;
            if (GridContainer.Instance.TerrainDic.TryGetValue(new Point(x + p.X, z + p.Z), out tb))
            {
                rangeList.Add(tb.gridID);   //得到攻击范围
            }
        }
    }

    /// <summary>
    /// 显示攻击范围，并范围是否能攻击
    /// </summary>
    public bool ShowAttackRange()
    {
        if (RangeList.Count == 0)
            return false;
        for (int i = 0; i < rangeList.Count; i++)
        {
            Point p = RangeList[i];
            GridContainer.Instance.TerrainDic[p].SetHighLight();
        }
        return true;
    }

    /// <summary>
    /// 判断能否攻击到
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool CheckAttackable(Point pos)
    {
        return RangeList.Contains(pos) ? true : false;
    }

    /// <summary>
    /// 停止显示攻击范围
    /// </summary>
    public void StopShowAttackRange()
    {
        if (RangeList == null) return;
        foreach (Point p in RangeList)
        {
            GridContainer.Instance.TerrainDic[p].StopHighLight();
        }
        rangeList.Clear();
        rangeList = null;
    }

    /// <summary>
    /// 被攻击
    /// </summary>
    /// <param name="firePower"></param>
    public void BeAttacked(float firePower)
    {
        int defendStar = GridContainer.Instance.TerrainDic[gridID].DefendStar;

        firePower *= (1f - 0.1f * defendStar);

        HP -= firePower;

        GameStatNotifier.Instance.BeAttack[(int)Side]+=firePower;
        GameStatNotifier.Instance.Attack[((int)Side+1)%2] += firePower;

        UpdateHPText();
    }

    /// <summary>
    /// 判断单位是否存活
    /// </summary>
    /// <returns></returns>
    public bool isAlive()
    {
        return HP > 0 ? true : false;
    }

    /// <summary>
    /// 被摧毁
    /// </summary>
    public override void BeDestroyed()
    {
        if(movedToken!=null)
            Destroy(movedToken.gameObject);
        Destroy(gameObject);
        GridContainer.Instance.UnitDic.Remove(gridID);

        GameStatNotifier.Instance.Destroyed[(int)Side]++;
        GameStatNotifier.Instance.Elimination[((int)Side+1)%2]++;
    }
    #endregion

    /// <summary>
    /// 隐藏单位，主要用于上车
    /// </summary>
    public void Hide()
    {
        GridContainer.Instance.UnitDic.Remove(gridID);
        gridID = null;
        gameObject.SetActive(false);

        if (movedToken != null)
            Destroy(movedToken.gameObject);
        movedToken = null;
    }

    /// <summary>
    /// 显示单位，主要用于下车
    /// </summary>
    /// <param name="showPos"></param>
    public void Show(Point showPos)
    {
        GridContainer.Instance.UnitDic[showPos] = this;
        gridID = showPos;
        gameObject.SetActive(true);
        Vector3 tp = GridContainer.Instance.TerrainDic[showPos].transform.position;
        tp.y++;
        transform.position = tp;
    }

    /// <summary>
    /// 更新血条
    /// </summary>
    public void UpdateHPText()
    {
        if (HP == 100) return;
        int hp = (int)(HP / 10f);
        SpriteRenderer sr = HPDisplay.gameObject.GetComponent<SpriteRenderer>();
        switch (hp)
        {
            case 9:
                sr.sprite = Resources.LoadAll<Sprite>("Sprites/Cursor/num1to9")[9];
                break;
            case 8:
                sr.sprite = Resources.LoadAll<Sprite>("Sprites/Cursor/num1to9")[7];
                break;
            case 7:
                sr.sprite = Resources.LoadAll<Sprite>("Sprites/Cursor/num1to9")[6];
                break;
            case 6:
                sr.sprite = Resources.LoadAll<Sprite>("Sprites/Cursor/num1to9")[5];
                break;
            case 5:
                sr.sprite = Resources.LoadAll<Sprite>("Sprites/Cursor/num1to9")[4];
                break;
            case 4:
                sr.sprite = Resources.LoadAll<Sprite>("Sprites/Cursor/num1to9")[3];
                break;
            case 3:
                sr.sprite = Resources.LoadAll<Sprite>("Sprites/Cursor/num1to9")[2];
                break;
            case 2:
                sr.sprite = Resources.LoadAll<Sprite>("Sprites/Cursor/num1to9")[1];
                break;
            case 1:
                sr.sprite = Resources.LoadAll<Sprite>("Sprites/Cursor/num1to9")[0];
                break;
            case 0:
                sr.sprite = Resources.LoadAll<Sprite>("Sprites/Cursor/num1to9")[0];
                break;
        }
    }

    public bool isLongRangeUnit()
    {
        return gridType == GridType.Artillery
            || gridType == GridType.Rockets
            || gridType == GridType.BattleShip;
    }
}