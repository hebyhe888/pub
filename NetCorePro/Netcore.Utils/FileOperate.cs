using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Utils
{
    public  class FileOperate
    {
        /// <summary>
        /// 读取文件内容
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static string FileStreamRead(string FilePath)
        {
            using (FileStream fs = new FileStream(FilePath, FileMode.Open))
            {
                Byte[] bys = new Byte[fs.Length];
                int r = fs.Read(bys, 0, bys.Length);
                return Encoding.UTF8.GetString(bys, 0, r);
            }
        }
        /// <summary>
        /// 向文件写入内容
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="Content"></param>
        public static void FileStreamWrite(string FilePath,string Content)
        {
            using (FileStream fsWrite = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(Content);
                fsWrite.Write(buffer, 0, buffer.Length);
            }
        }
    }
}
