using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : Singleton<ObjectManager>
{
    //由ObjectManager实例化的所有对象，ResourceObj包含已经实例化的GameObject,key是GameObject的InstanceId。
    //m_ResourceObjDic包含Active的，和在m_ResourceObjPool里的
    private Dictionary<int, ResourceObj> m_ResourceObjDic = new Dictionary<int, ResourceObj>();
    //GameObject对象池,key是Path的crc
    private Dictionary<uint, List<ResourceObj>> m_ResourceObjPool = new Dictionary<uint, List<ResourceObj>>();
    //正在异步加载的ResourceObj，Key 是GUID
    private Dictionary<long, ResourceObj> m_AsyncResObjs = new Dictionary<long, ResourceObj>();
    //ResourceObj类对象池
    private ClassObjectPool<ResourceObj> m_ResourceObjClassPool;

    private Transform m_RecycleTrs;
    private Transform m_SpawnTrs;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="spawnTrs"></param>
    /// <param name="recycleTrs"></param>
    public void Init(Transform spawnTrs, Transform recycleTrs)
    {
        m_ResourceObjClassPool = new ClassObjectPool<ResourceObj>(500);
        m_SpawnTrs = spawnTrs;
        m_RecycleTrs = recycleTrs;
        m_RecycleTrs.gameObject.SetActive(false);
    }

    /// <summary>
    /// 这个角色是否由ObjectManager创建
    /// </summary>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    public bool IsCreateByObjectManager(GameObject gameObject)
    {
        if (gameObject == null) return false;
        int instanceId = gameObject.GetInstanceID();
        if (m_ResourceObjDic.ContainsKey(instanceId))
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// 清空对象池
    /// </summary>
    public void ClearPool()
    {
        List<uint> tempList = new List<uint>();
        foreach (var item in m_ResourceObjPool)
        {
            tempList.Add(item.Key);
        }
        for (int i = 0; i < tempList.Count; i++)
        {
            ClearPoolObject(tempList[i]);
        }
        tempList.Clear();
    }

    /// <summary>
    /// 清除某个资源的对象池
    /// </summary>
    /// <param name="crc"></param>
    public void ClearPoolObject(uint crc)
    {
        List<ResourceObj> objs = null;
        if (m_ResourceObjPool.TryGetValue(crc, out objs))
        {
            for (int i = objs.Count - 1; i >= 0; i--)
            {
                ResourceObj obj = objs[i];
                if (obj != null)
                {
                    if (obj.bClear)
                    {
                        int tempGUID = obj.CloneObj.GetInstanceID();
                        obj.Reset();
                        GameObject.Destroy(obj.CloneObj);
                        m_ResourceObjClassPool.Recycle(obj);
                        m_ResourceObjDic.Remove(tempGUID);
                        objs.Remove(obj);
                    }
                }
            }
        }
        if (objs != null && objs.Count < 0)
        {
            m_ResourceObjPool.Remove(crc);
        }
    }

    /// <summary>
    /// 预加载
    /// </summary>
    public void PreLoadGameObject(string path, int count = 1, bool clear = false)
    {
        List<GameObject> tempGameObject = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            GameObject go = InstantiateGameObject(path, false, clear);
            tempGameObject.Add(go);
        }
        for (int i = 0; i < tempGameObject.Count; i++)
        {
            ReleseGameObject(tempGameObject[i]);
        }
        tempGameObject.Clear();
    }

    /// <summary>
    /// 从对象池中得到ResourceObj
    /// </summary>
    /// <returns></returns>
    public ResourceObj GetResourceObjFromPool(uint crc)
    {
        List<ResourceObj> tempList = null;
        if (m_ResourceObjPool.TryGetValue(crc, out tempList) && tempList != null && tempList.Count > 0)
        {
            ResourceObj obj = tempList[0];
            tempList.RemoveAt(0);
            //添加ResourceItem引用
            ResourceManager.Instance.AddRefCount(obj);
            obj.bInPool = false;
            if (System.Object.ReferenceEquals(obj.OfflineData, null))
            {
                obj.OfflineData.ResetProp();
            }
#if UNITY_EDITOR
            GameObject gameObject = obj.CloneObj;
            if (!System.Object.ReferenceEquals(obj, null))
            {
                if (gameObject.name.EndsWith("(Recycle)"))
                {

                    gameObject.name.Replace("(Recycle)", "");
                }
            }
#endif
            return obj;
        }
        else
        {
            return null;
        }
    }
    /// <summary>
    /// 取消加载
    /// </summary>
    public void CancleLoad(long guid)
    {
        ResourceObj obj = null;
        if (m_AsyncResObjs.TryGetValue(guid, out obj) && obj != null)
        {
            if (ResourceManager.Instance.CancleLoad(obj))
            {
                m_AsyncResObjs.Remove(guid);
                obj.Reset();
                m_ResourceObjClassPool.Recycle(obj);
            }

        }
    }

    /// <summary>
    /// 同步实例化对象
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isSpawnTrsChild"></param>
    /// <param name="clear"></param>
    /// <returns></returns>
    public GameObject InstantiateGameObject(string path, bool isSpawnTrsChild = false, bool clear = true)
    {
        if (string.IsNullOrEmpty(path)) return null;
        uint crc = Crc32.GetCrc32(path);
        ResourceObj resourceObj = GetResourceObjFromPool(crc);
        if (resourceObj == null)
        {
            resourceObj = m_ResourceObjClassPool.Spawn(true);
            resourceObj.bClear = clear;
            resourceObj = ResourceManager.Instance.LoadResourceObj(path, resourceObj);
            if (resourceObj != null)
            {
                GameObject prefab = (GameObject)resourceObj.ResItem.AssetObj;  
                if (prefab != null)
                {
                    resourceObj.CloneObj = GameObject.Instantiate(prefab) as GameObject;
                    resourceObj.OfflineData = resourceObj.CloneObj.GetComponent<OfflineData>();
                }
                else
                {
                    Debug.LogWarning("ObjectManager Instantiate error");
                    return null;
                }
            }
        }
        if (isSpawnTrsChild)
        {
            resourceObj.CloneObj.transform.SetParent(m_SpawnTrs, false);
        }

        int guid = resourceObj.CloneObj.GetInstanceID();
        if (!m_ResourceObjDic.ContainsKey(guid))
        {
            m_ResourceObjDic.Add(guid, resourceObj);
        }

        return resourceObj.CloneObj;

    }
    /// <summary>
    /// 异步实例化GameObject
    /// </summary>
    /// <param name="path"></param>
    /// <param name="loadPriority"></param>
    /// <param name="finishCallBack"></param>
    /// <param name="parem1"></param>
    /// <param name="parem2"></param>
    /// <param name="parem3"></param>
    /// <returns></returns>
    public long AsyncInstantiateGameObject(string path, EAysncLoadPriority loadPriority, OnAsyncObjFinsih finishCallBack, bool isSpawnTrsChild = true, bool clear = true, object parem1 = null, object parem2 = null, object parem3 = null)
    {
        if (string.IsNullOrEmpty(path))
        {
            return 0;
        }
        uint crc = Crc32.GetCrc32(path);
        //先从对象池中获取
        ResourceObj resourceObj = GetResourceObjFromPool(crc);
        if (resourceObj != null)
        {
            if (isSpawnTrsChild)
            {
                resourceObj.CloneObj.transform.SetParent(m_SpawnTrs);
            }
            if (finishCallBack != null)
            {
                finishCallBack(path, resourceObj.CloneObj, parem1, parem2, parem3);
            }
            return resourceObj.GUID;
        }
        //添加到正在异步加载的字典中
        long guid = ResourceManager.Instance.CreateGuid();
        m_AsyncResObjs.Add(guid, resourceObj);
        //给ResourceObj 赋值
        resourceObj = m_ResourceObjClassPool.Spawn(true);
        resourceObj.Crc = crc;
        resourceObj.GUID = guid;
        resourceObj.bIsSpawnTrsChild = isSpawnTrsChild;
        resourceObj.FinishCallBack = finishCallBack;
        resourceObj.bClear = clear;
        resourceObj.Param1 = parem1;
        resourceObj.Param2 = parem2;
        resourceObj.Param3 = parem3;
        //异步加载GameObject
        ResourceManager.Instance.AsyncLoadResourceObj(path, loadPriority, resourceObj, OnLoadFinishedGameObject);
        return guid;
    }
    /// <summary>
    /// 加载GameObject完成
    /// </summary>
    /// <param name="path"></param>
    /// <param name="obj"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="param3"></param>
    public void OnLoadFinishedGameObject(string path, ResourceObj obj)
    {
        if (obj == null)
        {
            Debug.LogError("异步加载的GameObject的中间类ResourceObj为空");
            return;
        }
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("异步加载GameObject的对象为空");
            return;
        }
        if (obj.ResItem == null)
        {
            Debug.LogError("异步加载的资源的中间类ResourceItem为空");
            return;
        }
        //加载完成，从正在加载的异步字典中移除
        if (m_AsyncResObjs.ContainsKey(obj.GUID))
        {
            m_AsyncResObjs.Remove(obj.GUID);
        }

        obj.CloneObj = GameObject.Instantiate((GameObject)obj.ResItem.AssetObj);
        if (obj.CloneObj != null)
        {
            obj.OfflineData = obj.CloneObj.GetComponent<OfflineData>();
            //添加到已经实例化完成的GameObject字典中
            int guid = obj.CloneObj.GetInstanceID();
            if (!m_ResourceObjDic.ContainsKey(guid))
            {
                m_ResourceObjDic.Add(guid, obj);
            }
            if (obj.bIsSpawnTrsChild)
            {
                obj.CloneObj.transform.SetParent(m_SpawnTrs);
            }
        }
        if (obj.FinishCallBack != null)
        {
            obj.FinishCallBack(path, obj.CloneObj, obj.Param1, obj.Param2, obj.Param3);
        }

    }
    /// <summary>
    /// 回收GameObject
    /// maxCacheCount <0 标志无限缓存
    /// </summary>
    public bool ReleseGameObject(GameObject obj, int maxCacheCount = -1, bool destoryObj = false, bool isRecycleTrsChild = true)
    {
        if (obj == null) return false;
        int guid = obj.GetInstanceID();
        ResourceObj resObj = null;
        if (!m_ResourceObjDic.TryGetValue(guid, out resObj) || resObj == null)
        {
            Debug.LogWarning(obj.name + "不是从ObjManager中实例化的");
            return false;
        }
        if (resObj.bInPool)
        {
            Debug.LogWarning(obj.name + "已经在对象池中");
            return false;
        }
#if UNITY_EDITOR
        obj.name += "(Recycle)";
#endif
        if (maxCacheCount == 0)
        {
            ResourceManager.Instance.ReleseResourceObj(resObj, destoryObj);
            m_ResourceObjDic.Remove(guid);
            resObj.Reset();
            m_ResourceObjClassPool.Recycle(resObj);
        }
        else
        {
            List<ResourceObj> rs = null;
            if (!m_ResourceObjPool.TryGetValue(resObj.Crc, out rs))
            {
                rs = new List<ResourceObj>();
                m_ResourceObjPool.Add(resObj.Crc, rs);
            }
            if (maxCacheCount < 0 || rs.Count < maxCacheCount)
            {
                rs.Add(resObj);
                //减少ResourceItem引用
                ResourceManager.Instance.ReducefCont(resObj);
                resObj.bInPool = true;
                if (isRecycleTrsChild)
                {

                    resObj.CloneObj.transform.SetParent(m_RecycleTrs);
                }
                else
                {
                    resObj.CloneObj.SetActive(true);
                }
            }
            else
            {
                ResourceManager.Instance.ReleseResourceObj(resObj, destoryObj);
                m_ResourceObjDic.Remove(guid);
                resObj.Reset();
                m_ResourceObjClassPool.Recycle(resObj);
            }
        }
        return true;

    }


    #region 类对象池
    protected Dictionary<Type, object> m_ClassObjectPoolDic = new Dictionary<Type, object>();
    public ClassObjectPool<T> GetOrCreateClassObjectPool<T>(int maxCount) where T : class, new()
    {
        Type t = typeof(T);
        if (!m_ClassObjectPoolDic.ContainsKey(t) || m_ClassObjectPoolDic[t] == null)
        {
            ClassObjectPool<T> pool = new ClassObjectPool<T>(maxCount);
            m_ClassObjectPoolDic.Add(t, pool);
        }
        return (ClassObjectPool<T>)m_ClassObjectPoolDic[t];
    }
    #endregion
}
