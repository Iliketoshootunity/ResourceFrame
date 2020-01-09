using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class ABMD5  {

    [XmlElement]
    public List<ABMD5Base> ABMD5BaseList = new List<ABMD5Base>();

}

public class ABMD5Base
{
    [XmlAttribute]
    public string Name { get; set; }
    [XmlAttribute]
    public string Md5 { get; set; }
    [XmlAttribute]
    public long Size { get; set; }
}
