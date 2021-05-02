using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TranslationConverter.functions
{
    class Misc
    {
        public static bool CheckFileExist(string inputFile)
        {
            if (!File.Exists(inputFile))
            {
                Console.Clear();
                Console.WriteLine($"{inputFile} does not exist!");
                return false;
            }
            else
            {
                return false;
            }
        }

        public static void CheckFileDel(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }

        public static void CheckFolderDel(string folder)
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }

        public static void CheckFolderEx(string folder)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }

        public static StreamWriter WriterMake(string fileName, bool append)
        {
            return new StreamWriter(fileName, append, Encoding.UTF8) {AutoFlush = true};
        }

        public static string CalculateMD5(string inputFile)
        {

            using (var md5Instance = MD5.Create())
            {
                using (var stream = File.OpenRead(inputFile))
                {
                    var hashResult = md5Instance.ComputeHash(stream);
                    return BitConverter.ToString(hashResult).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
