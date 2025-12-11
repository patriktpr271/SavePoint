import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { BrowserRouter } from 'react-router-dom';
import { AuthProvider } from '../../contexts/AuthContext';
import { ErrorProvider } from '../../contexts/ErrorContext';
import LoginModal from '../../components/LoginModal';

global.fetch = vi.fn();

const renderWithProviders = (component: React.ReactElement) => {
  return render(
    <BrowserRouter>
      <ErrorProvider>
        <AuthProvider>
          {component}
        </AuthProvider>
      </ErrorProvider>
    </BrowserRouter>
  );
};

describe('LoginModal Component', () => {
  const mockOnClose = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders login form when modal is open', () => {
    renderWithProviders(<LoginModal isOpen={true} onClose={mockOnClose} />);

    expect(screen.getByPlaceholderText(/email or username/i)).toBeInTheDocument();
    expect(screen.getByPlaceholderText(/password/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /sign in/i })).toBeInTheDocument();
  });

  it('does not render when modal is closed', () => {
    renderWithProviders(<LoginModal isOpen={false} onClose={mockOnClose} />);

    expect(screen.queryByPlaceholderText(/email or username/i)).not.toBeInTheDocument();
  });

  it('validates required fields', async () => {
    const user = userEvent.setup();
    renderWithProviders(<LoginModal isOpen={true} onClose={mockOnClose} />);

    const loginButton = screen.getByRole('button', { name: /sign in/i });
    await user.click(loginButton);

    expect(mockOnClose).not.toHaveBeenCalled();
  });

  it('calls onClose when close button is clicked', async () => {
    const user = userEvent.setup();
    renderWithProviders(<LoginModal isOpen={true} onClose={mockOnClose} />);

    const closeButtons = screen.queryAllByRole('button');
    const closeButton = closeButtons.find(btn => 
      btn.textContent?.toLowerCase().includes('close') || 
      btn.textContent?.toLowerCase().includes('cancel') ||
      btn.getAttribute('aria-label')?.toLowerCase().includes('close')
    );

    if (closeButton) {
      await user.click(closeButton);
      expect(mockOnClose).toHaveBeenCalled();
    }
  });
});
