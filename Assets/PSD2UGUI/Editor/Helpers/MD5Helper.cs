using System.IO;
using System.Security.Cryptography;

namespace PSD2UGUI
{
	internal static class MD5Helper
	{
		public static string FileMD5(string filePath)
		{
			byte[] retVal;
            using (FileStream file = new FileStream(filePath, FileMode.Open))
            {
	            MD5 md5 = MD5.Create();
				retVal = md5.ComputeHash(file);
			}
			return retVal.ToHex("x2");
		}

		public static string GetMD5(this byte[] bytes)
		{
			return MD5.Create().ComputeHash(bytes).ToHex("x2");
        }

		public static string GetMD5(this Stream stream)
		{
			return MD5.Create().ComputeHash(stream).ToHex("x2");
        }
	}
}
