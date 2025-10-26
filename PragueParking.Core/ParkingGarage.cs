using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core
{
    // Huvudklassen som hanterar all logik för garaget.
    public class ParkingGarage
    {
        public List<IParkingSpot> Spots { get; private set; }
        public Settings Settings { get; private set; }

        // Konstruktorn tar emot inställningar och en befintlig lista med platser
        // (t.ex. inläst från fil).
        public ParkingGarage(Settings settings, List<IParkingSpot> spots)
        {
            Settings = settings;
            Spots = spots;
            // Om listan är tom (t.ex. första körningen) måste vi initiera den.
            InitializeSpots();
        }

        // Skapar upp alla P-platser enligt inställningarna.
        private void InitializeSpots()
        {
            // Om Spots redan har innehåll (från fil), gör inget.
            if (Spots.Any())
            {
                // Se bara till att storleken stämmer med nya settings
                foreach (var spot in Spots)
                {
                    
                }
                return;
            }

            // Annars, skapa en ny lista med tomma platser.
            for (int i = 0; i < Settings.TotalSpots; i++)
            {
                int spotNumber = i + 1;
                //Endast de första 50 platserna (index 0-49) är för bussar
                bool isBusCompatible = i < 50;
                Spots.Add(new ParkingSpot(spotNumber, Settings.SpotSize, isBusCompatible));
            }
        }

        // Försöker parkera ett fordon.
        public string ParkVehicle(IVehicle vehicle)
        {
            // 1. Försök hitta en plats där fordonet kan dela med andra
            // (t.ex. en MC på en plats där det redan står en MC).
            var spotToShare = Spots.FirstOrDefault(s => s.OccupiedSize > 0 && s.CanFit(vehicle));
            if (spotToShare != null)
            {
                spotToShare.AddVehicle(vehicle);
                return $"Parked {vehicle.VehicleType} ({vehicle.RegNumber}) at spot {spotToShare.SpotNumber} (sharing).";
            }

            // 2. Om ingen delad plats hittas, hitta första helt tomma plats.
            var emptySpot = Spots.FirstOrDefault(s => s.OccupiedSize == 0 && s.CanFit(vehicle));
            if (emptySpot != null)
            {
                emptySpot.AddVehicle(vehicle);
                return $"Parked {vehicle.VehicleType} ({vehicle.RegNumber}) at spot {emptySpot.SpotNumber}.";
            }

            // 3. Om fordonet är en buss, och den inte fick plats
            if (vehicle is Bus)
            {
                return "Error: No compatible (first 50) and empty spot available for the bus.";
            }

            // 4. Om det inte är en buss, men garaget är fullt.
            return "Error: The parking garage is full.";
        }

        // Försöker checka ut ett fordon.
        public string CheckoutVehicle(string regNumber)
        {
            var spot = FindSpotByRegNumber(regNumber);
            if (spot == null)
            {
                return "Error: Vehicle not found.";
            }

            var vehicle = spot.RemoveVehicle(regNumber);
            if (vehicle == null)
            {
                // Detta bör "aldrig" hända om FindSpotByRegNumber fungerar,
                // men det är bra att ha en extra koll.
                return "Error: Vehicle found but could not be removed.";
            }

            // Beräkna kostnaden
            double price = CalculatePrice(vehicle);
            TimeSpan parkedTime = DateTime.Now - vehicle.ArrivalTime;

            return $"Checked out {vehicle.VehicleType} ({vehicle.RegNumber}).\n" +
                   $"Parked for: {parkedTime.Hours}h {parkedTime.Minutes}m\n" +
                   $"Total cost: {price:F2} CZK";
        }

        // Beräknar priset för ett fordon
        public double CalculatePrice(IVehicle vehicle)
        {
            TimeSpan parkedDuration = DateTime.Now - vehicle.ArrivalTime;

            // Kolla om det är gratis
            if (parkedDuration.TotalMinutes <= Settings.FreeMinutes)
            {
                return 0.0;
            }

            // Annars, räkna ut "påbörjade timmar".
            // Math.Ceiling() avrundar ALLTID uppåt.
            // 1.1 timmar -> 2.0
            // 0.8 timmar -> 1.0
            double totalHours = parkedDuration.TotalHours;
            double billableHours = Math.Ceiling(totalHours);

            return billableHours * vehicle.PricePerHour;
        }

        // Försöker flytta ett fordon
        public string MoveVehicle(string regNumber, int newSpotNumber)
        {
            var oldSpot = FindSpotByRegNumber(regNumber);
            if (oldSpot == null)
            {
                return "Error: Vehicle not found.";
            }

            // Kontrollera om den nya platsen existerar
            if (newSpotNumber < 1 || newSpotNumber > Spots.Count)
            {
                return $"Error: Spot {newSpotNumber} does not exist.";
            }

            var newSpot = Spots[newSpotNumber - 1]; // -1 för att listor är 0-indexerade

            // Hämta fordonet utan att ta bort det än
            var vehicle = oldSpot.ParkedVehicles.FirstOrDefault(v => v.RegNumber == regNumber.ToUpper());
            if (vehicle == null) return "Error: Vehicle not found (internal error).";

            // Kolla om fordonet får plats på den nya platsen
            if (newSpot.CanFit(vehicle))
            {
                // Ja, det får plats. Flytta det.
                oldSpot.RemoveVehicle(regNumber);
                newSpot.AddVehicle(vehicle);
                return $"Moved {vehicle.VehicleType} ({vehicle.RegNumber}) from spot {oldSpot.SpotNumber} to {newSpot.SpotNumber}.";
            }
            else
            {
                return $"Error: Vehicle ({vehicle.VehicleType}) does not fit at spot {newSpotNumber}.";
            }
        }

        // Söker efter ett fordon och returnerar det.
        public IVehicle? FindVehicle(string regNumber)
        {
            return FindSpotByRegNumber(regNumber)?
                .ParkedVehicles
                .FirstOrDefault(v => v.RegNumber == regNumber.ToUpper());
        }

        // Hjälpmetod för att hitta P-platsen ett fordon står på.
        private IParkingSpot? FindSpotByRegNumber(string regNumber)
        {
            string upperReg = regNumber.ToUpper();
            return Spots.FirstOrDefault(s => s.ParkedVehicles.Any(v => v.RegNumber == upperReg));
        }

        // Ladda om inställningar
        public bool UpdateSettings(Settings newSettings)
        {
            // Validering: Kolla om vi minskar antalet platser
            if (newSettings.TotalSpots < this.Settings.TotalSpots)
            {
                // Vi ska ta bort platser. Kolla om några av dem är upptagna.
                // Ex: Går från 100 till 80. Vi måste kolla plats 81-100 (index 80-99).
                for (int i = newSettings.TotalSpots; i < this.Settings.TotalSpots; i++)
                {
                    if (Spots[i].OccupiedSize > 0)
                    {
                        // Kan inte ta bort platser, en plats som ska tas bort är upptagen.
                        return false;
                    }
                }
                // Ok, platserna som ska tas bort var tomma. Ta bort dem.
                Spots.RemoveRange(newSettings.TotalSpots, this.Settings.TotalSpots - newSettings.TotalSpots);
            }
            // Kolla om vi lägger till platser
            else if (newSettings.TotalSpots > this.Settings.TotalSpots)
            {
                for (int i = this.Settings.TotalSpots; i < newSettings.TotalSpots; i++)
                {
                    int spotNumber = i + 1;
                    bool isBusCompatible = i < 50; // Samma regel gäller
                    Spots.Add(new ParkingSpot(spotNumber, newSettings.SpotSize, isBusCompatible));
                }
            }

            // Uppdatera storleken på alla befintliga, *tomma* platser.
            // (Vi kan inte ändra storlek på en upptagen plats).
            foreach (var spot in Spots)
            {
                if (spot.OccupiedSize == 0)
                {
                    // Detta är en ful-lösning. Vi borde haft en 'SetSize'-metod
                    // på ParkingSpot, men detta funkar för en nybörjare.
                    var newSpot = new ParkingSpot(spot.SpotNumber, newSettings.SpotSize, spot.IsBusCompatible);
                    Spots[spot.SpotNumber - 1] = newSpot;
                }
            }

            // Sätt de nya inställningarna.
            this.Settings = newSettings;
            return true;
        }
    }
}
