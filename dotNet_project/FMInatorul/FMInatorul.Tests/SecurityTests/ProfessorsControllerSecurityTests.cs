using FMInatorul.Controllers;
using FMInatorul.Data;
using FMInatorul.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq.Expressions;
using System.Security.Claims;
using Xunit;

namespace FMInatorul.Tests.SecurityTests
{
    public class ProfessorsControllerSecurityTests
    {
        private readonly ApplicationDbContext _mockDbContext;
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private ProfessorsController _controller;

        public ProfessorsControllerSecurityTests()
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
            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
        }

        // Test if professor is able to view the index page when unauthorized
        [Fact]
        public async Task Index_ShouldReturn401_WhenUserIsUnauthorized()
        {
            // Arrange
            _mockUserManager
                .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            _controller = new ProfessorsController(_mockDbContext, _mockUserManager.Object, _mockRoleManager.Object);

            // Act
            var result = await _controller.Index();

            // Assert
            var challengeResult = Assert.IsType<ChallengeResult>(result);
            Assert.NotNull(challengeResult);
        }

        // Test if professor can't edit subject when unauthorized
        [Fact]
        public async Task EditMaterie_ShouldReturn401_WhenUserIsUnauthorized()
        {
            await _mockDbContext.Database.OpenConnectionAsync();
            await _mockDbContext.Database.EnsureCreatedAsync();
            // Arrange
            _mockUserManager
                .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            _controller = new ProfessorsController(_mockDbContext, _mockUserManager.Object, _mockRoleManager.Object);

            // Act
            var result = await _controller.EditMaterie();

            // Assert
            var challengeResult = Assert.IsType<ChallengeResult>(result);
            Assert.NotNull(challengeResult);
        }

        [Fact]
        public async Task EditMaterie_WithProfessor_ShouldReturn401_WhenUserIsUnauthorized()
        {
            await _mockDbContext.Database.OpenConnectionAsync();
            await _mockDbContext.Database.EnsureCreatedAsync();
            // Arrange
            _mockUserManager
                .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            _controller = new ProfessorsController(_mockDbContext, _mockUserManager.Object, _mockRoleManager.Object);



            var facultate = new Facultate
            {
                Id = 1,
                nume = "Test Facultate"
            };
            await _mockDbContext.Facultati.AddAsync(facultate);

            var studentUser = new ApplicationUser
            {
                Id = "student123",
                FirstName = "testStudent",
                LastName = "1",
                IdStud = 1
            };
            await _mockDbContext.Users.AddAsync(studentUser);

            var professorUser = new ApplicationUser
            {
                Id = "professor123",
                FirstName = "testProfessor",
                LastName = "2",
                IdProf = 1
            };
            await _mockDbContext.Users.AddAsync(professorUser);
            await _mockDbContext.SaveChangesAsync();

            var student = new Student
            {
                Id = 1,
                ApplicationUserId = studentUser.Id,
                Year = 1,
                Semester = 1,
                FacultateID = facultate.Id
            };
            await _mockDbContext.Students.AddAsync(student);

            var professor = new Profesor
            {
                Id = 1,
                ApplicationUserId = professorUser.Id,
                FacultateID = facultate.Id
            };
            await _mockDbContext.Professors.AddAsync(professor);

            var materie = new Materie
            {
                Id = 1,
                nume = "Mathematics",
                anStudiu = 1,
                semestru = 1,
                descriere = "Mathematics 101",
                FacultateID = facultate.Id
            };
            await _mockDbContext.Materii.AddAsync(materie);

            var intrebareRasp = new IntrebariRasp
            {
                Id = 1,
                intrebare = "What is 2+2?",
                raspunsCorect = "4",
                validareProfesor = 1,
                MaterieId = materie.Id
            };
            await _mockDbContext.IntrebariRasps.AddAsync(intrebareRasp);

            var varianta = new Variante
            {
                Id = 1,
                Choice = "4",
                IntrebariRaspId = intrebareRasp.Id,
                VariantaCorecta = 1
            };
            await _mockDbContext.Variantes.AddAsync(varianta);
            await _mockDbContext.SaveChangesAsync();



            // Act
            var result = await _controller.EditMaterie(professor);

            // Assert
            var challengeResult = Assert.IsType<ChallengeResult>(result);
            Assert.NotNull(challengeResult);
        }

        // Test if professor can't validate question when unauthorized
        [Fact]
        public async Task Valideaza_ShouldReturn401_WhenUserIsUnauthorized()
        {
            await _mockDbContext.Database.OpenConnectionAsync();
            await _mockDbContext.Database.EnsureCreatedAsync();
            // Arrange
            _mockUserManager
                .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            _controller = new ProfessorsController(_mockDbContext, _mockUserManager.Object, _mockRoleManager.Object);

            // Act
            var result = await _controller.Valideaza(123); // Example integer parameter

            // Assert
            var challengeResult = Assert.IsType<ChallengeResult>(result);
            Assert.NotNull(challengeResult);
        }

        // Test if professor can't invalidate question when unauthorized
        [Fact]
        public async Task NuValideaza_ShouldReturn401_WhenUserIsUnauthorized()
        {
            await _mockDbContext.Database.OpenConnectionAsync();
            await _mockDbContext.Database.EnsureCreatedAsync();
            // Arrange
            _mockUserManager
                .Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser)null);

            _controller = new ProfessorsController(_mockDbContext, _mockUserManager.Object, _mockRoleManager.Object);

            // Act
            var result = await _controller.NuValideaza(123); // Example integer parameter

            // Assert
            var challengeResult = Assert.IsType<ChallengeResult>(result);
            Assert.NotNull(challengeResult);
        }
    }
}