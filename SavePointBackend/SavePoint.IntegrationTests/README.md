# SavePoint.IntegrationTests

This project contains integration tests for the SavePoint application. These tests verify that multiple components work correctly together, including database operations, API endpoints, and service integrations.

## Test Structure

### API Tests (`/Api`)
- **AuthControllerTests** - Tests authentication endpoints (register, login)
- **GameControllerTests** - Tests game-related API endpoints with real HTTP requests

### Database Tests (`/Database`)
- **GameRepositoryTests** - Tests repository operations with real database

### Service Integration Tests (`/Services`)
- **UserServiceIntegrationTests** - Tests user service with real Identity framework

### Test Fixtures (`/Fixtures`)
- **WebApplicationFixture** - Sets up test web application with test database

## Key Features

### ?? **Real Dependencies**
- **SQL Server LocalDB** - Real database for testing
- **ASP.NET Core Identity** - Real authentication system
- **Entity Framework Core** - Real data access layer
- **HTTP Client** - Real API requests

### ??? **Test Infrastructure**
- **WebApplicationFactory** - Creates test server
- **Test Database** - Isolated database per test run
- **Test Data Seeding** - Bogus library for realistic test data
- **Automatic Cleanup** - Database cleanup after tests

### ?? **Test Categories**

#### **API Integration Tests**
```csharp
[Fact]
public async Task Register_WithValidData_ReturnsSuccess()
{
    var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

#### **Database Integration Tests**
```csharp
[Fact]
public async Task GetGames_WithSearchFilter_ReturnsFilteredResults()
{
    var result = await repository.GetGames(search: searchTerm);
    result.Items.Should().Contain(g => g.Name.Contains(searchTerm));
}
```

#### **Service Integration Tests**
```csharp
[Fact]
public async Task RegisterAsync_WithValidData_CreatesUserAndDefaultLists()
{
    var result = await userService.RegisterAsync(registerDto);
    result.Succeeded.Should().BeTrue();
}
```

## Configuration

### Test Database
- Uses SQL Server LocalDB
- Creates unique database per test run
- Automatically seeds with test data
- Cleans up after test completion

### Test Data
- **Bogus** library generates realistic test data
- Seeded with games, genres, platforms, companies
- Consistent test data across test runs

## Running Tests

```bash
# Run all integration tests
dotnet test SavePoint.IntegrationTests

# Run specific test class
dotnet test SavePoint.IntegrationTests --filter "ClassName=AuthControllerTests"

# Run with verbose output
dotnet test SavePoint.IntegrationTests --verbosity normal

# Run tests in parallel (default)
dotnet test SavePoint.IntegrationTests --parallel
```

## Test Characteristics

### **Slower Execution**
- Database I/O operations
- HTTP request/response cycles
- Real authentication processes
- ~2-10 seconds per test vs milliseconds for unit tests

### **More Comprehensive**
- Tests entire request pipeline
- Validates database transactions
- Verifies serialization/deserialization
- Tests actual configuration

### **Environment Dependencies**
- Requires SQL Server LocalDB
- May need specific .NET SDK version
- Sensitive to configuration changes

## Best Practices Implemented

### ? **Test Isolation**
- Each test gets fresh database
- No shared state between tests
- Independent test data

### ? **Realistic Scenarios**
- End-to-end API workflows
- Real authentication flows
- Complex database queries

### ? **Proper Cleanup**
- Automatic database disposal
- Resource cleanup in fixtures
- No test pollution

### ? **Descriptive Test Names**
- Clear test intentions
- Expected outcomes in names
- Easy to understand failures

## Integration vs Unit Tests

| Aspect | Unit Tests | Integration Tests |
|--------|------------|-------------------|
| **Speed** | Fast (ms) | Slower (seconds) |
| **Scope** | Single component | Multiple components |
| **Dependencies** | Mocked | Real |
| **Confidence** | Component works | System works |
| **Run Frequency** | Every commit | Pre-deployment |
| **Complexity** | Simple setup | Complex setup |

## Test Coverage Areas

- ? **Authentication Flow** - Registration, login, validation
- ? **Game API Endpoints** - CRUD operations, filtering, pagination
- ? **Database Operations** - Repository patterns, complex queries
- ? **Service Layer** - Business logic with real dependencies
- ?? **External Services** - Could add IGDB API integration tests
- ? **Background Jobs** - Could add Hangfire integration tests

## Future Enhancements

1. **External Service Tests** - Test IGDB import with real/stubbed API
2. **Performance Tests** - Load testing for critical endpoints
3. **Security Tests** - Authorization and authentication edge cases
4. **End-to-End Workflows** - Complete user journeys
5. **Docker Integration** - Run tests in containerized environment

This integration test suite provides confidence that the entire SavePoint system works correctly when components are integrated together.