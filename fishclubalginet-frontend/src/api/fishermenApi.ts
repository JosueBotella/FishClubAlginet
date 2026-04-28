import { apiClient } from './apiClient';
import { Endpoints } from '../constants';
import type { FishermanDto, PaginatedResult } from '../types';

export async function getFishermen(
  skip: number,
  take: number,
  search?: string,
  showDeleted?: boolean
): Promise<PaginatedResult<FishermanDto>> {
  const url = Endpoints.Fishermen.GetAllPaged(skip, take, search, showDeleted);
  const { data } = await apiClient.get<