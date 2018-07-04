using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 为了配合现有代码逻辑，不产生过量修改，这里的继承关系，改为相互引用，如果参考使用，建议写成继承关系或者组件关系,单向的
/// </summary>
public static class ListViewCreater
{
    #region 创建列表


    /// <summary>
    /// 创建基本循环列表对象
    /// </summary>
    /// <typeparam name="T">子项类型</typeparam>
    /// <param name="grid">控件对象</param>
    /// <param name="source">数据源</param>
    /// <param name="type">子项赋值代码</param>
    /// <param name="template">子项模板</param>
    public static void CreateScrollView<T>(this RulerGrid grid, List<T> source, Type type, GameObject template)
    {
        if (grid.ruler == null)
        {
            grid.ruler = new BaseListView<T>();
            grid.ruler.SetRulerGrid(grid);
        }
        var creater = grid.ruler as ICreater<T>;
        if (creater != null)
        {
            creater.CreateScrollView(source, type, template);
        }
    }

    /// <summary>
    /// 创建标题循环列表对象,不带有收缩功能
    /// </summary>
    /// <typeparam name="T1">标题类型</typeparam>
    /// <typeparam name="T2">子项类型</typeparam>
    /// <param name="grid">控件对象</param>
    /// <param name="source">数据源</param>
    /// <param name="childType">子项赋值代码</param>
    /// <param name="child">子项模板</param>
    /// <param name="titleType">标签赋值代码</param>
    /// <param name="title">标签模板</param>
    public static void CreateScrollView<T1, T2>(this RulerGrid grid, Dictionary<T1, List<T2>> source, Type childType, GameObject child, Type titleType, GameObject title)
    {
        if (grid.ruler == null)
        {
            grid.ruler = new TitleListView<T1, T2>();
            grid.ruler.SetRulerGrid(grid);
        }
        var creater = grid.ruler as ICreater<T1, T2>;
        if (creater != null)
        {
            creater.CreateScrollView(source, childType, child, titleType, title);
        }
    }


    /// <summary>
    /// 创建标题循环列表对象,标签对象可以收缩
    /// </summary>
    /// <typeparam name="T1">标题类型</typeparam>
    /// <typeparam name="T2">子项类型</typeparam>
    /// <param name="grid">控件对象</param>
    /// <param name="source">数据源</param>
    /// <param name="childType">子项赋值代码</param>
    /// <param name="child">子项模板</param>
    /// <param name="titleType">标签赋值代码</param>
    /// <param name="title">标签赋值代码</param>
    /// <param name="state">标签初始化状态，填写null默认全部收起</param>
    public static void CreateScrollView<T1, T2>(this RulerGrid grid, Dictionary<T1, List<T2>> source, Type childType, GameObject child, Type titleType, GameObject title, List<bool> state)
    {
        if (grid.ruler == null)
        {
            grid.ruler = new TitleListView<T1, T2>();
            grid.ruler.SetRulerGrid(grid);
        }
        var creater = grid.ruler as ICreater<T1, T2>;
        if (creater != null)
        {
            creater.CreateScrollView(source, childType, child, titleType, title, state);
        }
    }

    #endregion

    #region 刷新列表

