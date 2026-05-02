import { useEffect, useMemo, useState } from 'react';
import {
  Modal,
  TextInput,
  Checkbox,
  Stack,
  Button,
  Group,
  Alert,
  Text,
} from '@mantine/core';
import { notifications } from '@mantine/notifications';
import { assignRole, removeRole } from '../../api/usersApi';
import type { UserDto } from '../../types';

interface EditUserModalProps {
  /** Usuario a editar. Si es null, el modal está cerrado. */
  user: UserDto | null;
  onClose: () => void;
  onUpdated: () => void;
}

const AVAILABLE_ROLES = ['Admin', 'Fisherman'] as const;

export default function EditUserModal({
  user,
  onClose,
  onUpdated,
}: EditUserModalProps) {
  const [selectedRoles, setSelectedRoles] = useState<string[]>([]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Sincroniza el estado local cada vez que cambia el usuario seleccionado
  useEffect(() => {
    if (user) {
      setSelectedRoles([...user.roles]);
      setError(null);
    }
  }, [user]);

  const initialRoles = useMemo(() => user?.roles ?? [], [user]);

  // Calcula qué roles añadir y cuáles quitar respecto al estado original
  const { rolesToAdd, rolesToRemove } = useMemo(() => {
    const toAdd = selectedRoles.filter((r) => !initialRoles.includes(r));
    const toRemove = initialRoles.filter((r) => !selectedRoles.includes(r));
    return { rolesToAdd: toAdd, rolesToRemove: toRemove };
  }, [selectedRoles, initialRoles]);

  const hasChanges = rolesToAdd.length > 0 || rolesToRemove.length > 0;
  const hasAtLeastOneRole = selectedRoles.length > 0;

  const handleToggleRole = (role: string, checked: boolean) => {
    setSelectedRoles((prev) =>
      checked ? [...prev, role] : prev.filter((r) => r !== role)
    );
  };

  const handleSave = async () => {
    if (!user || !hasChanges) return;

    setSaving(true);
    setError(null);
    try {
      // Aplicar primero los add y luego los remove. Si todo es atómico
      // a nivel de UX no importa, pero al menos garantizamos que el usuario
      // no se queda sin roles intermedios cuando se sustituye uno por otro.
      for (const role of rolesToAdd) {
        await assignRole(user.id, { role });
      }
      for (const role of rolesToRemove) {
        await removeRole(user.id, { role });
      }

      notifications.show({
        title: 'Roles actualizados',
        message: user.email,
        color: 'green',
      });
      onUpdated();
    } catch (err: unknown) {
      const msg =
        err instanceof Error
          ? err.message
          : 'No se pudieron actualizar los roles.';
      setError(msg);
    } finally {
      setSaving(false);
    }
  };

  const handleClose = () => {
    if (saving) return;
    setError(null);
    onClose();
  };

  return (
    <Modal
      opened={user !== null}
      onClose={handleClose}
      title="Editar usuario"
      closeOnClickOutside={!saving}
    >
      {user && (
        <Stack>
          {error && <Alert color="red">{error}</Alert>}

          <TextInput
            label="Email"
            value={user.email}
            readOnly
            variant="filled"
          />

          <div>
            <Text size="sm" fw={500} mb={6}>
              Roles
            </Text>
            <Stack gap={6}>
              {AVAILABLE_ROLES.map((role) => (
                <Checkbox
                  key={role}
                  label={role === 'Admin' ? 'Administrador' : 'Pescador'}
                  checked={selectedRoles.includes(role)}
                  onChange={(e) => handleToggleRole(role, e.currentTarget.checked)}
                  disabled={saving}
                />
              ))}
            </Stack>
            {!hasAtLeastOneRole && (
              <Text size="xs" c="red" mt={4}>
                El usuario debe tener al menos un rol.
              </Text>
            )}
          </div>

          {hasChanges && (
            <Alert color="blue" variant="light">
              <Text size="sm">
                {rolesToAdd.length > 0 && (
                  <>Se asignaran: <strong>{rolesToAdd.join(', ')}</strong>. </>
                )}
                {rolesToRemove.length > 0 && (
                  <>Se quitaran: <strong>{rolesToRemove.join(', ')}</strong>.</>
                )}
              </Text>
            </Alert>
          )}

          <Group justify="flex-end" mt="md">
            <Button variant="default" onClick={handleClose} disabled={saving}>
              Cancelar
            </Button>
            <Button
              onClick={handleSave}
              loading={saving}
              disabled={saving || !hasChanges || !hasAtLeastOneRole}
            >
              Guardar cambios
            </Button>
          </Group>
        </Stack>
      )}
    </Modal>
  );
}
