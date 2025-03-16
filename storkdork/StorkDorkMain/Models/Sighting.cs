﻿using System;
using System.Collections.Generic;

namespace StorkDorkMain.Models;

public partial class Sighting
{
    public int Id { get; set; }

    public int? SdUserId { get; set; }

    public int? BirdId { get; set; }

    public DateTime? Date { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? Notes { get; set; }

    public virtual Bird? Bird { get; set; }

    public virtual SdUser? SdUser { get; set; }

    // for SD-37
    public string? Country { get; set; }

    public string? CountrySubdivision { get; set; }
}