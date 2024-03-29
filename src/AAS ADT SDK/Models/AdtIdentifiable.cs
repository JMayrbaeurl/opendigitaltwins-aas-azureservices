﻿using System.Text.Json.Serialization;

namespace AAS.ADT.Models;

public class AdtIdentifiable : AdtReferable
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("administration")]
    public AdtAdministration? Administration { get; set; }
}