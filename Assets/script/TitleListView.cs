using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 带有标签的循环列表,外围控件需要pivot与底板一致
/// </summary>
public class TitleListView<T1, T2> : IRuler, ICreater<T1, T2>
{


    /// <summary>
    /// 组件对象持有
    /// </summary>
    private RulerGrid grid { get; set; }
    /// <summary>
    /// 子预制件
    /// </summary>
    public GameObject child { get; set; }
    /// <summary>
    /// 标签预制件
    /// </summary>
    public GameObject title { get; set; }
    /// <summary>
    /// 子对象初始化函数
    /// </summary>
    private Type childType { get; set; }
    /// <summary>
    /// 标签初始化函数
    /// </summary>
    private Type titleType { get; set; }
    /// <summary>
    /// 数据源
    /// </summary>
    private Dictionary<T1, List<T2>> source { get; set; }
    /// <summary>
    /// 标签索引
    /// </summary>
    private Dictionary<int, int> titleSource { get; set; }
    /// <summary>
    /// 标题集合，无奈的选择，为了兼容代码
    /// </summary>
    private List<T1> titleList { get; set; }
    /// <summary>
    /// 子对象索引
    /// </summary>
    private Dictionary<int, int> childSource { get; set; }
    /// <summary>
    /// 项集合，无奈的选择，为了兼容代码
    /// </summary>
    private List<T2> childList { get; set; }
    /// <summary>
    /// 状态接口 与titleList一一对应
    /// </summary>
    private List<bool> state { get; set; }
    /// <summary>
    /// 子项选中key,标记值
    /// </summary>
    private int selectKey { get; set; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="source"></param>
    /// <param name="childType"></param>
    /// <param name="child"></param>
    /// <param name="titleType"></param>
    /// <param name="title"></param>
    public void CreateScrollView(Dictionary<T1, List<T2>> source, Type childType, GameObject child, Type titleType, GameObject title)
    {
        this.selectKey = -1;
        ///建立池
        CreateItemPool(0);
        ///回收池内对象
        grid.ResetToInit();
        this.source = source;
        this.childType = childType;
        this.child = child;
        this.titleType = titleType;
        this.title = title;
        if (source.Count <= 0)
        {
            return;
        }
        grid.TryCreat();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="childType"></param>
    /// <param name="child"></param>
    /// <param name="titleType"></param>
    /// <param name="title"></param>
    /// <param name="state">可以为空，默认全部关闭</param>
    public void CreateScrollView(Dictionary<T1, List<T2>> source, Type childType, GameObject child, Type titleType, GameObject title, List<bool> state)
    {
        this.selectKey = -1;
        CreateItemPool(0);
        ///回收池内对象
        grid.ResetToInit();
        this.source = source;
        this.childType = childType;
        this.child = child;
        this.titleType = titleType;
        this.title = title;
        if (source.Count <= 0)
        {
            return;
        }
        OnStateInit(state, source);

        grid.TryCreat();
    }

    /// <summary>
    /// 刷新接口
    /// </summary>
    /// <param name="source"></param>
    public void ReflashSource(Dictionary<T1, List<T2>> source)
    {
        this.source = source;
        if (source.Count <= 0)
        {
            grid.ResetToInit();
            return;
        }
        grid.TryReflash();
    }
    /// <summary>
    /// 带显示状态刷新
    /// </summary>
    /// <param name="source"></param>
    /// <param name="state"></param>
    public void ReflashSource(Dictionary<T1, List<T2>> source, List<bool> state)
    {
        OnStateInit(state, source);
        ReflashSource(source);
    }
    /// <summary>
    /// 设置标签页状态
    /// </summary>
    /// <param name="state"></param>
    private void OnStateInit(List<bool> state, Dictionary<T1, List<T2>> source)
    {
        if (state == null)
        {
            this.state = new List<bool>();
            int len = source.Count;
            for (int i = 0; i < len; i++)
            {
                this.state.Add(false);
            }
        }
        else
        {
            if (state.Count != source.Count)
            {
                this.state = new List<bool>();
                int len = source.Count;
                for (int i = 0; i < len; i++)
                {
                    this.state.Add(false);
                }

#if DEBUG
                Debug.LogError("标签数据源不一致");
#endif

            }
            else
            {
                this.state = state;
            }
        }
    }
    /// <summary>
    /// 清空对象
    /// </summary>
    public void Clear()
    {
        this.child = null;
        this.title = null;
        this.childType = null;
        this.titleType = null;
        this.source = null;
        this.titleSource = null;
        this.childSource = null;
        this.selectKey = -1;
        this.state = null;
        this.childList = null;
        this.titleList = null;
    }
    /// <summary>
    /// 根据id获取拷贝对象
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ListViewItem GetCloneObjByIndex(int index)
    {
        ListViewItem item = null;
        if (childSource.ContainsKey(index))
        {
            item = GetChildItem();
        }
        if (titleSource.ContainsKey(index))
        {
            item = GetTitleItem();
        }
        return item;
    }
    /// <summary>
    /// 根据id获取类型
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Type GetCloneTypeByIndex(int index)
    {
        if (childSource.ContainsKey(index))
        {
            return childType;
        }
        if (titleSource.ContainsKey(index))
        {
            return titleType;
        }
        return null;
    }
    /// <summary>
    /// 根据下标获取状态
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool GetTitleState(int index)
    {
        return this.state[index];
    }



    /// <summary>
    /// 获取尺度数据
    /// </summary>
    /// <returns></returns>
    public float[,] GetRulerData()
    {
        if (this.titleSource == null)
        {
            this.titleSource = new Dictionary<int, int>();
        }
        if (this.childSource == null)
        {
            this.childSource = new Dictionary<int, int>();
        }
        if (this.childList == null)
        {
            this.childList = new List<T2>();
        }
        if (this.titleList == null)
        {
            this.titleList = new List<T1>();
        }
        this.titleSource.Clear();
        this.childSource.Clear();
        this.childList.Clear();
        this.titleList.Clear();
        int index = 0;
        int stateIndex = 0;
        var enumerator = this.source.GetEnumerator();
        while (enumerator.MoveNext())
        {
            ///标签添加
            var key = enumerator.Current.Key;
            this.titleSource.Add(index, this.titleList.Count);
            this.titleList.Add(key);
            index++;
            ///存在筛选条件时候筛选
            if ((this.state == null) ||
                (this.state != null && this.state[stateIndex]))
            {
                var list = enumerator.Current.Value;
                ///成员添加
                var len = list.Count;
                for (int i = 0; i < len; i++)
                {
                    this.childSource.Add(index, this.childList.Count);
                    this.childList.Add(list[i]);
                    index++;
                }
            }
            stateIndex++;
        }
        enumerator.Dispose();
        float[,] array = new float[this.titleList.Count + this.childList.Count, 2];
        if (!child.activeSelf)
        {
            child.SetActive(true);
        }
        UIWidget childSize = child.GetComponent<UIWidget>();
        if (!title.activeSelf)
        {
            title.SetActive(true);
        }
        UIWidget titleSize = title.GetComponent<UIWidget>();

        float startPos = 0;
        float addPosChild = 0;
        float addPosTitle = 0;

        ///根据滚动方向设置
        switch (grid.scrollView.movement)
        {
            ///水平
            case UIScrollView.Movement.Horizontal:
                addPosChild = childSize.width;
                addPosTitle = titleSize.width;
                break;
            ///垂直
            case UIScrollView.Movement.Vertical:
                addPosChild = childSize.height;
                addPosTitle = titleSize.height;
                break;
            default:
                throw new Exception("未定义模式");
        }


        index = 0;
        stateIndex = 0;
        enumerator = this.source.GetEnumerator();
        while (enumerator.MoveNext())
        {


            ///标签添加
            array[index, 0] = startPos;
            startPos += addPosTitle;
            array[index, 1] = startPos;
            startPos += grid.spaceValue;
            index++;
            var key = enumerator.Current.Key;
            ///存在筛选条件时候筛选
            if ((this.state == null) ||
                (this.state != null && this.state[stateIndex]))
            {
                ///成员添加
                var list = enumerator.Current.Value;
                var len = list.Count;
                for (int i = 0; i < len; i++)
                {
                    if (i == 0 && grid.spaceValue != grid.childspaceValue) {
                        startPos += grid.childspaceValue;
                    }
                    array[index, 0] = startPos;
                    startPos += addPosChild;
                    array[index, 1] = startPos;
                    startPos += grid.childspaceValue;
                    index++;
                }
            }
            stateIndex++;
        }
        enumerator.Dispose();
        child.SetActive(false);
        title.SetActive(false);
        return array;
    }
    /// <summary>
    /// 下标取值范围
    /// </summary>
    /// <returns></returns>
    public int IndexSearchRange()
    {
        return grid.cloneMax;
    }
    /// <summary>
    /// 池回收
    /// </summary>
    /// <param name="index"></param>
    /// <param name="obj"></param>
    public void RecyclingCloneObjByIndex(ListViewItem obj)
    {
        if (obj.sourceType == this.childType)
        {
            m_childPool.Store(obj);
        }
        if (obj.sourceType == this.titleType)
        {
            m_titlePool.Store(obj);
        }
    }
    /// <summary>
    /// 对项脚本进行绑定赋值
    /// </summary>
    /// <param name="index"></param>
    /// <param name="obj"></param>
    public void SetObjViewByIndex(int index, ListViewItem obj)
    {
        if (this.childSource != null && this.titleSource != null)
        {
            var itemIndex = -1;
            if (childSource.TryGetValue(index, out itemIndex))
            {
                obj.FillItem(this.childList, itemIndex);
            }
            if (titleSource.TryGetValue(index, out itemIndex))
            {
                obj.FillItem(this.titleList, itemIndex);
            }
        }
    }
    /// <summary>
    /// 设置组件
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

    }

    #region pool 沿用现有池对象,清理需要和大师比对
    private GameObject m_poolGo;
    private ObjectPool<ListViewItem> m_childPool = new ObjectPool<ListViewItem>();
    private ObjectPool<ListViewItem> m_titlePool = new ObjectPool<ListViewItem>();
    private ListViewItem CreateChild()
    {
        GameObject itemGO = GameObject.Instantiate(child);
        var item = itemGO.AddComponent(childType) as ListViewItem;
        item.OnSourceData(grid, childType);
        item.FindItem();
        if (null != itemGO)
        {
            itemGO.gameObject.SetActive(false);
        }
        return item;
    }
    private ListViewItem CreateTitle()
    {
        GameObject itemGO = GameObject.Instantiate(title);
        var item = itemGO.AddComponent(titleType) as ListViewItem;
        item.OnSourceData(grid, titleType);
        item.FindItem();
        if (null != itemGO)
        {
            itemGO.gameObject.SetActive(false);
        }
        return item;
    }
    private void Init(ListViewItem item)
    {
        item.gameObject.SetActive(false);
        item.gameObject.transform.parent = m_poolGo.transform;
    }
    private void CreateItemPool(int poolNum)
    {
        if (m_poolGo != null) return;
        m_poolGo = new GameObject();
        m_poolGo.name = "pool";
        m_poolGo.transform.localScale = Vector3.zero;
        m_poolGo.transform.parent = grid.transform.parent;

        m_childPool.Init(poolNum, CreateChild, Init);
        m_titlePool.Init(poolNum, CreateTitle, Init);

        if (Application.isPlaying)
        {
            for (int i = 0, imax = grid.transform.childCount; i < imax; i++)
            {
                var childTrans = grid.transform.GetChild(0);
                if (childTrans != null && childTrans.gameObject != null)
                {
                    var item = childTrans.gameObject.GetComponent(childType) as ListViewItem;
                    if (item != null)
                    {
                        m_childPool.Store(item);
                    }
                    item = childTrans.gameObject.GetComponent(titleType) as ListViewItem;
                    if (item != null)
                    {
                        m_titlePool.Store(item);
                    }
                }
            }
        }
    }
    private ListViewItem GetChildItem()
    {
        return m_childPool.GetObject();
    }
    private ListViewItem GetTitleItem()
    {
        return m_titlePool.GetObject();
    }














    #endregion
}


