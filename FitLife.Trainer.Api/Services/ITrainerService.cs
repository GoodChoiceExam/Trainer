using FitLife.Trainer.Api.DTOs;
using FitLife.Trainer.Api.Models;

namespace FitLife.Trainer.Api.Services;

public interface ITrainerService
{
    Task<List<PersonalTrainer>> GetAllAsync();
    Task<PersonalTrainer?> GetByIdAsync(Guid id);
    Task<PersonalTrainer> CreateAsync(TrainerRequest request);
    Task<PersonalTrainer?> UpdateAsync(Guid id, TrainerRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<TrainerBooking?> BookAsync(Guid trainerId, BookingRequest request);
    Task<TrainerBooking?> CancelBookingAsync(Guid trainerId, Guid bookingId);
    Task<List<TrainerBooking>> GetBookingsByMemberAsync(Guid memberId);
    Task<List<int>> GetBookedHoursAsync(Guid trainerId, DateOnly date);
}