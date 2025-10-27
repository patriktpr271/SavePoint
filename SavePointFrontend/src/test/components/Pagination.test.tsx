import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import Pagination from '../../components/Pagination';

describe('Pagination Component', () => {
  it('renders current page and total pages', () => {
    render(
      <Pagination
        currentPage={2}
        totalPages={10}
        onPageChange={vi.fn()}
        hasNextPage={true}
        hasPreviousPage={true}
      />
    );

    expect(screen.getByText('Page 2 of 10')).toBeInTheDocument();
  });

  it('disables previous button on first page', () => {
    render(
      <Pagination
        currentPage={1}
        totalPages={10}
        onPageChange={vi.fn()}
        hasNextPage={true}
        hasPreviousPage={false}
      />
    );

    const previousButton = screen.getByRole('button', { name: '«' });
    expect(previousButton).toBeDisabled();
  });

  it('disables next button on last page', () => {
    render(
      <Pagination
        currentPage={10}
        totalPages={10}
        onPageChange={vi.fn()}
        hasNextPage={false}
        hasPreviousPage={true}
      />
    );

    const nextButton = screen.getByRole('button', { name: '»' });
    expect(nextButton).toBeDisabled();
  });

  it('calls onPageChange with correct page number when next is clicked', async () => {
    const user = userEvent.setup();
    const onPageChange = vi.fn();

    render(
      <Pagination
        currentPage={2}
        totalPages={10}
        onPageChange={onPageChange}
        hasNextPage={true}
        hasPreviousPage={true}
      />
    );

    const nextButton = screen.getByRole('button', { name: '»' });
    await user.click(nextButton);

    expect(onPageChange).toHaveBeenCalledWith(3);
  });

  it('calls onPageChange with correct page number when previous is clicked', async () => {
    const user = userEvent.setup();
    const onPageChange = vi.fn();

    render(
      <Pagination
        currentPage={5}
        totalPages={10}
        onPageChange={onPageChange}
        hasNextPage={true}
        hasPreviousPage={true}
      />
    );

    const previousButton = screen.getByRole('button', { name: '«' });
    await user.click(previousButton);

    expect(onPageChange).toHaveBeenCalledWith(4);
  });

  it('does not call onPageChange when disabled previous button is clicked', async () => {
    const user = userEvent.setup();
    const onPageChange = vi.fn();

    render(
      <Pagination
        currentPage={1}
        totalPages={10}
        onPageChange={onPageChange}
        hasNextPage={true}
        hasPreviousPage={false}
      />
    );

    const previousButton = screen.getByRole('button', { name: '«' });
    await user.click(previousButton);

    expect(onPageChange).not.toHaveBeenCalled();
  });
});
