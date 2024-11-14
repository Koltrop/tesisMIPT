using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using PCRE;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;

namespace tesis
{
    public class TestLibrary
    {
        public static void RunTests(string folderPath, string namePattern, bool retest = false)
        {
            if (retest)
            {
                Console.WriteLine("Тестирование фамилий с System.Text.RegularExpressions:");
                TestWithRegex(folderPath, namePattern);

                Console.WriteLine("\nТестирование фамилий с PCRE.NET:");
                TestWithPCRE(folderPath, namePattern);

                Console.WriteLine("\nТестирование фамилий с Superpower:");
                TestWithSuperpower(folderPath, namePattern);
            }
        }

        // Функция для загрузки фамилий из файла
        public static IEnumerable<string> LoadSurnames(string filePath)
        {
            return File.ReadLines(filePath, Encoding.UTF8);
        }

        // Тестирование с использованием System.Text.RegularExpressions
        public static void TestWithRegex(string folderPath, string pattern)
        {
            foreach (var filePath in Directory.GetFiles(folderPath, "*.txt"))
            {
                Console.WriteLine($"\nФайл: {Path.GetFileName(filePath)}");
                foreach (var surname in LoadSurnames(filePath))
                {
                    bool isValid = Regex.IsMatch(surname, pattern);
                    Console.WriteLine($"Фамилия: {surname} - {(isValid ? "Корректна" : "Некорректна")} (System.Text.RegularExpressions)");
                }
            }
        }

        // Тестирование с использованием PCRE.NET
        public static void TestWithPCRE(string folderPath, string pattern)
        {
            var pcreRegex = new PcreRegex(pattern);
            foreach (var filePath in Directory.GetFiles(folderPath, "*.txt"))
            {
                Console.WriteLine($"\nФайл: {Path.GetFileName(filePath)}");
                foreach (var surname in LoadSurnames(filePath))
                {
                    bool isValid = pcreRegex.IsMatch(surname);
                    Console.WriteLine($"Фамилия: {surname} - {(isValid ? "Корректна" : "Некорректна")} (PCRE.NET)");
                }
            }
        }

        // Тестирование с использованием Superpower
        public static void TestWithSuperpower(string folderPath, string pattern)
        {
            var superpowerPattern = Span.Regex(pattern);
            foreach (var filePath in Directory.GetFiles(folderPath, "*.txt"))
            {
                Console.WriteLine($"\nФайл: {Path.GetFileName(filePath)}");
                foreach (var surname in LoadSurnames(filePath))
                {
                    bool isValid = superpowerPattern.TryParse(surname).HasValue;
                    Console.WriteLine($"Фамилия: {surname} - {(isValid ? "Корректна" : "Некорректна")} (Superpower)");
                }
            }
        }
    }
}
