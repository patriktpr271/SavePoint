using System;

namespace SavePoint.Common.Dtos.Lookups
{
    public class PlatformDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Abbreviation { get; set; }
    }
}