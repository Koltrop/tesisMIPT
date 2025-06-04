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
        static void Main(string[] args)
        {
            // Database connection string
            string connString = "Host=localhost;Database=tesis;Username=postgres;Password=7415";
  
            // Pattern for validating names and surnames
            string namePattern = @"^[A-Za-zÀ-ÖØ-öø-ÿ' -]+$";

            // Create output directory if it doesn't exist
            string outputDir = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "invalid_entries");
            Directory.CreateDirectory(outputDir);

            // Files for invalid entries
            string invalidForenamesFile = Path.Combine(outputDir, "invalid_forenames.txt");
            string invalidNamesFile = Path.Combine(outputDir, "invalid_names.txt");

            try
            {
                using (var conn = new NpgsqlConnection(connString))
                {
                    conn.Open();
                    Console.WriteLine("Connected to database successfully!");

                    // Check forenames (surnames)
                    Console.WriteLine("\nChecking forenames (surnames):");
                    using (var cmd = new NpgsqlCommand("SELECT id, forename FROM forenames", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        int invalidCount = 0;
                        using (StreamWriter writer = new StreamWriter(invalidForenamesFile, false, Encoding.UTF8))
                        {
                            writer.WriteLine("ID\tForename");
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string forename = reader.GetString(1);
                                bool isValid = Regex.IsMatch(forename, namePattern);
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
                            File.WriteAllText(invalidForenamesFile, "No invalid forenames found.");
                        }
                        else
                        {
                            Console.WriteLine($"Found {invalidCount} invalid forenames. Saved to {invalidForenamesFile}");
                        }
                    }

                    // Check names
                    Console.WriteLine("\nChecking names:");
                    using (var cmd = new NpgsqlCommand("SELECT id, name FROM names", conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        int invalidCount = 0;
                        using (StreamWriter writer = new StreamWriter(invalidNamesFile, false, Encoding.UTF8))
                        {
                            writer.WriteLine("ID\tName");
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string name = reader.GetString(1);
                                bool isValid = Regex.IsMatch(name, namePattern);
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
                            File.WriteAllText(invalidNamesFile, "No invalid names found.");
                        }
                        else
                        {
                            Console.WriteLine($"Found {invalidCount} invalid names. Saved to {invalidNamesFile}");
                        }
                    }
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
