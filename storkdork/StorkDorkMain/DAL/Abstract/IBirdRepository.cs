using StorkDorkMain.Models;

namespace StorkDorkMain.DAL.Abstract
{
    public interface IBirdRepository : IRepository<Bird>
    {
        Task<IEnumerable<Bird>> GetBirdsByName(string name);
        Task<IEnumerable<Bird>> GetBirdsByTaxonomy(string searchTerm);
        Task<(IEnumerable<Bird> Birds, int TotalCount)> GetRelatedBirds(
            string speciesCode, 
            string? reportAs, 
            string? categoryFilter = null,
            int page = 1, 
            int pageSize = 10);

        IEnumerable<string> GetAllOrders();
        IEnumerable<(string ScientificName, string CommonName)> GetAllFamilies();
        Task<IEnumerable<Bird>> GetBirdsByOrder(string order, string sortOrder = "name");
        Task<IEnumerable<Bird>> GetBirdsByFamily(string familyScientificName, string sortOrder = "name");
        Bird GetBirdBySpeciesCode(string speciesCode);
    }
}