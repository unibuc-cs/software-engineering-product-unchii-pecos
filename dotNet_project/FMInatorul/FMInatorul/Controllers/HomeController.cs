using FMInatorul.Data;
using FMInatorul.Models;
using FMInatorul.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NuGet.Protocol;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml.Serialization;

namespace FMInatorul.Controllers;

public class HomeController : Controller
{
	private readonly ILogger<HomeController> _logger;
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly ApplicationDbContext _db;

	public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
	{
		_db = context;
		_logger = logger;
		_userManager = userManager;
	}

	// Returns the main view of the application.
	public IActionResult Index()
	{
		return View();
	}

	// Returns a new version of the index view.
	public IActionResult IndexNew()
	{
        return View();
	}

	// Returns the privacy view.
	public IActionResult Privacy()
	{
		return View();
	}

	// Returns the error view with error details.
	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}

	// Returns the admin view if the user is an admin.
	[Authorize(Roles = "Admin")]
	public IActionResult Admin()
	{
		if (User.IsInRole("Admin"))
		{
			var Materii = _db.Materii;
			ViewBag.materii = Materii;
			return View();
		}
		return RedirectToAction("Index");
	}

	// Returns the view to upload a PDF for admins.
	[Authorize(Roles = "Admin")]
	public IActionResult UploadAdminPdf()
	{
		var materie_id = Convert.ToInt32(HttpContext.Request.Query["materie"]);
		ViewBag.materie = _db.Materii.Find(materie_id);
		ViewBag.materie_id = materie_id;

		return View();
	}

	// Adds questions to the database from JSON content.
	public IActionResult Add_Questions(JsonContent questions)
	{
		var materie_id = Convert.ToInt32(HttpContext.Request.Query["materie"]);
		ViewBag.materie = _db.Materii.Find(materie_id);

		return View(questions);
	}

	// Adds questions to the database from an uploaded PDF file.
	[HttpPost]
	public async Task<IActionResult> Add_Questions([FromForm] IFormFile file)
	{
		var materie_id = Convert.ToInt32(HttpContext.Request.Query["materie"]);
		ViewBag.materie = _db.Materii.Find(materie_id);

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
			var responseString = await response.Content.ReadAsStringAsync();
			Console.WriteLine(responseString);

			var questions = await response.Content.ReadAsStringAsync();
			var questions1 = JsonConvert.DeserializeObject<QuestionDtoList>(questions);

			foreach (var questionDto in questions1.Questions)
			{
				var stringrasp = "";
				var intrebareRasp = new IntrebariRasp
				{
					intrebare = questionDto.Question,
					validareProfesor = 0,
					MaterieId = materie_id,
					Variante = questionDto.Choices.Select(choice => new Variante
					{
						Choice = choice.Value,
						VariantaCorecta = choice.Key == questionDto.Answer ? 1 : 0
                    }).ToList(),
					raspunsCorect = questionDto.Choices.FirstOrDefault(choice => choice.Key == questionDto.Answer).Value
                };

				_db.IntrebariRasps.Add(intrebareRasp);
			}

			_db.SaveChanges();
			ViewBag.materie = _db.Materii.Find(materie_id);
            TempData["SuccessMessage"] = "Added questions successfully";
            return RedirectToAction("Admin", "Home"); //Pass the Quiz Model to the View
		}
		else
		{
			//Handle API upload error
			return StatusCode((int)response.StatusCode, $"Error uploading file: {response.ReasonPhrase}");
		}
	}
    // Checks if the uploaded file is a PDF.
    private bool IsPdfFile(IFormFile file)
	{
		return file.ContentType.Contains("application/pdf");
	}
}
