using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BaseItem : MonoBehaviour
{
    protected List<Button> m_AllButton = new List<Button>();
    protected List<Toggle> m_AllToggle = new List<Toggle>();
    protected List<Object> m_AllLoadObj = new List<Object>();
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
