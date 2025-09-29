using System;

namespace SavePoint.Common.Dtos.Games
{
    public class GameCardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? CoverUrl { get; set; }
        public double Rating { get; set; }
        public DateTime ReleaseDate { get; set; }       
        public decimal? PopularityScore { get; set; }
    }
}