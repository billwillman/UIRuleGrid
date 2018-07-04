using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 子项绑定对象
/// </summary>
public class ListViewItem : MonoBehaviour
{
    #region 
    /// <summary>
    /// 源数据类型
    /// </summary>
    public Type sourceType { get; set; }
    #endregion

    #region member
    protected int index { get; set; }
    protected RulerGrid grid { get; set; }
    #endregion

    #region virtual
    public virtual void OnSourceData(RulerGrid grid, Type type)
    {
        this.grid = grid;
        this.sourceType = type;
    }
    public virtual void FindItem()
    {

    }
    public virtual void FillItem(IList datas, int index)
    {
        this.index = index;
    }
    #endregion
}
