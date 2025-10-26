using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core
{
    public class Bus : Vehicle
    {
        public Bus(string regNumber, VehicleTypeConfig config) : base(regNumber, config) { }
        public Bus() : base() { }
    }
}
