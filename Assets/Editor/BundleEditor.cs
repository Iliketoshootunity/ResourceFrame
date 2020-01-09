﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


// 打包流程
//1.添加文件包路径
//2.添加Prefab的依赖项文件路径，在此过程中做AB包文件剔除，排除重复依赖的，和在文件包路径内的AB包
//3.为每个AB文件设置名字
//4.在已经打包好的AB包文件中增量剔除有差异的AB包
//5.配置XML 只记录在ABPathConfig配置的各项文件夹路径内的AB包，因为其他的用不到
//6.打包AB
//7.为5创建XML文件，创建二进制文件
//8.清空AssetBoundle Name

public class BundleEditor
{
    public static string AB_CONFIG_BYTE_PATH = "Assets/GameData/Data/AssetBundleConfig.bytes";
    public static string BUILD_TARGET_PATH = Application.dataPath + "/../AssetBundle/" + EditorUserBuildSettings.activeBuildTarget.ToString();
    public static string AB_PATH_CONFIG_PATH = "Assets/Editor/ABPathConfig.asset";

    //pregfab AssetBundle 列表
    public static Dictionary<string, List<string>> m_PrefabAssetBundles = new Dictionary<string, List<string>>();
    //Other AssetBundles  列表
    public static Dictionary<string, string> m_OtherAssetBundles = new Dictionary<string, string>();
    //资源文件列表
    //同一个资源不会添加两次(Prefab引用的资源在Other AssetBundle 包 里面时，不会添加)
    public static List<string> m_AssetFiles = new List<string>();
    //配置的AssetBundle路径列表
    public static List<string> m_ConfigAssetBundles = new List<string>();

