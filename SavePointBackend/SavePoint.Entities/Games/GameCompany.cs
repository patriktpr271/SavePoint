using SavePoint.Common.Enums;

namespace SavePoint.Entities.Games
{
    public class GameCompany : Common.BaseEntity
    {
        public Guid GameId { get; set; }
        
        public Guid CompanyId { get; set; }
        
        public CompanyRole Role { get; set; }
    }
}