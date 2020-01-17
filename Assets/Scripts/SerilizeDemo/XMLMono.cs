using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class XMLMono : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //TestSerilize();
        //TestBianry();
        //TestSerilize tt = TestBianryDeserilize();
        //Debug.Log(tt.Id);
        //Debug.Log(tt.Name);
        //for (int i = 0; i < tt.List.Count; i++)
        //{
        //    Debug.Log(tt.List[i]);
        //}

        using (FileStream stream = new FileStream(Application.dataPath + "/ServerInfo.xml", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
        {
            using (StreamWriter sw = new StreamWriter(stream, System.Text.Encoding.UTF8))
            {
                ServerInfo Info = new ServerInfo();
                Info.VersionInfo = new VersionInfo[1];
                Info.VersionInfo[0] = new VersionInfo();
                Info.VersionInfo[0].Version = "0.1";
                XmlSerializer x = new XmlSerializer(Info.GetType());
                x.Serialize(sw, Info);
            }
        }


        //TestLoadAssetBundle();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TestLoadAssetBundle()
    {
        AssetBundle configAb = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + "data");
        TextAsset textAssed = configAb.LoadAsset<TextAsset>("AssetBundleConfig.bytes");
        MemoryStream ms = new MemoryStream(textAssed.bytes);
        BinaryFormatter bf = new BinaryFormatter();
        AssetBundleConfig test = (AssetBundleConfig)bf.Deserialize(ms);
        ms.Close();
        string path = "Assets/GameData/Prefab/Attack.prefab";
        uint crc = Crc32.GetCrc32(path);
        if (test != null)
        {
            BaseAB baseAB = null;
            for (int i = 0; i < test.ABList.Count; i++)
            {
                if (test.ABList[i].Crc == crc)
                {
                    baseAB = test.ABList[i];
                }
            }

            for (int i = 0; i < baseAB.Depends.Count; i++)
            {
                AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + baseAB.Depends[i]);
            }
            AssetBundle ab = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + baseAB.ABName);
            if (ab)
            {
                GameObject go = GameObject.Instantiate(ab.LoadAsset<GameObject>(baseAB.AssetName));
            }
        }

    }

    public void TestSerilize()
    {
        TestSerilize test = CreateXmlClass(); ;
        FileStream stream = new FileStream(Application.dataPath + "XMLTest.xml", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        StreamWriter sw = new StreamWriter(stream, System.Text.Encoding.UTF8);
        XmlSerializer x = new XmlSerializer(test.GetType());
        x.Serialize(sw, test);
        stream.Close();
    }


    public TestSerilize CreateXmlClass()
    {
        TestSerilize test = new TestSerilize();
        test.Id = 1;
        test.Name = "OOO";
        test.List = new List<int>();
        test.List.Add(1);
        test.List.Add(2);
        return test;
    }

    public TestSerilize TestDeserilize()
    {
        FileStream fs = new FileStream(Application.dataPath + "XMLTest.xml", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

        StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8);
        XmlSerializer xs = new XmlSerializer(typeof(TestSerilize));
        TestSerilize t = (TestSerilize)xs.Deserialize(sr);
        fs.Close();
        return t;
    }


    public void TestBianry()
    {
        TestSerilize test = new TestSerilize();
        test.Id = 100;
        test.Name = "OOO";
        test.List = new List<int>();
        test.List.Add(10);
        test.List.Add(21);
        FileStream stream = new FileStream(Application.dataPath + "BianryTest.bytes", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(stream, test);
        stream.Close();
    }

    public TestSerilize TestBianryDeserilize()
    {
        FileStream stream = new FileStream(Application.dataPath + "BianryTest.bytes", FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
        BinaryFormatter bf = new BinaryFormatter();
        TestSerilize test = (TestSerilize)bf.Deserialize(stream);
        stream.Close();
        return test;
    }

}
