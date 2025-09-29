import { GameCardDto, PagedResult } from '../types';

export interface GameFilters {
  search?: string;
  genres?: string[];
  platforms?: string[];
  companies?: string[];
  minRating?: number;
  maxRating?: number;
  fromYear?: number;
  toYear?: number;
  sortBy?: 'name' | 'rating' | 'releaseDate' | 'popularity';
  sortOrder?: 'asc' | 'desc';
}

export interface Genre {
  id: string;
  name: string;
}

export interface Platform {
  id: string;
  name: string;
}

export interface Company {
  id: string;
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
  companies: Company[];
  minYear: number;
  maxYear: number;
}