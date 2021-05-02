using System;
using System.IO;

namespace TranslationConverter.functions
{
    static class TransProgressClean
    {
        public static void Run(bool translationClean, string inputFile, string workFolder)
        {
            if (Misc.CheckFileExist(inputFile))
                return;

            FolderTask(workFolder); // Deleting workfolders if they exist to avoid dupe mess

            foreach (var FreshTLFile in Directory.EnumerateFiles(@"NewCSV", "*.csv", SearchOption.AllDirectories))
            {
                Misc.CheckFolderDel($@"{workFolder}\1translation");
                Misc.CheckFolderDel($@"{workFolder}\2translation");
                Misc.CheckFolderDel($@"{workFolder}\3translation");
                Misc.CheckFolderDel($@"{workFolder}\4translation");
                var CurrentPers = FreshTLFile.Remove(FreshTLFile.Length - 4, 4).Remove(0, 8);
                MergeWithUpdatedRepo.Run(translationClean, workFolder, FreshTLFile);
                Advmatch.Runner(true, "TLCleanWork", CurrentPers); // checking for dupe adv files
                FillHDupes.RunTask(true, workFolder); // Populating H-files
                FillComDupes.RunTask(true, workFolder); // Populating Com-files

                Misc.CheckFolderDel($@"{workFolder}\1translate");
                Misc.CheckFileDel($@"{workFolder}\masterH.txt");
                Misc.CheckFileDel($@"{workFolder}\masterCom.txt");

                CleanupStyle.Runner(true, "TLCleanWork"); // Cleaning up formatting

                Misc.CheckFileDel($@"{workFolder}\ExportedCSV\c{CurrentPers}.csv");

                // Exporting to CSV
                CreateSCV.Runner(true, @"TLCleanWork\3translation", "adv");
                CreateSCV.Runner(true, @"TLCleanWork\3translation", "communication");
                CreateSCV.Runner(true, @"TLCleanWork\3translation", "h");

                // Zipping up
                ZipHandler.Runner(workFolder, CurrentPers);
            }

            
            Console.Clear();
            Console.WriteLine("Completed!");
        }

        private static void FolderTask(string WorkFolder)
        {
            if (Directory.Exists($"{WorkFolder}\\1translation"))
                Directory.Delete($"{WorkFolder}\\1translation", true);
            if (Directory.Exists($"{WorkFolder}\\2translation"))
                Directory.Delete($"{WorkFolder}\\2translation", true);
            if (Directory.Exists($"{WorkFolder}\\3translation"))
                Directory.Delete($"{WorkFolder}\\3translation", true);
        }
    }
}
