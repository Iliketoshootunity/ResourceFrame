using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassObjectPool<T> where T : class, new()
{
    //池
    protected Stack<T> m_Pool = new Stack<T>();
    //最大限制数量，如果为<=0,则表示可以无限存放
    protected int m_MaxCount;
    //当前未回收的数量
    protected int m_NoRecycleCount;

    public ClassObjectPool(int maxCount)
    {
        m_MaxCount = maxCount;
        for (int i = 0; i < m_MaxCount; i++)
        {
            T t = new T();
            m_Pool.Push(t);
        }
    }

    public T Spawn(bool createIfPoolEmpty)
    {
        T ret = null;
        if (m_Pool.Count > 0)
        {
            ret = m_Pool.Pop();
            if (ret == null)
            {
                if (createIfPoolEmpty)
                {
                    ret = new T();
                }
            }
            if (ret != null)
            {
                m_NoRecycleCount++;
            }
        }
        else
        {
            if (createIfPoolEmpty)
            {
                ret = new T();
                m_NoRecycleCount++;
            }
        }
        return ret;
    }

    public bool Recycle(T obj)
    {
        if (obj == null) return false;
        m_NoRecycleCount--;
        if (m_Pool.Count > m_MaxCount && m_MaxCount > 0)
        {
            obj = null;
            return false;
        }
        m_Pool.Push(obj);
        return true;
    }
}
