using System.Collections;
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
    public static string ASSETBUNDLE_CONFIG_PATH = Application.dataPath + "/GameData/Data";
    public static string BUILD_AB_PATH = Application.streamingAssetsPath;
    public static string ABPATHCONFIG_PATH = "Assets/Editor/ABPathConfig.asset";

    //所有的基于文件夹的Ab包： Key是Ab包名，value 是路径
    public static Dictionary<string, string> m_Dir_ABNameToPath = new Dictionary<string, string>();
    //所有的基于Prefab来查找资源的Ab包，key是物体名字，value是所依赖资源的路径
    public static Dictionary<string, List<string>> m_Prefab_ObjNameToDependPath = new Dictionary<string, List<string>>();
    //所有的AssetBundle文件（不是AB包，一个AB包包含多个资源），文件夹作过滤
    public static List<string> m_AllAssetBundleFile = new List<string>();
    //所有的配置的文件夹AB和Prefab AB
    public static List<string> m_ConfigFileList = new List<string>();

    [MenuItem("Tool/BuildAB")]
    public static void BuildAB()
    {
        m_Dir_ABNameToPath.Clear();
        m_AllAssetBundleFile.Clear();
        m_Prefab_ObjNameToDependPath.Clear();
        m_ConfigFileList.Clear();
        ABPathConfig config = AssetDatabase.LoadAssetAtPath<ABPathConfig>(ABPATHCONFIG_PATH);
        if (config == null)
        {
            return;
        }

        //文件夹
        foreach (var item in config.DirPathArr)
        {
            if (m_Dir_ABNameToPath.ContainsKey(item.ABName))
            {
                Debug.Log("文件AB包名重复，请检查!!!!!!!!!!!!!!!!!!!!!!");
            }
            else
            {
                m_Dir_ABNameToPath.Add(item.ABName, item.Path);
                m_AllAssetBundleFile.Add(item.Path);
                m_ConfigFileList.Add(item.Path);
            }
        }

        //Pregab

        //查找这个路径下的所有的prefab路径的GUID
        string[] guidArray = AssetDatabase.FindAssets("t:Prefab", config.PrefabDirPathArr.ToArray());
        for (int i = 0; i < guidArray.Length; i++)
        {
            //根据GUID获取路径
            string prefabPath = AssetDatabase.GUIDToAssetPath(guidArray[i]);
            EditorUtility.DisplayProgressBar("查找prefab", "Path:" + prefabPath, (i + 1) * 1 / guidArray.Length);
            m_ConfigFileList.Add(prefabPath);
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
                        m_AllAssetBundleFile.Add(denpendAssePathtArr[j]);
                        denpendAssetPathList.Add(denpendAssePathtArr[j]);
                    }
                }
                if (m_Prefab_ObjNameToDependPath.ContainsKey(go.name))
                {
                    Debug.LogErrorFormat("%s 与 %s 的预制体重名 这个预制体将不会参与打包，请检查", prefabPath, m_Prefab_ObjNameToDependPath[go.name]);
                }
                else
                {
                    m_Prefab_ObjNameToDependPath.Add(go.name, denpendAssetPathList);
                }

            }
        }

        //设置AssetBundle的后缀
        foreach (var item in m_Dir_ABNameToPath)
        {
            SetABName(item.Key, item.Value);
        }

        foreach (var item in m_Prefab_ObjNameToDependPath)
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
        //找到所有的AssetBundle
        string[] allAbName = AssetDatabase.GetAllAssetBundleNames();
        //key是资源的全路径 value是包名
        Dictionary<string, string> assetPathDic = new Dictionary<string, string>();
        for (int i = 0; i < allAbName.Length; i++)
        {
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
        FileStream fs = new FileStream(Application.dataPath + "/AssetBundleConfig.xml", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8);
        XmlSerializer xs = new XmlSerializer(typeof(AssetBundleConfig));
        xs.Serialize(sw, config);
        sw.Close();
        fs.Close();

        //序列化成二进制文件
        FileStream fs1 = new FileStream(ASSETBUNDLE_CONFIG_PATH + "/AssetBundleConfig.bytes", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(fs1, config);
        fs1.Close();
        //打包
        BuildPipeline.BuildAssetBundles(BUILD_AB_PATH, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
    }
    /// <summary>
    /// 增量删除多余的AB包
    /// </summary>
    public static void IncrementalDeleteAB()
    {
        string[] allABName = AssetDatabase.GetAllAssetBundleNames();
        DirectoryInfo di = new DirectoryInfo(BUILD_AB_PATH);
        FileInfo[] fileIfos = di.GetFiles();
        for (int i = 0; i < fileIfos.Length; i++)
        {
            if (m_AllAssetBundleFile.Contains(fileIfos[i].FullName) || fileIfos[i].FullName.EndsWith(".meta"))
            {
                continue;
            }
            if (File.Exists(fileIfos[i].FullName))
            {
                Debug.Log(fileIfos[i].FullName + "已经改名");
                File.Delete(fileIfos[i].FullName);
            }
        }

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
        for (int i = 0; i < m_AllAssetBundleFile.Count; i++)
        {
            if (path == m_AllAssetBundleFile[i])
            {
                return true;
            }
            //path在m_AllFileAB[i],且m_AllFileAB[i]后无路径，说明且m_AllFileAB[i]是个需要打包的文件夹
            //且path在这个文件夹内，说明不再需要被打包
            if (path.Contains(m_AllAssetBundleFile[i]))
            {
                if (path.Replace(m_AllAssetBundleFile[i], "")[0] == '/')
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
    /// <param name="assetPathDic"></param>
    public static AssetBundleConfig CreateConfig(Dictionary<string, string> assetPathDic)
    {
        AssetBundleConfig config = new AssetBundleConfig();
        config.ABList = new List<BaseAB>();
        foreach (var item in assetPathDic)
        {
            if (!ValidPath(item.Key))
            {
                continue;
            }
            BaseAB baseAB = new BaseAB();
            baseAB.AssetName = item.Key.Substring(item.Key.LastIndexOf("/") + 1);
            baseAB.ABName = item.Value;
            baseAB.Path = item.Key;
            baseAB.Crc = Crc32.GetCrc32(baseAB.Path);
            baseAB.Depends = new List<string>();
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
                if (assetPathDic.TryGetValue(allDependAssetPathArr[i], out abName))
                {
                    //如果这个依赖的资源和本身在同一个包，则不添加依赖
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
        for (int i = 0; i < m_ConfigFileList.Count; i++)
        {
            if (path.Contains(m_ConfigFileList[i]))
            {
                return true;
            }
        }
        return false;
    }

}