    /// <summary>
    /// 基本循环列表刷新
    /// </summary>
    /// <typeparam name="T">数据源类型</typeparam>
    /// <param name="grid">持有控件</param>
    /// <param name="source">数据源</param>
    public static void ReflashScrollView<T>(this RulerGrid grid, List<T> source)
    {
        if (grid.ruler == null)
        {
            return;
        }
        var creater = grid.ruler as ICreater<T>;
        if (creater != null)
        {
            creater.ReflashSource(source);
        }
    }
    /// <summary>
    /// 标签循环列表刷新
    /// </summary>
    /// <typeparam name="T1">标题数据类型</typeparam>
    /// <typeparam name="T2">子项数据类型</typeparam>
    /// <param name="grid">持有控件</param>
    /// <param name="source">数据源</param>
    public static void ReflashScrollView<T1, T2>(this RulerGrid grid, Dictionary<T1, List<T2>> source)
    {
        if (grid.ruler == null)
        {
            return;
        }
        var creater = grid.ruler as ICreater<T1, T2>;
        if (creater != null)
        {
            creater.ReflashSource(source);
        }
    }
    /// <summary>
    /// 标签循环列表刷新
    /// </summary>
    /// <typeparam name="T1">标题数据类型</typeparam>
    /// <typeparam name="T2">子项数据类型</typeparam>
    /// <param name="grid">持有控件</param>
    /// <param name="source">数据源</param>
    /// <param name="state">标签状态</param>
    public static void ReflashScrollView<T1, T2>(this RulerGrid grid, Dictionary<T1, List<T2>> source, List<bool> state)
    {
        if (grid.ruler == null)
        {
            return;
        }
        var creater = grid.ruler as ICreater<T1, T2>;
        if (creater != null)
        {
            creater.ReflashSource(source, state);
        }
    }
    /// <summary>
    /// 获取标题title状态
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="grid"></param>
    /// <param name="index"></param>
    public static bool GetTitleState<T1, T2>(this RulerGrid grid, int index)
    {
        if (grid.ruler == null)
        {
            return false;
        }
        var creater = grid.ruler as ICreater<T1, T2>;
        if (creater != null)
        {
            return creater.GetTitleState(index);
        }
        else
        {
            return false;
        }
    }

    #endregion

    /*
    #region 聊天
    /// <summary>
    /// 创建聊天循环列表对象
    /// </summary>
    /// <typeparam name="T">子项类型</typeparam>
    /// <param name="grid">控件对象</param>
    /// <param name="source">数据源</param>
    /// <param name="type">子项赋值代码</param>
    public static void CreateChatView<T>(this RulerGrid grid, List<T> source, Action<T, ChatItemTemplate> fillAction) where T : IChatMsg
    {
        if (grid.ruler == null)
        {
            grid.ruler = new ChatListView<T>();
            grid.ruler.SetRulerGrid(grid);
        }
        var creater = grid.ruler as IChatCreater<T>;
        if (creater != null)
        {
            creater.CreateScrollView(source, fillAction);
        }
    }
    /// <summary>
    /// 刷新聊天列表
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="grid"></param>
    /// <param name="source"></param>
    public static void ReflashChatView<T>(this RulerGrid grid, List<T> source) where T : IChatMsg
    {
        if (grid.ruler == null)
        {
            grid.ruler = new ChatListView<T>();
            grid.ruler.SetRulerGrid(grid);
        }
        var creater = grid.ruler as IChatCreater<T>;
        if (creater != null)
        {
            creater.ReflashSource(source);
        }
    }

    /// <summary>
    /// 注册组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="grid"></param>
    /// <param name="input"></param>
    /// <param name="ui"></param>
    public static void RegistChatView<T>(this RulerGrid grid, UIInput input, GameObject voice, UIBase ui, ChatKey key) where T : IChatMsg
    {
        if (grid.ruler == null)
        {
            grid.ruler = new ChatListView<T>();
            grid.ruler.SetRulerGrid(grid);
        }
        var creater = grid.ruler as IChatCreater<T>;
        if (creater != null)
        {
            creater.OnRegistration(input, voice, ui, key);
        }
    }
    /// <summary>
    /// 功能打开
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="grid"></param>
    public static void OnChatFuctionClick<T>(this RulerGrid grid) where T : IChatMsg
    {
        if (grid.ruler == null)
        {
            grid.ruler = new ChatListView<T>();
            grid.ruler.SetRulerGrid(grid);
        }
        var creater = grid.ruler as IChatCreater<T>;
        if (creater != null)
        {
            creater.OnFunctionClick();
        }
    }

    /// <summary>
    /// 发送聊天数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="grid"></param>
    /// <param name="datas"></param>
    /// <param name="kChatVo"></param>
    /// <param name="inputText"></param>
    public static void OnSendChat<T>(this RulerGrid grid, FriendVO vo) where T : IChatMsg
    {
        if (grid.ruler == null)
        {
            grid.ruler = new ChatListView<T>();
            grid.ruler.SetRulerGrid(grid);
        }
        var creater = grid.ruler as IChatCreater<T>;
        if (creater != null)
        {
            creater.OnSendChat(vo);
        }
    }

    /// <summary>
    /// 发送聊天数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="grid"></param>
    /// <param name="datas"></param>
    /// <param name="kChatVo"></param>
    /// <param name="inputText"></param>
    public static void OnSendChat<T>(this RulerGrid grid, BottleV0 vo) where T : IChatMsg
    {
        if (grid.ruler == null)
        {
            grid.ruler = new ChatListView<T>();
            grid.ruler.SetRulerGrid(grid);
        }
        var creater = grid.ruler as IChatCreater<T>;
        if (creater != null)
        {
            creater.OnSendChat(vo);
        }
    }

    /// <summary>
    /// 发送聊天数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="grid"></param>
    /// <param name="datas"></param>
    /// <param name="kChatVo"></param>
    /// <param name="inputText"></param>
    public static void OnSendChat<T>(this RulerGrid grid, long bottleId, string context, string voiceFilePath) where T : IChatMsg
    {
        if (grid.ruler == null)
        {
            grid.ruler = new ChatListView<T>();
            grid.ruler.SetRulerGrid(grid);
        }
        var creater = grid.ruler as IChatCreater<T>;
        if (creater != null)
        {
            creater.OnSendChat(bottleId, context, voiceFilePath);
        }
    }

    /// <summary>
    /// 设置聊天数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="grid"></param>
    /// <param name="datas"></param>
    /// <param name="kChatVo"></param>
    /// <param name="inputText"></param>
    public static void SetChatInputValue<T>(this RulerGrid grid, string value) where T : IChatMsg
    {
        if (grid.ruler == null)
        {
            grid.ruler = new ChatListView<T>();
            grid.ruler.SetRulerGrid(grid);
        }
        var creater = grid.ruler as IChatCreater<T>;
        if (creater != null)
        {
            creater.SetChatInputValue(value);
        }
    }



    #endregion
     */

