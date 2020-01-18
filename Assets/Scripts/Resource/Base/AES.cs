using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/// <summary>
/// AES文件的加密解密
/// </summary>
public class AES : MonoBehaviour
{
    public static string AESKey = "xiaohailin";
    /// <summary>
    /// 加密之后的标识头
    /// </summary>
    private static string AESHead = "AESHeadEnc";

    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="fiePath"></param>
    /// <param name="EncryptKey"></param>
    public static void AESFileEncrypt(string filePath, string encryptKey)
    {
        if (!File.Exists(filePath))
        {
            return;

        }
        try
        {
            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                if (fs != null)
                {
                    //看头是不是AESHeadEnc，如果是，表示已经加密过了
                    byte[] headBuffer = new byte[10];
                    fs.Read(headBuffer, 0, 10);
                    string headStr = Encoding.UTF8.GetString(headBuffer);
                    if (headStr == AESHead)
                    {
#if UNITY_EDITOR
                        Debug.Log(string.Format("{0}已经加密过了", filePath));
#endif
                        return;
                    }
                    //加密
                    fs.Seek(0, SeekOrigin.Begin);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, Convert.ToInt32(fs.Length));
                    fs.Seek(0, SeekOrigin.Begin);
                    fs.SetLength(0);
                    byte[] encryptBuffer = AESEncrypt(buffer, encryptKey);       
                    byte[] headBuffer2 = Encoding.UTF8.GetBytes(AESHead);
                    fs.Write(headBuffer2, 0, headBuffer2.Length);
                    fs.Write(encryptBuffer, 0, encryptBuffer.Length);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(string.Format(" AES File Encrypt Error {0}", e.ToString()));
        }

    }


