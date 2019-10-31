using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//异步加载不需要实例化的资源的回调
public delegate void OnAsyncObjFinsih(string path, Object obj, object param1 = null, object param2 = null, object param3 = null);
//实例化资源加载完成回调
public delegate void OnAsyncResObjFinsih(string path, ResourceObj obj, object param1 = null, object param2 = null, object param3 = null);

/// <summary>
/// 异步加载优先级的枚举
/// </summary>
public enum EAysncLoadPriority
{
    Hight = 0,
    Midddle,
    Slow,
    Number
}

/// <summary>
/// 从AB包中实例化的GameObject的中间类
/// </summary>
public class ResourceObj
{
    //资源路径的crc
    public uint Crc = 0;
    //存ResourceItem
    public ResourceItem ResItem = null;
    //实例化的游戏物体
    public GameObject CloneObj = null;
    //存GameObject的Instance Id
    public long GUID = 0;
    //转场景是否清除
    public bool bClear = true;
    //是否在对象池中
    public bool bInPool = false;
    //-----------------------异步的参数
    //保存是否是Spawn Transform的子物体
    public bool bIsSpawnTrsChild;
    //加载完成的回调
    public OnAsyncObjFinsih FinishCallBack;
    //回调参数
    public object Param1, Param2, Param3;

    public void Reset()
    {
        Crc = 0;
        ResItem = null;
        CloneObj = null;
        GUID = 0;
        bClear = true;
        bInPool = false;
        bIsSpawnTrsChild = true;
        FinishCallBack = null;
        Param1 = null;
        Param2 = null;
        Param3 = null;

    }
}

/// <summary>
///  异步加载加载的参数，在协程种一个个读取信息，进行真正的加载
/// </summary>
public class AsyncLoadAssetParam
{
    public List<AsyncCallBack> CallBackList = new List<AsyncCallBack>();
    public uint Crc;
    public string Path;
    public EAysncLoadPriority Priority = EAysncLoadPriority.Slow;

    public void Reset()
    {
        CallBackList.Clear();
        Crc = 0;
        Path = "";
        Priority = EAysncLoadPriority.Slow;
    }
}

/// <summary>
/// 异步加载回调的封装
/// </summary>
public class AsyncCallBack
{

    public OnAsyncResObjFinsih OnAsyncResObjLoadFinished;

    public OnAsyncObjFinsih OnAsyncObjLoadFinished;
    public object Param1, Param2, Param3;

    public void Reset()
    {
        OnAsyncObjLoadFinished = null;
        OnAsyncResObjLoadFinished = null;

        Param1 = null;
        Param2 = null;
        Param3 = null;
    }

}

public class ResourceManager : Singleton<ResourceManager>
{
    protected bool m_LoadAssetFormEditor = false;

    //缓存的资源 key是path crc value 是ResourceItem
    public Dictionary<uint, ResourceItem> AssetDic = new Dictionary<uint, ResourceItem>();
    //缓存引用计数为零的资源列表，达到缓存最大的时候释放这个列表里面最早没用的资源
    protected CMapList<ResourceItem> m_NoRefreceAssetMapList = new CMapList<ResourceItem>();

    //正在加载的资源(根据优先级)
    protected List<AsyncLoadAssetParam>[] m_LoadingAssetArray;
    //正在加载的资源 key是path crc value是 AsyncLoadAssetParam
    protected Dictionary<uint, AsyncLoadAssetParam> m_LoadingAssetDic = new Dictionary<uint, AsyncLoadAssetParam>();
    //AsyncLoadAssetParam池
    protected ClassObjectPool<AsyncLoadAssetParam> m_AsyncLoadAssetParamPool = ObjectManager.Instance.GetOrCreateClassObjectPool<AsyncLoadAssetParam>(50);
    //AsyncCallBack池
    protected ClassObjectPool<AsyncCallBack> m_AsyncCallBackPool = ObjectManager.Instance.GetOrCreateClassObjectPool<AsyncCallBack>(500);

    protected MonoBehaviour m_StartMono;

    //最长连续卡着加载资源的时间，单位微妙
    private const long MAXLOADRESTIME = 200000;

    //最大缓存个数
    private const int MAXCACHECOUNT = 500;

