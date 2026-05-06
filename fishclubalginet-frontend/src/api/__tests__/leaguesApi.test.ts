import { describe, it, expect, vi, beforeEach } from 'vitest';
import { getActiveLeague, getLeagues, createLeague } from '../leaguesApi';
import type { LeagueDto } from '../../types';

// Mockeamos el cliente axios subyacente
vi.mock('../apiClient', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
  },
}));

import { apiClient } from '../apiClient';

const mockedGet = apiClient.get as unknown as ReturnType<typeof vi.fn>;
const mockedPost = apiClient.post as unknown as ReturnType<typeof vi.fn>;

const sampleLeague: LeagueDto = {
  id: '00000000-0000-0000-0000-000000000001',
  name: 'Liga 2026',
  year: 2026,
  isActive: true,
  isArchived: false,
  minPoints: 5,
  worstResultsToDiscard: 0,
  competitionsCount: 3,
  lastUpdateUtc: '2026-04-29T12:00:00Z',
};

describe('leaguesApi', () => {
  beforeEach(() => {
    mockedGet.mockReset();
    mockedPost.mockReset();
  });

  describe('getActiveLeague', () => {
    it('devuelve la liga activa cuando hay una', async () => {
      mockedGet.mockResolvedValueOnce({ data: sampleLeague });
      const result = await getActiveLeague();
      expect(result).toEqual(sampleLeague);
    });

    it('devuelve null cuando el endpoint responde 404 (sin liga activa)', async () => {
      mockedGet.mockRejectedValueOnce({
        response: { status: 404 },
        isAxiosError: true,
      });
      const result = await getActiveLeague();
      expect(result).toBeNull();
    });

    it('relanza el error en respuestas no-404', async () => {
      mockedGet.mockRejectedValueOnce({
        response: { status: 500 },
        isAxiosError: true,
      });
      await expect(getActiveLeague()).rejects.toMatchObject({
        response: { status: 500 },
      });
    });
  });

  describe('getLeagues', () => {
    it('llama al endpoint paginado con los parámetros correctos', async () => {
      mockedGet.mockResolvedValueOnce({
        data: { items: [sampleLeague], totalCount: 1 },
      });

      const result = await getLeagues(0, 15, 2026);

      expect(result.items).toHaveLength(1);
      expect(result.totalCount).toBe(1);
      // El primer argumento debe contener la URL con los parámetros
      const calledUrl = mockedGet.mock.calls[0][0] as string;
      expect(calledUrl).toContain('skip=0');
      expect(calledUrl).toContain('take=15');
      expect(calledUrl).toContain('year=2026');
    });

    it('omite el parámetro year cuando no se pasa', async () => {
      mockedGet.mockResolvedValueOnce({ data: { items: [], totalCount: 0 } });
      await getLeagues(0, 10);
      const calledUrl = mockedGet.mock.calls[0][0] as string;
      expect(calledUrl).not.toContain('year=');
    });
  });

  describe('createLeague', () => {
    it('hace POST con el cuerpo correcto y devuelve el id', async () => {
      mockedPost.mockResolvedValueOnce({ data: { id: 'new-id' } });

      const result = await createLeague({
        name: 'Liga 2027',
        year: 2027,
        minPoints: 5,
        worstResultsToDiscard: 0,
      });

      expect(result).toEqual({ id: 'new-id' });
      expect(mockedPost).toHaveBeenCalledWith(expect.any(String), {
        name: 'Liga 2027',
        year: 2027,
        minPoints: 5,
        worstResultsToDiscard: 0,
      });
    });
  });
});
