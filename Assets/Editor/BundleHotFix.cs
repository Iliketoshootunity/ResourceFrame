using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;

/// <summary>
/// 版本资源热更窗口
/// 打包当前版本与指定的历史版本对比，筛选出需要更新的资源
/// 保存在指定文件夹内
/// </summary>
public class BundleHotFix : EditorWindow
{

    [MenuItem("Tool/打热更资源包")]
    public static void OpenWindow()
    {
        BundleHotFix m_BundleHotFix = EditorWindow.GetWindow<BundleHotFix>(false, "热更资源包", true);
        m_BundleHotFix.position = new Rect(500, 100, 600, 200);
        m_BundleHotFix.Show();
    }

    private string md5Path = "";
    private string hotCount = "1";
    private OpenFileName m_OpenFileName = null;
    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        md5Path = EditorGUILayout.TextField("ABMD5文件路径", md5Path, GUILayout.Width(350), GUILayout.Height(20));
        if (GUILayout.Button("选择版本ABMD5文件", GUILayout.Width(150), GUILayout.Height(20)))
        {
            m_OpenFileName = new OpenFileName();
            m_OpenFileName.structSize = Marshal.SizeOf(m_OpenFileName);
            m_OpenFileName.filter = "ABMD5文件(*.bytes)\0*.bytes";
            m_OpenFileName.file = new string(new char[256]);
            m_OpenFileName.maxFile = m_OpenFileName.file.Length;
            m_OpenFileName.fileTitle = new string(new char[64]);
            m_OpenFileName.maxFileTitle = m_OpenFileName.fileTitle.Length;
            m_OpenFileName.initialDir = (Application.dataPath + "/../Version").Replace("/", "\\");//默认路径
            m_OpenFileName.title = "选择MD5窗口";
            m_OpenFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
            if (LocalDialog.GetSaveFileName(m_OpenFileName))
            {
                Debug.Log(m_OpenFileName.file);
                md5Path = m_OpenFileName.file;
            }

        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        hotCount = EditorGUILayout.TextField("ABMD5文件路径", hotCount, GUILayout.Width(350), GUILayout.Height(20));
        GUILayout.EndHorizontal();
        if (GUILayout.Button("开始打热更包", GUILayout.Width(100), GUILayout.Height(20)))
        {
            if (!string.IsNullOrEmpty(md5Path) && md5Path.EndsWith("bytes"))
            {
                BundleEditor.BuildAB(true, md5Path, hotCount);
            }
        }


    }

}

