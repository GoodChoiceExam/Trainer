using FitLife.Trainer.Api.DTOs;
using FitLife.Trainer.Api.Models;
using FitLife.Trainer.Api.Repositories;

namespace FitLife.Trainer.Api.Services;

// Indeholder forretningslogik for trænere og bookinger.
// Controlleren kalder servicen, som delegerer databaseoperationer til repository.
public class TrainerService : ITrainerService
{
    private readonly ITrainerRepository _repository;

    public TrainerService(ITrainerRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<PersonalTrainer>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<PersonalTrainer?> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<PersonalTrainer> CreateAsync(TrainerRequest request)
    {
        var trainer = ToTrainer(request);
        return await _repository.AddAsync(trainer);
    }

    public async Task<TrainerBooking?> BookAsync(Guid trainerId, BookingRequest request)
    {
        var trainer = await _repository.GetByIdAsync(trainerId);
        if (trainer is null)
            return null;

        var booking = trainer.Book(request.MemberId, request.SessionTime);
        await _repository.UpdateAsync(trainer);
        return booking;
    }

    public async Task<TrainerBooking?> CancelBookingAsync(Guid trainerId, Guid bookingId)
    {
        var trainer = await _repository.GetByIdAsync(trainerId);
        if (trainer is null)
            return null;

        var booking = trainer.CancelBooking(bookingId);
        if (booking is null)
            return null;

        await _repository.UpdateAsync(trainer);
        return booking;
    }

    public async Task<List<TrainerBooking>> GetBookingsByMemberAsync(Guid memberId)
    {
        // Henter alle trænere og laver deres bookings om til én liste, filtrerer kun aktive bookinger
        var trainers = await _repository.GetAllAsync();
        return trainers
            .SelectMany(t => t.Bookings)
            .Where(b => b.MemberId == memberId && b.Status == BookingStatus.Booked)
            .ToList();
    }

    public async Task<List<int>> GetBookedHoursAsync(Guid trainerId, DateOnly date)
    {
        // Returnerer en liste af timer (fx 9, 11, 14) som er booket den givne dato
        // Bruges af frontend til at vise hvilke tider der er ledige
        var trainer = await _repository.GetByIdAsync(trainerId);
        if (trainer is null) return [];

        return trainer.Bookings
            .Where(b => b.Status == BookingStatus.Booked
                        && DateOnly.FromDateTime(b.SessionTime) == date)
            .Select(b => b.SessionTime.Hour)
            .ToList();
    }

    // Mapper en DTO til et objekt
    private static PersonalTrainer ToTrainer(TrainerRequest request) => new()
    {
        Name = request.Name,
        Bio = request.Bio,
        Specialties = request.Specialties,
        ExperienceYears = request.ExperienceYears,
        Rating = request.Rating,
        Sessions = request.Sessions
    };
}
