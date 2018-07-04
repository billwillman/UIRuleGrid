using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// 继承框体,原本设计应该是走构造传传递控制器的方式，现在阶段为了适应当前ui设计模式，故改为相互引用
/// 已经bug：由于过快往返拖动，导致UIScrollView内onpress方法没有办法重置false，属于底层问题，偶现不卡死，暂时不修复。
/// 现阶段支持左->右运动方向，上->下运动方式，其他模式使用镜像处理
/// </summary>
public class RulerGrid : UIWidget
{
    #region 暴露属性
    /// <summary>
    /// 间距设置,我觉得还是统一吧，不做暴露
    /// </summary>
    public int spaceValue = 2;
    public int childspaceValue = 2;//加一个子项间距的控制
    #endregion

    #region 缓存对象
    /// <summary>
    /// 界面持有
    /// </summary>
    public MonoBehaviour handleUI { get; set; }
    /// <summary>
    /// 管理器持有
    /// </summary>
    public IRuler ruler { get; set; }
    /// <summary>
    /// 初始化位置信息
    /// </summary>
    private Vector3 pos { get; set; }
    /// <summary>
    /// 滚动组件
    /// </summary>
    public UIScrollView scrollView { get; set; }
    /// <summary>
    /// 显示区域组件
    /// </summary>
    public UIPanel panelscroll { get; set; }
    /// <summary>
    /// 标尺数据,二维数组，长度是2的范围数组
    /// </summary>
    public float[,] rulerData { get; set; }
    /// <summary>
    /// 当前的显示数组
    /// </summary>
    private List<int> rulerIndexs = new List<int>();
    /// <summary>
    /// 当前显示对象,与上面数组一一对应
    /// </summary>
    private List<ListViewItem> objs = new List<ListViewItem>();
    /// <summary>
    /// 回收时候,临时变量
    /// </summary>
    private List<int> tempIndex = new List<int>();
    /// <summary>
    /// 回收时候,临时变量
    /// </summary>
    private List<ListViewItem> tempObj = new List<ListViewItem>();
    /// <summary>
    /// 执行帧
    /// </summary>
    private Action<RulerGrid> onUpdateAction { get; set; }
    /// <summary>
    /// 无滚动状态
    /// </summary>
    private bool noScrolling { get; set; }
    /// <summary>
    /// 显示区域长度
    /// </summary>
    public float viewLen { get; set; }
    /// <summary>
    /// 克隆个数
    /// </summary>
    private int cloneCount { get; set; }
    /// <summary>
    /// 克隆个数上限
    /// </summary>
    public int cloneMax { get; set; }
    /// <summary>
    /// 朝向指示
    /// </summary>
    private Vector3 forward { get; set; }
    /// <summary>
    /// 项的平均长度
    /// </summary>
    private float averageLength { get; set; }
    /// <summary>
    /// 显示下标
    /// </summary>
    private int showIndex_Start { get; set; }
    /// <summary>
    /// 显示下标
    /// </summary>
    private int showIndex_End { get; set; }
    /// <summary>
    /// 速度记录时间
    /// </summary>
    private float time { get; set; }
    /// <summary>
    /// 速度值
    /// </summary>
    private float speed { get; set; }
    /// <summary>
    /// 等待逻辑绑定列表
    /// </summary>
    private Dictionary<ListViewItem, int> waitDic = new Dictionary<ListViewItem, int>();
    /// <summary>
    /// 最大位移数目
    /// </summary>
    private float maxOffest { get; set; }
    /// <summary>
    /// 记录当前位移
    /// </summary>
    private float offest { get; set; }
    /// <summary>
    /// 初始化缩放系数
    /// </summary>
    private Vector3 onInitScale = Vector3.one;
    /// <summary>
    /// 是否在创建中
    /// </summary>
    private bool isCreating { get; set; }
    /// <summary>
    /// 是否创建完成后进行一次刷新
    /// </summary>
    private bool isReflashAfterCreating { get; set; }
    /// <summary>
    /// 是否在刷新中
    /// </summary>
    private bool isReflashing { get; set; }
    /// <summary>
    /// 是否刷新完成后进行一次刷新
    /// </summary>
    private bool isReflashAfterReflashing { get; set; }
    /// <summary>
    /// 是否倒着初始化
    /// </summary>
    private bool isInitByMax { get; set; }
    private bool isEndOfList { get; set; }
    /// <summary>
    /// 仅聊天使用
    /// </summary>
    public Action OnCreateAction { get; set; }
    /// <summary>
    /// 仅聊天使用
    /// </summary>
    public Action OnReflashAction { get; set; }

