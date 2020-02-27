using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public enum UIMsgID
{
    None = 0,
}

public class UIManager : Singleton<UIManager>
{
    private RectTransform m_UIRoot;
    private RectTransform m_WindowRoot;
    private Camera m_UICamera;
    private EventSystem m_EventSystem;
    private float m_CanvasRate = 0;
    private string m_UIPrefabPath = "Assets/GameData/Prefabs/UGUI/Panel/";

    private Dictionary<string, System.Type> m_RegisterDir = new Dictionary<string, System.Type>();
    private Dictionary<string, Window> m_WindowDic = new Dictionary<string, Window>();
    private List<Window> m_WindowList = new List<Window>();

    public RectTransform UIRoot
    {
        get
        {
            return m_UIRoot;
        }

    }



    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="uiRoot"></param>
    /// <param name="windowRoot"></param>
    /// <param name="uiCamera"></param>
    /// <param name="eventSystem"></param>
    public void Init(RectTransform uiRoot, RectTransform windowRoot, Camera uiCamera, EventSystem eventSystem)
    {
        m_UIRoot = uiRoot;
        m_WindowRoot = windowRoot;
        m_UICamera = uiCamera;
        m_EventSystem = eventSystem;
        m_CanvasRate = Screen.width / uiCamera.orthographicSize * 2;
    }

    /// <summary>
    /// 轮询
    /// </summary>
    public void OnUpdate()
    {
        for (int i = 0; i < m_WindowList.Count; i++)
        {
            if (m_WindowList[i] != null)
            {
                m_WindowList[i].OnUpdate();
            }

        }
    }

    /// <summary>
    /// 设置默认的选择对象
    /// </summary>
    public void SetNormalSelectObj(GameObject obj)
    {
        if (m_EventSystem == null)
        {
            m_EventSystem = EventSystem.current;
        }
        if (m_EventSystem != null)
        {
            m_EventSystem.firstSelectedGameObject = obj;
        }
    }

    /// <summary>
    /// 设置UI资源的加载路径
    /// </summary>
    /// <param name="path"></param>
    public void SetUIPrefabPath(string path)
    {
        m_UIPrefabPath = path;
    }
    /// <summary>
    /// 显示或者显示所有的UI
    /// </summary>
    /// <param name="active"></param>
    public void ShowOrHideAllUI(bool active)
    {
        if (UIRoot != null)
        {
            UIRoot.gameObject.SetActive(active);
        }
    }

    /// <summary>
    /// 注册UI界面
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="wndName"></param>
    public void RegisterWindow<T>(string wndName) where T : Window
    {
        m_RegisterDir[wndName] = typeof(T);
    }

    /// <summary>
    /// 发送消息给window
    /// </summary>
    /// <param name="wndName"></param>
    /// <param name="msgId"></param>
    /// <param name="paramList"></param>
    public void SendMessageToWnd(string wndName, UIMsgID msgId, params object[] paramList)
    {
        Window w = FindWindowByName(wndName);
        if (w != null)
        {
            w.OnMessage(msgId, paramList);
        }
    }

