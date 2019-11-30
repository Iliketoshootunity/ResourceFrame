using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMode : MonoBehaviour {

	// Use this for initialization
	void Start () {

        UIManager.Instance.OpenWindow("MenuPanel.prefab");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
