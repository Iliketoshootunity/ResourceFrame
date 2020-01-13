using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

public class ServerInfo  {

    //服务器的所有版本信息
    [XmlElement]
    public VersionInfo[] VersionInfo;
}

//当前版本对应的补丁
[System.Serializable]
public class VersionInfo
{
    //当前版本
    [XmlAttribute]
    public string Version;
    //所有的补丁包
    [XmlElement]
    public Patchs[] Patchs;
}

//一个总补丁包
[System.Serializable]
public class Patchs
{
    //版本 Hotcont
    [XmlAttribute]
    public int Version;

    //补丁描述信息
    [XmlAttribute]
    public string Des;

    //补丁文件
    [XmlElement]
    public List<Patch> Files;

}

//补丁包
[System.Serializable]
public class Patch
{
    [XmlAttribute]
    public string MD5;
    [XmlAttribute]
    public string Name;
    [XmlAttribute]
    public float Size;
    [XmlAttribute]
    public string Platform;
    [XmlAttribute]
    public string URL;
}
