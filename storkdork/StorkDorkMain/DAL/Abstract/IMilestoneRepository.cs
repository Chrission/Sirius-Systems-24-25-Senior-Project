using StorkDorkMain.Models;

namespace StorkDorkMain.DAL.Abstract;

public interface IMilestoneRepository : IRepository<Milestone>
{
    Task<int> GetSightingsMade(int SDUserID);
    Task<int> GetPhotosContributed(int SDUserID);
    int GetMilestoneTier(int Achievement);
    void IncrementSightingsMade(int SDUserID);
}