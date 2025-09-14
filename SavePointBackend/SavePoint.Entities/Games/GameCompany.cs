namespace SavePoint.Entities.Games
{
    public class GameCompany : Common.BaseEntity
    {
        public Guid GameId { get; set; }
        public Game Game { get; set; }
        public Guid CompanyId { get; set; }
        public Company Company { get; set; }
    }
}