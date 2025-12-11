import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import GameCard from '../../components/GameCard';
import { GameCardDto } from '../../interfaces/types';

vi.mock('../../components/GameDetailsModal', () => ({
  default: ({ isOpen, onClose }: { isOpen: boolean; onClose: () => void }) => (
    isOpen ? (
      <div data-testid="game-details-modal">
        <button onClick={onClose}>Close Modal</button>
      </div>
    ) : null
  ),
}));

describe('GameCard Component', () => {
  const mockGame: GameCardDto = {
    id: '123e4567-e89b-12d3-a456-426614174000',
    name: 'The Legend of Zelda: Breath of the Wild',
    coverUrl: 'https://example.com/cover.jpg',
    rating: 95.5,
    releaseDate: '2017-03-03',
  };

  it('renders game information correctly', () => {
    render(<GameCard game={mockGame} />);

    expect(screen.getByText(mockGame.name)).toBeInTheDocument();
    expect(screen.getByText('95.5/10')).toBeInTheDocument();
    expect(screen.getByText('2017')).toBeInTheDocument();
  });

  it('displays N/A when rating is not available', () => {
    const gameWithoutRating = {
      ...mockGame,
      rating: null as any,
    };

    render(<GameCard game={gameWithoutRating} />);

    expect(screen.getByText('N/A')).toBeInTheDocument();
  });

  it('renders game cover image with correct src', () => {
    render(<GameCard game={mockGame} />);

    const image = screen.getByAltText(mockGame.name) as HTMLImageElement;
    expect(image).toBeInTheDocument();
    expect(image.src).toBe(mockGame.coverUrl);
  });

  it('uses placeholder when coverUrl is not provided', () => {
    const gameWithoutCover: GameCardDto = {
      ...mockGame,
      coverUrl: undefined,
    };

    render(<GameCard game={gameWithoutCover} />);

    const image = screen.getByAltText(mockGame.name) as HTMLImageElement;
    expect(image.src).toContain('placeholder-game.jpg');
  });

  it('opens modal when card is clicked', async () => {
    const user = userEvent.setup();
    render(<GameCard game={mockGame} />);

    const card = screen.getByRole('img', { name: mockGame.name }).closest('div')?.parentElement;
    expect(card).toBeInTheDocument();

    if (card) {
      await user.click(card);
      expect(screen.getByTestId('game-details-modal')).toBeInTheDocument();
    }
  });

  it('applies cursor-pointer class for interactive feedback', () => {
    render(<GameCard game={mockGame} />);

    const image = screen.getByRole('img', { name: mockGame.name });
    const card = image.closest('[class*="card"]');
    expect(card).toHaveClass('cursor-pointer');
  });
});
