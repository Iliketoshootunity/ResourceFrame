﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ILRuntime.Runtime.Enviorment;
using System.IO;
using ILRuntime.Runtime;

public class ILRuntimeManager : Singleton<ILRuntimeManager>
{
    private AppDomain m_AppDomain;
    private const string DLLPATH = "Assets/GameData/Code/HotFix.dll.txt";
    private const string PDBPATH = "Assets/GameData/Code/HotFix.pdb.txt";
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
        TextAsset pbdText = ResourceManager.Instance.LoadResource<TextAsset>(PDBPATH);
        //PBD文件，调试数据，日志报错
        using (MemoryStream ms = new MemoryStream(dllText.bytes))
        {
            using (MemoryStream ms2 = new MemoryStream(pbdText.bytes))
            {
                m_AppDomain.LoadAssembly(ms, ms2, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
            }
        }

        OnHotFixLoad();
    }

    /// <summary>
    /// 一些测试方法
    /// </summary>
    private void OnHotFixLoad()
    {
        //第一种简单方法的调用
        m_AppDomain.Invoke("HotFix.TestClass", "TestClassStaticFunction", null, null);

        //先单独获取类
    }
}
