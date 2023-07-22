using System.Collections.Generic;

namespace LIOD
{
    public class Element
    {
        public static string GetClojure(string contents, int pointer)
        {
            // pointer points to '('

            // setting variables
            int nesting = 0;
            int length = 0;

            // getting length
            do
            {
                if (contents[pointer + length] == '(')
                    nesting++;
                if (contents[pointer + length] == ')')
                    nesting--;

                length++;
            } while (nesting > 0);

            // returning the result
            return contents.Substring(pointer, length);
        }

        public static string RemoveClojure(string contents, int pointer)
        {
            // getting length of next clojure
            int length = GetClojure(contents, pointer).Length;

            // removing clojure by pointer and length
            contents = contents.Remove(pointer, length);

            // returning the result
            return contents;
        }

        public static List<string> ParseClojure(string clojure)
        {
            // predefining variables
            List<string> elements = new List<string>();

            // all clojures is '(' + ' ' + operation_name + ' ' + args[] + ' ' + ')' => index = 2
            while (clojure[2] != ')')
            {
                // if it starts with '(', it will be processed as clojure
                if (clojure[2] == '(')
                {
                    elements.Add(GetClojure(clojure, 2));
                    clojure = RemoveClojure(clojure, 2);
                }

                // else if it starts with '[', it will be processed as precode
                if (clojure[2] == '[')
                {
                    elements.Add(GetPrecode(clojure, 2));
                    clojure = RemovePrecode(clojure, 2);
                }

                // if char = ' ' just remove it
                else if (clojure[2] == ' ')
                    clojure = clojure.Remove(2, 1);

                // elsewize as atom
                else
                {
                    elements.Add(GetAtom(clojure, 2));
                    clojure = RemoveAtom(clojure, 2);
                }
            }
            return elements;
        }

        public static string GetAtom(string contents, int pointer)
        {
            // pointer points to the first chat of atom
            string result = "";

            // will try get the atom while it is not ' ' and not 'eof'
            for (int i = pointer; i < contents.Length && contents[i] != ' '; i++)
                result += contents[i];
            
            // returning the atom
            return result;
        }

        public static string RemoveAtom(string contents, int pointer)
        {
            // getting length of next atom
            int length = GetAtom(contents, pointer).Length;

            // removing atom by pointer and length
            contents = contents.Remove(pointer, length);

            // returning the result
            return contents;
        }

        public static string GetPrecode(string contents, int pointer)
        {
            // pointer points to '['

            // setting variables
            int nesting = 0;
            int length = 0;

            // getting length
            do
            {
                if (contents[pointer + length] == '[')
                    nesting++;
                if (contents[pointer + length] == ']')
                    nesting--;

                length++;
            } while (nesting > 0);

            // returning the result
            return contents.Substring(pointer, length);
        }

        public static string RemovePrecode(string contents, int pointer)
        {
            // getting length of next clojure
            int length = GetPrecode(contents, pointer).Length;

            // removing clojure by pointer and length
            contents = contents.Remove(pointer, length);

            // returning the result
            return contents;
        }

        public static List<string> ParsePrecode(string precode)
        {
            // predefining variables
            List<string> elements = new List<string>();

            // all precodes is '[' + ' ' + operation_name + ' ' + args[] + ' ' + ']' => index = 2
            while (precode[2] != ']')
            {
                // if it starts with '(', it will be processed as clojure
                if (precode[2] == '(')
                {
                    elements.Add(GetClojure(precode, 2));
                    precode = RemoveClojure(precode, 2);
                }

                // else if it starts with '[', it will be processed as precode
                if (precode[2] == '[')
                {
                    elements.Add(GetPrecode(precode, 2));
                    precode = RemovePrecode(precode, 2);
                }

                // if char = ' ' just remove it
                else if (precode[2] == ' ')
                    precode = precode.Remove(2, 1);

                // elsewize as atom
                else
                {
                    elements.Add(GetAtom(precode, 2));
                    precode = RemoveAtom(precode, 2);
                }
            }
            return elements;
        }
    }
}