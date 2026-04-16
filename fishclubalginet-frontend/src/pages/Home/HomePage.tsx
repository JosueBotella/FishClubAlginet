import { Button, Container, Title, Text, Group } from '@mantine/core';
import { IconLogout } from '@tabler/icons-react';
import { useAuth } from '../../hooks';

export default function HomePage() {
  const { user, logout } = useAuth();

  return (
    <Container py="xl">
      <Group justify="space-between" mb="xl">
        <div>
          <Title order={2}>Bienvenido, {user?.email}</Title>
          <Text c="dimmed" size="sm">
            Roles: {user?.roles.join(', ') || 'Ninguno'}
          </Text>
        </div>
        <Button
          variant="light"
          color="red"
          leftSection={<IconLogout size={18} />}
          onClick={logout}
        >
          Cerrar sesión
        </Button>
      </Group>
    </Container>
  );
}
