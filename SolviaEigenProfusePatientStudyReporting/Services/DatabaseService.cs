using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Serilog;
using SolviaEigenProfusePatientStudyReporting.Models;
using System.Data;
using System.Diagnostics;

namespace SolviaEigenProfusePatientStudyReporting.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<int>> GetAvailableYearsAsync()
        {
            var years = new List<int>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = "SELECT DISTINCT YEAR(TimeLastUpdate) AS Year FROM patients ORDER BY Year DESC";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                years.Add(reader.GetInt32("Year"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return years;
        }

        public async Task<List<int>> GetAvailableMonthsAsync(int year)
        {
            var months = new List<int>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = "SELECT DISTINCT MONTH(TimeLastUpdate) AS Month FROM patients WHERE YEAR(TimeLastUpdate) = @Year ORDER BY Month";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Year", year);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                months.Add(reader.GetInt32("Month"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return months;
        }

        public async Task<List<Patient>> GetPatientsByYearAsync(int year)
        {
            var patients = new List<Patient>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"SELECT RecDateTime, TimeLastUpdate, 
                                    CONVERT(CAST(CONVERT(PatDicom USING latin1) AS BINARY) USING utf8mb4) AS converted_patdicom,
                                    PatDOB, PatID, PatGender, PatAge, PatWeight, 
                                    CONVERT(CAST(CONVERT(PatComments USING latin1) AS BINARY) USING utf8mb4) AS converted_patcomments
                                    FROM patients
                                    WHERE YEAR(TimeLastUpdate) = @Year";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Year", year);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Debug.WriteLine($"RecDateTime: {reader["RecDateTime"]}, " +
                                    $"TimeLastUpdate: {reader["TimeLastUpdate"]}, " +
                                    $"PatDicom: {reader["converted_patdicom"]}, " +
                                    $"PatDOB: {reader["PatDOB"]}, " +
                                    $"PatID: {reader["PatID"]}, " +
                                    $"PatGender: {reader["PatGender"]}, " +
                                    $"PatAge: {reader["PatAge"]}, " +
                                    $"PatComments: {reader["converted_patcomments"]}," +
                                    $"PatWeight: {reader["PatWeight"]}");


                                Patient p = new Patient();
                                p.RecDateTime = reader["RecDateTime"] as DateTime? ?? default(DateTime);
                                p.TimeLastUpdate = reader["TimeLastUpdate"] as DateTime? ?? default(DateTime);
                                p.PatDicom = reader["converted_patdicom"] as string ?? string.Empty;
                                p.PatDOB = reader["PatDOB"] as string ?? string.Empty;
                                p.PatID = reader["PatID"] as string ?? string.Empty;
                                p.PatGender = reader["PatGender"] as string ?? string.Empty;
                                p.PatAge = reader["PatAge"] as string ?? string.Empty;
                                p.PatWeight = reader["PatWeight"] as string ?? string.Empty;
                                p.PatComments = reader["converted_patcomments"] as string ?? string.Empty;
                                patients.Add(p);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return patients;
        }


        public async Task<List<Patient>> GetPatientsByYearAndMonthAsync(int year, int month)
        {
            var patients = new List<Patient>();

            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string query = @"SELECT RecDateTime, TimeLastUpdate, 
                                    CONVERT(CAST(CONVERT(PatDicom USING latin1) AS BINARY) USING utf8mb4) AS converted_patdicom,
                                    PatDOB, PatID, PatGender, PatAge, PatWeight, 
                                    CONVERT(CAST(CONVERT(PatComments USING latin1) AS BINARY) USING utf8mb4) AS converted_patcomments
                                    FROM patients
                                    WHERE YEAR(TimeLastUpdate) = @Year AND MONTH(TimeLastUpdate) = @Month";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Year", year);
                        command.Parameters.AddWithValue("@Month", month);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {

                                Debug.WriteLine($"RecDateTime: {reader["RecDateTime"]}, " +
                                    $"TimeLastUpdate: {reader["TimeLastUpdate"]}, " +
                                    $"PatDicom: {reader["converted_patdicom"]}, " +
                                    $"PatDOB: {reader["PatDOB"]}, " +
                                    $"PatID: {reader["PatID"]}, " +
                                    $"PatGender: {reader["PatGender"]}, " +
                                    $"PatAge: {reader["PatAge"]}, " +
                                    $"PatComments: {reader["converted_patcomments"]}," +
                                    $"PatWeight: {reader["PatWeight"]}");
                                    

                                Patient p = new Patient();
                                p.RecDateTime = reader["RecDateTime"] as DateTime? ?? default(DateTime);
                                p.TimeLastUpdate = reader["TimeLastUpdate"] as DateTime? ?? default(DateTime);
                                p.PatDicom = reader["converted_patdicom"] as string ?? string.Empty;
                                p.PatDOB = reader["PatDOB"] as string ?? string.Empty;
                                p.PatID = reader["PatID"] as string ?? string.Empty;
                                p.PatGender = reader["PatGender"] as string ?? string.Empty;
                                p.PatAge = reader["PatAge"] as string ?? string.Empty;
                                p.PatWeight = reader["PatWeight"] as string ?? string.Empty;
                                p.PatComments = reader["converted_patcomments"] as string ?? string.Empty;
                                patients.Add(p);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (logging, etc.)
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return patients;
        }
    }
}
