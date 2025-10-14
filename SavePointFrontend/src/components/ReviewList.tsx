import React, { useState, useEffect } from 'react';
import { ReviewDto } from '../interfaces/types';
import { reviewService } from '../services/reviewService';
import { useAuth } from '../contexts/AuthContext';
import ReviewItem from './ReviewItem';
import ReviewForm from './ReviewForm';

interface ReviewListProps {
  gameId: string;
  gameName?: string;
}

const ReviewList: React.FC<ReviewListProps> = ({ gameId, gameName }) => {
  const { user } = useAuth();
  const [reviews, setReviews] = useState<ReviewDto[]>([]);
  const [userReview, setUserReview] = useState<ReviewDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showReviewForm, setShowReviewForm] = useState(false);
  const [editingReview, setEditingReview] = useState<ReviewDto | null>(null);
  const [averageRating, setAverageRating] = useState<number>(0);
  const [totalCount, setTotalCount] = useState<number>(0);
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [hasMoreReviews, setHasMoreReviews] = useState<boolean>(true);
  const [sortBy, setSortBy] = useState<string>('newest');

  const pageSize = 10;

  // Load reviews and user's review
  const loadReviews = async (page: number = 1, append: boolean = false) => {
    if (page === 1) setLoading(true);
    else setLoadingMore(true);
    
    setError(null);

    try {
      // Get reviews for the game (this is the most important part)
      const reviewsResponse = await reviewService.getGameReviews(gameId, page, pageSize, sortBy);
      
      // Handle the API response structure
      const reviewsData = Array.isArray(reviewsResponse.reviews) ? reviewsResponse.reviews : [];
      const totalCountData = reviewsResponse.totalCount || 0;
      
      if (append) {
        setReviews(prev => [...prev, ...reviewsData]);
      } else {
        setReviews(reviewsData);
      }
      
      setTotalCount(totalCountData);
      setCurrentPage(page);
      setHasMoreReviews(reviewsData.length === pageSize);

    } catch (err) {
      console.error('Failed to load reviews:', err);
      setError(err instanceof Error ? err.message : 'Failed to load reviews');
      // Even if reviews fail, set empty array so UI doesn't break
      if (!append) {
        setReviews([]);
        setTotalCount(0);
      }
    }

    // Get average rating (separate from main reviews, don't let it fail everything)
    try {
      const ratingResponse = await reviewService.getGameAverageRating(gameId);
      setAverageRating(ratingResponse.averageRating);
    } catch (ratingError) {
      console.error('Failed to load average rating:', ratingError);
      setAverageRating(0); // Default to 0 if can't load
    }

    // Get user's review if logged in (separate from main reviews)
    if (user) {
      try {
        const userReviewResponse = await reviewService.getMyReviewForGame(gameId);
        setUserReview(userReviewResponse);
      } catch (userReviewError) {
        // User hasn't reviewed this game yet, which is fine
        console.log('User has not reviewed this game yet');
        setUserReview(null);
      }
    } else {
      setUserReview(null);
    }

    setLoading(false);
    setLoadingMore(false);
  };

  // Load more reviews
  const handleLoadMore = () => {
    if (!loadingMore && hasMoreReviews) {
      loadReviews(currentPage + 1, true);
    }
  };

  // Handle sort change
  const handleSortChange = (newSort: string) => {
    setSortBy(newSort);
    setCurrentPage(1);
    loadReviews(1, false);
  };

  // Handle review submission (create/update)
  const handleReviewSubmitted = () => {
    setShowReviewForm(false);
    setEditingReview(null);
    loadReviews(1, false); // Reload all reviews
  };

  // Handle edit review
  const handleEditReview = () => {
    setEditingReview(userReview);
    setShowReviewForm(true);
  };

  // Handle delete review
  const handleDeleteReview = () => {
    setUserReview(null);
    loadReviews(1, false); // Reload all reviews
  };

  // Load reviews on component mount or when dependencies change
  useEffect(() => {
    loadReviews(1, false);
  }, [gameId, user, sortBy]);

  const renderStars = (rating: number) => {
    return (
      <div className="flex items-center gap-1">
        {[1, 2, 3, 4, 5].map((star) => (
          <span
            key={star}
            className={`text-xl ${
              star <= Math.round(rating) ? 'text-yellow-400' : 'text-gray-300'
            }`}
          >
            ★
          </span>
        ))}
        <span className="ml-2 text-sm font-medium">
          {rating > 0 ? rating.toFixed(1) : '0.0'} / 5.0
        </span>
      </div>
    );
  };

  if (loading) {
    return (
      <div className="flex justify-center items-center py-8">
        <span className="loading loading-spinner loading-lg"></span>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Reviews Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h3 className="text-xl font-bold">Reviews</h3>
          <div className="flex items-center gap-4 mt-2">
            {renderStars(averageRating)}
            <span className="text-sm text-base-content/60">
              ({totalCount} review{totalCount !== 1 ? 's' : ''})
            </span>
          </div>
        </div>

        {/* Sort Dropdown */}
        {reviews && reviews.length > 0 && (
          <div className="form-control w-full sm:w-auto">
            <select 
              className="select select-bordered select-sm"
              value={sortBy}
              onChange={(e) => handleSortChange(e.target.value)}
            >
              <option value="newest">Newest First</option>
              <option value="oldest">Oldest First</option>
              <option value="highest">Highest Rated</option>
              <option value="lowest">Lowest Rated</option>
            </select>
          </div>
        )}
      </div>

      {/* User Review Section */}
      {user && (
        <div className="bg-base-200 rounded-lg p-4">
          {userReview ? (
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <h4 className="font-medium">Your Review</h4>
                <button
                  className="btn btn-ghost btn-sm"
                  onClick={handleEditReview}
                >
                  Edit Review
                </button>
              </div>
              <ReviewItem
                review={userReview}
                onReviewDeleted={handleDeleteReview}
              />
            </div>
          ) : (
            <div className="text-center py-4">
              <p className="text-sm text-base-content/70 mb-3">
                You haven't reviewed this game yet.
              </p>
              <button
                className="btn btn-primary btn-sm"
                onClick={() => setShowReviewForm(true)}
              >
                Write a Review
              </button>
            </div>
          )}
        </div>
      )}

      {/* Review Form Modal */}
      {showReviewForm && (
        <dialog className="modal modal-open">
          <div className="modal-box max-w-2xl">
            <ReviewForm
              gameId={gameId}
              existingReview={editingReview}
              onReviewSubmitted={handleReviewSubmitted}
              onCancel={() => {
                setShowReviewForm(false);
                setEditingReview(null);
              }}
            />
          </div>
          <form method="dialog" className="modal-backdrop">
            <button onClick={() => {
              setShowReviewForm(false);
              setEditingReview(null);
            }}>close</button>
          </form>
        </dialog>
      )}

      {/* Reviews List */}
      {error ? (
        <div className="alert alert-error">
          <svg className="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
          </svg>
          <div>
            <h3 className="font-bold">Error loading reviews</h3>
            <div className="text-xs">{error}</div>
          </div>
          <button className="btn btn-ghost btn-sm" onClick={() => loadReviews(1, false)}>
            Retry
          </button>
        </div>
      ) : (!reviews || reviews.length === 0) ? (
        <div className="text-center py-8">
          <div className="text-base-content/60 mb-4">
            <svg className="w-16 h-16 mx-auto mb-4 opacity-50" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M7 8h10m0 0V6a2 2 0 00-2-2H9a2 2 0 00-2 2v2m0 0v8a2 2 0 002 2h6a2 2 0 002-2V8M9 12h6" />
            </svg>
            <p className="text-lg font-medium">No reviews yet</p>
            <p className="text-sm">Be the first to review this game!</p>
          </div>
          {user && !userReview && (
            <button
              className="btn btn-primary"
              onClick={() => setShowReviewForm(true)}
            >
              Write the First Review
            </button>
          )}
        </div>
      ) : (
        <div className="space-y-4">
          {reviews && reviews.map((review) => (
            <ReviewItem key={`review-${review.id}`} review={review} />
          ))}

          {/* Load More Button */}
          {hasMoreReviews && (
            <div className="text-center py-4">
              <button
                className={`btn btn-outline ${loadingMore ? 'loading' : ''}`}
                onClick={handleLoadMore}
                disabled={loadingMore}
              >
                {loadingMore ? 'Loading...' : 'Load More Reviews'}
              </button>
            </div>
          )}
        </div>
      )}

      {/* Login Prompt for Non-Authenticated Users */}
      {!user && (
        <div className="bg-base-200 rounded-lg p-6 text-center">
          <p className="text-base-content/70 mb-3">
            Want to share your thoughts about this game?
          </p>
          <p className="text-sm text-base-content/60">
            Sign in to write a review and join the conversation!
          </p>
        </div>
      )}
    </div>
  );
};

export default ReviewList;