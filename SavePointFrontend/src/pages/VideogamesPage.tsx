import React, { useState } from 'react';
import GameFiltersComponent from '../components/videogames/GameFilters';
import GameGrid from '../components/videogames/GameGrid';
import { GameFilters as GameFiltersType } from '../interfaces/videogames/types';

const VideogamesPage: React.FC = () => {
  const [filters, setFilters] = useState<GameFiltersType>({
    search: '',
    genres: [],
    platforms: [],
    sortBy: 'name',
    sortOrder: 'asc'
  });

  const handleFiltersChange = (newFilters: GameFiltersType) => {
    setFilters(newFilters);
  };

  const handleClearFilters = () => {
    setFilters({
      search: '',
      genres: [],
      platforms: [],
      sortBy: 'name',
      sortOrder: 'asc'
    });
  };

  return (
    <div className="w-full min-h-screen pt-20 bg-base-200">
      <div className="flex">
        {/* Sidebar with Filters */}
        <GameFiltersComponent
          filters={filters}
          onFiltersChange={handleFiltersChange}
          onClearFilters={handleClearFilters}
        />

        {/* Main Content Area */}
        <main className="flex-1 ml-64 xl:ml-80">
          <GameGrid filters={filters} />
        </main>
      </div>
    </div>
  );
};

export default VideogamesPage;