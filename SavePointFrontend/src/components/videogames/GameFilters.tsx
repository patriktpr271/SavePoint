import React, { useState, useEffect } from 'react';
import { GameFilters as GameFiltersType, Genre, Platform, Company, FilterOptions } from '../../interfaces/videogames/types';

interface GameFiltersProps {
  filters: GameFiltersType;
  onFiltersChange: (filters: GameFiltersType) => void;
  onClearFilters: () => void;
}

const GameFiltersComponent: React.FC<GameFiltersProps> = ({ filters, onFiltersChange, onClearFilters }) => {
  const [filterOptions, setFilterOptions] = useState<FilterOptions | null>(null);
  const [loading, setLoading] = useState(true);
  const [platformSearch, setPlatformSearch] = useState('');
  const [showPlatformDropdown, setShowPlatformDropdown] = useState(false);
  const [filteredPlatforms, setFilteredPlatforms] = useState<Platform[]>([]);
  const [companySearch, setCompanySearch] = useState('');
  const [showCompanyDropdown, setShowCompanyDropdown] = useState(false);
  const [filteredCompanies, setFilteredCompanies] = useState<Company[]>([]);

  useEffect(() => {
    // Fetch filter options from backend
    const fetchFilterOptions = async () => {
      try {
        setLoading(true);
        // You'll need to implement these endpoints in your backend
        const [genresRes, platformsRes, companiesRes] = await Promise.all([
          fetch('/api/Lookup/genres'),
          fetch('/api/Lookup/platforms'),
          fetch('/api/Lookup/companies')
        ]);

        const genres: Genre[] = await genresRes.json();
        const platforms: Platform[] = await platformsRes.json();
        const companies: Company[] = await companiesRes.json();

        setFilterOptions({
          genres,
          platforms,
          companies,
          minYear: 1980,
          maxYear: new Date().getFullYear()
        });
      } catch (error) {
        console.error('Failed to load filter options:', error);
        // Set default empty options
        setFilterOptions({
          genres: [],
          platforms: [],
          companies: [],
          minYear: 1980,
          maxYear: new Date().getFullYear()
        });
      } finally {
        setLoading(false);
      }
    };

    fetchFilterOptions();
  }, []);

  // Filter platforms based on search
  useEffect(() => {
    if (filterOptions?.platforms && platformSearch) {
      const filtered = filterOptions.platforms.filter(platform => 
        platform.name.toLowerCase().includes(platformSearch.toLowerCase())
      );
      setFilteredPlatforms(filtered);
    } else {
      setFilteredPlatforms([]);
    }
  }, [platformSearch, filterOptions?.platforms]);

  // Filter companies based on search
  useEffect(() => {
    if (filterOptions?.companies && companySearch) {
      const filtered = filterOptions.companies.filter(company => 
        company.name.toLowerCase().includes(companySearch.toLowerCase())
      );
      setFilteredCompanies(filtered);
    } else {
      setFilteredCompanies([]);
    }
  }, [companySearch, filterOptions?.companies]);

  const handleFilterChange = (key: keyof GameFiltersType, value: any) => {
    onFiltersChange({
      ...filters,
      [key]: value
    });
  };

  const toggleGenre = (genreId: string) => {
    const currentGenres = filters.genres || [];
    const newGenres = currentGenres.includes(genreId)
      ? currentGenres.filter(id => id !== genreId)
      : [...currentGenres, genreId];
    handleFilterChange('genres', newGenres);
  };

  const togglePlatform = (platformId: string) => {
    const currentPlatforms = filters.platforms || [];
    const newPlatforms = currentPlatforms.includes(platformId)
      ? currentPlatforms.filter(id => id !== platformId)
      : [...currentPlatforms, platformId];
    handleFilterChange('platforms', newPlatforms);
  };

  const toggleCompany = (companyId: string) => {
    const currentCompanies = filters.companies || [];
    const newCompanies = currentCompanies.includes(companyId)
      ? currentCompanies.filter(id => id !== companyId)
      : [...currentCompanies, companyId];
    handleFilterChange('companies', newCompanies);
  };

  // Platform helper functions
  const getSelectedPlatformNames = () => {
    if (!filterOptions?.platforms || !filters.platforms) return [];
    return filterOptions.platforms
      .filter(platform => filters.platforms!.includes(platform.id))
      .map(platform => platform.name);
  };

  const addPlatform = (platform: Platform) => {
    const currentPlatforms = filters.platforms || [];
    if (!currentPlatforms.includes(platform.id)) {
      handleFilterChange('platforms', [...currentPlatforms, platform.id]);
    }
    setPlatformSearch('');
    setShowPlatformDropdown(false);
  };

  const removePlatform = (platformId: string) => {
    const currentPlatforms = filters.platforms || [];
    handleFilterChange('platforms', currentPlatforms.filter(id => id !== platformId));
  };

  // Company helper functions
  const getSelectedCompanyNames = () => {
    if (!filterOptions?.companies || !filters.companies) return [];
    return filterOptions.companies
      .filter(company => filters.companies!.includes(company.id))
      .map(company => company.name);
  };

  const addCompany = (company: Company) => {
    const currentCompanies = filters.companies || [];
    if (!currentCompanies.includes(company.id)) {
      handleFilterChange('companies', [...currentCompanies, company.id]);
    }
    setCompanySearch('');
    setShowCompanyDropdown(false);
  };

  const removeCompany = (companyId: string) => {
    const currentCompanies = filters.companies || [];
    handleFilterChange('companies', currentCompanies.filter(id => id !== companyId));
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
              value={filters.fromYear || ''}
              onChange={(e) => handleFilterChange('fromYear', e.target.value ? parseInt(e.target.value) : undefined)}
            />
            <input
              type="number"
              placeholder="To"
              min={filterOptions?.minYear}
              max={filterOptions?.maxYear}
              className="input input-bordered input-sm flex-1"
              value={filters.toYear || ''}
              onChange={(e) => handleFilterChange('toYear', e.target.value ? parseInt(e.target.value) : undefined)}
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
            
            {/* Selected Platforms */}
            {filters.platforms && filters.platforms.length > 0 && (
              <div className="mb-3">
                <div className="flex flex-wrap gap-1">
                  {filters.platforms.map((platformId) => {
                    const platform = filterOptions.platforms.find(p => p.id === platformId);
                    return platform ? (
                      <span
                        key={platformId}
                        className="badge badge-secondary badge-sm gap-1 cursor-pointer"
                        onClick={() => removePlatform(platformId)}
                      >
                        {platform.name}
                        <svg className="w-3 h-3 fill-current" viewBox="0 0 20 20">
                          <path d="M14.348 5.652a.5.5 0 0 0-.707 0L10 9.293 6.36 5.652a.5.5 0 1 0-.707.707L9.293 10l-3.64 3.64a.5.5 0 0 0 .707.708L10 10.707l3.64 3.641a.5.5 0 0 0 .708-.708L10.707 10l3.641-3.648a.5.5 0 0 0 0-.707z"/>
                        </svg>
                      </span>
                    ) : null;
                  })}
                </div>
              </div>
            )}
            
            {/* Platform Search */}
            <div className="relative">
              <input
                type="text"
                placeholder="Search and add platforms..."
                className="input input-bordered input-sm w-full"
                value={platformSearch}
                onChange={(e) => {
                  setPlatformSearch(e.target.value);
                  setShowPlatformDropdown(true);
                }}
                onFocus={() => setShowPlatformDropdown(true)}
                onBlur={() => setTimeout(() => setShowPlatformDropdown(false), 200)}
              />
              
              {/* Dropdown */}
              {showPlatformDropdown && platformSearch && filteredPlatforms.length > 0 && (
                <div className="absolute z-50 w-full mt-1 bg-base-100 border border-base-300 rounded-lg shadow-lg max-h-40 overflow-y-auto">
                  {filteredPlatforms.slice(0, 10).map((platform) => (
                    <div
                      key={platform.id}
                      className="px-3 py-2 hover:bg-base-200 cursor-pointer text-sm border-b border-base-300 last:border-b-0"
                      onClick={() => addPlatform(platform)}
                    >
                      {platform.name}
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        )}

        {/* Companies */}
        {filterOptions && filterOptions.companies.length > 0 && (
          <div className="mb-6">
            <label className="text-sm font-semibold text-base-content mb-3 block">Companies</label>
            
            {/* Selected Companies */}
            {filters.companies && filters.companies.length > 0 && (
              <div className="mb-3">
                <div className="flex flex-wrap gap-1">
                  {filters.companies.map((companyId) => {
                    const company = filterOptions.companies.find(c => c.id === companyId);
                    return company ? (
                      <span
                        key={companyId}
                        className="badge badge-accent badge-sm gap-1 cursor-pointer"
                        onClick={() => removeCompany(companyId)}
                      >
                        {company.name}
                        <svg className="w-3 h-3 fill-current" viewBox="0 0 20 20">
                          <path d="M14.348 5.652a.5.5 0 0 0-.707 0L10 9.293 6.36 5.652a.5.5 0 1 0-.707.707L9.293 10l-3.64 3.64a.5.5 0 0 0 .707.708L10 10.707l3.64 3.641a.5.5 0 0 0 .708-.708L10.707 10l3.641-3.648a.5.5 0 0 0 0-.707z"/>
                        </svg>
                      </span>
                    ) : null;
                  })}
                </div>
              </div>
            )}
            
            {/* Company Search */}
            <div className="relative">
              <input
                type="text"
                placeholder="Search and add companies..."
                className="input input-bordered input-sm w-full"
                value={companySearch}
                onChange={(e) => {
                  setCompanySearch(e.target.value);
                  setShowCompanyDropdown(true);
                }}
                onFocus={() => setShowCompanyDropdown(true)}
                onBlur={() => setTimeout(() => setShowCompanyDropdown(false), 200)}
              />
              
              {/* Dropdown */}
              {showCompanyDropdown && companySearch && filteredCompanies.length > 0 && (
                <div className="absolute z-50 w-full mt-1 bg-base-100 border border-base-300 rounded-lg shadow-lg max-h-40 overflow-y-auto">
                  {filteredCompanies.slice(0, 10).map((company) => (
                    <div
                      key={company.id}
                      className="px-3 py-2 hover:bg-base-200 cursor-pointer text-sm border-b border-base-300 last:border-b-0"
                      onClick={() => addCompany(company)}
                    >
                      {company.name}
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </aside>
  );
};

export default GameFiltersComponent;