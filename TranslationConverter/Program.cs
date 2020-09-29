using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace TranslationConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            string inputfile = "";

            // Checking that the input file is a csv 
            if (args.Length == 0 || args[0].Remove(0, args[0].Length - 4) == ".csv")
            {
                Console.WriteLine("You've passed a csv file!");
                inputfile = "csv";
            }
            else if (args.Length == 0 || args[0].Remove(0, args[0].Length - 1) == "//")
            {
                Console.WriteLine("You've passed a folder!");
                inputfile = "folder";
            }
            else
            {
                Console.WriteLine("You haven't passed any arguments!");
                //Environment.Exit(0);
            }

            // Offering Menu
            Restart:
            Console.WriteLine("\n~~~~~~~~~~~~~~~~~~~BetterRepack Translation Manipulation~~~~~~~~~~~~~~~~~~\n");
            Console.WriteLine("Welcome to the translation manipulation protocol, soldier! Choose something and get on with it! =P\n");
            Console.WriteLine("1. Convert csv to XUA compatible txts");
            Console.WriteLine("2. Check and populate duplicate h lines");
            Console.WriteLine("3. Cleanup translation for release");
            Console.WriteLine("4. Convert XUA folder to CSV");
            Console.WriteLine("5. Turn h duplicate checked folder back into CSV");
            Console.WriteLine("6. Turn cleanuped folder back into CSV");
            Console.WriteLine("66. Create dupechecked and cleaned CSVs");
            Console.WriteLine("Q. Exit");
            Console.WriteLine("");
            Console.Write("Please enter your choice: ");

            var MenuChoice = Console.ReadLine().ToLower();

            Console.WriteLine(MenuChoice.ToUpper());
            Console.Clear();

            // Launching part based on user input

            switch (MenuChoice)
            {
                case "1":
                    if (inputfile == "csv")
                        ReadCSV(args[0]);
                    else if (File.Exists("csvfile.csv"))
                        ReadCSV("csvfile.csv");
                    else
                        Console.WriteLine("You didn't pass an csv file!");
                    goto Restart;
                case "2":
                    if (Directory.Exists("1translation"))
                        FillHDupes();
                    else
                        Console.WriteLine("You need to generate the XUA folder from a CSV first!");
                    goto Restart;
                case "3":
                    if (Directory.Exists("2translation"))
                        CleanupTranslation("2translation");
                    else if (Directory.Exists("1translation"))
                        CleanupTranslation("1translation");
                    else
                        Console.WriteLine("You need to generate the XUA folder from a CSV first!");
                    goto Restart;
                case "4":
                    if (inputfile == "folder" && Directory.Exists(args[0]))
                    {
                        ReadXUA(args[0], "adv");
                        ReadXUA(args[0], "communication");
                        ReadXUA(args[0], "h");
                        Console.Clear();
                    }
                    else if (Directory.Exists("abdata"))
                    {
                        ReadXUA("abdata", "adv");
                        ReadXUA("abdata", "communication");
                        ReadXUA("abdata", "h");
                        Console.Clear();
                    }
                    else
                        Console.WriteLine("You need to pass a folder!");
                    goto Restart;
                case "5":
                    MakeCSV("2translation");
                    Console.Clear();
                    goto Restart;
                case "6":
                    MakeCSV("3translation");
                    Console.Clear();
                    goto Restart;
                case "66":
                    JustAQuicky("csvfile.csv","Treated");
                    goto Restart;
                case "q":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid key");
                    goto Restart;
            }

        }

        private static void JustAQuicky(string InputFile, string OutputFile)
        {
            string[] lines = System.IO.File.ReadAllLines(InputFile);
            string FinalLine = "";
            bool IsInH = false;

            if (File.Exists("master.txt"))
                File.Delete("master.txt");
            if (File.Exists($"{OutputFile}_DupeChecked.csv"))
                File.Delete($"{OutputFile}_DupeChecked.csv");
            if (File.Exists($"{OutputFile}_Cleaned.csv"))
                File.Delete($"{OutputFile}_Cleaned.csv");

            StreamWriter HMaster = new StreamWriter("master.txt", true);
            StreamWriter EndFile = new StreamWriter($"{OutputFile}_DupeChecked.csv", true);
            StreamWriter EndFileClean = new StreamWriter($"{OutputFile}_Cleaned.csv", true);

            foreach (string line in lines)
            {
                string[] csvsplit = line.Split(';');

                if (line.Contains("h\\") && csvsplit[0] != "")
                    IsInH = true;
                else if ((line.Contains("adv\\") || line.Contains("communication\\")) && csvsplit[0] != "")
                    IsInH = false;
                else if (IsInH)
                    HMaster.WriteLine($"{csvsplit[0]};{csvsplit[1]}");
            }
            HMaster.Close();

            string[] master = File.ReadAllLines("master.txt");
            IsInH = false;
            foreach (string line in lines)
            {
                string[] csvsplit = line.Split(';');
                bool hit = false;
                if (line.Contains("h\\") && csvsplit[0] != "")
                    IsInH = true;
                else if ((line.Contains("adv\\") || line.Contains("communication\\")) && csvsplit[0] != "")
                    IsInH = false;

                if(!IsInH)
                {
                    FinalLine = line;
                }
                else
                {
                    foreach (string HLine in master)
                    {
                        string[] HLineSplit = HLine.Split(';');
                        if (csvsplit[0].Contains("h\\"))
                            FinalLine = line;
                        if (HLineSplit[0] == csvsplit[0] && !hit)
                        {
                            FinalLine = $"{csvsplit[0]};{HLineSplit[1]};{csvsplit[2]}";
                            hit = true;
                        }
                    }
                }

                EndFile.WriteLine(FinalLine);
                
            }
            EndFile.Close();

            string[] DupeCheckedLines = System.IO.File.ReadAllLines($"{OutputFile}_DupeChecked.csv");

            foreach (string line in DupeCheckedLines)
            {
                string[] splittrans = line.Split(';');
                try
                {
                    splittrans[1] = RunFix(splittrans[1]);
                    string donetrans = $"{splittrans[0]};{splittrans[1]};{splittrans[2]}";

                    EndFileClean.WriteLine(donetrans);
                }
                catch (Exception err)
                {
                    Console.Out.WriteLine(err);
                }
            }
            EndFileClean.Close();
        }

        private static void ReadCSV(string InputFile)
        {
            string csvfile = InputFile;
            string TLFile = "";
            string TLPath;
            string[] lines = System.IO.File.ReadAllLines(csvfile);
            string LineType = "";
            string FinishedLine = "";
            bool TranslationComment;

            if (Directory.Exists("1translation"))
                Directory.Delete("1translation", true);

            if (File.Exists("master.txt"))
                File.Delete("master.txt");

            StreamWriter Hmaster = new StreamWriter("master.txt", true);

            foreach (string line in lines)
            {
                string[] csvsplit = line.Split(';');

                string WhereAmI = LineType;

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

                if (TLFile == "" && LineType != "FileName")
                    continue;

                switch (LineType)
                {
                    case "FileName": // Changing directory, makes sure it exists and writes comments to external file for preservation if they exist
                        TLFile = $"1translation\\{csvsplit[0]}";
                        if (!TLFile.Contains("abdata"))
                            TLFile = $"1translation\\abdata\\{csvsplit[0]}";
                        TLPath = TLFile.Remove(TLFile.Length - 15, 15);
                        if (!Directory.Exists(TLPath))
                            Directory.CreateDirectory(TLPath);
                        FinishedLine = "";
                        Console.WriteLine("Writing to " + TLFile);
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

                if (TLFile.Contains("h\\") && LineType == "Normal")
                {
                    Hmaster.WriteLine($"{csvsplit[0]}={csvsplit[1]}");
                }

                // TODO: Need to find a way to do this without endlessly opening the same file...

                using StreamWriter TranslationFile = new StreamWriter(TLFile, true);
                if (FinishedLine != "")
                    TranslationFile.Write(FinishedLine);
                TranslationFile.Close();
            }
            Hmaster.Close();
            Console.Clear();
        }

        public static void ReadXUA(string WorkingDir, string FolderName)
        {
            string category = "";
            HashSet<string> charactertypes = new HashSet<string>();

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

            foreach (string selectedcharacter in charactertypes)
            {
                Console.WriteLine($"Dealing with {selectedcharacter} in {FolderName}");
                string CSVFileName = $"CSVFiles\\{selectedcharacter}.csv";
                string ErrorFileName = $"CSVFiles\\Errors.txt";
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

                        if (charatype == selectedcharacter)
                        {



                            try
                            {
                                {
                                    foreach (string line in HTranslationFiles)
                                    {
                                        if (FirstLine)
                                        {
                                            outputfile.WriteLine("Original (JP);Current Fantrans (EN);TL Note");
                                            FirstLine = false;
                                        }

                                        if (newDoc)
                                        {
                                            outputfile.WriteLine($";;\n;;\n{HTransFile};;");
                                            newDoc = false;
                                        }

                                        if (line != "" && line != " ")
                                        {
                                            string[] splitLine = line.Replace(@"/", "").Split('=');
                                            outputfile.WriteLine($"{splitLine[0]};{splitLine[1]};");
                                        }
                                        else
                                        {
                                            outputfile.WriteLine($";;");
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Problem occurred in {HTransFile}!");
                                StreamWriter errorfile = new StreamWriter(ErrorFileName, true);
                                errorfile.WriteLine($"Problem occurred in {HTransFile}!");
                                errorfile.Close();
                                System.Diagnostics.Debug.WriteLine(e);
                            }

                        }
                    }

                }
                outputfile.Close();
            }
        }

        private static void FillHDupes()
        {
            string[] master = File.ReadAllLines("master.txt");
            string prevStepTL = "1translation\\abdata\\h";
            var filenumber = 0;

            List<string> CopyDirs = new List<string>
            {
                "adv",
                "communication"
            };

            foreach (string item in CopyDirs)
            {
                string SourcePath = $"1translation\\abdata\\{item}";
                string DestinationPath = $"2translation\\abdata\\{item}";

                // Creating directories
                foreach (string dirPath in Directory.GetDirectories(SourcePath, "*.*",
                    SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(SourcePath, DestinationPath));

                // Copying files, replacing duplicates
                foreach (string newPath in Directory.GetFiles(SourcePath, "*.txt",
                    SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(SourcePath, DestinationPath), true);
            }

            foreach (string HTransFile in Directory.EnumerateFiles(prevStepTL, "*.txt", SearchOption.AllDirectories))
            {
                string[] HTranslationFiles = File.ReadAllLines(HTransFile); // Filename obtained
                string outputdir = HTransFile.Replace("1translation", "2translation").Remove(HTransFile.Length - 15, 15);
                filenumber++;
                Console.Clear();
                Console.WriteLine("Working on file " + filenumber + " out of " + HTranslationFiles.Length + "...");

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

        private static void CleanupTranslation(string OldDir)
        {
            string sDir = OldDir;

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
            Console.Clear();
        }

        private static void MakeCSV( string WorkingFolder)
        {
            string workDir = WorkingFolder;
            File.Delete("newTranslation.csv");
            foreach (string HTransFile in Directory.EnumerateFiles(workDir + "\\abdata\\adv", "*.txt", SearchOption.AllDirectories))
            {
                string[] HTranslationFiles = File.ReadAllLines(HTransFile);
                StreamWriter file = new StreamWriter($"newTranslation.csv", true);

                file.Write(HTransFile.Remove(0, 13) + "\n");
                Console.WriteLine(HTransFile + ";;");

                foreach (string line in HTranslationFiles)
                {
                    string[] exportline = line.Split('=');
                    if (exportline[0] == "")
                        file.Write(";;\n");
                    else
                        file.Write(exportline[0] + ";" + exportline[1] + ";" + "\n");
                }
                file.Write(";;\n;;\n");
                file.Close();
            }
            foreach (string HTransFile in Directory.EnumerateFiles(workDir + "\\abdata\\communication", "*.txt", SearchOption.AllDirectories))
            {
                string[] HTranslationFiles = File.ReadAllLines(HTransFile);
                StreamWriter file = new StreamWriter($"newTranslation.csv", true);

                file.Write(HTransFile.Remove(0, 13) + "\n");
                Console.WriteLine(HTransFile + ";;");

                foreach (string line in HTranslationFiles)
                {
                    string[] exportline = line.Split('=');
                    if (exportline[0] == "")
                        file.Write(";;\n");
                    else
                        file.Write(exportline[0] + ";" + exportline[1] + ";" + "\n");
                }
                file.Write(";;\n;;\n");
                file.Close();
            }
            foreach (string HTransFile in Directory.EnumerateFiles(workDir + "\\abdata\\h", "*.txt", SearchOption.AllDirectories))
            {
                string[] HTranslationFiles = File.ReadAllLines(HTransFile);
                StreamWriter file = new StreamWriter($"newTranslation.csv", true);

                file.Write(HTransFile.Remove(0, 13) + "\n");
                Console.WriteLine(HTransFile + ";;");

                foreach (string line in HTranslationFiles)
                {
                    string[] exportline = line.Split('=');
                    if (exportline[0] == "")
                        file.Write(";;\n");
                    else
                        file.Write(exportline[0] + ";" + exportline[1] + ";" + "\n");
                }
                file.Write(";;\n;;\n");
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
