import React, { useState } from 'react';
import GameDetailsModal from './GameDetailsModal';
import { GameCardDto, GameDetailDto } from '../interfaces/types';

interface GameCardProps {
  game: GameCardDto;
}

const GameCard: React.FC<GameCardProps> = ({ game }) => {
  const [isModalOpen, setIsModalOpen] = useState(false);

  const handleCardClick = () => {
    setIsModalOpen(true);
  };

  return (
    <>
      <div 
        className="card bg-base-100 shadow-md cursor-pointer transform transition-transform hover:scale-105 hover:shadow-lg border border-base-300"
        onClick={handleCardClick}
      >
        <figure className="px-3 pt-3">
          <div className="aspect-[3/4] w-full overflow-hidden rounded-lg bg-base-200">
            <img
              src={game.coverUrl || '/placeholder-game.jpg'}
              alt={game.name}
              className="w-full h-full object-contain hover:object-cover transition-all duration-300"
              onError={(e) => {
                const target = e.target as HTMLImageElement;
                target.src = '/placeholder-game.jpg';
              }}
            />
          </div>
        </figure>
        <div className="card-body px-3 pb-3 pt-2">
          <h2 className="card-title text-xs font-bold leading-tight min-h-[2rem] flex items-start mb-1" title={game.name}>
            <span className="line-clamp-2 text-left">
              {game.name}
            </span>
          </h2>
          <div className="flex items-center justify-between mt-auto">
            <div className="flex items-center gap-1">
              <span className="text-xs font-medium text-base-content/80">
                {game.rating ? `${game.rating.toFixed(1)}/10` : 'N/A'}
              </span>
            </div>
            <div className="text-xs text-base-content/60 font-medium">
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