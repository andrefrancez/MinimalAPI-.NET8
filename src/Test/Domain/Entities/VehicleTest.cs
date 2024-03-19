using project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Domain.Entities
{
    [TestClass]
    public class VehicleTest
    {
        [TestMethod]
        public void TestGetSetVehicleProperties()
        {
            var vehicle = new Vehicle();

            vehicle.Id = 5;
            vehicle.Name = "Civic";
            vehicle.Make = "Honda";
            vehicle.ModelYear = 2014;
            vehicle.Color = "Red";
            //vehicle.Description = "Description";

            Assert.AreEqual(5, vehicle.Id);
            Assert.AreEqual("Civic", vehicle.Name);
            Assert.AreEqual("Honda", vehicle.Make);
            Assert.AreEqual(2014, vehicle.ModelYear);
            Assert.AreEqual("Red", vehicle.Color);
            //Assert.AreEqual("Description", vehicle.Description);
        }
    }
}
