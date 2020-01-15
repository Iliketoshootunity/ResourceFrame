using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{

    private static T m_Instance;

    public static T Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindObjectOfType(typeof(T)) as T;
            }
            if (m_Instance == null)
            {
                GameObject go = new GameObject(typeof(T).Name);
                go.AddComponent<T>();
            }
            return m_Instance;
        }
    }

}
