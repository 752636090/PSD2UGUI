using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace PSD2UGUI
{
    internal static class FileHelper
    {
        public static List<string> GetAllFiles(string dir, string searchPattern = "*")
        {
            List<string> list = new List<string>();
            GetAllFiles(list, dir, searchPattern);
            return list;
        }

        public static void GetAllFiles(List<string> files, string dir, string searchPattern = "*")
        {
            string[] fls = Directory.GetFiles(dir);
            foreach (string fl in fls)
            {
                files.Add(fl);
            }

            string[] subDirs = Directory.GetDirectories(dir);
            foreach (string subDir in subDirs)
            {
                GetAllFiles(files, subDir, searchPattern);
            }
        }

        public static void CleanDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                return;
            }
            foreach (string subdir in Directory.GetDirectories(dir))
            {
                Directory.Delete(subdir, true);
            }

            foreach (string subFile in Directory.GetFiles(dir))
            {
                File.Delete(subFile);
            }
        }

        public static void CopyDirectory(string srcDir, string tgtDir)
        {
            DirectoryInfo source = new DirectoryInfo(srcDir);
            DirectoryInfo target = new DirectoryInfo(tgtDir);

            if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("父目录不能拷贝到子目录！");
            }

            if (!source.Exists)
            {
                return;
            }

            if (!target.Exists)
            {
                target.Create();
            }

            FileInfo[] files = source.GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                File.Copy(files[i].FullName, Path.Combine(target.FullName, files[i].Name), true);
            }

            DirectoryInfo[] dirs = source.GetDirectories();

            for (int j = 0; j < dirs.Length; j++)
            {
                CopyDirectory(dirs[j].FullName, Path.Combine(target.FullName, dirs[j].Name));
            }
        }

        public static void ReplaceExtensionName(string srcDir, string extensionName, string newExtensionName)
        {
            if (Directory.Exists(srcDir))
            {
                string[] fls = Directory.GetFiles(srcDir);

                foreach (string fl in fls)
                {
                    if (fl.EndsWith(extensionName))
                    {
                        File.Move(fl, fl.Substring(0, fl.IndexOf(extensionName)) + newExtensionName);
                        File.Delete(fl);
                    }
                }

                string[] subDirs = Directory.GetDirectories(srcDir);

                foreach (string subDir in subDirs)
                {
                    ReplaceExtensionName(subDir, extensionName, newExtensionName);
                }
            }
        }

        /// <summary>
        /// 读取文本文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileText(string filePath)
        {
            string content = string.Empty;

            if (!File.Exists(filePath))
            {
                return content;
            }

            using (StreamReader sr = File.OpenText(filePath))
            {
                content = sr.ReadToEnd();
            }
            return content;
        }

        #region CreateTextFile 创建文本文件
        /// <summary>
        /// 创建文本文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content"></param>
        public static void CreateTextFile(string filePath, string content)
        {
            DeleteFile(filePath);

            using (FileStream fs = File.Create(filePath))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(content.ToString());
                }
            }
        }
        #endregion

        #region DeleteFile 删除文件
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="filePath"></param>
        public static void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

#if UNITY_EDITOR
        //private const int FO_DELETE = 0x3;
        //private const ushort FOF_NOCONFIRMATION = 0x10;
        //private const ushort FOF_ALLOWUNDO = 0x40;

        //[DllImport("shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        //private static extern int SHFileOperation([In, Out] _SHFILEOPSTRUCT str);

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        //public class _SHFILEOPSTRUCT
        //{
        //    public IntPtr hwnd;
        //    public UInt32 wFunc;
        //    public string pFrom;
        //    public string pTo;
        //    public UInt16 fFlags;
        //    public Int32 fAnyOperationsAborted;
        //    public IntPtr hNameMappings;
        //    public string lpszProgressTitle;
        //}
        ///// <summary>
        ///// 移动到回收站中
        ///// </summary>
        ///// <param name="path"></param>
        ///// <returns></returns>
        //public static int MoveToTrash(string path)
        //{
        //    _SHFILEOPSTRUCT pm = new _SHFILEOPSTRUCT();
        //    pm.wFunc = FO_DELETE;
        //    pm.pFrom = path + '\0';
        //    pm.pTo = null;
        //    pm.fFlags = FOF_ALLOWUNDO | FOF_NOCONFIRMATION;
        //    return SHFileOperation(pm);
        //} 
#endif
        #endregion

        #region CopyDirectory 拷贝文件夹
        ///// <summary>
        ///// 拷贝文件夹
        ///// </summary>
        ///// <param name="sourceDirName"></param>
        ///// <param name="destDirName"></param>
        //public static void CopyDirectory(string sourceDirName, string destDirName)
        //{
        //    try
        //    {
        //        if (!Directory.Exists(destDirName))
        //        {
        //            Directory.CreateDirectory(destDirName);
        //            File.SetAttributes(destDirName, File.GetAttributes(sourceDirName));

        //        }

        //        if (destDirName[destDirName.Length - 1] != Path.DirectorySeparatorChar)
        //            destDirName = destDirName + Path.DirectorySeparatorChar;

        //        string[] files = Directory.GetFiles(sourceDirName);
        //        foreach (string file in files)
        //        {
        //            if (File.Exists(destDirName + Path.GetFileName(file)))
        //                continue;
        //            FileInfo fileInfo = new FileInfo(file);
        //            if (fileInfo.Extension.Equals(".meta", StringComparison.CurrentCultureIgnoreCase)
        //                || fileInfo.Extension.Equals(".manifest", StringComparison.CurrentCultureIgnoreCase)
        //                )
        //                continue;

        //            File.Copy(file, destDirName + Path.GetFileName(file), true);
        //            File.SetAttributes(destDirName + Path.GetFileName(file), FileAttributes.Normal);
        //        }

        //        string[] dirs = Directory.GetDirectories(sourceDirName);
        //        foreach (string dir in dirs)
        //        {
        //            CopyDirectory(dir, destDirName + Path.GetFileName(dir));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        #endregion

        #region GetFileBuffer 读取本地文件到byte数组
        /// <summary>
        /// 读取本地文件到byte数组
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static byte[] GetFileBuffer(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            byte[] buffer = null;

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, buffer.Length);
            }
            return buffer;
        }
        #endregion



        public static void CopyFile(string srcDir, string tgtDir)
        {
            FileInfo source = new FileInfo(srcDir);
            FileInfo target = new FileInfo(tgtDir);

            if (!source.Exists)
            {
                return;
            }

            if (!target.Exists)
            {
                target.Create();
            }

            File.Copy(srcDir, tgtDir, true);
        }

        public static void CreateFile(string path, byte[] bytes)
        {
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllBytes(path, bytes);
        }
    }
}
