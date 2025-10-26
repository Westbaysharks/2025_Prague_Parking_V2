using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;  

using PragueParking.Core;
using PragueParking.Data;
using System.IO;

namespace PragueParking.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Sätt filvägar. AppContext.BaseDirectory är där .exe-filen körs.
            string settingsPath = Path.Combine(AppContext.BaseDirectory, "settings.json");
            string dataPath = Path.Combine(AppContext.BaseDirectory, "parkedvehicles.json");

            try
            {
                // 1. Skapa data-access lagret
                var dataAccess = new DataAccess();

                // 2. Läs in inställningar (skapar fil om den saknas)
                var settings = dataAccess.LoadSettings(settingsPath);

                // 3. Läs in parkeringsdata (skapar test-data om fil saknas)
                var spots = dataAccess.LoadData(dataPath, settings);

                // 4. Skapa garaget med inställningar och data
                var garage = new ParkingGarage(settings, spots);

                // 5. Skapa och starta menyn
                var menu = new MenuHandler(garage, dataAccess, settingsPath, dataPath);
                menu.ShowMainMenu();
            }
            catch (Exception ex)
            {
                // Fånga eventuella oväntade fel
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("A critical error occurred and the application must close.");
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
        }
    }
}
