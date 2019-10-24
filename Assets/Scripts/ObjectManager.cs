using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : Singleton<ObjectManager>
{
    //GameObject对象池
    private Dictionary<uint, List<ResourceObj>> m_ResourceObjPool = new Dictionary<uint, List<ResourceObj>>();
    //ResourceObj类对象池
    private ClassObjectPool<ResourceObj> m_ResourceObjClassPool = ObjectManager.Instance.GetOrCreateClassObjectPool<ResourceObj>(500);

    private Transform m_RecycleTrs;
    private Transform m_SpawnTrs;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="spawnTrs"></param>
    /// <param name="recycleTrs"></param>
    public void Init(Transform spawnTrs, Transform recycleTrs)
    {
        m_SpawnTrs = spawnTrs;
        m_RecycleTrs = spawnTrs;
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
    public GameObject Instantiate(string path, bool isSpawnTrsChild = false, bool clear = true)
    {
        if (string.IsNullOrEmpty(path)) return null;
        uint crc = Crc32.GetCrc32(path);
        ResourceObj resourceObj = GetResourceObjFromPool(crc);
        if (resourceObj == null)
        {
            resourceObj = m_ResourceObjClassPool.Spawn(true);
            resourceObj.Clear = clear;
            resourceObj = ResourceManager.Instance.LoadResourceObj(path, resourceObj);
            if (resourceObj != null)
            {
                GameObject prefab = (GameObject)resourceObj.AssetObj.AssetObj;
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
        return resourceObj.CloneObj;

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
