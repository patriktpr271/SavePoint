import React, { useState, useEffect } from 'react';
import GameCard from '../GameCard';
import Pagination from '../Pagination';
import { GameCardDto } from '../../interfaces/types';
import { GameFilters, GameSearchResponse } from '../../interfaces/videogames/types';

interface GameGridProps {
  filters: GameFilters;
}

const GameGrid: React.FC<GameGridProps> = ({ filters }) => {
  const [games, setGames] = useState<GameCardDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [paginationLoading, setPaginationLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [pagination, setPagination] = useState({
    pageNumber: 1,
    pageSize: 24, // More games per page since we have full screen
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false,
    totalCount: 0
  });

  const buildQueryString = (currentFilters: GameFilters, page: number = 1) => {
    const params = new URLSearchParams();
    params.append('pageNumber', page.toString());
    params.append('pageSize', pagination.pageSize.toString());

    if (currentFilters.search) params.append('search', currentFilters.search);
    if (currentFilters.genres && currentFilters.genres.length > 0) {
      currentFilters.genres.forEach(genreId => params.append('genres', genreId.toString()));
    }
    if (currentFilters.platforms && currentFilters.platforms.length > 0) {
      currentFilters.platforms.forEach(platformId => params.append('platforms', platformId.toString()));
    }
    if (currentFilters.minRating !== undefined) params.append('minRating', currentFilters.minRating.toString());
    if (currentFilters.maxRating !== undefined) params.append('maxRating', currentFilters.maxRating.toString());
    if (currentFilters.releaseYearFrom !== undefined) params.append('releaseYearFrom', currentFilters.releaseYearFrom.toString());
    if (currentFilters.releaseYearTo !== undefined) params.append('releaseYearTo', currentFilters.releaseYearTo.toString());
    if (currentFilters.sortBy) params.append('sortBy', currentFilters.sortBy);
    if (currentFilters.sortOrder) params.append('sortOrder', currentFilters.sortOrder);

    return params.toString();
  };

  const fetchGames = async (page: number = 1, isInitialLoad: boolean = false) => {
    if (isInitialLoad) {
      setLoading(true);
    } else {
      setPaginationLoading(true);
    }
    setError(null);

    try {
      const queryString = buildQueryString(filters, page);
      const response = await fetch(`/api/games?${queryString}`, {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
        },
      });

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data: GameSearchResponse = await response.json();
      setGames(data.items);
      setPagination({
        pageNumber: data.pageNumber,
        pageSize: data.pageSize,
        totalPages: data.totalPages,
        hasNextPage: data.hasNextPage,
        hasPreviousPage: data.hasPreviousPage,
        totalCount: data.totalCount
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to fetch games');
      console.error('Error fetching games:', err);
    } finally {
      if (isInitialLoad) {
        setLoading(false);
      } else {
        setPaginationLoading(false);
      }
    }
  };

  // Fetch games when filters change
  useEffect(() => {
    fetchGames(1, true);
  }, [filters]);

  const handlePageChange = (page: number) => {
    if (page !== pagination.pageNumber && page >= 1 && page <= pagination.totalPages && !paginationLoading) {
      fetchGames(page, false);
      // Scroll to top of grid
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  };

  if (loading) {
    return (
      <div className="flex-1 p-8">
        <div className="flex justify-center items-center h-96">
          <span className="loading loading-spinner loading-lg"></span>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex-1 p-8">
        <div className="flex justify-center items-center h-96">
          <div className="text-center">
            <div className="text-error text-xl mb-4">Failed to load games</div>
            <div className="text-base-content/60 mb-4">{error}</div>
            <button 
              type="button"
              className="btn btn-primary"
              onClick={() => fetchGames(pagination.pageNumber, true)}
            >
              Try Again
            </button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 p-8">
      {/* Results Header */}
      <div className="mb-6">
        <div className="flex items-center justify-between">
          <h2 className="text-2xl font-bold text-base-content">
            {filters.search ? `Search Results for "${filters.search}"` : 'All Games'}
          </h2>
          <div className="text-base-content/60">
            {pagination.totalCount.toLocaleString()} games found
          </div>
        </div>
      </div>

      {/* Games Grid */}
      {games.length > 0 ? (
        <>
          <div className="relative mb-8">
            {/* Pagination Loading Overlay */}
            {paginationLoading && (
              <div className="absolute inset-0 bg-base-200/50 backdrop-blur-sm z-10 flex items-center justify-center rounded-lg">
                <span className="loading loading-spinner loading-lg"></span>
              </div>
            )}
            
            <div className={`grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 2xl:grid-cols-8 gap-4 transition-opacity duration-200 ${paginationLoading ? 'opacity-50' : 'opacity-100'}`}>
              {games.map((game) => (
                <GameCard key={game.id} game={game} />
              ))}
            </div>
          </div>

          {/* Pagination */}
          {pagination.totalPages > 1 && (
            <div className="flex justify-center mt-8">
              <div className="join">
                {/* Previous Button */}
                <button
                  type="button"
                  className={`join-item btn ${!pagination.hasPreviousPage ? 'btn-disabled' : ''}`}
                  onClick={() => handlePageChange(pagination.pageNumber - 1)}
                  disabled={!pagination.hasPreviousPage || paginationLoading}
                >
                  «
                </button>

                {/* Page Numbers */}
                {(() => {
                  const currentPage = pagination.pageNumber;
                  const totalPages = pagination.totalPages;
                  const pageNumbers = [];
                  
                  // Show first page
                  if (currentPage > 3) {
                    pageNumbers.push(1);
                    if (currentPage > 4) pageNumbers.push('...');
                  }
                  
                  // Show pages around current page
                  for (let i = Math.max(1, currentPage - 2); i <= Math.min(totalPages, currentPage + 2); i++) {
                    pageNumbers.push(i);
                  }
                  
                  // Show last page
                  if (currentPage < totalPages - 2) {
                    if (currentPage < totalPages - 3) pageNumbers.push('...');
                    pageNumbers.push(totalPages);
                  }

                  return pageNumbers.map((pageNum, index) => (
                    <button
                      key={index}
                      type="button"
                      className={`join-item btn ${pageNum === currentPage ? 'btn-active' : ''} ${typeof pageNum === 'string' ? 'btn-disabled' : ''}`}
                      onClick={() => typeof pageNum === 'number' ? handlePageChange(pageNum) : undefined}
                      disabled={typeof pageNum === 'string' || paginationLoading}
                    >
                      {pageNum}
                    </button>
                  ));
                })()}

                {/* Next Button */}
                <button
                  type="button"
                  className={`join-item btn ${!pagination.hasNextPage ? 'btn-disabled' : ''}`}
                  onClick={() => handlePageChange(pagination.pageNumber + 1)}
                  disabled={!pagination.hasNextPage || paginationLoading}
                >
                  »
                </button>
              </div>
            </div>
          )}
        </>
      ) : (
        <div className="text-center py-20">
          <div className="text-6xl mb-4">🎮</div>
          <div className="text-xl text-base-content/60 mb-2">No games found</div>
          <div className="text-base-content/40">Try adjusting your filters or search terms</div>
        </div>
      )}
    </div>
  );
};

export default GameGrid;