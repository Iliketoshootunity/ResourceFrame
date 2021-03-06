﻿using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlatType
{
    None,
    QQ,
    WX
}
/// <summary>
/// 手机管理
/// </summary>
public class PhoneManager : Singleton<PhoneManager>
{
    public bool IsLogin { get; set; }

    public Action<PlatType> LoginSuccess;
    public Action<PlatType> LoginFailed;
    public Action<PlatType> LoginCancle;

    private PlatType CurPlatType;

    private const int UNITY_GET_STRING_QQAUTORVAILD = 1;       ////qq是否过期
    private const int UNITY_GET_STRING_QQREFRESHSESSION = 2;  // //qq 刷新并获取票据
    private const int UNITY_GET_STRING_WXAUTORVAILD = 3;       ////WX是否过期
    private const int UNITY_GET_STRING_WXREFRESHSESSION = 4;  // //WX 刷新并获取票据

    /// <summary>
    /// 登陆
    /// </summary>
    /// <param name="plat"></param>
    public void Login(PlatType plat)
    {
        if (plat == PlatType.QQ)
        {
            PlatformManager.Instance.SendUnityMsgToPlatform(PlatformManager.PLATFORM_MSG_QQLOGIN);
        }
        else if (plat == PlatType.WX)
        {
            PlatformManager.Instance.SendUnityMsgToPlatform(PlatformManager.PLATFORM_MSG_WXLOGIN);
        }
    }
    /// <summary>
    /// 登出
    /// </summary>
    public void LogOut()
    {
        if (CurPlatType == PlatType.QQ)
        {
            PlatformManager.Instance.SendUnityMsgToPlatform(PlatformManager.PLATFORM_MSG_QQLOGOUT);
        }
        else if (CurPlatType == PlatType.WX)
        {
            PlatformManager.Instance.SendUnityMsgToPlatform(PlatformManager.PLATFORM_MSG_WXLOGOUT);
        }
    }

    /// <summary>
    /// qq的票据是否有效
    /// </summary>
    /// <returns></returns>
    public bool QQAnthorVaild()
    {
        string str = PlatformManager.Instance.GetStringFromPlatform(UNITY_GET_STRING_QQAUTORVAILD);
        return Convert.ToBoolean(str);
    }

    /// <summary>
    /// 刷新获取qq票据
    /// </summary>
    /// <returns></returns>
    public JsonData GetQQSession()
    {
        string str = PlatformManager.Instance.GetStringFromPlatform(UNITY_GET_STRING_WXAUTORVAILD);
        JsonData data = JsonMapper.ToObject(str);
        return data;
    }


    /// <summary>
    /// qq的票据是否有效
    /// </summary>
    /// <returns></returns>
    public bool WXAnthorVaild()
    {
        string str = PlatformManager.Instance.GetStringFromPlatform(UNITY_GET_STRING_QQAUTORVAILD);
        return Convert.ToBoolean(str);
    }

    /// <summary>
    /// 刷新获取qq票据
    /// </summary>
    /// <returns></returns>
    public JsonData GetWXSession()
    {
        string str = PlatformManager.Instance.GetStringFromPlatform(UNITY_GET_STRING_WXREFRESHSESSION);
        JsonData data = JsonMapper.ToObject(str);
        return data;
    }

    /// <summary>
    /// 自动登陆
    /// </summary>
    public void AutoLogin()
    {
#if !UNITY_EDITOR
        if (PlayerPrefs.HasKey("LoginPlat"))
        {
            PlatType plat = (PlatType)Enum.Parse(typeof(PlatType), PlayerPrefs.GetString("LoginPlat"));
            if (plat == PlatType.QQ)
            {
                if (QQAnthorVaild())
                {
                    JsonData data = GetQQSession();
                    LoginCallBack(data, PlatType.QQ);
                }
            }
            else if (plat == PlatType.WX)
            {
                if (WXAnthorVaild())
                {
                    JsonData data = GetWXSession();
                    LoginCallBack(data, PlatType.WX);
                }
            }

        }
        else
        {
            if (QQAnthorVaild())
            {
                JsonData data = GetQQSession();
                LoginCallBack(data, PlatType.QQ);
            }
            else if(WXAnthorVaild())
            {
                JsonData data = GetWXSession();
                LoginCallBack(data, PlatType.WX);
            }
        }
#endif

    }

    /// <summary>
    /// 登陆结果回调
    /// </summary>
    /// <param name="data"></param>
    /// <param name="platType"></param>
    /// <param name="result"> 0 ：登陆成功 1：登陆失败 2：登陆取消</param>
    public void LoginCallBack(JsonData data, PlatType platType, int result = 0)
    {
        switch (result)
        {
            case 0:
                Debug.Log("获取票据： " + data.ToJson().ToString());
                string openid = (string)data["openid"];
                string opunionidenid = (string)data["unionid"];
                string token = (string)data["token"];
                IsLogin = true;
                CurPlatType = platType;
                PlayerPrefs.SetString("LoginPlat", CurPlatType.ToString());
                PlayerPrefs.Save();
                if (LoginSuccess != null)
                {
                    LoginSuccess(platType);
                }
                break;
            case 1:
                if (LoginFailed != null)
                {
                    LoginFailed(platType);
                }
                break;
            case 2:
                if (LoginCancle != null)
                {
                    LoginCancle(platType);
                }
                break;
        }

    }
}
