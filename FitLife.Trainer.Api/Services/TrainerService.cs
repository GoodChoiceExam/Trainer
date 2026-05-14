using FitLife.Trainer.Api.DTOs;
using FitLife.Trainer.Api.Models;
using MongoDB.Driver;

namespace FitLife.Trainer.Api.Services;

public class TrainerService : ITrainerService
{
    private readonly IMongoCollection<PersonalTrainer> _trainers;

    public TrainerService(IConfiguration configuration)
    {
        var mongoConn = configuration["MongoDB:ConnectionString"]!;
        var mongoDb = configuration["MongoDB:DatabaseName"]!;
        var collectionName = configuration["MongoDB:CollectionName"] ?? "trainers";

        var client = new MongoClient(mongoConn);
        var database = client.GetDatabase(mongoDb);
        _trainers = database.GetCollection<PersonalTrainer>(collectionName);
    }

    public async Task<List<PersonalTrainer>> GetAllAsync()
    {
        return await _trainers.Find(_ => true).ToListAsync();
    }

    public async Task<PersonalTrainer?> GetByIdAsync(Guid id)
    {
        return await _trainers.Find(t => t.Id == id).FirstOrDefaultAsync();
    }

    public async Task<PersonalTrainer> CreateAsync(TrainerRequest request)
    {
        var trainer = ToTrainer(request);
        await _trainers.InsertOneAsync(trainer);
        return trainer;
    }

    public async Task<PersonalTrainer?> UpdateAsync(Guid id, TrainerRequest request)
    {
        var existing = await GetByIdAsync(id);
        if (existing is null)
            return null;

        existing.Name = request.Name;
        existing.Bio = request.Bio;
        existing.Specialties = request.Specialties;
        existing.ExperienceYears = request.ExperienceYears;
        existing.Rating = request.Rating;
        existing.Sessions = request.Sessions;

        await _trainers.ReplaceOneAsync(t => t.Id == id, existing);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await _trainers.DeleteOneAsync(t => t.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<TrainerBooking?> BookAsync(Guid trainerId, BookingRequest request)
    {
        var trainer = await GetByIdAsync(trainerId);
        if (trainer is null)
            return null;

        var booking = trainer.Book(request.MemberId, request.SessionTime);
        await _trainers.ReplaceOneAsync(t => t.Id == trainerId, trainer);
        return booking;
    }

    public async Task<TrainerBooking?> CancelBookingAsync(Guid trainerId, Guid bookingId)
    {
        var trainer = await GetByIdAsync(trainerId);
        if (trainer is null)
            return null;

        var booking = trainer.CancelBooking(bookingId);
        if (booking is null)
            return null;

        await _trainers.ReplaceOneAsync(t => t.Id == trainerId, trainer);
        return booking;
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
    
    public async Task<List<TrainerBooking>> GetBookingsByMemberAsync(Guid memberId)
    {
        var trainers = await _trainers.Find(_ => true).ToListAsync();
        return trainers
            .SelectMany(t => t.Bookings)
            .Where(b => b.MemberId == memberId && b.Status == BookingStatus.Booked)
            .ToList();
    }
    
    public async Task<List<int>> GetBookedHoursAsync(Guid trainerId, DateOnly date)
    {
        var trainer = await GetByIdAsync(trainerId);
        if (trainer is null) return [];

        return trainer.Bookings
            .Where(b => b.Status == BookingStatus.Booked 
                        && DateOnly.FromDateTime(b.SessionTime.ToLocalTime()) == date)
            .Select(b => b.SessionTime.ToLocalTime().Hour)
            .ToList();
    }
}