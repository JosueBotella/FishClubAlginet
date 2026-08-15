import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MemoryRouter } from 'react-router-dom';
import { renderWithProviders } from '../../../test/renderWithProviders';
import ProfilePage from '../ProfilePage';
import type { AuthUser, FishermanProfileDto, MyCompetitionRegistrationDto, LeagueDto, CompetitionDto } from '../../../types';

// --- Mocks ---
const mockUseAuth = vi.fn<() => { user: AuthUser | null }>();
vi.mock('../../../hooks', () => ({
  useAuth: () => mockUseAuth(),
}));

vi.mock('../../../api/fishermenApi', () => ({
  getMyProfile: vi.fn(),
}));

vi.mock('../../../api/authApi', () => ({
  authApi: {
    changePassword: vi.fn(),
  },
}));

vi.mock('../../../api/competitionsApi', () => ({
  getMyRegistrations: vi.fn(),
  getCompetitionsByLeague: vi.fn(),
  registerFisherman: vi.fn(),
  removeRegistration: vi.fn(),
}));

vi.mock('../../../api', () => ({
  leaguesApi: {
    getAllLeagues: vi.fn(),
  },
}));

import { getMyProfile } from '../../../api/fishermenApi';
import { authApi } from '../../../api/authApi';
import {
  getMyRegistrations,
  getCompetitionsByLeague,
  registerFisherman,
} from '../../../api/competitionsApi';
import { leaguesApi } from '../../../api';

const mockedGetMyProfile = getMyProfile as ReturnType<typeof vi.fn>;
const mockedChangePassword = authApi.changePassword as ReturnType<typeof vi.fn>;
const mockedGetMyRegistrations = getMyRegistrations as ReturnType<typeof vi.fn>;
const mockedGetCompetitionsByLeague = getCompetitionsByLeague as ReturnType<typeof vi.fn>;
const mockedRegisterFisherman = registerFisherman as ReturnType<typeof vi.fn>;
const mockedGetAllLeagues = leaguesApi.getAllLeagues as ReturnType<typeof vi.fn>;

const adminUser: AuthUser = {
  id: 'u-1',
  email: 'admin@ejemplo.com',
  roles: ['Admin', 'Fisherman'],
};

const sampleProfile: FishermanProfileDto = {
  id: 1,
  firstName: 'Josue',
  lastName: 'Botella',
  dateOfBirth: '1990-01-15T00:00:00Z',
  documentType: 'Dni',
  documentNumber: '12345678A',
  federationLicense: 'FED-001',
  regionalLicense: null,
  street: 'C/ Mayor',
  number: '12',
  floorDoor: '3 izq',
  zipCode: '46230',
  city: 'Alginet',
  province: 'Valencia',
};

const sampleRegistrations: MyCompetitionRegistrationDto[] = [
  {
    resultId: 'res-1',
    competitionId: 'comp-1',
    competitionName: 'Manga Albufera',
    competitionNumber: 1,
    leagueId: 'league-1',
    leagueName: 'Liga Mar 2026',
    date: '2026-09-10T08:00:00Z',
    startTime: '08:00:00',
    endTime: '14:00:00',
    venue: 'Albufera',
    zone: 'Sector A',
    subspecialty: 'Mar',
    category: 'Seniors',
    status: 'RegistrationOpen',
    assignedSpotNumber: 5,
    weightInGrams: 2400,
    biggestCatchWeight: 1200,
    points: 15,
    ranking: 1,
    isValidated: true,
    didAttend: true,
    registrationDate: '2026-08-15T10:00:00Z',
  },
];

const sampleLeagues: LeagueDto[] = [
  {
    id: 'league-1',
    name: 'Liga Mar 2026',
    year: 2026,
    subspecialty: 'Mar',
    category: 'Seniors',
    isActive: true,
    isArchived: false,
    worstResultsToDiscard: 1,
  },
];

