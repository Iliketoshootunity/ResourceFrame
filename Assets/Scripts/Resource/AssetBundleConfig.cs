using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System;
[Serializable]
public class AssetBundleConfig
{
    [XmlElement("ABList")]
    public List<BaseAB> ABList;
}

[Serializable]
public class BaseAB
{
    [XmlAttribute("AssetName")]
    public string AssetName;
    [XmlAttribute("ABName")]
    public string ABName;
    [XmlAttribute("Crc")]
    public uint Crc;
    [XmlAttribute("path")]
    public string Path;
    [XmlElement("Depends")]
    public List<string> Depends;
}
