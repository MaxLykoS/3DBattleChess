﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Author: MaxLykoS
//CreatedTime: 2017.11.20
public class PanelBase : MonoBehaviour {

    //皮肤路径
    public string skinPath;
    //皮肤
    public GameObject skin;
    //层级
    public PanelLayer layer;
    //面板参数
    public object[] args;

    #region 声明周期
    public virtual void Init(params object[] args)
    {
        this.args = args;
    }

    //开始面板前
    public virtual void OnShowing() { }

    //显示面板后
    public virtual void OnShowed() { }

    //帧更新
    public virtual void Update() { }

    //关闭前
    public virtual void OnClosing() { }

    //关闭后
    public virtual void OnClosed() { }
    #endregion

    #region 操作
    protected virtual void Close()
    {
        string name = this.GetType().ToString();
        PanelMgr.Instance.ClosePanel(name);
    }
    #endregion
}
