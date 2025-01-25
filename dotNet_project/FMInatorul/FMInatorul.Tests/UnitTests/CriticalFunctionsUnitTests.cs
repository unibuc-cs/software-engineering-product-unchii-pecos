using FMInatorul.Controllers;
using FMInatorul.Data;
using FMInatorul.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;

namespace FMInatorul.Tests.UnitTests
{
	public class CriticalFunctionsUnitTests
	{
		private readonly StudentsController _controller;
		private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
		private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
		private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
		private readonly ApplicationDbContext _mockDbContext;

		public CriticalFunctionsUnitTests()
		{
			// Create an in-memory SQLite database
			var connection = new SqliteConnection("DataSource=:memory:");
			connection.Open();

			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseSqlite(connection)
				.Options;

			_mockDbContext = new ApplicationDbContext(options);

			// Ensure the database is created
			_mockDbContext.Database.EnsureCreated();

			// Mock dependencies
			_mockUserManager = new Mock<UserManager<ApplicationUser>>(
				Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

			_mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
				_mockUserManager.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);

			_mockRoleManager = new Mock<RoleManager<IdentityRole>>(
				Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);

			// Initialize controller with mock dependencies and mocked user context
			_controller = new StudentsController(_mockDbContext, _mockUserManager.Object, _mockRoleManager.Object)
			{
				ControllerContext = new ControllerContext
				{
					HttpContext = new DefaultHttpContext
					{
						User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
						{
							new Claim(ClaimTypes.NameIdentifier, "admin"),
						}, "mock")),
					},
				},
			};
		}

		[Fact]
		public async Task UploadPdf_QuizNotNullAsync()
		{
			var fileMock = new Mock<IFormFile>();
			// Set up a stream with some content for testing purposes
			var stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write("Sample PDF Content");
			writer.Flush();
			stream.Position = 0;

			fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
			fileMock.Setup(f => f.Length).Returns(stream.Length);
			fileMock.Setup(f => f.FileName).Returns("sample.pdf");
			fileMock.Setup(f => f.ContentType).Returns("application/pdf");

			// Act
			var actionResult = await _controller.UploadPdf(fileMock.Object);

			// Assert
			Assert.IsNotType<BadRequestObjectResult>(actionResult);
		}


		[Fact]
		public async Task UploadPdf_QuizHasQuestionsAsync()
		{
			var fileMock = new Mock<IFormFile>();
			var stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write("Sample PDF Content");
			writer.Flush();
			stream.Position = 0;

			fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
			fileMock.Setup(f => f.Length).Returns(stream.Length);
			fileMock.Setup(f => f.FileName).Returns("sample.pdf");
			fileMock.Setup(f => f.ContentType).Returns("application/pdf");

			// Act
			IActionResult actionResult = await _controller.UploadPdf(fileMock.Object);

			// Assert
			Assert.NotNull(actionResult);

			// Check if it's a ViewResult with a QuizModel
			if (actionResult is ViewResult viewResult)
			{
				var quiz = viewResult.Model as QuizModel;
				Assert.NotNull(quiz);
				Assert.NotNull(quiz.Questions);
				Assert.NotEmpty(quiz.Questions);
			}
			else if (actionResult is ObjectResult objectResult && objectResult.StatusCode == 400)
			{
				// Handle BadRequestObjectResult or other error responses
				Assert.Equal("Invalid file format. Only PDF files allowed!", objectResult.Value);
			}

		}


		[Fact]
		public async Task UploadPdf_QuestionsHaveText()
		{
			var fileMock = new Mock<IFormFile>();
			// Set up a stream with some content for testing purposes
			var stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write("Sample PDF Content");
			writer.Flush();
			stream.Position = 0;

			fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
			fileMock.Setup(f => f.Length).Returns(stream.Length);
			fileMock.Setup(f => f.FileName).Returns("sample.pdf");
			fileMock.Setup(f => f.ContentType).Returns("application/pdf");

			// Act
			IActionResult actionResult = await _controller.UploadPdf(fileMock.Object);

			// Assert
			Assert.NotNull(actionResult);
			if (actionResult is ViewResult viewResult)
			{
				var quiz = viewResult.Model as QuizModel;
				foreach (var question in quiz.Questions)
				{
					Assert.False(string.IsNullOrWhiteSpace(question.Question));
				}
			}
		}

		[Fact]
		public async Task UploadPdf_QuestionsHaveAnswers()
		{
			var fileMock = new Mock<IFormFile>();
			// Set up a stream with some content for testing purposes
			var stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write("Sample PDF Content");
			writer.Flush();
			stream.Position = 0;

			fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
			fileMock.Setup(f => f.Length).Returns(stream.Length);
			fileMock.Setup(f => f.FileName).Returns("sample.pdf");
			fileMock.Setup(f => f.ContentType).Returns("application/pdf");

			// Act
			IActionResult actionResult = await _controller.UploadPdf(fileMock.Object);

			// Assert
			if (actionResult is ViewResult viewResult)
			{
				var quiz = viewResult.Model as QuizModel;
				Assert.NotNull(actionResult);
				Assert.NotNull(quiz);
				foreach (var question in quiz.Questions)
				{
					Assert.NotNull(question.Choices);
					Assert.NotEmpty(question.Choices);
				}
			}
		}

		[Fact]
		public async Task UploadPdf_QuestionsHaveCorrectAnswers()
		{
			var fileMock = new Mock<IFormFile>();
			// Set up a stream with some content for testing purposes
			var stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write("Sample PDF Content");
			writer.Flush();
			stream.Position = 0;

			fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
			fileMock.Setup(f => f.Length).Returns(stream.Length);
			fileMock.Setup(f => f.FileName).Returns("sample.pdf");
			fileMock.Setup(f => f.ContentType).Returns("application/pdf");

			// Act
			IActionResult actionResult = await _controller.UploadPdf(fileMock.Object);

			// Assert
			if (actionResult is ViewResult viewResult)
			{
				var quiz = viewResult.Model as QuizModel;
				Assert.NotNull(quiz);
				foreach (var question in quiz.Questions)
				{
					Assert.NotNull(question.Answer);
				}
			}
		}
	}
}