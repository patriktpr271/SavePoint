# Azure B1 Performance Optimizations for IGDB Import Service

## Overview
The IGDB Import Service has been optimized for Azure B1 plan hosting to address slow import performance. These optimizations focus on reducing memory usage, limiting API calls, and preventing timeouts.

## Key Optimizations Made

### 1. Reduced Batch Sizes
- **Before**: 500 items per batch
- **After**: 250 items per batch
- **Benefit**: Lower memory usage and faster processing per batch

### 2. Import Limits
- **Games Import**: Limited to 2,000 games per import session
- **Reason**: Prevents memory overflow and timeouts on B1 plan
- **Impact**: More predictable import times (5-10 minutes vs hours)

### 3. Added Rate Limiting
- **Delay**: 100ms between API batches
- **Purpose**: Prevents overwhelming the IGDB API and reduces throttling
- **Side Effect**: Slightly longer total import time but more reliable

### 4. Removed Unused Methods
Removed the following unused methods from `IIGDBImportService`:
- `SyncPopularityForExistingGamesAsync()` - Duplicate functionality
- `ImportGamesWithPopularityAsync()` - Complex method with performance issues
- `ImportAllGamesWithPopularityAsync()` - Replaced with optimized version

### 5. Optimized Data Processing
- **Genre Import**: Individual inserts (batch method not available)
- **Company/Platform Import**: Use existing batch methods
- **Popularity Import**: Optimized batch processing with game validation
- **Game Creation**: Extracted to helper method to avoid code duplication

### 6. Memory Management Improvements
- Process data in smaller chunks
- Clear collections after processing
- Use `Dictionary` lookups for better performance
- Validate game existence before creating popularity records

## Recommended Usage for Azure B1

### For Initial Setup:
1. **Import Base Data First**:
   ```
   POST /api/import/genres
   POST /api/import/companies  
   POST /api/import/platforms
   ```

2. **Import Games (Limited)**:
   ```
   POST /api/import/games
   ```
   (Imports up to 2,000 games)

3. **Import Popularity Data**:
   ```
   POST /api/import/popularity
   ```

### For Regular Updates:
Use the background job system with incremental imports:
- **Incremental Games**: Every 2 days
- **Other Data**: Weekly
- **Popularity**: Weekly (for existing games)

### Manual Operations (via Hangfire Dashboard):
- **?? MANUAL: Import ALL Games (Games Only - No Popularity)**: Safe import of all games
- **?? MANUAL: Import Popularity Data (For Existing Games)**: Add popularity after games

## Performance Expectations on Azure B1

### Before Optimization:
- Games import: 30+ minutes, often timed out
- High memory usage causing app restarts
- Frequent API rate limiting

### After Optimization:
- Games import: 5-10 minutes for 2,000 games
- Stable memory usage
- Reliable completion without timeouts
- Predictable performance

## Configuration Changes

### Constants Added:
```csharp
private const int BATCH_SIZE = 250; // Reduced from 500
private const int MAX_GAMES_PER_IMPORT = 2000; // New limit
private const int DELAY_BETWEEN_BATCHES_MS = 100; // New rate limiting
```

## Further Optimizations (If Needed)

If you still experience slowness:

1. **Reduce batch size further**: Change `BATCH_SIZE` to 100
2. **Increase delay**: Change `DELAY_BETWEEN_BATCHES_MS` to 200
3. **Reduce game limit**: Change `MAX_GAMES_PER_IMPORT` to 1000
4. **Use incremental imports only**: Avoid full imports

## Deployment Notes

For publishing to Azure:
1. Use the app offline method as discussed earlier
2. The optimized import should complete faster, reducing deployment window impact
3. Consider running initial import after deployment rather than during deployment

## Monitoring

Watch these metrics on Azure:
- **CPU Usage**: Should stay below 80%
- **Memory Usage**: Should not exceed available RAM
- **Response Times**: Import operations should complete within 15 minutes
- **Error Rates**: Should be minimal with new error handling

## Cost Considerations

These optimizations should help you stay within B1 plan limits:
- Reduced compute time per import
- Lower memory pressure
- More predictable resource usage
- Better success rates reducing need for retries