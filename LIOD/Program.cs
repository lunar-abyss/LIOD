using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LIOD
{
    public class Program
    {
        public static string LIODFile;

        public static void Main(string[] args)
        {
            // predefining variables
            string contents;
            
            // getting file
            LIODFile = args[0];

            // reading file contents
            contents = GetFileContents();

            // using preprocessor
            contents = Preprocessor.Process(contents);

            // using analyser
            contents = Analyzer.Analyze(contents);

            // using generator
            contents = Generator.Generate(contents);

            // writing to file
            File.WriteAllText(Path.GetDirectoryName(LIODFile) + "\\" + Path.GetFileNameWithoutExtension(LIODFile) + ".lf", contents);

            // no auto close
            Console.ReadKey();
        }

        public static string GetFileContents()
        {
            // getting all file contents
            string contents = File.ReadAllText(LIODFile);
            return contents;
        }

        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Error: " + message);
            Console.ReadKey();
            Environment.Exit(-1);
        }
    }
}
