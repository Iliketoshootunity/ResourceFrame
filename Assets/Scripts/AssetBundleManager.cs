using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AssetBundleManager : Singleton<AssetBundleManager>
{
    /// <summary>
    /// 资源依赖关系表
    /// </summary>
    protected Dictionary<uint, ResourceItem> m_PathCrcToResourceItem = new Dictionary<uint, ResourceItem>();
    /// <summary>
    /// 存储已经加载的AssetBundle
    /// </summary>
    protected Dictionary<uint, AssetBundleItem> m_ABNameCrcToAssetBundleItem = new Dictionary<uint, AssetBundleItem>();
    /// <summary>
    /// AssetBundleItem类对象池
    /// </summary>
    protected ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool = ObjectManager.Instance.GetOrCreateClassObjectPool<AssetBundleItem>(500);
    /// <summary>
    /// 加载AssetBundle config
    /// </summary>
    public bool LoadAssetBundleConfig()
    {
        //反序列化AssetBundle config
        AssetBundle configAB = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/data");
        if (configAB == null)
        {
            Debug.LogError("AssetBundleconfig的AB包未加载到");
            return false;
        }
        AssetBundleConfig config = null;
        TextAsset textAsset = configAB.LoadAsset<TextAsset>("AssetBundleConfig.bytes");
        MemoryStream ms = new MemoryStream(textAsset.bytes);
        BinaryFormatter bf = new BinaryFormatter();
        config = (AssetBundleConfig)bf.Deserialize(ms);
        if (config == null)
        {
            Debug.LogError("AssetBundleConfig 反序列化失败 ");
            return false;
        }
        //存储所有的AssetBundle资源信息
        for (int i = 0; i < config.ABList.Count; i++)
        {
            BaseAB baseAB = config.ABList[i];
            ResourceItem resourceItem = new ResourceItem();
            resourceItem.Depends = new List<string>();
            resourceItem.AssetName = baseAB.AssetName;
            resourceItem.ABName = baseAB.ABName;
            resourceItem.Crc = baseAB.Crc;
            resourceItem.Depends = baseAB.Depends;
            resourceItem.AssetBundle = null;
            m_PathCrcToResourceItem.Add(resourceItem.Crc, resourceItem);
        }
        return true;
    }

    /// <summary>
    /// 根据路径的Cre加载资源的中间类ResourceItem
    /// </summary>
    /// <param name="pathCrc"></param>
    /// <returns></returns>
    public ResourceItem LoadResourceItem(uint pathCrc)
    {
        ResourceItem item = null;
        if (!m_PathCrcToResourceItem.TryGetValue(pathCrc, out item) || item == null)
        {
            Debug.LogError("存储的AB信息中未有此项");
            return null;
        }

        if (item.AssetBundle != null)
        {
            return item;
        }

        item.AssetBundle = LoadAssetBundle(item.ABName);

        if (item.AssetBundle != null)
        {
            //加载依赖项
            for (int i = 0; i < item.Depends.Count; i++)
            {
                if (!string.IsNullOrEmpty(item.Depends[i]))
                {
                    LoadAssetBundle(item.Depends[i]);
                }

            }
        }

        return item;

    }

    /// <summary>
    /// 释放ResourceItem
    /// </summary>
    /// <param name="item"></param>
    public void ReleseResourceItem(ResourceItem item)
    {
        if (item == null)
        {
            return;
        }
        //卸载依赖项
        for (int i = 0; i < item.Depends.Count; i++)
        {
            if (string.IsNullOrEmpty(item.Depends[i]))
                continue;
            UnLoadAssetBundle(item.Depends[i]);
        }
        //卸载自身
        UnLoadAssetBundle(item.ABName);
    }

    /// <summary>
    /// 查找ResourceItem
    /// </summary>
    /// <param name="pathCrc"></param>
    /// <returns></returns>
    public ResourceItem FindResourceItem(uint pathCrc)
    {
        return m_PathCrcToResourceItem[pathCrc];
    }



    /// <summary>
    /// 根据AB包名加载AssetBundle 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private AssetBundle LoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        uint crc = Crc32.GetCrc32(name);
        if (!m_ABNameCrcToAssetBundleItem.TryGetValue(crc, out item))
        {
            item = m_AssetBundleItemPool.Spawn(true);
            item.AssetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + name);                     //测试路径
            if (item.AssetBundle == null)
            {
                Debug.LogError("未能正确加载AssetBundle,请检查！！！！！");

            }
            m_ABNameCrcToAssetBundleItem.Add(crc, item);
        }
        item.RefCount++;
        return item.AssetBundle;
    }

    private void UnLoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        uint crc = Crc32.GetCrc32(name);
        if (m_ABNameCrcToAssetBundleItem.TryGetValue(crc, out item) && item != null)
        {
            item.RefCount--;
            //引用计数清空，彻底卸载掉这个资源
            if (item.RefCount <= 0 && item.AssetBundle != null)
            {
                item.AssetBundle.Unload(false);
                item.Reset();
                m_AssetBundleItemPool.Recycle(item);
                m_ABNameCrcToAssetBundleItem.Remove(crc);
            }
        }
    }

}


public class AssetBundleItem
{
    //AssetBundle
    public AssetBundle AssetBundle;
    //AssetBundle引用计数，单ReferenceCount==0时，卸载掉这个资源
    public int RefCount;

    public void Reset()
    {
        AssetBundle = null;
        RefCount = 0;
    }
}

public class ResourceItem
{
    //该资源名字
    public string AssetName = string.Empty;
    //该资源所在AB包的名字
    public string ABName = string.Empty;
    //该资源的crc
    public uint Crc = 0;
    //该资源的依赖的AB包
    public List<string> Depends = null;
    //该资源所在的AB包
    public AssetBundle AssetBundle = null;


    //引用计数，当引用计数为0时，可以选择卸载掉这个资源（将所在的AssetBundleItem 的引用计数减-，当将所在的AssetBundleItem 引用计数为0时，会卸载该资源所在的AB包）
    public int RefCount;
    //资源实体
    public UnityEngine.Object AssetObj;
    //GUID
    public long GUID;
    //最后使用时间
    public float LastUseTime;
}

