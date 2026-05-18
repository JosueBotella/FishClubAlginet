import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Title,
  Table,
  Group,
  ActionIcon,
  Text,
  Tooltip,
  Loader,
  Center,
  Alert,
  Badge,
  Pagination,
} from '@mantine/core';
import {
  IconChevronLeft,
  IconArchiveOff,
  IconCalendarEvent,
  IconHistory,
} from '@tabler/icons-react';
import { notifications } from '@mantine/notifications';
import { getLeagues, unarchiveLeague } from '../../api/leaguesApi';
import type { LeagueDto } from '../../types';
import { Routes } from '../../constants';
import ConfirmationModal from '../../components/ConfirmationModal';

const PAGE_SIZE = 15;

export default function AdminArchivedLeaguesPage() {
  const [leagues, setLeagues] = useState<LeagueDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [unarchiveTarget, setUnarchiveTarget] = useState<LeagueDto | null>(null);
  const [unarchiving, setUnarchiving] = useState(false);
  const navigate = useNavigate();

  const totalPages = Math.ceil(totalCount / PAGE_SIZE);

  const fetchLeagues = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const skip = (page - 1) * PAGE_SIZE;
      const result = await getLeagues(skip, PAGE_SIZE, undefined, true);
      setLeagues(result.items);
      setTotalCount(result.totalCount);
    } catch {
      setError('Error al cargar las ligas archivadas.');
    } finally {
      setLoading(false);
    }
  }, [page]);

  useEffect(() => {
    fetchLeagues();
  }, [fetchLeagues]);

  const handleUnarchiveConfirm = async () => {
    if (!unarchiveTarget) return;
    setUnarchiving(true);
    try {
      await unarchiveLeague(unarchiveTarget.id);
      notifications.show({
        title: 'Liga desarchivada',
        message: `${unarchiveTarget.name} (${unarchiveTarget.year}) está disponible en el listado principal.`,
        color: 'blue',
      });
      setUnarchiveTarget(null);
      await fetchLeagues();
    } catch {
      notifications.show({
        title: 'Error',
        message: 'No se pudo desarchivar la liga.',
        color: 'red',
      });
    } finally {
      setUnarchiving(false);
    }
  };

  return (
    <Container size="lg" py="md">
      <Group mb="md">
        <ActionIcon variant="subtle" onClick={() => navigate(Routes.Leagues)} title="Volver a ligas">
          <IconChevronLeft size={20} />
        </ActionIcon>
        <Title order={3}>
          <Group gap={6} component="span">
            <IconHistory size={22} />
            Histórico de ligas archivadas
          </Group>
        </Title>
      </Group>

      {error && (
        <Alert color="red" mb="md">
          {error}
        </Alert>
      )}

      {loading ? (
        <Center py="xl">
          <Loader size="lg" />
        </Center>
      ) : leagues.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">
          No hay ligas archivadas.
        </Text>
      ) : (
        <>
          <Table striped highlightOnHover withTableBorder>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Nombre</Table.Th>
                <Table.Th>Año</Table.Th>
                <Table.Th>Estado</Table.Th>
                <Table.Th>Concursos</Table.Th>
                <Table.Th style={{ width: 120 }}>Acciones</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {leagues.map((l) => (
                <Table.Tr key={l.id}>
                  <Table.Td>
                    <Text size="sm" fw={500}>{l.name}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm">{l.year}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Badge color="gray" variant="light">Archivada</Badge>
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm">{l.competitionsCount}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Group gap={4}>
                      <Tooltip label="Ver concursos">
                        <ActionIcon
                          variant="subtle"
                          color="teal"
                          onClick={() => navigate(Routes.competitionsFor(l.id))}
                        >
                          <IconCalendarEvent size={18} />
                        </ActionIcon>
                      </Tooltip>
                      <Tooltip label="Desarchivar">
                        <ActionIcon
                          variant="subtle"
                          color="blue"
                          onClick={() => setUnarchiveTarget(l)}
                        >
                          <IconArchiveOff size={18} />
                        </ActionIcon>
                      </Tooltip>
                    </Group>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>

          {totalPages > 1 && (
            <Group justify="center" mt="md">
              <Pagination total={totalPages} value={page} onChange={setPage} size="sm" />
              <Text size="xs" c="dimmed">
                {totalCount} liga{totalCount !== 1 ? 's' : ''} archivada{totalCount !== 1 ? 's' : ''}
              </Text>
            </Group>
          )}
        </>
      )}

      <ConfirmationModal
        opened={unarchiveTarget !== null}
        title="Desarchivar liga"
        description={`¿Quieres recuperar "${unarchiveTarget?.name} (${unarchiveTarget?.year})" del histórico? Quedará inactiva y deberás activarla manualmente si es necesario.`}
        confirmLabel="Desarchivar"
        confirmColor="blue"
        isLoading={unarchiving}
        onConfirm={handleUnarchiveConfirm}
        onCancel={() => setUnarchiveTarget(null)}
      />
    </Container>
  );
}
