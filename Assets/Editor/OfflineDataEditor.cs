using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class OfflineDataEditor
{
    [MenuItem("Assets/生成离线数据")]
    public static void AssetCreateOfflineData()
    {
        GameObject[] objects = Selection.gameObjects;
        for (int i = 0; i < objects.Length; i++)
        {
            EditorUtility.DisplayProgressBar("添加离线数据", "正在修改：" + objects[i] + "............", 1.0f / objects.Length * i);
            CreateOfflineData(objects[i]);
        }
        EditorUtility.ClearProgressBar();
    }

    public static void CreateOfflineData(GameObject obj)
    {
        OfflineData data = obj.GetComponent<OfflineData>();
        if (data == null)
        {
            data = obj.AddComponent<OfflineData>();
        }
        data.BindData();
        EditorUtility.SetDirty(obj);
        Debug.Log("修改了" + obj.gameObject.name + "离线数据");
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }
    [MenuItem("Assets/生成特效 离线数据")]
    public static void AssetCreateEffectData()
    {
        GameObject[] objects = Selection.gameObjects;
        for (int i = 0; i < objects.Length; i++)
        {
            EditorUtility.DisplayProgressBar("添加离线离线数据", "正在修改：" + objects[i] + "............", i + 1 / (float)objects.Length);
            CreateEffectOfflineData(objects[i]);
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Assets/生成所有的特效 离线数据")]
    public static void AllCreateEffectData()
    {
        string[] allStr = AssetDatabase.FindAssets("t;Prefab", new string[] { "Assets/GameData/Prefab/Effect" });
        for (int i = 0; i < allStr.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(allStr[i]);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if(go)
            {
                EditorUtility.DisplayProgressBar("添加离线离线数据", "正在修改：" + assetPath + "............", i + 1 / (float)allStr.Length);
                CreateEffectOfflineData(go);
            }

        }
        EditorUtility.ClearProgressBar();
    }

    public static void CreateEffectOfflineData(GameObject obj)
    {
        EffectOfflineData data = obj.GetComponent<EffectOfflineData>();
        if (data == null)
        {
            data = obj.AddComponent<EffectOfflineData>();
        }
        data.BindData();
        EditorUtility.SetDirty(obj);
        Debug.Log("修改了" + obj.gameObject.name + "离线数据");
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/生成UI 离线数据")]
    public static void AssetCreateUIData()
    {
        GameObject[] objects = Selection.gameObjects;
        for (int i = 0; i < objects.Length; i++)
        {
            EditorUtility.DisplayProgressBar("添加离线离线数据", "正在修改：" + objects[i] + "............", i + 1 / (float)objects.Length);
            CreateUIOfflineData(objects[i]);
        }
        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Assets/生成所有的UI 离线数据")]
    public static void AllCreateUIData()
    {
        string[] allStr = AssetDatabase.FindAssets("t;Prefab", new string[] { "Assets/GameData/Prefab/UI" });
        for (int i = 0; i < allStr.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(allStr[i]);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (go)
            {
                EditorUtility.DisplayProgressBar("添加离线离线数据", "正在修改：" + assetPath + "............", i + 1 / (float)allStr.Length);
                CreateUIOfflineData(go);
            }

        }
        EditorUtility.ClearProgressBar();
    }

    public static void CreateUIOfflineData(GameObject obj)
    {
        UIOfflineData data = obj.GetComponent<UIOfflineData>();
        if (data == null)
        {
            data = obj.AddComponent<UIOfflineData>();
        }
        data.BindData();
        EditorUtility.SetDirty(obj);
        Debug.Log("修改了" + obj.gameObject.name + "离线数据");
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();
    }
}