    /// <summary>
    /// 根据名字查找window
    /// </summary>
    /// <param name="wndName"></param>
    /// <returns></returns>
    public Window FindWindowByName(string wndName)
    {
        if (m_WindowDic.ContainsKey(wndName))
        {
            return m_WindowDic[wndName];
        }
        return null;
    }
    /// <summary>
    /// 创建窗口
    /// </summary>
    /// <param name="wndName"></param>
    /// <param name="bTop"></param>
    /// <param name="paramList"></param>
    public Window OpenWindow(string wndName, bool bTop = true, bool resource = false, params object[] paramList)
    {
        Window wnd = FindWindowByName(wndName);
        if (wnd == null)
        {
            System.Type windowType = m_RegisterDir[wndName];
            if (windowType == null)
            {
                Debug.LogError(wndName + " 没有注册");
                return null;
            }
            else
            {
                wnd = System.Activator.CreateInstance(windowType) as Window;
            }
            string path = m_UIPrefabPath + wndName;
            GameObject go = null;
            if (resource)
            {
                go = GameObject.Instantiate(Resources.Load<GameObject>(wndName.Replace(".prefab", "")));
            }
            else
            {
                go = ObjectManager.Instance.InstantiateGameObject(path);
            }
            if (go == null)
            {
                Debug.LogError(wndName + " 创建失败,找不到资源");
                return null;
            }

            m_WindowDic.Add(wndName, wnd);
            m_WindowList.Add(wnd);

            wnd.GameObjcet = go;
            wnd.Transform = go.GetComponent<RectTransform>();
            wnd.WndName = wndName;
            wnd.OnAwake(paramList);
            go.transform.SetParent(m_WindowRoot.transform);
            go.transform.localScale = Vector3.one;
            if (bTop)
            {
                go.transform.SetAsLastSibling();
            }
            go.GetComponent<UIOfflineData>().ResetProp();

            wnd.OnShow(paramList);
        }
        else
        {
            ShowWindow(wndName, bTop, paramList);
        }

        return wnd;
    }

    public void CloseWindow(string wndName, bool bDestory)
    {
        Window wnd = FindWindowByName(wndName);
        CloseWindow(wnd);
    }

    /// <summary>
    /// 关闭窗口
    /// </summary>
    public void CloseWindow(Window wnd, bool bDestory = false, bool resorce = false)
    {
        if (wnd == null) return;
        if (m_WindowList.Contains(wnd) && m_WindowDic.ContainsValue(wnd))
        {
            m_WindowList.Remove(wnd);
            m_WindowDic.Remove(wnd.WndName);

            wnd.OnDisable();
            wnd.OnClose();
            if (resorce)
            {
                GameObject.Destroy(wnd.GameObjcet);
            }
            if (bDestory)
            {
                ObjectManager.Instance.ReleseGameObject(wnd.GameObjcet, 0, true);
            }
            else
            {
                ObjectManager.Instance.ReleseGameObject(wnd.GameObjcet, isRecycleTrsChild: false);
            }

            wnd.GameObjcet = null;
            wnd = null;

        }
    }
    /// <summary>
    /// 关闭所有的窗口
    /// </summary>
    public void CloseAllWindow()
    {
        for (int i = m_WindowList.Count - 1; i >= 0; i--)
        {
            CloseWindow(m_WindowList[i]);
        }
    }
    /// <summary>
    /// 显示窗口
    /// </summary>
    /// <param name="wndName"></param>
    /// <param name="bTop"></param>
    /// <param name="paramList"></param>
    public void ShowWindow(string wndName, bool bTop, params object[] paramList)
    {
        Window wnd = null;
        if (m_WindowDic.TryGetValue(wndName, out wnd) && wnd != null)
        {
            if (m_WindowList.Contains(wnd))
            {
                ShowWindow(wnd, bTop, paramList);
            }
        }
    }
    /// <summary>
    /// 显示窗口
    /// </summary>
    /// <param name="window"></param>
    /// <param name="bTop"></param>
    /// <param name="paramLis"></param>
    public void ShowWindow(Window window, bool bTop, params object[] paramLis)
    {
        if (window != null)
        {
            if (bTop && window.GameObjcet)
            {
                window.GameObjcet.transform.SetAsLastSibling();
            }
            window.OnShow(paramLis);
        }
    }
    /// <summary>
    /// 隐藏窗口
    /// </summary>
    /// <param name="wndName"></param>
    public void HideWindow(string wndName)
    {
        Window wnd = null;
        if (m_WindowDic.TryGetValue(wndName, out wnd) && wnd != null)
        {
            if (m_WindowList.Contains(wnd))
            {
                HideWindow(wnd);
            }
        }
    }
    /// <summary>
    /// 隐藏窗口
    /// </summary>
    /// <param name="wnd"></param>
    public void HideWindow(Window wnd)
    {
        if (wnd != null)
        {
            wnd.GameObjcet.SetActive(false);
            wnd.OnDisable();
        }
    }

}