    /// <summary>
    /// 设置刻度对象
    /// </summary>
    /// <param name="ruler"></param>
    public void SetRuler(IRuler ruler)
    {
        this.ruler = ruler;
    }
    /// <summary>
    /// 尝试创建，生成缓存数据
    /// </summary>
    public void TryCreat()
    {
        if (!this.gameObject.activeInHierarchy)
        {
            return;
        }
        if (this.ruler == null)
        {
            return;
        }
        this.isInitByMax = false;
        this.isEndOfList = false;
        this.ResetSelectKey();
        this.isCreating = true;
        this.isReflashing = false;
        /////对数据进行赋值
        this.scrollView = this.transform.parent.GetComponent<UIScrollView>();
        if (this.scrollView != null)
        {
            ///避免惯性过大
            this.scrollView.momentumAmount = 10;
        }
        else
        {
            return;
        }
        this.panelscroll = this.transform.parent.GetComponent<UIPanel>();
        if (this.panelscroll != null)
        {
            this.pos = this.panelscroll.transform.localPosition;
        }
        else
        {
            return;
        }
        OnMoveAreaCreate(this);
        this.rulerData = ruler.GetRulerData();
        if (this.OnCreateAction != null)
        {
            OnCreateAction();
        }
        this.SetOnActionDone(this.OnCreatDone);
        ///去除这两步
        this.DoStep(CreaterSteps);
    }
    /// <summary>
    /// 创建完成
    /// </summary>
    private void OnCreatDone()
    {
        this.isCreating = false;
        if (this.isReflashAfterCreating)
        {
            //Debug.LogError("补偿刷新");
            this.isReflashAfterCreating = false;
            TryReflash();
        }
    }
    /// <summary>
    /// 尝试刷新
    /// </summary>
    public void TryReflash()
    {
        if (!this.gameObject.activeInHierarchy)
        {
            return;
        }
        if (this.isCreating)
        {
            this.isReflashAfterCreating = true;
            return;
        }
        if (this.isReflashing)
        {
            this.isReflashAfterReflashing = true;
            return;
        }

        this.isReflashing = true;

        if (this.ruler == null)
        {
            return;
        }
        if (scrollView == null || this.panelscroll == null)
        {
            TryCreat();
            return;
        }
        this.rulerData = ruler.GetRulerData();
        this.OnBreadMoving();
        if (this.OnReflashAction != null)
        {
            OnReflashAction();
        }
        this.SetOnActionDone(this.OnReflashDone);
        this.DoStep(ReflasherSteps);
    }
    /// <summary>
    /// 刷新结束
    /// </summary>
    private void OnReflashDone()
    {
        this.isReflashing = false;
        if (this.isReflashAfterReflashing)
        {
            //Debug.LogError("补偿刷新");
            this.isReflashAfterReflashing = false;
            TryReflash();
        }
    }
    /// <summary>
    /// 获取整个刻度的长度
    /// </summary>
    public float GetListLength()
    {
        var index = GetListCount() - 1;
        return this.rulerData[index, 1];
    }
    /// <summary>
    /// 获取列表个数
    /// </summary>
    /// <returns></returns>
    public int GetListCount()
    {
        return rulerData.Length / 2;
    }
    /// <summary>
    /// 获取每项平均长度
    /// </summary>
    private float GetAverageLength()
    {
        var count = GetListCount();
        return this.rulerData[count - 1, 1] / count;
    }
    /// <summary>
    /// 销毁时清空关系
    /// </summary>
    public void OnDestroy()
    {
        if (this.scrollView != null)
        {
            this.scrollView.onDragStarted = null;
            this.scrollView.onStoppedMoving = null;
        }
        this.scrollView = null;
        this.rulerData = null;
        this.panelscroll = null;
        this.onUpdateAction = null;
        this.noScrolling = false;
        this.rulerIndexs.Clear();
        this.objs.Clear();
    }
    /// <summary>
    /// 重置回初始状态
    /// </summary>
    public void ResetToInit()
    {
        OnBreadMoving();
        ResetInitPosition();
        ClearAllChildren();
    }
    /// <summary>
    /// 重置初始化坐标位置
    /// </summary>
    private void ResetInitPosition()
    {
        if (this.scrollView != null)
        {
            this.scrollView.DisableSpring();
        }
        if (this.panelscroll != null)
        {
            this.panelscroll.transform.localPosition = this.pos;
            this.panelscroll.clipOffset = Vector2.zero;
        }
    }
    /// <summary>
    /// 缩放系数特殊处理,平时很少调用
    /// </summary>
    public void SetInitScale(Vector3 scale)
    {
        this.onInitScale = scale;
    }
    /// <summary>
    /// 池回收所有对象,这里有个逻辑顺序bug，要求清理函数必须早于ruler.RecyclingCloneObjByIndex()调用不然会造成池回收错误
    /// </summary>
    private void ClearAllChildren()
    {
        if (ruler != null)
        {
            int len = this.objs.Count;
            for (int i = 0; i < len; i++)
            {
                var obj = this.objs[i];
                if (obj != null)
                {
                    ruler.RecyclingCloneObjByIndex(obj);
                }
            }
        }
        this.rulerIndexs.Clear();
        this.objs.Clear();
        this.tempIndex.Clear();
        this.tempObj.Clear();
        this.waitDic.Clear();
    }
    /// <summary>
    /// 打断滚动
    /// </summary>
    private void OnBreadMoving()
    {
        this.onUpdateAction = null;
        this.noScrolling = false;
        if (this.scrollView != null)
        {
            this.scrollView.StopDragMove();
            this.scrollView.onDragStarted = null;
            this.scrollView.onStoppedMoving = null;
        }
    }
    /// <summary>
    /// 设置持有UI对象
    /// </summary>
    /// <param name="ui"></param>
    public void RegistHandleUIClass(MonoBehaviour ui)
    {
        this.handleUI = ui;
    }
    /// <summary>
    /// 移动到谷底
    /// </summary>
    public void MoveToEnd()
    {
        var value = GetListLength();
        if (value <= viewLen)
        {
            return;
        }
        ///滚到底部
        switch (this.scrollView.movement)
        {
            ///水平
            case UIScrollView.Movement.Horizontal:
                this.panelscroll.transform.localPosition = this.pos + Vector3.right * (value - viewLen);
                this.panelscroll.clipOffset = Vector2.left * (value - viewLen);
                break;
            ///垂直
            case UIScrollView.Movement.Vertical:
                this.panelscroll.transform.localPosition = this.pos + Vector3.up * (value - viewLen);
                this.panelscroll.clipOffset = Vector2.down * (value - viewLen);
                break;
            default:
                throw new Exception("未定义模式");
        }

    }
    /// <summary>
    /// 最大值初始化,trycreat后调用
    /// </summary>
    public void InitByMax()
    {
        this.isInitByMax = true;
        SetEndOfList(true);
    }
    /// <summary>
    /// 设置到达尾部状态
    /// </summary>
    /// <param name="value"></param>
    private void SetEndOfList(bool value)
    {
        if (this.isEndOfList != value)
        {
            this.isEndOfList = value;
            this.ruler.OnScollEndOfList(this.isEndOfList);
        }
    }
    /// <summary>
    /// 创建滑动区域对象
    /// </summary>
    private static void OnMoveAreaCreate(RulerGrid grid)
    {
        var size = grid.panelscroll.GetViewSize();
        var clipSize = Vector2.zero;
        if (grid.panelscroll.clipping == UIDrawCall.Clipping.SoftClip)
        {
            clipSize = grid.panelscroll.clipSoftness;
        }
        ///撑满整个区域
        grid.pivot = Pivot.Center;
        grid.gameObject.transform.localPosition = Vector3.zero;
        grid.height = (int)(size.y - clipSize.y * 2);
        grid.width = (int)(size.x - clipSize.x * 2);

        ///根据滚动方向设置
        switch (grid.scrollView.movement)
        {
            ///水平
            case UIScrollView.Movement.Horizontal:
                grid.pivot = Pivot.Left;
                grid.viewLen = grid.width;
                grid.forward = Vector3.right;
                break;
            ///垂直
            case UIScrollView.Movement.Vertical:
                grid.pivot = Pivot.Top;
                grid.viewLen = grid.height;
                grid.forward = Vector3.down;
                break;
            default:
                throw new Exception("未定义模式");
        }
    }
    #region select Key
    private int? selectKey_Int { get; set; }
    private uint? selectKey_Uint { get; set; }
    private string selectKey_Str { get; set; }
    private long? selectKey_long { get; set; }
    private ulong? selectKey_ulong { get; set; }


