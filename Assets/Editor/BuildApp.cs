using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class BuildApp
{

    public static void Copy(string sourceDirPath, string targetDirPath)
    {
        try
        {
            //如果源路径不存在
            if (!Directory.Exists(sourceDirPath))
            {
                return;
            }
            if (!Directory.Exists(sourceDirPath))
            {
                Directory.CreateDirectory(targetDirPath);
            }
            string[] files = Directory.GetFileSystemEntries(sourceDirPath);
            for (int i = 0; i < files.Length; i++)
            {
                //如果是文件夹
                if (Directory.Exists(files[i]))
                {
                    Copy(files[i] + Path.DirectorySeparatorChar, targetDirPath + Path.GetDirectoryName(files[i]));
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

}
