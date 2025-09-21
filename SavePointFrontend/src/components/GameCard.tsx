import React, { useState } from 'react';
import GameDetailsModal from './GameDetailsModal';

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

interface GameCardProps {
  game: Game;
}

const GameCard: React.FC<GameCardProps> = ({ game }) => {
  const [isModalOpen, setIsModalOpen] = useState(false);

  const handleCardClick = () => {
    setIsModalOpen(true);
  };

  return (
    <>
      <div 
        className="card bg-base-100 shadow-xl cursor-pointer transform transition-transform hover:scale-105 hover:shadow-2xl"
        onClick={handleCardClick}
      >
        <figure className="px-4 pt-4">
          <img
            src={game.coverUrl || '/placeholder-game.jpg'}
            alt={game.name}
            className="rounded-xl w-full h-64 object-cover"
            onError={(e) => {
              const target = e.target as HTMLImageElement;
              target.src = '/placeholder-game.jpg';
            }}
          />
        </figure>
        <div className="card-body px-4 pb-4">
          <h2 className="card-title text-lg font-bold truncate" title={game.name}>
            {game.name}
          </h2>
          <div className="flex items-center justify-between mt-2">
            <div className="rating rating-sm">
              <span className="text-sm font-medium">
                {game.rating ? `${game.rating.toFixed(1)}/10` : 'No rating'}
              </span>
            </div>
            <div className="text-xs text-gray-500">
              {new Date(game.releaseDate).getFullYear()}
            </div>
          </div>
        </div>
      </div>

      <GameDetailsModal
        game={game}
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
      />
    </>
  );
};

export default GameCard;