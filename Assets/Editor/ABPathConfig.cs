using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ABPathConfig", menuName = "ABPathConfig", order = 500)]
public class ABPathConfig : ScriptableObject
{
    //基于prefab打包路径 每个Prefab的名字必须是唯一的
    [Header("基于prefab打包路径")]
    public List<string> PrefabDirPathArr = new List<string>();
    //基于文件打包路径
    [Header("基于文件打包路径")]
    public List<DirAB> DirPathArr = new List<DirAB>();

    [System.Serializable]
    public struct DirAB
    {
        public string ABName;
        public string Path;
    }
}
