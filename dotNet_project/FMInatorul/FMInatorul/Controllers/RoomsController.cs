using FMInatorul.Data;
using FMInatorul.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Specialized;
using System.Text.Json;

namespace FMInatorul.Controllers
{
    public class RoomsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<RoomHub> _roomHubContext;

        public RoomsController(ApplicationDbContext context, IHubContext<RoomHub> roomHubContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _roomHubContext = roomHubContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // to help map the JSON request
        public class JoinRoomRequest
        {
            public string Code { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> JoinRoom([FromBody] JoinRoomRequest request)
        {
            if (string.IsNullOrEmpty(request.Code))
            {
                return Json(new { success = false, message = "Invalid room code" });
            }

            // use ApplicationUser to get identity user id
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "You must be logged in to join a room" });
            }

            // find the student profile
            var student = await _context.Students
            .Include(s => s.ApplicationUser)
            .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            if (student == null)
            {
                return BadRequest(new { success = false, message = "No student profile found!" });
            }

            // find the room
            var room = await _context.Rooms
                .FirstOrDefaultAsync(r => r.Code == request.Code);
            if (room == null)
            {
                return Json(new { success = false, message = "Room not found" });
            }

            // join the room
            var participantExists = await _context.Participants
                                .AnyAsync(p => p.RoomId == room.RoomId && p.StudentId == student.Id);
            if (!participantExists)
            {
                var participant = new Participant
                {
                    RoomId = room.RoomId,
                    StudentId = student.Id
                };
                _context.Participants.Add(participant);
                await _context.SaveChangesAsync();
            }

            var firstName = student.ApplicationUser.FirstName;
            var lastName = student.ApplicationUser.LastName;
            var fullName = $"{firstName} {lastName}";
            // notify the room
            await _roomHubContext.Clients.Group(room.Code)
                .SendAsync("UserJoined", fullName);

            return Json(new { success = true, message = $"Joined room {room.Code} successfully." });
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateRoom([FromBody] JoinRoomRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Code))
            {
                return BadRequest("Invalid data.");
            }

            int materieId;
            if (!int.TryParse(request.Code, out materieId))
            {
                return BadRequest("Invalid Materie ID.");
            }

            Console.WriteLine("am ajuns si aici");
            materieId = int.Parse(request.Code);
            Console.WriteLine(materieId);
            // Căutăm materia în baza de date
            var materie = await _context.Materii.FindAsync(materieId);
            if (materie == null)
            {
                return BadRequest("Materia nu a fost găsită.");
            }

            // get the user
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "You must be logged in to create a room" });
            }

            // find the student profile
            var student = await _context.Students
           .Include(s => s.ApplicationUser)
           .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            if (student == null)
            {
                return Json(new { success = false, message = "No student profile found!" });
            }

            // generate code
            string code;
            do
            {
                code = new Random().Next(100000, 999999).ToString();
            } while (await _context.Rooms.AnyAsync(r => r.Code == code));

            // create & save room
            var room = new Room { Code = code , MaterieID = materieId, Materie = materie};

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            // add host as participant
            var participant = new Participant
            {
                RoomId = room.RoomId,
                StudentId = student.Id
            };
            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            return Json(new { code });
        }

        [HttpPost]
        public async Task<IActionResult> LeaveRoom([FromBody] string code)
        {
            // current user
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "You must be logged in to leave a room" });
            }

            // find the student profile
            var student = await _context.Students
           .Include(s => s.ApplicationUser)
           .FirstOrDefaultAsync(s => s.ApplicationUserId == userId);
            if (student == null)
            {
                return Json(new { success = false, message = "No student profile found!" });
            }

            // find room by code
            var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Code == code);
            if (room == null)
            {
                return Json(new { success = false, message = "Room not found" });
            }

            // find participant
            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.RoomId == room.RoomId && p.StudentId == student.Id);
            if (participant == null)
            {
                return Json(new { success = false, message = "You are not in this room" });
            }

            // remove participant
            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();

            // check if the room is empty
            bool isRoomEmpty = !await _context.Participants.AnyAsync(p => p.RoomId == room.RoomId);
            if (isRoomEmpty)
            {
                _context.Rooms.Remove(room);
                await _context.SaveChangesAsync();
            }

            // notify via SignalR
            var fullName = $"{student.ApplicationUser.FirstName} {student.ApplicationUser.LastName}";

            await _roomHubContext.Clients.Group(code)
                .SendAsync("UserLeft", fullName);

            return Json(new { success = true, message = "You have left the room." });
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Lobby(string code)
        {
            var room = _context.Rooms
                .Include(r => r.Participants)
                    .ThenInclude(p => p.Student)
                        .ThenInclude(s => s.ApplicationUser)
                .FirstOrDefault(r => r.Code == code);

            if (room == null)
            {
                return NotFound("Room does not exist.");
            }

            var userId = _userManager.GetUserId(User);
            var student = _context.Students
                .Include(s => s.ApplicationUser)
                .FirstOrDefault(s => s.ApplicationUserId == userId);

            var userFullName = (student is not null)
                ? $"{student.ApplicationUser.FirstName} {student.ApplicationUser.LastName}"
                : string.Empty;

            ViewBag.UserFullName = userFullName;

            return View(room);
        }

        [HttpPost]
        public async Task<IActionResult> StartGame([FromBody] string code)
        {
            // find the room
            var room = await _context.Rooms
                .Include(r => r.Participants)
                    .ThenInclude(p => p.Student)
                        .ThenInclude(s => s.ApplicationUser)
                .FirstOrDefaultAsync(r => r.Code == code);
            if (room == null)
            {
                return Json(new { success = false, message = "Room not found" });
            }

            await _roomHubContext.Clients.Group(code)
                .SendAsync("StartGame");

            return Json(new { success = true, message = "Game started" });
        }

        public IActionResult Game()
        {
            return View();
        }
    }
}
