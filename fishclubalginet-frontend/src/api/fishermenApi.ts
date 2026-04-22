import { apiClient } from './apiClient';
import { Endpoints } from '../constants';
import type { FishermanDto, PaginatedResult } from '../types';

export async function getFishermen(
  skip: number,
  take: number,
  search?: string
): Promise<PaginatedResult<FishermanDto>> {
  const url = Endpoints.Fishermen.GetAllPaged(skip, take, search);
  const { data } = await apiClient.get<PaginatedResult<FishermanDto>>(url);
  return data;
}

export async function deleteFisherman(id: number): Promise<void> {
  await apiClient.delete(Endpoints.Fishermen.Delete(id));
}
