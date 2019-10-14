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
    protected ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool = ObjectManager.GetInstance().GetOrCreateClassObjectPool<AssetBundleItem>(500);
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
        //存储所有的AB信息
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
        item.ReferenceCount++;
        return item.AssetBundle;
    }

    private void UnLoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        uint crc = Crc32.GetCrc32(name);
        if (m_ABNameCrcToAssetBundleItem.TryGetValue(crc, out item) && item != null)
        {
            item.ReferenceCount--;
            //引用计数清空，彻底卸载掉这个资源
            if (item.ReferenceCount <= 0 && item.AssetBundle != null)
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
    public AssetBundle AssetBundle;
    //引用技术，单ReferenceCount==0时，可选择卸载掉这个资源
    public int ReferenceCount;

    public void Reset()
    {
        AssetBundle = null;
        ReferenceCount = 0;
    }
}

public class ResourceItem
{
    public string AssetName = string.Empty;
    public string ABName = string.Empty;
    public uint Crc = 0;
    public string Path = string.Empty;
    public List<string> Depends = null;
    public AssetBundle AssetBundle = null;
}

