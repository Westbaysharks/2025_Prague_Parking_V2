using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PragueParking.Core
{
    public class MC : Vehicle
    {
        // Konstruktor för att skapa ett nytt fordon
        public MC(string regNumber, VehicleTypeConfig config) : base(regNumber, config)
        {
        }

        // Konstruktor för JSON-deserialisering
        [JsonConstructor]
        protected MC() : base()
        {
        }
    }
}