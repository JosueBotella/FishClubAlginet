import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { renderWithProviders } from '../../../test/renderWithProviders';
import LeagueStandingsPage from '../LeagueStandingsPage';
import type {
  LeagueStandingsMatrixDto,
  SeasonBiggestCatchDto,
  LeagueDto,
} from '../../../types';


vi.mock('../../../api/leaguesApi', () => ({
  getLeagueStandingsMatrix: vi.fn(),
  getSeasonBiggestCatch: vi.fn(),
  getActiveLeague: vi.fn(),
  getLeagues: vi.fn(),
}));

import {
  getLeagueStandingsMatrix,
  getSeasonBiggestCatch,
  getActiveLeague,
  getLeagues,
} from '../../../api/leaguesApi';

const mockedGetMatrix = getLeagueStandingsMatrix as ReturnType<typeof vi.fn>;
const mockedGetBiggestCatch = getSeasonBiggestCatch as ReturnType<typeof vi.fn>;
const mockedGetActiveLeague = getActiveLeague as ReturnType<typeof vi.fn>;
const mockedGetLeagues = getLeagues as ReturnType<typeof vi.fn>;

const mockLeague: LeagueDto = {
  id: 'league-101',
  name: 'Liga Alginet 2026',
  year: 2026,
  isActive: true,
  isArchived: false,
  minPoints: 5,
  worstResultsToDiscard: 1,
  competitionsCount: 2,
  lastUpdateUtc: '2026-01-01T00:00:00Z',
};

const mockMatrixData: LeagueStandingsMatrixDto = {
  leagueId: 'league-101',
  leagueName: 'Liga Alginet 2026',
  year: 2026,
  worstResultsToDiscard: 1,
  competitions: [
    {
      id: 'comp-1',
      competitionNumber: 1,
      name: 'Concurso Playa',
      date: '2026-04-10T08:00:00Z',
    },
    {
      id: 'comp-2',
      competitionNumber: 2,
      name: 'Concurso Puerto',
      date: '2026-05-15T08:00:00Z',
    },
  ],
  byPoints: [
    {
      fishermanId: 1,
      fullName: 'Paco García',
      competitionsAttended: 2,
      totalPoints: 20,
      pointsAfterDiscard: 15,
      totalWeightGrams: 4500,
      resultsPerCompetition: {
        'comp-1': {
          weightInGrams: 2500,
          points: 15,
          ranking: 1,
          didAttend: true,
          isDiscarded: false,
        },
        'comp-2': {
          weightInGrams: 2000,
          points: 5,
          ranking: 3,
          didAttend: true,
          isDiscarded: true,
        },
      },
    },
    {
      fishermanId: 2,
      fullName: 'Jose Botella',
      competitionsAttended: 2,
      totalPoints: 12,
      pointsAfterDiscard: 10,
      totalWeightGrams: 3000,
      resultsPerCompetition: {
        'comp-1': {
          weightInGrams: 1000,
          points: 2,
          ranking: 4,
          didAttend: true,
          isDiscarded: true,
        },
        'comp-2': {
          weightInGrams: 2000,
          points: 10,
          ranking: 2,
          didAttend: true,
          isDiscarded: false,
        },
      },
    },
  ],
  byWeight: [
    {
      fishermanId: 1,
      fullName: 'Paco García',
      competitionsAttended: 2,
      totalPoints: 20,
      pointsAfterDiscard: 15,
      totalWeightGrams: 4500,
      resultsPerCompetition: {
        'comp-1': {
          weightInGrams: 2500,
          points: 15,
          ranking: 1,
          didAttend: true,
          isDiscarded: false,
        },
        'comp-2': {
          weightInGrams: 2000,
          points: 5,
          ranking: 3,
          didAttend: true,
          isDiscarded: false,
        },
      },
    },
  ],
};

const mockBiggestCatch: SeasonBiggestCatchDto = {
  leagueId: 'league-101',
  leagueName: 'Liga Alginet 2026',
  fishermanId: 1,
  fishermanName: 'Paco García',
  weightInGrams: 1850,
  competitionId: 'comp-1',
  competitionNumber: 1,
  competitionName: 'Concurso Playa',
  competitionDate: '2026-04-10T08:00:00Z',
};


describe('LeagueStandingsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockedGetActiveLeague.mockResolvedValue(mockLeague);
    mockedGetLeagues.mockResolvedValue({ items: [mockLeague], totalCount: 1, skip: 0, take: 50 });
    mockedGetMatrix.mockResolvedValue(mockMatrixData);
    mockedGetBiggestCatch.mockResolvedValue(mockBiggestCatch);
  });

  it('muestra el loader inicialmente mientras carga', () => {
    mockedGetMatrix.mockReturnValue(new Promise(() => {})); // Never resolves
    renderWithProviders(
      <MemoryRouter>
        <LeagueStandingsPage />
      </MemoryRouter>,
    );

    expect(screen.getByText(/Clasificación Detallada/i)).toBeInTheDocument();
  });

  it('muestra la matriz por puntos con pescadores y alerta de descartes', async () => {
    renderWithProviders(
      <MemoryRouter>
        <LeagueStandingsPage />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText(/Liga Alginet 2026/i)).toBeInTheDocument();
    });

    expect(screen.getAllByText('Paco García').length).toBeGreaterThanOrEqual(1);
    expect(screen.getAllByText('Jose Botella').length).toBeGreaterThanOrEqual(1);
    expect(screen.getByText(/Esta liga descarta los/i)).toBeInTheDocument();
    expect(screen.getByRole('tab', { name: /Por puntos/i })).toBeInTheDocument();
    expect(screen.getByRole('tab', { name: /Por peso/i })).toBeInTheDocument();
    expect(screen.getByRole('tab', { name: /Pieza Mayor/i })).toBeInTheDocument();
  });



  it('muestra mensaje de error si falla la carga de la matriz', async () => {
    mockedGetMatrix.mockRejectedValue(new Error('Network error'));

    renderWithProviders(
      <MemoryRouter>
        <LeagueStandingsPage />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByText(/Error al cargar la clasificación detallada/i),
      ).toBeInTheDocument();
    });
  });

  it('muestra mensaje cuando no hay ninguna liga disponible', async () => {
    mockedGetActiveLeague.mockResolvedValue(null);
    mockedGetLeagues.mockResolvedValue({ items: [], totalCount: 0, skip: 0, take: 50 });

    renderWithProviders(
      <MemoryRouter>
        <LeagueStandingsPage />
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(
        screen.getByText(/No hay ninguna liga disponible para consultar clasificaciones/i),
      ).toBeInTheDocument();
    });
  });
});

