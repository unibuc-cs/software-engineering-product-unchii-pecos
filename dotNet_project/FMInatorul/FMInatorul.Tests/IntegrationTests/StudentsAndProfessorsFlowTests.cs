using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using FMInatorul.Controllers;
using FMInatorul.Models;
using FMInatorul.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Security.Claims;

namespace FMInatorul.Tests.IntegrationTests
{
    public class StudentsAndProfessorsFlowTests
    {
        private StudentsController _controller;
        private ProfessorsController _controllerProfessor;
        private Mock<UserManager<ApplicationUser>> _mockUserManager;
        private Mock<RoleManager<IdentityRole>> _mockRoleManager;
        private ApplicationDbContext _mockDbContext;

        public StudentsAndProfessorsFlowTests()
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

            _mockRoleManager = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(),
                null, null, null, null
            );

            _controller = new StudentsController(_mockDbContext, _mockUserManager.Object, _mockRoleManager.Object);
            _controllerProfessor = new ProfessorsController(_mockDbContext, _mockUserManager.Object, _mockRoleManager.Object);
        }

        [Fact]
        public async Task Student_UploadPdf_ShouldSaveFileAndGenerateQuiz()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.pdf");
            mockFile.Setup(f => f.Length).Returns(1024); // valid file size
            mockFile.Setup(f => f.ContentType).Returns("application/pdf"); // valid content type
            mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[] { 1, 2, 3, 4 }));

            // Act
            var result = await _controller.UploadPdf(mockFile.Object);

            // Assert
            Assert.IsType<ObjectResult>(result);
        }


        [Fact]
        public async Task EditCollegeProf_UserIsStudent_ReturnsForbidResult()
        {
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
            var mockStudent = new Profesor
            {
                ApplicationUserId = studentUser.Id,
                FacultateID = facultate.Id 
            };

            var result = _controllerProfessor.EditCollegeProf(1, mockStudent);

            // Assert
            Assert.IsType<ForbidResult>(result); // Expecting ForbidResult
        }
    }
}
