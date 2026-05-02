import { apiClient } from './apiClient';
import { Endpoints } from '../constants';
import type { FishermanDto, FishermanProfileDto, PaginatedResult } from '../types';

export async function getFishermen(
  skip: number,
  take: number,
  search?: string,
  showDeleted?: boolean
): Promise<PaginatedResult<FishermanDto>> {
  const url = Endpoints.Fishermen.GetAllPaged(skip, take, search, showDeleted);
  const { data } = await apiClient.get<PaginatedResult<FishermanDto>>(url);
  return data;
}

export async function deleteFisherman(id: number): Promise<void> {
  await apiClient.delete(Endpoints.Fishermen.Delete(id));
}

/**
 * Obtiene los datos del Fisherman asociado al usuario autenticado.
 * Devuelve null cuando el usuario no tiene ficha (típico de Admins puros).
 */
export async function getMyProfile(): Promise<FishermanProfileDto | null> {
  try {
    const { data } = await apiClient.get<FishermanProfileDto>(
      Endpoints.Fishermen.MyProfile
    );
    return data;
  } catch (err: unknown) {
    // 404 = el usuario no tiene Fisherman asociado (caso normal para Admins puros)
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
