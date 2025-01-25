using FMInatorul.Controllers;
using FMInatorul.Data;
using FMInatorul.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using System.Text.Json;

namespace FMInatorul.Tests.SecurityTests
{
    public static class HttpHelperRoom
    {
        /// <summary>
        /// Sets up a mock HttpContext with a given user ID and optional additional claims.
        /// </summary>
        public static void SetupAuthenticatedUser(Controller controller, string userId, List<Claim> additionalClaims = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            if (additionalClaims != null)
            {
                claims.AddRange(additionalClaims);
            }

            var mockClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(context => context.User).Returns(mockClaimsPrincipal); // Ensure User is set
            mockHttpContext.Setup(context => context.TraceIdentifier).Returns($"trace-{userId}");

            // Ensure the controller context has the mocked HttpContext with the user
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };
        }

        /// <summary>
        /// Sets up mock HttpContext with room-specific claims.
        /// </summary>
        public static void SetupRoomContext(Controller controller, int roomId, string roomCode, string userId = "user123")
        {
            var roomClaims = new List<Claim>
            {
                new Claim("RoomId", roomId.ToString()),
                new Claim("RoomCode", roomCode),
                new Claim(ClaimTypes.NameIdentifier, userId)  
            };

            SetupAuthenticatedUser(controller, userId, roomClaims); 
        }

        /// <summary>
        /// Sets up mock HttpContext for a participant with room and student-specific claims.
        /// </summary>
        public static void SetupParticipantContext(Controller controller, int roomId, int studentId)
        {
            var participantClaims = new List<Claim>
        {
            new Claim("RoomId", roomId.ToString()),
            new Claim("StudentId", studentId.ToString())
        };

            SetupAuthenticatedUser(controller, "participant-user", participantClaims);
        }
    }

    public class RoomsControllerSecurityTests
    {
        private readonly ApplicationDbContext _mockDbContext;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private readonly Mock<IHubContext<RoomHub>> _mockRoomHubContext;
        private RoomsController _controller;

        public RoomsControllerSecurityTests()
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
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null
            );
            _mockRoomHubContext = new Mock<IHubContext<RoomHub>>();

            _controller = new RoomsController(
                _mockDbContext, _mockRoomHubContext.Object, _mockUserManager.Object, _mockRoleManager.Object);
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

        // Test: User is unauthorized when trying to join a room
        [Fact]
        public async Task JoinRoom_ShouldReturnUnauthorized_WhenUserIsNotLoggedIn()
        {
            // Arrange
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string)null);
            var request = new RoomsController.JoinRoomRequest { Code = "123456" };

            // Act
            var result = await _controller.JoinRoom(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);

            // Expected and actual result comparison
            var expectedResult = new { success = false, message = "You must be logged in to join a room" };
            var actualResult = jsonResult.Value;

            Assert.Equal(JsonSerializer.Serialize(expectedResult), JsonSerializer.Serialize(actualResult));
        }

        // Test: User successfully joins a room
        [Fact]
        public async Task JoinRoom_ShouldReturnSuccess_WhenUserIsAuthorized()
        {
            // Arrange
            var userId = "user123";  // Set a valid user ID

            // Mock UserManager to return userId correctly
            SetupHttpContextWithUser(userId); ////////////// Maybe HTTP is wrong here???

            // Set up the HttpContext with the correct user
            HttpHelperRoom.SetupRoomContext(_controller, 1, "123456", userId);

            var request = new RoomsController.JoinRoomRequest { Code = "123456" };

            // Act
            var result = await _controller.JoinRoom(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal("{\"success\":true,\"message\":\"Joined room 123456 successfully.\"}", jsonResult.Value.ToString());
        }

        // Test: Joining a non-existent room
        [Fact]
        public async Task JoinRoom_ShouldReturnNotFound_WhenRoomDoesNotExist()
        {
            // Arrange
            var userId = "user123";
            HttpHelperRoom.SetupAuthenticatedUser(_controller, userId);
            HttpHelperRoom.SetupRoomContext(_controller, 1, "123456");
            var request = new RoomsController.JoinRoomRequest { Code = "999999" };

            // Act
            var result = await _controller.JoinRoom(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal("{\"success\":false,\"message\":\"Room not found\"}", jsonResult.Value.ToString());
        }

        // Test: User creates a room successfully
        [Fact]
        public async Task CreateRoom_ShouldReturnSuccess_WhenUserIsAuthorized()
        {
            await _mockDbContext.Database.OpenConnectionAsync();
            await _mockDbContext.Database.EnsureCreatedAsync();

            // Arrange
            var userId = "user123";
            HttpHelperRoom.SetupAuthenticatedUser(_controller, userId);
            var request = new RoomsController.JoinRoomRequest { Code = "101" };

            // Act
            var result = await _controller.CreateRoom(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Contains("code", jsonResult.Value.ToString());
        }

        // Test: User cannot create a room without login
        [Fact]
        public async Task CreateRoom_ShouldReturnUnauthorized_WhenUserIsNotLoggedIn()
        {
            await _mockDbContext.Database.OpenConnectionAsync();
            await _mockDbContext.Database.EnsureCreatedAsync();

            // Arrange
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string)null);
            var request = new RoomsController.JoinRoomRequest { Code = "101" };

            // Act
            var result = await _controller.CreateRoom(request);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal("{\"success\":false,\"message\":\"You must be logged in to create a room\"}", jsonResult.Value.ToString());
        }

        // Test: User leaves a room successfully
        [Fact]
        public async Task LeaveRoom_ShouldReturnSuccess_WhenUserIsInRoom()
        {
            // Arrange
            var userId = "user123";
            HttpHelperRoom.SetupRoomContext(_controller, 1, "123456");

            // Act
            var result = await _controller.LeaveRoom("123456");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal("{\"success\":true,\"message\":\"You have left the room.\"}", jsonResult.Value.ToString());
        }

        // Test: User tries to leave a room they are not in
        [Fact]
        public async Task LeaveRoom_ShouldReturnError_WhenUserIsNotInRoom()
        {
            // Arrange
            var userId = "user123";
            HttpHelperRoom.SetupRoomContext(_controller, 1, "654321");

            // Act
            var result = await _controller.LeaveRoom("123456");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.Equal("{\"success\":false,\"message\":\"You are not in this room\"}", jsonResult.Value.ToString());
        }
    }
}
