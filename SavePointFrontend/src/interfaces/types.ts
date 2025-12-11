export interface GameCardDto {
  id: string; 
  name: string;
  coverUrl?: string;
  rating: number; 
  releaseDate: string; 
  popularityScore?: number; 
}

export interface GameDetailDto extends GameCardDto {
  summary?: string;
  reviewCount: number;
  averageUserRating: number;
  genres: Array<{ id: string; name: string }>;   
  platforms: Array<{ id: string; name: string }>; 
  companies: Array<{ id: string; name: string; role: string }>; 
}

export interface PagedResult<T> {
  items: T[];
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

export interface UserListDto {
  id: string;
  name: string;
  description?: string;
  isPublic: boolean;
  isDefault: boolean;
  defaultListType?: string;
  createdAt: string;
  updatedAt?: string;
  userId: string;
  userName?: string;
  gameCount: number;
  upvotes: number;
  downvotes: number;
  userVote?: boolean;
  games?: GameCardDto[];
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

export enum DefaultListType {
  WantToPlay = 'WantToPlay',
  Finished = 'Finished'
}

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

export interface ReviewDto {
  id: string;
  userId: string;
  gameId: string;
  rating: number;
  content?: string;
  createdAt: string;
  updatedAt: string;
  userName?: string;
  userDisplayName?: string;
  gameName?: string;
}

export interface CreateReviewDto {
  gameId: string;
  rating: number;
  content?: string;
}

export interface UpdateReviewDto {
  rating: number;
  content?: string;
}

export interface ReviewStatisticsDto {
  totalReviews: number;
  averageRating: number;
}