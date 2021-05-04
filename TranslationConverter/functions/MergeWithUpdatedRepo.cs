using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace TranslationConverter.functions
{
    internal static class MergeWithUpdatedRepo
    {
        public static void Run(bool translationClean, string workingDir, string FreshTLFile)
        {
            var hMasterWriter = Misc.WriterMake($@"{workingDir}\masterH.txt", false);
            var comMasterWriter = Misc.WriterMake($@"{workingDir}\masterCom.txt", false);
            Dictionary<string, string> hMasterDictionary = new Dictionary<string, string>();
            Dictionary<string, string> comMasterDictionary = new Dictionary<string, string>();

            var currentCSVSection = "";
                var CurrentPers = FreshTLFile.Remove(FreshTLFile.Length - 4, 4).Remove(0, 8);

            foreach (var RepoTLFile in Directory.EnumerateFiles(@"abdata", "*.txt", SearchOption.AllDirectories))
            {
                if (!CheckChar(CurrentPers, RepoTLFile)) continue;

                var FreshTLReader = File.ReadAllLines(FreshTLFile);
                var RepoTLReader = File.ReadAllLines(RepoTLFile);

                string outputDir;

                Console.WriteLine(RepoTLFile);

                outputDir = $@"{workingDir}\1translation\{RepoTLFile.Remove(RepoTLFile.Length - 15, 15)}";

                Misc.CheckFolderEx(outputDir);
                var fileWriter = Misc.WriterMake($@"{workingDir}\1translation\{RepoTLFile}", false);

                foreach (var currentRepoLine in RepoTLReader)
                {
                    var currentRepoLineSplit = currentRepoLine.Replace("//", "").Split('=');
                    var hit = false;

                    if (currentRepoLineSplit[0] == "")
                    {
                        fileWriter.WriteLine("");
                        continue;
                    }

                    foreach (var FreshTLLine in FreshTLReader)
                    {
                        var currentFreshLineSplit = FreshTLLine.Split(';');

                        if (currentFreshLineSplit[0].Contains(@"abdata\adv\scenario") ||
                            currentFreshLineSplit[0].Contains(@"abdata\communication") ||
                            currentFreshLineSplit[0].Contains(@"abdata\h"))
                            currentCSVSection = currentFreshLineSplit[0];

                        if (currentCSVSection != RepoTLFile || hit || currentFreshLineSplit[0].Replace(" ", "") !=
                            currentRepoLineSplit[0].Replace(" ", "")) continue;

                        var result = $"{currentRepoLineSplit[0]}={currentFreshLineSplit[1]}";
                        if (translationClean)
                            try
                            {
                                result += $"={currentFreshLineSplit[2]}";
                            }
                            catch (Exception)
                            {
                                result += "=";
                            }

                        if (RepoTLFile.Contains("h\\"))
                        {
                            try
                            {
                                hMasterDictionary.Add(currentFreshLineSplit[0], currentFreshLineSplit[1]);
                                if (translationClean)
                                {
                                    try
                                    {
                                        hMasterWriter.WriteLine($"{currentFreshLineSplit[0]}={currentFreshLineSplit[1]}={currentFreshLineSplit[2]}");
                                    }
                                    catch (Exception)
                                    {
                                        hMasterWriter.WriteLine($"{currentFreshLineSplit[0]}={currentFreshLineSplit[1]}=");
                                    }
                                }
                                else
                                {
                                    hMasterWriter.WriteLine($"{currentFreshLineSplit[0]}={currentFreshLineSplit[1]}");
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                        else if (RepoTLFile.Contains("communication\\"))
                        {
                            if ( currentFreshLineSplit[1] == String.Empty) 
                                break;

                            currentFreshLineSplit[0] = Regex.Replace(currentFreshLineSplit[0], @"(^[a-zA-Z]+\[.*]:)", "");
                            try
                            {
                                comMasterDictionary.Add(currentFreshLineSplit[0], currentFreshLineSplit[1]);
                                if (translationClean)
                                {
                                    try
                                    {
                                        comMasterWriter.WriteLine($"{currentFreshLineSplit[0]}={currentFreshLineSplit[1]}={currentFreshLineSplit[2]}");
                                    }
                                    catch (Exception)
                                    {
                                        comMasterWriter.WriteLine($"{currentFreshLineSplit[0]}={currentFreshLineSplit[1]}=");
                                    }
                                }
                                else
                                {
                                    comMasterWriter.WriteLine($"{currentFreshLineSplit[0]}={currentFreshLineSplit[1]}");
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }

                        fileWriter.WriteLine(result);
                        hit = true;
                    }

                    if (!hit)
                    {
                        var result = $"{currentRepoLineSplit[0]}=";
                        if (translationClean)
                            result += "=";

                        fileWriter.WriteLine(result);
                    }
                }

                fileWriter.Close();
            }
            comMasterWriter.Close();
            hMasterWriter.Close();
        }

        public static bool CheckChar(string CurrentPers, string CurrentFile)
        {
            var advMatch = CurrentFile.Contains($"c{CurrentPers}") && CurrentFile.Contains(@"abdata\adv\scenario");
            var comMatch = CurrentFile.Contains($@"communication_{CurrentPers}") ||
                           CurrentFile.Contains($@"optiondisplayitems_{CurrentPers}") ||
                           CurrentFile.Contains($@"communication_off_{CurrentPers}");
            var hMatch = CurrentFile.Contains($"personality_voice_c{CurrentPers}");
            if (advMatch || comMatch || hMatch) return true;
            return false;
        }
    }
}