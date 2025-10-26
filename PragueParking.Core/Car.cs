using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PragueParking.Core
{
    public class Car : Vehicle
    {
        public Car(string regNumber, VehicleTypeConfig config) : base(regNumber, config) { }
        public Car() : base() { }
    }
}
