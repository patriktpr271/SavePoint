using System;
using System.Collections.Generic;

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
        public List<CompanyDto> Companies { get; set; } = new();
    }

    public class GenreDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class PlatformDto  
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class CompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}