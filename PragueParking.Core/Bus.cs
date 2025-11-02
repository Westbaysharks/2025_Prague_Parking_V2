using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using System;

namespace PragueParking.Core
{
    public class Bus : Vehicle
    {
        // Konstruktor för att skapa ett nytt fordon
        public Bus(string regNumber, VehicleTypeConfig config) : base(regNumber, config)
        {
        }

        // Konstruktor för JSON-deserialisering
        [JsonConstructor]
        protected Bus() : base()
        {
        }
    }
}
