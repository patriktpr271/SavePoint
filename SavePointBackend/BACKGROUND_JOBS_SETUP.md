# Background Jobs Setup

## Overview
This project uses Hangfire for background job processing with automatic recurring jobs for IGDB data imports.

## Recurring Jobs (Automatic)

### 1. Weekly Incremental Games Import
- **Job Name**: `weekly-games-with-popularity`
- **Schedule**: Every Monday at 2 AM UTC (`0 2 * * 1`)
- **What it does**: 
  - Imports games that have been updated since the last successful import
  - Checks if games already exist in the database
  - Updates existing games if their update date is newer
  - Adds new games
  - Populates popularity data from IGDB API
- **Method**: `ExecuteIncrementalGamesWithPopularityImport()`

### 2. Weekly Incremental Base Data Import
- **Job Name**: `weekly-base-data`
- **Schedule**: Every Monday at 1 AM UTC (`0 1 * * 1`) - runs before games import
- **What it does**:
  - Imports genres, companies, and platforms incrementally
  - Checks if data already exists in the database
  - Updates existing data if update date is newer
  - Adds new data
- **Method**: `ExecuteIncrementalBaseDataImport()`

### 3. Biannual Full Database Sync
- **Job Name**: `biannual-full-sync`
- **Schedule**: January 1st and July 1st at 3 AM UTC (`0 3 1 1,7 *`)
- **What it does**:
  - Full sync of ALL games from IGDB (no date filtering)
  - Only checks by external game ID (doesn't use update dates)
  - Imports ALL games that don't exist in the database
  - Populates popularity data for all imported games
- **Method**: `ExecuteFullDatabaseSync()`

## Manual Job Triggering (via Hangfire Dashboard)

You can manually trigger any of these jobs from the Hangfire dashboard:

### Manual Jobs Available:
1. **🎮 MANUAL: Incremental Games with Popularity Import**
   - Manually run the weekly incremental games import
   
2. **📊 MANUAL: Incremental Base Data Import**
   - Manually run the weekly incremental base data import
   
3. **⚠️ MANUAL: Full Database Sync (One-Time Only)**
   - Manually run the full sync (use sparingly, this is a heavy operation)

## Accessing Jobs

### Hangfire Dashboard
- **URL**: `http://localhost:5044/hangfire` (or your configured port)
- **Authentication**: Requires Admin role
- **Features**:
  - View all recurring jobs
  - Manually trigger any job
  - View job history and status
  - Monitor job execution in real-time
  - See failed jobs and retry them

## Configuration

### Hangfire Settings
- **Database**: SQL Server with schema `HangFire`
- **Worker Count**: Uses `Environment.ProcessorCount`
- **Queues**: `default`, `import`
- **Automatic Retries**: 2 attempts for automatic jobs, 1 for manual jobs

### Job Initialization
Jobs are automatically initialized on application startup via `JobInitializationService`.

## Architecture

### Services Involved:
1. **BackgroundJobService**: Orchestrates all background jobs
2. **IIGDBImportService**: Handles actual IGDB API calls and data import
3. **ImportJobRunRepository**: Tracks job execution history
4. **ImportStatisticsRepository**: Stores import statistics

### Job Tracking:
- Each job run is tracked in the database
- Statistics are updated after each job completes
- Last successful import date is stored for incremental imports
- Job metadata includes execution details and duration

## Import Logic

### Incremental Import Logic:
1. Get last successful import date from database
2. If no previous import exists, default to 7 days ago
3. Query IGDB API with `where updated_at > {last_import_date}`
4. For each game:
   - Check if game exists by external ID
   - If exists: compare update dates, update if newer
   - If doesn't exist: add new game
5. Update statistics and job tracking

### Full Sync Logic:
1. Import ALL games from IGDB without date filtering
2. Only checks if game exists by external game ID
3. Skips games that already exist (doesn't update)
4. Adds all missing games with popularity data

## Monitoring

### View Job Status:
Access via Hangfire dashboard to see:
- Currently running jobs
- Queued jobs
- Completed jobs (last 7 days)
- Failed jobs with error details
- Job execution time
- Records processed/added/updated/failed

### Statistics:
View import statistics via Hangfire dashboard or database queries:
- Total records per data type
- Last successful import timestamp
- Success/failure counts
- Average import duration
- Next scheduled import time

## Troubleshooting

### Jobs Not Running:
1. Check Hangfire dashboard for error messages
2. Verify SQL Server connection
3. Check application logs
4. Ensure `JobInitializationService` started successfully

### Failed Jobs:
1. View error details in Hangfire dashboard
2. Check application logs for detailed error messages
3. Retry failed jobs manually from dashboard
4. Jobs auto-retry up to 2 times

### Manual Trigger Not Working:
1. Ensure you're logged in as Admin
2. Check authentication configuration
3. Verify job is registered in Hangfire

## Notes

- **Full Sync**: Use the biannual full sync sparingly as it's a resource-intensive operation
- **Incremental Updates**: Weekly incremental imports keep data up-to-date efficiently
- **Job Order**: Base data imports run before games to ensure referential integrity
- **Timezone**: All schedules are in UTC
