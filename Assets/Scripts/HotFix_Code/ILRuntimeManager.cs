using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using System.IO;

public class ILRuntimeManager : Singleton<ILRuntimeManager>
{
    private AppDomain m_AppDomain;
    private const string DLLPATH= "Assets/GameData/HotCode/HotFix.dll.txt";
    private const string PDBPath = "Assets/GameData/HotCode/HotFix.pdb.txt";
    public AppDomain AppDomain
    {
        get
        {
            return m_AppDomain;
        }
    }

    public void Init()
    {
        LoadHotHitAssembly();
    }

    /// <summary>
    /// 加载热更的程序集
    /// </summary>
    private void LoadHotHitAssembly()
    {
        //整个工程只有一个AppDomain
        m_AppDomain = new ILRuntime.Runtime.Enviorment.AppDomain();
        //读取热更资源的Dll
        TextAsset dllText = ResourceManager.Instance.LoadResource<TextAsset>(DLLPATH);
        //PBD文件，调试数据，日志报错
        using (MemoryStream ms = new MemoryStream(dllText.bytes))
        {
            m_AppDomain.LoadAssembly(ms, null, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
        }

        OnHotFixLoad();
    }

    /// <summary>
    /// 一些测试方法
    /// </summary>
    private void OnHotFixLoad()
    {

    }
}
