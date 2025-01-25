using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Xunit;
using FMInatorul.Controllers;
using FMInatorul.Models;
using FMInatorul.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using FMInatorul.Utilities;

namespace FMInatorul.Tests.SecurityTests
{
    public static class HttpHelper
    {
        public static Func<string, string, string, Task<string>> GetJwtTokenAsync =
            async (username, secret, url) =>
            {
                // Original implementation here
                return await Task.FromResult("OriginalToken");
            };
    }

    public class StudentsControllerSecurityTests
    {
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private readonly ApplicationDbContext _mockDbContext;
        private StudentsController _controller;

        public StudentsControllerSecurityTests()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(connection)
                .Options;

            _mockDbContext = new ApplicationDbContext(options);

            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                    Mock.Of<IUserStore<ApplicationUser>>(),
                    null, null, null, null, null, null, null, null
                );
            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(_mockUserManager.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
        }

        // Test for Index action
        [Fact]
        public async Task Index_ShouldReturn401_WhenUserIsUnauthorized()
        {
            _mockUserManager
                .Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            _controller = new StudentsController(_mockDbContext, _mockUserManager.Object, _mockRoleManager.Object);

            // Act
            var result = await _controller.Index();

            // Assert
            var statusResult = Assert.IsType<ChallengeResult>(result); // ChallengeResult is expected
            Assert.NotNull(statusResult); // Ensures the result is not null
        }

        // Test for Play action
        [Fact]
        public async Task Play_ShouldReturn401_WhenUserIsUnauthorized()
        {
            await _mockDbContext.Database.OpenConnectionAsync();
            await _mockDbContext.Database.EnsureCreatedAsync();
            
            _mockUserManager
                .Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            _controller = new StudentsController(_mockDbContext, _mockUserManager.Object, _mockRoleManager.Object);

            // Act
            var result = await _controller.Play();

            // Assert
            var challengeResult = Assert.IsType<ChallengeResult>(result); // Expect ChallengeResult (401 Unauthorized)
            Assert.NotNull(challengeResult);
        }

        // Test for UploadPdf GET action
        [Fact]
        public async void UploadPdf_ShouldReturn401_WhenUserIsNotAauthorized()
        {
            _mockUserManager
                .Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            _controller = new StudentsController(_mockDbContext, _mockUserManager.Object, _mockRoleManager.Object);

            // Act
            var result = _controller.UploadPdf();

            // Assert
            var challengeResult = Assert.IsType<ChallengeResult>(result); // Expect ChallengeResult (401 Unauthorized)
            Assert.NotNull(challengeResult);
        }

        // Test for UploadPdf POST action
        [Fact]
        public async Task UploadPdf_ShouldReturn401_WhenAuthenticationFails()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.ContentType).Returns("application/pdf");
            fileMock.Setup(f => f.Length).Returns(1024); // Mock file size of 1KB
            var file = fileMock.Object;

            // Mock GetJwtTokenAsync to return an empty string for this test
            HttpHelper.GetJwtTokenAsync = async (username, secret, url) => string.Empty;

            _controller = new StudentsController(_mockDbContext, _mockUserManager.Object, _mockRoleManager.Object);

            // Act
            var result = await _controller.UploadPdf(file);

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.NotNull(statusResult);
            Assert.Equal(401, statusResult.StatusCode);
            Assert.Equal("Failed to authenticate with the Flask API", statusResult.Value);

            // Reset the static method to avoid interference with other tests
            HttpHelper.GetJwtTokenAsync = async (username, secret, url) => await Task.FromResult("OriginalToken");
        }

        // Test for ChatView action
        [Fact]
        public void ChatView_ShouldReturn401_WhenUserIsUnauthorized()
        {
            // Arrange
            _mockUserManager
                .Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            _controller = new StudentsController(_mockDbContext, _mockUserManager.Object, _mockRoleManager.Object);

            // Act
            var result = _controller.ChatView();

            // Assert
            var challengeResult = Assert.IsType<ChallengeResult>(result); // Expect ChallengeResult (401 Unauthorized)
            Assert.NotNull(challengeResult);
        }

        // Test for MateriiSingle action
        [Fact]
        public async Task MateriiSingle_ShouldReturn401_WhenUserIsUnauthorized()
        {
            await _mockDbContext.Database.OpenConnectionAsync();
            await _mockDbContext.Database.EnsureCreatedAsync();

            _mockUserManager
                .Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            _controller = new StudentsController(_mockDbContext, _mockUserManager.Object, _mockRoleManager.Object);

            // Act
            var result = await _controller.MateriiSingle();

            // Assert
            var challengeResult = Assert.IsType<ChallengeResult>(result); // Expect ChallengeResult (401 Unauthorized)
            Assert.NotNull(challengeResult);
        }

        // Test for EditYear GET action
        [Fact]
        public async Task EditYear_ShouldReturn401_WhenUserIsUnauthorized()
        {
            await _mockDbContext.Database.OpenConnectionAsync();
            await _mockDbContext.Database.EnsureCreatedAsync();

            _mockUserManager
                .Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            _controller = new StudentsController(_mockDbContext, _mockUserManager.Object, _mockRoleManager.Object);

            // Act
            var result = await _controller.EditYear();

            // Assert
            var challengeResult = Assert.IsType<ChallengeResult>(result); // Expect ChallengeResult (401 Unauthorized)
            Assert.NotNull(challengeResult);
        }

        // Test for EditSemester GET action
        [Fact]
        public async Task EditSemester_ShouldReturn401_WhenUserIsUnauthorized()
        {
            await _mockDbContext.Database.OpenConnectionAsync();
            await _mockDbContext.Database.EnsureCreatedAsync();

            _mockUserManager
                .Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            _controller = new StudentsController(_mockDbContext, _mockUserManager.Object, _mockRoleManager.Object);

            // Act
            var result = await _controller.EditSemester();

            // Assert
            var challengeResult = Assert.IsType<ChallengeResult>(result); // Expect ChallengeResult (401 Unauthorized)
            Assert.NotNull(challengeResult);
        }

        // Test for EditCollege GET action
        [Fact]
        public async Task EditCollege_ShouldReturn401_WhenUserIsUnauthorized()
        {
            await _mockDbContext.Database.OpenConnectionAsync();
            await _mockDbContext.Database.EnsureCreatedAsync();

            _mockUserManager
                .Setup(m => m.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            _controller = new StudentsController(_mockDbContext, _mockUserManager.Object, _mockRoleManager.Object);

            // Act
            var result = await _controller.EditCollege();

            // Assert
            var challengeResult = Assert.IsType<ChallengeResult>(result); // Expect ChallengeResult (401 Unauthorized)
            Assert.NotNull(challengeResult);
        }
    }
}
