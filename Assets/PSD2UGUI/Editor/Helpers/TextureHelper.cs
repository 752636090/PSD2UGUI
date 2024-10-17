using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Policy;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Color = UnityEngine.Color;
using CSImage = System.Drawing.Image;
using Graphics = UnityEngine.Graphics;

namespace PSD2UGUI
{
    internal static class TextureHelper
    {
        public static Texture2D DeCompress(this Texture source)
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
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        public static Texture2D ToTexture2D(this Sprite sprite)
        {
            Texture2D texture = new((int)sprite.rect.width, (int)sprite.rect.height);
            Texture2D srcTexture = sprite.texture;
            if (!sprite.texture.isReadable)
            {
                srcTexture = srcTexture.DeCompress();
            }
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
        /// <returns>border: 左-上-右-下</returns>
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

                border.x = maxStartIndex;
                border.z = self.width - (maxStartIndex + maxSameCount - 1) - 1;
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

                border.y = maxStartIndex;
                border.w = self.height - (maxStartIndex + maxSameCount - 1) - 1;
            }

            // border 左-上-右-下
            int width = horizontal ? border.z + border.x : self.width;
            int height = vertical ? border.w + border.y : self.height;
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

        public static string GetTextureMD5(this Texture2D texture)
        {
            return texture.DeCompress().EncodeToPNG().GetMD5();
        }

        public static string GetFileMD5(this Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            if (path == null)
            {
                Debug.LogError("文件不存在");
                return texture.GetTextureMD5();
            }
            return MD5Helper.FileMD5(path);
        }

        public static void SaveAsSprite(this Texture2D texture, string path, int4 border = default)
        {
            FileHelper.CreateFile(path, texture.DeCompress().EncodeToPNG());
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