const sampleOpenComps: CompetitionDto[] = [
  {
    id: 'comp-2',
    leagueId: 'league-1',
    competitionNumber: 2,
    name: 'Manga Cullera',
    date: '2026-10-15T08:00:00Z',
    startTime: '08:00:00',
    endTime: '14:00:00',
    venue: 'Cullera',
    zone: null,
    subspecialty: 'Mar',
    category: 'Seniors',
    status: 'RegistrationOpen',
    maxSpots: 20,
    participantCount: 8,
    lastUpdateUtc: '2026-08-15T00:00:00Z',
    biggestCatchMinWeightInGrams: 500,
  },
];

function renderProfilePage() {
  return renderWithProviders(
    <MemoryRouter>
      <ProfilePage />
    </MemoryRouter>
  );
}

describe('ProfilePage (Member Portal)', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseAuth.mockReturnValue({ user: adminUser });
    mockedGetMyProfile.mockResolvedValue(sampleProfile);
    mockedGetMyRegistrations.mockResolvedValue(sampleRegistrations);
    mockedGetAllLeagues.mockResolvedValue(sampleLeagues);
    mockedGetCompetitionsByLeague.mockResolvedValue(sampleOpenComps);
  });

  it('muestra el nombre del pescador en el encabezado y sus roles', async () => {
    renderProfilePage();

    await waitFor(() => {
      expect(screen.getByText('Josue Botella')).toBeInTheDocument();
    });

    expect(screen.getAllByText('Admin').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Fisherman').length).toBeGreaterThan(0);
  });

  it('muestra la ficha del pescador en la pestaña de perfil', async () => {
    renderProfilePage();

    await waitFor(() => {
      expect(screen.getByText(/Ficha del Pescador/i)).toBeInTheDocument();
    });

    expect(screen.getByDisplayValue('Josue')).toBeInTheDocument();
    expect(screen.getByDisplayValue('Botella')).toBeInTheDocument();
    expect(screen.getByDisplayValue(/Dni - 12345678A/)).toBeInTheDocument();
  });

  it('permite cambiar a la pestaña Mis Concursos y ver la tabla con inscripciones', async () => {
    const user = userEvent.setup();
    renderProfilePage();

    await waitFor(() => {
      expect(screen.getByText(/Mis Concursos/i)).toBeInTheDocument();
    });

    await user.click(screen.getByRole('tab', { name: /Mis Concursos/i }));

    await waitFor(() => {
      expect(screen.getByText(/#1 Manga Albufera/i)).toBeInTheDocument();
      expect(screen.getByText(/Liga Mar 2026/i)).toBeInTheDocument();
      expect(screen.getByText(/Pesquera #5/i)).toBeInTheDocument();
      expect(screen.getByText(/2400 g/i)).toBeInTheDocument();
    });
  });

  it('permite cambiar a la pestaña Inscripciones Abiertas y registrarse', async () => {
    const user = userEvent.setup();
    mockedRegisterFisherman.mockResolvedValue({ id: 'res-2' });

    renderProfilePage();

    await waitFor(() => {
      expect(screen.getByRole('tab', { name: /Inscripciones Abiertas/i })).toBeInTheDocument();
    });

    await user.click(screen.getByRole('tab', { name: /Inscripciones Abiertas/i }));

    await waitFor(() => {
      expect(screen.getByText(/Manga Cullera/i)).toBeInTheDocument();
    });

    const enrollBtn = screen.getByRole('button', { name: /Inscribirme ahora/i });
    await user.click(enrollBtn);

    await waitFor(() => {
      expect(mockedRegisterFisherman).toHaveBeenCalledWith('comp-2', 1);
    });
  });

  it('cambia la contraseña correctamente cuando los campos son válidos', async () => {
    mockedChangePassword.mockResolvedValue({ data: undefined });

    const user = userEvent.setup();
    renderProfilePage();

    await user.type(screen.getByLabelText(/Contraseña actual/i), 'oldPass1');
    await user.type(screen.getByLabelText(/^Contraseña nueva/i), 'newPass1');
    await user.type(screen.getByLabelText(/Confirmar contraseña nueva/i), 'newPass1');

    await user.click(screen.getByRole('button', { name: /Actualizar contraseña/i }));

    await waitFor(() => {
      expect(mockedChangePassword).toHaveBeenCalledWith({
        currentPassword: 'oldPass1',
        newPassword: 'newPass1',
      });
    });
  });
});
