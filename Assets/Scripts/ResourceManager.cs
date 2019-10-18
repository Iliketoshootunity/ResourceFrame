using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void OnAysncLoadAssetFinishedDelegate(UnityEngine.Object param1, UnityEngine.Object parem2, UnityEngine.Object parem3);

public class AsyncLoadAssetParam
{
    public List<AsyncLoadAssetDelegate> m_EndDelegate;
}

public class AsyncLoadAssetDelegate
{
    //public
}

public class ResourceManager : Singleton<ResourceManager>
{
    protected bool m_LoadAssetBundleFromEditor = false;

    //缓存的资源
    public Dictionary<uint, ResourceItem> AssetDic = new Dictionary<uint, ResourceItem>();

    protected CMapList<ResourceItem> m_NoRefreceAssetMapList = new CMapList<ResourceItem>();


    protected MonoBehaviour m_StartMono;

    public void Init(MonoBehaviour startMono)
    {
        m_StartMono = startMono;

    }

    //最大缓存个数
    private const int MAXCACHECOUNT = 500;
    /// <summary>
    /// 加载无须实例化的资源，比如说声音片段，图片等等
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public T LoadResource<T>(string path) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(path)) return null;
        uint crc = Crc32.GetCrc32(path);
        ResourceItem item = GetCacheResourceItem(crc);
        if (item != null)
        {
            return (T)(item.AssetObj);
        }
        UnityEngine.Object obj = null;
#if UNITY_EDITOR
        if (m_LoadAssetBundleFromEditor)
        {
            item = AssetBundleManager.GetInstance().FindResourceItem(crc);
            if (item.AssetObj == null)
            {
                item.AssetObj = LoadAssetFromEditor<T>(path);
            }
            if (item.AssetObj != null)
            {
                obj = item.AssetObj;
            }
        }
#endif
        if (obj == null)
        {
            item = AssetBundleManager.GetInstance().LoadResourceItem(crc);
            if (item != null && item.AssetName != null)
            {
                if (item.AssetObj)
                {
                    obj = item.AssetObj;
                }
                else
                {
                    obj = item.AssetBundle.LoadAsset<T>(item.AssetName);
                }
            }
        }
        CacheResourceItem(path, ref item, crc, obj);
        return obj as T;
    }

    /// <summary>
    /// 释放不需要实例化的资源 ,根据路径
    /// </summary>
    public void ReleseResourceItem(string path, bool destoryObj = false)
    {
        if (string.IsNullOrEmpty(path)) return;

        uint crc = Crc32.GetCrc32(path);
        ResourceItem item;
        if (AssetDic.TryGetValue(crc, out item) && item != null)
        {
            item.RefCount--;
            DestoryResourceItem(item, destoryObj);
        }
        else
        {
            Debug.LogError("并没有再 缓存列表种找到资源，程序逻辑错误，请检查！！");
        }
    }

    /// <summary>
    /// 释放不需要实例化的资源 ,
    /// </summary>
    public void ReleseResourceItem(UnityEngine.Object obj, bool destoryObj = false)
    {
        if (obj == null) return;
        ResourceItem item = null;
        foreach (var temp in AssetDic)
        {
            if (temp.Value.GUID == obj.GetInstanceID())
            {
                item = temp.Value;
            }
        }

        if (item == null)
        {
            Debug.LogWarning("这个资源并不是通过ResourceManager加载出来的，请检查！！！");
            return;
        }
        item.RefCount--;
        DestoryResourceItem(item, destoryObj);
    }

    /// <summary>
    /// 删除资源
    /// </summary>
    /// <param name="item"></param>
    /// <param name="destoryObj"></param>
    public void DestoryResourceItem(ResourceItem item, bool destoryObj = false)
    {
        if (item == null || item.RefCount > 0)
        {
            return;
        }
        if (!AssetDic.Remove(item.Crc))
        {
            return;
        }

        if (!destoryObj)
        {
            m_NoRefreceAssetMapList.InsertToHead(item);
            return;
        }

        item.AssetObj = null;
#if UNITY_EDITOR
        Resources.UnloadUnusedAssets();
#endif
    }
    /// <summary>
    /// 淘汰末尾的，不经常使用的资源
    /// </summary>
    public void WaskOut()
    {
        while (m_NoRefreceAssetMapList.Size() >= MAXCACHECOUNT)
        {
            for (int i = 0; i < MAXCACHECOUNT / 2; i++)
            {
                ResourceItem item = m_NoRefreceAssetMapList.Back();
                DestoryResourceItem(item, true);
            }

        }
    }

    /// <summary>
    /// 获取缓存资源
    /// </summary>
    /// <param name="crc"></param>
    /// <param name="addRefCount"></param>
    /// <returns></returns>
    public ResourceItem GetCacheResourceItem(uint crc, int addRefCount = 1)
    {
        ResourceItem item = null;
        if (AssetDic.TryGetValue(crc, out item))
        {
            item.RefCount += addRefCount;
            item.LastUseTime = UnityEngine.Time.realtimeSinceStartup;
        }
        return item;
    }

    /// <summary>
    /// 缓存资源
    /// </summary>
    /// <param name="path"></param>
    /// <param name="item"></param>
    /// <param name="crc"></param>
    /// <param name="obj"></param>
    /// <param name="addrefcount"></param>
    public void CacheResourceItem(string path, ref ResourceItem item, uint crc, Object obj, int addrefcount = 1)
    {
        WaskOut();

        if (item == null)
        {
            Debug.LogError("ResourceItem is null " + path);
            return;
        }
        if (item.AssetBundle == null)
        {
            Debug.LogError("ResourceItem AssetBundle is null " + path);
            return;
        }
        if (obj == null)
        {
            Debug.LogError("ResourceItem AssetObj is null " + path);
            return;
        }
        item.GUID = obj.GetInstanceID();
        item.AssetObj = obj;
        item.RefCount += addrefcount;
        item.LastUseTime = Time.realtimeSinceStartup;
        ResourceItem oldItem = null;
        if (AssetDic.TryGetValue(crc, out oldItem))
        {
            AssetDic[crc] = item;
        }
        else
        {
            AssetDic.Add(crc, item);
        }

    }

