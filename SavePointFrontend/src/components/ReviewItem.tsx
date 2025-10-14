import React, { useState } from 'react';
import { ReviewDto } from '../interfaces/types';
import { useAuth } from '../contexts/AuthContext';
import { reviewService } from '../services/reviewService';

interface ReviewItemProps {
  review: ReviewDto;
  onReviewUpdated?: () => void;
  onReviewDeleted?: () => void;
  showGameName?: boolean;
}

const ReviewItem: React.FC<ReviewItemProps> = ({ 
  review, 
  onReviewUpdated, 
  onReviewDeleted,
  showGameName = false 
}) => {
  const { user } = useAuth();
  const [isDeleting, setIsDeleting] = useState(false);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);

  const isOwner = user?.id === review.userId;

  const handleDelete = async () => {
    if (!isOwner) return;
    
    setIsDeleting(true);
    try {
      await reviewService.deleteReview(review.id);
      onReviewDeleted?.();
    } catch (error) {
      console.error('Failed to delete review:', error);
    } finally {
      setIsDeleting(false);
      setShowDeleteConfirm(false);
    }
  };

  const renderStars = (rating: number) => {
    return (
      <div className="flex items-center gap-1">
        {[1, 2, 3, 4, 5].map((star) => (
          <span
            key={star}
            className={`text-lg ${
              star <= rating ? 'text-yellow-400' : 'text-gray-300'
            }`}
          >
            ★
          </span>
        ))}
        <span className="ml-2 text-sm font-medium text-base-content/70">
          {rating}/5
        </span>
      </div>
    );
  };

  const formatDate = (dateString: string) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  return (
    <div className="card bg-base-100 border border-base-300 shadow-sm">
      <div className="card-body p-4">
        {/* Header with user info and rating */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 mb-3">
          <div className="flex items-center gap-3">
            {/* User Avatar */}
            <div className="avatar placeholder">
              <div className="bg-primary text-primary-content rounded-full w-10">
                <span className="text-sm font-bold">
                  {(review.userDisplayName || review.userName || 'U').charAt(0).toUpperCase()}
                </span>
              </div>
            </div>
            
            {/* User Name and Date */}
            <div>
              <div className="font-medium text-sm">
                {review.userDisplayName || review.userName || 'Anonymous User'}
                {isOwner && (
                  <span className="badge badge-primary badge-xs ml-2">You</span>
                )}
              </div>
              <div className="text-xs text-base-content/60">
                {formatDate(review.createdAt)}
                {review.updatedAt !== review.createdAt && (
                  <span className="ml-1">(edited)</span>
                )}
              </div>
            </div>
          </div>

          {/* Rating */}
          <div className="flex items-center gap-2">
            {renderStars(review.rating)}
          </div>
        </div>

        {/* Game Name (if showing) */}
        {showGameName && review.gameName && (
          <div className="text-sm text-base-content/70 mb-2">
            Review for: <span className="font-medium">{review.gameName}</span>
          </div>
        )}

        {/* Review Content */}
        {review.content && (
          <div className="text-sm text-base-content/90 leading-relaxed mb-3">
            {review.content}
          </div>
        )}

        {/* Actions (only for owner) */}
        {isOwner && (onReviewUpdated || onReviewDeleted) && (
          <div className="card-actions justify-end gap-2">
            {onReviewUpdated && (
              <button
                className="btn btn-ghost btn-sm"
                onClick={onReviewUpdated}
              >
                <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M15.232 5.232l3.536 3.536m-2.036-5.036a2.5 2.5 0 113.536 3.536L6.5 21.036H3v-3.572L16.732 3.732z" />
                </svg>
                Edit
              </button>
            )}
            
            {onReviewDeleted && (
              <>
                <button
                  className="btn btn-ghost btn-sm text-error hover:bg-error hover:text-error-content"
                  onClick={() => setShowDeleteConfirm(true)}
                  disabled={isDeleting}
                >
                  <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
                  </svg>
                  Delete
                </button>

                {/* Delete Confirmation Modal */}
                {showDeleteConfirm && (
                  <dialog className="modal modal-open">
                    <div className="modal-box">
                      <h3 className="font-bold text-lg">Confirm Delete</h3>
                      <p className="py-4">
                        Are you sure you want to delete this review? This action cannot be undone.
                      </p>
                      <div className="modal-action">
                        <button
                          className="btn btn-ghost"
                          onClick={() => setShowDeleteConfirm(false)}
                          disabled={isDeleting}
                        >
                          Cancel
                        </button>
                        <button
                          className={`btn btn-error ${isDeleting ? 'loading' : ''}`}
                          onClick={handleDelete}
                          disabled={isDeleting}
                        >
                          {isDeleting ? 'Deleting...' : 'Delete Review'}
                        </button>
                      </div>
                    </div>
                    <form method="dialog" className="modal-backdrop">
                      <button onClick={() => setShowDeleteConfirm(false)}>close</button>
                    </form>
                  </dialog>
                )}
              </>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default ReviewItem;