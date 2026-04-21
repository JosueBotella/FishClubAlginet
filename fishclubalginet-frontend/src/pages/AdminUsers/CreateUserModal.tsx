import { useState } from 'react';
import {
  Modal,
  TextInput,
  PasswordInput,
  Select,
  Button,
  Stack,
  Alert,
} from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import { createUser } from '../../api/usersApi';

interface CreateUserModalProps {
  opened: boolean;
  onClose: () => void;
  onCreated: () => void;
}

export default function CreateUserModal({
  opened,
  onClose,
  onCreated,
}: CreateUserModalProps) {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const form = useForm({
    initialValues: {
      email: '',
      password: '',
      role: 'Fisherman',
    },
    validate: {
      email: (v) =>
        /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(v) ? null : 'Email no valido',
      password: (v) =>
        v.length >= 6 ? null : 'Minimo 6 caracteres',
      role: (v) =>
        ['Admin', 'Fisherman'].includes(v) ? null : 'Rol no valido',
    },
  });

  const handleSubmit = async (values: typeof form.values) => {
    setLoading(true);
    setError(null);
    try {
      await createUser({
        email: values.email,
        password: values.password,
        role: values.role,
      });
      notifications.show({
        title: 'Usuario creado',
        message: values.email,
        color: 'green',
      });
      form.reset();
      onCreated();
    } catch (err: unknown) {
      const msg =
        err instanceof Error ? err.message : 'Error al crear el usuario.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  const handleClose = () => {
    form.reset();
    setError(null);
    onClose();
  };

  return (
    <Modal opened={opened} onClose={handleClose} title="Crear usuario">
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          {error && <Alert color="red">{error}</Alert>}

          <TextInput
            label="Email"
            placeholder="usuario@ejemplo.com"
            required
            {...form.getInputProps('email')}
          />

          <PasswordInput
            label="Contrasena"
            placeholder="Minimo 6 caracteres"
            required
            {...form.getInputProps('password')}
          />

          <Select
            label="Rol"
            data={[
              { value: 'Admin', label: 'Administrador' },
              { value: 'Fisherman', label: 'Pescador' },
            ]}
            required
            {...form.getInputProps('role')}
          />

          <Button type="submit" loading={loading} fullWidth>
            Crear
          </Button>
        </Stack>
      </form>
    </Modal>
  );
}
