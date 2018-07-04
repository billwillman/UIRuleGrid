using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 下拉刷新列表，仅支持下拉刷新模式，别的模式还没时间适配
/// </summary>
public class RefreshedListView<T> : IRuler, IRefreshedCreater<T>
{
    /// <summary>
    /// 是否在显示尾部
    /// </summary>
    private bool isEnd { get; set; }
    /// <summary>
    /// 组件对象持有
    /// </summary>
    private RulerGrid grid { get; set; }
    /// <summary>
    /// 项的功能代码
    /// </summary>
    private Type type { get; set; }
    /// <summary>
    /// 列表数据源
    /// </summary>
    private List<T> source = new List<T>();
    /// <summary>
    /// 模板对象
    /// </summary>
    private GameObject template { get; set; }
    /// <summary>
    /// 下拉至底部时候出发时间
    /// </summary>
    private Action onEndOfViewAction { get; set; }
    /// <summary>
    /// 刷新锁
    /// </summary>
    private bool isRefreshedLocked { get; set; }

    /// <summary>
    /// 创建循环列表,创建会重置循环列表
    /// </summary>
    /// <param name="source">数据集合</param>
    /// <param name="t">数据逻辑实现代码</param>
    public void CreateScrollView(List<T> source, Type type, GameObject template, Action OnEndOfViewAction)
    {
        this.isRefreshedLocked = false;
        this.onEndOfViewAction = OnEndOfViewAction;
        ///建立池
        CreateItemPool(0);
        ///回收池内对象
        grid.ResetToInit();
        this.template = template;
        this.source.Clear();
        this.source.AddRange(source);
        this.type = type;
        if (source.Count <= 0)
        {
            return;
        }
        grid.TryCreat();
    }

    /// <summary>
    /// 刷新
    /// </summary>
    /// <param name="list"></param>
    public void ReflashSource(List<T> list)
    {

        this.source.Clear();
        this.source.AddRange(list);
        if (source.Count <= 0)
        {
            grid.ResetToInit();
            return;
        }
        grid.TryReflash();
    }
    /// <summary>
    /// 实现刻度
    /// </summary>
    /// <returns></returns>
    public float[,] GetRulerData()
    {
        float[,] array = new float[source.Count, 2];
        //if (!template.activeSelf)
        //{
        //    template.SetActive(true);
        //}
        //var size = NGUIMath.CalculateRelativeWidgetBounds(template.transform);
        var size = this.template.GetComponent<UIWidget>();
        float startPos = 0;
        float addPos = 0;
        ///根据滚动方向设置
        switch (grid.scrollView.movement)
        {
            ///水平
            case UIScrollView.Movement.Horizontal:
                addPos = size.width;
                break;
            ///垂直
            case UIScrollView.Movement.Vertical:
                addPos = size.height;
                break;
            default:
                throw new Exception("未定义模式");
        }
        int len = source.Count;
        for (int i = 0; i < len; i++)
        {
            array[i, 0] = startPos;
            startPos += addPos;
            array[i, 1] = startPos;
            ///插入间距
            startPos += grid.spaceValue;
        }
        //template.SetActive(false);
        return array;
    }
    /// <summary>
    /// 清理接口
    /// </summary>
    public void Clear()
    {
        grid.OnDestroy();
        this.type = null;
        this.source.Clear();
        this.template = null;
        //this.label = null;
        this.onEndOfViewAction = null;
    }
    /// <summary>
    /// 根据index获取拷贝对象
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ListViewItem GetCloneObjByIndex(int index)
    {
        var item = GetGridItem();
        ///基础类型列表不用处理
        return item;
    }
    /// <summary>
    /// 根据id获取类型
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Type GetCloneTypeByIndex(int index)
    {
        return type;
    }
    /// <summary>
    /// 根据index回收拷贝对象
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public void RecyclingCloneObjByIndex(ListViewItem obj)
    {
        m_itemCOPool.Store(obj);
    }
    /// <summary>
    /// 对项代码进行赋值
    /// </summary>
    /// <param name="index"></param>
    /// <param name="obj"></param>
    public void SetObjViewByIndex(int index, ListViewItem obj)
    {
        if (this.source != null && index >= 0 && index < this.source.Count)
        {
            obj.FillItem(this.source, index);
        }
    }
    /// <summary>
    /// 下标索引范围,优化可选项
    /// </summary>
    /// <returns></returns>
    public int IndexSearchRange()
    {
        return grid.cloneMax;
    }
    /// <summary>
    /// 构造时候传入组件
    /// </summary>
    /// <param name="grid"></param>
    public void SetRulerGrid(RulerGrid grid)
    {
        this.grid = grid;
    }
    /// <summary>
    /// 到达列表底部
    /// </summary>
    /// <param name="isEnd"></param>
    public void OnScollEndOfList(bool isEnd)
    {
        if (this.onEndOfViewAction != null && !this.isEnd && isEnd && !this.isRefreshedLocked)
        {
            this.onEndOfViewAction();
        }
        this.isEnd = isEnd;
    }
    /// <summary>
    /// 显示等待刷新动画
    /// </summary>
    public void ShowRefreshing()
    {

#if DEBUG
        Debug.LogError("拉去数据");
#endif

        this.isRefreshedLocked = true;
    }
    /// <summary>
    /// 关闭刷新面板
    /// </summary>
    public void CloseRefreshing()
    {

#if DEBUG
        Debug.LogError("关闭数据");
#endif

        this.isRefreshedLocked = false;
    }

    #region pool 沿用现有池对象,清理需要和大师比对
    private GameObject m_poolGo;
    private ObjectPool<ListViewItem> m_itemCOPool = new ObjectPool<ListViewItem>();
    private ListViewItem CreateItemGO()
    {
        GameObject itemGO = GameObject.Instantiate(template);
        var item = itemGO.AddComponent(type) as ListViewItem;
        item.OnSourceData(grid, type);
        item.FindItem();
        if (null != itemGO)
        {
            itemGO.gameObject.SetActive(false);
        }
        return item;
    }
    private void Init(ListViewItem itemCO)
    {
        itemCO.gameObject.SetActive(false);
        itemCO.gameObject.transform.parent = m_poolGo.transform;
    }
    private void CreateItemPool(int poolNum)
    {
        if (m_poolGo != null) return;
        m_poolGo = new GameObject();
        m_poolGo.name = "pool";
        m_poolGo.transform.localScale = Vector3.zero;
        m_poolGo.transform.parent = grid.transform.parent;

        m_itemCOPool.Init(poolNum, CreateItemGO, Init);
        if (Application.isPlaying)
        {
            for (int i = 0, imax = grid.transform.childCount; i < imax; i++)
            {
                var childTrans = grid.transform.GetChild(0);
                if (childTrans != null && childTrans.gameObject != null)
                {
                    var item = childTrans.gameObject.GetComponent(type) as ListViewItem;
                    if (item != null)
                    {
                        m_itemCOPool.Store(item);
                    }
                }
            }
        }
    }
    private ListViewItem GetGridItem()
    {
        return m_itemCOPool.GetObject();
    }












    #endregion
}


