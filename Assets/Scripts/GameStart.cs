using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public AudioSource AS;
    public AudioClip AC;

    public GameObject Go;

    
    // Start is called before the first frame update
    void Start()
    {
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
        string path = "Assets/GameData/Sounds/senlin.mp3";
        //AC = ResourceManager.Instance.LoadResource<AudioClip>(path);
        //AS.clip = AC;
        //AS.Play();
        //StartCoroutine(Test());

        ResourceManager.Instance.AsyncLoadAsset(path, EAysncLoadPriority.Hight, TestCallBack);

        Vector2 s = new Vector2(67, -90);
        s.Normalize();
        Debug.Log(s);
    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ResourceManager.Instance.ReleseResourceItem(AC, true);
            AS.clip = null;
            AC = null;
        }
    }


    public void TestCallBack(string path, Object obj, object param1 = null, object param2 = null, object param3 = null)
    {
        AS.clip = (AudioClip)obj;
        AC = AS.clip;
    }


}

public class AA : StateMachineBehaviour
{

}
