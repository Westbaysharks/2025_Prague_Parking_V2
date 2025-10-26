using Newtonsoft.Json;
using PragueParking.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

// 1. Rätt namnrymd
namespace PragueParking.Data
{
    // 2. Rätt klassnamn
    public class DataAccess
    {
        // Inställningar för JSON-hantering så att den kan spara/läsa interfaces (IVehicle, IParkingSpot)
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto, // Viktig för att hantera IVehicle/IParkingSpot
            Formatting = Formatting.Indented
        };

        // Laddar inställningar från settings.json
        public Settings LoadSettings(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    // Skapa en standard-fil om den inte finns
                    var defaultSettings = new Settings();
                    defaultSettings.VehicleTypes = new List<VehicleTypeConfig>
                    {
                        new VehicleTypeConfig { Type = "Bike", Size = 1, PricePerHour = 5 },
                        new VehicleTypeConfig { Type = "MC", Size = 2, PricePerHour = 10 },
                        new VehicleTypeConfig { Type = "Car", Size = 4, PricePerHour = 20 },
                        new VehicleTypeConfig { Type = "Bus", Size = 16, PricePerHour = 80 }
                    };
                    // Använd standardvärden från Settings-klassen om de finns
                    // defaultSettings.TotalSpots = new Settings().TotalSpots; // Redan 100 som standard
                    // defaultSettings.SpotSize = new Settings().SpotSize;     // Redan 4 som standard
                    // defaultSettings.FreeMinutes = new Settings().FreeMinutes; // Redan 10 som standard
                    SaveSettings(defaultSettings, path);
                    return defaultSettings;
                }

                string json = File.ReadAllText(path);
                // Returnera ett nytt Settings-objekt om deserialiseringen misslyckas
                return JsonConvert.DeserializeObject<Settings>(json, _jsonSettings) ?? new Settings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}. Returning default settings.");
                // Returnera standardinställningar vid fel
                return new Settings();
            }
        }

        // Sparar inställningar till settings.json
        public void SaveSettings(Settings settings, string path)
        {
            try
            {
                string json = JsonConvert.SerializeObject(settings, _jsonSettings);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        // Laddar parkeringsdata (parkedvehicles.json)
        public List<IParkingSpot> LoadData(string path, Settings settings) // settings behövs inte här längre
        {
            try
            {
                if (!File.Exists(path))
                {
                    // Returnera en tom lista om filen inte finns (garaget skapar nya platser)
                    return new List<IParkingSpot>();
                }

                string json = File.ReadAllText(path);

                // Deserialisera till den konkreta typen ParkingSpot
                var spots = JsonConvert.DeserializeObject<List<ParkingSpot>>(json, _jsonSettings);

                // Konvertera List<ParkingSpot> till List<IParkingSpot> för retur
                // Returnera tom lista om deserialiseringen misslyckas
                return spots?.ToList<IParkingSpot>() ?? new List<IParkingSpot>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading parking data: {ex.Message}. Returning empty list.");
                // Returnera tom lista vid fel
                return new List<IParkingSpot>();
            }
        }

        // Sparar parkeringsdata (parkedvehicles.json)
        public void SaveData(List<IParkingSpot> spots, string path)
        {
            try
            {
                string json = JsonConvert.SerializeObject(spots, _jsonSettings);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                // Hantera eventuella skrivfel
                Console.WriteLine($"Error saving data: {ex.Message}");
            }
        }
    }
}