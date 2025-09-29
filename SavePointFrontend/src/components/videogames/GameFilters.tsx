import React, { useState, useEffect } from 'react';
import { GameFilters as GameFiltersType, Genre, Platform, FilterOptions } from '../../interfaces/videogames/types';

interface GameFiltersProps {
  filters: GameFiltersType;
  onFiltersChange: (filters: GameFiltersType) => void;
  onClearFilters: () => void;
}

const GameFiltersComponent: React.FC<GameFiltersProps> = ({ filters, onFiltersChange, onClearFilters }) => {
  const [filterOptions, setFilterOptions] = useState<FilterOptions | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Fetch filter options from backend
    const fetchFilterOptions = async () => {
      try {
        setLoading(true);
        // You'll need to implement these endpoints in your backend
        const [genresRes, platformsRes] = await Promise.all([
          fetch('/api/games/genres'),
          fetch('/api/games/platforms')
        ]);

        const genres: Genre[] = await genresRes.json();
        const platforms: Platform[] = await platformsRes.json();

        setFilterOptions({
          genres,
          platforms,
          minYear: 1980,
          maxYear: new Date().getFullYear()
        });
      } catch (error) {
        console.error('Failed to load filter options:', error);
        // Set default empty options
        setFilterOptions({
          genres: [],
          platforms: [],
          minYear: 1980,
          maxYear: new Date().getFullYear()
        });
      } finally {
        setLoading(false);
      }
    };

    fetchFilterOptions();
  }, []);

  const handleFilterChange = (key: keyof GameFiltersType, value: any) => {
    onFiltersChange({
      ...filters,
      [key]: value
    });
  };

  const toggleGenre = (genreId: number) => {
    const currentGenres = filters.genres || [];
    const newGenres = currentGenres.includes(genreId)
      ? currentGenres.filter(id => id !== genreId)
      : [...currentGenres, genreId];
    handleFilterChange('genres', newGenres);
  };

  const togglePlatform = (platformId: number) => {
    const currentPlatforms = filters.platforms || [];
    const newPlatforms = currentPlatforms.includes(platformId)
      ? currentPlatforms.filter(id => id !== platformId)
      : [...currentPlatforms, platformId];
    handleFilterChange('platforms', newPlatforms);
  };

  if (loading) {
    return (
      <aside className="fixed top-20 left-0 h-[calc(100vh-5rem)] w-64 xl:w-80 bg-base-200 border-r border-base-300 z-40">
        <div className="p-6 h-full overflow-y-auto">
          <div className="flex justify-center items-center h-32">
            <span className="loading loading-spinner loading-md"></span>
          </div>
        </div>
      </aside>
    );
  }

  return (
    <aside className="fixed top-20 left-0 h-[calc(100vh-5rem)] w-64 xl:w-80 bg-base-200 border-r border-base-300 z-40">
      <div className="p-6 h-full overflow-y-auto">
        {/* Header */}
        <div className="flex items-center justify-between mb-6 pb-2 border-b border-base-300">
          <h2 className="text-lg font-semibold text-base-content">Filters</h2>
          <button 
            className="btn btn-ghost btn-xs" 
            onClick={onClearFilters}
          >
            Clear All
          </button>
        </div>

        {/* Search */}
        <div className="mb-6">
          <label className="text-sm font-semibold text-base-content mb-2 block">Search</label>
          <input
            type="text"
            placeholder="Search games..."
            className="input input-bordered input-sm w-full"
            value={filters.search || ''}
            onChange={(e) => handleFilterChange('search', e.target.value)}
          />
        </div>

        {/* Sort */}
        <div className="mb-6">
          <label className="text-sm font-semibold text-base-content mb-2 block">Sort By</label>
          <select
            className="select select-bordered select-sm w-full mb-2"
            value={filters.sortBy || 'name'}
            onChange={(e) => handleFilterChange('sortBy', e.target.value)}
          >
            <option value="name">Name</option>
            <option value="rating">Rating</option>
            <option value="releaseDate">Release Date</option>
            <option value="popularity">Popularity</option>
          </select>
          <select
            className="select select-bordered select-sm w-full"
            value={filters.sortOrder || 'asc'}
            onChange={(e) => handleFilterChange('sortOrder', e.target.value)}
          >
            <option value="asc">Ascending</option>
            <option value="desc">Descending</option>
          </select>
        </div>

        {/* Rating Range */}
        <div className="mb-6">
          <label className="text-sm font-semibold text-base-content mb-2 block">Rating</label>
          <div className="flex gap-2">
            <input
              type="number"
              placeholder="Min"
              min="0"
              max="10"
              step="0.1"
              className="input input-bordered input-sm flex-1"
              value={filters.minRating || ''}
              onChange={(e) => handleFilterChange('minRating', e.target.value ? parseFloat(e.target.value) : undefined)}
            />
            <input
              type="number"
              placeholder="Max"
              min="0"
              max="10"
              step="0.1"
              className="input input-bordered input-sm flex-1"
              value={filters.maxRating || ''}
              onChange={(e) => handleFilterChange('maxRating', e.target.value ? parseFloat(e.target.value) : undefined)}
            />
          </div>
        </div>

        {/* Release Year Range */}
        <div className="mb-6">
          <label className="text-sm font-semibold text-base-content mb-2 block">Release Year</label>
          <div className="flex gap-2">
            <input
              type="number"
              placeholder="From"
              min={filterOptions?.minYear}
              max={filterOptions?.maxYear}
              className="input input-bordered input-sm flex-1"
              value={filters.releaseYearFrom || ''}
              onChange={(e) => handleFilterChange('releaseYearFrom', e.target.value ? parseInt(e.target.value) : undefined)}
            />
            <input
              type="number"
              placeholder="To"
              min={filterOptions?.minYear}
              max={filterOptions?.maxYear}
              className="input input-bordered input-sm flex-1"
              value={filters.releaseYearTo || ''}
              onChange={(e) => handleFilterChange('releaseYearTo', e.target.value ? parseInt(e.target.value) : undefined)}
            />
          </div>
        </div>

        {/* Genres */}
        {filterOptions && filterOptions.genres.length > 0 && (
          <div className="mb-6">
            <label className="text-sm font-semibold text-base-content mb-3 block">Genres</label>
            <div className="space-y-2 max-h-40 overflow-y-auto">
              {filterOptions.genres.map((genre) => (
                <label key={genre.id} className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    className="checkbox checkbox-sm"
                    checked={filters.genres?.includes(genre.id) || false}
                    onChange={() => toggleGenre(genre.id)}
                  />
                  <span className="text-sm text-base-content">{genre.name}</span>
                </label>
              ))}
            </div>
          </div>
        )}

        {/* Platforms */}
        {filterOptions && filterOptions.platforms.length > 0 && (
          <div className="mb-6">
            <label className="text-sm font-semibold text-base-content mb-3 block">Platforms</label>
            <div className="space-y-2 max-h-40 overflow-y-auto">
              {filterOptions.platforms.map((platform) => (
                <label key={platform.id} className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    className="checkbox checkbox-sm"
                    checked={filters.platforms?.includes(platform.id) || false}
                    onChange={() => togglePlatform(platform.id)}
                  />
                  <span className="text-sm text-base-content">{platform.name}</span>
                </label>
              ))}
            </div>
          </div>
        )}
      </div>
    </aside>
  );
};

export default GameFiltersComponent;