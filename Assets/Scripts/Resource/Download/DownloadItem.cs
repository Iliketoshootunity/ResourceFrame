using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 下载基类
/// </summary>
public abstract class DownloadItem
{
    /// <summary>
    /// 下载的路径
    /// </summary>
    protected string m_Url;

    public string Url
    {
        get
        {
            return m_Url;
        }
    }
    /// <summary>
    /// 保存的路径(不包含文件名)
    /// </summary>
    protected string m_SavePath;

    public string SavePath
    {
        get
        {
            return m_SavePath;
        }
    }
    /// <summary>
    /// 文件名(不包含后缀名)
    /// </summary>
    protected string m_FileNameWithoutExtention;

    public string FileNameWithoutExtention
    {
        get
        {
            return m_FileNameWithoutExtention;
        }
    }
    /// <summary>
    /// 文件后缀
    /// </summary>
    protected string m_FileExtension;

    public string FileExtension
    {
        get
        {
            return m_FileExtension;
        }
    }

    /// <summary>
    ///  文件名(包含后缀名)
    /// </summary>
    protected string m_FileName;

    public string FileName
    {
        get
        {
            return m_FileName;
        }
    }
    /// <summary>
    /// 保存的文件路径(全路径)
    /// </summary>
    protected string m_SaveFile;

    public string SaveFile
    {
        get
        {
            return m_SaveFile;
        }
    }

    /// <summary>
    /// 是否开始下载
    /// </summary>
    protected bool m_StartLoad;

    public bool StartLoad
    {
        get
        {
            return m_StartLoad;
        }
    }

    public DownloadItem(string url, string savePath)
    {
        m_Url = url;
        m_SavePath = savePath;
        m_FileNameWithoutExtention = Path.GetFileNameWithoutExtension(url);
        m_FileExtension = Path.GetExtension(url);
        m_FileName = Path.GetFileName(m_Url);
        m_SaveFile = string.Format("{0}/{1}", m_SavePath, m_FileName);
        m_StartLoad = false;
    }

    public virtual IEnumerator Download(Action callBack = null)
    {
        yield return null;
    }

    public abstract long GetCurLength();

    public abstract long GetLength();

    public abstract float GetProgress();

    public abstract void Destory();
}
