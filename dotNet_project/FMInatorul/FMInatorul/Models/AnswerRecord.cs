namespace FMInatorul.Models
{
    public class AnswerRecord
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public int ParticipantId { get; set; }
        public string Answer { get; set; }
        public bool IsCorrect { get; set; }

        public IntrebariRasp Question { get; set; }
        public Participant Participant { get; set; }
    }

}
