using System;
using System.Collections.Generic;

namespace StorkDorkMain.Models;

public partial class Bird
{
    public int Id { get; set; }

    public string? CommonName { get; set; } 

    public string? ScientificName { get; set; }

    public string? SpeciesCode { get; set; }

    public virtual ICollection<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();

    public virtual ICollection<Sighting> Sightings { get; set; } = new List<Sighting>();
}
