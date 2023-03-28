using Microsoft.Data.Sqlite;
using Microsoft.VisualBasic.FileIO;
using System.Data;
using System.Globalization;
using System.Runtime.CompilerServices;

internal class Program
{
    static string connectionString = "Data Source=HabitTracker.db";

    private static void Main(string[] args)
    {
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var tableCmd = connection.CreateCommand();

            tableCmd.CommandText = @"CREATE TABLE IF NOT EXISTS drinking_water (
                                 Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                 Date TEXT,
                                 Quantity INTEGER)";

            tableCmd.ExecuteNonQuery();

            connection.Close();
        }

        GetUserInput();

        static void GetUserInput()
        {
            bool closeApp = false;
            while (closeApp == false)
            {
                Console.WriteLine("\nMAIN MENU");
                Console.WriteLine("\nWhat would you like to do ?\n");
                Console.WriteLine("- Type 0 to Close the Application.");
                Console.WriteLine("- Type 1 to View All Records.");
                Console.WriteLine("- Type 2 to Insert Record.");
                Console.WriteLine("- Type 3 to Delete Record.");
                Console.WriteLine("- Type 4 to Update Record.\n\n");

                string commandInput = Console.ReadLine();

                switch (commandInput)
                {
                    case "0":
                        Console.WriteLine("\nGoodbye !");
                        closeApp = true;
                        Environment.Exit(0);
                        break;
                    case "1":
                        GetAllRecords();
                        break;
                    case "2":
                        Insert();
                        break;
                    case "3":
                        Delete();
                        break;
                    case "4":
                        Update();
                        break;
                    default:
                        Console.WriteLine("\n|---> Invalid Command. Please type a number from 0 to 4 <---|\n");
                        break;
                }
            }
        }

        static void Insert()
        {
            Console.Clear();

            string date = GetDateInput();

            int quantity = GetNumberInput("Please insert number of glasses or other measure of your choice" +
                                          "(no decimals allowed)\n\nType 0 to return to the menu.");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText =
                      $"INSERT INTO drinking_water(date, quantity) VALUES('{date}', {quantity})";
                tableCmd.ExecuteNonQuery();
                connection.Close();
            }
        }

        static int GetNumberInput(string message)
        {
            Console.WriteLine(message);

            string numberInput = Console.ReadLine();

            if (numberInput == "0") GetUserInput();

            while (!Int32.TryParse(numberInput, out _) || Convert.ToInt32(numberInput) < 0) 
            {
                Console.WriteLine("\n|---> Invalid number <---|\n");
                numberInput = Console.ReadLine();
            }

            int finalInput = Convert.ToInt32(numberInput);

            return finalInput;
        }

        static string GetDateInput()
        {
            Console.Clear();

            Console.WriteLine("Please insert the date: (Format: dd-mm-yy).\n\nType 0 to return to main menu");

            string dateInput = Console.ReadLine();

            if (dateInput == "0") GetUserInput();

            while(!DateTime.TryParseExact(dateInput, "dd-MM-yy", new CultureInfo("en-US"), DateTimeStyles.None, out _))
            {
                Console.WriteLine("\n|---> Invalid date (Format: dd-mm-yy) Try again <---|\n");
                dateInput = Console.ReadLine();
            }

            return dateInput;
        }

        static void GetAllRecords()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = "SELECT * FROM drinking_water";

                List<DrinkingWater> tableData = new();

                SqliteDataReader reader = tableCmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while(reader.Read())
                    {
                        tableData.Add(new DrinkingWater
                        {
                            Id = reader.GetInt32(0),
                            Date = DateTime.ParseExact(reader.GetString(1), "dd-MM-yy", new CultureInfo("en-US")),
                            Quantity = reader.GetInt32(2)
                        });
                    }
                }
                else
                {
                    Console.WriteLine("No rows found");
                }

                connection.Close();

                Console.WriteLine("-----------------------------------------------------\n");
                foreach (var drinkingWater in tableData)
                {
                    Console.WriteLine($"{drinkingWater.Id} - {drinkingWater.Date.ToString("dd-MMM-yyyy")} - Quantity: {drinkingWater.Quantity}");
                }
                Console.WriteLine("\n-----------------------------------------------------");
            }
        }

        static void Delete()
        {
            GetAllRecords();

            var recordId = GetNumberInput("\n\nPlease type the Id of the record you want to delete");

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = $"DELETE from drinking_water WHERE Id = '{recordId}'";

                int rowCount = tableCmd.ExecuteNonQuery();

                if( rowCount == 0 )
                {
                    Console.WriteLine($"\n|---> Record with Id {recordId} doesn't exist <---|\n");
                    Delete();
                }
            }
            Console.WriteLine($"\n\nRecord with Id {recordId} has been deleted.\n\n");
            GetUserInput();
        }

        static void Update()
        {
            GetAllRecords();
            var recordId = GetNumberInput("\n\nPlease type the Id of the record you would like to update, or type 0 to get back to the menu.");

            using (var connection = new SqliteConnection(connectionString)) 
            {
                connection.Open();

                var checkCmd = connection.CreateCommand();
                checkCmd.CommandText = $"SELECT EXISTS(SELECT 1 FROM drinking_water WHERE Id = {recordId})";
                int checkQuery = Convert.ToInt32(checkCmd.ExecuteScalar());

                if( checkQuery == 0 )
                {
                    Console.WriteLine($"\n|---> Record with Id {recordId} doesn't exist <---|\n");
                    connection.Close();
                    Update();
                }

                string date = GetDateInput();

                int quantity = GetNumberInput("\n\nPlease insert number of glasses of other measure of your choice.");

                var tableCmd = connection.CreateCommand();
                tableCmd.CommandText = $"UPDATE drinking_water SET date = '{date}', quantity = {quantity} WHERE Id = {recordId}";

                tableCmd.ExecuteNonQuery();
                connection.Close();
            }
        }
    }
}

public class DrinkingWater
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int Quantity { get; set; }
}