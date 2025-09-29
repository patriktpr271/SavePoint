import React from 'react';
import Sidebar from './Sidebar';
import PopularGamesSection from './PopularGamesSection';
import { POPULARITY_TYPES } from '../interfaces/types';

const HomeScreen: React.FC = () => {
  return (
    <div className="w-full min-h-screen pt-20">
      {/* Main Content Area - Full Width */}
      <main className="w-full px-4 sm:px-6 lg:px-8 py-8">
        {/* Header Section */}
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-center mb-4">
            🎮 SavePoint Gaming Hub
          </h1>
          <p className="text-xl text-center text-base-content/70 mx-auto">
            Discover the most popular games across different categories
          </p>
        </div>

        {/* Popular Games Sections */}
        <div className="space-y-8">
          {POPULARITY_TYPES.map((popularityType) => (
            <PopularGamesSection 
              key={popularityType.id} 
              popularityType={popularityType} 
            />
          ))}
        </div>
      </main>
    </div>
  );
};

export default HomeScreen;