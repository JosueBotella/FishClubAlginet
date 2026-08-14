import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { renderWithProviders } from '../../../test/renderWithProviders';
import CompetitionResultsPage from '../CompetitionResultsPage';
import type { CompetitionDto, CompetitionResultDto } from '../../../types';

vi.mock('../../../api/competitionsApi', () => ({
  getCompetitionById: vi.fn(),
  getCompetitionResults: vi.fn(),
  registerFisherman: vi.fn(),
  removeRegistration: vi.fn(),
  updateCompetitionResult: vi.fn(),
  updateBiggestCatchConfig: vi.fn(),
}));

vi.mock('../../../api/fishermenApi', () => ({
  getFishermen: vi.fn(),
}));

vi.mock('../../../hooks', () => ({
  useAuth: vi.fn(),
}));

import { getCompetitionById, getCompetitionResults } from '../../../api/competitionsApi';
import { useAuth } from '../../../hooks';

const mockedGetCompById = getCompetitionById as ReturnType<typeof vi.fn>;
const mockedGetCompResults = getCompetitionResults as ReturnType<typeof vi.fn>;
const mockedUseAuth = useAuth as ReturnType<typeof vi.fn>;

const mockCompetition: CompetitionDto = {
  id: 'comp-101',
  competitionNumber: 1,
  name: 'Concurso Playa',
  date: '2026-04-10T08:00:00Z',
  startTime: '08:00:00',
  endTime: '13:00:00',
  venue: 'Playa Alginet',
  zone: 'Sector A',
  subspecialty: 'Mar',
  category: 'Seniors',
  maxSpots: 30,
  participantCount: 2,
  status: 'ResultsDraft',
  biggestCatchMinWeightInGrams: 500,
  leagueId: 'league-1',
  lastUpdateUtc: '2026-01-01T00:00:00Z',
};

const mockResults: CompetitionResultDto[] = [
  {
    id: 'res-1',
    competitionId: 'comp-101',
    fishermanId: 10,
    registrationDate: '2026-04-01T10:00:00Z',
    assignedSpotNumber: 5,
    didAttend: true,
    weightInGrams: 2500,
    biggestCatchWeight: 750,
    points: 15,
    ranking: 1,
    isValidated: false,
  },
  {
    id: 'res-2',
    competitionId: 'comp-101',
    fishermanId: 12,
    registrationDate: '2026-04-01T10:00:00Z',
    assignedSpotNumber: 12,
    didAttend: true,
    weightInGrams: 1800,
    biggestCatchWeight: null,
    points: 10,
    ranking: 2,
    isValidated: false,
  },
];



describe('CompetitionResultsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockedGetCompById.mockResolvedValue(mockCompetition);
    mockedGetCompResults.mockResolvedValue(mockResults);
  });

  it('como Admin, muestra los resultados con controles de edición y configuración de Pieza Mayor', async () => {
    mockedUseAuth.mockReturnValue({
      user: { id: 'admin-1', username: 'admin', roles: ['Admin'] },
      hasRole: (role: string) => role === 'Admin',
    });

    renderWithProviders(
      <MemoryRouter initialEntries={['/competitions/comp-101/results']}>
        <Routes>
          <Route path="/competitions/:competitionId/results" element={<CompetitionResultsPage />} />
        </Routes>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Resultados del concurso')).toBeInTheDocument();
    });

    expect(screen.getByText('#10')).toBeInTheDocument();
    expect(screen.getByText('#12')).toBeInTheDocument();
    expect(screen.getByText('Mínimo pieza mayor (g):')).toBeInTheDocument();
    expect(screen.getByText('Acciones')).toBeInTheDocument();
  });

  it('como Fisherman (miembro), muestra los resultados en modo solo lectura sin controles de edición', async () => {
    mockedUseAuth.mockReturnValue({
      user: { id: 'user-1', username: 'pescador', roles: ['Fisherman'] },
      hasRole: (role: string) => role === 'Fisherman',
    });

    renderWithProviders(
      <MemoryRouter initialEntries={['/competitions/comp-101/results']}>
        <Routes>
          <Route path="/competitions/:competitionId/results" element={<CompetitionResultsPage />} />
        </Routes>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Resultados del concurso')).toBeInTheDocument();
    });

    expect(screen.getByText('#10')).toBeInTheDocument();
    expect(screen.getByText('#12')).toBeInTheDocument();
    expect(screen.queryByText('Acciones')).not.toBeInTheDocument();
    expect(screen.queryByText('Mínimo pieza mayor (g):')).not.toBeInTheDocument();
    expect(screen.getByText(/Mínimo para calificar como Pieza Mayor:/i)).toBeInTheDocument();
  });

  it('muestra mensaje cuando no hay participantes inscritos', async () => {
    mockedUseAuth.mockReturnValue({
      user: { id: 'user-1', username: 'pescador', roles: ['Fisherman'] },
      hasRole: (role: string) => role === 'Fisherman',
    });
    mockedGetCompResults.mockResolvedValue([]);

    renderWithProviders(
      <MemoryRouter initialEntries={['/competitions/comp-101/results']}>
        <Routes>
          <Route path="/competitions/:competitionId/results" element={<CompetitionResultsPage />} />
        </Routes>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('No hay inscripciones en este concurso.')).toBeInTheDocument();
    });
  });
});
