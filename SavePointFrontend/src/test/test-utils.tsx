import { render, RenderOptions } from '@testing-library/react';
import { BrowserRouter } from 'react-router-dom';
import { AuthProvider } from '../contexts/AuthContext';
import { ErrorProvider } from '../contexts/ErrorContext';

export const renderWithProviders = (
  ui: React.ReactElement,
  options?: Omit<RenderOptions, 'wrapper'>
) => {
  const AllProviders = ({ children }: { children: React.ReactNode }) => (
    <BrowserRouter>
      <ErrorProvider>
        <AuthProvider>
          {children}
        </AuthProvider>
      </ErrorProvider>
    </BrowserRouter>
  );

  return render(ui, { wrapper: AllProviders, ...options });
};

export const mockGameCardDto = {
  id: '123e4567-e89b-12d3-a456-426614174000',
  name: 'Test Game',
  coverUrl: 'https://example.com/cover.jpg',
  rating: 85.5,
  releaseDate: '2023-01-15',
  popularityScore: 100,
};

export const mockUserListDto = {
  id: '123e4567-e89b-12d3-a456-426614174001',
  name: 'My Favorites',
  description: 'Games I love',
  isPublic: true,
  isDefault: false,
  createdAt: '2023-01-01',
  userId: 'user-123',
  gameCount: 5,
  upvotes: 10,
  downvotes: 2,
};

export const mockReviewDto = {
  id: '123e4567-e89b-12d3-a456-426614174002',
  userId: 'user-123',
  gameId: 'game-123',
  rating: 5,
  content: 'Great game!',
  createdAt: '2023-01-01',
  updatedAt: '2023-01-01',
  userName: 'testuser',
  userDisplayName: 'Test User',
};

export const mockPagedResult = <T,>(items: T[]) => ({
  items,
  totalCount: items.length,
  pageNumber: 1,
  pageSize: 10,
  totalPages: Math.ceil(items.length / 10),
  hasNextPage: false,
  hasPreviousPage: false,
});

export const waitForAsync = () => new Promise(resolve => setTimeout(resolve, 0));

export * from '@testing-library/react';
export { default as userEvent } from '@testing-library/user-event';
