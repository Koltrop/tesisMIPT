using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Npgsql;
using System.IO;

namespace tesis
{
    internal class Program
    {
        private static HashSet<char> GetAllUniqueCharacters(NpgsqlConnection conn)
        {
            var uniqueChars = new HashSet<char>();

            // Get all characters from forenames
            using (var cmd = new NpgsqlCommand("SELECT forename FROM forenames", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string forename = reader.GetString(0);
                    foreach (char c in forename)
                    {
                        uniqueChars.Add(c);
                    }
                }
            }

            // Get all characters from names
            using (var cmd = new NpgsqlCommand("SELECT name FROM names", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    foreach (char c in name)
                    {
                        uniqueChars.Add(c);
                    }
                }
            }

            return uniqueChars;
        }

        private static string GenerateRegexPattern(HashSet<char> uniqueChars)
        {
            var ranges = new List<string>();
            var specialChars = new List<char>();

            // Group characters into ranges
            var orderedChars = uniqueChars.OrderBy(c => c).ToList();
            char? rangeStart = null;
            char? rangeEnd = null;

            foreach (char c in orderedChars)
            {
                if (char.IsLetter(c) || c == ' ' || c == '-' || c == '\'')
                {
                    if (!rangeStart.HasValue)
                    {
                        rangeStart = c;
                    }
                    rangeEnd = c;
                }
                else
                {
                    if (rangeStart.HasValue && rangeEnd.HasValue)
                    {
                        if (rangeStart == rangeEnd)
                        {
                            ranges.Add(Regex.Escape(rangeStart.ToString()));
                        }
                        else
                        {
                            ranges.Add($"{Regex.Escape(rangeStart.ToString())}-{Regex.Escape(rangeEnd.ToString())}");
                        }
                        rangeStart = null;
                        rangeEnd = null;
                    }
                    specialChars.Add(c);
                }
            }

            // Add the last range if exists
            if (rangeStart.HasValue && rangeEnd.HasValue)
            {
                if (rangeStart == rangeEnd)
                {
                    ranges.Add(Regex.Escape(rangeStart.ToString()));
                }
                else
                {
                    ranges.Add($"{Regex.Escape(rangeStart.ToString())}-{Regex.Escape(rangeEnd.ToString())}");
                }
            }

            // Build the final pattern
            var pattern = new StringBuilder("^[");
            pattern.Append(string.Join("", ranges));
            if (specialChars.Any())
            {
                pattern.Append(string.Join("", specialChars.Select(c => Regex.Escape(c.ToString()))));
            }
            pattern.Append("]+$");

            return pattern.ToString();
        }

        private static void TestPattern(string pattern, NpgsqlConnection conn, string outputDir)
        {
            // Generate timestamp for filenames
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string invalidForenamesFile = Path.Combine(outputDir, $"invalid_forenames_{timestamp}.txt");
            string invalidNamesFile = Path.Combine(outputDir, $"invalid_names_{timestamp}.txt");
            var regex = new Regex(pattern);

            // Test forenames
            using (var cmd = new NpgsqlCommand("SELECT id, forename FROM forenames", conn))
            using (var reader = cmd.ExecuteReader())
            {
                int invalidCount = 0;
                using (StreamWriter writer = new StreamWriter(invalidForenamesFile, false, Encoding.UTF8))
                {
                    writer.WriteLine($"Regular Expression: {pattern}");
                    writer.WriteLine("ID\tForename");
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string forename = reader.GetString(1);
                        bool isValid = regex.IsMatch(forename);
                        if (!isValid)
                        {
                            Console.WriteLine($"Forename: {forename} - Invalid");
                            writer.WriteLine($"{id}\t{forename}");
                            invalidCount++;
                        }
                    }
                }
                if (invalidCount == 0)
                {
                    Console.WriteLine("All forenames are valid!");
                    File.WriteAllText(invalidForenamesFile, $"Regular Expression: {pattern}\nNo invalid forenames found.");
                }
                else
                {
                    Console.WriteLine($"Found {invalidCount} invalid forenames. Saved to {invalidForenamesFile}");
                }
            }

            // Test names
            using (var cmd = new NpgsqlCommand("SELECT id, name FROM names", conn))
            using (var reader = cmd.ExecuteReader())
            {
                int invalidCount = 0;
                using (StreamWriter writer = new StreamWriter(invalidNamesFile, false, Encoding.UTF8))
                {
                    writer.WriteLine($"Regular Expression: {pattern}");
                    writer.WriteLine("ID\tName");
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        string name = reader.GetString(1);
                        bool isValid = regex.IsMatch(name);
                        if (!isValid)
                        {
                            Console.WriteLine($"Name: {name} - Invalid");
                            writer.WriteLine($"{id}\t{name}");
                            invalidCount++;
                        }
                    }
                }
                if (invalidCount == 0)
                {
                    Console.WriteLine("All names are valid!");
                    File.WriteAllText(invalidNamesFile, $"Regular Expression: {pattern}\nNo invalid names found.");
                }
                else
                {
                    Console.WriteLine($"Found {invalidCount} invalid names. Saved to {invalidNamesFile}");
                }
            }
        }

        static void Main(string[] args)
        {
            string connString = "Host=localhost;Database=tesis;Username=postgres;Password=7415";
            string outputDir = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "invalid_entries");
            Directory.CreateDirectory(outputDir);

            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    Console.WriteLine("Connected to database successfully!");

                    // Generate optimized regex pattern
                    Console.WriteLine("\nAnalyzing database content...");
                    var uniqueChars = GetAllUniqueCharacters(conn);
                    string pattern = GenerateRegexPattern(uniqueChars);
                    Console.WriteLine($"Generated pattern: {pattern}");

                    // Test the pattern
                    Console.WriteLine("\nTesting pattern against database...");
                    TestPattern(pattern, conn, outputDir);

                    // Save the pattern to a file with timestamp
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string patternFile = Path.Combine(outputDir, $"generated_pattern_{timestamp}.txt");
                    File.WriteAllText(patternFile, pattern);
                    Console.WriteLine($"\nPattern saved to: {patternFile}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("\nPress Enter to exit...");
            Console.ReadLine();
        }
    }
}
