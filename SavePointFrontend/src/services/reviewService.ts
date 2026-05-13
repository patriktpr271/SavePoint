import { ReviewDto, CreateReviewDto, UpdateReviewDto, ReviewStatisticsDto, PagedResult, ReviewSentimentDto } from '../interfaces/types';

import { ErrorHandler } from './errorHandler';

const API_BASE_URL = '/api';

export const reviewService = {
  // Get reviews for a specific game (paginated)
  async getGameReviews(
    gameId: string, 
    pageNumber: number = 1, 
    pageSize: number = 10,
    sortBy: string = 'newest'
  ): Promise<{ reviews: ReviewDto[]; totalCount: number }> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString(),
      sortBy
    });

    const url = `${API_BASE_URL}/review/game/${gameId}?${params}`;

    const response = await ErrorHandler.fetchWithErrorHandling(url);

    const result = await response.json();
    
    // Handle the tuple response structure from backend (Properties are capitalized)
    return {
      reviews: result.Reviews || result.reviews || [],
      totalCount: result.TotalCount || result.totalCount || 0
    };
  },

  // Get average rating for a game
  async getGameAverageRating(gameId: string): Promise<{ gameId: string; averageRating: number }> {
    const response = await ErrorHandler.fetchWithErrorHandling(
      `${API_BASE_URL}/review/game/${gameId}/average`
    );

    return response.json();
  },

  // Get review count for a game
  async getGameReviewCount(gameId: string): Promise<{ gameId: string; reviewCount: number }> {
    const response = await fetch(
      `${API_BASE_URL}/review/game/${gameId}/count`,
      {
        credentials: 'include',
      }
    );

    if (!response.ok) {
      throw new Error('Failed to fetch review count');
    }

    return response.json();
  },

  // Get a specific review by ID
  async getReviewById(reviewId: string): Promise<ReviewDto> {
    const response = await fetch(
      `${API_BASE_URL}/review/${reviewId}`,
      {
        credentials: 'include',
      }
    );

    if (!response.ok) {
      throw new Error('Failed to fetch review');
    }

    return response.json();
  },

  // Get current user's review for a specific game
  async getMyReviewForGame(gameId: string): Promise<ReviewDto | null> {
    const response = await fetch(
      `${API_BASE_URL}/review/my/game/${gameId}`,
      {
        credentials: 'include',
      }
    );

    if (response.status === 404) {
      return null; // User hasn't reviewed this game yet
    }

    if (!response.ok) {
      throw new Error('Failed to fetch user review');
    }

    return response.json();
  },

  // Get current user's reviews (paginated)
  async getMyReviews(pageNumber: number = 1, pageSize: number = 10): Promise<{ reviews: ReviewDto[]; totalCount: number }> {
    const params = new URLSearchParams({
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString()
    });

    const response = await fetch(
      `${API_BASE_URL}/review/my?${params}`,
      {
        credentials: 'include',
      }
    );

    if (!response.ok) {
      throw new Error('Failed to fetch user reviews');
    }

    const result = await response.json();
    
    // Handle the tuple response structure from backend (Properties are capitalized)
    return {
      reviews: result.Reviews || result.reviews || [],
      totalCount: result.TotalCount || result.totalCount || 0
    };
  },

  // Create a new review
  async createReview(reviewData: CreateReviewDto): Promise<ReviewDto> {
    const response = await fetch(
      `${API_BASE_URL}/review`,
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(reviewData),
      }
    );

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(errorData.message || 'Failed to create review');
    }

    return response.json();
  },

  // Update an existing review
  async updateReview(reviewId: string, reviewData: UpdateReviewDto): Promise<ReviewDto> {
    const response = await fetch(
      `${API_BASE_URL}/review/${reviewId}`,
      {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(reviewData),
      }
    );

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(errorData.message || 'Failed to update review');
    }

    return response.json();
  },

  // Delete a review
  async deleteReview(reviewId: string): Promise<void> {
    const response = await fetch(
      `${API_BASE_URL}/review/${reviewId}`,
      {
        method: 'DELETE',
        credentials: 'include',
      }
    );

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({}));
      throw new Error(errorData.message || 'Failed to delete review');
    }
  },

  // Get sentiment analysis for a review (produced by the Lambda).
  // Returns null when the Lambda hasn't processed it yet (HTTP 202).
  async getReviewSentiment(reviewId: string): Promise<ReviewSentimentDto | null> {
    const response = await fetch(
      `${API_BASE_URL}/review/${reviewId}/sentiment`,
      { credentials: 'include' }
    );

    if (response.status === 202 || response.status === 404) {
      return null;
    }

    if (!response.ok) {
      throw new Error('Failed to fetch review sentiment');
    }

    return response.json();
  },

  // Get user review statistics
  async getUserReviewStatistics(userId?: string): Promise<ReviewStatisticsDto> {
    const endpoint = userId 
      ? `${API_BASE_URL}/review/statistics/user/${userId}`
      : `${API_BASE_URL}/review/statistics/my`;

    const response = await fetch(endpoint, {
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error('Failed to fetch review statistics');
    }

    return response.json();
  },
};