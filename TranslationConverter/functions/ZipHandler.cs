using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ionic.Zip;
using Ionic.Zlib;

namespace TranslationConverter.functions
{
    class ZipHandler
    {
        public static void Runner(string workFolder, string CurrentPers)
        {
            foreach (var translationFile in Directory.EnumerateFiles($@"{workFolder}\3translation", "*.txt", SearchOption.AllDirectories))
            {
                string[] fileReader = File.ReadAllLines(translationFile);
                string currentPath = Path.GetDirectoryName(translationFile);

                Misc.CheckFolderEx(currentPath.Replace("3translation", "4translation"));

                var fileWriter = Misc.WriterMake(translationFile.Replace("3translation", "4translation"), false);
                foreach (var currentLine in fileReader)
                {
                    string[] currentLineSplit = currentLine.Split('=');
                    try
                    {
                        fileWriter.WriteLine($"{currentLineSplit[0]}={currentLineSplit[1]}");
                    }
                    catch (Exception)
                    {
                        fileWriter.WriteLine(currentLine);
                    }
                }
                fileWriter.Close();
            }

            ZipHandler.Seal(@"TLCleanWork\4translation", workFolder, $"c{CurrentPers}.zip");
        }

        public static void Extract(string inputFile, string outputFolder)
        {
            if (Directory.Exists(outputFolder))
                Directory.Delete(outputFolder, true);

            Directory.CreateDirectory(outputFolder);
            try
            {
                using (var zip = ZipFile.Read(inputFile))
                {
                    zip.ExtractAll(outputFolder, ExtractExistingFileAction.OverwriteSilently);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }

        public static void Seal(string tempFolder, string outputFolder, string filename)
        {
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            try
            {
                using (var zip = new ZipFile())
                {
                    zip.CompressionLevel = CompressionLevel.Level0;
                    zip.CompressionMethod = CompressionMethod.None;
                    zip.AddDirectory(tempFolder);
                    zip.Save($"{outputFolder}\\{filename}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }
    }
}