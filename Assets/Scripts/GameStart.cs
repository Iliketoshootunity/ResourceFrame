using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
public class GameStart : MonoBehaviour
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
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(spawnTrs, recycleTrs);
        GameMapManager.Instance.Init(this);
        UIManager.Instance.Init(UIRoot, MidleWindowRoot, UICamera, EventSystem);
        //GameMapManager.Instance.LoadScene(ConStr.MENUSCENE);
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

    private void RegisterWindow()
    {
        UIManager.Instance.RegisterWindow<LoadingWindow>(ConStr.LOADINGPANEL);
        UIManager.Instance.RegisterWindow<MenuWindow>(ConStr.MENUPANEL);
    }
}

public class AA : StateMachineBehaviour
{

}
