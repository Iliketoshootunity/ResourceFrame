using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DownloadAssetBundle : DownloadItem
{
    private UnityWebRequest m_WebRequest;
    public DownloadAssetBundle(string url, string savePath) : base(url, savePath)
    {

    }

    public override IEnumerator Download(Action callBack = null)
    {
        m_WebRequest = new UnityWebRequest(m_Url);
        m_WebRequest.timeout = 30;
        yield return m_WebRequest.SendWebRequest();
        if (m_WebRequest.isNetworkError)
        {
            Debug.LogError("ReadServerXml Error:" + m_WebRequest.error);
        }
        else
        {
            FileTool.CreateFile(m_SaveFile, m_WebRequest.downloadHandler.data);

        }
    }
    public override void Destory()
    {
        if (m_WebRequest != null)
        {
            m_WebRequest.Dispose();
            m_WebRequest = null;
        }
    }

    public override long GetCurLength()
    {
        if (m_WebRequest != null)
        {
            return (long)m_WebRequest.downloadedBytes;
        }
        return -1;
    }

    public override long GetLength()
    {
        return 0;
    }

    public override float GetProgress()
    {
        if (m_WebRequest != null)
        {
            return (long)m_WebRequest.downloadProgress;
        }
        return -1;
    }
}
