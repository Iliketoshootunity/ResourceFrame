using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class BundleEditor
{
    public static string ABCONFIGPATH = "Assets/Editor/ABConfig.asset";
    public static Dictionary<string, string> m_AllFileDir = new Dictionary<string, string>();

    [MenuItem("Tool/BuildAB")]
    public static void BuildAB()
    {
        m_AllFileDir.Clear();
        ABConfig config = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);
        if (config == null)
        {
            return;
        }
        foreach (var item in config.AllFileDirAB)
        {
            if (m_AllFileDir.ContainsKey(item.ABName))
            {
                Debug.Log("文件AB包名重复，请检查!!!!!!!!!!!!!!!!!!!!!!");
            }
            else
            {
                m_AllFileDir.Add(item.ABName, item.Path);
            }
        }
        //查找这个路径下的所有的prefab路径的GUID
        string[] guidArray = AssetDatabase.FindAssets("t:Prefab", config.AllPrefabPath.ToArray());

        for (int i = 0; i < guidArray.Length; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(guidArray[i]);
            EditorUtility.DisplayProgressBar("查找prefab", "Path:" + prefabPath, (i + 1) * 1 / guidArray.Length);
        }
        EditorUtility.ClearProgressBar();
    }

}
