using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ABConfig", menuName = "ABConfig", order = 500)]
public class ABConfig : ScriptableObject
{
    //每个Prefab的名字必须是唯一的
    public List<string> AllPrefabPath = new List<string>();

    public List<FileDirAB> AllFileDirAB = new List<FileDirAB>();

    [System.Serializable]
    public struct FileDirAB
    {
        public string ABName;
        public string Path;
    }
}
