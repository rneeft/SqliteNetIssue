using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Xamarin.Essentials;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App1
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        
        private static readonly Func<string, bool> IsSqlFile = x => x.EndsWith(".sql", StringComparison.OrdinalIgnoreCase);

        async private void Button_Click(object sender, RoutedEventArgs e)
        {
            var assembly = typeof(Program).Assembly;
            var files = assembly
                .GetManifestResourceNames()
                .Where(IsSqlFile)
                .OrderBy(x => x)
                .ToList();

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "TheDatabase.db");

            if (File.Exists(databasePath))
                File.Delete(databasePath);

            var connection = new SQLiteAsyncConnection(databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

            foreach (var file in files)
            {
                Console.WriteLine($"Executing file '{file}'");
                var contents = string.Empty;
                using (var stream = assembly.GetManifestResourceStream(file))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                {
                    contents = await reader.ReadToEndAsync();
                }

                await connection.RunInTransactionAsync(y =>
                {
                    string sql = string.Empty;
                    try
                    {
                        contents.Split(';', StringSplitOptions.RemoveEmptyEntries)
                             .Select(x => x.Trim())
                             .Where(x => !string.IsNullOrWhiteSpace(x))
                             .ToList()
                             .ForEach(l =>
                             {
                                 sql = l;
                                 y.Execute(sql);
                             });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception" + ex.Message);

                        // ending up here... is failed.
                        Debugger.Break();
                        throw;
                    }
                });
            }

            await connection.CloseAsync();

            // ending up here... it works
            Debugger.Break();
        }
    }
}
