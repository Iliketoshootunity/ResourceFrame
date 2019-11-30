using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuWindow : Window
{

    public MenuPanel MenuPanel;

    public override void OnAwake(params object[] paramList)
    {
        base.OnAwake(paramList);
        MenuPanel = GameObjcet.GetComponent<MenuPanel>();
        AsyncChangeImageSprite("Assets/GameData/Texture/Test1.png", MenuPanel.Sprite1);
    }
}
