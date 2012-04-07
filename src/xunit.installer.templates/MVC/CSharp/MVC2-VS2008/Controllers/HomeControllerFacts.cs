using System.Web.Mvc;
using Xunit;
using $mvcprojectname$.Controllers;

namespace $safeprojectname$.Controllers
{
    public class HomeControllerFacts
    {
        public class Index
        {
			[Fact]
			public void ReturnsViewResultWithDefaultViewName()
			{
				// Arrange
                var controller = new HomeController();

				// Act
                var result = controller.Index();

				// Assert
				var viewResult = Assert.IsType<ViewResult>(result);
				Assert.Empty(viewResult.ViewName);
			}

            [Fact]
            public void SetsViewDataWithNoModel()
            {
				// Arrange
                var controller = new HomeController();

                // Act
                var result = controller.Index();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Equal("Welcome to ASP.NET MVC!", viewResult.ViewData["Message"]);
                Assert.Null(viewResult.ViewData.Model);
            }
        }

        public class About
        {
			[Fact]
			public void ReturnsViewResultWithDefaultViewName()
			{
				// Arrange
                var controller = new HomeController();

				// Act
                var result = controller.About();

				// Assert
				var viewResult = Assert.IsType<ViewResult>(result);
				Assert.Empty(viewResult.ViewName);
			}

            [Fact]
            public void SetsViewDataWithNoModel()
            {
				// Arrange
                var controller = new HomeController();

				// Act
                var result = controller.About();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Null(viewResult.ViewData.Model);
            }
        }
    }
}