import { TopGamesSnapshotDto } from '../interfaces/types';

// VITE_TOP_GAMES_URL is the public S3 URL for top-games/latest.json,
// emitted by Terraform and baked in at build time by scripts/deploy-aws.sh.
const TOP_GAMES_URL = (import.meta.env.VITE_TOP_GAMES_URL || '').trim();

export const topGamesService = {
  isConfigured(): boolean {
    return TOP_GAMES_URL.length > 0;
  },

  async getLatest(): Promise<TopGamesSnapshotDto | null> {
    if (!TOP_GAMES_URL) {
      return null;
    }

    // Cache-busting query so the browser doesn't keep stale snapshots.
    const url = `${TOP_GAMES_URL}?t=${Math.floor(Date.now() / 60000)}`;

    const response = await fetch(url, { credentials: 'omit' });
    if (!response.ok) {
      throw new Error(`Failed to load top games snapshot (HTTP ${response.status})`);
    }
    return response.json();
  },
};
