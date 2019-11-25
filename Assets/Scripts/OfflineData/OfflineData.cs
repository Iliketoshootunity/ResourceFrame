using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 离线数据
/// </summary>
public class OfflineData : MonoBehaviour
{
    public Rigidbody Rigi;
    public Collider Collider;
    public Transform[] AllPoint;
    public int[] AllPointChildCount;
    public bool[] AllPointActive;
    public Vector3[] AllPos;
    public Vector3[] AllScale;
    public Quaternion[] AllRot;
    /// <summary>
    /// 还原属性
    /// </summary>
    public virtual void ResetProp()
    {
        int allPointCount = AllPoint.Length;
        for (int i = 0; i < allPointCount; i++)
        {
            Transform temp = AllPoint[i];
            if (temp != null)
            {
                temp.localPosition = AllPos[i];
                temp.localScale = AllScale[i];
                temp.localRotation = AllRot[i];
                temp.gameObject.SetActive(AllPointActive[i]);


                if (AllPointActive[i])
                {
                    if (!temp.gameObject.activeSelf)
                    {
                        temp.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (temp.gameObject.activeSelf)
                    {
                        temp.gameObject.SetActive(false);
                    }
                }

                int childCount = temp.childCount;
                if (childCount > AllPointChildCount[i])
                {
                    for (int j = AllPointChildCount[i]; j < childCount; j++)
                    {
                        GameObject go = temp.GetChild(j).gameObject;
                        if (!ObjectManager.Instance.IsCreateByObjectManager(go))
                        {
                            Destroy(go);
                        }
                    }

                }
            }
        }
    }
    /// <summary>
    /// 编辑器下保存初始数据
    /// </summary>
    public virtual void BindData()
    {
        Rigi = GetComponentInChildren<Rigidbody>(true);
        Collider = GetComponentInChildren<Collider>(true);
        AllPoint = GetComponentsInChildren<Transform>(true);
        AllPointChildCount = new int[AllPoint.Length];
        AllPointActive = new bool[AllPoint.Length];
        AllPos = new Vector3[AllPoint.Length];
        AllScale = new Vector3[AllPoint.Length];
        AllRot = new Quaternion[AllPoint.Length];
        for (int i = 0; i < AllPoint.Length; i++)
        {
            AllPointChildCount[i] = AllPoint[i].childCount;
            AllPointActive[i] = AllPoint[i].gameObject.activeSelf;
            AllPos[i] = AllPoint[i].localPosition;
            AllScale[i] = AllPoint[i].localScale;
            AllRot[i] = AllPoint[i].localRotation;
        }
    }
}
