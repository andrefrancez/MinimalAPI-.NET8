using Moq;
using project.Domain.Entities;

namespace TestProject.Domain.Entities
{
    public class AdminTest
    {
        [Fact]
        public void TestGetSetAdminProperties()
        {
            // Arrange
            var mockAdmin = new Mock<Admin>();

            // Act
            mockAdmin.Object.Id = 1;
            mockAdmin.Object.Email = "test@test.com";
            mockAdmin.Object.Password = "test123";
            mockAdmin.Object.Profile = "Editor";

            // Assert
            Assert.Equal(1, mockAdmin.Object.Id);
            Assert.Equal("test@test.com", mockAdmin.Object.Email);
            Assert.Equal("test123", mockAdmin.Object.Password);
            Assert.Equal("Editor", mockAdmin.Object.Profile);
        }
    }
}
