using System;
using System.Collections.Generic;
using System.IO;

namespace TranslationConverter.functions
{
    static class Advmatch
    {
        public static void Runner(bool translationClean, string workFolder, string charaType)
        {
            Dictionary<string, string> fileDictionary = new Dictionary<string, string>();
            foreach (var advFile in Directory.EnumerateFiles($@"abdata\adv", "*.txt", SearchOption.AllDirectories))
            {
                if(!MergeWithUpdatedRepo.CheckChar(charaType, advFile)) continue;

                string currentMD5 = Misc.CalculateMD5(advFile);

                string directoryIn = Path.GetDirectoryName($@"{workFolder}\1translation\{advFile}");
                string directoryOut = Path.GetDirectoryName($@"{workFolder}\2translation\{advFile}");

                try
                {
                    fileDictionary.Add(currentMD5, directoryIn);
                    Console.WriteLine($"New: { directoryIn}\translation.txt");
                }
                catch (Exception)
                {
                    directoryIn = fileDictionary[currentMD5];
                    Console.WriteLine($"Dupe: { directoryIn}\translation.txt");
                }

                if (directoryIn.Contains(@"\30\61"))
                    directoryIn = directoryIn.Replace(@"\30\61", @"\00\61");
                else if (directoryIn.Contains(@"\30\62"))
                    directoryIn = directoryIn.Replace(@"\30\62", @"\00\62");

                Misc.CheckFolderEx(directoryOut);
                Misc.CheckFileDel($@"{directoryOut}\translation.txt");
                File.Copy($@"{directoryIn}\translation.txt", $@"{directoryOut}\translation.txt");
            }
        }
    }
}
