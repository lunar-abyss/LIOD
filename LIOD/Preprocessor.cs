using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LIOD
{
    public class Preprocessor
    {
        public static List<string> Used = new List<string>();

        public static string Process(string contents)
        {
            // firstly uses should be expanded
            contents = ExpandUses(contents);
            
            // secondly remove all comments
            contents = RemoveAllComments(contents);

            // thirdly separate '(' and ')'
            contents = Separate(contents);

            // and in the end - standartizing
            contents = Standartize(contents);

            // showing debug info
            ShowDebugInfo(contents);

            // returning the final value
            return contents;
        }

        public static string ExpandUses(string contents)
        {
            // expanding uses with contents of other files
            while (contents.Contains("(use "))
            {
                // defining variables
                int index;
                string clojure, path, conts = "";

                // getting index of the start of using
                index = contents.IndexOf("(use ");

                // getting the clojure and removing it from the contents
                clojure = Element.GetClojure(contents, index);
                contents = Element.RemoveClojure(contents, index);

                // getting file path and it's contents
                path = Path.GetDirectoryName(Program.LIODFile) + "\\" + clojure.Substring(0, clojure.Length - 1).Substring(5) + ".liod";

                // don't multiple use
                if (!Used.Contains(path))
                {
                    conts = File.ReadAllText(path);
                    Used.Add(path);
                }

                // inserting file contents inside of main contents
                contents = contents.Insert(index, conts);
            }

            return contents;
        }

        public static string RemoveAllComments(string contents)
        {
            // predefining variables
            int start, end;

            // adding a new line to escape the case when the comment is in the end of the file
            contents += "\n";

            // removes comment started with ';' to the end of the line
            while (contents.Contains(';'))
            {
                // firstly - index of ';'
                start = contents.IndexOf(";");

                // secondly - index of '\n'
                end = contents.IndexOf("\n", start) + 1;

                // and in the end - removing
                contents = contents.Remove(start, end - start);
            }

            // returning the result
            return contents;
        }

        public static string Separate(string contents)
        {
            // special chars
            // contents = contents.Replace("\\(", "\x01");
            // contents = contents.Replace("\\)", "\x02");
            // contents = contents.Replace("\\[", "\x03");
            // contents = contents.Replace("\\]", "\x04");

            // separating all chars, which can stay together
            contents = contents.Replace("(", " ( ");
            contents = contents.Replace(")", " ) ");
            contents = contents.Replace("[", " [ ");
            contents = contents.Replace("]", " ] ");

            // returning the result
            return contents;
        }

        public static string Standartize(string contents)
        {
            // destroying tabs, extra spaces, new lines
            return Regex.Replace(contents, @"\s+", " ").Trim(' ');
        }

        public static void ShowDebugInfo(string contents)
        {
            // showing contents
            Console.WriteLine("PREPROCESSOR:");
            Console.WriteLine(contents);
            Console.WriteLine();
        }
    }
}
