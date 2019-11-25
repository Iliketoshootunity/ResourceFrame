using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMapManager : Singleton<GameMapManager>
{

    public Action LoadSceneStartCallBack;
    public Action LoadSceneFinishedCallBack;

    private string m_CurrentMapName;
    public string CurrentMapName { get { return m_CurrentMapName; } }
    private bool m_bAlreadyLoadScene;
    public bool bAlreadyLoadScene { get { return m_bAlreadyLoadScene; } }

    public static int LoadingProgress = 0;

    private MonoBehaviour m_Mono;

    public void Init(MonoBehaviour mono)
    {
        m_Mono = mono;
    }


    IEnumerator LoadSceneAsync(string name)
    {
        if (LoadSceneStartCallBack != null)
        {
            Load
        }
    }
}
