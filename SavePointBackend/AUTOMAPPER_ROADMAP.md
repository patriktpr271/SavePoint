# AutoMapper Implementation Roadmap for SavePoint

## Overview
This document provides a comprehensive guide for implementing and extending AutoMapper usage across the SavePoint project.

## Current Implementation

### ? Completed
1. **AutoMapper Setup**
   - Added AutoMapper NuGet packages to BusinessLogic project
   - Configured AutoMapper in Program.cs with dependency injection
   - Created mapping profiles for Games and Users

2. **Game DTOs**
   - `GameCardDto` - For listing/card views
   - `GameDetailDto` - For detailed game information
   - AutoMapper profile: `GameMappingProfile`

3. **User DTOs**
   - `RegisterDto` - For user registration
   - `UserProfileDto` - For user profile display (future use)
   - AutoMapper profile: `UserMappingProfile`

4. **Service Layer Updates**
   - `GameService` now returns DTOs instead of entities
   - `UserService` uses AutoMapper for registration
   - Repository layer remains unchanged (still returns entities)

## Architecture Pattern

```
Controller (DTOs) 
    ?
Service Layer (Entity ? DTO mapping via AutoMapper)
    ?
Repository Layer (Entities)
    ?
Database (Entities via EF Core)
```

## Future DTO Implementation Guide

### Step 1: Create DTOs
Create DTOs in the appropriate namespace under `SavePoint.Common\Dtos\`:

```
SavePoint.Common\Dtos\
??? Games\
?   ??? GameCardDto.cs ?
?   ??? GameDetailDto.cs ?
?   ??? CreateGameDto.cs (future)
?   ??? UpdateGameDto.cs (future)
??? Users\
?   ??? RegisterDto.cs ?
?   ??? UserProfileDto.cs ?
?   ??? LoginDto.cs (future)
?   ??? UpdateUserDto.cs (future)
??? Reviews\
?   ??? ReviewDto.cs (future)
?   ??? CreateReviewDto.cs (future)
?   ??? ReviewSummaryDto.cs (future)
??? Lists\
    ??? UserListDto.cs (future)
    ??? CreateListDto.cs (future)
    ??? ListItemDto.cs (future)
