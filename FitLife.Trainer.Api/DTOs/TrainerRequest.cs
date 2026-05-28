namespace FitLife.Trainer.Api.DTOs;

// Indeholder de felter klienten sender når en træner skal oprettes eller opdateres.
public record TrainerRequest(
    string Name,
    string Bio,
    List<string> Specialties,
    int ExperienceYears,
    double Rating,
    int Sessions
);