    #region 增加/减少ResourceItem的引用计数
    /// <summary>
    /// 增加引用计数,根据ResourceObj
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="refCount"></param>
    public void AddRefCount(ResourceObj obj, int refCount = 1)
    {
        if (obj == null)
        {
            Debug.LogWarning("ResourceObj 不能为空");
            return;
        }
        AddRefCount(obj.Crc, refCount);
    }
    /// <summary>
    /// 增加引用计数，根据Path
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="refCount"></param>
    public void AddRefCount(uint pathCrc, int refCount = 1)
    {
        ResourceItem item = null;
        if (AssetDic.TryGetValue(pathCrc, out item) && item != null)
        {
            item.RefCount += refCount;
            return;
        }
        Debug.LogWarning("找不到匹配的ResourceItem");
    }
    /// <summary>
    /// 减少引用计数，根据ResourceObj
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="refCount"></param>
    public void ReducefCont(ResourceObj obj, int refCount = 1)
    {
        if (obj == null)
        {
            Debug.LogWarning("ResourceObj 不能为空");
            return;
        }
        ReducefCont(obj.Crc, refCount);
    }
    /// <summary>
    /// 减少引用计数，根据Path
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="refCount"></param>
    public void ReducefCont(uint pathCrc, int refCount = 1)
    {
        ResourceItem item = null;
        if (AssetDic.TryGetValue(pathCrc, out item) && item != null)
        {
            item.RefCount -= refCount;
            if (item.RefCount < 0)
            {
                item.RefCount = 0;
            }
            return;
        }
        Debug.LogWarning("找不到匹配的ResourceItem");
    }
    #endregion

    #region 初始化
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="startMono"></param>
    public void Init(MonoBehaviour startMono)
    {
        m_StartMono = startMono;
        m_LoadingAssetArray = new List<AsyncLoadAssetParam>[(int)EAysncLoadPriority.Number];
        for (int i = 0; i < (int)EAysncLoadPriority.Number; i++)
        {
            List<AsyncLoadAssetParam> temp = new List<AsyncLoadAssetParam>();
            m_LoadingAssetArray[i] = temp;
        }

        startMono.StartCoroutine(IEAsyncLoadAsset());
    }
    #endregion

    #region 预加载
    /// <summary>
    /// 预加载无需实例化资源,加载的资源在跳转场景时不会清掉
    /// </summary>
    public void PreloadResource(string path)
    {
        uint crc = Crc32.GetCrc32(path);
        LoadResource<Object>(path);
        ResourceItem item = null;
        if (AssetDic.TryGetValue(crc, out item))
        {
            item.Clear = false;
        }
    }
    #endregion

    #region 清除缓存
    /// <summary>
    /// 清除缓存
    /// </summary>
    public void ClearCache()
    {
        List<ResourceItem> tempList = new List<ResourceItem>();
        foreach (var item in AssetDic)
        {
            if (item.Value.Clear)
            {
                tempList.Add(item.Value);
            }
        }

        for (int i = 0; i < tempList.Count; i++)
        {
            DestoryResourceItem(tempList[i], true);
        }

        tempList.Clear();
    }
    #endregion

    #region 加载
    public ResourceObj LoadResourceObj(string path, ResourceObj Obj)
    {
        if (string.IsNullOrEmpty(path)) return null;
        uint crc = Crc32.GetCrc32(path);
        if (Obj.Crc == 0 || Obj.Crc != crc)
        {
            Obj.Crc = Crc32.GetCrc32(path);
        }
        Object objTemp = LoadResource<Object>(path);
        if (objTemp)
        {
            ResourceItem item = null;
            if (AssetDic.TryGetValue(Obj.Crc, out item))
            {
                item.Clear = Obj.bClear;
                Obj.ResItem = item;
                return Obj;
            }
            else
            {
                Debug.LogWarning("LoadResourceObj 逻辑错误，请检查");
                return null;
            }
        }
        else
        {
            Debug.LogError("LoadResourceObj 无法正确加载资源");
            return null;
        }

    }

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
        if (m_LoadAssetFormEditor)
        {
            item = AssetBundleManager.Instance.FindResourceItem(crc);
            if (item == null)
            {
                item = new ResourceItem();
            }
            if (item.AssetObj == null)
            {
                item.AssetObj = LoadAssetFromEditor<T>(path);
                obj = item.AssetObj;
            }
        }
#endif
        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResourceItem(crc);
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
    /// 异步加载预制体
    /// </summary>
    /// <param name="path"></param>
    /// <param name="loadPriority"></param>
    /// <param name="obj"></param>
    public void AsyncLoadPrefab(string path, EAysncLoadPriority loadPriority, ResourceObj obj)
    {
        if (string.IsNullOrEmpty(path)) return;
        uint crc = Crc32.GetCrc32(path);
        //已经在缓存内
        ResourceItem item = GetCacheResourceItem(crc);
        if (item != null)
        {
            obj.CloneObj = GameObject.Instantiate((GameObject)item.AssetObj);
            if (obj.FinishCallBack != null)
            {
                obj.FinishCallBack(path, obj.CloneObj, obj.Param1, obj.Param2, obj.Param2);
            }
            return;
        }
    }

