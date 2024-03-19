using project.Domain.Entities;

namespace Test.Domain.Entities
{
    [TestClass]
    public class AdminTest
    {
        [TestMethod]
        public void TestGetSetAdminProperties()
        {
            // Arrange
            var adm = new Admin();

            // Act
            adm.Id = 1;
            adm.Email = "test@test.com";
            adm.Password = "test123";
            adm.Profile = "Editor";

            // Assert
            Assert.AreEqual(1, adm.Id);
            Assert.AreEqual("test@test.com", adm.Email);
            Assert.AreEqual("test123", adm.Password);
            Assert.AreEqual("Editor", adm.Profile);
        }
    }
}
