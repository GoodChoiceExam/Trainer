using FitLife.Trainer.Api.Models;

namespace FitLife.Trainer.Api.Repositories;

public interface ITrainerRepository
{
    Task<List<PersonalTrainer>> GetAllAsync();
    Task<PersonalTrainer?> GetByIdAsync(Guid id);
    Task<PersonalTrainer> AddAsync(PersonalTrainer trainer);
    Task<PersonalTrainer> UpdateAsync(PersonalTrainer trainer);
    Task<bool> DeleteAsync(Guid id);
}
