using FMInatorul.Controllers;
using FMInatorul.Data;
using FMInatorul.Models;
using FMInatorul.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace FMInatorul.Tests.SecurityTests
{
    public class HomeControllerSecurityTests
    {
        private HomeController _controller;
        private readonly ApplicationDbContext _mockDbContext;
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private Mock<ILogger<HomeController>> _mockLogger;

        public HomeControllerSecurityTests()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            _mockDbContext = new ApplicationDbContext(options);
            _mockDbContext.Database.EnsureCreated();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                    Mock.Of<IUserStore<ApplicationUser>>(),
                    null, null, null, null, null, null, null, null
                );

            _mockLogger = new Mock<ILogger<HomeController>>();

            _controller = new HomeController(_mockDbContext, _mockLogger.Object, _mockUserManager.Object);
        }

        private void SetupHttpContextWithUser(string userId)
        {
            // Create mock ClaimsPrincipal
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            var mockClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            // Mock HttpContext
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(context => context.User).Returns(mockClaimsPrincipal);
            mockHttpContext.Setup(context => context.TraceIdentifier).Returns("test-trace-id");

            // Assign mock HttpContext to the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };
        }

        // Helper function to simulate authenticated user
        private void SetupAuthenticatedUser(string userId)
        {
            // Create a ClaimsPrincipal with the specified user ID
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
            var mockClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            // Mock HttpContext
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(context => context.User).Returns(mockClaimsPrincipal);
            mockHttpContext.Setup(context => context.TraceIdentifier).Returns("test-trace-id");

            // Assign mock HttpContext to the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            // Mock UserManager behavior
            _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
        }

        // Helper function to simulate user being an Admin
        private void SetupAdminUser(string userId)
        {
            // Create claims for Admin role
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, "Admin")
        };
            var mockClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            // Mock HttpContext
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(context => context.User).Returns(mockClaimsPrincipal);
            mockHttpContext.Setup(context => context.TraceIdentifier).Returns("test-trace-id");

            // Assign mock HttpContext to the controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            // Mock UserManager behavior
            _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
        }


        // Test if the Index action returns the view correctly
        [Fact]
        public void Index_ReturnsViewResult_WhenCalled()
        {
            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        // Test if IndexNew returns the view correctly
        [Fact]
        public void IndexNew_ReturnsViewResult_WhenCalled()
        {
            // Act
            var result = _controller.IndexNew();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        // Test if Privacy returns the view correctly
        [Fact]
        public void Privacy_ReturnsViewResult_WhenCalled()
        {
            // Act
            var result = _controller.Privacy();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        // Test if error handling works correctly
        [Fact]
        public void Error_ReturnsViewResult_WithErrorDetails()
        {
            SetupHttpContextWithUser("test-user-id");

            // Act
            var result = _controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.Model);
            var errorViewModel = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.NotNull(errorViewModel.RequestId); // Checks that RequestId is set
            Assert.Equal("test-trace-id", errorViewModel.RequestId); // Verifies TraceIdentifier
        }


        // Test if non-admin users are redirected when accessing the Admin page
        [Fact]
        public void Admin_ReturnsRedirect_WhenUserIsNotAdmin()
        {
            var userId = "user123";
            SetupAuthenticatedUser(userId);

            // Act
            var result = _controller.Admin();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName); // Should redirect to "Index"
        }

        // Test if admin users can access the Admin page
        [Fact]
        public void Admin_ReturnsView_WhenUserIsAdmin()
        {
            var userId = "admin123";
            SetupAdminUser(userId);

            var materii = new List<Materie>
        {
            new Materie { Id = 1, nume = "Math", anStudiu = 1, descriere = "We like math.", FacultateID = 1, semestru = 2 }
        };
            var mockDbSet = new Mock<DbSet<Materie>>();
            mockDbSet.As<IQueryable<Materie>>().Setup(m => m.Provider).Returns(materii.AsQueryable().Provider);
            mockDbSet.As<IQueryable<Materie>>().Setup(m => m.Expression).Returns(materii.AsQueryable().Expression);
            mockDbSet.As<IQueryable<Materie>>().Setup(m => m.ElementType).Returns(materii.AsQueryable().ElementType);
            mockDbSet.As<IQueryable<Materie>>().Setup(m => m.GetEnumerator()).Returns(materii.GetEnumerator());

            // Act
            var result = _controller.Admin();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var viewBagMaterii = viewResult.ViewData["materii"];
            Assert.NotNull(viewBagMaterii);
        }

        [Fact]
        public async Task Add_Questions_FileUpload_ReturnsBadRequest_WhenFileIsInvalid()
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.ContentType).Returns("application/zip");
            var file = fileMock.Object;

            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "materie", "1" }
            });
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { Request = { Query = queryCollection } }
            };

            // Act
            var result = await _controller.Add_Questions(file);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No file uploaded!", badRequestResult.Value);
        }

        // Test if file upload is handled correctly with PDF
        [Fact]
        public async Task Add_Questions_FileUpload_ReturnsSuccess_WhenFileIsPdf()
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.ContentType).Returns("application/pdf"); // Valid content type
            fileMock.Setup(f => f.Length).Returns(1024); // 1 KB file size 
            var file = fileMock.Object;

            var queryCollection = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "materie", "1" }
            });
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { Request = { Query = queryCollection } }
            };

            // Act
            var result = await _controller.Add_Questions(file);

            // Assert
            if (result is ObjectResult objectResult)
            {
                Assert.Equal((int)HttpStatusCode.Unauthorized, objectResult.StatusCode);
            }
            else
            {
                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Admin", redirectResult.ActionName);
            }
        }
    }
}
