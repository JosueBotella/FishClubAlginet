import { apiClient } from './apiClient';
import { Endpoints } from '../constants';
import type {
  CompetitionDto,
  CompetitionResultDto,
  CreateCompetitionRequest,
  RegisterFishermanRequest,
} from '../types';

export async function getCompetitionsByLeague(leagueId: string): Promise<CompetitionDto[]> {
  const { data } = await apiClient.get<CompetitionDto[]>(
    Endpoints.Competitions.ByLeague(leagueId)
  );
  return data;
}

export async function createCompetition(request: CreateCompetitionRequest): Promise<{ id: string }> {
  const { data } = await apiClient.post<{ id: string }>(Endpoints.Competitions.Base, request);
  return data;
}

export async function registerFisherman(
  competitionId: string,
  fishermanId: number
): Promise<{ id: string }> {
  const body: RegisterFishermanRequest = { competitionId, fishermanId };
  const { data } = await apiClient.post<{ id: string }>(
    Endpoints.Competitions.Register(competitionId),
    body
  );
  return data;
}

export async function getCompetitionResults(
  competitionId: string
): Promise<CompetitionResultDto[]> {
  const { data } = await apiClient.get<CompetitionResultDto[]>(
    Endpoints.Competitions.Results(competitionId)
  );
  return data;
}
