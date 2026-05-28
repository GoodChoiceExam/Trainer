using FitLife.Trainer.Api.DTOs;
using FitLife.Trainer.Api.Models;
using FitLife.Trainer.Api.Repositories;

namespace FitLife.Trainer.Api.Services;

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

    public async Task<PersonalTrainer?> UpdateAsync(Guid id, TrainerRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null)
            return null;

        existing.Name = request.Name;
        existing.Bio = request.Bio;
        existing.Specialties = request.Specialties;
        existing.ExperienceYears = request.ExperienceYears;
        existing.Rating = request.Rating;
        existing.Sessions = request.Sessions;

        return await _repository.UpdateAsync(existing);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<TrainerBooking?> BookAsync(Guid trainerId, BookingRequest request)
    {
        var trainer = await _repository.GetByIdAsync(trainerId);
        if (trainer is null)
            return null;

        if (trainer.Bookings.Any(b => b.Status == BookingStatus.Booked
                                      && b.SessionTime.Date == request.SessionTime.Date
                                      && b.SessionTime.Hour == request.SessionTime.Hour))
            throw new InvalidOperationException("Tidspunktet er allerede booket.");

        var booking = new TrainerBooking
        {
            MemberId = request.MemberId,
            TrainerId = trainerId,
            SessionTime = request.SessionTime,
            BookedAt = DateTime.UtcNow,
            Status = BookingStatus.Booked
        };

        trainer.Bookings.Add(booking);
        await _repository.UpdateAsync(trainer);
        return booking;
    }

    public async Task<TrainerBooking?> CancelBookingAsync(Guid trainerId, Guid bookingId)
    {
        var trainer = await _repository.GetByIdAsync(trainerId);
        if (trainer is null)
            return null;

        var booking = trainer.Bookings.FirstOrDefault(b => b.Id == bookingId);
        if (booking is null)
            return null;

        booking.Status = BookingStatus.Cancelled;
        await _repository.UpdateAsync(trainer);
        return booking;
    }

    public async Task<List<TrainerBooking>> GetBookingsByMemberAsync(Guid memberId)
    {
        var trainers = await _repository.GetAllAsync();
        return trainers
            .SelectMany(t => t.Bookings)
            .Where(b => b.MemberId == memberId && b.Status == BookingStatus.Booked)
            .ToList();
    }

    public async Task<List<int>> GetBookedHoursAsync(Guid trainerId, DateOnly date)
    {
        var trainer = await _repository.GetByIdAsync(trainerId);
        if (trainer is null) return [];

        return trainer.Bookings
            .Where(b => b.Status == BookingStatus.Booked
                        && DateOnly.FromDateTime(b.SessionTime) == date)
            .Select(b => b.SessionTime.Hour)
            .ToList();
    }

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
