﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace HWapi.Models;

public partial class Spot
{
    public int SpotId { get; set; }

    public int? CategoryId { get; set; }

    public string SpotTitle { get; set; }

    public string SpotDescription { get; set; }

    public string Address { get; set; }

    public string TrafficInfo { get; set; }

    public string Longitude { get; set; }

    public string Latitude { get; set; }

    public string OpenTime { get; set; }

    public string ContactPhone { get; set; }

    public DateTime? DateCreated { get; set; }
}