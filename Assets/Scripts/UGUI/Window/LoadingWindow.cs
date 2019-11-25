using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingWindow : Window
{

    public LoadingPanel Panel;

    public override void OnAwake(params object[] paramList)
    {
        base.OnAwake(paramList);
        Panel = GameObjcet.GetComponent<LoadingPanel>();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        float value = GameMapManager.LoadingProgress / 100f;
        Panel.LoadingSlider.value = value;
        if (GameMapManager.LoadingProgress >= 100)
        {
            UIManager.Instance.CloseWindow(WndName, true);
        }
    }
}
