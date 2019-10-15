using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : Singleton<ResourceManager>
{


}
/// <summary>
/// 双向链表节点
/// </summary>
/// <typeparam name="T"></typeparam>
public class DoubleLinkedListNode<T> where T : class, new()
{
    public DoubleLinkedListNode<T> Next;
    public DoubleLinkedListNode<T> Prev;
    public T Content;
}

/// <summary>
/// 双向链表
/// </summary>
/// <typeparam name="T"></typeparam>

public class DoubleLinkedList<T> where T : class, new()
{
    public DoubleLinkedListNode<T> Head;
    public DoubleLinkedListNode<T> Tail;

    protected ClassObjectPool<DoubleLinkedListNode<T>> NodePool = ObjectManager.GetInstance().GetOrCreateClassObjectPool<DoubleLinkedListNode<T>>(500);

    protected int m_Count;

    public int Count
    {
        get
        {
            return m_Count;
        }
    }
    /// <summary>
    ///  添加到表头
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToHead(T t)
    {
        if (t == null)
        {
            return null;
        }
        DoubleLinkedListNode<T> temp = NodePool.Spawn(true);
        temp.Next = null;
        temp.Prev = null;
        temp.Content = t;
        return AddToHead(temp);
    }
    /// <summary>
    /// 添加到表头
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToHead(DoubleLinkedListNode<T> node)
    {
        if (node == null)
        {
            return null;
        }
        if (Head == null)
        {
            Head = Tail = node;
        }
        else
        {
            node.Prev = null;
            Head.Prev = node;
            node.Next = Head;
            Head = node;

        }
        m_Count++;
        return node;
    }
    /// <summary>
    /// 添加到表尾
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToTail(T t)
    {
        if (t == null)
        {
            return null;
        }
        DoubleLinkedListNode<T> temp = NodePool.Spawn(true);
        temp.Next = null;
        temp.Prev = null;
        temp.Content = t;
        return AddToTail(temp);
    }

    /// <summary>
    /// 添加到表尾
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public DoubleLinkedListNode<T> AddToTail(DoubleLinkedListNode<T> node)
    {
        if (node == null)
        {
            return null;
        }
        node.Next = null;
        Tail.Next = node;
        node.Prev = Tail;
        Tail = node;
        return Tail;
    }
    /// <summary>
    /// 移除节点
    /// </summary>
    /// <param name="node"></param>
    public void RemoveNode(DoubleLinkedListNode<T> node)
    {
        if (node == null)
        {
            return;
        }
        if (node == Head)
        {
            Head = node.Next;
        }
        if (node == Tail)
        {
            Tail = node.Prev;
        }
        if (node.Prev != null)
            node.Prev.Next = node.Next;
        if (node.Next != null)
            node.Next.Prev = node.Prev;
        node.Prev = null;
        node.Next = null;
        NodePool.Recycle(node);
        m_Count--;
    }
    /// <summary>
    /// 移动到头部
    /// </summary>
    /// <param name="node"></param>
    public void MoveToHead(DoubleLinkedListNode<T> node)
    {
        if (node == null)
        {
            return;
        }
        if (node == Head)
        {
            return;
        }
        if (node.Prev == null && node.Next == null) return;
        if (node == Tail)
        {
            Tail = node.Prev;
        }
        if (node.Prev != null)
            node.Prev.Next = node.Next;
        if (node.Next != null)
            node.Next.Prev = node.Prev;

        node.Prev = null;
        node.Next = Head;
        Head.Prev = node;

        if (Tail == null)
        {
            Tail = Head;
        }
    }
}

public class CMapList<T> where T : class, new()
{

}
