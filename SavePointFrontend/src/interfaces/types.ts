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