import { apiClient } from './apiClient';
import { Endpoints } from '../constants';
import type {
  LeagueDto,
  CreateLeagueRequest,
  UpdateLeagueRequest,
  PaginatedResult,
  LeagueStandingsDto,
  LeagueStandingsMatrixDto,
  SeasonBiggestCatchDto,
} from '../types';

export async function getLeagues(
  skip: number,
  take: number,
  year?: number,
  archived?: boolean
): Promise<PaginatedResult<LeagueDto>> {
  const url = Endpoints.Leagues.GetAllPaged(skip, take, year, archived);
  const { data } = await apiClient.get<PaginatedResult<LeagueDto>>(url);
  return data;
}

export async function getActiveLeague(): Promise<LeagueDto | null> {
  try {
    const { data } = await apiClient.get<LeagueDto>(Endpoints.Leagues.GetActive);
    return data;
  } catch (err: unknown) {
    if (
      typeof err === 'object' &&
      err !== null &&
      'response' in err &&
      (err as { response?: { status?: number } }).response?.status === 404
    ) {
      return null;
    }
    throw err;
  }
}

export async function createLeague(request: CreateLeagueRequest): Promise<{ id: string }> {
  const { data } = await apiClient.post<{ id: string }>(Endpoints.Leagues.Base, request);
  return data;
}

export async function updateLeague(id: string, request: UpdateLeagueRequest): Promise<LeagueDto> {
  const { data } = await apiClient.put<LeagueDto>(Endpoints.Leagues.ById(id), request);
  return data;
}

export async function activateLeague(id: string): Promise<LeagueDto> {
  const { data } = await apiClient.post<LeagueDto>(Endpoints.Leagues.Activate(id), {});
  return data;
}

export async function archiveLeague(id: string): Promise<LeagueDto> {
  const { data } = await apiClient.post<LeagueDto>(Endpoints.Leagues.Archive(id), {});
  return data;
}

export async function unarchiveLeague(id: string): Promise<LeagueDto> {
  const { data } = await apiClient.put<LeagueDto>(Endpoints.Leagues.Unarchive(id), {});
  return data;
}

export async function getLeagueStandings(id: string): Promise<LeagueStandingsDto> {
  const { data } = await apiClient.get<LeagueStandingsDto>(Endpoints.Leagues.Standings(id));
  return data;
}

export async function getLeagueStandingsMatrix(id: string): Promise<LeagueStandingsMatrixDto> {
  const { data } = await apiClient.get<LeagueStandingsMatrixDto>(Endpoints.Leagues.StandingsMatrix(id));
  return data;
}

export async function getSeasonBiggestCatch(id: string): Promise<SeasonBiggestCatchDto | null> {
  try {
    const { data } = await apiClient.get<SeasonBiggestCatchDto>(Endpoints.Leagues.BiggestCatch(id));
    return data;
  } catch (err: unknown) {
    if (
      typeof err === 'object' &&
      err !== null &&
      'response' in err &&
      (err as { response?: { status?: number } }).response?.status === 404
    ) {
      return null;
    }
    throw err;
  }
}
