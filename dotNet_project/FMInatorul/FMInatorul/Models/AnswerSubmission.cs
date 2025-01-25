namespace FMInatorul.Models
{
    public class AnswerSubmission
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public int ParticipantId { get; set; }
        public string RoomCode { get; set; }
        public string Answer { get; set; }
        public DateTime SubmittedAt { get; set; }

        public IntrebariRasp Question { get; set; }
        public Participant Participant { get; set; }
    }
}
