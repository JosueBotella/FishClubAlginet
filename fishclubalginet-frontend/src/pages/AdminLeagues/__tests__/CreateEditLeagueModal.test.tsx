import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { renderWithProviders } from '../../../test/renderWithProviders';
import CreateEditLeagueModal from '../CreateEditLeagueModal';
import type { LeagueDto } from '../../../types';

// Mock del API: solo necesitamos saber que se llama y con qué argumentos.
vi.mock('../../../api/leaguesApi', () => ({
  createLeague: vi.fn(),
  updateLeague: vi.fn(),
}));

import { createLeague, updateLeague } from '../../../api/leaguesApi';

const mockedCreate = createLeague as ReturnType<typeof vi.fn>;
const mockedUpdate = updateLeague as ReturnType<typeof vi.fn>;

const existingLeague: LeagueDto = {
  id: 'abc-123',
  name: 'Liga 2025',
  year: 2025,
  isActive: false,
  isArchived: false,
  minPoints: 5,
  worstResultsToDiscard: 2,
  competitionsCount: 0,
  lastUpdateUtc: '2025-01-01T00:00:00Z',
};

describe('CreateEditLeagueModal', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockedCreate.mockResolvedValue({ id: 'new-id' });
    mockedUpdate.mockResolvedValue(existingLeague);
  });

  it('en modo crear, muestra el título "Crear liga" y campos vacíos por defecto', async () => {
    renderWithProviders(
      <CreateEditLeagueModal
        opened={true}
        league={null}
        onClose={() => {}}
        onSuccess={() => {}}
      />,
    );

    expect(screen.getByText(/Crear liga/i)).toBeInTheDocument();
    // Año por defecto = año actual
    const yearInput = screen.getByLabelText(/Año/i);
    expect(yearInput).toHaveValue(String(new Date().getFullYear()));
  });

  it('en modo editar, precarga los valores de la liga y deshabilita el campo Año', () => {
    renderWithProviders(
      <CreateEditLeagueModal
        opened={true}
        league={existingLeague}
        onClose={() => {}}
        onSuccess={() => {}}
      />,
    );

    expect(screen.getByText(/Editar liga/i)).toBeInTheDocument();
    expect(screen.getByDisplayValue('Liga 2025')).toBeInTheDocument();

    const yearInput = screen.getByLabelText(/Año/i);
    expect(yearInput).toBeDisabled();
  });

  it('botón submit deshabilitado cuando no hay cambios (form no dirty)', () => {
    renderWithProviders(
      <CreateEditLeagueModal
        opened={true}
        league={existingLeague}
        onClose={() => {}}
        onSuccess={() => {}}
      />,
    );
    expect(screen.getByRole('button', { name: /Guardar cambios/i })).toBeDisabled();
  });

  it('al crear con datos válidos, llama a createLeague con los valores del form y dispara onSuccess', async () => {
    const user = userEvent.setup();
    const onSuccess = vi.fn();

    renderWithProviders(
      <CreateEditLeagueModal
        opened={true}
        league={null}
        onClose={() => {}}
        onSuccess={onSuccess}
      />,
    );

    await user.type(screen.getByLabelText(/Nombre/i), 'Liga 2027');

    await user.click(screen.getByRole('button', { name: /^Crear$/i }));

    await waitFor(() => {
      expect(mockedCreate).toHaveBeenCalledTimes(1);
    });
    expect(mockedCreate).toHaveBeenCalledWith(
      expect.objectContaining({ name: 'Liga 2027', minPoints: 5, worstResultsToDiscard: 0 }),
    );
    expect(onSuccess).toHaveBeenCalledTimes(1);
    expect(mockedUpdate).not.toHaveBeenCalled();
  });

  it('al editar, llama a updateLeague (no createLeague) con sólo los campos editables', async () => {
    const user = userEvent.setup();
    const onSuccess = vi.fn();

    renderWithProviders(
      <CreateEditLeagueModal
        opened={true}
        league={existingLeague}
        onClose={() => {}}
        onSuccess={onSuccess}
      />,
    );

    // Modificamos el nombre para que el form esté dirty
    const nameInput = screen.getByDisplayValue('Liga 2025');
    await user.clear(nameInput);
    await user.type(nameInput, 'Liga 2025 (renombrada)');

    await user.click(screen.getByRole('button', { name: /Guardar cambios/i }));

    await waitFor(() => {
      expect(mockedUpdate).toHaveBeenCalledTimes(1);
    });
    expect(mockedUpdate).toHaveBeenCalledWith(
      'abc-123',
      expect.objectContaining({
        name: 'Liga 2025 (renombrada)',
        minPoints: 5,
        worstResultsToDiscard: 2,
      }),
    );
    // El payload de update NO debe incluir year — el backend no lo permite
    const callBody = mockedUpdate.mock.calls[0][1] as Record<string, unknown>;
    expect(callBody).not.toHaveProperty('year');
    expect(onSuccess).toHaveBeenCalledTimes(1);
  });

  it('valida que el nombre no esté vacío', async () => {
    const user = userEvent.setup();

    renderWithProviders(
      <CreateEditLeagueModal
        opened={true}
        league={null}
        onClose={() => {}}
        onSuccess={() => {}}
      />,
    );

    // Sin escribir nombre, intentar enviar
    await user.click(screen.getByRole('button', { name: /^Crear$/i }));

    expect(
      screen.getByText(/El nombre es obligatorio/i),
    ).toBeInTheDocument();
    expect(mockedCreate).not.toHaveBeenCalled();
  });
});
