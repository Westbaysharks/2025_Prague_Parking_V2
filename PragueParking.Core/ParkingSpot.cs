using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace PragueParking.Core
{
    public class ParkingSpot : IParkingSpot
    {
        public int SpotNumber { get; set; }
        public int TotalSize { get; set; }
        public bool IsBusCompatible { get; set; }

        // [JsonIgnore] talar om för JSON-serialiseraren att inte spara
        // denna egenskap, eftersom den kan räknas ut vid behov.
        [JsonIgnore]
        public int OccupiedSize => ParkedVehicles.Sum(v => v.Size);

        public List<IVehicle> ParkedVehicles { get; set; }

        // Konstruktor för att skapa en ny, tom P-plats.
        public ParkingSpot(int number, int size, bool isBusCompatible)
        {
            SpotNumber = number;
            TotalSize = size;
            IsBusCompatible = isBusCompatible;
            ParkedVehicles = new List<IVehicle>();
        }

        // Tom konstruktor för JSON-serialiseraren.
        public ParkingSpot()
        {
            ParkedVehicles = new List<IVehicle>();
        }

        // Kollar om ett fordon får plats.
        public bool CanFit(IVehicle vehicle)
        {
            // Kollar om storleken räcker OCH
            // om fordonet är en buss, kollar att platsen är bus-kompatibel.
            bool hasSpace = (OccupiedSize + vehicle.Size) <= TotalSize;
            bool compatible = (vehicle is Bus) ? IsBusCompatible : true;
            return hasSpace && compatible;
        }

        // Försöker lägga till ett fordon.
        public bool AddVehicle(IVehicle vehicle)
        {
            if (CanFit(vehicle))
            {
                ParkedVehicles.Add(vehicle);
                return true; // Lyckades
            }
            return false; // Misslyckades
        }

        // Tar bort ett fordon baserat på reg-nummer.
        public IVehicle? RemoveVehicle(string regNumber)
        {
            var vehicle = ParkedVehicles.FirstOrDefault(v => v.RegNumber == regNumber.ToUpper());
            if (vehicle != null)
            {
                ParkedVehicles.Remove(vehicle);
            }
            return vehicle;
        }
    }
}
