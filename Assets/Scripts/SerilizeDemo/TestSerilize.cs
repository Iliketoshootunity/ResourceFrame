using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System;
[Serializable]
public class TestSerilize 
{
    [XmlAttribute("Id")]
    public int Id;
    [XmlAttribute("Name")]
    public string Name;
    [XmlElement("List")]
    public List<int> List;
}