    /// <summary>
    /// 创建时候重置下标
    /// </summary>
    public void ResetSelectKey()
    {
        this.selectKey_Int = null;
        this.selectKey_Uint = null;
        this.selectKey_Str = string.Empty;
        this.selectKey_long = null;
        this.selectKey_ulong = null;
    }
    /// <summary>
    /// 设置子项标签,创建后调用方可生效
    /// </summary>
    /// <param name="index"></param>
    public void SetChildState(int index)
    {
        this.selectKey_Int = index;
    }
    /// <summary>
    /// 根据下标判断
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool GetChildState(int index)
    {
        return this.selectKey_Int == index;
    }
    /// <summary>
    /// 设置子项标签,创建后调用方可生效
    /// </summary>
    /// <param name="index"></param>
    public void SetChildState(uint index)
    {
        this.selectKey_Uint = index;
    }
    /// <summary>
    /// 根据下标判断
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool GetChildState(uint index)
    {
        return this.selectKey_Uint == index;
    }
    /// <summary>
    /// 设置子项标签,创建后调用方可生效
    /// </summary>
    /// <param name="index"></param>
    public void SetChildState(string index)
    {
        this.selectKey_Str = index;
    }
    /// <summary>
    /// 根据下标判断
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool GetChildState(string index)
    {
        return this.selectKey_Str == index;
    }
    /// <summary>
    /// 设置子项标签,创建后调用方可生效
    /// </summary>
    /// <param name="index"></param>
    public void SetChildState(long index)
    {
        this.selectKey_long = index;
    }
    /// <summary>
    /// 根据下标判断
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool GetChildState(long index)
    {
        return this.selectKey_long == index;
    }
    /// <summary>
    /// 设置子项标签,创建后调用方可生效
    /// </summary>
    /// <param name="index"></param>
    public void SetChildState(ulong index)
    {
        this.selectKey_ulong = index;
    }
    /// <summary>
    /// 根据下标判断
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool GetChildState(ulong index)
    {
        return this.selectKey_ulong == index;
    }

