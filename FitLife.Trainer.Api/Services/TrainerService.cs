using Microsoft.Extensions.Caching.Memory;
using FitLife.Trainer.Api.DTOs;
using FitLife.Trainer.Api.Models;
using MongoDB.Driver;


namespace FitLife.Trainer.Api.Services;

public class TrainerService : ITrainerService
{
    private readonly IMongoCollection<PersonalTrainer> _trainers;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<TrainerService> _logger;

    private const string AllTrainersCacheKey = "all_trainers";

    public TrainerService(IMongoDatabase database, IMemoryCache memoryCache, ILogger<TrainerService> logger)
    {
        _trainers = database.GetCollection<PersonalTrainer>("trainers");
        _memoryCache = memoryCache;
        _logger = logger;
    }
    
    private void SetInCache<T>(string key, T value)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = DateTime.Now.AddHours(1),
            SlidingExpiration = TimeSpan.FromMinutes(10),
            Priority = CacheItemPriority.High
        };
        _memoryCache.Set(key, value, options);
    }

    private T? GetFromCache<T>(string key)
    {
        _memoryCache.TryGetValue(key, out T? value);
        return value;
    }

    private void RemoveFromCache(string key)
    {
        _memoryCache.Remove(key);
    }

    public async Task<List<PersonalTrainer>> GetAllAsync()
    {
        var cached = GetFromCache<List<PersonalTrainer>>(AllTrainersCacheKey);
        if (cached is not null)
        {
            _logger.LogInformation("Cache hit: returning all trainers from cache");
            return cached;
        }

        _logger.LogInformation("Cache miss: fetching all trainers from database");
        var trainers = await _trainers.Find(_ => true).ToListAsync();
        SetInCache(AllTrainersCacheKey, trainers);
        return trainers;
    }

    public async Task<PersonalTrainer?> GetByIdAsync(Guid id)
    {
        var cacheKey = $"trainer_{id}";
        var cached = GetFromCache<PersonalTrainer>(cacheKey);
        if (cached is not null)
        {
            _logger.LogInformation("Cache hit: returning trainer {Id} from cache", id);
            return cached;
        }

        _logger.LogInformation("Cache miss: fetching trainer {Id} from database", id);
        var trainer = await _trainers.Find(t => t.Id == id).FirstOrDefaultAsync();
        if (trainer is not null)
            SetInCache(cacheKey, trainer);
        return trainer;
    }

    public async Task<PersonalTrainer> CreateAsync(TrainerRequest request)
    {
        var trainer = ToTrainer(request);
        await _trainers.InsertOneAsync(trainer);
        RemoveFromCache(AllTrainersCacheKey);
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
        RemoveFromCache(AllTrainersCacheKey);
        RemoveFromCache($"trainer_{id}");
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await _trainers.DeleteOneAsync(t => t.Id == id);
        RemoveFromCache(AllTrainersCacheKey);
        RemoveFromCache($"trainer_{id}");
        return result.DeletedCount > 0;
    }

    public async Task<TrainerBooking?> BookAsync(Guid trainerId, BookingRequest request)
    {
        var trainer = await GetByIdAsync(trainerId);
        if (trainer is null)
            return null;

        var booking = trainer.Book(request.MemberId, request.SessionTime);
        await _trainers.ReplaceOneAsync(t => t.Id == trainerId, trainer);
        RemoveFromCache(AllTrainersCacheKey);
        RemoveFromCache($"trainer_{trainerId}");
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
        var trainers = await GetAllAsync();
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
                        && DateOnly.FromDateTime(b.SessionTime) == date)
            .Select(b => b.SessionTime.Hour)
            .ToList();
    }
}