﻿using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using CSImage = System.Drawing.Image;
using Image = UnityEngine.UI.Image;

namespace PSD2UGUI
{
    internal static class Class1
    {
        //[MenuItem("Test/Test")]
        //private static void Test()
        //{
        //    string path = "Assets/Demo/Bundles/UIRes/DlgTest/图层 2.png";
        //    Debug.Log(MD5Helper.FileMD5(path));
        //    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        //    Debug.Log(texture.GetTextureMD5());
        //    texture.SaveAsSprite("Assets/Demo/图层  2.png");
        //    texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Demo/图层  2.png");
        //    Debug.Log(texture.GetTextureMD5());
        //    //Debug.Log(MD5Helper.FileMD5("Assets/Demo/Bundles/UIRes/DlgTest/图层 1.png"));
        //    //Debug.Log(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Demo/Bundles/UIRes/DlgTest/图层 1.png").GetTextureMD5());
        //}

        //[MenuItem("Test/TestImageMd5")]
        //private static void TestImageMd5()
        //{
        //    string path = "Assets/Demo/QQ20241015-162007.png";
        //    string result1 = MD5.Create().ComputeHash(AssetDatabase.LoadAssetAtPath<Texture2D>(path).GetRawTextureData()).ToHex("x2");
        //    string result2 = MD5Helper.FileMD5(path);
        //    string result3 = MD5.Create().ComputeHash(FileHelper.GetFileBuffer(path)).ToHex("x2");
        //    string result4 = MD5.Create().ComputeHash(AssetDatabase.LoadAssetAtPath<Texture2D>(path).DeCompress().GetRawTextureData()).ToHex("x2");
        //    Debug.Log(result1 == result2);
        //    Debug.Log(result1);
        //    Debug.Log(result2);
        //    Debug.Log(result3);
        //    Debug.Log(result4);
        //    //Debug.Log(AssetDatabase.LoadAssetAtPath<Texture2D>(path).GetRawTextureData().Length);
        //    //Debug.Log(FileHelper.GetFileBuffer(path).Length);

        //    Debug.Log(MD5.Create().ComputeHash(AssetDatabase.LoadAssetAtPath<Texture2D>(path).DeCompress().EncodeToPNG()).ToHex("x2"));

        //    CSImage image = CSImage.FromFile(path);
        //    Bitmap bitmap = new(image);
        //    bitmap.Save("Assets/Demo/QQ20241015-162007_1.png");
        //    image.Dispose();
        //    bitmap.Dispose();
        //    image = CSImage.FromFile("Assets/Demo/QQ20241015-162007_1.png");
        //    bitmap = new(image);
        //    bitmap.Save("Assets/Demo/QQ20241015-162007_2.png");
        //    image.Dispose();
        //    bitmap.Dispose();
        //    Debug.Log(MD5Helper.FileMD5("Assets/Demo/QQ20241015-162007_1.png"));
        //    Debug.Log(MD5Helper.FileMD5("Assets/Demo/QQ20241015-162007_2.png"));
        //}

        //[MenuItem("Test/TestTextureMd5")]
        //public static void TestTextureMd5()
        //{
        //    string path = "Assets/Demo/QQ20241015-162007.png";
        //    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        //    Texture2D texture = sprite.ToTexture2D(false);
        //    Debug.Log(MD5.Create().ComputeHash(texture.GetRawTextureData()).ToHex("x2"));
        //    Debug.Log(MD5.Create().ComputeHash(texture.EncodeToPNG()).ToHex("x2"));
        //    texture.Apply();
        //    Debug.Log(MD5.Create().ComputeHash(texture.GetRawTextureData()).ToHex("x2"));
        //    Debug.Log(MD5.Create().ComputeHash(texture.EncodeToPNG()).ToHex("x2"));
        //    FileHelper.CreateFile("Assets/Demo/QQ20241015-162007_1.png", texture.EncodeToPNG());
        //    AssetDatabase.Refresh();
        //    //Debug.Log(MD5.Create().ComputeHash(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Demo/QQ20241015-162007_1.png").EncodeToPNG()).ToHex("x2"));

        //    Debug.Log(MD5.Create().ComputeHash(texture.DeCompress().EncodeToPNG()).ToHex("x2"));
        //    Debug.Log(MD5.Create().ComputeHash(AssetDatabase.LoadAssetAtPath<Texture>("Assets/Demo/QQ20241015-162007_1.png").DeCompress().EncodeToPNG()).ToHex("x2"));
        //    Debug.Log(MD5Helper.FileMD5("Assets/Demo/QQ20241015-162007_1.png"));
        //}

        //[MenuItem("Test/TestMd5Time")]
        //public static void TestMd5Time()
        //{
        //    string path = "Assets/Demo/QQ20241015-162007.png";
        //    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

        //    DateTime time = DateTime.Now;
        //    byte[] bytes1 = texture/*.DeCompress()*/.EncodeToPNG();
        //    Debug.Log(DateTime.Now - time);

        //    time = DateTime.Now;
        //    FileHelper.CreateFile("Assets/Demo/QQ20241015-162007_3.png", texture/*.DeCompress()*/.EncodeToPNG());
        //    //using CSImage image = CSImage.FromFile("Assets/Demo/QQ20241015-162007_1.png");
        //    //using Bitmap bitmap = new(image);
        //    byte[] bytes2 = FileHelper.GetFileBuffer("Assets/Demo/QQ20241015-162007_3.png");
        //    Debug.Log(DateTime.Now - time);

        //    Debug.Log(bytes1.GetMD5());
        //    Debug.Log(bytes2.GetMD5());
        //}

        //[MenuItem("Test/TestCreateTexture")]
        //public static void TestCreateTexture()
        //{
        //    string path = "Assets/Demo/QQ20241015-162007.png";
        //    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        //    texture.SaveAsSprite("Assets/Demo/QQ20241015-162007_3.png");
        //    Debug.Log(AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Demo/QQ20241015-162007_3.png"));
        //}
    }
}