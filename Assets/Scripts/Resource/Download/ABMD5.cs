using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

/// <summary>
/// 初始打包的AsstBundle信息
/// </summary>
[System.Serializable]
public class ABMD5  {

    [XmlElement]
    public List<ABMD5Base> ABMD5BaseList = new List<ABMD5Base>();

}
[System.Serializable]
public class ABMD5Base
{
    [XmlAttribute]
    public string Name { get; set; }
    [XmlAttribute]
    public string Md5 { get; set; }
    [XmlAttribute]
    public float Size { get; set; }
}
