using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    /// <summary>
    /// 设置场景内容
    /// </summary>
    /// <param name="name"></param>
    void SetSceneSetting(string name)
    {
        //根据配置表来
    }

    IEnumerator LoadSceneAsync(string name)
    {
        if (LoadSceneStartCallBack != null)
        {
            LoadSceneStartCallBack();
        }
        ClearCache();
        m_bAlreadyLoadScene = false;
        AsyncOperation ao = SceneManager.LoadSceneAsync(ConStr.EMPTYSCENE,LoadSceneMode.Single);
        while (ao != null && !ao.isDone)
        {
            yield return new WaitForEndOfFrame();
        }
        LoadingProgress = 0;
        int targetProgress = 0;
        AsyncOperation asyncScene = SceneManager.LoadSceneAsync(name);
        if (asyncScene != null && !asyncScene.isDone)
        {
            asyncScene.allowSceneActivation = false;
            while (asyncScene.progress < 0.9f)
            {
                targetProgress = (int)asyncScene.progress * 100;
                yield return new WaitForEndOfFrame();
                //平滑过渡
                while (LoadingProgress < targetProgress)
                {
                    ++LoadingProgress;
                    yield return new WaitForEndOfFrame();
                }
            }

            m_CurrentMapName = name;
            SetSceneSetting(name);
            //自行加载剩余的10%
            targetProgress = 100;
            while (LoadingProgress < targetProgress - 2)
            {
                ++LoadingProgress;
                yield return new WaitForEndOfFrame();
            }
            LoadingProgress = 100;
            asyncScene.allowSceneActivation = true;
            m_bAlreadyLoadScene = true;
            if (LoadSceneFinishedCallBack != null)
            {
                LoadSceneFinishedCallBack();
            }
        }

    }
    /// <summary>
    /// 跳场景需要清除的东西
    /// </summary>
    public void ClearCache()
    {
        ObjectManager.Instance.ClearPool();
        ResourceManager.Instance.ClearCache();
    }
}
