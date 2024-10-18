using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;
using CSImage = System.Drawing.Image;
using Graphics = UnityEngine.Graphics;
using CSColor = System.Drawing.Color;

namespace PSD2UGUI
{
    internal static class TextureHelper
    {
        public static Texture2D DeCompress(this Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Default);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(source.width, source.height);
            readableText.filterMode = FilterMode.Point;
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        public static Texture2D ToTexture2D(this Sprite sprite)
        {
            Texture2D texture = new((int)sprite.rect.width, (int)sprite.rect.height);
            texture.name = sprite.name;
            Texture2D srcTexture = sprite.texture;
            srcTexture = srcTexture.DeCompress();
            texture.filterMode = FilterMode.Point;
            texture.SetPixels(srcTexture.GetPixels(
                (int)sprite.textureRect.x,
                (int)sprite.textureRect.y,
                (int)sprite.textureRect.width,
                (int)sprite.textureRect.height));
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="horizontal">横向拉伸</param>
        /// <param name="vertical">纵向拉伸</param>
        /// <param name="apply"></param>
        /// <returns>border: 左-下-右-上</returns>
        public static (Texture2D texture, int4 border) Slice(this Texture2D self, bool horizontal, bool vertical)
        {
            // meta文件保存的顺序是左-下-右-上，然而Importer是左-上-右-下
            int4 border = int4.zero;

            if (horizontal)
            {
                // Key: StartIndex Value: SameCount
                Dictionary<int, int> sameCountDict = new() { { 0, 1 } };
                int lastStartIndex = 0;
                int maxStartIndex = 0;
                int maxSameCount = 1;
                for (int x = 1; x < self.width; x++)
                {
                    bool same = true;
                    for (int y = 0; y < self.height; y++)
                    {
                        Color color1 = self.GetPixel(x, y);
                        Color color2 = self.GetPixel(x - 1, y);
                        if (math.abs(color1.r - color2.r) > 0.01f
                            || math.abs(color1.g - color2.g) > 0.01f
                            || math.abs(color1.b - color2.b) > 0.01f
                            || math.abs(color1.a - color2.a) > 0.01f)
                        {
                            same = false;
                            break;
                        }
                        //if (x > 1)
                        //{
                        //    Color color3 = self.GetPixel(x - 2, y);
                        //    if (math.abs(color1.r - color3.r) > 0.01f
                        //        || math.abs(color1.g - color3.g) > 0.01f
                        //        || math.abs(color1.b - color3.b) > 0.01f
                        //        || math.abs(color1.a - color3.a) > 0.01f)
                        //    {
                        //        same = false;
                        //        break;
                        //    } 
                        //}
                    }
                    if (same)
                    {
                        if (++sameCountDict[lastStartIndex] > maxSameCount)
                        {
                            maxStartIndex = lastStartIndex;
                            maxSameCount = sameCountDict[lastStartIndex];
                        }
                    }
                    else
                    {
                        lastStartIndex = x;
                        sameCountDict[lastStartIndex] = 1;
                    }
                }

                int reduce = math.min(2, maxSameCount / 2 - 1);
                border.x = maxStartIndex + reduce;
                border.z = self.width - (maxStartIndex + maxSameCount - 1) - 1 + reduce;
            }

            if (vertical)
            {
                // Key: StartIndex Value: SameCount
                Dictionary<int, int> sameCountDict = new() { { 0, 1 } };
                int lastStartIndex = 0;
                int maxStartIndex = 0;
                int maxSameCount = 1;
                for (int y = 1; y < self.height; y++)
                {
                    bool same = true;
                    for (int x = 0; x < self.width; x++)
                    {
                        Color color1 = self.GetPixel(x, y);
                        Color color2 = self.GetPixel(x, y - 1);
                        if (math.abs(color1.r - color2.r) > 0.01f
                            || math.abs(color1.g - color2.g) > 0.01f
                            || math.abs(color1.b - color2.b) > 0.01f
                            || math.abs(color1.a - color2.a) > 0.01f)
                        {
                            same = false;
                            break;
                        }
                    }
                    if (same)
                    {
                        if (++sameCountDict[lastStartIndex] > maxSameCount)
                        {
                            maxStartIndex = lastStartIndex;
                            maxSameCount = sameCountDict[lastStartIndex];
                        }
                    }
                    else
                    {
                        lastStartIndex = y;
                        sameCountDict[lastStartIndex] = 1;
                    }
                }

                int reduce = math.min(2, maxSameCount / 2 - 1);
                border.y = maxStartIndex + reduce;
                border.w = self.height - (maxStartIndex + maxSameCount - 1) - 1 + reduce;
            }

            // border 左-下-右-上
            int width = horizontal ? border.z + border.x + 1 : self.width;
            int height = vertical ? border.w + border.y + 1 : self.height;
            Texture2D texture = new(width, height);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int realX = x > border.x ? x + (self.width - width) : x;
                    int realY = y > border.y ? y + (self.height - height) : y;
                    texture.SetPixel(x, y, self.GetPixel(realX, realY));
                }
            }
            texture.Apply();
            return (texture, border);
        }

