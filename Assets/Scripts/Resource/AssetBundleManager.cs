using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AssetBundleManager : Singleton<AssetBundleManager>
{
    /// <summary>
    /// 资源依赖关系表 Key 是path的crc value是ResourceItem 
    /// </summary>
    protected Dictionary<uint, ResourceItem> m_ResourceItemDic = new Dictionary<uint, ResourceItem>();
    /// <summary>
    /// 存储已经加载的AssetBundle key 是AB Name的Crc, Value 是AssetBundleItem
    /// </summary>
    protected Dictionary<uint, AssetBundleItem> m_AssetBundleItemDic = new Dictionary<uint, AssetBundleItem>();
    /// <summary>
    /// AssetBundleItem类对象池
    /// </summary>
    protected ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool = ObjectManager.Instance.GetOrCreateClassObjectPool<AssetBundleItem>(500);

    protected string PackedABPath
    {
        get
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            return Application.persistentDataPath + "/Origin";
#else
            return Application.streamingAssetsPath;
#endif
        }


    }

    /// <summary>
    /// 加载AssetBundle config
    /// </summary>
    public bool LoadAssetBundleConfig()
    {
        //反序列化AssetBundle config
        string hotABPath = HotPatchManager.Instance.ComputeABPath("assetbundleconfig");
        string fullName = string.IsNullOrEmpty(hotABPath) ? PackedABPath + "/" + "assetbundleconfig" : hotABPath;
        byte[] buffer = AES.AESFileDecryptBytes(fullName, "xiaohailin");
        AssetBundle configAB = AssetBundle.LoadFromMemory(buffer);
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
        ms.Close();
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
            m_ResourceItemDic.Add(resourceItem.Crc, resourceItem);
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
        if (!m_ResourceItemDic.TryGetValue(pathCrc, out item) || item == null)
        {
            Debug.LogError("存储的AB信息中未有此项");
            return null;
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
        return m_ResourceItemDic[pathCrc];
    }



    /// <summary>
    /// 根据AB包名加载AssetBundle 
    /// </summary>
    /// <param name="abName"></param>
    /// <returns></returns>
    private AssetBundle LoadAssetBundle(string abName)
    {
        AssetBundleItem item = null;
        uint crc = Crc32.GetCrc32(abName);
        if (!m_AssetBundleItemDic.TryGetValue(crc, out item))
        {
            item = m_AssetBundleItemPool.Spawn(true);
            string hotABPath = HotPatchManager.Instance.ComputeABPath(abName);
            string fullName = string.IsNullOrEmpty(hotABPath) ? PackedABPath + "/" + abName : hotABPath;
            //解密AB包
            byte[] buffer = AES.AESFileDecryptBytes(fullName, "xiaohailin");
            item.AssetBundle = AssetBundle.LoadFromMemory(buffer);
            if (item.AssetBundle == null)
            {
                Debug.LogError("未能正确加载AssetBundle,请检查！！！！！");

            }
            m_AssetBundleItemDic.Add(crc, item);
        }
        item.RefCount++;
        return item.AssetBundle;
    }

    private void UnLoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        uint crc = Crc32.GetCrc32(name);
        if (m_AssetBundleItemDic.TryGetValue(crc, out item) && item != null)
        {
            item.RefCount--;
            //引用计数清空，彻底卸载掉这个资源
            if (item.RefCount <= 0 && item.AssetBundle != null)
            {
                item.AssetBundle.Unload(true);
                item.Reset();
                m_AssetBundleItemPool.Recycle(item);
                m_AssetBundleItemDic.Remove(crc);
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

/// <summary>
/// 从AB包加载的资源的中间类
/// </summary>
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
    //是否跳转场景的时候清掉
    public bool Clear = true;
}

