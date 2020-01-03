using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
#if UNITY_ANDROID || UNITY_IOS
public class PlatformScript : MonoBehaviour
{

    public struct PlatformMsg
    {
        public int iMsgId;
        public int iParam1;
        public int iParam2;
        public int iParam3;
        public string strParam1;
        public string strParam2;
        public string strParam3;
    }

    private Queue<PlatformMsg> PlatformMsgList = new Queue<PlatformMsg>();

    /// <summary>
    /// 接收来自平台的消息
    /// </summary>
    /// <param name="msg"></param>
    public void OnMessage(string msg)
    {
        JsonData Jd = JsonMapper.ToObject(msg);
        PlatformMsg Msg = new PlatformMsg();
        Msg.iMsgId = (int)Jd["iMsgId"];
        Msg.iMsgId = (int)Jd["iParam1"];
        Msg.iMsgId = (int)Jd["iParam2"];
        Msg.iMsgId = (int)Jd["iParam3"];
        Msg.strParam1 = (string)Jd["strParam1"];
        Msg.strParam2 = (string)Jd["strParam2"];
        Msg.strParam3 = (string)Jd["strParam3"];

        PlatformMsgList.Enqueue(Msg);
    }

    private void Update()
    {
        while (PlatformMsgList.Count > 0)
        {
            PlatformMsg Msg = PlatformMsgList.Dequeue();
            //处理消息
            switch (Msg.iMsgId)
            {
                default:
                    break;
            }
        }
    }
}
#endif
