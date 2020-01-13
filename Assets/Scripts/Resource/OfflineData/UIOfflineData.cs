using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI的离线数据
/// </summary>
public class UIOfflineData : OfflineData
{
    public Vector2[] AllAhchorMax;
    public Vector2[] AllAnchorMin;
    public Vector2[] AllPivot;
    public Vector2[] AllSizeDelta;
    public Vector3[] AllAnchoredPos;
    public ParticleSystem[] AllParticle;

    public override void ResetProp()
    {
        int allPointCount = AllPoint.Length;
        for (int i = 0; i < allPointCount; i++)
        {
            if (AllPoint[i] == null) continue;
            RectTransform temp = AllPoint[i] as RectTransform;
            if (temp != null)
            {
                temp.localPosition = AllPos[i];
                temp.localScale = AllScale[i];
                temp.localRotation = AllRot[i];
                temp.gameObject.SetActive(AllPointActive[i]);
                temp.anchorMax = AllAhchorMax[i];
                temp.anchorMin = AllAnchorMin[i];
                temp.pivot = AllPivot[i];
                temp.sizeDelta = AllSizeDelta[i];
                temp.anchoredPosition3D= AllAnchoredPos[i];
                int childCount = temp.childCount;

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

        for (int i = 0; i < AllParticle.Length; i++)
        {
            AllParticle[i].Clear();
            AllParticle[i].Play();
        }
    }

    public override void BindData()
    {
        Transform[] AllTransform = gameObject.GetComponentsInChildren<Transform>();
        int allTranformCount = AllTransform.Length;
        for (int i = 0; i < allTranformCount; i++)
        {
            if (!(AllTransform[i] is RectTransform))
            {
                AllTransform[i].gameObject.AddComponent<RectTransform>();
            }
        }
        AllPoint = gameObject.GetComponentsInChildren<RectTransform>(true);
        AllParticle = gameObject.GetComponentsInChildren<ParticleSystem>(true);
        AllPointChildCount = new int[allTranformCount];
        AllPointActive = new bool[allTranformCount];
        AllPos = new Vector3[allTranformCount];
        AllScale = new Vector3[allTranformCount];
        AllRot = new Quaternion[allTranformCount];
        AllAhchorMax = new Vector2[allTranformCount];
        AllAnchorMin = new Vector2[allTranformCount];
        AllPivot = new Vector2[allTranformCount];
        AllSizeDelta = new Vector2[allTranformCount];
        AllAnchoredPos = new Vector3[allTranformCount];
        for (int i = 0; i < allTranformCount; i++)
        {
            RectTransform rect = AllTransform[i] as RectTransform;
            AllPointChildCount[i] = rect.childCount;
            AllPointActive[i] = rect.gameObject.activeSelf;
            AllPos[i] = rect.localPosition;
            AllScale[i] = rect.localScale;
            AllRot[i] = rect.localRotation;
            AllAhchorMax[i] = rect.anchorMax;
            AllAnchorMin[i] = rect.anchorMin;
            AllPivot[i] = rect.pivot;
            AllSizeDelta[i] = rect.sizeDelta;
            AllAnchoredPos[i] = rect.anchoredPosition3D;
        }
    }

}
