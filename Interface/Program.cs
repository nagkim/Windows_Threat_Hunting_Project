using System;
//using System.Data.SQLite; // Add this for SQLite support
using System.Windows.Forms;

namespace Interface
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Initialize the application
            

            Application.SetCompatibleTextRenderingDefault(false);

            // Test SQLite connection
           // TestSQLiteConnection();

            // Run the main form
            Application.Run(new Form1());
        }

        /// <summary>
        /// Tests the SQLite database connection
        /// </summary>
        //private static void TestSQLiteConnection()
        //{
        //    string dbPath = @"Data Source=C:\Users\nagkim\Documents\database.db;"; // Update the file path as needed

        //    try
        //    {
        //        using (SQLiteConnection connection = new SQLiteConnection(dbPath))
        //        {
        //            connection.Open();
        //            Console.WriteLine("SQLite database connected successfully!");

                    
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"SQLite connection failed: {ex.Message}");
        //    }
        //}
    }
}