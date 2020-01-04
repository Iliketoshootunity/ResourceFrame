using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildApp
{
    public static string m_AppName = PlayerSettings.productName;
    public static string m_AndroidPath = Application.dataPath + "/../BuildTarget/Android/";
    public static string m_iOSPath = Application.dataPath + "/../BuildTarget/iOS/";
    public static string m_WindowsPath = Application.dataPath + "/../BuildTarget/WIndows/";

    [MenuItem("Build/标准包")]
    public static void Build()
    {
        //打AssetBundle
        BundleEditor.BuildAB();
        //打包后的AssetBundle的路径
        string sourcePath = BundleEditor.BUILD_TARGET_PATH + "/";
        string targetPath = Application.streamingAssetsPath + "/";
        //将初始资源转到到StreamAsset路径下
        Copy(sourcePath, targetPath);
        string savePath = "";
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            savePath = m_AndroidPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget.ToString() + string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now) + ".apk";
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
        {
            savePath = m_iOSPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget.ToString() + string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now);
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64 || EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows)
        {
            savePath = m_WindowsPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget + string.Format("_{0:yyyy_MM_dd_HH_mm}/{1}.exe", DateTime.Now, m_AppName);
        }
        BuildPipeline.BuildPlayer(FindEnableLevel(), savePath, EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);
        //打包之后删除
        DeleteStreamAssets();
    }

    /// <summary>
    /// 复制
    /// </summary>
    /// <param name="sourceDirPath">源路径</param>
    /// <param name="targetDirPath">目标路径</param>
    public static void Copy(string sourceDirPath, string targetDirPath)
    {
        try
        {
            //如果源路径不存在
            if (!Directory.Exists(sourceDirPath))
            {
                return;
            }
            //如果目标路径没有
            if (!Directory.Exists(targetDirPath))
            {
                Directory.CreateDirectory(targetDirPath);
            }
            string[] files = Directory.GetFileSystemEntries(sourceDirPath);
            for (int i = 0; i < files.Length; i++)
            {
                //如果是文件夹
                if (Directory.Exists(files[i]))
                {
                    string childSourceDirPath = files[i] + "/";
                    string childTargetDirPath = targetDirPath + Path.GetDirectoryName(files[i]) + "/";
                    Copy(childSourceDirPath, childTargetDirPath);
                }
                else
                {
                    string newTargetPath = targetDirPath + Path.GetFileName(files[i]);
                    File.Copy(files[i], newTargetPath, true);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

    }

    public static string[] FindEnableLevel()
    {
        List<string> levelList = new List<string>();
        foreach (var item in EditorBuildSettings.scenes)
        {
            if (!item.enabled) continue;
            levelList.Add(item.path);
        }
        return levelList.ToArray();
    }

    public static void DeleteStreamAssets()
    {
        string[] files = Directory.GetFileSystemEntries(Application.streamingAssetsPath + "/");
        for (int i = 0; i < files.Length; i++)
        {
            if(Directory.Exists(files[i]))
            {
                Directory.Delete(files[i]);
            }
            else if(File.Exists(files[i]))
            {
                File.Delete(files[i]);
            }
        }

    }

}