#if UNITY_EDITOR
    protected T LoadAssetFromEditor<T>(string path) where T : UnityEngine.Object
    {
        T t = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        return t;
    }
#endif
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
    protected DoubleLinkedList<T> m_DoubleLinkedList = new DoubleLinkedList<T>();
    protected Dictionary<T, DoubleLinkedListNode<T>> m_ContentToNode = new Dictionary<T, DoubleLinkedListNode<T>>();



    public void Clear()
    {
        for (int i = 0; i < m_ContentToNode.Count; i++)
        {
            m_DoubleLinkedList.RemoveNode(m_DoubleLinkedList.Tail);
        }
    }

    public void InsertToHead(T t)
    {
        if (t == null) return;
        DoubleLinkedListNode<T> node = null;
        if (m_ContentToNode.TryGetValue(t, out node) && node != null)
        {
            m_DoubleLinkedList.MoveToHead(node);
            return;
        }
        node = m_DoubleLinkedList.AddToHead(t);
        m_ContentToNode.Add(t, node);
    }

    public void Pop()
    {
        if (m_DoubleLinkedList.Tail != null)
        {
            Remove(m_DoubleLinkedList.Tail.Content);
        }
    }

    public void Remove(T t)
    {
        if (t == null) return;
        DoubleLinkedListNode<T> node = null;
        if (m_ContentToNode.TryGetValue(t, out node) && node != null)
        {
            m_DoubleLinkedList.RemoveNode(node);
            m_ContentToNode.Remove(t);
        }
    }

    public int Size()
    {
        return m_ContentToNode.Count;
    }
    /// <summary>
    /// 获取尾部的内容
    /// </summary>
    /// <returns></returns>
    public T Back()
    {
        return m_DoubleLinkedList.Tail == null ? null : m_DoubleLinkedList.Tail.Content;
    }

    public bool Find(T t)
    {
        if (t == null) return false;
        DoubleLinkedListNode<T> node = null;
        if (m_ContentToNode.TryGetValue(t, out node) && node != null)
        {
            return true;
        }
        return false;
    }

    public void Refresh(T t)
    {
        if (t == null) return;
        DoubleLinkedListNode<T> node = null;
        if (m_ContentToNode.TryGetValue(t, out node) && node != null)
        {
            m_DoubleLinkedList.MoveToHead(node);
        }
    }

}
