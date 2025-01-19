using System.ComponentModel.DataAnnotations;

namespace FMInatorul.Models
{
    public class Participant
    {
        [Key]
        public int ParticipantId { get; set; }

        // 1:1
        public int RoomId { get; set; }
        public Room Room { get; set; }

        // 1:1
        public int StudentId { get; set; }
        public Student Student { get; set; }
    }
}
