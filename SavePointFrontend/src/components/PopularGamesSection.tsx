import React, { useState, useEffect } from 'react';
import GameCard from './GameCard';
import GameSectionCard from './GameSectionCard';
import Pagination from './Pagination';
import { GameCardDto, PagedResult, PopularityType } from '../interfaces/types';

interface PopularGamesSectionProps {
  popularityType: PopularityType;
}

const PopularGamesSection: React.FC<PopularGamesSectionProps> = ({ popularityType }) => {
  const [games, setGames] = useState<GameCardDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [paginationLoading, setPaginationLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [pagination, setPagination] = useState({
    pageNumber: 1,
    pageSize: 8,
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false,
    totalCount: 0
  });

  const fetchGames = async (page: number = 1, isInitialLoad: boolean = false) => {
    if (isInitialLoad) {
      setLoading(true);
    } else {
      setPaginationLoading(true);
    }
    setError(null);
    
    try {
      const response = await fetch(
        `/api/game/popular/${popularityType.id}?pageNumber=${page}&pageSize=8`,
        {
          method: 'GET',
          headers: {
            'Content-Type': 'application/json',
          },
        }
      );

      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      const data: PagedResult<GameCardDto> = await response.json();
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
      setError(err instanceof Error ? err.message : `Failed to fetch ${popularityType.name} games`);
      console.error(`Error fetching ${popularityType.name} games:`, err);
    } finally {
      if (isInitialLoad) {
        setLoading(false);
      } else {
        setPaginationLoading(false);
      }
    }
  };

  useEffect(() => {
    fetchGames(1, true);
  }, [popularityType.id]);

  const handlePageChange = (page: number) => {
    console.log('Page change requested:', { from: pagination.pageNumber, to: page });
    if (page !== pagination.pageNumber && page >= 1 && page <= pagination.totalPages && !paginationLoading) {
      fetchGames(page, false);
    }
  };

  const handleRetry = (e: React.MouseEvent<HTMLButtonElement>) => {
    e.preventDefault();
    e.stopPropagation();
    fetchGames(pagination.pageNumber, true);
  };

  if (loading) {
    return (
      <GameSectionCard
        title={popularityType.name}
        description={popularityType.description}
        icon={popularityType.icon}
      >
        <div className="flex justify-center items-center h-24">
          <span className="loading loading-spinner loading-md"></span>
        </div>
      </GameSectionCard>
    );
  }

  if (error) {
    return (
      <GameSectionCard
        title={popularityType.name}
        description={popularityType.description}
        icon={popularityType.icon}
      >
        <div className="text-center py-6">
          <div className="text-error text-md mb-2">Failed to load games</div>
          <div className="text-base-content/60 text-xs mb-3">{error}</div>
          <button 
            type="button"
            className="btn btn-primary btn-xs"
            onClick={handleRetry}
          >
            Try Again
          </button>
        </div>
      </GameSectionCard>
    );
  }

  return (
    <GameSectionCard
      title={popularityType.name}
      description={popularityType.description}
      icon={popularityType.icon}
      gameCount={pagination.totalCount}
    >
      {/* Games Grid */}
      {games.length > 0 ? (
        <>
          <div className="relative">
            {/* Pagination Loading Overlay */}
            {paginationLoading && (
              <div className="absolute inset-0 bg-base-200/50 backdrop-blur-sm z-10 flex items-center justify-center rounded-lg">
                <span className="loading loading-spinner loading-md"></span>
              </div>
            )}
            
            <div className={`grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 2xl:grid-cols-8 gap-3 transition-opacity duration-200 ${paginationLoading ? 'opacity-50' : 'opacity-100'}`}>
              {games.map((game) => (
                <GameCard key={game.id} game={game} />
              ))}
            </div>
          </div>
          
          {/* Pagination */}
          {pagination.totalPages > 1 && (
            <Pagination
              currentPage={pagination.pageNumber}
              totalPages={pagination.totalPages}
              onPageChange={handlePageChange}
              hasNextPage={pagination.hasNextPage}
              hasPreviousPage={pagination.hasPreviousPage}
            />
          )}
        </>
      ) : (
        <div className="text-center py-6">
          <div className="text-base-content/60 text-sm">
            No games found in this category
          </div>
        </div>
      )}
    </GameSectionCard>
  );
};

export default PopularGamesSection;