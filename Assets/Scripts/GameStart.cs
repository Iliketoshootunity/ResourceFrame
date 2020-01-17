using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
public class GameStart : MonoSingleton<GameStart>
{
    public RectTransform UIRoot;
    public RectTransform MidleWindowRoot;
    public Camera UICamera;
    public EventSystem EventSystem;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        Transform spawnTrs = transform.Find("SpawnTrs");
        Transform recycleTrs = transform.Find("RecycleTrs");
        HotPatchManager.Instance.Init(this);
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(spawnTrs, recycleTrs);
        GameMapManager.Instance.Init(this);
        UIManager.Instance.Init(UIRoot, MidleWindowRoot, UICamera, EventSystem);
        HotPatchManager.Instance.CheckVersion(Test);
        //GameMapManager.Instance.LoadScene(ConStr.MENUSCENE);
    }

    // Update is called once per frame
    void Update()
    {
        UIManager.Instance.OnUpdate();
    }
    private void Test(bool o)
    {
        StartCoroutine(HotPatchManager.Instance.StartDownload(()=> { 

        }));
    }


    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        Resources.UnloadUnusedAssets();
#endif
    }

    public IEnumerator StartGame()
    {
        Debug.Log("加载资源信息");
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        yield return null;
    }

    private void RegisterWindow()
    {
        UIManager.Instance.RegisterWindow<LoadingWindow>(ConStr.LOADINGPANEL);
        UIManager.Instance.RegisterWindow<MenuWindow>(ConStr.MENUPANEL);
    }

    public static void OpenPopupWindow(string title, string desc, Action downloadCallBack, Action cancelCallBack)
    {
        GameObject go = GameObject.Instantiate(Resources.Load<GameObject>("PouupItem"));
        if (go != null)
        {
            PouupItem item = go.GetComponent<PouupItem>();
            go.transform.SetParent(UIManager.Instance.UIRoot.transform, false);
            item.Show(title, desc, downloadCallBack, cancelCallBack);
        }
    }
}

public class AA : StateMachineBehaviour
{

}
