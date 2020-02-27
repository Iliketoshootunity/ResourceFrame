using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Networking;

//1.先和本地打包时的ABMD5文件做对比，得到本此热更需要修改的内容
//2.再和之前下载的文件比，得到真正需要下载的内容
//3.(解包)安卓平台因为不能用流数据操纵StreamAssets,所有得下载到Application.persistentDataPath +/Origin
public class HotPatchManager : Singleton<HotPatchManager>
{
    /// <summary>
    /// 当前版本
    /// </summary>
    private string m_CurVersion;
    /// <summary>
    /// 当前包名
    /// </summary>
    private string m_CurPackage;

    /// <summary>
    /// 用于携程的Mono
    /// </summary>
    private MonoBehaviour m_Mono;

    /// <summary>
    /// 读取下来的服务器信息
    /// </summary>
    private ServerInfo m_ServerInfo;

    /// <summary>
    ///本地的的服务器信息
    /// </summary>
    private ServerInfo m_LocalInfo;
    /// <summary>
    /// 当前版本信息
    /// </summary>
    private VersionInfo m_GameVersion;

    /// <summary>
    /// 下载下来的服务器热歌信息XML存储路径
    /// </summary>
    private string m_ServerXMLPath = Application.persistentDataPath + "/ServerInfo.xml";
    /// <summary>
    /// 拷贝自m_ServerXMLPath的本地热更信息路径
    /// </summary>
    private string m_LocalXMLPath = Application.persistentDataPath + "/LocalInfo.xml";
    /// <summary>
    /// 所有的热更包
    /// </summary>
    private Dictionary<string, Patch> m_HotPatchDic = new Dictionary<string, Patch>();
    /// <summary>
    /// 当前所有需要热更的包（包括曾经热更过，因为都是和打APP包时的资源做对比）
    /// </summary>
    private Patchs m_CurGamePatch;
    /// <summary>
    /// 需要下载的热更包
    /// </summary>
    private List<Patch> m_DownLoadPatchList = new List<Patch>();
    /// <summary>
    /// 需要下载的热更包
    /// </summary>
    private Dictionary<string, Patch> m_DownLoadPatchDic = new Dictionary<string, Patch>();
    /// <summary>
    /// 需要下载的热更包 名字和MD5映射
    /// </summary>
    private Dictionary<string, string> m_DownloadMD5Dic = new Dictionary<string, string>();
    /// <summary>
    /// 从服务器下载下来的资源的路径
    /// </summary>
    private string m_DownloadPath = Application.persistentDataPath + "/Download";
    /// <summary>
    /// 已经下载完的热更包
    /// </summary>
    private List<Patch> m_AlreadyDownload = new List<Patch>();
    /// <summary>
    /// 正在下载的AB包
    /// </summary>
    private DownloadItem m_DownloadingAB;
    /// <summary>
    /// 是否已经开始下载
    /// </summary>
    private bool m_StartDownload;
    /// <summary>
    /// 读取服务器信息错误回调
    /// </summary>
    public Action ReadServerInfoError;
    /// <summary>
    /// 下载资源出错
    /// </summary>
    public Action<string> DownloadItemError;
    /// <summary>
    /// 因为出错重新下载的次数
    /// </summary>
    private int m_RestartDownloadCount = 0;
    /// <summary>
    /// 因为出错重新下载的最大次数
    /// </summary>
    private const int RestartDownloadMaxNumber = 4;
    /// <summary>
    /// 需要下载的个数
    /// </summary>
    private int m_DownloadCount;
    /// <summary>
    /// 需要下载的数据大小 KB
    /// </summary>
    private float m_DownloadSize;

    public string CurVersion
    {
        get
        {
            return m_CurVersion;
        }
    }

    public string CurPackage
    {
        get
        {
            return m_CurPackage;
        }
    }

    public float DownloadSize
    {
        get
        {
            return m_DownloadSize;
        }
    }


