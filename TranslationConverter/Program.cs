using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;

namespace TranslationConverter
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            var inputfile = "";

            // Checking that the input file is a csv 
            if (args.Length == 0)
            {

            }
            else if (args[0].Remove(0, args[0].Length - 4) == ".csv")
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
            Console.WriteLine(
                "Welcome to the translation manipulation protocol, soldier! Choose something and get on with it! =P\n");
            Console.WriteLine("1. Convert csv to XUA compatible txts");
            Console.WriteLine("2. Check and populate duplicate h lines");
            Console.WriteLine("3. Cleanup translation for release");
            Console.WriteLine("4. Convert XUA folder to CSV");
            Console.WriteLine("5. Turn h duplicate checked folder back into CSV");
            Console.WriteLine("6. Turn cleanuped folder back into CSV");
            Console.WriteLine("7. Remove Steam comparison from CSV");
            Console.WriteLine("66. Create dupechecked and cleaned CSVs");
            Console.WriteLine("Q. Exit");
            Console.WriteLine("");
            Console.Write("Please enter your choice: ");

            var menuChoice = Console.ReadLine().ToLower();

            Console.WriteLine(menuChoice.ToUpper());
            Console.Clear();

            // Launching part based on user input

            switch (menuChoice)
            {
                case "1":
                    if (inputfile == "csv")
                        ReadCsv(args[0]);
                    else if (File.Exists("csvfile.csv"))
                        ReadCsv("csvfile.csv");
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
                        ReadXua(args[0], "adv");
                        ReadXua(args[0], "communication");
                        ReadXua(args[0], "h");
                        Console.Clear();
                    }
                    else if (Directory.Exists("abdata"))
                    {
                        ReadXua("abdata", "adv");
                        ReadXua("abdata", "communication");
                        ReadXua("abdata", "h");
                        Console.Clear();
                    }
                    else
                    {
                        Console.WriteLine("You need to pass a folder!");
                    }

                    goto Restart;
                case "5":
                    MakeCsv("2translation");
                    Console.Clear();
                    goto Restart;
                case "6":
                    MakeCsv("3translation");
                    Console.Clear();
                    goto Restart;
                case "7":
                    if (inputfile == "csv")
                        RemoveSteamComparison(args[0], "SteamCleaned.csv");
                    else
                        Console.WriteLine("You didn't pass an csv file!");
                    goto Restart;
                case "66":
                    JustAQuicky("csvfile.csv", "Treated");
                    goto Restart;
                case "67":
                    MissingEnding("Treated_Cleaned.csv", "MissingEnding.csv");
                    goto Restart;
                case "99":
                    AppendToAbdata();
                    goto Restart;
                case "q":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid key");
                    goto Restart;
            }
        }

        private static void AppendToAbdata()
        {
            if (!Directory.Exists("3translation"))
            {
                return;
            }

            foreach (var hTransFile in Directory.EnumerateFiles("3translation", "*.txt",
                SearchOption.AllDirectories))
            {
                var outputDir = hTransFile.Replace("3translation", "4translation");
                var currentoriginal = hTransFile.Replace("3translation\\", "");
                var tl3file = File.ReadAllLines(hTransFile);
                var originalfile = File.ReadAllLines(currentoriginal);


                var tlPath = outputDir.Remove(outputDir.Length - 15, 15);

                if (!Directory.Exists(tlPath))
                    Directory.CreateDirectory(tlPath);

                var outputfile = new StreamWriter(tlPath + "translation.txt");

                foreach (var currentLine in originalfile)
                {
                    bool match = false;

                    var currentLineSplit = currentLine.Replace("//","").Split("=");

                    foreach (var ofileLine in tl3file)
                    {
                        var ofileLineSplit = ofileLine.Replace("//", "").Split("=");
                        if (ofileLineSplit[0] == currentLineSplit[0])
                            match = true;

                        if (match)
                        {
                            outputfile.WriteLine(ofileLine);
                            break;
                        }
                    }

                    if (!match)
                    {
                        outputfile.WriteLine(currentLine);
                    }
                }
                outputfile.Close();
            }

            //var lines = File.ReadAllLines(inputFile);
            //var linecount = 0;

                //foreach (var line in lines)
                //{
                //    var csvsplit = line.Split(';');
                //    string currentOriginal = "shouldn't happen";

                //    if (csvsplit[0].Contains(".txt"))
                //    {
                //        currentOriginal = csvsplit[0];
                //    }
                //    else if (csvsplit[1] == "")
                //    {

                //    }
                //    else
                //    {
                //        replacer = Regex.Replace(replacer, "[ ]{2,}", " ");
                //    }
                //}
        }

        private static void MakeSerialRegister()
        {
            var serialRegister = "SerialReg.txt";
            int serial = 0;

            // Deleting existing serialRegister
            if(File.Exists(serialRegister))
                File.Delete(serialRegister);

            var serialWorker = new StreamWriter(serialRegister, false);

            // Deleting other stuffs
            if(Directory.Exists("translation1"))
                Directory.Delete("translation1");
            if (Directory.Exists("translation2"))
                Directory.Delete("translation2");
            if (Directory.Exists("3translation"))
                Directory.Delete("3translation");

            // Reading the results, and producing new register
            foreach (var hTransFile in Directory.EnumerateFiles("abdata", "*.txt",
                SearchOption.AllDirectories))
            {
                var hTranslationFiles = File.ReadAllLines(hTransFile);
                foreach (var line in hTranslationFiles)
                {
                    var exportline = line.Split('=');
                    if (exportline[0] != "")
                    {
                        serial++;
                        serialWorker.WriteLine(serial + ";" + exportline[0]);
                    }
                }
            }
            serialWorker.Close();
        }

        private static void RemoveSteamComparison(string inputFile, string outputFile)
        {
            var lines = File.ReadAllLines(inputFile);
            if (lines[0].Contains("Current Steamtrans"))
            {
                StreamWriter writer = new StreamWriter(outputFile, false);
                foreach (string line in lines)
                {
                    string[] lineSplit = line.Split(';');
                    writer.WriteLine($"{lineSplit[0]};{lineSplit[1]};{lineSplit[3]}");
                }
                writer.Close();
            }
        }

        private static void MissingEnding(string inputFile, string outputFile)
        {
            var lines = File.ReadAllLines(inputFile);
            int number = 0;

            StreamWriter writer = new StreamWriter(outputFile, false);
            foreach (string line in lines)
            {
                string[] splitLine = line.Split(';');
                number++;
                bool isActualLine = splitLine[1] != "";
                bool missingEnd = false;

                if(!isActualLine)
                {
                    writer.WriteLine($"{line};");
                    continue;
                }

                string theThingIReallyCareAbout = splitLine[1].Substring(splitLine[1].Length - 1);
                string theThingIReallyCareAbout2 = splitLine[1].Substring(0,1);
                string lineCut = splitLine[1].Remove(0, splitLine[1].Length - 1);
                if (splitLine[1] != "Pastor" && splitLine[1] != "Mob" && theThingIReallyCareAbout != "♪" &&
                    theThingIReallyCareAbout != "]" && theThingIReallyCareAbout != "*" &&
                    theThingIReallyCareAbout != "』" && theThingIReallyCareAbout != "-" &&
                    theThingIReallyCareAbout != "~" && theThingIReallyCareAbout != "\"" &&
                    theThingIReallyCareAbout != "`" && theThingIReallyCareAbout != ")" &&
                    theThingIReallyCareAbout != "." && theThingIReallyCareAbout != "!" &&
                    theThingIReallyCareAbout != "?" && (!line.Contains(".txt") && !line.Contains("Current Fantrans")))
                {
                    writer.WriteLine($"{splitLine[0]};{splitLine[1]};{splitLine[2]};Punctuation");
                }
                else if (theThingIReallyCareAbout2 == "(")
                {

                    writer.WriteLine($"{splitLine[0]};{splitLine[1]};{splitLine[2]};Caution");
                }
                else
                {
                    writer.WriteLine($"{splitLine[0]};{splitLine[1]};{splitLine[2]};");
                }
            }
            writer.Close();
        }

        private static void JustAQuicky(string inputFile, string outputFile)
        {
            var lines = File.ReadAllLines(inputFile);
            var finalLine = "";
            var isInH = false;

            if (File.Exists("master.txt"))
                File.Delete("master.txt");
            if (File.Exists($"{outputFile}_DupeChecked.csv"))
                File.Delete($"{outputFile}_DupeChecked.csv");
            if (File.Exists($"{outputFile}_Cleaned.csv"))
                File.Delete($"{outputFile}_Cleaned.csv");

            var hMaster = new StreamWriter("master.txt", false);
            var endFile = new StreamWriter($"{outputFile}_DupeChecked.csv", false);
            var endFileClean = new StreamWriter($"{outputFile}_Cleaned.csv", false);

            foreach (var line in lines)
            {
                var csvsplit = line.Split(';');

                if (line.Contains("h\\") && csvsplit[0] != "")
                    isInH = true;
                else if ((line.Contains("adv\\") || line.Contains("communication\\")) && csvsplit[0] != "")
                    isInH = false;
                else if (isInH)
                    hMaster.WriteLine($"{csvsplit[0]};{csvsplit[1]}");
            }

            hMaster.Close();

            var master = File.ReadAllLines("master.txt");
            isInH = false;
            foreach (var line in lines)
            {
                var csvsplit = line.Split(';');
                var hit = false;
                if (line.Contains("h\\") && csvsplit[0] != "")
                    isInH = true;
                else if ((line.Contains("adv\\") || line.Contains("communication\\")) && csvsplit[0] != "")
                    isInH = false;

                if (!isInH)
                    finalLine = line;
                else
                    foreach (var hLine in master)
                    {
                        var hLineSplit = hLine.Split(';');
                        if (csvsplit[0].Contains("h\\"))
                            finalLine = line;
                        if (hLineSplit[0] != csvsplit[0] || hit) continue;
                        finalLine = $"{csvsplit[0]};{hLineSplit[1]};{csvsplit[2]}";
                        hit = true;
                    }

                endFile.WriteLine(finalLine);
            }

            endFile.Close();

            var dupeCheckedLines = File.ReadAllLines($"{outputFile}_DupeChecked.csv");

            foreach (var line in dupeCheckedLines)
            {
                var splittrans = line.Split(';');
                try
                {
                    if(splittrans[1].Length >= 1)
                        splittrans[1] = RunFix(splittrans[1]);
                    var donetrans = $"{splittrans[0]};{splittrans[1]};{splittrans[2]}";

                    endFileClean.WriteLine(donetrans);
                }
                catch (Exception err)
                {
                    Console.Out.WriteLine(err);
                }
            }

            endFileClean.Close();
        }

        private static void ReadCsv(string inputFile)
        {
            var csvfile = inputFile;
            var tlFile = "";
            var lines = File.ReadAllLines(csvfile);
            var lineType = "";
            var finishedLine = "";

            if (Directory.Exists("1translation"))
                Directory.Delete("1translation", true);

            if (File.Exists("master.txt"))
                File.Delete("master.txt");

            var hmaster = new StreamWriter("master.txt", true);

            foreach (var line in lines)
            {
                var csvsplit = line.Split(';');

                var whereAmI = lineType;

                // Deciding what we're dealing with
                if (csvsplit[0].Contains("adv\\"))
                    lineType = "ADV";
                else if (csvsplit[0].Contains("h\\"))
                    lineType = "H";
                else if (csvsplit[0].Contains("communication\\"))
                    lineType = "COMMUNICATION";
                else if (csvsplit[0] == "")
                    lineType = "BlankLine";
                else
                    lineType = "Normal";

                // Generalizing linetype
                if (lineType == "ADV" || lineType == "H" || lineType == "COMMUNICATION")
                    lineType = "FileName";

                if (tlFile == "" && lineType != "FileName")
                    continue;

                switch (lineType)
                {
                    case "FileName"
                        : // Changing directory, makes sure it exists and writes comments to external file for preservation if they exist
                        tlFile = $"1translation\\{csvsplit[0]}";
                        if (!tlFile.Contains("abdata"))
                            tlFile = $"1translation\\abdata\\{csvsplit[0]}";
                        var tlPath = tlFile.Remove(tlFile.Length - 15, 15);
                        if (!Directory.Exists(tlPath))
                            Directory.CreateDirectory(tlPath);
                        finishedLine = "";
                        Console.WriteLine("Writing to " + tlFile);
                        break;
                    case "BlankLine":
                        finishedLine = "\n";
                        break;
                    case "Normal":
                        finishedLine = $"{csvsplit[0]}={csvsplit[1]}\n";
                        break;
                }

                if (tlFile.Contains("h\\") && lineType == "Normal") hmaster.WriteLine($"{csvsplit[0]}={csvsplit[1]}");

                // TODO: Need to find a way to do this without endlessly opening the same file...

                using var translationFile = new StreamWriter(tlFile, true);
                if (finishedLine != "")
                    translationFile.Write(finishedLine);
                translationFile.Close();
            }

            hmaster.Close();
            Console.Clear();
        }

        private static void ReadXua(string workingDir, string folderName)
        {
            var category = "";
            var charactertypes = new HashSet<string>();

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
                        var match = Regex.Match(hTransFile, @"communication\\.*communication_(\d\d)");

                        if (match.Groups[1].Value == "")
                            match = Regex.Match(hTransFile, @"communication\\.*optiondisplayitems_(\d\d)");

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
                Console.WriteLine($"Dealing with {selectedcharacter} in {folderName}");
                var csvFileName = $"CSVFiles\\{selectedcharacter}.csv";
                var errorFileName = "CSVFiles\\Errors.txt";
                var firstLine = false;

                if (!Directory.Exists("CSVFiles"))
                    Directory.CreateDirectory("CSVFiles");

                if (!File.Exists(csvFileName))
                    firstLine = true;

                var outputfile = new StreamWriter(csvFileName, true);

                foreach (var hTransFile in Directory.EnumerateFiles(workingDir, "*.txt", SearchOption.AllDirectories))
                {
                    var newDoc = true;
                    var charatype = "";
                    var hTranslationFiles = File.ReadAllLines(hTransFile);

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
                                        ? $";;\n;;\n{hTransFile};;;"
                                        : $";;\n;;\n{hTransFile};;");

                                    newDoc = false;
                                }

                                if (line != "" && line != " ")
                                {
                                    var splitLine = line.Replace(@"/", "").Split('=');
                                    bool steamHit = false;
                                    if (steamAlternativeExists)
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
                                        outputfile.WriteLine(steamAlternativeExists
                                            ? $"{splitLine[0]};{splitLine[1]};;"
                                            : $"{splitLine[0]};{splitLine[1]};");
                                    }

                                    if(!steamHit && steamAlternativeExists)
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

        private static void FillHDupes()
        {
            var master = File.ReadAllLines("master.txt");
            var prevStepTl = "1translation\\abdata\\h";
            var filenumber = 0;

            var copyDirs = new List<string>
            {
                "adv",
                "communication"
            };

            foreach (var item in copyDirs)
            {
                var sourcePath = $"1translation\\abdata\\{item}";
                var destinationPath = $"2translation\\abdata\\{item}";

                // Creating directories
                foreach (var dirPath in Directory.GetDirectories(sourcePath, "*.*",
                    SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

                // Copying files, replacing duplicates
                foreach (var newPath in Directory.GetFiles(sourcePath, "*.txt",
                    SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
            }

            foreach (var hTransFile in Directory.EnumerateFiles(prevStepTl, "*.txt", SearchOption.AllDirectories))
            {
                var hTranslationFiles = File.ReadAllLines(hTransFile); // Filename obtained
                var outputdir = hTransFile.Replace("1translation", "2translation").Remove(hTransFile.Length - 15, 15);
                filenumber++;
                Console.Clear();
                Console.WriteLine("Working on file " + filenumber + " out of " + hTranslationFiles.Length + "...");

                if (!Directory.Exists(outputdir))
                    Directory.CreateDirectory(outputdir);

                var file = new StreamWriter($"{outputdir}\\translation.txt", true);

                foreach (var oldLine in hTranslationFiles)
                {
                    var splitOldLine = oldLine.Split('='); // splitOldLine[0] for comparison
                    var hit = false;

                    foreach (var masterLine in master)
                    {
                        var splitMasterLine = masterLine.Split('='); // splitMasterLine[0] for comparison


                        if (splitOldLine[0] != splitMasterLine[0] || hit) continue;
                        file.Write(splitMasterLine[0] + "=" + splitMasterLine[1] + "\n");
                        hit = true;
                    }

                    if (splitOldLine[0] == "")
                        file.Write("\n");
                    else if (!hit)
                        try
                        {
                            file.Write(splitOldLine[0] + "=" + splitOldLine[1] + "\n");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                }

                file.Close();
            }
        }

        private static void CleanupTranslation(string oldDir)
        {
            var sDir = oldDir;

            if (Directory.Exists("3translation"))
                Directory.Delete("3translation", true);

            foreach (var translationFile in Directory.EnumerateFiles(sDir, "*.txt", SearchOption.AllDirectories))
            {
                var translationLine = File.ReadAllLines(translationFile);
                var translationOutputDir = "3" + translationFile.Remove(translationFile.Length - 15, 15).Remove(0, 1);
                var translationOutputFile = translationOutputDir + "translation.txt";

                Console.WriteLine(translationOutputFile);
                foreach (var translationSentence in translationLine)
                {
                    var splittrans = translationSentence.Split('=');
                    Directory.CreateDirectory(translationOutputDir);
                    try
                    {
                        splittrans[1] = RunFix(splittrans[1]);
                        var donetrans = splittrans[0] + "=" + splittrans[1];

                        using var hit = new StreamWriter(translationOutputFile, true);
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

        private static void MakeCsv(string workingFolder)
        {
            var workDir = workingFolder;
            File.Delete("newTranslation.csv");
            foreach (var hTransFile in Directory.EnumerateFiles(workDir + "\\abdata\\adv", "*.txt",
                SearchOption.AllDirectories))
            {
                var hTranslationFiles = File.ReadAllLines(hTransFile);
                var file = new StreamWriter("newTranslation.csv", true);

                file.Write(hTransFile.Remove(0, 13) + "\n");
                Console.WriteLine(hTransFile + ";;");

                foreach (var line in hTranslationFiles)
                {
                    var exportline = line.Split('=');
                    if (exportline[0] == "")
                        file.Write(";;\n");
                    else
                        file.Write(exportline[0] + ";" + exportline[1] + ";" + "\n");
                }

                file.Write(";;\n;;\n");
                file.Close();
            }

            foreach (var hTransFile in Directory.EnumerateFiles(workDir + "\\abdata\\communication", "*.txt",
                SearchOption.AllDirectories))
            {
                var hTranslationFiles = File.ReadAllLines(hTransFile);
                var file = new StreamWriter("newTranslation.csv", true);

                file.Write(hTransFile.Remove(0, 13) + "\n");
                Console.WriteLine(hTransFile + ";;");

                foreach (var line in hTranslationFiles)
                {
                    var exportline = line.Split('=');
                    if (exportline[0] == "")
                        file.Write(";;\n");
                    else
                        file.Write(exportline[0] + ";" + exportline[1] + ";" + "\n");
                }

                file.Write(";;\n;;\n");
                file.Close();
            }

            foreach (var hTransFile in Directory.EnumerateFiles(workDir + "\\abdata\\h", "*.txt",
                SearchOption.AllDirectories))
            {
                var hTranslationFiles = File.ReadAllLines(hTransFile);
                var file = new StreamWriter("newTranslation.csv", true);

                file.Write(hTransFile.Remove(0, 13) + "\n");
                Console.WriteLine(hTransFile + ";;");

                foreach (var line in hTranslationFiles)
                {
                    var exportline = line.Split('=');
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
            //replacer = replacer.Replace("....", "...");

            replacer = Regex.Replace(replacer, @"(^|[^\.])(\.{3})+(\.{1,2})([^\.]|$)", "$1$2$4");
            replacer = Regex.Replace(replacer, @"[\s](?<!\.)(\.{3})+(?!\.)([\S]|$)", "$2$3 $4");
            replacer = Regex.Replace(replacer, @"(^|[^\.])(\.{2})([^\.]|$)", "$1$2.$3");
            replacer = Regex.Replace(replacer, "[ ]{2,}", " ");

            foreach (string entry in killstringList)
            {
                replacer = replacer.Replace(entry, "");
            }

            return replacer.TrimEnd();
        }
    }
}