using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TranslationConverter.functions
{
    class RepoBlankLineBSRemover
    {
        public static void Runner()
        {
            foreach (var RepoTLFile in Directory.EnumerateFiles(@"abdata\adv", "*.txt", SearchOption.AllDirectories))
            {
                var lines = File.ReadAllLines(RepoTLFile);
                if (lines[0] == "")
                {
                    File.WriteAllLines(RepoTLFile, lines.Skip(1).ToArray());
                }
            }
        }
    }
}
