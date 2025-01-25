using FMInatorul.Data;
using FMInatorul.Migrations;
using FMInatorul.Models;
using FMInatorul.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;

namespace FMInatorul.Controllers
{
	[Authorize]
	public class StudentsController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly ApplicationDbContext db;

		public StudentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			db = context;
			_userManager = userManager;
			_roleManager = roleManager;
		}

		// Returns the index view for students.
		public async Task<IActionResult> Index()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				return Challenge();
			}
			var student = await db.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == user.Id);

			if (student.CompletedProfile == false)
			{
				if (TempData.ContainsKey("ErrorMessage"))
				{
					ViewBag.Message = TempData["ErrorMessage"];
					return View(user);
				}
				else
				{
					TempData["ErrorMessage"] = "Please finish completing your profile.";
					ViewBag.Message = TempData["ErrorMessage"];
					return View(user);
				}
			}

            if (TempData.ContainsKey("SuccessMessage"))
			{
				ViewBag.Message = TempData["SuccessMessage"];
				return View(user);
			}
			return View(user);
		}

		// Returns the play view.
		public async Task<IActionResult> Play()
		{
            var userId = _userManager.GetUserId(User);

            // Găsește student pe baza ApplicationUserId
            var student = await db.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

            if (string.IsNullOrEmpty(userId))
            {
                return new ChallengeResult(); // Return 401 Unauthorized
            }

            if (student == null)
            {
                return NotFound();
            }

            // Obține materiile care aparțin facultății studentului selectat
            var materii = await db.Materii
                .Where(m => m.FacultateID == student.FacultateID)
                .Where(m => m.anStudiu == student.Year)
                .Where(m => m.semestru == student.Semester)
                .ToListAsync();
            ViewBag.Materii = materii;

            return View();
		}

		// Returns the view to upload a PDF file.
		public IActionResult UploadPdf()
		{
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return new ChallengeResult(); // Return 401 Unauthorized
            }

            return View();
		}

		// Handles the post request to upload a PDF file.
		[HttpPost]
		public async Task<IActionResult> UploadPdf([FromForm] IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				return BadRequest("No file uploaded!");
			}

			if (!IsPdfFile(file))
			{
				return BadRequest("Invalid file format. Only PDF files allowed!");
			}
            var apiSecret = Environment.GetEnvironmentVariable("API_PASSWORD");
            var jwtToken = await HttpHelper.GetJwtTokenAsync("dotnet_user", apiSecret, "https://api.fminatorul.xyz/login");
            if (string.IsNullOrEmpty(jwtToken))
            {
                return StatusCode(401, "Failed to authenticate with the Flask API");
            }


            var response = await HttpHelper.UploadPdfToFlaskApiAsync(file, "https://api.fminatorul.xyz/api/generate-quiz-external-links", jwtToken);

			if (response.IsSuccessStatusCode)
			{
				//Get the string repsponse from the API
				var responseString = await response.Content.ReadAsStringAsync();
				var quiz = JsonConvert.DeserializeObject<QuizModel>(responseString); // Deserialize the JSON response

                Debug.WriteLine($"Response String: {responseString}");

				return View("Quiz", quiz); //Pass the quiz model to the view
			}
			else
			{
				// Handle the API error
				return StatusCode((int)response.StatusCode, $"Error uploading file: {response.ReasonPhrase}");
			}
		}

		// Checks if the uploaded file is a PDF.
		private bool IsPdfFile(IFormFile file)
		{
			return file.ContentType.Contains("application/pdf");
		}


        // Returns the chat view.
        public ActionResult ChatView()
		{
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return new ChallengeResult(); // Return 401 Unauthorized
            }

            return View();
		}

		// Handles the post request to submit a quiz.
		[HttpPost]
		public IActionResult SubmitQuiz(QuizModel quiz)
		{
            return View("Results", quiz);
		}

		// Returns the view for single subject selection.
		public async Task<IActionResult> MateriiSingle()
		{

            var userId = _userManager.GetUserId(User);

            // Găsește student pe baza ApplicationUserId
            var student = await db.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

            if (string.IsNullOrEmpty(userId))
            {
                return new ChallengeResult(); // Return 401 Unauthorized
            }

            if (student == null)
            {
                return NotFound();
            }

            // Obține materiile care aparțin facultății studentului selectat
            var materii = await db.Materii
                .Where(m => m.FacultateID == student.FacultateID)
				.Where(m => m.anStudiu == student.Year)
                .Where(m => m.semestru == student.Semester)
                .ToListAsync();
            ViewBag.Materii = materii;
			return View();
		}

		// Returns the view to edit the student's year.
		public async Task<IActionResult> EditYear()
		{
			var userId = _userManager.GetUserId(User);
			var student = await db.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            if (string.IsNullOrEmpty(userId))
            {
                return new ChallengeResult(); // Return 401 Unauthorized
            }
            if (student == null)
			{
				return NotFound();
			}
			if (TempData.ContainsKey("ErrorMessage"))
			{
				ViewBag.Message = TempData["ErrorMessage"];
				return View(student);
			}
			return View(student);
		}

		// Handles the post request to edit the student's year.
		[HttpPost]
		public IActionResult EditYear(int id, Student student)
		{
			if (ModelState.IsValid)
			{
				return View(student);
			}
			else
			{
				Student studentToUpdate = db.Students.Where(stu => stu.Id == id).First();

				if (studentToUpdate == null)
				{
					return NotFound();
				}

				studentToUpdate.Year = student.Year;
				if (studentToUpdate.Year != 0 && studentToUpdate.Semester != 0 && studentToUpdate.FacultateID != 1)
				{
					studentToUpdate.CompletedProfile = true;
				}
				if (student.Year >= 4 || student.Year <= 0)
				{
					TempData["ErrorMessage"] = "Year must be between 1 and 3.";
					return RedirectToAction("EditYear", "Students");
				}
				db.SaveChanges();
				TempData["SuccessMessage"] = "Year updated successfully.";
				return RedirectToAction("Index", "Students");
			}
		}

		// Returns the view to edit the student's semester.
		public async Task<IActionResult> EditSemester()
		{
			var userId = _userManager.GetUserId(User);
			var student = await db.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            if (string.IsNullOrEmpty(userId))
            {
                return new ChallengeResult(); // Return 401 Unauthorized
            }
            if (student == null)
			{
				return NotFound();
			}
			if (TempData.ContainsKey("ErrorMessage"))
			{
				ViewBag.Message = TempData["ErrorMessage"];
				return View(student);
			}
			return View(student);
		}

		// Handles the post request to edit the student's semester.
		[HttpPost]
		public IActionResult EditSemester(int id, Student student)
		{
			if (ModelState.IsValid)
			{
				return View(student);
			}
			else
			{
				Student studentToUpdate = db.Students.Where(stu => stu.Id == id).First();

				if (studentToUpdate == null)
				{
					return NotFound();
				}

				studentToUpdate.Semester = student.Semester;
				if (studentToUpdate.Year != 0 && studentToUpdate.Semester != 0 && studentToUpdate.FacultateID != 1)
                {
					studentToUpdate.CompletedProfile = true;
				}
				if (student.Semester >= 3 || student.Semester <= 0)
				{
					TempData["ErrorMessage"] = "Semester must be between 1 and 2.";
					return RedirectToAction("EditSemester", "Students");
				}
				db.SaveChanges();
				TempData["SuccessMessage"] = "Semester updated successfully.";
				return RedirectToAction("Index", "Students");
			}
		}

        
        public async Task<IActionResult> EditCollege()
        {
            var userId = _userManager.GetUserId(User);
            var student = await db.Students.FirstOrDefaultAsync(s => s.ApplicationUserId == userId);

            if (string.IsNullOrEmpty(userId))
            {
                return new ChallengeResult(); // Return 401 Unauthorized
            }

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student not found.";
                return RedirectToAction("Index"); 
            }

            // Populate the ViewBag with faculties for the dropdown list
            var facultati = await db.Facultati.Where(f => f.Id != 1).ToListAsync();

            ViewBag.Facultati = facultati.Select(f => new SelectListItem
            {
                Value = f.Id.ToString(),
                Text = f.nume
            }).ToList();


            if (TempData.ContainsKey("ErrorMessage"))
            {
                ViewBag.Message = TempData["ErrorMessage"];
                return View(student);
            }
            
            return View(student);
        }

      
        [HttpPost]
        public IActionResult EditCollege(int id, Student student)
        {
            if (ModelState.IsValid)
            {
                return View(student);
            }
            else
            {
                Student studentToUpdate = db.Students.Where(stu => stu.Id == id).FirstOrDefault();

                if (studentToUpdate == null)
                {
                    return NotFound();
                }

                studentToUpdate.FacultateID = student.FacultateID;

                if (studentToUpdate.Year != 0 && studentToUpdate.Semester != 0 && studentToUpdate.FacultateID !=1)
                {
                    studentToUpdate.CompletedProfile = true;
                }
                if (student.FacultateID ==1 || student.FacultateID == null)
                {
                    TempData["ErrorMessage"] = "A college must be selected";
                    return RedirectToAction("EditCollege", "Students");
                }
                db.SaveChanges();
                TempData["SuccessMessage"] = "College updated successfully.";
                return RedirectToAction("Index", "Students");
            }
        }
        
		public IActionResult ShowIntrebari()
		{
            var materie_id = Convert.ToInt32(HttpContext.Request.Query["materie"]);
            //luam intrebarile de la materia respectiva care sunt aprobate de profesori
            var intrebari = db.IntrebariRasps
								.Where(i => i.MaterieId == materie_id && i.validareProfesor == 1)
                                .Include(i => i.Materie)
								.Include(i => i.Variante)
                                .ToList();

			return View(intrebari);
        }
       

    }
}