    ////////////////////////////////////////////
    //安卓平台解包
    /// <summary>
    /// 在包里的MD5
    /// </summary>
    private Dictionary<string, ABMD5Base> m_PackedDic = new Dictionary<string, ABMD5Base>();
    /// <summary>
    /// 需要解包的文件
    /// </summary>
    private List<string> m_UnPackedList = new List<string>();
    /// <summary>
    /// 解包后放的资源目录
    /// </summary>
    private string m_OriginPath = Application.persistentDataPath + "/Origin";
    /// <summary>
    /// 需要解包的大小
    /// </summary>
    private float m_UnPackedSize;
    /// <summary>
    /// 已经解包的大小
    /// </summary>
    private float m_AlreadyUnPackedSize;

    /// <summary>
    /// 是否开始劫包
    /// </summary>
    private bool m_StartUnPacked;

    public float UnPackedSize
    {
        get
        {
            return m_UnPackedSize;
        }
    }

    public float AlreadyUnPackedSize
    {
        get
        {
            return m_AlreadyUnPackedSize;
        }
    }

    public bool GetStartUnPacked
    {
        get
        {
            return m_StartUnPacked;
        }
    }
    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="mono"></param>
    public void Init(MonoBehaviour mono)
    {
        m_Mono = mono;
        ReadMD5();
    }

    /// <summary>
    /// 读取MD5
    /// </summary>
    public void ReadMD5()
    {
        m_PackedDic.Clear();
        TextAsset amd5Text = Resources.Load<TextAsset>("ABMD5");
        if (amd5Text == null)
        {
            Debug.LogError("ABMD5 读取不到");
            return;
        }
        using (MemoryStream ms = new MemoryStream(amd5Text.bytes))
        {
            BinaryFormatter bf = new BinaryFormatter();
            ABMD5 md5 = (ABMD5)bf.Deserialize(ms);
            foreach (var item in md5.ABMD5BaseList)
            {
                m_PackedDic.Add(item.Name, item);
            }
        }
    }

    /// <summary>
    /// 计算需要解包的文件
    /// </summary>
    /// <returns></returns>
    public bool ComputeUnPackedFile()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        if (!Directory.Exists(m_OriginPath))
        {
            Directory.CreateDirectory(m_OriginPath);
        }
        m_UnPackedList.Clear();
        foreach (var item in m_PackedDic)
        {
            string filePath = m_OriginPath + "/" + item.Value.Name;
            if (!File.Exists(filePath))
            {
                m_UnPackedList.Add(item.Key);
            }
            else
            {
                string md5 = MD5Manager.Instance.BuildFileMd5(filePath);
                if (md5 != item.Value.Md5)
                {
                    m_UnPackedList.Add(item.Key);
                }
            }
        }

        foreach (var item in m_PackedDic)
        {
            if (m_UnPackedList.Contains(item.Key))
            {
                m_UnPackedSize += item.Value.Size;
            }
        }

        return m_UnPackedList.Count > 0;
#else
        return false;
#endif

    }

    /// <summary>
    /// 获取解包进度
    /// </summary>
    /// <returns></returns>
    public float GetUnPackedProgress()
    {
        return m_AlreadyUnPackedSize / m_UnPackedSize;
    }
    /// <summary>
    /// 解包(只有安卓需要解包)
    /// 因为android读取StreamingAssets只能用www/UnityWebRequest
    /// </summary>
    public void StartUnPacked(Action callBack)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        m_StartUnPacked = true;
        m_Mono.StartCoroutine(StartUnPackedIE(callBack));
