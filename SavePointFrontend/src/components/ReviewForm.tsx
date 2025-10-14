import React, { useState } from 'react';
import { ReviewDto, CreateReviewDto, UpdateReviewDto } from '../interfaces/types';
import { reviewService } from '../services/reviewService';
import { useFormSubmission } from '../hooks/useErrorHandling';
import { useError } from '../contexts/ErrorContext';
import { ErrorHandler, AppError } from '../services/errorHandler';

interface ReviewFormProps {
  gameId: string;
  existingReview?: ReviewDto | null;
  onReviewSubmitted: () => void;
  onCancel: () => void;
}

const ReviewForm: React.FC<ReviewFormProps> = ({
  gameId,
  existingReview,
  onReviewSubmitted,
  onCancel
}) => {
  const [rating, setRating] = useState(existingReview?.rating || 5);
  const [content, setContent] = useState(existingReview?.content || '');
  const [error, setError] = useState<string | null>(null);
  const [validationErrors, setValidationErrors] = useState<Record<string, string[]> | null>(null);
  
  const { submitForm } = useFormSubmission();
  const { isLoading } = useError();
  const isEdit = !!existingReview;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Clear previous errors
    setError(null);
    setValidationErrors(null);
    
    // Client-side validation
    if (!rating || rating < 1 || rating > 5) {
      setError('Please provide a rating between 1 and 5 stars');
      return;
    }

    if (content && (content.length < 10 || content.length > 2000)) {
      setError('Review content must be between 10 and 2000 characters');
      return;
    }

    // Submit using enhanced error handling
    await submitForm(async () => {
      if (isEdit && existingReview) {
        const updateData: UpdateReviewDto = {
          rating,
          content: content.trim() || undefined
        };
        return await reviewService.updateReview(existingReview.id, updateData);
      } else {
        const createData: CreateReviewDto = {
          gameId,
          rating,
          content: content.trim() || undefined
        };
        return await reviewService.createReview(createData);
      }
    }, {
      onSuccess: () => {
        onReviewSubmitted();
      },
      onValidationError: (errors) => {
        setValidationErrors(errors);
      },
      onError: (error: AppError) => {
        // For non-validation errors, show them locally
        setError(ErrorHandler.getUserFriendlyMessage(error));
      },
      context: isEdit ? 'Update Review' : 'Create Review'
    });
  };

  const renderStarRating = () => {
    return (
      <div className="flex items-center gap-1">
        {[1, 2, 3, 4, 5].map((star) => (
          <button
            key={star}
            type="button"
            className={`text-2xl transition-colors ${
              star <= rating 
                ? 'text-yellow-400 hover:text-yellow-500' 
                : 'text-gray-300 hover:text-yellow-300'
            }`}
            onClick={() => setRating(star)}
          >
            ★
          </button>
        ))}
        <span className="ml-2 text-sm font-medium">
          {rating} star{rating !== 1 ? 's' : ''}
        </span>
      </div>
    );
  };

  return (
    <div className="card bg-base-100 shadow-lg border border-base-300">
      <div className="card-body">
        <h3 className="card-title text-lg">
          {isEdit ? 'Edit Your Review' : 'Write a Review'}
        </h3>

        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Rating Section */}
          <div className="form-control">
            <label className="label">
              <span className="label-text font-medium">Rating *</span>
            </label>
            {renderStarRating()}
          </div>

          {/* Content Section */}
          <div className="form-control">
            <label className="label">
              <span className="label-text font-medium">Review (Optional)</span>
              <span className="label-text-alt text-xs">
                {content.length}/2000 characters
              </span>
            </label>
            <textarea
              className="textarea textarea-bordered h-32 resize-none"
              placeholder="Share your thoughts about this game... (optional)"
              value={content}
              onChange={(e) => setContent(e.target.value)}
              maxLength={2000}
            />
            {content.length > 0 && content.length < 10 && (
              <label className="label">
                <span className="label-text-alt text-warning">
                  Minimum 10 characters required for review text
                </span>
              </label>
            )}
          </div>

          {/* Error Messages */}
          {error && (
            <div className="alert alert-error">
              <svg className="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <span className="text-sm">{error}</span>
            </div>
          )}

          {/* Validation Errors */}
          {validationErrors && (
            <div className="alert alert-warning">
              <svg className="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z" />
              </svg>
              <div>
                <div className="font-bold">Validation Errors:</div>
                <ul className="text-sm mt-1 space-y-1">
                  {Object.entries(validationErrors).map(([field, errors]) =>
                    errors.map((err, idx) => (
                      <li key={`${field}-${idx}`}>
                        <span className="font-medium">{field}:</span> {err}
                      </li>
                    ))
                  )}
                </ul>
              </div>
            </div>
          )}

          {/* Form Actions */}
          <div className="card-actions justify-end gap-2">
            <button
              type="button"
              className="btn btn-ghost"
              onClick={onCancel}
              disabled={isLoading}
            >
              Cancel
            </button>
            <button
              type="submit"
              className={`btn btn-primary ${isLoading ? 'loading' : ''}`}
              disabled={isLoading}
            >
              {isLoading 
                ? (isEdit ? 'Updating...' : 'Submitting...') 
                : (isEdit ? 'Update Review' : 'Submit Review')
              }
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default ReviewForm;