    /// <summary>
    /// 异步加载不需要实例化的资源
    /// </summary>
    /// <param name="path"></param>
    public void AsyncLoadAsset(string path, EAysncLoadPriority loadPriority, OnAsyncObjFinsih finishCallBack, Object parem1 = null, Object parem2 = null, Object parem3 = null, uint crc = 0)
    {
        if (crc == 0)
        {
            crc = Crc32.GetCrc32(path);
        }
        //已经在缓存内
        ResourceItem item = GetCacheResourceItem(crc);
        if (item != null)
        {
            if (finishCallBack != null)
            {
                finishCallBack(path, item.AssetObj, parem1, parem2, parem3);
                return;
            }
        }
        //是否在正在加载的队列种
        AsyncLoadAssetParam loadingAsset = null;
        if (!m_LoadingAssetDic.TryGetValue(crc, out loadingAsset) && loadingAsset == null)
        {
            loadingAsset = m_AsyncLoadAssetParamPool.Spawn(true);
            loadingAsset.Crc = crc;
            loadingAsset.Path = path;
            loadingAsset.Priority = loadPriority;
        }
        //添加回调
        AsyncCallBack callBack = m_AsyncCallBackPool.Spawn(true);
        callBack.OnAsyncObjLoadFinished = finishCallBack;
        callBack.Param1 = parem1;
        callBack.Param2 = parem2;
        callBack.Param3 = parem3;
        loadingAsset.CallBackList.Add(callBack);
        //添加在根据优先级分类的容器
        m_LoadingAssetArray[(int)loadPriority].Add(loadingAsset);
        //添加在key是crc的字典
        m_LoadingAssetDic.Add(crc, loadingAsset);
    }

