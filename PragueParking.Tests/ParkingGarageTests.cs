using Microsoft.VisualStudio.TestTools.UnitTesting;
using PragueParking.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;

namespace PragueParking.Tests
{
    [TestClass]
    public class ParkingGarageTests
    {
        // En hjälpmetod för att skapa standard-inställningar för testerna
        public Settings GetTestSettings()
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
        public void Test2_MoveVehicle_Successful()
        {
            // --- ARRANGE (Förbered) ---
            var settings = GetTestSettings();
            var garage = new ParkingGarage(settings, new List<IParkingSpot>());
            var carConfig = settings.VehicleTypes.First(v => v.Type == "Car");
            var car = VehicleFactory.CreateVehicle("CAR-001", carConfig);

            // Parkera bilen (hamnar på plats 1, index 0)
            garage.ParkVehicle(car);

            // --- ACT (Agera) ---
            // Flytta från plats 1 till (den tomma) plats 2
            string result = garage.MoveVehicle("CAR-001", 2);

            // --- ASSERT (Kontrollera) ---
            Assert.IsTrue(result.Contains("Moved"), "Meddelandet ska vara 'Moved'.");

            var oldSpot = garage.Spots[0]; // Plats 1 (index 0)
            var newSpot = garage.Spots[1]; // Plats 2 (index 1)

            // Kontrollera att plats 1 nu är tom
            Assert.AreEqual(0, oldSpot.OccupiedSize, "Gamla platsen (1) ska nu vara tom.");

            // Kontrollera att plats 2 har bilen
            Assert.AreEqual(car.Size, newSpot.OccupiedSize, "Nya platsen (2) ska nu vara upptagen av bilen.");
            Assert.AreEqual("CAR-001", newSpot.ParkedVehicles.First().RegNumber, "Rätt bil ska finnas på nya platsen.");
        }

        [TestMethod]
        public void Test3_MoveVehicle_Fail_SpotIsFull()
        {
            // --- ARRANGE (Förbered) ---
            var settings = GetTestSettings();
            var garage = new ParkingGarage(settings, new List<IParkingSpot>());
            var carConfig = settings.VehicleTypes.First(v => v.Type == "Car");

            var car1 = VehicleFactory.CreateVehicle("CAR-001", carConfig);
            var car2 = VehicleFactory.CreateVehicle("CAR-002", carConfig);

            garage.ParkVehicle(car1); // Parkerar på plats 1
            garage.ParkVehicle(car2); // Parkerar på plats 2

            // --- ACT (Agera) ---
            // Försök flytta CAR-001 till plats 2, som redan är upptagen av CAR-002
            string result = garage.MoveVehicle("CAR-001", 2);

            // --- ASSERT (Kontrollera) ---
            Assert.IsTrue(result.StartsWith("Error"), "Resultatet ska vara ett felmeddelande.");
            Assert.IsTrue(result.Contains("does not fit"), "Felet ska vara att bilen 'inte får plats'.");

            var spot1 = garage.Spots[0]; // Plats 1 (index 0)
            var spot2 = garage.Spots[1]; // Plats 2 (index 1)

            // Kontrollera att CAR-001 är kvar på plats 1
            Assert.AreEqual(1, spot1.ParkedVehicles.Count, "Plats 1 ska fortfarande ha ett fordon.");
            Assert.AreEqual("CAR-001", spot1.ParkedVehicles.First().RegNumber, "CAR-001 ska vara kvar på plats 1.");

            // Kontrollera att CAR-002 är kvar på plats 2
            Assert.AreEqual(1, spot2.ParkedVehicles.Count, "Plats 2 ska fortfarande ha ett fordon.");
            Assert.AreEqual("CAR-002", spot2.ParkedVehicles.First().RegNumber, "CAR-002 ska vara kvar på plats 2.");
        }
    }
}