    #region 下拉刷新列表
    /// <summary>
    /// 创建下拉循环列表对象
    /// </summary>
    /// <typeparam name="T">子项类型</typeparam>
    /// <param name="grid">控件对象</param>
    /// <param name="source">数据源</param>
    /// <param name="type">子项赋值代码</param>
    public static void CreateRefreshedView<T>(this RulerGrid grid, List<T> source, Type type, GameObject template, Action OnEndOfViewAction)
    {
        if (grid.ruler == null)
        {
            grid.ruler = new RefreshedListView<T>();
            grid.ruler.SetRulerGrid(grid);
        }
        var creater = grid.ruler as IRefreshedCreater<T>;
        if (creater != null)
        {
            creater.CreateScrollView(source, type, template, OnEndOfViewAction);
        }
    }
    /// <summary>
    /// 下拉循环列表刷新
    /// </summary>
    /// <typeparam name="T">数据源类型</typeparam>
    /// <param name="grid">持有控件</param>
    /// <param name="source">数据源</param>
    public static void ReflashReflashedScrollView<T>(this RulerGrid grid, List<T> source)
    {
        if (grid.ruler == null)
        {
            return;
        }
        var creater = grid.ruler as IRefreshedCreater<T>;
        if (creater != null)
        {
            creater.ReflashSource(source);
        }
    }

    /// <summary>
    /// 显示刷新信息,并且锁住等待刷新完成
    /// </summary>
    /// <typeparam name="T">数据源类型</typeparam>
    /// <param name="grid">持有控件</param>
    /// <param name="source">数据源</param>
    public static void ShowRefreshingView<T>(this RulerGrid grid)
    {
        if (grid.ruler == null)
        {
            return;
        }
        var creater = grid.ruler as IRefreshedCreater<T>;
        if (creater != null)
        {
            creater.ShowRefreshing();
        }
    }
    /// <summary>
    /// 关闭刷新信息，解锁完成刷新
    /// </summary>
    /// <typeparam name="T">数据源类型</typeparam>
    /// <param name="grid">持有控件</param>
    /// <param name="source">数据源</param>
    public static void CloseRefreshingView<T>(this RulerGrid grid)
    {
        if (grid.ruler == null)
        {
            return;
        }
        var creater = grid.ruler as IRefreshedCreater<T>;
        if (creater != null)
        {
            creater.CloseRefreshing();
        }
    }
    #endregion
}
