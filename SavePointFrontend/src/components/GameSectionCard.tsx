import React from 'react';

interface GameSectionCardProps {
  title: string;
  description: string;
  icon: string;
  gameCount?: number;
  children: React.ReactNode;
}

const GameSectionCard: React.FC<GameSectionCardProps> = ({
  title,
  description,
  icon,
  gameCount,
  children
}) => {
  return (
    <div className="card bg-base-100 shadow-md border border-base-300">
      <div className="card-body p-4">
        {/* Header */}
        <div className="flex items-center justify-between mb-4">
          <div className="flex items-center gap-2">
            <span className="text-xl">{icon}</span>
            <div>
              <h3 className="card-title text-lg">{title}</h3>
              <p className="text-base-content/70 text-xs">{description}</p>
            </div>
          </div>
          {gameCount !== undefined && (
            <div className="badge badge-outline badge-sm">
              {gameCount} games
            </div>
          )}
        </div>

        {/* Content */}
        {children}
      </div>
    </div>
  );
};

export default GameSectionCard;