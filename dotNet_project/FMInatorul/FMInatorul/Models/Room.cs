namespace FMInatorul.Models;
using System.ComponentModel.DataAnnotations;
public class Room
{
    [Key]
    public int RoomId { get; set; }

    [Required]
    [MaxLength(6)]
    public string Code { get; set; }    
    public ICollection<Participant> Participants { get; set; }
    public int MaterieID { get; set; }
    public virtual Materie Materie { get; set; }
}