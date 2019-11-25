using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
/// <summary>
/// 界面基类（界面逻辑）
/// </summary>
public class Window
{
    public GameObject GameObjcet { get; set; }
    public Transform Transform { get; set; }
    public string WndName { get; set; }

    protected List<Button> m_AllButton = new List<Button>();
    protected List<Toggle> m_AllToggle = new List<Toggle>();
    protected List<Object> m_AllLoadObj = new List<Object>();

    public virtual bool OnMessage(UIMsgID msgId, params object[] paramList)
    {
        return true;
    }

    public virtual void OnAwake(params object[] paramList) { }
    public virtual void OnShow(params object[] paramList) { }
    public virtual void OnUpdate() { }
    public virtual void OnDisable() { }
    public virtual void OnClose()
    {
        RemoveAllButtonListener();
        RemoveAllToggleListener();
        m_AllToggle.Clear();
        m_AllButton.Clear();
        for (int i = 0; i < m_AllLoadObj.Count; i++)
        {
            ResourceManager.Instance.ReleseResourceItem(m_AllLoadObj[i], true);
        }
    }

    public bool ChangeImageSprite(string path, Image image, bool setNativeSize = false)
    {
        if (image == null) return false;
        Sprite sp = ResourceManager.Instance.LoadResource<Sprite>(path);
        if (sp != null)
        {
            if (image.sprite != null) image.sprite = null;
            image.sprite = sp;
            if (setNativeSize) image.SetNativeSize();
            return true;
        }
        return false;
    }
    public void AddButtonClickListener(Button btn, UnityAction action)
    {
        if (btn == null) return;
        if (m_AllButton.Contains(btn))
        {
            btn.onClick.RemoveAllListeners();
        }
        else
        {
            m_AllButton.Add(btn);
        }
        btn.onClick.AddListener(action);
        btn.onClick.AddListener(PlaySoundOnBtnClicked);
    }
    public void AddToggleChangedListener(Toggle toggle, UnityAction<bool> action)
    {
        if (toggle == null) return;
        if (m_AllToggle.Contains(toggle))
        {
            toggle.onValueChanged.RemoveAllListeners();
        }
        else
        {
            m_AllToggle.Add(toggle);
        }
        toggle.onValueChanged.AddListener(action);
        toggle.onValueChanged.AddListener(PlaySoundOnToggleChanged);
    }

    public void RemoveAllButtonListener()
    {
        foreach (var item in m_AllButton)
        {
            item.onClick.RemoveAllListeners();
        }
    }
    public void RemoveAllToggleListener()
    {
        foreach (var item in m_AllToggle)
        {
            item.onValueChanged.RemoveAllListeners();
        }
    }

    protected virtual void PlaySoundOnBtnClicked()
    {

    }

    protected virtual void PlaySoundOnToggleChanged(bool value)
    {

    }

}