    /// <summary>
    /// 解密（会改动加密文件，不适合运行时使用）
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="DecryptKey"></param>
    public static void AESFileDecrypt(string filePath, string DecryptKey)
    {
        if (!File.Exists(filePath))
        {
            return;
        }
        try
        {
            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                if (fs != null)
                {
                    byte[] buffer = new byte[10];
                    fs.Read(buffer, 0, buffer.Length);
                    string headStr = Encoding.UTF8.GetString(buffer);
                    if (headStr == AESHead)
                    {
                        byte[] fileBuffer = new byte[fs.Length -buffer.Length];
                        fs.Read(fileBuffer, 0, Convert.ToInt32(fs.Length) - buffer.Length);
                        fs.Seek(0, SeekOrigin.Begin);
                        fs.SetLength(0);
                        byte[] fileBufferDecrypt = AES.AESDecrypt(fileBuffer, DecryptKey);
                        fs.Write(fileBufferDecrypt, 0, fileBufferDecrypt.Length);
                    }
                    else
                    {
                        Debug.LogError(string.Format(" AES File Decrypt Error {0}", "头字符串未匹配成功"));
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format(" AES File Decrypt Error {0}", e.ToString()));
        }
    }
    /// <summary>
    /// 解密，并返回解密后的byte[]数组
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="DecryptKey"></param>
    public static byte[] AESFileDecryptBytes(string filePath, string DecryptKey)
    {
        byte[] retBuff = null;
        if (!File.Exists(filePath))
        {
            return null;
        }
        try
        {
            using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                if (fs != null)
                {
                    byte[] buffer = new byte[10];
                    fs.Read(buffer, 0, buffer.Length);
                    string headStr = Encoding.UTF8.GetString(buffer);
                    if (headStr == AESHead)
                    {
                        byte[] fileBuffer = new byte[fs.Length - buffer.Length];
                        fs.Read(fileBuffer, 0, Convert.ToInt32(fs.Length) - buffer.Length);
                        retBuff = AES.AESDecrypt(fileBuffer, DecryptKey);
                    }
                    else
                    {
                        Debug.LogError(string.Format(" AES File Decrypt Error {0}", "头字符串未匹配成功"));
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(string.Format(" AES File Decrypt Error {0}", e.ToString()));
        }
        return retBuff;
    }


    /// <summary>
    /// AES 加密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
    /// </summary>
    /// <param name="EncryptString">待加密密文</param>
    /// <param name="EncryptKey">加密密钥</param>
    public static string AESEncrypt(string EncryptString, string EncryptKey)
    {
        return Convert.ToBase64String(AESEncrypt(Encoding.Default.GetBytes(EncryptString), EncryptKey));
    }

    /// <summary>
    /// AES 加密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
    /// </summary>
    /// <param name="EncryptString">待加密密文</param>
    /// <param name="EncryptKey">加密密钥</param>
    public static byte[] AESEncrypt(byte[] EncryptByte, string EncryptKey)
    {
        if (EncryptByte.Length == 0) { throw (new Exception("明文不得为空")); }
        if (string.IsNullOrEmpty(EncryptKey)) { throw (new Exception("密钥不得为空")); }
        byte[] m_strEncrypt;
        byte[] m_btIV = Convert.FromBase64String("Rkb4jvUy/ye7Cd7k89QQgQ==");
        byte[] m_salt = Convert.FromBase64String("gsf4jvkyhye5/d7k8OrLgM==");
        Rijndael m_AESProvider = Rijndael.Create();
        try
        {
            MemoryStream m_stream = new MemoryStream();
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(EncryptKey, m_salt);
            ICryptoTransform transform = m_AESProvider.CreateEncryptor(pdb.GetBytes(32), m_btIV);
            CryptoStream m_csstream = new CryptoStream(m_stream, transform, CryptoStreamMode.Write);
            m_csstream.Write(EncryptByte, 0, EncryptByte.Length);
            m_csstream.FlushFinalBlock();
            m_strEncrypt = m_stream.ToArray();
            m_stream.Close(); m_stream.Dispose();
            m_csstream.Close(); m_csstream.Dispose();
        }
        catch (IOException ex) { throw ex; }
        catch (CryptographicException ex) { throw ex; }
        catch (ArgumentException ex) { throw ex; }
        catch (Exception ex) { throw ex; }
        finally { m_AESProvider.Clear(); }
        return m_strEncrypt;
    }


    /// <summary>
    /// AES 解密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
    /// </summary>
    /// <param name="DecryptString">待解密密文</param>
    /// <param name="DecryptKey">解密密钥</param>
    public static string AESDecrypt(string DecryptString, string DecryptKey)
    {
        return Convert.ToBase64String(AESDecrypt(Encoding.Default.GetBytes(DecryptString), DecryptKey));
    }

    /// <summary>
    /// AES 解密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
    /// </summary>
    /// <param name="DecryptString">待解密密文</param>
    /// <param name="DecryptKey">解密密钥</param>
    public static byte[] AESDecrypt(byte[] DecryptByte, string DecryptKey)
    {
        if (DecryptByte.Length == 0) { throw (new Exception("密文不得为空")); }
        if (string.IsNullOrEmpty(DecryptKey)) { throw (new Exception("密钥不得为空")); }
        byte[] m_strDecrypt;
        byte[] m_btIV = Convert.FromBase64String("Rkb4jvUy/ye7Cd7k89QQgQ==");
        byte[] m_salt = Convert.FromBase64String("gsf4jvkyhye5/d7k8OrLgM==");
        Rijndael m_AESProvider = Rijndael.Create();
        try
        {
            MemoryStream m_stream = new MemoryStream();
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(DecryptKey, m_salt);
            ICryptoTransform transform = m_AESProvider.CreateDecryptor(pdb.GetBytes(32), m_btIV);
            CryptoStream m_csstream = new CryptoStream(m_stream, transform, CryptoStreamMode.Write);
            m_csstream.Write(DecryptByte, 0, DecryptByte.Length);
            m_csstream.FlushFinalBlock();
            m_strDecrypt = m_stream.ToArray();
            m_stream.Close(); m_stream.Dispose();
            m_csstream.Close(); m_csstream.Dispose();
        }
        catch (IOException ex) { throw ex; }
        catch (CryptographicException ex) { throw ex; }
        catch (ArgumentException ex) { throw ex; }
        catch (Exception ex) { throw ex; }
        finally { m_AESProvider.Clear(); }
        return m_strDecrypt;
    }

}
