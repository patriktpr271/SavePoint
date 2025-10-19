# Streamlined Background Jobs Implementation

## Overview
The background job system has been completely streamlined to only include the two essential jobs you requested:

1. **Incremental Games with Popularity Import** - Runs weekly on Monday at 2:00 AM UTC
2. **Incremental Base Data Import** - Runs weekly on Monday at 1:00 AM UTC

## What Was Removed

### From IBackgroundJobService Interface:
- `ScheduleFullImport()`
- `ScheduleIncrementalGamesImport()`
- `ScheduleCompleteGamesImport()`
- `ScheduleImport(string dataType, bool incremental)`
- `ExecuteGamesOnlyImport()`
- `ExecutePopularityOnlyImport()`
- `ManualImportAllGamesOnly()`
- `ManualImportPopularityOnly()`

### From IIGDBImportService Interface:
- `ImportGenresAsync()`
- `ImportGamesAsync()`
- `ImportCompaniesAsync()`
- `ImportPlatformsAsync()`
- `ImportAllDataAsync()`
- `ImportPopularityAsync()`
- `ImportGamesWithBatchedPopularityAsync()`
- `ImportAllGamesOnlyAsync()`
- `ImportPopularityForExistingGamesAsync()`
- All individual incremental methods

### From Controllers:
- Removed all old import endpoints except the two new ones
- Simplified BackgroundJobsController to only essential endpoints

## New Implementation Details

### 1. Incremental Games with Popularity Import

**What it does:**
- Fetches all games updated since the last import (or 7 days ago if first run)
- For each game:
  - Checks if it exists in the database by external ID
  - Creates/updates the game with all relationships (genres, platforms, companies)
  - Fetches and imports popularity data for the game (types 1, 2, 5)
  - Logs whether it's a new game or update

**Schedule:** Every Monday at 2:00 AM UTC
**Job Type:** `GamesWithPopularity`

### 2. Incremental Base Data Import

**What it does:**
- Imports genres updated since last import
- Imports companies updated since last import (uses batch processing)
- Imports platforms updated since last import (uses batch processing)
- Each type checks for existing data and updates accordingly

**Schedule:** Every Monday at 1:00 AM UTC (runs before games import)
**Job Type:** `BaseData`

## API Endpoints

### Background Jobs Controller (`/api/backgroundjobs`)
- `POST /games-with-popularity` - Schedule incremental games with popularity import
- `POST /base-data` - Schedule incremental base data import
- `POST /setup-recurring` - Set up the weekly recurring jobs
- `GET /status` - Get job status and statistics
- `GET /statistics` - Get import statistics
- `DELETE /{jobId}` - Cancel a specific job

### Import Controller (`/api/import`)
- `POST /games-with-popularity` - Manual incremental games with popularity import
- `POST /base-data` - Manual incremental base data import

## Key Features

### Smart Incremental Logic
- Uses IGDB's `updated_at` field to only fetch recently updated data
- Tracks last successful import date per job type
- Defaults to 7 days ago if no previous import exists

### Popularity Integration
- Automatically fetches popularity data for each game during import
- Supports popularity types 1, 2, and 5 from IGDB
- Handles cases where no popularity data exists

### Performance Optimized
- 250 item batch sizes (optimized for Azure B1)
- 100ms delays between API calls to prevent rate limiting
- Efficient database lookups using external IDs

### Error Handling
- Individual game/item processing wrapped in try-catch
- Continues processing even if individual items fail
- Detailed logging for troubleshooting

## Scheduling

Both jobs are set up to run automatically every Monday:
1. **1:00 AM UTC** - Base Data Import (ensures genres, companies, platforms are up to date)
2. **2:00 AM UTC** - Games with Popularity Import (imports games with fresh base data)

## Manual Execution

Both jobs can be triggered manually:
- Via API endpoints (require Admin role)
- Via Hangfire dashboard (marked as MANUAL jobs)
- Immediate execution for testing/maintenance

## Database Tracking

The system tracks:
- Job execution history in `ImportJobRun` table
- Statistics per job type in `ImportStatistics` table
- Last successful import dates for incremental logic
- Success/failure rates and average durations

This streamlined approach provides exactly what you need: efficient, weekly incremental imports that keep your database up to date with minimal resource usage on Azure B1.