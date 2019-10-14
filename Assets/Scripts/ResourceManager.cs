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
            node = Head;

        }
        return node;
    }
}
