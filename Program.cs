using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;

namespace tesis
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string folderPath = Path.Combine(
               Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName,
               "surname_files");

            // Шаблон для фамилий, поддерживающий UTF-8 символы и знаки
            string namePattern = @"^[A-Za-zÀ-ÖØ-öø-ÿ' -]+$";

            //retest: true for tests
            TestLibrary.RunTests(folderPath, namePattern, retest: false);

            foreach (var filePath in Directory.GetFiles(folderPath, "*.txt"))
            {
                var surnames = File.ReadLines(filePath, Encoding.UTF8);
                var count = 0;
                Console.WriteLine($"\nФайл: {Path.GetFileName(filePath)}");
                foreach (var surname in surnames)
                {
                    bool isValid = Regex.IsMatch(surname, namePattern);
                    if (!isValid)
                    {
                        Console.WriteLine($"Фамилия: {surname} - Некорректна");
                        count++;
                    }
                }
                if(count == 0)
                {
                    Console.WriteLine($"Все фамиля в {filePath} прошли проверки");
                }
            }

            Console.ReadLine();
        }
    }
}
