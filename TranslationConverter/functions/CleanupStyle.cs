using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TranslationConverter.functions
{
    class CleanupStyle
    {
        public static void Runner(bool translationClean, string WorkDir)
        {
            string startDir = $@"{WorkDir}\2translation";
            string endDir = $@"{WorkDir}\3translation";
            Misc.CheckFolderDel(endDir);

            foreach (var translationFile in Directory.EnumerateFiles(startDir, "*.txt", SearchOption.AllDirectories))
            {
                string[] translationFileReader = File.ReadAllLines(translationFile);
                string outFile = translationFile.Remove(translationFile.Length - 15, 15)
                    .Replace("2translation", "3translation");
                Misc.CheckFolderEx(outFile);
                var Outputter = Misc.WriterMake($@"{outFile}\translation.txt", false);

                foreach (var translationLine in translationFileReader)
                {
                    string[] translationLineSplit = translationLine.Split('=');

                    string result;

                    try
                    {
                        string temp = RunFix(translationLineSplit[1]);
                        result = $"{translationLineSplit[0]}={temp}";
                        if (translationClean)
                        {
                            try
                            {
                                result += $@"={translationLineSplit[2]}";
                            }
                            catch (Exception)
                            {
                                result += $@"=";
                            }
                        }
                    }
                    catch (Exception)
                    {
                        result = translationLine;
                    }
                    Outputter.WriteLine(result);
                }
                Outputter.Close();
            }
        }

        public static string RunFix(string translation)
        {
            var replacer = translation;

            if (replacer == "")
            {
                return replacer.TrimEnd();
            }

            List<string> killstringList = new List<string>
            {
                "(Anal Virgin)",
                "(Limited to lovers)",
                "(Beginning Dialogue)",
                "(Rejection and Removal)",
                "(Too Intense, Rejected)",
                "(Rejection)",
                "CHOICE:",
                "(Rejection and Removal)",
                "(First Kiss)",
                "(Rejection and Removal)",
                "(Virgin)",
                "(First Kiss)",
                "(Limited to lovers)",
                "(Limited to the first touch during an H-scene)",
                "(First Insertion)"
            };

            replacer = replacer.Replace("…", "...");
            //replacer = replacer.Replace("..", "...");
            replacer = replacer.Replace("....", "...");
            replacer = replacer.Replace("......", "...");

            replacer = Regex.Replace(replacer, @"(^|[^\.])(\.{3})+(\.{1,2})([^\.]|$)", "$1$2$4");
            //replacer = Regex.Replace(replacer, @"[\s](?<!\.)(\.{3})+(?!\.)([\S]|$)", "$2$3 $4");
            replacer = Regex.Replace(replacer, @"(^|[^\.])(\.{2})([^\.]|$)", "$1$2.$3");
            replacer = Regex.Replace(replacer, "[ ]{2,}", " ");

            replacer = replacer.Replace("\"\"", "\"");
            //replacer = replacer.Replace("$3 $4", "");

            foreach (string entry in killstringList)
            {
                replacer = replacer.Replace(entry, "");
            }

            return replacer.TrimEnd();
        }
    }
}
