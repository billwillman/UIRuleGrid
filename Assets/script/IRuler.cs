using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;




/// <summary>
/// list继承接口
/// </summary>
public interface IRuler
{
    /// <summary>
    /// 设置grid
    /// </summary>
    /// <param name="grid"></param>
    void SetRulerGrid(RulerGrid grid);
    /// <summary>
    /// rulerData 设置方法
    /// </summary>
    /// <returns></returns>
    float[,] GetRulerData();
    /// <summary>
    /// 根据id获取克隆对象
    /// </summary>
    /// <returns></returns>
    ListViewItem GetCloneObjByIndex(int index);
    /// <summary>
    /// 根据id获取类型
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    Type GetCloneTypeByIndex(int index);
    /// <summary>
    /// 回收克隆对象
    /// </summary>
    /// <param name="index"></param>
    /// <param name="obj"></param>
    void RecyclingCloneObjByIndex(ListViewItem obj);
    /// <summary>
    /// 通过脚本代码绑定数据
    /// </summary>
    /// <param name="index"></param>
    /// <param name="obj"></param>
    void SetObjViewByIndex(int index, ListViewItem obj);
    /// <summary>
    /// 清理接口
    /// </summary>
    void Clear();
    /// <summary>
    /// 下标索引范围
    /// </summary>
    /// <returns></returns>
    int IndexSearchRange();
    /// <summary>
    /// 是否到达底部，isEnd true为底部
    /// </summary>
    /// <param name="isEnd"></param>
    void OnScollEndOfList(bool isEnd);
}

/*
public interface IChatCreater<T1>
{
    void CreateScrollView(List<T1> source, Action<T1, ChatItemTemplate> fillAction);
    void ReflashSource(List<T1> source);
    void OnRegistration(UIInput input, GameObject voice, MonoBehaviour ui, ChatKey key);
    void OnFunctionClick();
  //  void OnSendChat(FriendVO vo);
  //  void OnSendChat(BottleV0 vo);
    void OnSendChat(long bottleId, string context, string voiceFilePath);
    void SetChatInputValue(string value);
}*/


public interface IRefreshedCreater<T1>
{
    void CreateScrollView(List<T1> source, Type type, GameObject template, Action OnEndOfViewAction);
    void ReflashSource(List<T1> source);
    void ShowRefreshing();
    void CloseRefreshing();
}

public interface ICreater<T1>
{
    void CreateScrollView(List<T1> source, Type type, GameObject template);
    void ReflashSource(List<T1> source);
    //void SetChildState(int index);
    //bool GetChildState(int index);
}
public interface ICreater<T1, T2>
{
    void CreateScrollView(Dictionary<T1, List<T2>> source, Type childType, GameObject child, Type titleType, GameObject title);
    void CreateScrollView(Dictionary<T1, List<T2>> source, Type childType, GameObject child, Type titleType, GameObject title, List<bool> state);
    void ReflashSource(Dictionary<T1, List<T2>> source);
    void ReflashSource(Dictionary<T1, List<T2>> source, List<bool> state);
    bool GetTitleState(int index);

}