        public static string GetTextureMD5(this Sprite sprite)
        {
            Texture2D texture = sprite.border == Vector4.zero ? sprite.texture : sprite.ToTexture2D();
            return texture.GetTextureMD5();
        }

        public static string GetTextureMD5(this Texture2D source)
        {
            //return texture.DeCompress().EncodeToPNG().GetMD5();
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Default);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D texture = new Texture2D(source.width, source.height);
            texture.filterMode = FilterMode.Point;
            texture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            //readableText.Apply();
            RenderTexture.active = previous;

            using MemoryStream stream = new();
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    Color color = texture.GetPixel(x, y);
                    int simplePixel = ((int)math.round(color.r * 2) << 6)
                        | ((int)math.round(color.g * 2) << 4)
                        | ((int)math.round(color.b * 2) << 2)
                        | ((int)math.round(color.a * 2) << 0);
                    stream.WriteByte((byte)simplePixel);
                }
            }
            stream.Seek(0, SeekOrigin.Begin);
            return stream.GetMD5();
        }

        public static string GetMD5(this CSImage image, MemoryStream tempStream = null)
        {
            bool needDispose = tempStream == null;
            tempStream = tempStream ?? new();
            tempStream.SetLength(0);
            tempStream.Seek(0, SeekOrigin.Begin);
            using Bitmap bitmap = new(image);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    CSColor color = bitmap.GetPixel(x, y);
                    int simplePixel = ((int)math.round(color.R * 2 / 256f) << 6)
                        | ((int)math.round(color.G * 2 / 256f) << 4)
                        | ((int)math.round(color.B * 2 / 256f) << 2)
                        | ((int)math.round(color.A * 2 / 256f) << 0);
                    tempStream.WriteByte((byte)simplePixel);
                }
            }
            tempStream.Seek(0, SeekOrigin.Begin);
            string md5 = tempStream.GetMD5();
            if (needDispose)
            {
                tempStream.Dispose();
            }
            return md5;
        }

        //public static string GetTextureMD5ByCS(this Texture2D texture)
        //{
        //    using MemoryStream stream1 = new(texture.DeCompress().EncodeToPNG());
        //    using CSImage csImage = CSImage.FromStream(stream1);
        //    using Bitmap bitmap = new(csImage);
        //    using MemoryStream stream2 = new();
        //    bitmap.Save(stream2, ImageFormat.Png);
        //    stream2.Seek(0, SeekOrigin.Begin);
        //    return stream2.GetMD5();
        //}

        //public static string GetTextureMD5ByCS(this Texture2D texture)
        //{
        //}

        //public static string GetFileMD5(this Texture2D texture)
        //{
        //    string path = AssetDatabase.GetAssetPath(texture);
        //    if (path == null)
        //    {
        //        Debug.LogError($"文件不存在 {texture.name}");
        //        return texture.GetTextureMD5();
        //    }
        //    return MD5Helper.FileMD5(path);
        //}

        public static void SaveAsSprite(this Texture2D texture, string path, int4 border = default)
        {
            FileHelper.CreateFile(path, texture.DeCompress().EncodeToPNG());
            //using MemoryStream stream1 = new(texture.DeCompress().EncodeToPNG());
            //using CSImage csImage = CSImage.FromStream(stream1);
            //using Bitmap bitmap = new(csImage);
            //string directory = Path.GetDirectoryName(path);
            //if (!Directory.Exists(directory))
            //{
            //    Directory.CreateDirectory(directory);
            //}
            //bitmap.Save(path, ImageFormat.Png);
            AssetDatabase.ImportAsset(path);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            if (!border.Equals(default))
            {
                importer.spriteBorder = new(border.x, border.y, border.z, border.w); 
            }
            importer.SaveAndReimport();
        }
    }
}
