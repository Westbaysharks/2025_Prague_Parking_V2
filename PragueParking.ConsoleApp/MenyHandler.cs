using PragueParking.Core;
using PragueParking.Data;
using Spectre.Console;
using Spectre.Console.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PragueParking.ConsoleApp
{
    // Denna klass innehåller all logik för att visa menyer och
    // interagera med användaren.
    public class MenuHandler
    {
        private readonly ParkingGarage _garage;
        private readonly DataAccess _dataAccess;
        private readonly string _settingsPath;
        private readonly string _dataPath;

        public MenuHandler(ParkingGarage garage, DataAccess dataAccess, string settingsPath, string dataPath)
        {
            _garage = garage;
            _dataAccess = dataAccess;
            _settingsPath = settingsPath;
            _dataPath = dataPath;
        }

        // Huvudmenyn
        public void ShowMainMenu()
        {
            bool keepRunning = true;
            while (keepRunning)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new FigletText("Prague Parking").Color(Color.Yellow));
                ShowGarageOverview(); // Visa översikt

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Main Menu")
                        .PageSize(10)
                        .AddChoices(new[]
                        {
                            "Park Vehicle", "Checkout Vehicle", "Move Vehicle",
                            "Search for Vehicle", "Show Garage Map", "Reload Settings", "Exit"
                        }));

                switch (choice)
                {
                    case "Park Vehicle":
                        ParkVehicle();
                        break;
                    case "Checkout Vehicle":
                        CheckoutVehicle();
                        break;
                    case "Move Vehicle":
                        MoveVehicle();
                        break;
                    case "Search for Vehicle":
                        SearchVehicle();
                        break;
                    case "Show Garage Map":
                        ShowGarageMap();
                        break;
                    case "Reload Settings":
                        ReloadSettings();
                        break;
                    case "Exit":
                        keepRunning = false;
                        break;
                }
            }
        }

        // Visar en snabb översikt över lediga platser
        private void ShowGarageOverview()
        {
            int totalSpots = _garage.Spots.Count;
            int occupiedSpots = _garage.Spots.Count(s => s.OccupiedSize > 0);
            int freeSpots = totalSpots - occupiedSpots;

            var stats = new BreakdownChart()
                .Width(60)
                .AddItem("Occupied", occupiedSpots, Color.Red)
                .AddItem("Free", freeSpots, Color.Green);

            AnsiConsole.Write(new Panel(stats).Header("Garage Status"));
        }

        // Logik för att parkera fordon
        private void ParkVehicle()
        {
            var vehicleTypeConfig = AnsiConsole.Prompt(
                new SelectionPrompt<VehicleTypeConfig>()
                    .Title("Which type of vehicle?")
                    .UseConverter(config => $"{config.Type} (Size: {config.Size}, {config.PricePerHour} CZK/h)")
                    .AddChoices(_garage.Settings.VehicleTypes));

            var regNumber = AnsiConsole.Ask<string>("Enter registration number:").ToUpper();

            if (_garage.FindVehicle(regNumber) != null)
            {
                AnsiConsole.MarkupLine($"[red]Error: Vehicle {regNumber} is already parked.[/]");
                Pause();
                return;
            }

            var vehicle = VehicleFactory.CreateVehicle(regNumber, vehicleTypeConfig);
            if (vehicle == null)
            {
                AnsiConsole.MarkupLine("[red]Error: Could not create vehicle.[/]");
                Pause();
                return;
            }

            string result = _garage.ParkVehicle(vehicle);
            AnsiConsole.MarkupLine($"[green]{result}[/]");

            // Spara ändringen till fil
            _dataAccess.SaveData(_garage.Spots, _dataPath);
            Pause();
        }

        // Logik för att checka ut
        private void CheckoutVehicle()
        {
            var regNumber = AnsiConsole.Ask<string>("Enter registration number:").ToUpper();
            string result = _garage.CheckoutVehicle(regNumber);

            if (result.StartsWith("Error"))
            {
                AnsiConsole.MarkupLine($"[red]{result}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[green]{result}[/]");
                // Spara ändringen till fil
                _dataAccess.SaveData(_garage.Spots, _dataPath);
            }
            Pause();
        }

        // Logik för att flytta fordon
        private void MoveVehicle()
        {
            var regNumber = AnsiConsole.Ask<string>("Enter registration number:").ToUpper();
            var newSpot = AnsiConsole.Ask<int>($"Enter new spot number (1-{_garage.Spots.Count}):");

            string result = _garage.MoveVehicle(regNumber, newSpot);

            if (result.StartsWith("Error"))
            {
                AnsiConsole.MarkupLine($"[red]{result}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[green]{result}[/]");
                // Spara ändringen till fil
                _dataAccess.SaveData(_garage.Spots, _dataPath);
            }
            Pause();
        }

        // Logik för att söka
        private void SearchVehicle()
        {
            var regNumber = AnsiConsole.Ask<string>("Enter registration number:").ToUpper();
            var spot = _garage.Spots.FirstOrDefault(s => s.ParkedVehicles.Any(v => v.RegNumber == regNumber));

            if (spot == null)
            {
                AnsiConsole.MarkupLine($"[red]Vehicle {regNumber} not found.[/]");
            }
            else
            {
                var vehicle = spot.ParkedVehicles.First(v => v.RegNumber == regNumber);
                TimeSpan parkedTime = DateTime.Now - vehicle.ArrivalTime;
                AnsiConsole.MarkupLine($"[green]Vehicle {regNumber} ({vehicle.VehicleType})[/] is parked at [yellow]spot {spot.SpotNumber}[/].");
                AnsiConsole.MarkupLine($"Arrived at: {vehicle.ArrivalTime}");
                AnsiConsole.MarkupLine($"Parked for: {parkedTime.Hours}h {parkedTime.Minutes}m");
            }
            Pause();
        }

        // Visa en karta över garaget
        private void ShowGarageMap()
        {
            AnsiConsole.Clear();
            var table = new Table().Border(TableBorder.Rounded).Expand();

            // Skapa 10 kolumner
            for (int i = 0; i < 10; i++) table.AddColumn($"[bold]C{i + 1}[/]");

            // Fyll tabellen med rader (10 platser per rad)
            var spots = _garage.Spots.ToList();
            for (int i = 0; i < spots.Count; i += 10)
            {
                var rowItems = new IRenderable[10];
                for (int j = 0; j < 10; j++)
                {
                    int idx = i + j;
                    if (idx >= spots.Count)
                    {
                        rowItems[j] = new Panel(" ");
                        continue;
                    }

                    var spot = spots[idx];
                    string content;
                    string color;

                    if (spot.OccupiedSize == 0)
                    {
                        content = $"P{spot.SpotNumber}\n[green]Free[/]";
                        color = "green";
                    }
                    else if (spot.OccupiedSize < spot.TotalSize)
                    {
                        content = $"P{spot.SpotNumber}\n[yellow]{spot.OccupiedSize}/{spot.TotalSize}[/]";
                        color = "yellow";
                    }
                    else
                    {
                        content = $"P{spot.SpotNumber}\n[red]Full[/]";
                        color = "red";
                    }

                    if (spot.IsBusCompatible) content += "\n[grey](Bus)[/]";

                    rowItems[j] = new Panel(content).BorderColor(Color.Parse(color));
                }
                table.AddRow(rowItems);
            }

            AnsiConsole.Write(table);
            Pause();
        }

        // Ladda om inställningar
        private void ReloadSettings()
        {
            if (!AnsiConsole.Confirm("Are you sure you want to reload settings from settings.json?"))
            {
                return;
            }

            try
            {
                var newSettings = _dataAccess.LoadSettings(_settingsPath);
                bool success = _garage.UpdateSettings(newSettings);

                if (success)
                {
                    AnsiConsole.MarkupLine("[green]Settings reloaded successfully.[/]");
                    AnsiConsole.MarkupLine($"[green]Total spots: {_garage.Settings.TotalSpots}[/]");
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Error: Could not reload settings.[/]");
                    AnsiConsole.MarkupLine("[red]Reason: Cannot reduce number of spots while spots being removed are occupied.[/]");
                    // Återställ till de gamla inställningarna
                    _garage.UpdateSettings(_garage.Settings);
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]An error occurred while reading settings.json:[/]");
                AnsiConsole.WriteException(ex);
            }
            Pause();
        }

        // Hjälpmetod för att pausa skärmen
        private void Pause()
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
        }
    }
}