    #endregion

    #endregion

    #region 逐帧
    /// <summary>
    /// 是否有完成触发
    /// </summary>
    private Action OnActionDone { get; set; }
    private int stepIndex { get; set; }
    private Action<RulerGrid>[] actions { get; set; }
    /// <summary>
    /// 事件
    /// </summary>
    /// <param name="OnActionDone"></param>
    private void SetOnActionDone(Action OnActionDone)
    {
        this.OnActionDone = OnActionDone;
    }
    /// <summary>
    /// 设置事件
    /// </summary>
    private void DoNext()
    {
        if (this.stepIndex >= actions.Length || actions == null)
        {
            return;
        }
        this.onUpdateAction = actions[this.stepIndex];
        this.stepIndex++;
    }
    /// <summary>
    /// 跳过
    /// </summary>
    /// <param name="value"></param>
    private void DoSkip(int value = 1)
    {
        this.stepIndex += value;
    }
    /// <summary>
    /// 重置执行下标
    /// </summary>
    private void ResetIndex()
    {
        this.stepIndex = 0;
    }
    /// <summary>
    /// 执行步骤
    /// </summary>
    private void DoStep(Action<RulerGrid>[] actions)
    {
        this.stepIndex = 0;
        this.actions = actions;
        DoNext();
    }
    /// <summary>
    /// 终止植帧循环
    /// </summary>
    private void Done()
    {
        this.onUpdateAction = null;
        if (this.OnActionDone != null)
        {
            //特殊处理
            var action = this.OnActionDone;
            this.OnActionDone = null;
            action();
        }
    }
    /// <summary>
    /// 控制器
    /// </summary>
    protected override void OnUpdate()
    {
        if (this.onUpdateAction != null && this != null)
        {
            this.onUpdateAction(this);
        }
        base.OnUpdate();
    }
    #endregion

    #region 参数
    /// <summary>
    /// 速度临界值
    /// </summary>
    private const int SpeedPoint = 1500;
    /// <summary>
    /// 拷贝缓存对象
    /// </summary>
    private const int CloneCache = 4;
    #endregion

