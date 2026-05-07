namespace FitLife.Trainer.Api.DTOs;

public record BookingRequest(
    Guid MemberId,
    DateTime SessionTime
);