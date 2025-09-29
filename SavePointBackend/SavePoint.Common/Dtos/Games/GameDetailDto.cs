using System;
using System.Collections.Generic;
using SavePoint.Common.Dtos.Lookups;

namespace SavePoint.Common.Dtos.Games
{
    public class GameDetailDto : GameCardDto
    {
        public string? Summary { get; set; }
        public int ReviewCount { get; set; }
        public double AverageUserRating { get; set; }
        
        // Detailed information with richer structure for frontend
        public List<GenreDto> Genres { get; set; } = new();
        public List<PlatformDto> Platforms { get; set; } = new();
        public List<CompanyGameDto> Companies { get; set; } = new();
    }

    // Special DTO for companies in game context (includes role)
    public class CompanyGameDto : CompanyDto
    {
        public string Role { get; set; } = string.Empty;
    }
}