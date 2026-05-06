export interface LeagueDto {
  id: string;
  name: string;
  year: number;
  isActive: boolean;
  isArchived: boolean;
  minPoints: number;
  worstResultsToDiscard: number;
  competitionsCount: number;
  lastUpdateUtc: string;
}

export interface CreateLeagueRequest {
  name: string;
  year: number;
  minPoints: number;
  worstResultsToDiscard: number;
}

export interface UpdateLeagueRequest {
  name: string;
  minPoints: number;
  worstResultsToDiscard: number;
}

export type LeagueFormData = CreateLeagueRequest;
