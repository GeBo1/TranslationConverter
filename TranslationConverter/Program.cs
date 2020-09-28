using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace TranslationConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;

            // Checking that the input file is a csv 
            if (args.Length == 0 || args[0].Remove(0, args[0].Length - 4) != ".csv")
            {
                Console.WriteLine("Please drag a .csv file at this application to use");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            // Dealing with shortcuts
            try
            {
                switch (args[1])
                {
                    case "convertcsv":
                        CSVExtract(args);
                        Environment.Exit(0);
                        break;
                    case "csv":
                        MakeCSV();
                        Environment.Exit(0);
                        break;
                    case "cleanup":
                        CleanupTranslation();
                        Environment.Exit(0);
                        break;
                    case "fuckit":
                        ReadXUA("abdata", "adv");
                        ReadXUA("abdata", "communication");
                        ReadXUA("abdata", "h");
                        Environment.Exit(0);
                        break;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(0);
            }

            // Splitting the csv to txt XUA translations
            CSVExtract(args);

            // Dupe filling

            FillDupes();

            // Running Cleanup

            CleanupTranslation();

            // aaaaand back to csv ;-)

        }

        private static void CSVExtract(string[] args)
        {
            string csvfile = args[0];
            string TLFile = "";
            string TLPath;
            string[] lines = System.IO.File.ReadAllLines(csvfile);
            string LineType;
            string FinishedLine = "";
            bool TranslationComment;

            if (Directory.Exists("1translation"))
                Directory.Delete("1translation", true);

            foreach (string line in lines)
            {
                string[] csvsplit = line.Split(';');

                // Deciding what we're dealing with
                if (csvsplit[0].Contains("adv\\"))
                    LineType = "ADV";
                else if (csvsplit[0].Contains("h\\"))
                    LineType = "H";
                else if (csvsplit[0].Contains("communication\\"))
                    LineType = "COMMUNICATION";
                else if (csvsplit[0] == "")
                    LineType = "BlankLine";
                else
                    LineType = "Normal";

                // Generalizing linetype
                TranslationComment = (csvsplit[2] != "") ? TranslationComment = true : TranslationComment = false;
                if (LineType == "ADV" || LineType == "H" || LineType == "COMMUNICATION")
                    LineType = "FileName";


                switch (LineType)
                {
                    case "FileName": // Changing directory, makes sure it exists and writes comments to external file for preservation if they exist
                        TLFile = $"1translation\\{csvsplit[0]}";
                        TLPath = TLFile.Remove(TLFile.Length - 15, 15);
                        if (!Directory.Exists(TLPath))
                            Directory.CreateDirectory(TLPath);
                        FinishedLine = "";
                        break;
                    case "BlankLine":
                        FinishedLine = "\n";
                        break;
                    case "Normal":
                        FinishedLine = $"{csvsplit[0]}={csvsplit[1]}\n";
                        break;
                    default:
                        break;
                }

                // TODO: Need to find a way to do this without endlessly opening the same file...

                using StreamWriter TranslationFile = new StreamWriter(TLFile, true);
                if (FinishedLine != "")
                    TranslationFile.Write(FinishedLine);
                TranslationFile.Close();
            }
        }

        public static void ReadXUA(string WorkingDir, string FolderName)
        {
            int serial = 0;
            string category = "";
            HashSet<string> charactertypes = new HashSet<string>();
            Dictionary<string, int> LineList = new Dictionary<string, int>();

            foreach (var HTransFile in Directory.EnumerateFiles(WorkingDir, FolderName + "\\*.txt", SearchOption.AllDirectories))
            {
                string charatype = "";

                if (HTransFile.Contains("adv\\"))
                    category = "adv";
                else if (HTransFile.Contains("h\\"))
                    category = "h";
                else if (HTransFile.Contains("communication\\"))
                    category = "communication";

                if (category == "adv")
                {
                    var match = Regex.Match(HTransFile, @"adv\\\w+\\(c\d\d)\\");

                    if (match.Groups[1].Value != "")
                        charatype = match.Groups[1].Value;
                }
                if (category == "h")
                {
                    var match = Regex.Match(HTransFile, @"h\\list\\.*personality_voice_(c\d\d)");

                    if (match.Groups[1].Value != "")
                        charatype = match.Groups[1].Value;
                }
                if (category == "communication")
                {
                    var match = Regex.Match(HTransFile, @"communication\\.*communication_(\d\d)");

                    if (match.Groups[1].Value != "")
                        charatype = "c" + match.Groups[1].Value;
                }
                if (charatype != "")
                {
                    charactertypes.Add(charatype);
                    //Console.WriteLine(charatype);
                }
            }

            foreach(string selectedcharacter in charactertypes)
            {
                Console.WriteLine($"Dealing with {selectedcharacter} in {FolderName}");
                string CSVFileName = $"CSVFiles\\{selectedcharacter}.csv";
                bool FirstLine = false;

                if (!Directory.Exists("CSVFiles"))
                    Directory.CreateDirectory("CSVFiles");

                if (!File.Exists(CSVFileName))
                    FirstLine = true;

                StreamWriter outputfile = new StreamWriter(CSVFileName, true);

                foreach (string HTransFile in Directory.EnumerateFiles(WorkingDir, $"*.txt", SearchOption.AllDirectories))
                {
                    bool newDoc = true;
                    string charatype = "";
                    string[] HTranslationFiles = File.ReadAllLines(HTransFile);

                    if (HTransFile.Contains("adv\\"))
                        category = "adv";
                    else if (HTransFile.Contains("h\\"))
                        category = "h";
                    else if (HTransFile.Contains("communication\\"))
                        category = "communication";

                    if (category == FolderName)
                    {

                        if (category == "adv")
                        {
                            var match = Regex.Match(HTransFile, @"adv\\\w+\\(c\d\d)\\");

                            if (match.Groups[1].Value != "")
                                charatype = match.Groups[1].Value;
                        }
                        if (category == "h")
                        {
                            var match = Regex.Match(HTransFile, @"h\\list\\.*personality_voice_(c\d\d)");

                            if (match.Groups[1].Value != "")
                                charatype = match.Groups[1].Value;
                        }
                        if (category == "communication")
                        {
                            var match = Regex.Match(HTransFile, @"communication\\.*communication_(\d\d)");

                            if (match.Groups[1].Value != "")
                                charatype = "c" + match.Groups[1].Value;
                        }

                        //if (charatype != "")
                        //{
                        //    Console.WriteLine(charatype);
                        //    Console.WriteLine(HTransFile);
                        //    Console.WriteLine("--------------------");
                        //}

                        if (charatype == selectedcharacter)
                        {



                            try
                            {
                                {
                                    foreach (string line in HTranslationFiles)
                                    {
                                        serial++;
                                        if (FirstLine)
                                        {
                                            outputfile.WriteLine("Original (JP);Current Fantrans (EN);TL Note;LineID");
                                            FirstLine = false;
                                        }

                                        if (newDoc)
                                        {
                                            outputfile.WriteLine($";;;\n;;;\n{HTransFile};;;");
                                            newDoc = false;
                                        }

                                        if (line != "")
                                        {
                                            string[] splitLine = line.Split('=');
                                            if(!LineList.ContainsKey(splitLine[0].Replace(@"/", "")))
                                                LineList.Add(splitLine[0].Replace(@"/", ""), serial);

                                            int FinalNumber = 0;

                                            foreach (KeyValuePair<string, int> test in LineList)
                                            {
                                                //Console.WriteLine(test.Key);
                                                if (!LineList.ContainsKey(line.Replace(@"/", "")))
                                                {
                                                    FinalNumber = test.Value;
                                                }
                                            }
                                            outputfile.WriteLine($"{splitLine[0].Replace(@"/", "")};{splitLine[1]};;{FinalNumber}");
                                        }
                                        else
                                        {
                                            outputfile.WriteLine($";;;");
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Problem occurred in {HTransFile}!");
                                System.Diagnostics.Debug.WriteLine(e);
                            }

                        }
                    }

                }
                outputfile.Close();
            }
        }

        private static void FillDupes()
        {
            string[] master = System.IO.File.ReadAllLines("master.txt");
            string prevStepTL = "1translation\\h";
            var filenumber = 0;

            List<string> CopyDirs = new List<string>
            {
                "adv",
                "communication"
            };

            foreach (string item in CopyDirs)
            {
                string SourcePath = $"1translation\\{item}";
                string DestinationPath = $"2translation\\{item}";

                // Creating directories
                foreach (string dirPath in Directory.GetDirectories(SourcePath, "*",
                    SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));

                // Copying files, replacing duplicates
                foreach (string newPath in Directory.GetFiles(SourcePath, "*.*",
                    SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
            }

            foreach (string HTransFile in Directory.EnumerateFiles(prevStepTL, "*.txt", SearchOption.AllDirectories))
            {
                string[] HTranslationFiles = File.ReadAllLines(HTransFile); // Filename obtained
                string outputdir = HTransFile.Replace("1translation", "2translation").Remove(HTransFile.Length - 15, 15);
                filenumber++;
                Console.Clear();
                Console.WriteLine("Working on file " + filenumber + " out of " + HTransFile.Length + "...");

                if (!Directory.Exists(outputdir))
                    Directory.CreateDirectory(outputdir);

                StreamWriter file = new StreamWriter($"{outputdir}\\translation.txt", true);

                foreach (string oldLine in HTranslationFiles)
                {
                    string[] splitOldLine = oldLine.Split('='); // splitOldLine[0] for comparison
                    bool hit = false;

                    foreach (string masterLine in master)
                    {
                        string[] splitMasterLine = masterLine.Split('='); // splitMasterLine[0] for comparison


                        if (splitOldLine[0] == splitMasterLine[0] && !hit)
                        {
                            file.Write(splitMasterLine[0] + "=" + splitMasterLine[1] + "\n");
                            hit = true;
                        }
                    }

                    if (splitOldLine[0] == "")
                        file.Write("\n");
                    else if (!hit)
                    {
                        try
                        {
                            file.Write(splitOldLine[0] + "=" + splitOldLine[1] + "\n");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }

                }

                file.Close();
            }
        }

        private static void CleanupTranslation()
        {
            string sDir = "2translation";

            if (Directory.Exists("3translation"))
                Directory.Delete("3translation", true);

            foreach (string TranslationFile in Directory.EnumerateFiles(sDir, "*.txt", SearchOption.AllDirectories))
            {
                string[] TranslationLine = File.ReadAllLines(TranslationFile);
                string TranslationOutputDir = "3" + TranslationFile.Remove(TranslationFile.Length - 15, 15).Remove(0, 1);
                string TranslationOutputFile = TranslationOutputDir + "translation.txt";

                Console.WriteLine(TranslationOutputFile);
                foreach (string TranslationSentence in TranslationLine)
                {
                    string[] splittrans = TranslationSentence.Split('=');
                    Directory.CreateDirectory(TranslationOutputDir);
                    try
                    {
                        splittrans[1] = RunFix(splittrans[1]);
                        string donetrans = splittrans[0] + "=" + splittrans[1];

                        using StreamWriter hit = new StreamWriter(TranslationOutputFile, true);
                        hit.WriteLine(splittrans[0] + "=" + splittrans[1]);
                        hit.Close();
                    }
                    catch (Exception err)
                    {
                        Console.Out.WriteLine(err);
                    }
                }
            }
        }

        private static void MakeCSV()
        {
            string workDir = "2translation";
            File.Delete("newTranslation.csv");
            foreach (string HTransFile in Directory.EnumerateFiles(workDir + "\\adv", "*.txt", SearchOption.AllDirectories))
            {
                string[] HTranslationFiles = File.ReadAllLines(HTransFile);
                StreamWriter file = new StreamWriter($"newTranslation.csv", true);

                file.Write(HTransFile.Remove(0, 13) + "\n");
                Console.WriteLine(HTransFile + ";;");

                foreach (string line in HTranslationFiles)
                {
                    string[] exportline = line.Split('=');
                    if (exportline[0] == "")
                        file.Write("\n");
                    else
                        file.Write(exportline[0] + ";" + exportline[1] + ";" + exportline[2] + "\n");
                }
                file.Write("\n\n");
                file.Close();
            }
            foreach (string HTransFile in Directory.EnumerateFiles(workDir + "\\communication", "*.txt", SearchOption.AllDirectories))
            {
                string[] HTranslationFiles = File.ReadAllLines(HTransFile);
                StreamWriter file = new StreamWriter($"newTranslation.csv", true);

                file.Write(HTransFile.Remove(0, 13) + "\n");
                Console.WriteLine(HTransFile + ";;");

                foreach (string line in HTranslationFiles)
                {
                    string[] exportline = line.Split('=');
                    if (exportline[0] == "")
                        file.Write("\n");
                    else
                        file.Write(exportline[0] + ";" + exportline[1] + ";" + exportline[2] + "\n");
                }
                file.Write("\n\n");
                file.Close();
            }
            foreach (string HTransFile in Directory.EnumerateFiles(workDir + "\\h", "*.txt", SearchOption.AllDirectories))
            {
                string[] HTranslationFiles = File.ReadAllLines(HTransFile);
                StreamWriter file = new StreamWriter($"newTranslation.csv", true);

                file.Write(HTransFile.Remove(0, 13) + "\n");
                Console.WriteLine(HTransFile + ";;");

                foreach (string line in HTranslationFiles)
                {
                    string[] exportline = line.Split('=');
                    if (exportline[0] == "")
                        file.Write("\n");
                    else if (exportline[2] != "")
                        file.Write(exportline[0] + ";" + exportline[1] + ";" + exportline[2] + "\n");
                    else
                        file.Write(exportline[0] + ";" + exportline[1] + ";" + "\n");
                }
                file.Write(";;;\n;;;\n");
                file.Close();
            }
        }

        public static string RunFix(string translation)
        {
            string replacer = translation;
            replacer = replacer.Replace("…", "...");
            //replacer = replacer.Replace("..", "...");
            //replacer = replacer.Replace("....", "...");

            replacer = Regex.Replace(replacer, @"(^|[^\.])(\.{3})+(\.{1,2})([^\.]|$)", "$1$2$4");
            replacer = Regex.Replace(replacer, @"[\s](?<!\.)(\.{3})+(?!\.)([\S]|$)", "$2$3 $4");
            replacer = Regex.Replace(replacer, @"(^|[^\.])(\.{2})([^\.]|$)", "$1$2.$3");
            replacer = Regex.Replace(replacer, "[ ]{2,}", " ");

            return replacer.TrimEnd();
        }
    }
}
