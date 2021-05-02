using System;
using System.IO;

namespace TranslationConverter.functions
{
    static class FillComDupes
    {
        public static void RunTask(bool translationClean, string workingDir)
        {
            Console.Clear();
            var prevStepTl = $@"{workingDir}\1translation\abdata\communication";

            RunComparison(prevStepTl, translationClean, workingDir); // Actually doing the file comparison stuffs
        }

        private static void RunComparison(string prevStepTl, bool translationClean, string workingDir)
        {
            var master = File.ReadAllLines($@"{workingDir}\masterCom.txt");
            var filenumber = 0;

            foreach (var hTransFile in Directory.EnumerateFiles(prevStepTl, "*.txt", SearchOption.AllDirectories))
            {
                var hTranslationFiles = File.ReadAllLines(hTransFile); // Filename obtained
                var outputdir = hTransFile.Replace("1translation", "2translation").Remove(hTransFile.Length - 15, 15);
                filenumber++;
                Console.WriteLine($@"Working on file {outputdir}translation.txt...");
                //Console.Clear();

                Misc.CheckFolderEx(outputdir);

                var file = Misc.WriterMake($@"{outputdir}\translation.txt", true);

                foreach (var oldLine in hTranslationFiles)
                {
                    var splitOldLine = oldLine.Split('='); // splitOldLine[0] for comparison
                    var hit = false;

                    foreach (var masterLine in master)
                    {
                        var splitMasterLine = masterLine.Split('='); // splitMasterLine[0] for comparison

                        if (splitOldLine[0] != splitMasterLine[0] && !hit)
                        {
                            splitMasterLine[0] = splitMasterLine[0].Replace("♡", "");
                            splitMasterLine[0] = splitMasterLine[0].Replace("★", "");
                        }

                        if (splitOldLine[0] != splitMasterLine[0] || hit) continue;
                        if (translationClean)
                        {
                            try
                            {
                                file.Write(splitMasterLine[0] + "=" + splitMasterLine[1] + "=" + splitOldLine[2] + "\n");
                            }
                            catch (Exception)
                            {
                                file.Write(splitMasterLine[0] + "=" + splitMasterLine[1] + "\n");
                            }
                        }
                        else
                            file.Write($"{splitMasterLine[0]}={splitMasterLine[1]}\n");
                        hit = true;
                    }

                    if (splitOldLine[0] == "")
                        file.Write("\n");
                    else if (!hit)
                        try
                        {
                            file.Write(splitOldLine[0] + "=" + splitOldLine[1] + "=" + splitOldLine[2] + "\n");
                        }
                        catch (Exception)
                        {
                            file.Write(splitOldLine[0] + "=" + splitOldLine[1] + "\n");
                        }
                }

                file.Close();
            }
        }
    }
}
