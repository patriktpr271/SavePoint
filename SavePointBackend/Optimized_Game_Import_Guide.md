# Optimized Game Import with Batch Popularity and Full Sync

## Major Optimizations Made

### 1. ?? **Batch Popularity Import**
**Problem:** The original implementation made individual API calls for each game's popularity (very inefficient).

**Solution:** Now batches all games and fetches popularity for multiple games in a single API call per popularity type.

#### Before (Inefficient):
```csharp
// For each game individually
foreach (var game in games)
{
    await ImportPopularityForGame(game.Id.Value, gameEntity.Id); // Individual API call
}
```

#### After (Optimized):
```csharp
// Process all games first, then batch import popularity
var gameEntities = new List<(Game entity, long externalId)>();
foreach (var game in games)
{
    // Process game...
    gameEntities.Add((gameEntity, game.Id.Value));
}
// Single batched popularity import for all games
await ImportPopularityForGamesBatch(gameEntities);
```

**Performance Impact:**
- **Before**: 250 games = 750 API calls (250 games × 3 popularity types)
- **After**: 250 games = 3 API calls (1 per popularity type for all games)
- **~99% reduction in API calls!**

### 2. ?? **New Full Database Sync Job**

Added a third job type for one-time full synchronization:

#### **ImportAllGamesFullSyncAsync()**
- **Purpose**: Import ALL games from IGDB that don't exist in your database
- **Logic**: Checks only by external game ID (no date filtering)
- **Use Case**: Run once initially to populate your database completely
- **Efficiency**: Skips games that already exist, only imports missing ones

#### Key Features:
- Loads all existing external IDs into memory at start for fast lookups
- Processes games in batches of 250
- Includes batch popularity import for new games
- Tracks: new games added, existing games skipped

## Updated Job Structure

### 1. **Incremental Games with Popularity** (Weekly - Monday 2 AM)
- Imports games updated since last run
- Uses optimized batch popularity import
- Perfect for keeping database up-to-date

### 2. **Incremental Base Data** (Weekly - Monday 1 AM)  
- Imports updated genres, companies, platforms
- Runs before games import

### 3. **Full Database Sync** (One-time/Manual only)
- Imports ALL missing games from IGDB
- No recurring schedule
- Perfect for initial database population

## API Endpoints

### Background Jobs (`/api/backgroundjobs`)
```http
POST /games-with-popularity     # Schedule incremental games
POST /base-data                # Schedule incremental base data  
POST /full-sync                # Schedule full database sync
POST /setup-recurring          # Setup weekly recurring jobs
GET  /status                   # Get job status
GET  /statistics               # Get import statistics
DELETE /{jobId}                # Cancel job
```

### Direct Import (`/api/import`) 
```http
POST /games-with-popularity     # Direct incremental games import
POST /base-data                # Direct incremental base data import
POST /full-sync                # Direct full database sync
```

## Hangfire Dashboard Jobs

### Automatic Jobs:
- **?? Incremental Games with Popularity Import** - Weekly automation
- **?? Incremental Base Data Import** - Weekly automation  
- **?? Full Database Sync** - Manual execution only

### Manual Jobs:
- **?? MANUAL: Incremental Games with Popularity Import**
- **?? MANUAL: Incremental Base Data Import**
- **?? MANUAL: Full Database Sync (One-Time Only)**

## Usage Recommendations

### Initial Setup:
1. **Run Full Sync Once**: `POST /api/backgroundjobs/full-sync`
   - This populates your database with ALL games from IGDB
   - Takes longer but only needs to run once

2. **Setup Recurring Jobs**: `POST /api/backgroundjobs/setup-recurring`
   - Sets up weekly incremental imports
   - Keeps your database updated automatically

### Ongoing Operations:
- The weekly jobs handle everything automatically
- Full sync is manual-only (never runs automatically)
- Monitor via `/api/backgroundjobs/status`

## Performance Benefits

### API Efficiency:
- **99% reduction** in popularity API calls
- Batch processing reduces API rate limiting
- More reliable imports on Azure B1

### Database Efficiency:
- Smart existence checking using HashSet lookups
- Batch database operations where possible
- Reduced memory usage with targeted queries

### Azure B1 Optimized:
- 250-item batches prevent memory issues
- 100ms delays prevent API throttling
- Efficient error handling continues processing

## Example Full Sync Output:
```
Starting full database sync - importing all games not in database
Found 1,250 existing games in database
Processing batch starting at offset 0...
Added new game: Cyberpunk 2077
Added new game: The Witcher 3
Skipped existing game: Half-Life 2
Importing popularity for 47 games...
Imported 141 popularity records
Processed batch of 250 games. New: 47, Skipped: 203, Total processed: 47
...
Full database sync completed! New games added: 2,847, Total processed: 2,847, Skipped existing: 15,423
```

This optimized approach gives you the best of both worlds: efficient incremental updates for ongoing maintenance and a powerful one-time sync for initial population!