using System;
using System.Collections.Generic;
using System.Linq;

namespace LIOD
{
    public class Analyzer
    {
        static int DefMaxValue = 255;
        static int DefMinValue = 0;
        static int DefStepValue = 256;

        public static string Analyze(string contents)
        {
            // predefining variables
            List<Definition> definitions = new List<Definition>();
            List<Macro> macros = new List<Macro>();
            List<string> clojure = new List<string>();
            List<string> precode = new List<string>();

            // analysing
            for (int i = 0; i < contents.Length; i++)
            {
                // clojure
                if (contents[i] == '(')
                {
                    // for pretty simple getting atom and arguments
                    clojure = Element.ParseClojure(Element.GetClojure(contents, i));

                    // preexpanding
                    clojure = PreExpand(clojure, definitions);

                    // def
                    if (clojure[0] == "def")
                    {
                        // getting def's name
                        string value, name = clojure[1];

                        // can't define a number
                        if (int.TryParse(name, out _))
                            continue;

                        // concating val
                        string val = "";
                        for (int j = 2; j < clojure.Count; j++)
                            val += clojure[j] + " ";

                        // if second argument exist - value, else - 0
                        if (clojure.Count > 2)
                            value = val.Trim(' ');
                        else
                            value = "";

                        // if the value is existing constant replace it with its value
                        if (DefinitionsContains(value, definitions))
                            value = GetDefinitionValue(value, definitions);

                        // remove definition from the list, if it exist
                        definitions.Remove(new Definition(name, GetDefinitionValue(name, definitions)));

                        // adding definition to list and removing it from contents
                        definitions.Add(new Definition(name, value));
                        contents = Element.RemoveClojure(contents, i);
                    }

                    // mac
                    else if (clojure[0] == "mac")
                    {
                        // getting mac's name
                        string name = clojure[1], value = "";

                        // getting arguments and removing mac from them
                        List<string> args = Element.ParseClojure(clojure[2]);
                        args.RemoveAt(0);

                        // getting the value add removing extra space
                        for (int j = 3; j < clojure.Count; j++)
                            value += clojure[j] + " ";
                        value = value.Trim();

                        // adding macro to list and removing it from contents
                        macros.Add(new Macro(name, args, value));
                        contents = Element.RemoveClojure(contents, i);
                    }
                    
                    // container
                    else if (clojure[0] == "con")
                    {
                        // passing con keyword
                        i += 4;
                    }

                    // expand macro
                    else
                    {
                        // predefining variables
                        Macro macro;
                        List<string> args = new List<string>();
                        List<Definition> defs = new List<Definition>();
                        string expanded;

                        // getting macro
                        macro = GetMacro(clojure[0], macros);

                        // getting all arguments
                        for (int j = 1; j < clojure.Count; j++)
                            args.Add(clojure[j]);

                        // bind arguments, firstly should check if there any arguments, can passed to macro
                        if (macro.Arguments.Count != 0)
                        {
                            int j = 0;

                            // while there any arguments with ungiven value
                            for (; j < macro.Arguments.Count; j++)
                                
                                // macro has argument and it was given
                                if (j < args.Count)
                                    defs.Add(new Definition(macro.Arguments[j], args[j]));

                                // macro hasan argument, but it wasn't given
                                else
                                    defs.Add(new Definition(macro.Arguments[j], ""));

                            // arguments are overgiven
                            while (j < args.Count)
                            {
                                Definition def = defs.Last();
                                def.Value += " " + args[j++];
                                defs[defs.Count - 1] = def;
                            }
                        }

                        // expanding macro's arguments
                        expanded = ExpandArguments(new string(macro.Value.ToCharArray()), defs);
                        
                        // replacing the clojure with the macro 
                        contents = Element.RemoveClojure(contents, i);
                        contents = contents.Insert(i, expanded);

                        // back for recheck
                        i--;
                    }
                }

                // precode
                else if (contents[i] == '[')
                {
                    // for simple editing
                    precode = Element.ParsePrecode(Element.GetPrecode(contents, i));
                    string replace = "";

                    // preexpanding
                    precode = PreExpand(precode, definitions);

                    // setting unsigned bitness
                    if (precode[0][0] == 'u')
                    {
                        // getting the bitness
                        int bitness = int.Parse(precode[0].Substring(1));

                        // setting the values
                        DefStepValue = (int)Math.Pow(2, bitness);
                        DefMinValue = 0;
                        DefMaxValue = DefStepValue - 1;
                    }

                    // setting signed bitness
                    else if (precode[0][0] == 's')
                    {
                        // getting the bitness
                        int bitness = int.Parse(precode[0].Substring(1));

                        // setting the values
                        DefStepValue = (int)Math.Pow(2, bitness);
                        DefMinValue = 0 - DefStepValue / 2;
                        DefMaxValue = DefStepValue / 2 - 1;
                    }

                    // incrementing def
                    else if (precode[0] == "++")
                    {
                        // definition properties
                        string name = precode[1];
                        string value = GetDefinitionValue(name, definitions);
                        string newvalue = ToValue(int.Parse(value) + 1).ToString();

                        // remove definition from the list
                        definitions.Remove(new Definition(name, value));

                        // adding definition to list
                        definitions.Add(new Definition(name, newvalue));
                    }

                    // decrementing def
                    else if (precode[0] == "--")
                    {
                        // definition properties
                        string name = precode[1];
                        string value = GetDefinitionValue(name, definitions);
                        string newvalue = ToValue(int.Parse(value) - 1).ToString();

                        // remove definition from the list
                        definitions.Remove(new Definition(name, value));

                        // adding definition to list
                        definitions.Add(new Definition(name, newvalue));
                    }

                    // adding to temp
                    else if (precode[0] == "%")
                    {
                        // the definition
                        string def = precode[1];
                        string value = GetDefinitionValue(def, definitions);

                        // adding it to list
                        Generator.Temps.Add(new Temp(value));
                    }

                    // concat
                    else if (precode[0] == "$")
                    {
                        // getting def's name
                        string value = "", name = precode[1];

                        // can't define a number
                        if (int.TryParse(name, out _))
                            continue;

                        // getting the value
                        precode.RemoveAt(1);
                        precode.RemoveAt(0);
                        foreach (string code in precode)
                            value += code;

                        // remove definition from the list
                        definitions.Remove(new Definition(name, GetDefinitionValue(name, definitions)));

                        // adding definition to list
                        definitions.Add(new Definition(name, value));
                    }

                    // if def exist
                    else if (precode[0] == "?")
                    {
                        // getting the name
                        string def = precode[1];

                        // concating else
                        string elsy = "";
                        for (int j = 3; j < precode.Count; j++)
                            elsy += precode[j] + " ";

                        // if it exist
                        if (DefinitionsContains(def, definitions))
                            replace = precode[2];
                        else if (precode.Count > 3)
                            replace = elsy.Trim(' ');
                    }

                    // if equal
                    else if (precode[0] == "==")
                    {
                        // getting defs
                        string def1 = GetDefinitionValue(precode[1], definitions);
                        string def2 = GetDefinitionValue(precode[2], definitions);

                        // concating else
                        string elsy = "";
                        for (int j = 4; j < precode.Count; j++)
                            elsy += precode[j] + " ";

                        // if def1 == def2
                        if (def1 == def2)
                            replace = precode[3];
                        else if (precode.Count > 3)
                            replace = elsy.Trim(' ');
                    }

                    // error
                    else if (precode[0] == "!")
                    {
                        Program.Error(precode[1].Substring(2, precode[1].Length - 4));
                    }

                    // end
                    else if (precode[0] == "&")
                    {
                        contents += " " + precode[1];
                    }

                    // return 
                    else if (precode[0] == "=>")
                    {
                        replace = "\0\0";
                    }

                    // removing precode
                    contents = Element.RemovePrecode(contents, i);
                    contents = contents.Insert(i, replace);

                    // back for recheck
                    i--;
                }

                // atom
                else if (contents[i] != ' ' || contents[i] != ')' || contents[i] != ']')
                {
                    // getting the atom
                    string atom = Element.GetAtom(contents, i);

                    // if atom is definition
                    if (DefinitionsContains(atom, definitions))
                    {
                        contents = Element.RemoveAtom(contents, i);
                        contents = contents.Insert(i, GetDefinitionValue(atom, definitions));
                        i--;
                    }

                    // elsewize it is non-compileable value
                    else
                        i += atom.Length;
                }
            }

            // removing spaces for "purity"
            contents = Preprocessor.Standartize(contents);

            // showing some debug information
            ShowDebugInfo(contents, definitions, macros);

            // returning processed contents
            return contents;
        }

