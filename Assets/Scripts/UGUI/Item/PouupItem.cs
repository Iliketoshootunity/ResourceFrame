using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PouupItem : BaseItem
{
    public Text TitleText;
    public Text DescText;
    public Button OkButton;
    public Button CancelButton;
    public void Show(string title, string desc, Action downloadCallBack, Action cancelCallBack)
    {
        TitleText.text = title;
        DescText.text = desc;
        AddButtonClickListener(OkButton, () =>
        {
            if (downloadCallBack != null)
            {
                downloadCallBack();
            }
        });
        AddButtonClickListener(CancelButton, () =>
        {
            if (cancelCallBack != null)
            {
                cancelCallBack();
            }
        });
    }

    public void OnDestroy()
    {
        RemoveAllButtonListener();
    }
}
