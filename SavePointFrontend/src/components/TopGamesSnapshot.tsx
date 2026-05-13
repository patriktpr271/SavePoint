import React, { useEffect, useState } from 'react';
import { GameCardDto, TopGamesSnapshotDto } from '../interfaces/types';
import { topGamesService } from '../services/topGamesService';

const formatRelative = (iso: string): string => {
  const diffMs = Date.now() - new Date(iso).getTime();
  const diffMin = Math.floor(diffMs / 60000);
  if (diffMin < 1) return 'just now';
  if (diffMin < 60) return `${diffMin} min ago`;
  const diffH = Math.floor(diffMin / 60);
  if (diffH < 24) return `${diffH} hr ago`;
  return `${Math.floor(diffH / 24)} d ago`;
};

const TopGamesSnapshot: React.FC = () => {
  const [snapshot, setSnapshot] = useState<TopGamesSnapshotDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!topGamesService.isConfigured()) {
      setLoading(false);
      return;
    }

    let cancelled = false;
    topGamesService.getLatest()
      .then((data) => { if (!cancelled) { setSnapshot(data); setLoading(false); } })
      .catch((e: Error) => { if (!cancelled) { setError(e.message); setLoading(false); } });
    return () => { cancelled = true; };
  }, []);

  if (!topGamesService.isConfigured()) {
    return null;
  }

  if (loading) {
    return (
      <div className="card bg-base-100 border border-base-300 shadow-sm">
        <div className="card-body">
          <h2 className="card-title">Top games (Lambda snapshot)</h2>
          <p className="text-base-content/60">Loading…</p>
        </div>
      </div>
    );
  }

  if (error || !snapshot) {
    return (
      <div className="card bg-base-100 border border-base-300 shadow-sm">
        <div className="card-body">
          <h2 className="card-title">Top games (Lambda snapshot)</h2>
          <p className="text-error text-sm">Could not load snapshot: {error ?? 'no data'}</p>
        </div>
      </div>
    );
  }

  const items: GameCardDto[] = snapshot.data.items ?? [];

  return (
    <div className="card bg-base-100 border border-base-300 shadow-sm">
      <div className="card-body">
        <div className="flex items-center justify-between flex-wrap gap-2">
          <h2 className="card-title">
            🛰️ Top games — Lambda snapshot
          </h2>
          <span
            className="badge badge-ghost"
            title={`Source: ${snapshot.source}`}
          >
            updated {formatRelative(snapshot.generatedAt)}
          </span>
        </div>

        {items.length === 0 ? (
          <p className="text-base-content/60">Snapshot is empty — the Lambda hasn't found any popular games yet.</p>
        ) : (
          <ol className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3 mt-2">
            {items.map((game, idx) => (
              <li key={game.id} className="flex flex-col items-center text-center">
                {game.coverUrl ? (
                  <img src={game.coverUrl} alt={game.name} className="w-20 h-28 object-cover rounded mb-2" />
                ) : (
                  <div className="w-20 h-28 bg-base-300 rounded mb-2 flex items-center justify-center text-xs">no cover</div>
                )}
                <div className="text-sm font-medium line-clamp-2">
                  #{idx + 1} {game.name}
                </div>
                {game.rating > 0 && (
                  <div className="text-xs text-base-content/60">★ {game.rating.toFixed(1)}</div>
                )}
              </li>
            ))}
          </ol>
        )}
      </div>
    </div>
  );
};

export default TopGamesSnapshot;
