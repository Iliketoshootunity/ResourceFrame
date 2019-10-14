using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : Singleton<ObjectManager>
{
    protected Dictionary<Type, object> m_ClassObjectPoolDic = new Dictionary<Type, object>();
    public ClassObjectPool<T> GetOrCreateClassObjectPool<T>(int maxCount) where T : class, new()
    {
        Type t = typeof(T);
        if (!m_ClassObjectPoolDic.ContainsKey(t) || m_ClassObjectPoolDic[t] == null)
        {
            ClassObjectPool<T> pool = new ClassObjectPool<T>(maxCount);
            m_ClassObjectPoolDic.Add(t, pool);
        }
        return (ClassObjectPool<T>)m_ClassObjectPoolDic[t];
    }
}
