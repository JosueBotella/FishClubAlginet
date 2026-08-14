import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { MemoryRouter, Routes, Route } from 'react-router-dom';
import { renderWithProviders } from '../../../test/renderWithProviders';
import AdminCompetitionsPage from '../AdminCompetitionsPage';
import type { CompetitionDto } from '../../../types';

vi.mock('../../../api/competitionsApi', () => ({
  getCompetitionsByLeague: vi.fn(),
  openRegistration: vi.fn(),
  closeRegistration: vi.fn(),
  reopenRegistration: vi.fn(),
  assignSpots: vi.fn(),
  moveToResultsDraft: vi.fn(),
  validateResults: vi.fn(),
}));

vi.mock('../../../hooks', () => ({
  useAuth: vi.fn(),
}));

import { getCompetitionsByLeague } from '../../../api/competitionsApi';
import { useAuth } from '../../../hooks';

const mockedGetCompetitions = getCompetitionsByLeague as ReturnType<typeof vi.fn>;
const mockedUseAuth = useAuth as ReturnType<typeof vi.fn>;

const mockCompetitions: CompetitionDto[] = [
  {
    id: 'comp-101',
    competitionNumber: 1,
    name: 'Concurso Apertura',
    date: '2026-04-10T08:00:00Z',
    startTime: '08:00:00',
    endTime: '13:00:00',
    venue: 'Playa Alginet',
    zone: 'Sector A',
    subspecialty: 'Mar',
    category: 'Seniors',
    maxSpots: 30,
    participantCount: 15,
    status: 'Planned',
    biggestCatchMinWeightInGrams: 500,
    leagueId: 'league-1',
    lastUpdateUtc: '2026-01-01T00:00:00Z',
  },
  {
    id: 'comp-102',
    competitionNumber: 2,
    name: 'Concurso Puerto',
    date: '2026-05-15T08:00:00Z',
    startTime: '07:30:00',
    endTime: '12:30:00',
    venue: 'Puerto Valencia',
    zone: 'Dársena',
    subspecialty: 'Mar',
    category: 'Seniors',
    maxSpots: 25,

    participantCount: 20,
    status: 'RegistrationOpen',
    biggestCatchMinWeightInGrams: null,
    leagueId: 'league-1',
    lastUpdateUtc: '2026-01-01T00:00:00Z',
  },
];

describe('AdminCompetitionsPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockedGetCompetitions.mockResolvedValue(mockCompetitions);
  });

  it('como Admin, muestra el botón "Nuevo concurso" y los botones de gestión de estado', async () => {
    mockedUseAuth.mockReturnValue({
      user: { id: 'admin-1', username: 'admin', roles: ['Admin'] },
      hasRole: (role: string) => role === 'Admin',
    });

    renderWithProviders(
      <MemoryRouter initialEntries={['/leagues/league-1/competitions']}>
        <Routes>
          <Route path="/leagues/:leagueId/competitions" element={<AdminCompetitionsPage />} />
        </Routes>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Concurso Apertura')).toBeInTheDocument();
    });

    expect(screen.getByText('Nuevo concurso')).toBeInTheDocument();
    expect(screen.getByText('Concurso Puerto')).toBeInTheDocument();
    expect(screen.getByTitle('Volver')).toBeInTheDocument();
  });

  it('como Fisherman (miembro), muestra la lista de concursos pero OCULTA el botón "Nuevo concurso" y botones de gestión', async () => {
    mockedUseAuth.mockReturnValue({
      user: { id: 'user-1', username: 'pescador', roles: ['Fisherman'] },
      hasRole: (role: string) => role === 'Fisherman',
    });

    renderWithProviders(
      <MemoryRouter initialEntries={['/leagues/league-1/competitions']}>
        <Routes>
          <Route path="/leagues/:leagueId/competitions" element={<AdminCompetitionsPage />} />
        </Routes>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('Concurso Apertura')).toBeInTheDocument();
    });

    expect(screen.queryByText('Nuevo concurso')).not.toBeInTheDocument();
    expect(screen.getByText('Concurso Puerto')).toBeInTheDocument();
    expect(screen.getByText('Playa Alginet / Sector A')).toBeInTheDocument();
  });

  it('muestra mensaje si la liga no tiene concursos registrados', async () => {
    mockedUseAuth.mockReturnValue({
      user: { id: 'user-1', username: 'pescador', roles: ['Fisherman'] },
      hasRole: (role: string) => role === 'Fisherman',
    });
    mockedGetCompetitions.mockResolvedValue([]);

    renderWithProviders(
      <MemoryRouter initialEntries={['/leagues/league-1/competitions']}>
        <Routes>
          <Route path="/leagues/:leagueId/competitions" element={<AdminCompetitionsPage />} />
        </Routes>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText('No hay concursos en esta liga.')).toBeInTheDocument();
    });
  });

  it('muestra alerta de error si falla la llamada API', async () => {
    mockedUseAuth.mockReturnValue({
      user: { id: 'user-1', username: 'pescador', roles: ['Fisherman'] },
      hasRole: (role: string) => role === 'Fisherman',
    });
    mockedGetCompetitions.mockRejectedValue(new Error('Network error'));

    renderWithProviders(
      <MemoryRouter initialEntries={['/leagues/league-1/competitions']}>
        <Routes>
          <Route path="/leagues/:leagueId/competitions" element={<AdminCompetitionsPage />} />
        </Routes>
      </MemoryRouter>,
    );

    await waitFor(() => {
      expect(screen.getByText(/Error al cargar los concursos de la liga/i)).toBeInTheDocument();
    });
  });
});
