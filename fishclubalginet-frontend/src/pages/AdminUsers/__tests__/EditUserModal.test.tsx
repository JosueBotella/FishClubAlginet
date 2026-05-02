import { describe, it, expect, vi, beforeEach } from 'vitest';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { renderWithProviders } from '../../../test/renderWithProviders';
import EditUserModal from '../EditUserModal';
import type { UserDto } from '../../../types';

// Mockeamos el API client para que los tests no llamen a la red.
// Las funciones quedan controladas con vi.fn() para poder verificar llamadas.
vi.mock('../../../api/usersApi', () => ({
  assignRole: vi.fn(),
  removeRole: vi.fn(),
}));

import { assignRole, removeRole } from '../../../api/usersApi';

const userOnlyFisherman: UserDto = {
  id: 'u-1',
  email: 'pescador@ejemplo.com',
  isLockedOut: false,
  roles: ['Fisherman'],
};

const userBothRoles: UserDto = {
  id: 'u-2',
  email: 'admin@ejemplo.com',
  isLockedOut: false,
  roles: ['Admin', 'Fisherman'],
};

describe('EditUserModal', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    (assignRole as ReturnType<typeof vi.fn>).mockResolvedValue(undefined);
    (removeRole as ReturnType<typeof vi.fn>).mockResolvedValue(undefined);
  });

  it('no renderiza el contenido del modal cuando user es null', () => {
    renderWithProviders(
      <EditUserModal user={null} onClose={() => {}} onUpdated={() => {}} />,
    );
    // Sin user, el Modal no muestra ningún contenido editable.
    expect(screen.queryByText(/Editar usuario/i)).not.toBeInTheDocument();
  });

  it('preselecciona los roles que ya tiene el usuario', () => {
    renderWithProviders(
      <EditUserModal
        user={userOnlyFisherman}
        onClose={() => {}}
        onUpdated={() => {}}
      />,
    );

    const adminCheckbox = screen.getByRole('checkbox', { name: /Administrador/i });
    const fishermanCheckbox = screen.getByRole('checkbox', { name: /Pescador/i });

    expect(adminCheckbox).not.toBeChecked();
    expect(fishermanCheckbox).toBeChecked();
  });

  it('Guardar está deshabilitado si no hay cambios', () => {
    renderWithProviders(
      <EditUserModal
        user={userBothRoles}
        onClose={() => {}}
        onUpdated={() => {}}
      />,
    );

    const saveButton = screen.getByRole('button', { name: /Guardar cambios/i });
    expect(saveButton).toBeDisabled();
  });

  it('Guardar se deshabilita si se quitan todos los roles', async () => {
    const user = userEvent.setup();
    renderWithProviders(
      <EditUserModal
        user={userOnlyFisherman}
        onClose={() => {}}
        onUpdated={() => {}}
      />,
    );

    const fishermanCheckbox = screen.getByRole('checkbox', { name: /Pescador/i });
    await user.click(fishermanCheckbox); // desmarca el único rol

    expect(fishermanCheckbox).not.toBeChecked();
    expect(
      screen.getByRole('button', { name: /Guardar cambios/i }),
    ).toBeDisabled();
    expect(
      screen.getByText(/El usuario debe tener al menos un rol/i),
    ).toBeInTheDocument();
  });

  it('al añadir un rol, llama solo a assignRole con el rol nuevo', async () => {
    const user = userEvent.setup();
    const onUpdated = vi.fn();

    renderWithProviders(
      <EditUserModal
        user={userOnlyFisherman}
        onClose={() => {}}
        onUpdated={onUpdated}
      />,
    );

    // Añade rol Admin
    await user.click(screen.getByRole('checkbox', { name: /Administrador/i }));

    // El alert resumen debe avisar del cambio
    expect(screen.getByText(/Se asignaran:/)).toBeInTheDocument();

    // Guarda
    await user.click(screen.getByRole('button', { name: /Guardar cambios/i }));

    expect(assignRole).toHaveBeenCalledTimes(1);
    expect(assignRole).toHaveBeenCalledWith('u-1', { role: 'Admin' });
    expect(removeRole).not.toHaveBeenCalled();
    expect(onUpdated).toHaveBeenCalledTimes(1);
  });

  it('al quitar un rol, llama solo a removeRole con el rol eliminado', async () => {
    const user = userEvent.setup();
    const onUpdated = vi.fn();

    renderWithProviders(
      <EditUserModal
        user={userBothRoles}
        onClose={() => {}}
        onUpdated={onUpdated}
      />,
    );

    // Quita rol Admin (deja solo Fisherman)
    await user.click(screen.getByRole('checkbox', { name: /Administrador/i }));

    // Guarda
    await user.click(screen.getByRole('button', { name: /Guardar cambios/i }));

    expect(removeRole).toHaveBeenCalledTimes(1);
    expect(removeRole).toHaveBeenCalledWith('u-2', { role: 'Admin' });
    expect(assignRole).not.toHaveBeenCalled();
    expect(onUpdated).toHaveBeenCalledTimes(1);
  });

  it('al sustituir roles, hace primero el assign y luego el remove (sin estado intermedio sin roles)', async () => {
    const user = userEvent.setup();
    const callOrder: string[] = [];

    (assignRole as ReturnType<typeof vi.fn>).mockImplementation(async () => {
      callOrder.push('assign');
    });
    (removeRole as ReturnType<typeof vi.fn>).mockImplementation(async () => {
      callOrder.push('remove');
    });

    renderWithProviders(
      <EditUserModal
        user={userOnlyFisherman}
        onClose={() => {}}
        onUpdated={() => {}}
      />,
    );

    // Añade Admin y quita Fisherman → debería ejecutar assign primero
    await user.click(screen.getByRole('checkbox', { name: /Administrador/i }));
    await user.click(screen.getByRole('checkbox', { name: /Pescador/i }));

    await user.click(screen.getByRole('button', { name: /Guardar cambios/i }));

    expect(callOrder).toEqual(['assign', 'remove']);
  });
});
