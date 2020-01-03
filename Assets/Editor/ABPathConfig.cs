using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ABPathConfig", menuName = "ABPathConfig", order = 500)]
public class ABPathConfig : ScriptableObject
{
    //基于prefab打包路径 每个Prefab的名字必须是唯一的
    [Header("预制体资源路径")]
    public List<string> PrefabAssetDirectorys = new List<string>();
    //基于文件打包路径
    [Header("其他资源路径")]
    public List<OtherDirectoryConfig> OtherAssetDirectorys = new List<OtherDirectoryConfig>();

    [System.Serializable]
    public struct OtherDirectoryConfig
    {
        public string ABName;
        public string Path;
    }
}
