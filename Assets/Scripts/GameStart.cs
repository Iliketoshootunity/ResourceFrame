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



        //AssetBundleManager.Instance.LoadAssetBundleConfig();
        //ResourceManager.Instance.Init(this);
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


    }



    // Update is called once per frame
    void Update()
    {
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
