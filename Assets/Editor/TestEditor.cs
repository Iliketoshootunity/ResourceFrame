using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TestEditor 
{
    private static string DLLPATH = "Assets/GameData/Code/HotFix.dll";
    private static string PDBPATH = "Assets/GameData/Code/HotFix.pdb";

    [MenuItem("Tools/修改热更dll为txt")]
    public static void ChangeDllName()
    {
        if (File.Exists(DLLPATH))
        {
            string targetPath = DLLPATH + ".txt";
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            File.Move(DLLPATH, targetPath);
        }

        if (File.Exists(PDBPATH))
        {
            string targetPath = PDBPATH + ".txt";
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            File.Move(PDBPATH, targetPath);
        }
        AssetDatabase.Refresh();
    }
}
