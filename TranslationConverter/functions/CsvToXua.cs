using System;
using System.Collections.Generic;
using System.IO;

namespace TranslationConverter.functions
{
    static class CsvToXua
    {
        public static void Run(bool translationClean, string inputFile, string workingDir)
        {
            if (Misc.CheckFileExist(inputFile))
                return;
            var csvfile = inputFile;
            var tlFile = "";
            var lines = File.ReadAllLines(csvfile);
            var finishedLine = "";

            Misc.CheckFolderEx(workingDir);
            Misc.CheckFileDel($@"{workingDir}\masterH.txt"); 
            Misc.CheckFileDel($@"{workingDir}\masterCom.txt");

            var hmaster = Misc.WriterMake($@"{workingDir}\masterH.txt", true);
            var cmaster = Misc.WriterMake($@"{workingDir}\masterCom.txt", true);
            Dictionary<string, string> hMasterDictionary = new Dictionary<string, string>();
            Dictionary<string, string> comMasterDictionary = new Dictionary<string, string>();

            foreach (var line in lines)
            {
                var csvsplit = line.Split(';');

                if(line == ";")
                    continue;

                // Deciding what we're dealing with
                string lineType;
                switch (csvsplit[0])
                {
                    case string a when a.Contains("adv\\"):
                        lineType = "ADV";
                        break;
                    case string a when a.Contains("h\\"):
                        lineType = "H";
                        break;
                    case string a when a.Contains("communication\\"):
                        lineType = "COMMUNICATION";
                        break;
                    case string a when a.Contains("abdata\\"):
                        lineType = "GENERIC";
                        break;
                    default:
                        lineType = "Normal";
                        break;
                }

                // Generalizing linetype
                if (lineType == "ADV" || lineType == "H" || lineType == "COMMUNICATION" || lineType == "GENERIC")
                    lineType = "FileName";

                if (tlFile == "" && lineType != "FileName")
                    continue;

                switch (lineType)
                {
                    case "FileName"
                        : // Changing directory, makes sure it exists and writes comments to external file for preservation if they exist
                        tlFile = $@"{workingDir}\1translation\{csvsplit[0]}";
                        if (!tlFile.Contains("abdata"))
                            tlFile = $@"{workingDir}\1translation\abdata\{csvsplit[0]}";
                        var tlPath = tlFile.Remove(tlFile.Length - 15, 15);

                        Misc.CheckFolderEx(tlPath);

                        finishedLine = "";
                        Console.WriteLine("Writing to " + tlFile);
                        continue;
                    case "BlankLine":
                        finishedLine = "";
                        break;
                    case "Normal":
                        finishedLine = $"{csvsplit[0]}={csvsplit[1]}";
                        if (translationClean)
                        {
                            try
                            {
                                finishedLine += $"={csvsplit[2]}";
                            }
                            catch (Exception)
                            {

                            }
                        }
                        break;
                }


                finishedLine += "\n";

                if (tlFile.Contains("h\\") && lineType == "Normal")
                {
                    try
                    {
                        hMasterDictionary.Add(csvsplit[0], csvsplit[1]);
                        hmaster.WriteLine($"{csvsplit[0]}={csvsplit[1]}");
                    }
                    catch (Exception)
                    {
                        
                    }
                }

                if (tlFile.Contains("communication\\") && lineType == "Normal")
                {
                    try
                    {
                        comMasterDictionary.Add(csvsplit[0], csvsplit[1]);
                        cmaster.WriteLine($"{csvsplit[0]}={csvsplit[1]}");
                    }
                    catch (Exception)
                    {

                    }
                }

                // TODO: Need to find a way to do this without endlessly opening the same file...

                var translationFile = Misc.WriterMake(tlFile, true);
                if (finishedLine != "")
                    translationFile.Write(finishedLine);
                translationFile.Close();
            }

            hmaster.Close();
            cmaster.Close();
            Console.Clear();
            Console.WriteLine("Successfully completed task!");
        }
    }
}