#else
#endif
    }
    /// <summary>
    /// 解包携程
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartUnPackedIE(Action callBack)
    {
        foreach (var item in m_UnPackedList)
        {
            string packedPath = Application.streamingAssetsPath + "/" + item;
            UnityWebRequest webRequest = UnityWebRequest.Get(packedPath);
            webRequest.timeout = 300;
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
                Debug.LogError("Start UnPacked Error" + webRequest.error);
            }
            else
            {
                byte[] buffer = webRequest.downloadHandler.data;
                FileTool.CreateFile(m_OriginPath + "/" + item, buffer);
            }

            if (m_PackedDic.ContainsKey(item))
            {
                m_AlreadyUnPackedSize += m_PackedDic[item].Size;
            }
            webRequest.Dispose();
        }

        m_StartUnPacked = false;

        if (callBack != null)
        {
            callBack();
        }
    }



    /// <summary>
    /// 检查是否需要热更
    /// </summary>
    /// <param name="hotCallBack"></param>
    public void CheckVersion(Action<bool> hotCallBack = null)
    {
        ReadLocalVersion();
        m_Mono.StartCoroutine(ReadServerXml(() =>
        {
            if (m_ServerInfo == null)
            {
                if (ReadServerInfoError != null)
                {
                    ReadServerInfoError();
                }
            }
            //获取本地版本信息
            foreach (var item in m_ServerInfo.VersionInfo)
            {
                if (item.Version == CurVersion)
                {
                    m_GameVersion = item;
                    break;
                }
            }
            if (m_GameVersion == null)
            {
                Debug.LogError("当前版本未在服务器上记录");
                return;
            }

            GetHotAB();

            if (CheckLocalAndServerInfo())
            {
                ComputeDownLoadHotAB();

                if (File.Exists(m_ServerXMLPath))
                {
                    if (File.Exists(m_LocalXMLPath))
                    {
                        File.Delete(m_LocalXMLPath);
                    }
                    File.Move(m_ServerXMLPath, m_LocalXMLPath);
                }
            }
            else
            {
                //防止文件损坏的情况
                ComputeDownLoadHotAB();
            }


            if (hotCallBack != null)
            {
                hotCallBack(m_DownLoadPatchList.Count > 0);
            }

            m_DownloadCount = m_DownLoadPatchList.Count;
            for (int i = 0; i < m_DownLoadPatchList.Count; i++)
            {
                m_DownloadSize += m_DownLoadPatchList[i].Size;
            }

        }));
    }

    /// <summary>
    /// 读取本地打包时的版本
    /// </summary>
    public void ReadLocalVersion()
    {
        TextAsset versionText = Resources.Load<TextAsset>("version");
        if (versionText == null)
        {
            Debug.LogError("未读取到本地版本");
            return;
        }
        string[] all = versionText.text.Split('\n');
        if (all.Length > 0)
        {
            string[] infoList = all[0].Split('|');
            if (infoList.Length >= 2)
            {
                m_CurVersion = infoList[0].Split(':')[1];
                m_CurPackage = infoList[1].Split(':')[1];
            }

        }
    }

    /// <summary>
    /// 读取
    /// </summary>
    /// <param name="callBack"></param>
    private IEnumerator ReadServerXml(Action callBack = null)
    {
        string xmlUrl = "http://127.0.0.1/ServerInfo.xml";
        UnityWebRequest webRequest = UnityWebRequest.Get(xmlUrl);
        webRequest.timeout = 300;
        yield return webRequest.SendWebRequest();
        if (webRequest.isNetworkError)
        {
            Debug.LogError("ReadServerXml Error:" + webRequest.error);
        }
        else
        {
            FileTool.CreateFile(m_ServerXMLPath, webRequest.downloadHandler.data);

            using (FileStream fs = new FileStream(m_ServerXMLPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(ServerInfo));
                    m_ServerInfo = (ServerInfo)xs.Deserialize(sr);
                }
            }

        }

        if (callBack != null)
        {
            callBack();
        }
    }

    /// <summary>
    /// 检查本地和服务器上的版本,判断是否更新
    /// </summary>
    /// <returns></returns>
    private bool CheckLocalAndServerInfo()
    {
        if (!File.Exists(m_LocalXMLPath))
        {
            return true;
        }
        else
        {
            using (FileStream fs = new FileStream(m_LocalXMLPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(ServerInfo));
                    m_LocalInfo = (ServerInfo)xs.Deserialize(sr);
                    VersionInfo localVersion = null;
                    foreach (var item in m_LocalInfo.VersionInfo)
                    {
                        if (item.Version == CurVersion)
                        {
                            localVersion = item;
                        }
                    }
                    if (localVersion != null && localVersion.Patchs != null && localVersion.Patchs.Length > 0 && localVersion.Patchs[localVersion.Patchs.Length - 1] != null && m_GameVersion != null && m_GameVersion.Patchs != null && m_GameVersion.Patchs.Length > 0 && m_GameVersion.Patchs[m_GameVersion.Patchs.Length - 1] != null)
                    {
                        if (localVersion.Patchs[localVersion.Patchs.Length - 1].Version != m_GameVersion.Patchs[m_GameVersion.Patchs.Length - 1].Version)
                        {
                            return true;
                        }
                    }
                }
            }

        }

        return false;
    }
    /// <summary>
    /// 获取热更的AB包
    /// </summary>
    private void GetHotAB()
    {
        m_HotPatchDic.Clear();
        //本地必须有东西
        if (m_GameVersion != null && m_GameVersion.Patchs != null && m_GameVersion.Patchs.Length > 0)
        {
            //每次只获取最新的一次打热更包的情况
            //因为每次打的热更包都只是和打包时资源的做比较
            //所以可以知道所有需要热更的信息（包括已经热更过了的）
            Patchs lastPatchs = m_GameVersion.Patchs[m_GameVersion.Patchs.Length - 1];
            if (lastPatchs != null && lastPatchs.Files != null)
            {
                foreach (var item in lastPatchs.Files)
                {
                    m_HotPatchDic.Add(item.Name, item);
                }
            }
            m_CurGamePatch = lastPatchs;
        }
        else
        {
            Debug.LogError("本地版本内容为空");
        }
    }

    /// <summary>
    /// 计算需要下载的AB
    /// </summary>
    private void ComputeDownLoadHotAB()
    {
        m_DownLoadPatchList.Clear();
        m_DownLoadPatchDic.Clear();
        m_DownloadMD5Dic.Clear();
        foreach (var item in m_CurGamePatch.Files)
        {
            if ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) && item.Platform == "StandaloneWindows64")
            {
                AddDownLoadList(item);
            }
            else if ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.Android) && item.Platform == "Android")
            {
                AddDownLoadList(item);
            }
            else if ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.IPhonePlayer) && item.Platform == "IOS")
            {
                AddDownLoadList(item);
            }
        }
    }

    /// <summary>
    /// 添加需要下载的平台
    /// </summary>
    /// <param name="patch"></param>
    private void AddDownLoadList(Patch patch)
    {
        //对照这个资源是否需要下载
        string filePath = m_DownloadPath + "/" + patch.Name;
        if (!File.Exists(filePath))
        {
            //没有说明需要下载
            m_DownLoadPatchList.Add(patch);
            m_DownLoadPatchDic.Add(patch.Name, patch);
            m_DownloadMD5Dic.Add(patch.Name, patch.MD5);
        }
        else
        {
            //资源的md5码不一致说明需要下载
            string md5 = MD5Manager.Instance.BuildFileMd5(filePath);
            if (patch.MD5 != md5)
            {
                m_DownLoadPatchList.Add(patch);
                m_DownLoadPatchDic.Add(patch.Name, patch);
                m_DownloadMD5Dic.Add(patch.Name, patch.MD5);
            }
        }

    }

    /// <summary>
    /// 开始下载
    /// </summary>
    /// <param name="callBack"></param>
    /// <returns></returns>
    public IEnumerator StartDownload(Action callBack, List<Patch> downloadPatchList = null)
    {
        if (!Directory.Exists(m_DownloadPath))
        {
            Directory.CreateDirectory(m_DownloadPath);
        }
        m_AlreadyDownload.Clear();
        m_StartDownload = false;
        if (downloadPatchList != null)
        {
            m_DownLoadPatchList = downloadPatchList;
        }
        List<DownloadAssetBundle> downloadAssts = new List<DownloadAssetBundle>();
        foreach (var item in m_DownLoadPatchList)
        {
            DownloadAssetBundle d = new DownloadAssetBundle(item.URL, m_DownloadPath);
            downloadAssts.Add(d);
        }
        foreach (var item in downloadAssts)
        {
            m_DownloadingAB = item;
            yield return m_Mono.StartCoroutine(item.Download());
            item.Destory();
            Patch patch = null;
            if (m_DownLoadPatchDic.TryGetValue(item.FileName, out patch))
            {
                m_AlreadyDownload.Add(patch);
            }
        }

        //检查下载文件的MD5
        CheckDownloadMD5(downloadAssts, callBack);
    }

    /// <summary>
    /// 对已经下载完的包进行MD5校验，查看是否资源和XML表的数据是否一致
    /// 如果不一致，重新下载，重新下载有次数限制
    /// </summary>
    /// <param name="downloadAssts"></param>
    /// <param name="callBack"></param>
    private void CheckDownloadMD5(List<DownloadAssetBundle> downloadAssts, Action callBack)
    {
        List<Patch> m_DownloadList = new List<Patch>();
        for (int i = 0; i < downloadAssts.Count; i++)
        {
            string xmlMd5 = "";
            if (m_DownloadMD5Dic.TryGetValue(downloadAssts[i].FileName, out xmlMd5))
            {
                string downloadMd5 = MD5Manager.Instance.BuildFileMd5(downloadAssts[i].SaveFile);
                //检查下载下来的包的MD5和XML的MD5的数据并不一致，则需要重新下载
                if (downloadMd5 != xmlMd5)
                {
                    Patch patch = FindPatchByName(downloadAssts[i].FileName);
                    m_DownloadList.Add(patch);
                }
            }
            else
            {
                Debug.LogError(string.Format("{0}未在下载的字段中找到", downloadAssts[i].FileName));
            }
        }
        if (m_DownloadList.Count <= 0)
        {
            //下载的文件无误
            if (callBack != null)
            {
                callBack();
            }
            m_StartDownload = false;
        }
        else
        {
            if (m_RestartDownloadCount >= RestartDownloadMaxNumber)
            {
                m_DownloadMD5Dic.Clear();
                string allName = "";
                for (int i = 0; i < m_DownloadList.Count; i++)
                {
                    allName += m_DownloadList[i].Name + ";";
                }
                if (DownloadItemError != null)
                {
                    DownloadItemError(allName);
                }
                m_StartDownload = false;
            }
            else
            {
                m_DownloadMD5Dic.Clear();
                for (int i = 0; i < m_DownloadList.Count; i++)
                {
                    m_DownloadMD5Dic.Add(m_DownloadList[i].Name, m_DownloadList[i].MD5);
                }
                m_RestartDownloadCount++;
                //重新下载出错的资源
                m_Mono.StartCoroutine(StartDownload(callBack, m_DownloadList));
            }

        }
    }


    /// <summary>
    /// 根据名字查找Patch
    /// </summary>
    /// <param name="patchName"></param>
    /// <returns></returns>
    private Patch FindPatchByName(string patchName)
    {
        Patch patch = null;
        if (m_DownLoadPatchDic.TryGetValue(patchName, out patch))
        {
            return patch;
        }
        return patch;
    }

    /// <summary>
    /// 获取已经下载的资源
    /// </summary>
    /// <returns></returns>
    public float GetAlreadyDownloadSize()
    {
        float size = 0;
        for (int i = 0; i < m_AlreadyDownload.Count; i++)
        {
            size += m_AlreadyDownload[i].Size;
        }
        if (m_DownloadingAB != null)
        {
            Patch patch = null;
            if (m_DownLoadPatchDic.TryGetValue(m_DownloadingAB.FileName, out patch))
            {
                if (!m_AlreadyDownload.Contains(patch))
                {
                    size += m_DownloadingAB.GetCurLength();
                }
            }
        }
        return size;

    }

    /// <summary>
    /// 获取已经下载的进度
    /// </summary>
    /// <returns></returns>
    public float GetAlreadyDownloadProgress()
    {
        return GetAlreadyDownloadSize() / DownloadSize;
    }

    /// <summary>
    /// 获取在Download目录下的Ab包的路径
    /// </summary>
    /// <param name="abName"></param>
    /// <returns></returns>
    public string ComputeABPath(string abName)
    {
        Patch patch = null;
        if (m_HotPatchDic.TryGetValue(abName, out patch) && patch != null)
        {
            return m_DownloadPath + "/" + abName;
        }
        return "";
    }

}


public class FileTool
{
    public static void CreateFile(string filePath, byte[] dataBuffer)
    {
        //if (File.Exists(filePath))
        //{
        //    File.Delete(filePath);
        //}
        //using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        //{
        //    using (StreamWriter sw = new StreamWriter(fs))
        //    {
        //        sw.Write(dataBuffer);
        //    }
        //}

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        FileInfo file = new FileInfo(filePath);
        Stream stream = file.Create();
        stream.Write(dataBuffer, 0, dataBuffer.Length);
        stream.Close();
        stream.Dispose();
    }
}