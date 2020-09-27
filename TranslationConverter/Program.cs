using System;
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
            // Checking that the input file is a csv 
            if(args.Length == 0 || args[0].Remove(0, args[0].Length - 4) != ".csv")
            {
                Console.WriteLine("Please drag a .csv file at this application to use");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            // Splitting the csv to txt XUA translations
            string csvfile = args[0];
            string TLFile = "Undefined";
            string TLPath = "Undefined";
            string[] lines = System.IO.File.ReadAllLines(csvfile);
            foreach (string line in lines)
            {
                string[] csvsplit = line.Split(';');
                if (csvsplit[3] == "(x)")
                {
                    TLFile = $"1translation\\{csvsplit[0]}";
                    TLPath = TLFile.Remove(TLFile.Length - 15, 15);
                    if (!Directory.Exists(TLPath))
                        Directory.CreateDirectory(TLPath);
                }
                else if (csvsplit[3] == "(y)")
                {
                    using (StreamWriter hit = new StreamWriter(TLFile, true))
                    {
                        hit.WriteLine("");
                    }
                }
                else if (csvsplit[3] == "(z)")
                {
                    using (StreamWriter hit = new StreamWriter(TLFile, true))
                    {
                        hit.WriteLine(csvsplit[0] + "=" + csvsplit[1]);
                        Console.WriteLine(csvsplit[0] + "=" + csvsplit[1]);
                    }
                }
                else if (csvsplit[3] == "(xx)")
                {
                    using (StreamWriter hit = new StreamWriter(TLFile, true))
                    {
                        hit.WriteLine(csvsplit[0] + "=");
                        Console.WriteLine(csvsplit[0] + "=");
                    }
                }
                if (TLPath.Contains("h\\list") && csvsplit[3] == "(z)")
                {
                    using (StreamWriter hit = new StreamWriter("master.txt", true))
                    {
                        hit.WriteLine(csvsplit[0] + "=" + csvsplit[1]);
                    }
                }
            }

            // Dupe filling

            string[] master = System.IO.File.ReadAllLines("master.txt");
            string prevStepTL = "1translation\\h";
            var filenumber = 0;

            foreach (string HTransFile in Directory.EnumerateFiles(prevStepTL, "*.txt", SearchOption.AllDirectories))
            {
                string[] HTranslationFiles = File.ReadAllLines(HTransFile); // Filename obtained
                string outputdir = HTransFile.Replace("1translation", "2translation").Remove(HTransFile.Length - 15, 15);
                string finalLine = "unassigned";
                filenumber++;
                Console.Clear();
                Console.WriteLine("Working on file " + filenumber + " out of " + HTransFile.Length + "...");

                foreach (string oldLine in HTranslationFiles)
                {
                    string[] splitOldLine = oldLine.Split('='); // splitOldLine[0] for comparison

                    foreach (string masterLine in master)
                    {
                        string[] splitMasterLine = masterLine.Split('='); // splitMasterLine[0] for comparison


                        if (splitOldLine[0] == splitMasterLine[0])
                            finalLine = splitMasterLine[0] + "=" + splitMasterLine[1];
                    }

                    if (finalLine.Length == 0)
                        finalLine = splitOldLine[0] + "=" + splitOldLine[1];

                    if (!Directory.Exists(outputdir))
                        Directory.CreateDirectory(outputdir);

                    using (StreamWriter writer = new StreamWriter($"{outputdir}\\translation.txt", true))
                    {
                        //Console.WriteLine($"{outputdir}\\translation.txt");
                        writer.WriteLine(finalLine);
                    }
                }
            }

                Console.ReadKey();
            // Running Cleanup

            string sDir = "translation";

            if (Directory.Exists("3translation"))
                Directory.Delete("3translation", true);

            foreach (string TranslationFile in Directory.EnumerateFiles(sDir, "*.txt", SearchOption.AllDirectories))
            {
                string[] TranslationLine = File.ReadAllLines(TranslationFile);
                string TranslationOutputDir = "3" + TranslationFile.Remove(TranslationFile.Length - 15, 15);
                string TranslationOutputFile = TranslationOutputDir + "translation.txt";

                Console.WriteLine(TranslationOutputFile);
                foreach (string TranslationSentence in TranslationLine)
                {
                    string[] splittrans = TranslationSentence.Split('=');
                    Directory.CreateDirectory(TranslationOutputDir);
                    try
                    {
                        splittrans[1] = runfix(splittrans[1]);
                        string donetrans = splittrans[0] + "=" + splittrans[1];

                        using (StreamWriter hit = new StreamWriter(TranslationOutputFile, true))
                        {
                            hit.WriteLine(splittrans[0] + "=" + splittrans[1]);
                        }
                    }
                    catch (Exception err)
                    {
                        Console.Out.WriteLine(err);
                    }
                }
            }

        }

        public static string runfix(string translation)
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
