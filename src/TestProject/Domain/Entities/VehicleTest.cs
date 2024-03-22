using Moq;
using project.Domain.Entities;

namespace TestProject.Domain.Entities
{
    public class VehicleTest
    {
        [Fact]
        public void TestGetSetVehicleProperties()
        {
            // Arrange
            var mockVehicle = new Mock<Vehicle>();

            // Act
            mockVehicle.Object.Id = 1;
            mockVehicle.Object.Name = "Civic";
            mockVehicle.Object.Make = "Honda";
            mockVehicle.Object.ModelYear = 2014;
            mockVehicle.Object.Color = "Red";
            //mockVehicle.Object.Description = "Description";

            // Assert
            Assert.Equal(1, mockVehicle.Object.Id);
            Assert.Equal("Civic", mockVehicle.Object.Name);
            Assert.Equal("Honda", mockVehicle.Object.Make);
            Assert.Equal(2014, mockVehicle.Object.ModelYear);
            Assert.Equal("Red", mockVehicle.Object.Color);
            //Assert.Equal("Description", mockVehicle.Object.Description);
        }
    }
}
