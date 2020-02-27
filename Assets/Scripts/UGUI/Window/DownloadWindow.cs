using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownloadWindow : Window
{
    private DownloadPanel m_Panel;
    private float m_Sumtime;

    public override void OnAwake(params object[] paramList)
    {
        base.OnAwake();
        m_Panel = GameObjcet.GetComponent<DownloadPanel>();
        m_Panel.gameObject.SetActive(true);
        m_Panel.ProgeressSlider.value = 0;
        m_Panel.ProgeressDesText.text = "下载中...";
        m_Panel.ProgeressValueText.text = string.Format("{0:F}M/s", 0);
        HotPatchManager.Instance.ReadServerInfoError += ServerInfoError;
        HotPatchManager.Instance.DownloadItemError += DownloadItemError;
        m_Sumtime = 0;
#if UNITY_EDITOR
        DownloadFinished();
#else
         if (HotPatchManager.Instance.ComputeUnPackedFile())
        {
            m_Panel.ProgeressDesText.text = "解包中...";
            HotPatchManager.Instance.StartUnPacked(() =>
            {
                m_Sumtime = 0;
                HotPatch();
            });
        }
        else
        {
            HotPatch();
        }
    
#endif
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if (HotPatchManager.Instance.GetStartUnPacked)
        {
            m_Sumtime += Time.deltaTime;
            m_Panel.ProgeressSlider.value = HotPatchManager.Instance.GetUnPackedProgress();
            float speed = (HotPatchManager.Instance.AlreadyUnPackedSize / 1024.0f) / m_Sumtime;
            m_Panel.ProgeressValueText.text = string.Format("{0:F} M/S", speed);
        }

        if (HotPatchManager.Instance.GetStartUnPacked)
        {
            m_Sumtime += Time.deltaTime;
            m_Panel.ProgeressSlider.value = HotPatchManager.Instance.GetAlreadyDownloadProgress();
            float speed = (HotPatchManager.Instance.GetAlreadyDownloadSize() / 1024.0f) / m_Sumtime;
            m_Panel.ProgeressValueText.text = string.Format("{0:F} M/S", speed);
        }
    }

    public override void OnClose()
    {
        HotPatchManager.Instance.ReadServerInfoError -= ServerInfoError;
        HotPatchManager.Instance.DownloadItemError -= DownloadItemError;
    }

    private void HotPatch()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            GameStart.OpenPopupWindow("网络连接错误", "网络连接失败,请检查网络是否正常", () => { Application.Quit(); }, () => { Application.Quit(); });
        }
        else
        {
            CheckVersion();
        }
    }

    private void CheckVersion()
    {
        HotPatchManager.Instance.CheckVersion((hot) =>
        {
            if (hot)
            {
                GameStart.OpenPopupWindow("热更", string.Format("当前版本未{0}，有{1:F}M大小热更包，确定是否下载", HotPatchManager.Instance.CurVersion, HotPatchManager.Instance.DownloadSize),
                    OnClickStartDownload, OnClickCancleDownload);
            }
            else
            {
                DownloadFinished();
            }

        });
    }

    private void OnClickStartDownload()
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
            if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork)
            {
                GameStart.OpenPopupWindow("下载确认", "当前使用的时手机流量，是否继续下载",
                StartDownload, OnClickCancleDownload);
            }
        }
        else
        {
            StartDownload();
        }
    }

    private void StartDownload()
    {
        m_Panel.gameObject.SetActive(true);
        m_Panel.ProgeressSlider.value = 0;
        m_Panel.ProgeressDesText.text = "下载中...";
        m_Panel.ProgeressValueText.text = string.Format("{0:F}M/s", 0);
        GameStart.Instance.StartCoroutine(HotPatchManager.Instance.StartDownload(DownloadFinished));
    }

    private void DownloadFinished()
    {
        GameStart.Instance.StartCoroutine(DownloadFinishedIE());
    }

    private IEnumerator DownloadFinishedIE()
    {
        yield return GameStart.Instance.StartCoroutine(GameStart.Instance.StartGame(m_Panel.ProgeressSlider, m_Panel.ProgeressDesText));
        UIManager.Instance.CloseWindow(this, true, true);
    }

    private void OnClickCancleDownload()
    {
        Application.Quit();
    }
    public void ServerInfoError()
    {
        GameStart.OpenPopupWindow("服务器列表获取失败", "服务器列表获取失败，请检查网络链接是否正常？尝试重新下载！", CheckVersion, () => { Application.Quit(); });
    }
    public void DownloadItemError(string erroInfo)
    {
        GameStart.OpenPopupWindow("资源下载失败", string.Format("{0}等资源下载失败，请重新尝试下载！", erroInfo), () => { Application.Quit(); }, () => { Application.Quit(); });
    }

}
