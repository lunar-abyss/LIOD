using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace LIOD
{
    internal class Generator
    {
        public static List<Temp> Temps = new List<Temp>();

        public static string Generate(string contents)
        {
            // predefining variables
            Stack<int> temps = new Stack<int>();
            Stack<int> left = new Stack<int>();
            string newcontents = "";
            int tempscounter = 0;

            for (int i = 0; i < contents.Length; i++)
            {
                // clojure begins
                if (contents[i] == '(')
                {
                    // pushing i onto the stack
                    left.Push(i);

                    // pushing tempscounter onto the stack
                    temps.Push(tempscounter);
                    tempscounter = 0;
                }

                // expanding when clojure ends
                else if (contents[i] == ')')
                {
                    // getting the clojure and removing it from contents
                    int start = left.Pop();
                    string clojure = Element.GetClojure(contents, start);
                    contents = Element.RemoveClojure(contents, start);
                    i = start;

                    // makes free all temps, which was used
                    FreeTemp(tempscounter);

                    // getting temps was used by previous
                    tempscounter = temps.Pop();

                    // if current clojure returns something
                    if (clojure.Contains("\0\0"))
                    {
                        // getting next free temp
                        string temp = GetNextFreeTemp();

                        // replacing it
                        clojure = clojure.Replace("\0\0", temp);
                        contents = contents.Insert(start, temp);

                        // incrementing count of used
                        tempscounter++;
                    }

                    newcontents += clojure.Remove(0, 6).TrimEnd(')');
                }

                // if not inside of a clojure
                else if (left.Count == 0)
                    newcontents += contents[i];
            }

            // pure
            newcontents = Preprocessor.Standartize(newcontents);

            // returning need values
            // newcontents = newcontents.Replace("\x01", "(");
            // newcontents = newcontents.Replace("\x02", ")");
            // newcontents = newcontents.Replace("\x03", "[");
            // newcontents = newcontents.Replace("\x04", "]");

            // dubug
            ShowDebugInfo(newcontents);

            // returning results
            return newcontents;
        }

        public static string GetNextFreeTemp()
        {
            // getting next free temp and setting it to busy
            for (int i = 0; i < Temps.Count; i++)
            {
                if (!Temps[i].Busy)
                {
                    Temp temp = Temps[i];
                    temp.Busy = true;
                    Temps[i] = temp;
                    return temp.Name;
                }
            }
            return "";
        }

        public static void FreeTemp(int count)
        {
            Temps.Reverse();

            for (int i = 0; i < Temps.Count && count > 0; i++)
            {
                if (Temps[i].Busy)
                {
                    Temp temp = Temps[i];
                    temp.Busy = false;
                    Temps[i] = temp;
                    count--;
                }
            }

            Temps.Reverse();
        }
        
        public static void ShowDebugInfo(string newcontents)
        {
            Console.WriteLine("GENERATOR:");
            Console.WriteLine(newcontents);
            Console.WriteLine();
        }
    }

    struct Temp
    {
        public string Name;
        public bool Busy;

        public Temp(string name)
        {
            Name = name;
            Busy = false;
        }
    }
}
