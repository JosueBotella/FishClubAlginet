import { describe, it, expect, vi, beforeEach } from 'vitest';
import {
  getCompetitionsByLeague,
  getCompetitionById,
  createCompetition,
  registerFisherman,
  getCompetitionResults,
  openRegistration,
  closeRegistration,
  reopenRegistration,
  assignSpots,
  moveToResultsDraft,
  validateResults,
  removeRegistration,
  updateCompetitionResult,
  updateBiggestCatchConfig,
} from '../competitionsApi';
import type { CompetitionDto, CompetitionResultDto, CreateCompetitionRequest } from '../../types';

vi.mock('../apiClient', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    patch: vi.fn(),
    delete: vi.fn(),
  },
}));

import { apiClient } from '../apiClient';

const mockedGet = apiClient.get as unknown as ReturnType<typeof vi.fn>;
const mockedPost = apiClient.post as unknown as ReturnType<typeof vi.fn>;
const mockedPut = apiClient.put as unknown as ReturnType<typeof vi.fn>;
const mockedPatch = apiClient.patch as unknown as ReturnType<typeof vi.fn>;
const mockedDelete = apiClient.delete as unknown as ReturnType<typeof vi.fn>;

const sampleCompetition: CompetitionDto = {
  id: 'comp-1',
  leagueId: 'league-1',
  competitionNumber: 1,
  name: 'Concurso 1',
  date: '2026-06-01',
  startTime: '08:00:00',
  endTime: '14:00:00',
  venue: 'Puerto Alginet',
  zone: 'Zona A',
  subspecialty: 'AguaDulce' as const,
  category: 'Seniors' as const,
  status: 'Planned',
  maxSpots: 20,
  participantCount: 0,
  lastUpdateUtc: '2026-05-01T00:00:00Z',
  biggestCatchMinWeightInGrams: 500,
};

const sampleResult: CompetitionResultDto = {
  id: 'res-1',
  competitionId: 'comp-1',
  fishermanId: 1,
  assignedSpotNumber: 5,
  didAttend: true,
  weightInGrams: 4500,
  biggestCatchWeight: 1200,
  points: 100,
  ranking: 1,
  isValidated: false,
  registrationDate: '2026-05-02T10:00:00Z',
  isBiggestCatch: true,
};

describe('competitionsApi', () => {
  beforeEach(() => {
    mockedGet.mockReset();
    mockedPost.mockReset();
    mockedPut.mockReset();
    mockedPatch.mockReset();
    mockedDelete.mockReset();
  });

  describe('getCompetitionsByLeague', () => {
    it('obtiene el listado de concursos de una liga', async () => {
      mockedGet.mockResolvedValueOnce({ data: [sampleCompetition] });
      const result = await getCompetitionsByLeague('league-1');
      expect(result).toEqual([sampleCompetition]);
      expect(mockedGet).toHaveBeenCalledWith('api/competitions?leagueId=league-1');
    });
  });

  describe('getCompetitionById', () => {
    it('obtiene un concurso por su ID', async () => {
      mockedGet.mockResolvedValueOnce({ data: sampleCompetition });
      const result = await getCompetitionById('comp-1');
      expect(result).toEqual(sampleCompetition);
      expect(mockedGet).toHaveBeenCalledWith('api/competitions/comp-1');
    });
  });

  describe('createCompetition', () => {
    it('envia la peticion POST para crear un concurso', async () => {
      mockedPost.mockResolvedValueOnce({ data: { id: 'comp-1' } });
      const requestData: CreateCompetitionRequest = {
        leagueId: 'league-1',
        competitionNumber: 1,
        name: 'Concurso 1',
        date: '2026-06-01',
        startTime: '08:00:00',
        endTime: '14:00:00',
        venue: 'Puerto Alginet',
        zone: 'Zona A',
        subspecialty: 'AguaDulce',
        category: 'Seniors',
        maxSpots: 20,
        biggestCatchMinWeightInGrams: 500,
      };
      const result = await createCompetition(requestData);
      expect(result).toEqual({ id: 'comp-1' });
      expect(mockedPost).toHaveBeenCalledWith('api/competitions', requestData);
    });
  });

  describe('registerFisherman', () => {
    it('inscribe a un pescador en un concurso', async () => {
      mockedPost.mockResolvedValueOnce({ data: { id: 'res-1' } });
      const result = await registerFisherman('comp-1', 1);
      expect(result).toEqual({ id: 'res-1' });
      expect(mockedPost).toHaveBeenCalledWith('api/competitions/comp-1/register', {
        competitionId: 'comp-1',
        fishermanId: 1,
      });
    });
  });

  describe('getCompetitionResults', () => {
    it('obtiene los resultados de un concurso', async () => {
      mockedGet.mockResolvedValueOnce({ data: [sampleResult] });
      const result = await getCompetitionResults('comp-1');
      expect(result).toEqual([sampleResult]);
      expect(mockedGet).toHaveBeenCalledWith('api/competitions/comp-1/results');
    });
  });

  describe('flujo de estados', () => {
    it('openRegistration abre la inscripcion', async () => {
      mockedPost.mockResolvedValueOnce({});
      await openRegistration('comp-1');
      expect(mockedPost).toHaveBeenCalledWith('api/competitions/comp-1/open-registration', {});
    });

    it('closeRegistration cierra la inscripcion', async () => {
      mockedPost.mockResolvedValueOnce({});
      await closeRegistration('comp-1');
      expect(mockedPost).toHaveBeenCalledWith('api/competitions/comp-1/close-registration', {});
    });

    it('reopenRegistration reabre la inscripcion', async () => {
      mockedPut.mockResolvedValueOnce({});
      await reopenRegistration('comp-1');
      expect(mockedPut).toHaveBeenCalledWith('api/competitions/comp-1/reopen-registration', {});
    });

    it('assignSpots asigna las pesqueras', async () => {
      mockedPost.mockResolvedValueOnce({});
      await assignSpots('comp-1');
      expect(mockedPost).toHaveBeenCalledWith('api/competitions/comp-1/assign-spots', {});
    });

    it('moveToResultsDraft pasa el concurso a borrador de resultados', async () => {
      mockedPost.mockResolvedValueOnce({});
      await moveToResultsDraft('comp-1');
      expect(mockedPost).toHaveBeenCalledWith('api/competitions/comp-1/results-draft', {});
    });

    it('validateResults valida los resultados', async () => {
      mockedPost.mockResolvedValueOnce({});
      await validateResults('comp-1');
      expect(mockedPost).toHaveBeenCalledWith('api/competitions/comp-1/validate-results', {});
    });

    it('removeRegistration elimina una inscripcion', async () => {
      mockedDelete.mockResolvedValueOnce({});
      await removeRegistration('res-1');
      expect(mockedDelete).toHaveBeenCalledWith('api/competitions/results/res-1');
    });

    it('updateCompetitionResult actualiza pesaje y pieza mayor', async () => {
      mockedPut.mockResolvedValueOnce({});
      await updateCompetitionResult('res-1', true, 4500, 1200);
      expect(mockedPut).toHaveBeenCalledWith('api/competitions/results/res-1', {
        didAttend: true,
        weightInGrams: 4500,
        biggestCatchWeight: 1200,
      });
    });

    it('updateBiggestCatchConfig actualiza la configuracion de pieza mayor', async () => {
      mockedPatch.mockResolvedValueOnce({});
      await updateBiggestCatchConfig('comp-1', { minWeightInGrams: 600 });
      expect(mockedPatch).toHaveBeenCalledWith('api/competitions/comp-1/biggest-catch-config', {
        minWeightInGrams: 600,
      });
    });
  });
});
