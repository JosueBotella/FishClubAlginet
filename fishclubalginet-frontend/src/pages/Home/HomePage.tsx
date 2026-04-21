import { Container, Title, Text } from '@mantine/core';
import { useAuth } from '../../hooks';

export default function HomePage() {
  const { user } = useAuth();

  return (
    <Container py="xl">
      <Title order={2}>Bienvenido, {user?.email}</Title>
      <Text c="dimmed" size="sm" mt="xs">
        Roles: {user?.roles.join(', ') || 'Ninguno'}
      </Text>
    </Container>
  );
}
