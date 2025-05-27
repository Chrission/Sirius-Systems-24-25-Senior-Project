using System.Collections.Generic;
using StorkDorkMain.Models;

namespace StorkDorkMain.Models
{
    public class NearbySightingsViewModel
    {
        public int Radius { get; set; } = 25;
        public decimal DefaultLatitude { get; set; }
        public decimal DefaultLongitude { get; set; }
        public IEnumerable<NearbySighting> RecentSightings { get; set; } = new List<NearbySighting>();
    }
    
    public class NearbySighting
    {
        public string SpeciesCode { get; set; }
        public string CommonName { get; set; }
        public string ScientificName { get; set; }
        public string LocationName { get; set; }
        public string ObservationDate { get; set; }
        public int? Count { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}