using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameStart : MonoSingleton<GameStart>
{
    public bool LoadAssetFormEditor = true;
    private RectTransform UIRoot;
    private RectTransform MidleWindowRoot;
    private Camera UICamera;
    private EventSystem EventSystem;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        ObjectManager.Instance.Init(transform.Find("SpawnTrs"), transform.Find("RecycleTrs"));
        UIManager.Instance.Init(transform.Find("Canvas") as RectTransform, transform.Find("Canvas/Middle") as RectTransform, transform.Find("Canvas/UICamera").GetComponent<Camera>(), transform.Find("EventSystem").GetComponent<EventSystem>());
        ResourceManager.Instance.Init(this);
        ResourceManager.Instance.LoadAssetFormEditor = LoadAssetFormEditor;
        HotPatchManager.Instance.Init(this);
        RegisterWindow();

    }

    void Start()
    {
        UIManager.Instance.OpenWindow(ConStr.DOWNLOAD, true, true);
    }

    // Update is called once per frame
    void Update()
    {
        UIManager.Instance.OnUpdate();
    }



    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
        Resources.UnloadUnusedAssets();
#endif
    }

    public IEnumerator StartGame(Slider slider, Text text)
    {
        slider.value = 0;
        yield return null;
        text.text = "加载本地数据";
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        slider.value = 0.1f;
        yield return null;
        text.text = "加载dll";
        ILRuntimeManager.Instance.Init();
        slider.value = 0.2f;
        yield return null;
        text.text = "加载数据表";
        //
        slider.value = 0.5f;
        yield return null;
        text.text = "加载配置";
        //
        slider.value = 0.9f;
        yield return null;
        text.text = "初始化地图";
        GameMapManager.Instance.Init(this);
        slider.value = 1f;
        yield return null;
        GameMapManager.Instance.LoadScene(ConStr.MENUSCENE);
    }

    private void RegisterWindow()
    {
        UIManager.Instance.RegisterWindow<DownloadWindow>(ConStr.DOWNLOAD);
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

    public static void TestUnPacked()
    {
        bool isPacked = HotPatchManager.Instance.ComputeUnPackedFile();
        if (isPacked)
        {

        }
    }

    private void Test(bool o)
    {
        StartCoroutine(HotPatchManager.Instance.StartDownload(() =>
        {

        }));
    }
}

public class AA : StateMachineBehaviour
{

}
