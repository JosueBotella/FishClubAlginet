import { Title, Text, Button, Container, Group } from '@mantine/core';
import { IconArrowLeft } from '@tabler/icons-react';
import { useNavigate } from 'react-router-dom';
import { Routes } from '../../constants';

export default function NotFoundPage() {
  const navigate = useNavigate();

  return (
    <Container py="xl">
      <Title order={1} c="dimmed">
        404
      </Title>
      <Title order={3} mt="xs">
        Pagina no encontrada
      </Title>
      <Text c="dimmed" mt="sm">
        La ruta solicitada no existe.
      </Text>
      <Group mt="lg">
        <Button
          variant="light"
          leftSection={<IconArrowLeft size={18} />}
          onClick={() => navigate(Routes.Home)}
        >
          Volver al inicio
        </Button>
      </Group>
    </Container>
  );
}
