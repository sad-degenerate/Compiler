using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Compiler
{
    public class NewLangCode
    {
        public string Code { get; private set; }
        private string CompilerVersion = "v4.0";

        public NewLangCode(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "Путь к файлу не может быть пустым.");

            Rewrite(filePath);
        }

        public string Compile(string fileName)
        {
            var provider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompillerVersion", CompilerVersion } });

            var parameters = new CompilerParameters(new string[] { "mscorlib.dll", "System.Core.dll" }, fileName, true);

            parameters.GenerateExecutable = true;

            var results = provider.CompileAssemblyFromSource(parameters, Code);

            if (results.Errors.HasErrors)
            {
                var res = "";
                foreach (var error in results.Errors.Cast<CompilerError>())
                    res += $"Строка {error.Line}: {error.ErrorText}\n";

                return res;
            }
            else
            {
                return "";
            }
        }

        private void Rewrite(string filePath)
        {
            var count = 1;

            var file = new StreamReader(filePath);

            var line = "";
            while ((line = file.ReadLine()) != null)
            {
                if (line[0] != ';')
                    throw new Exception($"Отсутствует запятая в начале строчки (строчка №{count}).");
                else
                    line = line.Substring(1, line.Length - 1);

                if (line.Contains("!"))
                {
                    var text = "";
                    var write = false;

                    for (var i = 0; i < line.Length; i++)
                    {
                        if (write)
                        {
                            if (line[i] != '!')
                                text += line[i];
                            else
                            {
                                write = false;
                                text += "\");";
                            }
                        }
                        else if (line[i] == '!')
                        {
                            write = true;
                            text += "Console.WriteLine(\"";
                        }    
                    }

                    if (write == true)
                        throw new Exception($"Обнаружен не парный восклицательный знак (строчка №{count})");

                    line = text;
                }

                Code += line;

                count++;
            }

            file.Close();
        }
    }
}