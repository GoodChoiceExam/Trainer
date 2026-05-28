namespace FitLife.Trainer.Api.Models;

// Repræsenterer en enkelt booking af en træner. Gemmes indlejret i PersonalTrainer-dokumentet i MongoDB.
public class TrainerBooking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MemberId { get; set; }
    public Guid TrainerId { get; set; }
    public DateTime SessionTime { get; set; }
    public DateTime BookedAt { get; set; }
    public BookingStatus Status { get; set; }
}