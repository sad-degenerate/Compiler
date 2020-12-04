using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Compiler
{
    public class NewLangCode
    {
        public string Code { get; private set; }
        private string compilerVersion = "v4.0";

        public NewLangCode(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath), "Путь к файлу не может быть пустым.");

            Rewrite(filePath);
        }

        public string Compile(string fileName)
        {
            var provider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompillerVersion", compilerVersion } });
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

        private string Semicolon(string line, int lineNumber)
        {
            if (line[0] != ';')
                throw new Exception($"Отсутствует запятая в начале строчки (строчка №{lineNumber}).");
            else
                return line.Substring(1, line.Length - 1);
        }

        private void Rewrite(string filePath)
        {
            var lineNumber = 1;
            var file = new StreamReader(filePath);

            string line;
            while ((line = file.ReadLine()) != null)
            {
                line = Semicolon(line, lineNumber);

                if (line.Contains('!'))
                    line = ExclamationOutput(line);

                if (line.Contains('"') && line.Contains('*'))
                    line = StringMultiplication(line);

                if (line.Contains('^'))
                    line = Caret(line);

                Code += line;

                lineNumber++;
            }

            file.Close();
        }

        private string Caret(string line)
        {
            var carretPos = 0;
            for (var i = 0; i < line.Length; i++)
                if (line[i] == '^')
                    carretPos = i;

            var power = "";
            var powerPos = (0, 0);
            for (var i = carretPos; i < line.Length; i++)
            {
                if (char.IsDigit(line[i]))
                {
                    if (string.IsNullOrWhiteSpace(power))
                        powerPos.Item1 = i;

                    power += line[i];
                }
                else
                    if (!string.IsNullOrWhiteSpace(power))
                    {
                        powerPos.Item2 = i;
                        break;
                    }
            }

            var number = "";
            var numberPos = (0, 0);
            for (var i = carretPos; i > 0; i--)
            {
                if (char.IsDigit(line[i]))
                {
                    if (string.IsNullOrWhiteSpace(number))
                        numberPos.Item1 = i;

                    number += line[i];
                }
                else
                    if (!string.IsNullOrWhiteSpace(number))
                    {
                        numberPos.Item2 = i;
                        break;
                    }
            }

            var newLine = "";
            for (var i = 0; i < numberPos.Item1; i++)
                newLine += line[i];

            newLine += $"{Math.Pow(int.Parse(number), int.Parse(power))}";

            for (var i = powerPos.Item2; i < line.Length; i++)
                newLine += line[i];

            return newLine;
        }

        private string StringMultiplication(string line)
        {
            var quotesPos = new List<int>();
            var starPos = 0;

            for (var i = 0; i < line.Length; i++)
                if (line[i] == '"')
                    quotesPos.Add(i);

            if (quotesPos.Count < 2)
                return line;

            for (var i = 0; i < line.Length; i++)
                if (line[i] == '*')
                {
                    if (quotesPos[0] < i && quotesPos[1] < i)
                        starPos = i;
                    break;
                }

            if (starPos == 0)
                return line;

            var digit = "";
            var digitPos = (0, 0);
            for (var i = starPos; i < line.Length; i++)
            {
                if (char.IsDigit(line[i]))
                {
                    if (string.IsNullOrWhiteSpace(digit))
                        digitPos.Item1 = i;

                    digit += line[i];
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(digit))
                    {
                        digitPos.Item2 = i;
                        break;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(digit))
                return line;

            var newLine = "";
            for (var i = 0; i < quotesPos[0]; i++)
                newLine += line[i];

            var multString = "";
            for (var i = quotesPos[0] + 1; i < quotesPos[1]; i++)
                multString += line[i];

            newLine += "\"";
            for (var i = 0; i < int.Parse(digit); i++)
                newLine += multString;
            newLine += "\"";

            for (var i = digitPos.Item2; i < line.Length; i++)
                newLine += line[i];

            return newLine;
        }

        private string ExclamationOutput(string line)
        {
            var count = 1;
            var newString = "";
            foreach (var part in line.Split('"'))
            {
                var parts = part.Split('!');

                if (count % 2 == 1)
                    for (var i = 0; i < parts.Length; i++)
                    {
                        if (i % 2 == 1 && i != parts.Length - 1)
                            newString += $"Console.WriteLine(\"{parts[i]}\");";
                        else
                            newString += parts[i];
                    }
                else
                    newString += "\"" + part + "\"";

                count++;
            }

            return newString;
        }
    }
}