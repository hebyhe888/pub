using Microsoft.Extensions.Hosting.Internal;
using System;
using System.IO;
using System.Net;

namespace Utils
{
    public class FileHelper
    {
        /// <summary>
        /// 将文件转换为byte数组
        /// </summary>
        /// <param name="path">文件地址</param>
        /// <returns>转换后的byte数组</returns>
        public static byte[] File2Bytes(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                string message = String.Format("读取文件时发生异常:{0}不存在", path);
                return new byte[0];
            }

            FileStream fs = null;
            byte[] buff = null;
            try
            {
                FileInfo fi = new FileInfo(path);
                buff = new byte[fi.Length];
                fs = fi.OpenRead();
                fs.Read(buff, 0, Convert.ToInt32(fs.Length));
                //fs.Close();
            }
            catch (Exception ex)
            {
                string message = String.Format("读取文件时发生异常:{0}", ex.Message);
                throw new Exception(message);
            }
            finally
            {
                if (null != fs)
                {
                    fs.Close();
                }
            }
            return buff;
        }


        /// <summary>
        /// 将PDF的Base64转换为文件并保存到指定地址
        /// </summary>
        /// <param name="buff">byte数组</param>
        /// <param name="savepath">保存地址</param>
        public static void SavePDFBase64AsFile(string pdfBase64, string savepath)
        {
            System.IO.MemoryStream stream = new System.IO.MemoryStream(Convert.FromBase64String(pdfBase64));
            byte[] bytes = stream.ToArray();
            // 将Byte数组转化PDF文件
            SaveBytesAsFile(bytes, savepath);
        }

        /// <summary>
        /// 将PDF的byte数组字符串转化为文件并保存到指定地址
        /// </summary>
        /// <param name="streamStr">byte数组字符串</param>
        /// <param name="savepath">保存地址</param>
        public static void SavePDFStreamAsFile(string streamStr, string savepath)
        {
            string arrayStream = streamStr.TrimStart('[').TrimEnd(']');
            string[] byteStreams = arrayStream.Split(',');
            int len = byteStreams.Length;
            byte[] bytes = new byte[len];
            // stream字符串还原Byte数组
            for (int i = 0; i < len; i++)
            {
                bytes[i] = (byte)Convert.ToInt32(byteStreams[i]);
            }
            // 将Byte数组转化PDF文件
            SaveBytesAsFile(bytes, savepath);
        }


        /// <summary>
        /// 将Byte数组转化为磁盘文件
        /// </summary>
        /// <param name="bytes">byte数组</param>
        /// <param name="savepath">保存地址</param>
        public static void SaveBytesAsFile(byte[] bytes, string savepath)
        {
            FileStream fs = null;
            BinaryWriter bw = null;
            try
            {
                fs = new FileStream(savepath, FileMode.Create);
                bw = new BinaryWriter(fs);
                bw.Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                string exMsg = string.Format(">>>> 保存文件时发生异常:: {0}  StackTrace: {1}", ex.Message, ex.StackTrace);
                throw new Exception(exMsg);
            }
            finally
            {
                if (null != bw)
                {
                    bw.Close();
                }
                if (null != fs)
                {
                    fs.Close();
                }
            }
        }

        //获取文件大小
        public static int GetFileLength(string path)
        {
            int length = 0;
            FileInfo fileInfo = null;
            try
            {
                fileInfo = new System.IO.FileInfo(path);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                // 其他处理异常的代码
            }
            // 如果文件存在
            if (fileInfo != null && fileInfo.Exists)
            {
                System.Diagnostics.FileVersionInfo info = System.Diagnostics.FileVersionInfo.GetVersionInfo(path);
                length = (int)Math.Ceiling(fileInfo.Length / 1024.0);
            }
            else
            {
                Console.WriteLine("指定的文件路径不正确!");
            }
            return length;
        }

        //获取文件ContentMD5
        public static string GetContentMD5FromFile(string filePath)
        {
            string ContentMD5 = null;
            try
            {
                FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                // 先计算出上传内容的MD5，其值是一个128位（128 bit）的二进制数组
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                // 再对这个二进制数组进行base64编码
                ContentMD5 = Convert.ToBase64String(retVal).ToString();
                return ContentMD5;
            }
            catch (Exception ex)
            {
                throw new Exception($"错误信息:计算文件的Content-MD5值时发生异常：{ex.Message}");
            }
        }
        /// <summary>
        /// 下载网络远程文件
        /// </summary>
        /// <param name="remote_path">远程文件全路径</param>
        /// <param name="filename">文件名带扩展名</param>
        /// <param name="basedir">相对基目录</param>
        /// <returns>返回数据库存储路径</returns>
        public static string DownLoadFile(string remote_path, string filename,string basedir= "Archive")
        {
            Stream inStream = null;
            FileStream filestream = null;
            var databaseimgurl = string.Empty;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(remote_path);
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                inStream = resp.GetResponseStream();
                long filesizeBytes = resp.ContentLength;
                var serverdir= Path.GetFullPath($"~/{basedir}").Replace("~\\","");
                if (!Directory.Exists(serverdir))
                {
                    Directory.CreateDirectory(serverdir);
                }
                var savefilepath = Path.GetFullPath($"~/{basedir}/{filename}").Replace("~\\","");
                databaseimgurl = $"~/{basedir}/{filename}";
                filestream = new FileStream(savefilepath, FileMode.OpenOrCreate, FileAccess.Write);
                int length = 1024;
                byte[] buffer = new byte[1025];
                int bytesread = 0;
                while ((bytesread = inStream.Read(buffer, 0, length)) > 0)
                {
                    filestream.Write(buffer, 0, bytesread);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (inStream != null)
                    inStream.Close();
                if (filestream != null)
                    filestream.Close();
            }
            return databaseimgurl;
        }
    }
}
