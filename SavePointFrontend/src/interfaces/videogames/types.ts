import { GameCardDto, PagedResult } from '../types';

export interface GameFilters {
  search?: string;
  genres?: number[];
  platforms?: number[];
  minRating?: number;
  maxRating?: number;
  releaseYearFrom?: number;
  releaseYearTo?: number;
  sortBy?: 'name' | 'rating' | 'releaseDate' | 'popularity';
  sortOrder?: 'asc' | 'desc';
}

export interface Genre {
  id: number;
  name: string;
}

export interface Platform {
  id: number;
  name: string;
}

export interface GameSearchParams extends GameFilters {
  pageNumber?: number;
  pageSize?: number;
}

export interface GameSearchResponse extends PagedResult<GameCardDto> {
  // Inherits all properties from PagedResult<GameCardDto>
}

export interface FilterOptions {
  genres: Genre[];
  platforms: Platform[];
  minYear: number;
  maxYear: number;
}