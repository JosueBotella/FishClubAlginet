import { useEffect, useState } from 'react';
import {
  Container,
  Title,
  Paper,
  Stack,
  Group,
  Text,
  Badge,
  Divider,
  TextInput,
  PasswordInput,
  Button,
  Alert,
  Loader,
  Center,
  SimpleGrid,
} from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import { IconUser, IconKey, IconAddressBook } from '@tabler/icons-react';
import { useAuth } from '../../hooks';
import { getMyProfile } from '../../api/fishermenApi';
import { authApi } from '../../api/authApi';
import type { FishermanProfileDto } from '../../types';

export default function ProfilePage() {
  const { user } = useAuth();

  const [profile, setProfile] = useState<FishermanProfileDto | null>(null);
  const [loadingProfile, setLoadingProfile] = useState(true);
  const [profileError, setProfileError] = useState<string | null>(null);

  const [changing, setChanging] = useState(false);
  const [pwdError, setPwdError] = useState<string | null>(null);

  const pwdForm = useForm({
    initialValues: {
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    },
    validate: {
      currentPassword: (v) =>
        v.length === 0 ? 'La contrasena actual es obligatoria' : null,
      newPassword: (v) =>
        v.length >= 6 ? null : 'Minimo 6 caracteres',
      confirmPassword: (v, values) =>
        v === values.newPassword ? null : 'Las contrasenas no coinciden',
    },
  });

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoadingProfile(true);
      setProfileError(null);
      try {
        const data = await getMyProfile();
        if (!cancelled) setProfile(data);
      } catch {
        if (!cancelled) setProfileError('No se pudieron cargar los datos del pescador.');
      } finally {
        if (!cancelled) setLoadingProfile(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, []);

  const handleChangePassword = async (values: typeof pwdForm.values) => {
    setChanging(true);
    setPwdError(null);
    try {
      await authApi.changePassword({
        currentPassword: values.currentPassword,
        newPassword: values.newPassword,
      });
      notifications.show({
        title: 'Contrasena actualizada',
        message: 'Tu contrasena se ha cambiado correctamente.',
        color: 'green',
      });
      pwdForm.reset();
    } catch (err: unknown) {
      const msg =
        err instanceof Error
          ? err.message
          : 'No se pudo cambiar la contrasena. Verifica la actual.';
      setPwdError(msg);
    } finally {
      setChanging(false);
    }
  };

  const formatDate = (iso: string) => {
    const d = new Date(iso);
    return d.toLocaleDateString('es-ES', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  };

  const formatAddress = (p: FishermanProfileDto) => {
    const line1 = [p.street, p.number, p.floorDoor].filter(Boolean).join(' ');
    const line2 = [p.zipCode, p.city, p.province].filter(Boolean).join(' ');
    return [line1, line2].filter(Boolean).join(', ');
  };

  return (
    <Container size="md" py="md">
      <Title order={3} mb="md">
        Mi perfil
      </Title>

      {/* Bloque 1: Datos de cuenta */}
      <Paper p="md" withBorder mb="md">
        <Group mb="sm">
          <IconUser size={20} />
          <Title order={5}>Datos de cuenta</Title>
        </Group>
        <Divider mb="md" />
        <Stack gap="xs">
          <Group>
            <Text fw={500} w={120}>
              Email:
            </Text>
            <Text>{user?.email ?? '-'}</Text>
          </Group>
          <Group>
            <Text fw={500} w={120}>
              Roles:
            </Text>
            <Group gap={4}>
              {user?.roles.map((role) => (
                <Badge
                  key={role}
                  size="sm"
                  variant="light"
                  color={role === 'Admin' ? 'blue' : 'teal'}
                >
                  {role}
                </Badge>
              ))}
            </Group>
          </Group>
        </Stack>
      </Paper>

      {/* Bloque 2: Datos de pescador (oculto si no hay ficha) */}
      {loadingProfile ? (
        <Paper p="md" withBorder mb="md">
          <Center py="md">
            <Loader size="sm" />
          </Center>
        </Paper>
      ) : profileError ? (
        <Alert color="red" mb="md">
          {profileError}
        </Alert>
      ) : profile ? (
        <Paper p="md" withBorder mb="md">
          <Group mb="sm">
            <IconAddressBook size={20} />
            <Title order={5}>Datos personales (Pescador)</Title>
          </Group>
          <Divider mb="md" />
          <SimpleGrid cols={{ base: 1, sm: 2 }} spacing="sm">
            <ProfileField label="Nombre" value={profile.firstName} />
            <ProfileField label="Apellidos" value={profile.lastName} />
            <ProfileField
              label="Fecha de nacimiento"
              value={formatDate(profile.dateOfBirth)}
            />
            <ProfileField
              label="Documento"
              value={`${profile.documentType} - ${profile.documentNumber}`}
            />
            <ProfileField
              label="Licencia federativa"
              value={profile.federationLicense ?? '-'}
            />
            <ProfileField
              label="Licencia regional"
              value={profile.regionalLicense ?? '-'}
            />
            <ProfileField
              label="Direccion"
              value={formatAddress(profile) || '-'}
              span={2}
            />
          </SimpleGrid>
        </Paper>
      ) : null}

      {/* Bloque 3: Cambio de contrasena */}
      <Paper p="md" withBorder>
        <Group mb="sm">
          <IconKey size={20} />
          <Title order={5}>Cambiar contrasena</Title>
        </Group>
        <Divider mb="md" />

        <form onSubmit={pwdForm.onSubmit(handleChangePassword)}>
          <Stack>
            {pwdError && <Alert color="red">{pwdError}</Alert>}

            <PasswordInput
              label="Contrasena actual"
              required
              {...pwdForm.getInputProps('currentPassword')}
            />
            <PasswordInput
              label="Contrasena nueva"
              required
              {...pwdForm.getInputProps('newPassword')}
            />
            <PasswordInput
              label="Confirmar contrasena nueva"
              required
              {...pwdForm.getInputProps('confirmPassword')}
            />

            <Group justify="flex-end">
              <Button type="submit" loading={changing} disabled={changing}>
                Cambiar contrasena
              </Button>
            </Group>
          </Stack>
        </form>
      </Paper>
    </Container>
  );
}

function ProfileField({
  label,
  value,
  span = 1,
}: {
  label: string;
  value: string;
  span?: number;
}) {
  return (
    <TextInput
      label={label}
      value={value}
      readOnly
      variant="filled"
      style={span === 2 ? { gridColumn: '1 / -1' } : undefined}
    />
  );
}
