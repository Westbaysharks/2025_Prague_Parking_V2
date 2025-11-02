using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core
{
    // En "Factory"-klass är ett designmönster som förenklar
    // skapandet av objekt. Vi slipper ha en stor "switch"-sats
    // i vår menykod.
    public static class VehicleFactory
    {
        public static IVehicle? CreateVehicle(string regNumber, VehicleTypeConfig config)
        {
            // Baserat på textsträngen från konfigurationsfilen skapar vi en instans av rätt fordonsklass.

            switch (config.Type.ToUpper())
            {
                case "BIKE":
                    return new Bike(regNumber, config);
                case "MC":
                    return new MC(regNumber, config);
                case "CAR":
                    return new Car(regNumber, config);
                case "BUS":
                    return new Bus(regNumber, config);
                default:
                    return null; // Om fordonstypen inte känns igen
            }
        }

    }
}