    /// <summary>
    /// 异步加载资源协程
    /// </summary>
    /// <returns></returns>
    protected IEnumerator IEAsyncLoadAsset()
    {
        long lastYieldTime = System.DateTime.Now.Ticks;
        while (true)
        {
            bool haveYile = false;
            for (int i = 0; i < (int)EAysncLoadPriority.Number; i++)
            {
                if (m_LoadingAssetArray[(int)EAysncLoadPriority.Hight].Count > 0)
                {
                    i = (int)EAysncLoadPriority.Hight;
                }
                else if (m_LoadingAssetArray[(int)EAysncLoadPriority.Midddle].Count > 0)
                {
                    i = (int)EAysncLoadPriority.Midddle;
                }
                if (m_LoadingAssetArray[i].Count <= 0)
                {
                    continue;
                }
                AsyncLoadAssetParam loadAsset = m_LoadingAssetArray[i][0];
                if (loadAsset == null)
                {
                    Debug.LogError("AsyncLoadAssetParam 未正确加载，请检查");
                }
                List<AsyncCallBack> callBacks = loadAsset.CallBackList;
                ResourceItem item = null;
                Object obj = null;
#if UNITY_EDITOR
                if (m_LoadAssetFormEditor)
                {
                    obj = LoadAssetFromEditor<Object>(loadAsset.Path);
                    //模拟异步
                    yield return new WaitForSeconds(0.5f);
                    item = AssetBundleManager.Instance.FindResourceItem(loadAsset.Crc);
                    if (item == null)
                    {
                        //在编辑器状态下，可能没进行打包，不能AssetBundleManager中找到
                        item = new ResourceItem();
                        item.Crc = loadAsset.Crc;
                    }
                }

#endif
                if (obj == null)
                {
                    //获取带有AB包的ResourceItem
                    item = AssetBundleManager.Instance.LoadResourceItem(loadAsset.Crc);
                    if (item != null)
                    {

                        AssetBundleRequest request = item.AssetBundle.LoadAssetAsync(item.AssetName);
                        yield return request;
                        if (request.isDone)
                        {
                            obj = request.asset;
                        }
                        lastYieldTime = System.DateTime.Now.Ticks;
                    }
                }

                CacheResourceItem(loadAsset.Path, ref item, item.Crc, obj, callBacks.Count);

                for (int j = 0; j < callBacks.Count; j++)
                {
                    //触发加载完成回调
                    if (callBacks[j] != null && callBacks[j].OnAsyncObjLoadFinished != null)
                    {
                        callBacks[j].OnAsyncObjLoadFinished(loadAsset.Path, item.AssetObj, callBacks[j].Param1, callBacks[j].Param2, callBacks[j].Param3);
                        callBacks[j].OnAsyncObjLoadFinished = null;
                    }

                    callBacks[j].Reset();
                    m_AsyncCallBackPool.Recycle(callBacks[j]);
                }

                obj = null;
                callBacks.Clear();

                m_AsyncLoadAssetParamPool.Recycle(loadAsset);
                m_LoadingAssetDic.Remove(loadAsset.Crc);
                m_LoadingAssetArray[i].RemoveAt(0);
                loadAsset.Reset();

                if (System.DateTime.Now.Ticks - lastYieldTime > MAXLOADRESTIME)
                {
                    yield return null;
                    lastYieldTime = System.DateTime.Now.Ticks;
                    haveYile = true;
                }
            }

            if (!haveYile || System.DateTime.Now.Ticks - lastYieldTime > MAXLOADRESTIME)
            {
                yield return null;
                lastYieldTime = System.DateTime.Now.Ticks;
            }
        }
    }

#if UNITY_EDITOR
    protected T LoadAssetFromEditor<T>(string path) where T : UnityEngine.Object
    {
        T t = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
        return t;
    }
#endif
    #endregion
    #region 释放

    /// <summary>
    /// 释放ResourceObj
    /// </summary>
    public void ReleseResourceObj(ResourceObj Obj, bool destoryObj)
    {
        if (Obj == null)
        {
            Debug.LogWarning("ResourceObj 逻辑上不可能为空，请检查");
            return;
        }
        if (Obj.Crc == 0 || Obj.CloneObj == null)
        {
            Debug.LogWarning("ResourceObj的数据 逻辑上不可能为空，请检查");
            return;
        }
        ResourceItem item = null;
        if (!AssetDic.TryGetValue(Obj.Crc, out item))
        {
            Debug.LogError("ResourceObj的路径Crc和ResourceItem表AssetDic不匹配，请检查");
            return;
        }
        if (item == null)
        {
            Debug.LogError("ResourceItem 不能为空，请检查");
            return;
        }
        GameObject.Destroy(Obj.CloneObj);
        item.RefCount--;
        DestoryResourceItem(item, destoryObj);
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
    /// 释放不需要实例化的资源 ,,根据对象
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
        if (!destoryObj)
        {
            //如果不删除 则加入到RefCount==0的容器中
            m_NoRefreceAssetMapList.InsertToHead(item);
            return;
        }

        /*进入彻底清除ResourceItem的流程*/

        //在AssetDic移除
        if (!AssetDic.Remove(item.Crc))
        {
            return;
        }
        //在m_NoRefreceAssetMapList移除
        m_NoRefreceAssetMapList.Remove(item);

        //释放AssetBundle的引用
        AssetBundleManager.Instance.ReleseResourceItem(item);

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
                if (item != null)
                {
                    DestoryResourceItem(item, true);
                }

            }

        }
    }
    #endregion
    #region 获取
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
        if (!m_LoadAssetFormEditor)
        {
            if (item.AssetBundle == null)
            {
                Debug.LogError("ResourceItem AssetBundle is null " + path);
                return;
            }
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
    #endregion





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

    protected ClassObjectPool<DoubleLinkedListNode<T>> NodePool = ObjectManager.Instance.GetOrCreateClassObjectPool<DoubleLinkedListNode<T>>(500);

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
