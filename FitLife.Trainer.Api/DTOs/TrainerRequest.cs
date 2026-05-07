namespace FitLife.Trainer.Api.DTOs;

public record TrainerRequest(
    string Name,
    string Bio,
    List<string> Specialties,
    int ExperienceYears,
    double Rating,
    int Sessions
);