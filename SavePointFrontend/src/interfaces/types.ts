// Main DTO that matches backend GameCardDto
export interface GameCardDto {
  id: string; // Guid from backend
  name: string;
  coverUrl?: string;
  rating: number; // double from backend
  releaseDate: string; // DateTime serialized as string
  popularityScore?: number; // decimal? from backend
}

export interface GameDetailDto extends GameCardDto {
  summary?: string;
  reviewCount: number;
  averageUserRating: number;
  genres: Array<{ id: string; name: string }>;     // Changed from gameGenres
  platforms: Array<{ id: string; name: string }>;  // Changed from gamePlatforms  
  companies: Array<{ id: string; name: string; role: string }>; // Changed from gameCompanies
}

export interface PagedResult<T> {
  items: T[]; // IList<T> from backend becomes T[] in frontend
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface PopularityType {
  id: number;
  name: string;
  description: string;
  icon: string;
}

export const POPULARITY_TYPES: PopularityType[] = [
  {
    id: 1,
    name: "Visits",
    description: "Most visited games on our platform",
    icon: "👁️"
  },
  {
    id: 2,
    name: "Want to Play",
    description: "Games most added to wishlists",
    icon: "⭐"
  },
  {
    id: 5,
    name: "24hr Peak Players",
    description: "Games with highest player count in last 24h",
    icon: "🔥"
  }
];

// User List related DTOs
export interface UserListDto {
  id: string; // Guid from backend
  name: string;
  description?: string;
  isPublic: boolean;
  isDefault: boolean;
  defaultListType?: string;
  createdAt: string; // DateTime serialized as string
  updatedAt?: string; // DateTime serialized as string
  userId: string;
  userName?: string;
  gameCount: number;
  upvotes: number;
  downvotes: number;
  userVote?: boolean; // true = upvote, false = downvote, null = no vote
  games?: GameCardDto[]; // Only included when includeGames=true
}

export interface CreateUserListDto {
  name: string;
  description?: string;
  isPublic: boolean;
}

export interface UpdateUserListDto {
  name: string;
  description?: string;
  isPublic: boolean;
}

export interface AddGameToListDto {
  gameId: string;
}

export interface VoteOnListDto {
  isUpvote: boolean;
}

// Default list types enum
export enum DefaultListType {
  WantToPlay = 'WantToPlay',
  Finished = 'Finished'
}

// Extended User interface to include navigation properties
export interface UserProfileDto {
  id: string;
  username: string;
  email: string;
  displayName: string;
  bio?: string;
  roles: string[];
  createdAt: string;
  lastLoginAt?: string;
  listCount: number;
  publicListCount: number;
}

// Review related DTOs
export interface ReviewDto {
  id: string; // Guid from backend
  userId: string;
  gameId: string;
  rating: number; // 1-5 integer
  content?: string;
  createdAt: string; // DateTime serialized as string
  updatedAt: string; // DateTime serialized as string
  userName?: string; // From navigation property
  userDisplayName?: string; // From navigation property
  gameName?: string; // From navigation property
}

export interface CreateReviewDto {
  gameId: string;
  rating: number; // 1-5 integer, required
  content?: string; // Optional text content
}

export interface UpdateReviewDto {
  rating: number; // 1-5 integer, required
  content?: string; // Optional text content
}

export interface ReviewStatisticsDto {
  totalReviews: number;
  averageRating: number;
}