    #region 流程控制器
    /// <summary>
    /// 创建流程
    /// </summary>
    private static Action<RulerGrid>[] CreaterSteps = new Action<RulerGrid>[4]
    {
        SetBackgroudLength,
        OnCloneStep,
        StartClone,
        OnScrollingStep,
    };
    /// <summary>
    /// 刷新流程
    /// </summary>
    private static Action<RulerGrid>[] ReflasherSteps = new Action<RulerGrid>[5]
    {
        SetBackgroudLength,
        OnCloneStep,
        CheckClone,
        CheckPosition,
        OnScrollingStep,
    };
    /// <summary>
    /// 滚动流程
    /// </summary>
    private static Action<RulerGrid>[] ScrollingSteps = new Action<RulerGrid>[3]
    {
        OnCalculationIndex,
        MoveObjPos,
        OnChildViewReflash,
    };
    #endregion

    #region 初始化阶段
    /// <summary>
    /// 设置背板长度
    /// </summary>
    /// <param name="grid"></param>
    private static void SetBackgroudLength(RulerGrid grid)
    {
        if (grid == null)
        {
            return;
        }
        grid.maxOffest = grid.GetListLength();
        grid.noScrolling = false;
        ///根据滚动方向设置
        switch (grid.scrollView.movement)
        {
            ///水平
            case UIScrollView.Movement.Horizontal:
                if (grid.maxOffest > grid.viewLen)
                {
                    grid.width = (int)grid.maxOffest + 1;
                }
                else
                {
                    grid.width = (int)grid.viewLen;
                    grid.noScrolling = true;
                }
                break;
            ///垂直
            case UIScrollView.Movement.Vertical:
                if (grid.maxOffest > grid.viewLen)
                {
                    grid.height = (int)grid.maxOffest + 1;
                }
                else
                {
                    grid.height = (int)grid.viewLen;
                    grid.noScrolling = true;
                }
                break;
            default:
                throw new Exception("未定义模式");
        }
        grid.averageLength = grid.GetAverageLength();
        ///是否需要滑动
        if (grid.noScrolling)
        {
            grid.scrollView.enabled = false;
            grid.ResetInitPosition();
        }
        else
        {
            grid.scrollView.enabled = true;
        }
        OnInitStepFisnished(grid);
    }
    /// <summary>
    /// 第一阶段执行完毕
    /// </summary>
    private static void OnInitStepFisnished(RulerGrid grid)
    {
        if (grid == null)
        {
            return;
        }
        grid.DoNext();
    }
    #endregion

    #region 拷贝阶段
    /// <summary>
    /// 刷新时候是否需要执行clone
    /// </summary>
    /// <param name="grid"></param>
    private static void CheckClone(RulerGrid grid)
    {
        if (grid == null)
        {
            return;
        }
        float offest = GetOffest(grid, false);
        grid.offest = offest;
        int index, start, end = 0;
        var maxCount = grid.GetListCount();
        GetIndex(grid, maxCount, out index, out start, out end);

        ///逐检测一次
        for (int i = start; i < end; i++)
        {
            ///需要构建
            if (grid != null && grid.ruler != null)
            {
                OnCloneMethod(grid, i);
            }
        }

        OnCloneTempClean(grid);
        grid.DoNext();
    }
    /// <summary>
    /// 需要一帧校对位置
    /// </summary>
    /// <param name="grid"></param>
    private static void CheckPosition(RulerGrid grid)
    {
        if (grid == null)
        {
            return;
        }
        //系统级别有问题
        //grid.scrollView.RestrictWithinBounds(true);
        OffestProtect(grid, true);
        grid.DoNext();
    }