```

### Step 2: Create AutoMapper Profiles
For each domain, create a mapping profile in `SavePoint.BusinessLogic\Mappings\`:

```csharp
// Example: ReviewMappingProfile.cs
public class ReviewMappingProfile : Profile
{
    public ReviewMappingProfile()
    {
        // Entity to DTO
        CreateMap<Review, ReviewDto>();
        
        // Input DTO to Entity
        CreateMap<CreateReviewDto, Review>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }
}
```

### Step 3: Register Profile in Program.cs
Add the new profile to the AutoMapper configuration:

```csharp
builder.Services.AddAutoMapper(typeof(GameMappingProfile), typeof(UserMappingProfile), typeof(ReviewMappingProfile));
```

### Step 4: Update Service Interfaces
Update service interfaces to work with DTOs:

```csharp
public interface IReviewService
{
    Task<PagedResult<ReviewDto>> GetReviewsForGameAsync(Guid gameId, int pageNumber = 1, int pageSize = 20);
    Task<ReviewDto?> GetReviewByIdAsync(Guid id);
    Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto);
}
```

### Step 5: Implement Service Methods
Use AutoMapper in service implementations:

```csharp
public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IMapper _mapper;

    public ReviewService(IReviewRepository reviewRepository, IMapper mapper)
    {
        _reviewRepository = reviewRepository;
        _mapper = mapper;
    }

    public async Task<ReviewDto> CreateReviewAsync(CreateReviewDto dto)
    {
        var review = _mapper.Map<Review>(dto);
        var savedReview = await _reviewRepository.CreateAsync(review);
        return _mapper.Map<ReviewDto>(savedReview);
    }
}
```

## Best Practices

### 1. DTO Naming Conventions
- **Input DTOs**: `Create{Entity}Dto`, `Update{Entity}Dto`
- **Output DTOs**: `{Entity}Dto`, `{Entity}SummaryDto`, `{Entity}DetailDto`
- **Query DTOs**: `{Entity}FilterDto`, `{Entity}SearchDto`

### 2. Mapping Considerations
- **Ignore sensitive fields**: Always ignore passwords, security tokens, etc.
- **Handle null navigation properties**: Use conditional mapping for optional relationships
- **Custom mapping logic**: Use `ForMember()` for complex transformations
- **Context-specific fields**: Handle in service layer (like PopularityScore)

### 3. Performance Optimization
- **Projection mapping**: Use `ProjectTo<TDto>()` for database projections when possible
- **Lazy loading**: Be careful with navigation properties in DTOs
- **Selective inclusion**: Only include necessary fields in DTOs

### 4. Error Handling
```csharp
public async Task<GameDetailDto?> GetGameByIdAsync(Guid id)
{
    var game = await _gameRepository.GetByIdWithDetailsAsync(id);
    if (game == null)
        return null;
    
    return _mapper.Map<GameDetailDto>(game);
}
```

## Planned Future DTOs

### Priority 1 (Next Sprint)
1. **Review System DTOs**
   - `ReviewDto` - Display reviews
   - `CreateReviewDto` - Create new review
   - `ReviewSummaryDto` - Brief review info for game cards

2. **User Management DTOs**
   - `LoginDto` - User authentication
   - `UpdateUserProfileDto` - Profile updates
   - `UserSummaryDto` - Basic user info

### Priority 2 (Future Sprints)
1. **List Management DTOs**
   - `UserListDto` - User game lists
   - `CreateListDto` - Create new list
   - `ListItemDto` - Individual list items

2. **Search and Filter DTOs**
   - `GameSearchDto` - Game search parameters
   - `GameFilterDto` - Game filtering options

### Priority 3 (Advanced Features)
1. **Analytics DTOs**
   - `PopularityDetailDto` - Detailed popularity metrics
   - `UserActivityDto` - User activity tracking

2. **Admin DTOs**
   - `AdminGameDto` - Game management
   - `AdminUserDto` - User management

## Migration Strategy for Existing Code

1. **Gradual Migration**: Migrate one domain at a time (Games ?, Users ?, Reviews next)
2. **Backward Compatibility**: Keep existing endpoints working during migration
3. **Testing**: Add comprehensive tests for mapping configurations
4. **Documentation**: Update API documentation with new DTO schemas

## Common Mapping Patterns

### Complex Property Mapping
```csharp
CreateMap<Game, GameCardDto>()
    .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => 
        src.GameGenres.Select(gg => gg.Genre.Name).ToList()));
```

### Conditional Mapping
```csharp
CreateMap<User, UserProfileDto>()
    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => 
        src.EmailPublic ? src.Email : "[Private]"));
```

### Flattening
```csharp
CreateMap<Review, ReviewDto>()
    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
    .ForMember(dest => dest.GameName, opt => opt.MapFrom(src => src.Game.Name));
```

## Testing AutoMapper Configurations

Create unit tests for mapping profiles:

```csharp
[Test]
public void GameMappingProfile_ShouldBeValid()
{
    var configuration = new MapperConfiguration(cfg => cfg.AddProfile<GameMappingProfile>());
    configuration.AssertConfigurationIsValid();
}
```

## Conclusion

This roadmap provides a structured approach to implementing DTOs across the SavePoint project. Following these patterns will ensure consistent, maintainable, and efficient data transfer throughout the application.

**Key Benefits:**
- ? Separation of concerns between API contracts and domain models
- ? Better performance through selective data transfer
- ? Enhanced security by controlling exposed data
- ? Improved maintainability with consistent mapping patterns
- ? Future-proof architecture for API versioning