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

export async function openRegistration(competitionId: string): Promise<void> {
  await apiClient.post(Endpoints.Competitions.OpenRegistration(competitionId), {});
}

export async function closeRegistration(competitionId: string): Promise<void> {
  await apiClient.post(Endpoints.Competitions.CloseRegistration(competitionId), {});
}

export async function removeRegistration(resultId: string): Promise<void> {
  await apiClient.delete(Endpoints.Competitions.RemoveResult(resultId));
}

export async function updateCompetitionResult(
  resultId: string,
  didAttend: boolean,
  weightInGrams: number,
  biggestCatchWeight: number | null
): Promise<void> {
  await apiClient.put(Endpoints.Competitions.UpdateResult(resultId), {
    didAttend,
    weightInGrams,
    biggestCatchWeight,
  });
}