    /// <summary>
    /// 初始化阶段
    /// </summary>
    private static void OnCloneStep(RulerGrid grid)
    {
        if (grid == null)
        {
            return;
        }
        ///计算一个长度view显示几个
        var len = grid.viewLen;
        var count = grid.GetListCount();
        grid.cloneMax = -1;
        grid.cloneCount = 0;
        for (int i = 0; i < count; i++)
        {
            if (grid.rulerData[i, 0] > len || grid.rulerData[i, 1] > len)
            {
                ///克隆个数确认
                grid.cloneMax = i + CloneCache;
                break;
            }
        }
        if (grid.cloneMax == -1)
        {
            grid.cloneMax = count;
        }
        if (grid.cloneMax >= count)
        {
            grid.cloneMax = count;
        }
        grid.offest = 0;
        ///缓存数据到缓存区域
        grid.tempIndex.Clear();
        grid.tempObj.Clear();
        grid.tempIndex.AddRange(grid.rulerIndexs);
        grid.tempObj.AddRange(grid.objs);
        grid.rulerIndexs.Clear();
        grid.objs.Clear();

        grid.DoNext();
    }
    /// <summary>
    /// 开始拷贝
    /// </summary>
    /// <param name="grid"></param>
    private static void StartClone(RulerGrid grid)
    {
        if (grid == null)
        {
            return;
        }
        if (grid.cloneCount < grid.cloneMax)
        {
            if (grid.ruler != null)
            {
                if (grid.isInitByMax)
                {
                    var index = grid.GetListCount() - 1 - grid.cloneCount;
                    if (index >= 0)
                    {
                        OnCloneMethod(grid, index);
                    }
                }
                else
                {
                    OnCloneMethod(grid, grid.cloneCount);
                }
            }
            grid.cloneCount++;
        }
        else
        {
            ///回收不使用的对象
            OnCloneTempClean(grid);
            OnCloneStepFinished(grid);
        }
    }
    /// <summary>
    /// 拷贝阶段结束
    /// </summary>
    private static void OnCloneStepFinished(RulerGrid grid)
    {
        if (grid == null)
        {
            return;
        }
        if (grid.noScrolling)
        {
            grid.Done();
            return;
        }
        ///完成拷贝
        grid.DoNext();
    }
    /// <summary>
    /// 拷贝方法
    /// </summary>
    private static void OnCloneMethod(RulerGrid grid, int index)
    {
        if (grid == null)
        {
            return;
        }

        ListViewItem item = null;

        if (grid.tempIndex.Contains(index))
        {
            ///去缓存对象
            int i = grid.tempIndex.IndexOf(index);
            item = grid.tempObj[i];

            if (item == null || (item.sourceType != grid.ruler.GetCloneTypeByIndex(index)))
            {
                ///拷贝对象类型不一致
                if (item != null)
                {
                    item.gameObject.SetActive(false);
                }
                ///重新拷贝
                item = grid.ruler.GetCloneObjByIndex(index);
                item.transform.parent = grid.transform;
            }
            else
            {
                grid.tempObj[i] = null;
            }
        }
        else
        {
            item = grid.ruler.GetCloneObjByIndex(index);
            item.transform.parent = grid.transform;
        }


        item.transform.localScale = grid.onInitScale;
        item.transform.localPosition = grid.forward * grid.rulerData[index, 0];
        item.gameObject.SetActive(true);
        grid.ruler.SetObjViewByIndex(index, item);
        grid.rulerIndexs.Add(index);
        grid.objs.Add(item);
    }
    /// <summary>
    /// 清空缓存的对象
    /// </summary>
    /// <param name="grid"></param>
    private static void OnCloneTempClean(RulerGrid grid)
    {
        if (grid == null)
        {
            return;
        }

        ///回收不使用的对象
        int len = grid.tempIndex.Count;
        for (int i = 0; i < len; i++)
        {
            var item = grid.tempObj[i];
            if (item == null)
            {
                continue;
            }
            item.gameObject.SetActive(false);
            grid.ruler.RecyclingCloneObjByIndex(item);
        }
        grid.tempIndex.Clear();
        grid.tempObj.Clear();
    }
    #endregion

    #region 滚动阶段

