using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public AudioSource AS;
    public AudioClip AC;

    // Start is called before the first frame update
    void Start()
    {
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        string path = "Assets/GameData/Sounds/senlin.mp3";
        AC = ResourceManager.Instance.LoadResource<AudioClip>(path);
        AS.clip = AC;
        AS.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            ResourceManager.Instance.ReleseResourceItem(AC);
            AS.clip = null;
            AC = null;
        }
    }
}
