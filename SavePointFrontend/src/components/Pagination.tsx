import React from 'react';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

const Pagination: React.FC<PaginationProps> = ({
  currentPage,
  totalPages,
  onPageChange,
  hasNextPage,
  hasPreviousPage
}) => {
  const handlePreviousPage = (e: React.MouseEvent<HTMLButtonElement>) => {
    e.preventDefault();
    e.stopPropagation();
    if (hasPreviousPage && currentPage > 1) {
      onPageChange(currentPage - 1);
    }
  };

  const handleNextPage = (e: React.MouseEvent<HTMLButtonElement>) => {
    e.preventDefault();
    e.stopPropagation();
    if (hasNextPage && currentPage < totalPages) {
      onPageChange(currentPage + 1);
    }
  };

  return (
    <div className="flex justify-center items-center gap-2 mt-6">
      <button
        type="button"
        className={`btn btn-sm ${!hasPreviousPage ? 'btn-disabled' : 'btn-outline'}`}
        onClick={handlePreviousPage}
        disabled={!hasPreviousPage}
      >
        «
      </button>
      
      <span className="text-sm text-base-content/70 px-4">
        Page {currentPage} of {totalPages}
      </span>
      
      <button
        type="button"
        className={`btn btn-sm ${!hasNextPage ? 'btn-disabled' : 'btn-outline'}`}
        onClick={handleNextPage}
        disabled={!hasNextPage}
      >
        »
      </button>
    </div>
  );
};

export default Pagination;