using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace PragueParking.Core
{
    // Ett gränssnitt (interface) definierar ett "kontrakt" för vad en klass måste innehålla.
    // Alla fordon, oavsett typ, MÅSTE ha dessa egenskaper.
    public interface IVehicle
    {
        string RegNumber { get; }
        int Size { get; }
        DateTime ArrivalTime { get; }
        string VehicleType { get; }
        double PricePerHour { get; }
    }
}