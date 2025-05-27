using Microsoft.EntityFrameworkCore;
using StorkDorkMain.DAL.Abstract;
using StorkDorkMain.Data;
using StorkDorkMain.Models;

namespace StorkDorkMain.DAL.Concrete
{
    public class BirdRepository : Repository<Bird>, IBirdRepository
    {
        private DbSet<Bird> _birds;
        public BirdRepository(StorkDorkDbContext context) : base(context)
        {
            _birds = context.Birds;
        }

        public async Task<IEnumerable<Bird>> GetBirdsByName(string name)
        {
            return await _birds.Where(b => 
                EF.Functions.Like(b.CommonName, $"%{name}%") || 
                EF.Functions.Like(b.ScientificName, $"%{name}%")
            ).ToListAsync();
        }

        public async Task<IEnumerable<Bird>> GetBirdsByTaxonomy(string searchTerm)
        {
            return await _birds.Where(b => 
                EF.Functions.Like(b.Order, $"%{searchTerm}%") || 
                EF.Functions.Like(b.FamilyCommonName, $"%{searchTerm}%") || 
                EF.Functions.Like(b.FamilyScientificName, $"%{searchTerm}%")
            ).ToListAsync();
        }

        public async Task<(IEnumerable<Bird> Birds, int TotalCount)> GetRelatedBirds(
            string speciesCode, 
            string? reportAs, 
            string? categoryFilter = null,
            int page = 1, 
            int pageSize = 10)
        {
            var relatedBirds = new List<Bird>();

            // Find subspecies, forms, and domestic variants that report as the same species
            if (!string.IsNullOrEmpty(reportAs))
            {
                var sameSpecies = await _birds.Where(b => 
                    b.ReportAs == reportAs && 
                    b.SpeciesCode != speciesCode && 
                    (b.Category == "species" || 
                     b.Category == "issf" || 
                     b.Category == "domestic" ||
                     b.Category == "form" ||
                     b.Category == "intergrade"))
                .ToListAsync();
                relatedBirds.AddRange(sameSpecies);
            }

            // Get the current bird's common name to use in hybrid search
            var currentBird = await _birds.FirstOrDefaultAsync(b => b.SpeciesCode == speciesCode);
            if (currentBird != null)
            {
                var nameToSearch = currentBird.CommonName.Split(' ')[0];

                // Find hybrids and intergrades
                var hybrids = await _birds.Where(b => 
                    (b.Category == "hybrid" || b.Category == "intergrade") && 
                    (EF.Functions.Like(b.CommonName, $"%{nameToSearch}%") || 
                     EF.Functions.Like(b.ScientificName, $"%{nameToSearch}%")))
                .ToListAsync();
                relatedBirds.AddRange(hybrids);

                // Find slash and spuh combinations
                var uncertainSpecies = await _birds.Where(b =>
                    (b.Category == "slash" || b.Category == "spuh") &&
                    (EF.Functions.Like(b.CommonName, $"%{nameToSearch}%") ||
                     EF.Functions.Like(b.ScientificName, $"%{nameToSearch}%")))
                .ToListAsync();
                relatedBirds.AddRange(uncertainSpecies);
            }

            // Apply category filter if provided
            var filteredBirds = categoryFilter != null 
                ? relatedBirds.Where(b => b.Category == categoryFilter)
                : relatedBirds;

            // Get total count before pagination
            var totalCount = filteredBirds.Count();

            // Apply pagination
            var paginatedBirds = filteredBirds
                .Distinct()
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            return (paginatedBirds, totalCount);
        }

        private static IEnumerable<string>? _cachedOrders;

        private static IEnumerable<(string ScientificName, string CommonName)>? _cachedFamilies;
        private static readonly object _cacheLock = new object();

        public IEnumerable<string> GetAllOrders()
        {
            if (_cachedOrders == null)
            {
                lock (_cacheLock)
                {
                    if (_cachedOrders == null)
                    {
                        _cachedOrders = _birds
                            .Where(b => b.Order != null)
                            .Select(b => b.Order ?? string.Empty)
                            .Distinct()
                            .OrderBy(o => o)
                            .ToList();
                    }
                }
            }
            return _cachedOrders;
        }

        public IEnumerable<(string ScientificName, string CommonName)> GetAllFamilies()
        {
            if (_cachedFamilies == null)
            {
                lock (_cacheLock)
                {
                    if (_cachedFamilies == null)
                    {
                        _cachedFamilies = _birds
                            .Select(b => new { b.FamilyScientificName, b.FamilyCommonName })
                            .Distinct()
                            .OrderBy(f => f.FamilyCommonName)
                            .AsEnumerable()
                            .Select(f => (ScientificName: f.FamilyScientificName ?? string.Empty, 
                                        CommonName: f.FamilyCommonName ?? string.Empty))
                            .ToList();
                    }
                }
            }
            return _cachedFamilies;
        }

        public async Task<IEnumerable<Bird>> GetBirdsByOrder(string order, string sortOrder = "name")
        {
            if (string.IsNullOrEmpty(order))
                return Enumerable.Empty<Bird>();

            var query = _birds.Where(b => b.Order.ToLower() == order.ToLower());
            
            query = sortOrder switch
            {
                "name" => query.OrderBy(b => b.CommonName),
                "name_desc" => query.OrderByDescending(b => b.CommonName),
                "scientific" => query.OrderBy(b => b.ScientificName),
                "scientific_desc" => query.OrderByDescending(b => b.ScientificName),
                _ => query.OrderBy(b => b.CommonName)
            };
            
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Bird>> GetBirdsByFamily(string familyScientificName, string sortOrder)
        {
            if (string.IsNullOrEmpty(familyScientificName))
                return Enumerable.Empty<Bird>();

            var query = _birds.Where(b => b.FamilyScientificName.ToLower() == familyScientificName.ToLower());
            
            query = ApplySort(query, sortOrder);
            
            return await query.ToListAsync();
        }

        private IQueryable<Bird> ApplySort(IQueryable<Bird> query, string sortOrder)
        {
            return sortOrder switch
            {
                "name_desc" => query.OrderByDescending(b => b.CommonName),
                "scientific" => query.OrderBy(b => b.ScientificName),
                "scientific_desc" => query.OrderByDescending(b => b.ScientificName),
                _ => query.OrderBy(b => b.CommonName) // default sort by name ascending
            };
        }

        public Bird GetBirdBySpeciesCode(string speciesCode)
        {
            return _birds.FirstOrDefault(b => b.SpeciesCode == speciesCode);
        }
    }
}