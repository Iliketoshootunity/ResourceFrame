using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : Singleton<ObjectManager>
{
    //GameObject对象池,key是Path的crc
    private Dictionary<uint, List<ResourceObj>> m_ResourceObjPool = new Dictionary<uint, List<ResourceObj>>();
    //由ObjectManager实例化的所有对象，key是InstanceId
    private Dictionary<int, ResourceObj> m_ResourceObjDic = new Dictionary<int, ResourceObj>();
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
    /// 同步实例化对象
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isSpawnTrsChild"></param>
    /// <param name="clear"></param>
    /// <returns></returns>
    public GameObject Instantiate(string path, bool isSpawnTrsChild = true, bool clear = true)
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
                    resourceObj.CloneObj = GameObject.Instantiate<GameObject>(prefab);
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
    /// 回收GameObject
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
