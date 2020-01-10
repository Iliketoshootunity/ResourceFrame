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
        Debug.Log("总内存：" + PlatformManager.Instance.GetLongFromPlatform(1) / 1024.0f / 1024.0f);
        Debug.Log("剩余内存：" + PlatformManager.Instance.GetLongFromPlatform(2) / 1024.0f / 1024.0f);
        Debug.Log("使用内存：" + PlatformManager.Instance.GetLongFromPlatform(3) / 1024.0f / 1024.0f);
        DontDestroyOnLoad(this.gameObject);

        Transform spawnTrs = transform.Find("SpawnTrs");
        Transform recycleTrs = transform.Find("RecycleTrs");
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(spawnTrs, recycleTrs);
        GameMapManager.Instance.Init(this);

        UIManager.Instance.Init(UIRoot, MidleWindowRoot, UICamera, EventSystem);
        UIManager.Instance.RegisterWindow<LoadingWindow>(ConStr.LOADINGPANEL);
        UIManager.Instance.RegisterWindow<MenuWindow>(ConStr.MENUPANEL);
        //ObjectManager.Instance.InstantiateGameObject("Assets/GameData/Prefabs/Cube.prefab");
        ObjectManager.Instance.InstantiateGameObject("Assets/GameData/Prefabs/Attack.prefab");
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
}

public class AA : StateMachineBehaviour
{

}
