using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using PCRE;
using Superpower;
using Superpower.Model;
using Superpower.Parsers;
using System.IO;

namespace tesis
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Папка с файлами фамилий
            string folderPath = Path.Combine(
                Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName,
                "surname_files");
            // Шаблон для фамилий, поддерживающий UTF-8 символы и знаки
            string namePattern = @"^[A-Za-zÀ-ÖØ-öø-ÿ' -]+$";

            Console.WriteLine("Тестирование фамилий с System.Text.RegularExpressions:");
            TestWithRegex(folderPath, namePattern, "System.Text.RegularExpressions");

            Console.WriteLine("\nТестирование фамилий с PCRE.NET:");
            TestWithPCRE(folderPath, namePattern);

            Console.WriteLine("\nТестирование фамилий с Superpower:");
            TestWithSuperpower(folderPath, namePattern);

            Console.ReadLine();
        }

        // Функция для загрузки фамилий из файла
        static IEnumerable<string> LoadSurnames(string filePath)
        {
            return File.ReadLines(filePath, Encoding.UTF8);
        }

        // Тестирование с использованием System.Text.RegularExpressions
        static void TestWithRegex(string folderPath, string pattern, string libraryName)
        {
            foreach (var filePath in Directory.GetFiles(folderPath, "*.txt"))
            {
                Console.WriteLine($"\nФайл: {Path.GetFileName(filePath)}");
                foreach (var surname in LoadSurnames(filePath))
                {
                    bool isValid = Regex.IsMatch(surname, pattern);
                    Console.WriteLine($"Фамилия: {surname} - {(isValid ? "Корректна" : "Некорректна")} ({libraryName})");
                }
            }
        }

        // Тестирование с использованием PCRE.NET
        static void TestWithPCRE(string folderPath, string pattern)
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
        static void TestWithSuperpower(string folderPath, string pattern)
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
