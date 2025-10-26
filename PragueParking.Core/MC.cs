using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core
{
    public class MC : Vehicle
    {
        public MC(string regNumber, VehicleTypeConfig config) : base(regNumber, config) { }
        public MC() : base() { }
    }
}