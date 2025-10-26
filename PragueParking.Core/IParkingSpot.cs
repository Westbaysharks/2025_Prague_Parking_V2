using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core
{
    // Kontrakt för vad en parkeringsplats måste kunna göra.
    public interface IParkingSpot
    {
        int SpotNumber { get; }
        int TotalSize { get; }
        bool IsBusCompatible { get; }
        int OccupiedSize { get; }
        List<IVehicle> ParkedVehicles { get; }

        bool AddVehicle(IVehicle vehicle);
        IVehicle? RemoveVehicle(string regNumber);
        bool CanFit(IVehicle vehicle);
    }
}