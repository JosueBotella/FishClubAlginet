import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { renderWithProviders } from '../../../test/renderWithProviders';
import ProfilePage from '../ProfilePage';
import type { AuthUser, FishermanProfileDto } from '../../../types';

// --- Mocks ---
// useAuth: devolvemos un usuario controlado por test.
const mockUseAuth = vi.fn<() => { user: AuthUser | null }>();
vi.mock('../../../hooks', () => ({
  useAuth: () => mockUseAuth(),
}));

// API calls: getMyProfile y authApi.changePassword.
vi.mock('../../../api/fishermenApi', () => ({
  getMyProfile: vi.fn(),
}));

vi.mock('../../../api/authApi', () => ({
  authApi: {
    changePassword: vi.fn(),
  },
}));

import { getMyProfile } from '../../../api/fishermenApi';
import { authApi } from '../../../api/authApi';

const mockedGetMyProfile = getMyProfile as ReturnType<typeof vi.fn>;
const mockedChangePassword = authApi.changePassword as ReturnType<typeof vi.fn>;

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

describe('ProfilePage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseAuth.mockReturnValue({ user: adminUser });
  });

  it('muestra el email y los roles del usuario logado', async () => {
    mockedGetMyProfile.mockResolvedValue(null);

    renderWithProviders(<ProfilePage />);

    expect(screen.getByText('admin@ejemplo.com')).toBeInTheDocument();
    expect(screen.getByText('Admin')).toBeInTheDocument();
    expect(screen.getByText('Fisherman')).toBeInTheDocument();
  });

  it('muestra el bloque "Datos personales" cuando getMyProfile devuelve un perfil', async () => {
    mockedGetMyProfile.mockResolvedValue(sampleProfile);

    renderWithProviders(<ProfilePage />);

    await waitFor(() => {
      expect(
        screen.getByText(/Datos personales \(Pescador\)/i),
      ).toBeInTheDocument();
    });

    expect(screen.getByDisplayValue('Josue')).toBeInTheDocument();
    expect(screen.getByDisplayValue('Botella')).toBeInTheDocument();
    expect(screen.getByDisplayValue(/Dni - 12345678A/)).toBeInTheDocument();
  });

  it('oculta el bloque "Datos personales" cuando el usuario no tiene Fisherman (null)', async () => {
    mockedGetMyProfile.mockResolvedValue(null);

    renderWithProviders(<ProfilePage />);

    // Esperamos a que termine el loading.
    await waitFor(() => {
      expect(mockedGetMyProfile).toHaveBeenCalled();
    });

    expect(
      screen.queryByText(/Datos personales \(Pescador\)/i),
    ).not.toBeInTheDocument();
  });

  it('cambia la contraseña correctamente cuando los campos son válidos', async () => {
    mockedGetMyProfile.mockResolvedValue(null);
    mockedChangePassword.mockResolvedValue({ data: undefined });

    const user = userEvent.setup();
    renderWithProviders(<ProfilePage />);

    await user.type(screen.getByLabelText(/Contrasena actual/i), 'oldPass1');
    await user.type(screen.getByLabelText(/^Contrasena nueva/i), 'newPass1');
    await user.type(screen.getByLabelText(/Confirmar contrasena nueva/i), 'newPass1');

    await user.click(screen.getByRole('button', { name: /Cambiar contrasena/i }));

    await waitFor(() => {
      expect(mockedChangePassword).toHaveBeenCalledWith({
        currentPassword: 'oldPass1',
        newPassword: 'newPass1',
      });
    });
  });

  it('no llama al API si la confirmación no coincide con la nueva contraseña', async () => {
    mockedGetMyProfile.mockResolvedValue(null);

    const user = userEvent.setup();
    renderWithProviders(<ProfilePage />);

    await user.type(screen.getByLabelText(/Contrasena actual/i), 'oldPass1');
    await user.type(screen.getByLabelText(/^Contrasena nueva/i), 'newPass1');
    await user.type(
      screen.getByLabelText(/Confirmar contrasena nueva/i),
      'different',
    );

    await user.click(screen.getByRole('button', { name: /Cambiar contrasena/i }));

    // El form bloquea el submit por validación; no se debe llamar al API.
    expect(mockedChangePassword).not.toHaveBeenCalled();
    expect(
      screen.getByText(/Las contrasenas no coinciden/i),
    ).toBeInTheDocument();
  });
});