        public static string ExpandArguments(string contents, List<Definition> definitions)
        {
            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i] != ' ' && contents[i] != ')' && contents[i] != '(')
                {
                    // getting the atom
                    string atom = Element.GetAtom(contents, i);

                    // if atom is definition
                    if (DefinitionsContains(atom, definitions))
                    {
                        contents = Element.RemoveAtom(contents, i);
                        contents = contents.Insert(i, GetDefinitionValue(atom, definitions));
                        i += GetDefinitionValue(atom, definitions).Length;
                    }

                    // not full expansion
                    else
                        i += atom.Length;
                }
            }
            
            // returning the result
            return contents;
        }

        public static bool DefinitionsContains(string name, List<Definition> definitions)
        {
            foreach (Definition definition in definitions)
                if (definition.Name == name)
                    return true;
            return false;
        }

        public static string GetDefinitionValue(string name, List<Definition> definitions)
        {
            foreach (Definition definition in definitions)
                if (definition.Name == name)
                    return definition.Value;
            return name;
        }

        public static bool MacrosContains(string name, List<Macro> macros)
        {
            foreach (Macro macro in macros)
                if (macro.Name == name)
                    return true;
            return false;
        }

        public static Macro GetMacro(string name, List<Macro> macros)
        {
            foreach (Macro macro in macros)
                if (macro.Name == name)
                    return new Macro(macro.Name, macro.Arguments, macro.Value);
            Program.Error("Macro \"" + name + "\" is not defined. ( LIOD00 )");
            throw new Exception("");
        }

        public static void ShowDebugInfo(string contents, List<Definition> definitions, List<Macro> macros)
        {
            // writing title and contents
            Console.WriteLine("ANALYSER:");
            Console.WriteLine(contents);
            Console.WriteLine();

            // all definitions
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("definition name:\tvalue");
            Console.ForegroundColor = ConsoleColor.Gray;
            foreach (Definition definition in definitions)
            {
                Console.Write(definition.Name);
                Console.CursorLeft = 24;
                Console.WriteLine(definition.Value);
            }
            Console.WriteLine();

            // all definitions
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("macro name:\t\targs\t\t\tvalue");
            Console.ForegroundColor = ConsoleColor.Gray;
            foreach (Macro macro in macros)
            {
                Console.Write(macro.Name);
                Console.CursorLeft = 24;
                foreach (string arg in macro.Arguments)
                    Console.Write(arg + " ");
                Console.CursorLeft = 48;
                Console.WriteLine(macro.Value);
            }

            Console.WriteLine();
        }

        public static int ToValue(int value)
        {
            while (value > DefMaxValue)
                value -= DefStepValue;
            while (value < DefMinValue)
                value += DefStepValue;

            return value;
        }

        public static List<string> PreExpand(List<string> clojure, List<Definition> definitions)
        {
            for (int i = 0; i < clojure.Count; i++)
            {
                if (clojure[i][0] == '[')
                {
                    List<string> precode = Element.ParsePrecode(clojure[i]);
                    if (precode[0] == "^")
                        clojure[i] = GetDefinitionValue(precode[1], definitions);
                }
            }

            return clojure;
        }
    }

    // a structure for definitions
    public struct Definition
    {
        public string Name;
        public string Value;

        public Definition(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }

    // a structure for macros
    public struct Macro
    {
        public string Name;
        public List<string> Arguments;
        public string Value;

        public Macro(string name, List<string> args, string value)
        {
            Name = name;
            Arguments = args;
            Value = value;
        }
    }
}