    /// <summary>
    /// 初始化阶段
    /// </summary>
    private static void OnScrollingStep(RulerGrid grid)
    {
        if (grid == null)
        {
            return;
        }
        ///这里可以考虑优化一下
        grid.scrollView.onDragStarted = () =>
        {
            grid.time = Time.realtimeSinceStartup;
            grid.speed = 0;
            grid.waitDic.Clear();
            grid.DoStep(ScrollingSteps);
        };
        grid.scrollView.onStoppedMoving = () =>
        {
            grid.Done();
        };
        grid.Done();
    }
    /// <summary>
    /// 计算刷新坐标
    /// </summary>
    private static void OnCalculationIndex(RulerGrid grid)
    {
        if (grid == null)
        {
            return;
        }
        float offest = GetOffest(grid, true);
        ///减少计算次数
        if (Math.Abs(offest - grid.offest) <= grid.averageLength * 0.1f)
        {
            if (grid.waitDic.Count > 0)
            {
                ///跳过
                grid.DoSkip();
                grid.DoNext();
            }
            return;
        }
        ///计算出朝向
        bool isForward = false;
        switch (grid.scrollView.movement)
        {
            ///水平
            case UIScrollView.Movement.Horizontal:
                isForward = offest > grid.offest;
                break;
            ///垂直
            case UIScrollView.Movement.Vertical:
                isForward = offest < grid.offest;
                break;
            default:
                throw new Exception("未定义模式");
        }
        ///这里计算速度存在bug,存在数值浮动，不是线性
        var timeValue = Time.realtimeSinceStartup - grid.time;
        grid.time = Time.realtimeSinceStartup;
        var speed = Math.Abs(offest - grid.offest) / timeValue;
        grid.speed = speed;
        grid.offest = offest;
        ///估算出更新对象
        int index, start, end = 0;
        var maxCount = grid.GetListCount();
        GetIndex(grid, maxCount, out index, out start, out end);

        for (int i = start; i < index; i++)
        {
            if (grid.rulerData[i, 0] > offest || grid.rulerData[i, 1] >= offest)
            {
                grid.showIndex_Start = i;
                break;
            }
        }
        for (int i = end - 1; i >= index; i--)
        {
            if (grid.rulerData[i, 0] < offest + grid.viewLen || grid.rulerData[i, 1] < offest + grid.viewLen)
            {
                grid.showIndex_End = i;
                break;
            }
        }
        int limitedCount = (int)(grid.viewLen / grid.averageLength) + 1;
        ///补偿对象
        if (isForward)
        {
            grid.showIndex_End += 4;
            if (grid.showIndex_End >= maxCount)
            {
                grid.showIndex_End = maxCount - 1;
            }
            var value = grid.showIndex_End - limitedCount;
            if (value < grid.showIndex_Start)
            {
                grid.showIndex_Start = value;
            }
            if (grid.showIndex_Start < 0)
            {
                grid.showIndex_Start = 0;
            }
        }
        else
        {
            grid.showIndex_Start -= 4;
            if (grid.showIndex_Start < 0)
            {
                grid.showIndex_Start = 0;
            }
            var value = grid.showIndex_Start + limitedCount;
            if (value > grid.showIndex_End)
            {
                grid.showIndex_End = value;
            }
            if (grid.showIndex_End >= maxCount)
            {
                grid.showIndex_End = maxCount - 1;
            }
        }
        ///暂时不分帧
        grid.DoNext();
        //MoveObjPos(grid);
    }
    /// <summary>
    /// 计算偏移量
    /// </summary>
    /// <param name="grid"></param>
    private static float GetOffest(RulerGrid grid, bool isMoveing)
    {
        float offest = 0;
        ///根据滚动方向设置
        switch (grid.scrollView.movement)
        {
            ///水平
            case UIScrollView.Movement.Horizontal:
                offest = grid.panelscroll.clipOffset.x;
                break;
            ///垂直
            case UIScrollView.Movement.Vertical:
                offest = grid.panelscroll.clipOffset.y * -1;
                break;
            default:
                throw new Exception("未定义模式");
        }
        if (offest < 0)
        {
            offest = 0;
        }
        float max = grid.maxOffest - grid.viewLen;
        if (offest > max)
        {
            offest = max;
        }
        if (isMoveing)
        {
            if (offest >= max - 5)
            {
                grid.SetEndOfList(true);
            }
            else
            {
                grid.SetEndOfList(false);
            }
        }
        return offest;
    }
    /// <summary>
    /// 坐标保护
    /// </summary>
    /// <param name="grid"></param>
    private static void OffestProtect(RulerGrid grid, bool isStory)
    {
        if (grid == null)
        {
            return;
        }
        if (grid.scrollView.enabled == false)
        {
            return;
        }
        float offest = 0;
        float max = grid.maxOffest - grid.viewLen;
        ///根据滚动方向设置
        switch (grid.scrollView.movement)
        {
            ///水平
            case UIScrollView.Movement.Horizontal:
                offest = grid.panelscroll.clipOffset.x;
                if (offest > max)
                {
                    if (isStory)
                    {
                        Vector3 pos = grid.panel.transform.localPosition + (offest - max) * Vector3.right;
                        pos.x = Mathf.Round(pos.x);
                        pos.y = Mathf.Round(pos.y);
                        SpringPanel.Begin(grid.scrollView.gameObject, pos, 13f).strength = 8f;
                    }
                    else
                    {
                        grid.scrollView.MoveRelative((offest - max) * Vector3.right);
                    }
                }

                break;
            ///垂直
            case UIScrollView.Movement.Vertical:
                offest = grid.panelscroll.clipOffset.y * -1;
                if (offest > max)
                {
                    if (isStory)
                    {
                        Vector3 pos = grid.panel.transform.localPosition + (max - offest) * Vector3.up;
                        pos.x = Mathf.Round(pos.x);
                        pos.y = Mathf.Round(pos.y);
                        SpringPanel.Begin(grid.scrollView.gameObject, pos, 13f).strength = 8f;
                    }
                    else
                    {
                        grid.scrollView.MoveRelative((max - offest) * Vector3.up);
                    }
                }
                break;
            default:
                throw new Exception("未定义模式");
        }

    }
    /// <summary>
    /// 计算出预估下标
    /// </summary>
    /// <returns></returns>
    private static void GetIndex(RulerGrid grid, int maxCount, out int index, out int start, out int end)
    {
      
        index = (int)((grid.offest + grid.viewLen * 0.5f) / grid.averageLength);
        if (index > maxCount)
        {
            index = maxCount;
        }
        else if (index < 0)
        {
            index = 0;
        }
        ///当前位置向前，向后索引grid.cloneMax个数，可以缩小最大取值范围提升执行效率
        start = index - grid.ruler.IndexSearchRange();
        if (start < 0)
        {
            start = 0;
        }
        end = index + grid.ruler.IndexSearchRange();
        if (end >= maxCount)
        {
            end = maxCount;
        }
    }
    /// <summary>
    /// 移动预制件，伸缩惯性
    /// </summary>
    /// <param name="grid"></param>
    private static void MoveObjPos(RulerGrid grid)
    {
        if (grid == null)
        {
            return;
        }

        grid.tempIndex.Clear();
        grid.tempObj.Clear();

        ///回收对象
        int len = grid.rulerIndexs.Count;
        for (int i = 0; i < len; i++)
        {
            var value = grid.rulerIndexs[i];
            if (value >= grid.showIndex_Start && value <= grid.showIndex_End)
            {
                ///无需处理
            }
            else
            {
                grid.tempIndex.Add(value);
                grid.tempObj.Add(grid.objs[i]);
            }
        }

        grid.rulerIndexs.RemoveAll((p) => grid.tempIndex.Contains(p));
        grid.objs.RemoveAll((p) => grid.tempObj.Contains(p));

        bool isNeedViewUpdate = false;

        if (grid.ruler != null)
        {
            var count = grid.tempIndex.Count;
            for (int i = 0; i < count; i++)
            {
                var obj = grid.tempObj[i];
                grid.ruler.RecyclingCloneObjByIndex(obj);
            }
            grid.tempIndex.Clear();
            grid.tempObj.Clear();


            ///找出需要移动的对象
            for (int i = grid.showIndex_Start; i <= grid.showIndex_End; i++)
            {
                if (!grid.rulerIndexs.Contains(i))
                {
                    var obj = grid.ruler.GetCloneObjByIndex(i);
                    obj.transform.parent = grid.transform;
                    obj.transform.localScale = grid.onInitScale;
                    obj.transform.localPosition = grid.forward * grid.rulerData[i, 0];
                    obj.gameObject.SetActive(true);
                    if (grid.speed > SpeedPoint)
                    {
                        if (grid.waitDic.ContainsKey(obj))
                        {

                            grid.waitDic[obj] = i;
                        }
                        else
                        {
                            grid.waitDic.Add(obj, i);
                        }
                    }
                    else
                    {
                        if (grid.waitDic.ContainsKey(obj))
                        {
                            grid.waitDic.Remove(obj);
                        }
                        grid.ruler.SetObjViewByIndex(i, obj);
                    }
                    grid.rulerIndexs.Add(i);
                    grid.objs.Add(obj);
                }
            }
        }
        if (grid.speed <= SpeedPoint && grid.waitDic.Count > 0)
        {
            isNeedViewUpdate = true;
        }
        if (isNeedViewUpdate)
        {
            grid.DoNext();
        }
        else
        {
            grid.ResetIndex();
            grid.DoNext();
        }
    }
    /// <summary>
    /// 刷新一次子项显示
    /// </summary>
    /// <param name="grid"></param>
    private static void OnChildViewReflash(RulerGrid grid)
    {
        if (grid == null)
        {
            return;
        }
        if (grid.waitDic.Count > 0)
        {
            var enumerator = grid.waitDic.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var index = enumerator.Current.Value;
                var obj = enumerator.Current.Key;
                if (obj.transform.parent == grid.transform)
                {

                    if (grid.ruler != null && obj != null)
                    {
                        grid.ruler.SetObjViewByIndex(index, obj);
                    }
                }
            }
        }
        grid.waitDic.Clear();
        grid.ResetIndex();
        grid.DoNext();
    }
    #endregion
}


