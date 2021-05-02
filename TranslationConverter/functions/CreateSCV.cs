using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TranslationConverter.functions
{
    static class CreateSCV
    {
        public static void Runner(bool translationClean, string workingDir, string folderName)
        {
            var category = "";
            var charactertypes = new HashSet<string>();

            if (Directory.Exists($"{workingDir}\\abdata"))
                workingDir = $"{workingDir}\\abdata";

            foreach (var hTransFile in Directory.EnumerateFiles(workingDir, folderName + "\\*.txt",
                SearchOption.AllDirectories))
            {
                var charatype = "";

                if (hTransFile.Contains("adv\\"))
                    category = "adv";
                else if (hTransFile.Contains("h\\"))
                    category = "h";
                else if (hTransFile.Contains("communication\\"))
                    category = "communication";

                switch (category)
                {
                    case "adv":
                        {
                            var match = Regex.Match(hTransFile, @"adv\\\w+\\(c\d\d)\\");

                            if (match.Groups[1].Value != "")
                                charatype = match.Groups[1].Value;
                            break;
                        }
                    case "h":
                        {
                            var match = Regex.Match(hTransFile, @"h\\list\\.*personality_voice_(c\d\d)");

                            if (match.Groups[1].Value != "")
                                charatype = match.Groups[1].Value;
                            break;
                        }
                    case "communication":
                        {
                            var match = Regex.Match(hTransFile, @"communication_(\d\d)");

                            if (match.Groups[1].Value == "")
                                match = Regex.Match(hTransFile, @"optiondisplayitems_(\d\d)");

                            if (match.Groups[1].Value == "")
                                match = Regex.Match(hTransFile, @"communication_off_(\d\d)");

                            if (match.Groups[1].Value != "")
                                charatype = "c" + match.Groups[1].Value;
                            break;
                        }
                }

                if (charatype != "")
                    charactertypes.Add(charatype);
                //Console.WriteLine(charatype);
            }

            foreach (var selectedcharacter in charactertypes)
            {
                string CSVFolder = $@"{workingDir.Replace(@"\3translation\abdata", "")}\ExportedCSV";
                Console.WriteLine($"Dealing with {selectedcharacter} in {folderName}");
                var csvFileName = $"{CSVFolder}\\{selectedcharacter}.csv";
                var errorFileName = $"{CSVFolder}\\Errors.txt";
                var firstLine = false;

                Misc.CheckFolderEx(CSVFolder);

                if (!File.Exists(csvFileName))
                    firstLine = true;

                var outputfile = Misc.WriterMake(csvFileName, true);

                foreach (var hTransFile in Directory.EnumerateFiles(workingDir, "*.txt", SearchOption.AllDirectories))
                {
                    var newDoc = true;
                    var charatype = "";
                    var hTranslationFiles = File.ReadAllLines(hTransFile);

                    string filenameforex = hTransFile.Remove(0, workingDir.Length);

                    if (hTransFile.Contains("adv\\"))
                        category = "adv";
                    else if (hTransFile.Contains("h\\"))
                        category = "h";
                    else if (hTransFile.Contains("communication\\"))
                        category = "communication";

                    if (category != folderName) continue;
                    switch (category)
                    {
                        case "adv":
                            {
                                var match = Regex.Match(hTransFile, @"adv\\\w+\\(c\d\d)\\");

                                if (match.Groups[1].Value != "")
                                    charatype = match.Groups[1].Value;
                                break;
                            }
                        case "h":
                            {
                                var match = Regex.Match(hTransFile, @"h\\list\\.*personality_voice_(c\d\d)");

                                if (match.Groups[1].Value != "")
                                    charatype = match.Groups[1].Value;
                                break;
                            }
                        case "communication":
                            {
                                var match = Regex.Match(hTransFile, @"communication\\.*communication_(\d\d)");

                                if (match.Groups[1].Value == "")
                                    match = Regex.Match(hTransFile, @"communication\\.*optiondisplayitems_(\d\d)");

                                if (match.Groups[1].Value == "")
                                    match = Regex.Match(hTransFile, @"communication\\.*communication_off_(\d\d)");

                                if (match.Groups[1].Value != "")
                                    charatype = "c" + match.Groups[1].Value;
                                break;
                            }
                    }

                    bool steamAlternativeExists = File.Exists(hTransFile.Replace("abdata", "abdatasteam"));

                    if (charatype != selectedcharacter) continue;
                    try
                    {
                        {
                            foreach (var line in hTranslationFiles)
                            {
                                if (firstLine)
                                {
                                    outputfile.WriteLine(steamAlternativeExists
                                        ? "Original;Current Fantrans;Current Steamtrans;TL Note"
                                        : "Original;Current Fantrans;TL Note");

                                    firstLine = false;
                                }

                                if (newDoc)
                                {
                                    outputfile.WriteLine(steamAlternativeExists
                                        ? $";;\n;;\nabdata{filenameforex};;;"
                                        : $";;\n;;\nabdata{filenameforex};;");

                                    newDoc = false;
                                }

                                if (line != "" && line != " ")
                                {
                                    var splitLine = line.Replace(@"/", "").Split('=');
                                    bool steamHit = false;
                                    if (steamAlternativeExists && !translationClean)
                                    {
                                        string[] steamLines = File.ReadAllLines(hTransFile.Replace("abdata", "abdatasteam"));

                                        foreach (var steamLine in steamLines)
                                        {
                                            var splitSteamLine = steamLine.Replace(@"/", "").Split('=');
                                            if (splitSteamLine[0] != splitLine[0] && splitSteamLine[0] != "")
                                                continue;

                                            if (steamHit)
                                                continue;

                                            outputfile.WriteLine($"{splitLine[0]};{splitLine[1]};{splitSteamLine[1]};");
                                            steamHit = true;
                                        }
                                    }
                                    else
                                    {
                                        if (translationClean && splitLine.Length >= 2)
                                        {
                                            try
                                            {
                                                outputfile.WriteLine($"{splitLine[0]};{splitLine[1]};{splitLine[2]}");
                                            }
                                            catch (Exception)
                                            {
                                                outputfile.WriteLine($"{splitLine[0]};{splitLine[1]};");
                                            }
                                        }
                                        else if (translationClean)
                                            outputfile.WriteLine($"{splitLine[0]};{splitLine[1]};");
                                        else
                                        {
                                            outputfile.WriteLine(steamAlternativeExists
                                                ? $"{splitLine[0]};{splitLine[1]};;"
                                                : $"{splitLine[0]};{splitLine[1]};");
                                        }

                                    }

                                    if (!steamHit && steamAlternativeExists)
                                        outputfile.WriteLine($"{splitLine[0]};{splitLine[1]};;");

                                }
                                else
                                {
                                    outputfile.WriteLine(steamAlternativeExists ? ";;;" : ";;");
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Problem occurred in {hTransFile}!");
                        var errorfile = new StreamWriter(errorFileName, true);
                        errorfile.WriteLine($"Problem occurred in {hTransFile}!");
                        errorfile.Close();
                        Debug.WriteLine(e);
                    }
                }

                outputfile.Close();
            }
        }
    }
}
