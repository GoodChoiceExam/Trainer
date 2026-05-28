using FitLife.Trainer.Api.Models;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

namespace FitLife.Trainer.Api.Repositories;

// MongoDB-implementering af ITrainerRepository med in-memory cache.
// Trænere caches i op til 1 time da de sjældent ændrer sig
public class TrainerRepository : ITrainerRepository
{
    private readonly IMongoCollection<PersonalTrainer> _trainers;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<TrainerRepository> _logger;

    private const string AllTrainersCacheKey = "all_trainers";

    public TrainerRepository(IMongoDatabase database, IMemoryCache memoryCache, ILogger<TrainerRepository> logger)
    {
        _trainers = database.GetCollection<PersonalTrainer>("trainers");
        _memoryCache = memoryCache;
        _logger = logger;
    }

    private void SetInCache<T>(string key, T value)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = DateTime.Now.AddHours(1),    // Cachen udløber senest efter 1 time
            SlidingExpiration = TimeSpan.FromMinutes(10),     // Udløber også hvis ikke tilgået i 10 minutter
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

    public async Task<PersonalTrainer> AddAsync(PersonalTrainer trainer)
    {
        await _trainers.InsertOneAsync(trainer);
        // Invaliderer listen af alle trænere så den næste forespørgsel henter fra databasen
        RemoveFromCache(AllTrainersCacheKey);
        return trainer;
    }

    public async Task<PersonalTrainer> UpdateAsync(PersonalTrainer trainer)
    {
        await _trainers.ReplaceOneAsync(t => t.Id == trainer.Id, trainer);
        RemoveFromCache(AllTrainersCacheKey);
        RemoveFromCache($"trainer_{trainer.Id}");
        return trainer;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await _trainers.DeleteOneAsync(t => t.Id == id);
        RemoveFromCache(AllTrainersCacheKey);
        RemoveFromCache($"trainer_{id}");
        return result.DeletedCount > 0;
    }

}
