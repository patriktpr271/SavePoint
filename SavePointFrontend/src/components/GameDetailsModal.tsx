import React, { useState, useEffect } from 'react';
import { GameCardDto, GameDetailDto } from '../interfaces/types';

interface GameDetailsModalProps {
  game: GameCardDto;
  isOpen: boolean;
  onClose: () => void;
}

const GameDetailsModal: React.FC<GameDetailsModalProps> = ({ game, isOpen, onClose }) => {
  const [gameDetails, setGameDetails] = useState<GameDetailDto | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen && game.id) {
      fetchGameDetails(game.id);
    }
  }, [isOpen, game.id]);

  const fetchGameDetails = async (gameId: string) => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetch(`/api/game/${gameId}`);
      if (!response.ok) {
        throw new Error('Failed to fetch game details');
      }
      const details: GameDetailDto = await response.json();
      setGameDetails(details);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unknown error');
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  // Use gameDetails if available, otherwise fall back to basic game data
  const displayGame = gameDetails || {
    ...game,
    reviewCount: 0,
    averageUserRating: 0,
    genres: [],
    platforms: [],
    companies: []
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  };

  const getRatingColor = (rating: number) => {
    if (rating >= 8) return 'text-green-500';
    if (rating >= 6) return 'text-yellow-500';
    if (rating >= 4) return 'text-orange-500';
    return 'text-red-500';
  };

  return (
    <dialog className="modal modal-open modal-backdrop:bg-gray-800/50 modal-backdrop:backdrop-blur-sm">
      <div className="modal-box max-w-4xl w-full max-h-[90vh] overflow-y-auto">
        {loading ? (
          <div className="flex justify-center items-center h-64">
            <span className="loading loading-spinner loading-lg"></span>
          </div>
        ) : error ? (
          <div className="text-center py-12">
            <div className="text-error text-xl mb-4">Failed to load game details</div>
            <div className="text-base-content/60 mb-4">{error}</div>
            <button className="btn btn-primary" onClick={() => fetchGameDetails(game.id)}>Try Again</button>
          </div>
        ) : (
          <div className="flex flex-col lg:flex-row gap-6">
            {/* Game Cover */}
            <div className="flex-shrink-0">
              <img
                src={displayGame.coverUrl || '/placeholder-game.jpg'}
                alt={displayGame.name}
                className="w-full lg:w-80 h-auto rounded-lg shadow-lg object-cover"
                onError={(e) => {
                  const target = e.target as HTMLImageElement;
                  target.src = '/placeholder-game.jpg';
                }}
              />
            </div>

            {/* Game Details */}
            <div className="flex-1">
              <h1 className="text-3xl font-bold mb-4">{displayGame.name}</h1>
            
            {/* Rating and Release Date */}
            <div className="flex flex-wrap gap-4 mb-6">
              <div className="stat bg-base-200 rounded-lg p-4">
                <div className="stat-title">Rating</div>
                <div className={`stat-value text-2xl ${getRatingColor(displayGame.rating)}`}>
                  {displayGame.rating ? `${displayGame.rating.toFixed(1)}/10` : 'Not rated'}
                </div>
              </div>
              <div className="stat bg-base-200 rounded-lg p-4">
                <div className="stat-title">Release Date</div>
                <div className="stat-value text-xl">
                  {formatDate(displayGame.releaseDate)}
                </div>
              </div>
            </div>

            {/* Summary */}
            {displayGame.summary && (
              <div className="mb-6">
                <h3 className="text-xl font-semibold mb-2">Summary</h3>
                <p className="text-base-content/80 leading-relaxed">
                  {displayGame.summary}
                </p>
              </div>
            )}

            {/* Genres */}
            {displayGame.genres && displayGame.genres.length > 0 && (
              <div className="mb-4">
                <h3 className="text-lg font-semibold mb-2">Genres</h3>
                <div className="flex flex-wrap gap-2">
                  {displayGame.genres.map((genre) => (
                    <span 
                      key={genre.id} 
                      className="badge badge-primary badge-lg"
                    >
                      {genre.name}
                    </span>
                  ))}
                </div>
              </div>
            )}

            {/* Platforms */}
            {displayGame.platforms && displayGame.platforms.length > 0 && (
              <div className="mb-4">
                <h3 className="text-lg font-semibold mb-2">Platforms</h3>
                <div className="flex flex-wrap gap-2">
                  {displayGame.platforms.map((platform) => (
                    <span 
                      key={platform.id} 
                      className="badge badge-secondary badge-lg"
                    >
                      {platform.name}
                    </span>
                  ))}
                </div>
              </div>
            )}

            {/* Companies */}
            {displayGame.companies && displayGame.companies.length > 0 && (
              <div className="mb-4">
                <h3 className="text-lg font-semibold mb-2">Companies</h3>
                <div className="space-y-2">
                  {displayGame.companies.map((company) => (
                    <div key={company.id} className="flex justify-between items-center bg-base-200 rounded-lg p-3">
                      <span className="font-medium">{company.name}</span>
                      <span className="badge badge-outline">{company.role}</span>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>
        </div>
        )}

        {/* Modal Actions */}
        <div className="modal-action">
          <button className="btn btn-primary" onClick={onClose}>
            Close
          </button>
        </div>
      </div>
      <form method="dialog" className="modal-backdrop">
        <button onClick={onClose}>close</button>
      </form>
    </dialog>
  );
};

export default GameDetailsModal;