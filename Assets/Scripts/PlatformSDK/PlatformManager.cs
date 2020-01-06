using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PlatformManager : Singleton<PlatformManager>
{
    private GameObject m_PlatformObj;
    private PlatformScript m_PlatformScript;
    void Init()
    {
        m_PlatformObj = new GameObject("PlatformObj");
        m_PlatformObj.hideFlags = HideFlags.HideAndDontSave;
        m_PlatformScript = m_PlatformObj.AddComponent<PlatformScript>();
        if (!Application.isPlaying) return;
        GameObject.DontDestroyOnLoad(m_PlatformObj);
#if UNITY_ANDROID && !UNITY_EDITOR
        GameHelperClass = new AndroidJavaClass("com.Custom.MyGame.GameHelper");
        Debug.Log("Init com.Custom.MyGame.GameHelperper");
#endif
    }



    //从底层获取Long数据
    public const int TOTAL_MEMORY = 1;                 //总内存 
    public const int REMAINING_MEMORY = 2;             //剩余内存
    public const int USEDD_MEMORY = 3;                 //使用的内存

    //发送消息到平台
    public const int PLATFORM_MSG_QQLOGIN = 1;//QQ登录
    public const int PLATFORM_MSG_QQLOGOUT = 2;//QQ注销
    public const int PLATFORM_MSG_WXLOGIN = 3;//WX登录
    public const int PLATFORM_MSG_WXLOGOUT = 4;//WX注销


#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaClass GameHelperClass;

    /// <summary>
    /// 发送消息给平台
    /// </summary>
    public void SendUnityMsgToPlatform(int iMsg, int iParam1 = 0, int iParam2 = 0, int iParam3 = 0, string strParam1 = "", string strParam2 = "", string strParam3 = "")
    {
        if (GameHelperClass == null) return;
        GameHelperClass.CallStatic("SendUnityMsgToPlatform", iMsg, iParam1, iParam2, iParam3, strParam1, strParam2, strParam3);
    }
    /// <summary>
    /// 从平台获取int数据
    /// </summary>
    /// <param name="type"></param>
    public int GetIntFromPlatform(int type)
    {
        if (GameHelperClass == null) return 0;
        return GameHelperClass.Call<int>("GetIntFromPlatform", type);
    }
    /// <summary>
    /// 从平台获取long数据
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public long GetLongFromPlatform(int type)
    {
        if (GameHelperClass == null) return 0;
        return GameHelperClass.Call<long>("GetLongFromPlatform", type);
    }
    /// <summary>
    /// 从平台获取long数据
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public long GetLongFromPlatform2(int type, int iParam1, int iParam2, int iParam3, string strParam1, string strParam2, string strParam3)
    {
        if (GameHelperClass == null) return 0;
        return GameHelperClass.Call<long>("GetLongFromPlatform2", type, iParam1, iParam2, iParam3, strParam1, strParam2, strParam3);
    }
    /// <summary>
    /// 从平台获取string数据
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public string GetStringFromPlatform(int type)
    {
        if (GameHelperClass == null) return "";
        return GameHelperClass.Call<string>("GetStringFromPlatform", type);
    }
#else




    /// <summary>
    /// 发送消息给平台
    /// </summary>
    public void SendUnityMsgToPlatform(int iMsg, int iParam1 = 0, int iParam2 = 0, int iParam3 = 0, string strParam1 = "", string strParam2 = "", string strParam3 = "")
    {

    }
    /// <summary>
    /// 从平台获取int数据
    /// </summary>
    /// <param name="type"></param>
    public int GetIntFromPlatform(int type)
    {
        return 0;
    }
    /// <summary>
    /// 从平台获取long数据
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public long GetLongFromPlatform(int type)
    {
        switch (type)
        {
            case TOTAL_MEMORY:
                return (long)GetTotalMemory();
            case REMAINING_MEMORY:
                return (long)GetRemaingMemory();
            case USEDD_MEMORY:
                return GetUsedMemory();
            default:
                return 0;
        }
    }
    /// <summary>
    /// 从平台获取long数据
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public long GetLongFromPlatform2(int type, int iParam1, int iParam2, int iParam3, string strParam1, string strParam2, string strParam3)
    {
        return 0;
    }
    /// <summary>
    /// 从平台获取string数据
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public string GetStringFromPlatform(int type)
    {
        return string.Empty;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        //系统内存总量
        public ulong ullTotalPhys;
        //系统可用内存
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    //extern  调用 其他平台 dll 关键字
    [DllImport("kernel32.dll")]
    protected static extern void GlobalMemoryStatus(ref MEMORYSTATUSEX lpBuff);

    /// <summary>
    /// 获取剩余内存
    /// </summary>
    /// <returns></returns>
    protected ulong GetRemaingMemory()
    {
        MEMORYSTATUSEX ms = new MEMORYSTATUSEX();
        ms.dwLength = 64;
        GlobalMemoryStatus(ref ms);
        return ms.ullAvailPhys;
    }
    /// <summary>
    /// 获取总内存
    /// </summary>
    /// <returns></returns>
    protected ulong GetTotalMemory()
    {
        MEMORYSTATUSEX ms = new MEMORYSTATUSEX();
        ms.dwLength = 64;
        GlobalMemoryStatus(ref ms);
        return ms.ullTotalPhys;
    }
    /// <summary>
    /// 获取使用的内存
    /// </summary>
    /// <returns></returns>
    protected long GetUsedMemory()
    {
        return UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();
    }
#endif
}
