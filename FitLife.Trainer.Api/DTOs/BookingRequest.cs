namespace FitLife.Trainer.Api.DTOs;

// Indeholder de felter klienten sender når en træner skal bookes.
public record BookingRequest(
    Guid MemberId,
    DateTime SessionTime
);