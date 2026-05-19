using FitLife.Trainer.Api.Models;
using MongoDB.Driver;

namespace FitLife.Trainer.Api.Repositories;

public class TrainerRepository : ITrainerRepository
{
    private readonly IMongoCollection<PersonalTrainer> _trainers;

    public TrainerRepository(IMongoDatabase database)
    {
        _trainers = database.GetCollection<PersonalTrainer>("trainers");
    }

    public async Task<List<PersonalTrainer>> GetAllAsync()
    {
        return await _trainers.Find(_ => true).ToListAsync();
    }

    public async Task<PersonalTrainer?> GetByIdAsync(Guid id)
    {
        return await _trainers.Find(t => t.Id == id).FirstOrDefaultAsync();
    }

    public async Task<PersonalTrainer> AddAsync(PersonalTrainer trainer)
    {
        await _trainers.InsertOneAsync(trainer);
        return trainer;
    }

    public async Task<PersonalTrainer> UpdateAsync(PersonalTrainer trainer)
    {
        await _trainers.ReplaceOneAsync(t => t.Id == trainer.Id, trainer);
        return trainer;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await _trainers.DeleteOneAsync(t => t.Id == id);
        return result.DeletedCount > 0;
    }
}
