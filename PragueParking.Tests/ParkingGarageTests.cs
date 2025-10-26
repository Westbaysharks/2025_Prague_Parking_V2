using Microsoft.VisualStudio.TestTools.UnitTesting;
using PragueParking.Core;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PragueParking.Tests
{
    [TestClass]
    public class ParkingGarageTests
    {
        // En hjälpmetod för att skapa standard-inställningar för testerna
        private Settings GetTestSettings()
        {
            return new Settings
            {
                TotalSpots = 100,
                SpotSize = 4,
                FreeMinutes = 10,
                VehicleTypes = new List<VehicleTypeConfig>
                {
                    new VehicleTypeConfig { Type = "Car", Size = 4, PricePerHour = 20 },
                    new VehicleTypeConfig { Type = "MC", Size = 2, PricePerHour = 10 }
                }
            };
        }

        [TestMethod]
        public void Test1_ParkVehicle_Successful()
        {
            // --- ARRANGE (Förbered) ---
            var settings = GetTestSettings();
            var garage = new ParkingGarage(settings, new List<IParkingSpot>());
            var carConfig = settings.VehicleTypes.First(v => v.Type == "Car");
            var car = VehicleFactory.CreateVehicle("TEST-001", carConfig);

            // --- ACT (Agera) ---
            string result = garage.ParkVehicle(car);
            var parkedVehicle = garage.FindVehicle("TEST-001");

            // --- ASSERT (Kontrollera) ---
            Assert.IsNotNull(parkedVehicle); // Bilen ska hittas
            Assert.AreEqual("TEST-001", parkedVehicle.RegNumber); // Reg-numret ska stämma
            Assert.IsTrue(result.Contains("Parked")); // Meddelandet ska vara positivt
        }

        [TestMethod]
        public void Test2_CheckoutVehicle_CalculatesPriceCorrectly()
        {
            // --- ARRANGE (Förbered) ---
            var settings = GetTestSettings();
            // Sätt gratisminuter till 0 för att förenkla pris-testet
            settings.FreeMinutes = 0;
            var garage = new ParkingGarage(settings, new List<IParkingSpot>());
            var mcConfig = settings.VehicleTypes.First(v => v.Type == "MC");

            // Skapa en MC manuellt för att kunna sätta ankomsttiden
            var mc = new MC("TEST-MC1", mcConfig);

            // Reflection är ett sätt att ändra privata värden,
            // vilket är användbart i tester. Vi sätter ankomsttiden
            // till 1 timme och 30 minuter sedan.
            // Priset ska då vara för 2 "påbörjade" timmar.
            var arrivalTimeProperty = typeof(Vehicle).GetProperty("ArrivalTime");
            arrivalTimeProperty.SetValue(mc, DateTime.Now.AddMinutes(-90)); // 1.5 timmar sedan

            garage.ParkVehicle(mc);

            // --- ACT (Agera) ---
            string result = garage.CheckoutVehicle("TEST-MC1");

            // --- ASSERT (Kontrollera) ---
            // Pris: 10 CZK/timme. Påbörjade timmar: 2. Totalpris: 20.
            bool correctPrice = result.Contains("Total cost: 20.00 CZK");
            Assert.IsTrue(correctPrice);
        }
    }
}