    [MenuItem("Tool/BuildAB")]
    public static void BuildAB()
    {
        m_OtherAssetBundles.Clear();
        m_AssetFiles.Clear();
        m_PrefabAssetBundles.Clear();
        m_ConfigAssetBundles.Clear();
        ABPathConfig config = AssetDatabase.LoadAssetAtPath<ABPathConfig>(AB_PATH_CONFIG_PATH);
        if (config == null)
        {
            return;
        }

        //其他资源的AssetBundle
        foreach (var item in config.OtherAssetDirectorys)
        {
            if (m_OtherAssetBundles.ContainsKey(item.ABName))
            {
                Debug.Log("文件AB包名重复，请检查!!!!!!!!!!!!!!!!!!!!!!");
            }
            else
            {
                m_OtherAssetBundles.Add(item.ABName, item.Path);
                m_AssetFiles.Add(item.Path);
                m_ConfigAssetBundles.Add(item.Path);
            }
        }

        //Prefab资源的AssetBundle
        //查找这个路径下的所有的prefab路径的GUID
        string[] guidArray = AssetDatabase.FindAssets("t:Prefab", config.PrefabAssetDirectorys.ToArray());
        for (int i = 0; i < guidArray.Length; i++)
        {
            //根据GUID获取路径
            string prefabPath = AssetDatabase.GUIDToAssetPath(guidArray[i]);
            EditorUtility.DisplayProgressBar("查找prefab", "Path:" + prefabPath, (i + 1) * 1 / guidArray.Length);
            m_ConfigAssetBundles.Add(prefabPath);
            //添加到要打包的路径
            if (!ContainAllAssetPath(prefabPath))
            {
                //获取依赖项
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                string[] denpendAssePathtArr = AssetDatabase.GetDependencies(prefabPath);
                List<string> denpendAssetPathList = new List<string>();
                for (int j = 0; j < denpendAssePathtArr.Length; j++)
                {
                    if (!ContainAllAssetPath(denpendAssePathtArr[j]) && !denpendAssePathtArr[j].EndsWith(".cs"))
                    {
                        m_AssetFiles.Add(denpendAssePathtArr[j]);
                        denpendAssetPathList.Add(denpendAssePathtArr[j]);
                    }
                }
                if (m_PrefabAssetBundles.ContainsKey(go.name))
                {
                    Debug.LogErrorFormat("%s 与 %s 的预制体重名 这个预制体将不会参与打包，请检查", prefabPath, m_PrefabAssetBundles[go.name]);
                }
                else
                {
                    m_PrefabAssetBundles.Add(go.name, denpendAssetPathList);
                }

            }
        }

        //设置AssetBundle的后缀
        foreach (var item in m_OtherAssetBundles)
        {
            SetABName(item.Key, item.Value);
        }

        foreach (var item in m_PrefabAssetBundles)
        {
            SetABName(item.Key, item.Value);
        }

        BuildAssetBundle();

        //清除AB包名,这是因为在设置是更改AB包名，会造成资源的meta文件修改，不利于上传
        //把一个已经标记Bundle Name 资源移动到文件夹中的时候，下次打包会出错
        string[] oldNames = AssetDatabase.GetAllAssetBundleNames();
        for (int i = 0; i < oldNames.Length; i++)
        {
            AssetDatabase.RemoveAssetBundleName(oldNames[i], true);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();
    }


    public static void BuildAssetBundle()
    {
        //设置各个资源的所属的AB包名之后

        //找到所有的AssetBundle
        string[] allAbName = AssetDatabase.GetAllAssetBundleNames();
        //所有的资源，key是资源的全路径 value是AB包名
        Dictionary<string, string> assetPathDic = new Dictionary<string, string>();
        for (int i = 0; i < allAbName.Length; i++)
        {
            //获取这个AB包含的资源
            string[] dependAssetArr = AssetDatabase.GetAssetPathsFromAssetBundle(allAbName[i]);
            for (int j = 0; j < dependAssetArr.Length; j++)
            {
                if (!dependAssetArr[j].EndsWith(".cs"))
                {
                    assetPathDic.Add(dependAssetArr[j], allAbName[i]);
                }
                EditorUtility.DisplayProgressBar("AB下的资源", "资源" + dependAssetArr[j], j + 1 / dependAssetArr.Length);
            }

        }
        //增量删除多余的AB包
        IncrementalDeleteAB();
        //生成自己的配置表
        AssetBundleConfig config = CreateConfig(assetPathDic);

        //序列成XML
        string xmlPath = Application.dataPath + "/AssetBundleConfig.xml";
        if (File.Exists(xmlPath))
        {
            File.Delete(xmlPath);
        }
        FileStream fs = new FileStream(xmlPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
        XmlSerializer xs = new XmlSerializer(typeof(AssetBundleConfig));
        xs.Serialize(sw, config);
        sw.Close();
        fs.Close();



        //序列化成二进制文件
        FileStream fs1 = new FileStream(AB_CONFIG_BYTE_PATH, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        fs1.Seek(0, SeekOrigin.Begin);
        fs1.SetLength(0);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs1, config);
        fs1.Close();
        //刷新上一步后的资源目录
        AssetDatabase.Refresh();
        SetABName("assetbundleconfig", AB_CONFIG_BYTE_PATH);

        if (!Directory.Exists(BUILD_TARGET_PATH))
        {
            Directory.CreateDirectory(BUILD_TARGET_PATH);
        }

        //打包
        BuildPipeline.BuildAssetBundles(BUILD_TARGET_PATH, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
    }
    /// <summary>
    /// 增量删除多余的AB包
    /// </summary>
    public static void IncrementalDeleteAB()
    {
        string[] allABName = AssetDatabase.GetAllAssetBundleNames();
        //获取旧的AB包文件信息
        DirectoryInfo di = new DirectoryInfo(BUILD_TARGET_PATH);
        FileInfo[] files = di.GetFiles("*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            //如果旧的和新的一致，则不需要删除
            //即使里面的内容不同了，只要路径引用，它内部打包的时候，会自动比较差异，增量打包，提高打包速度
            if (ContainABName(files[i].Name, allABName) || files[i].FullName.EndsWith(".meta") || files[i].Name.EndsWith(".manifest") || files[i].Name.EndsWith("assetbundleconfig"))
            {
                continue;
            }
            Debug.Log(files[i].FullName + "已经改名,或者删除了");
            if (File.Exists(files[i].FullName))
            {
                File.Delete(files[i].FullName);
            }
            if (File.Exists(files[i].FullName + ".manifest"))
            {
                File.Exists(files[i].FullName + ".manifest");
            }
        }

    }

    public static bool ContainABName(string name, string[] strs)
    {
        for (int i = 0; i < strs.Length; i++)
        {
            if (name == strs[i])
                return true;

        }
        return false;
    }

    public static void SetABName(string name, string path)
    {
        AssetImporter ai = AssetImporter.GetAtPath(path);
        if (ai == null)
        {
            Debug.LogErrorFormat("%s 找不到资源", path);
        }
        else
        {
            ai.assetBundleName = name;
        }
    }

    public static void SetABName(string name, List<string> pathList)
    {
        for (int i = 0; i < pathList.Count; i++)
        {
            SetABName(name, pathList[i]);
        }
    }

    public static bool ContainAllAssetPath(string path)
    {
        for (int i = 0; i < m_AssetFiles.Count; i++)
        {
            if (path == m_AssetFiles[i])
            {
                return true;
            }
            //如果这个是个文件夹
            if (Directory.Exists(m_AssetFiles[i]))
            {
                //如果此路径在这个文件夹内
                if (path.Contains(m_AssetFiles[i]))
                {
                    return true;
                }
            }
        }
        return false;
    }
    /// <summary>
    /// 生成资源配置表
    /// </summary>
    /// <param name="assetPathDic">所有的资源Key 是路径，Value是AB包名</param>
    public static AssetBundleConfig CreateConfig(Dictionary<string, string> assetPathDic)
    {
        AssetBundleConfig config = new AssetBundleConfig();
        config.ABList = new List<BaseAB>();
        foreach (var item in assetPathDic)
        {
            if (!ValidPath(item.Key))
            {
                //引用的资源不在指定的文件夹内
                Debug.LogError(item.Key + "不在指定的文件夹内，请设置");
                continue;
            }
            BaseAB baseAB = new BaseAB();
            baseAB.AssetName = item.Key.Substring(item.Key.LastIndexOf("/") + 1);
            baseAB.ABName = item.Value;
            baseAB.Path = item.Key;
            baseAB.Crc = Crc32.GetCrc32(baseAB.Path);
            baseAB.Depends = new List<string>();
            //资源依赖项
            string[] allDependAssetPathArr = AssetDatabase.GetDependencies(baseAB.Path);
            for (int i = 0; i < allDependAssetPathArr.Length; i++)
            {
                //自身
                if (allDependAssetPathArr[i] == baseAB.Path)
                    continue;
                //不添加脚本依赖
                if (allDependAssetPathArr[i].EndsWith(".cs"))
                    continue;
                string abName = "";
                //判断资源的依赖项
                if (assetPathDic.TryGetValue(allDependAssetPathArr[i], out abName))
                {
                    //如果这个依赖的资源和本身在同一个包，则不添加
                    //按照规则，这是Prefab依赖的资源
                    if (abName == baseAB.ABName)
                    {
                        continue;
                    }
                    if (!baseAB.Depends.Contains(abName))
                    {
                        baseAB.Depends.Add(abName);
                    }
                }
            }
            config.ABList.Add(baseAB);

        }
        return config;

    }


    public static bool ValidPath(string path)
    {
        for (int i = 0; i < m_ConfigAssetBundles.Count; i++)
        {
            if (path.Contains(m_ConfigAssetBundles[i]))
            {
                return true;
            }
        }
        return false;
    }

    #region 热更信息相关

    /// <summary>
    /// 保存AssetBundleMd5信息
    /// </summary>
    public static void SaveABMd5()
    {
        //找到打包后Assetbundle的文件信息
        ABMD5 abmds = new ABMD5();
        abmds.ABMD5BaseList = new List<ABMD5Base>();
        string[] files = Directory.GetFiles(BUILD_TARGET_PATH);
        for (int i = 0; i < files.Length; i++)
        {
            //剔除不需要的文件
            if (files[i].EndsWith(".meta") || files[i].EndsWith(""))
            {
                continue;
            }
            FileInfo file = new FileInfo(files[i]);
            ABMD5Base ab = new ABMD5Base();
            ab.Name = file.Name;
            abmds.ABMD5BaseList.Add(ab);
        }
        //序列化成二进制文件
        string path = Application.dataPath + "/Resources/ABMD5_Version" + PlayerSettings.bundleVersion;
        FileStream fs1 = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        fs1.Seek(0, SeekOrigin.Begin);
        fs1.SetLength(0);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs1, abmds);
        fs1.Close();      
    }
    #endregion

}
