import React from 'react';

interface Game {
  id: number;
  name: string;
  summary?: string;
  coverUrl?: string;
  rating: number;
  releaseDate: string;
  gameGenres?: Array<{ id: number; name: string }>;
  gamePlatforms?: Array<{ id: number; name: string }>;
  gameCompanies?: Array<{ id: number; name: string; role: string }>;
}

interface GameDetailsModalProps {
  game: Game;
  isOpen: boolean;
  onClose: () => void;
}

const GameDetailsModal: React.FC<GameDetailsModalProps> = ({ game, isOpen, onClose }) => {
  if (!isOpen) return null;

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
        <div className="flex flex-col lg:flex-row gap-6">
          {/* Game Cover */}
          <div className="flex-shrink-0">
            <img
              src={game.coverUrl || '/placeholder-game.jpg'}
              alt={game.name}
              className="w-full lg:w-80 h-auto rounded-lg shadow-lg object-cover"
              onError={(e) => {
                const target = e.target as HTMLImageElement;
                target.src = '/placeholder-game.jpg';
              }}
            />
          </div>

          {/* Game Details */}
          <div className="flex-1">
            <h1 className="text-3xl font-bold mb-4">{game.name}</h1>
            
            {/* Rating and Release Date */}
            <div className="flex flex-wrap gap-4 mb-6">
              <div className="stat bg-base-200 rounded-lg p-4">
                <div className="stat-title">Rating</div>
                <div className={`stat-value text-2xl ${getRatingColor(game.rating)}`}>
                  {game.rating ? `${game.rating.toFixed(1)}/10` : 'Not rated'}
                </div>
              </div>
              <div className="stat bg-base-200 rounded-lg p-4">
                <div className="stat-title">Release Date</div>
                <div className="stat-value text-xl">
                  {formatDate(game.releaseDate)}
                </div>
              </div>
            </div>

            {/* Summary */}
            {game.summary && (
              <div className="mb-6">
                <h3 className="text-xl font-semibold mb-2">Summary</h3>
                <p className="text-base-content/80 leading-relaxed">
                  {game.summary}
                </p>
              </div>
            )}

            {/* Genres */}
            {game.gameGenres && game.gameGenres.length > 0 && (
              <div className="mb-4">
                <h3 className="text-lg font-semibold mb-2">Genres</h3>
                <div className="flex flex-wrap gap-2">
                  {game.gameGenres.map((genre) => (
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
            {game.gamePlatforms && game.gamePlatforms.length > 0 && (
              <div className="mb-4">
                <h3 className="text-lg font-semibold mb-2">Platforms</h3>
                <div className="flex flex-wrap gap-2">
                  {game.gamePlatforms.map((platform) => (
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
            {game.gameCompanies && game.gameCompanies.length > 0 && (
              <div className="mb-4">
                <h3 className="text-lg font-semibold mb-2">Companies</h3>
                <div className="space-y-2">
                  {game.gameCompanies.map((company) => (
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