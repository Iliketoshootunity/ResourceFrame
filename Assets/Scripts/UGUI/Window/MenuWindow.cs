using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuWindow : Window
{

    public MenuPanel MenuPanel;

    public override void OnAwake(params object[] paramList)
    {
        base.OnAwake(paramList);
        MenuPanel = GameObjcet.GetComponent<MenuPanel>();
        AsyncChangeImageSprite("Assets/GameData/UGUI/Test1.png", MenuPanel.Sprite1);

        PhoneManager.Instance.LoginSuccess += LoginSuccessCallBack;
        PhoneManager.Instance.LoginFailed += LoginFailedCallBack;
        PhoneManager.Instance.LoginCancle += LoginCancelCallBack;

        AddButtonClickListener(MenuPanel.QQLoginBtn, QQLogin);
        AddButtonClickListener(MenuPanel.WXLoginBtn, WXLogin);
        AddButtonClickListener(MenuPanel.LogOutBtn, Logout);
    }

    public void QQLogin()
    {
        PhoneManager.Instance.Login(PlatType.QQ);
    }

    public void WXLogin()
    {
        PhoneManager.Instance.Login(PlatType.WX);
    }

    public void Logout()
    {
        PhoneManager.Instance.LogOut();
        MenuPanel.LoginObj.SetActive(true);
        MenuPanel.LogOutBtn.gameObject.SetActive(false);
    }

    public void LoginSuccessCallBack(PlatType platType)
    {
        Debug.Log("登陆成功");
        MenuPanel.LoginObj.SetActive(false);
        MenuPanel.LogOutBtn.gameObject.SetActive(true);
    }
    public void LoginFailedCallBack(PlatType platType)
    {
        Debug.Log("登陆失败");
    }
    public void LoginCancelCallBack(PlatType platType)
    {
        Debug.Log("登陆取消");
    }
}
