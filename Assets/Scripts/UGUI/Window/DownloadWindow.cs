using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownloadWindow : Window
{
    private DownloadPanel m_Panel;

    public override void OnAwake(params object[] paramList)
    {
        m_Panel = GameObjcet.GetComponent<DownloadPanel>();
        m_Panel.gameObject.SetActive(false);
        HotPatchManager.Instance.ReadServerInfoError += ServerInfoError;
        HotPatchManager.Instance.DownloadItemError += DownloadItemError;

    }

    public override void OnClose()
    {
        HotPatchManager.Instance.ReadServerInfoError -= ServerInfoError;
        HotPatchManager.Instance.DownloadItemError -= DownloadItemError;
    }

    public void ServerInfoError()
    {

    }
    public void DownloadItemError(string erroInfo)
    {

    }

}
