import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import {
  Card,
  TextInput,
  PasswordInput,
  Button,
  Title,
  Text,
  Stack,
  Center,
  Alert,
} from '@mantine/core';
import { useForm } from '@mantine/form';
import { IconLogin, IconAlertCircle } from '@tabler/icons-react';
import { useAuth } from '../../hooks';
import { authApi } from '../../api';
import { Routes } from '../../constants';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const form = useForm({
    initialValues: {
      userName: '',
      password: '',
    },
    validate: {
      userName: (v) => (v.trim().length === 0 ? 'Email es obligatorio' : null),
      password: (v) => (v.length === 0 ? 'Contraseña es obligatoria' : null),
    },
  });

  const handleSubmit = async (values: typeof form.values) => {
    setError(null);
    setIsLoading(true);

    try {
      const response = await authApi.login(values);
      login(response.data.token);

      const returnUrl = searchParams.get('returnUrl') ?? Routes.Home;
      navigate(returnUrl, { replace: true });
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        setError('Credenciales inválidas.');
      } else {
        setError('Error de conexión. Inténtalo de nuevo.');
      }
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Center h="100vh" bg="gray.1">
      <Card shadow="md" padding="xl" radius="md" w={400} withBorder>
        <Stack gap="xs" align="center" mb="lg">
          <Title order={3}>Bienvenido</Title>
          <Text c="dimmed" size="sm">
            Introduce tus credenciales
          </Text>
        </Stack>

        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack gap="md">
            <TextInput
              label="Email"
              placeholder="ejemplo@correo.com"
              autoComplete="username"
              {...form.getInputProps('userName')}
            />

            <PasswordInput
              label="Contraseña"
              placeholder="******"
              autoComplete="current-password"
              {...form.getInputProps('password')}
            />

            {error && (
              <Alert
                icon={<IconAlertCircle size={16} />}
                color="red"
                variant="light"
              >
                {error}
              </Alert>
            )}

            <Button
              type="submit"
              fullWidth
              loading={isLoading}
              leftSection={<IconLogin size={18} />}
              mt="xs"
            >
              Iniciar Sesión
            </Button>
          </Stack>
        </form>
      </Card>
    </Center>
  );
}
