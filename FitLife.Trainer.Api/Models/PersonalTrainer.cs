namespace FitLife.Trainer.Api.Models;

public class PersonalTrainer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public List<string> Specialties { get; set; } = [];
    public int ExperienceYears { get; set; }
    public double Rating { get; set; }
    public int Sessions { get; set; }
    public List<TrainerBooking> Bookings { get; set; } = [];
}
