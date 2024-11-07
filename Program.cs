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

namespace tesis
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string[] emails = { "test@example.com", "invalid-email", "user@domain", "another.email@domain.com" };
            string[] phoneNumbers = { "+1234567890", "123456", "+1(234)567-890", "+987654321098765" };
            string[] names = { "Jean-Paul","O'Connor","Élodie Durand","Franz Müller","Chloé","Nino D’Angelo",
                              "Jean123", "@Jean-Paul", "Jean-Paul!" };

            // Регулярные выражения
            string emailPattern = @"^[\w\.-]+@[a-zA-Z\d\.-]+\.[a-zA-Z]{2,6}$";
            string phonePattern = @"^\+?[1-9]\d{1,14}$";
            String namePattern = @"^[A-Za-zÀ-ÖØ-öø-ÿ' -]+$";

            Console.WriteLine("Проверка электронной почты с System.Text.RegularExpressions:");
            foreach (var email in emails)
            {
                bool isValid = Regex.IsMatch(email, emailPattern);
                Console.WriteLine($"Email: {email} - {(isValid ? "Корректен" : "Некорректен")}");
            }

            Console.WriteLine("\nПроверка телефонных номеров с System.Text.RegularExpressions:");
            foreach (var phone in phoneNumbers)
            {
                bool isValid = Regex.IsMatch(phone, phonePattern);
                Console.WriteLine($"Телефон: {phone} - {(isValid ? "Корректен" : "Некорректен")}");
            }

            Console.WriteLine("\nПроверка Имя с System.Text.RegularExpressions:");
            foreach (var name in names)
            {
                bool isValid = Regex.IsMatch(name, namePattern);
                Console.WriteLine($"Телефон: {name} - {(isValid ? "Корректен" : "Некорректен")}");
            }
            // Примеры с PCRE.NET
            Console.WriteLine("\nПроверка электронной почты с PCRE.NET:");
            foreach (var email in emails)
            {
                bool isValid = PcreRegex.IsMatch(email, emailPattern);
                Console.WriteLine($"Email: {email} - {(isValid ? "Корректен" : "Некорректен")}");
            }

            Console.WriteLine("\nПроверка телефонных номеров с PCRE.NET:");
            foreach (var phone in phoneNumbers)
            {
                bool isValid = PcreRegex.IsMatch(phone, phonePattern);
                Console.WriteLine($"Email: {phone} - {(isValid ? "Корректен" : "Некорректен")}");
            }

            Console.WriteLine("\nПроверка Имя с PCRE.NET:");
            foreach (var name in names)
            {
                bool isValid = PcreRegex.IsMatch(name, namePattern);
                Console.WriteLine($"Email: {name} - {(isValid ? "Корректен" : "Некорректен")}");
            }

            Console.WriteLine("\nПроверка электронной почты с Superpower:");
            foreach (var email in emails)
            {
                bool isValid = EmailParser(email);
                Console.WriteLine($"Email: {email} - {(isValid ? "Корректен" : "Некорректен")}");
            }

            Console.WriteLine("\nПроверка телефонных номеров с Superpower:");
            foreach (var phone in phoneNumbers)
            {
                bool isValid = PhoneParser(phone);
                Console.WriteLine($"Телефон: {phone} - {(isValid ? "Корректен" : "Некорректен")}");
            }

            Console.WriteLine("\nПроверка Имя с Superpower:");
            foreach (var name in names)
            {
                var phonePatternSuperpower = Span.Regex(@"^[A-Za-zÀ-ÖØ-öø-ÿ' -]+$");
                bool isValid = phonePatternSuperpower.TryParse(name).HasValue;
                Console.WriteLine($"Телефон: {name} - {(isValid ? "Корректен" : "Некорректен")}");
            }
            Console.ReadLine();
        }
        static bool EmailParser(string input)
        {
            var localPart = Character.LetterOrDigit.Or(Character.EqualTo('.')).AtLeastOnce();
            var domain = Character.LetterOrDigit.AtLeastOnce()
                .IgnoreThen(Character.EqualTo('.'))
                .IgnoreThen(Character.Letter.AtLeastOnce());

            var emailPattern = localPart
                .IgnoreThen(Character.EqualTo('@'))
                .IgnoreThen(domain);

            return emailPattern.TryParse(input).HasValue;
        }

        // Парсер для проверки телефонных номеров
        static bool PhoneParser(string input)
        {
            // Простое регулярное выражение для проверки номера телефона (использует Superpower)
            var phonePattern = Span.Regex(@"^\+?[1-9]\d{1,3}(-?\d{2,4}){2,3}$");

            return phonePattern.TryParse(input).HasValue;
        }
    }
}
