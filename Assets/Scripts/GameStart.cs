using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public AudioSource AS;
    protected AudioClip AC;

    public GameObject Go;

    private float time;
    // Start is called before the first frame update
    void Start()
    {
        Transform spawnTrs = transform.Find("SpawnTrs");
        Transform recycleTrs = transform.Find("RecycleTrs");
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(spawnTrs, recycleTrs);

        //string path = "Assets/GameData/Sounds/senlin.mp3";
        ////AC = ResourceManager.Instance.LoadResource<AudioClip>(path);
        ////AS.clip = AC;
        ////AS.Play();
        ////StartCoroutine(Test());
        ////time = Time.realtimeSinceStartup;
        //ResourceManager.Instance.PreloadResource(path);
        ////ResourceManager.Instance.AsyncLoadAsset(path, EAysncLoadPriority.Hight, TestCallBack);
        ////AC = ResourceManager.Instance.LoadResource<AudioClip>(path);
        //Vector2 s = new Vector2(67, -90);
        //s.Normalize();
        //Debug.Log(s);

        float Dot = Vector2.Dot((new Vector2(1, 1)).normalized, new Vector2(0, 1));
        Debug.Log("SSS" + Dot);
        Debug.Log("SSS" + Mathf.Sqrt(2) * Mathf.Cos(45));
        Vector3 p1 = new Vector3(0, 150, 0);
        Vector3 p2 = new Vector3(0, -210, 0);
        Quaternion q1 = Quaternion.Euler(p1);
        Quaternion q2 = Quaternion.Euler(p1);


    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            string path = "Assets/GameData/Prefab/Attack.prefab";
            //Go = ObjectManager.Instance.InstantiateGameObject(path, true);
            ObjectManager.Instance.AsyncInstantiateGameObject(path, EAysncLoadPriority.Hight, GameObjectCallBack);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ObjectManager.Instance.ReleseGameObject(Go);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            ObjectManager.Instance.ReleseGameObject(Go, 0, true);
        }
        if (Input.touches.Length > 0)
        {
            Touch temp = Input.GetTouch(0);
            Debug.Log(temp.fingerId);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            ResourceManager.Instance.ReleseResourceItem(AC, true);
            AS.Stop();
            AS.clip = null;
            AC = null;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            string path = "Assets/GameData/Sounds/senlin.mp3";
            time = Time.realtimeSinceStartup;
            //ResourceManager.Instance.AsyncLoadAsset(path, EAysncLoadPriority.Hight, TestCallBack);
            AC = ResourceManager.Instance.LoadResource<AudioClip>(path);
            Debug.Log(Time.realtimeSinceStartup - time);
        }
    }

    public void GameObjectCallBack(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        Go = (GameObject)obj;
    }


    public void TestCallBack(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        AS.clip = (AudioClip)obj;
        AS.Play();
        AC = AS.clip;
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
