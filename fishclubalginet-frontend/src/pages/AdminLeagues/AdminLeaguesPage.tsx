import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Title,
  Table,
  Group,
  Badge,
  ActionIcon,
  TextInput,
  Button,
  Pagination,
  Text,
  Tooltip,
  Loader,
  Center,
  Alert,
} from '@mantine/core';
import {
  IconSearch,
  IconPlus,
  IconRefresh,
  IconTrophy,
  IconArchive,
  IconPlayerPlay,
  IconEdit,
  IconCalendarEvent,
  IconHistory,
  IconChartBar,
} from '@tabler/icons-react';
import { notifications } from '@mantine/notifications';
import {
  getLeagues,
  activateLeague,
  archiveLeague,
} from '../../api/leaguesApi';
import type { LeagueDto } from '../../types';
import { Routes } from '../../constants';
import CreateEditLeagueModal from './CreateEditLeagueModal';
import ConfirmationModal from '../../components/ConfirmationModal';

const PAGE_SIZE = 15;

function leagueStatusBadge(league: LeagueDto) {
  if (league.isActive) {
    return <Badge color="green" variant="light">Activa</Badge>;
  }
  return <Badge color="blue" variant="light">Inactiva</Badge>;
}

export default function AdminLeaguesPage() {
  const [leagues, setLeagues] = useState<LeagueDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [searchYear, setSearchYear] = useState<string>('');
  const [yearFilter, setYearFilter] = useState<number | undefined>(undefined);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [editLeague, setEditLeague] = useState<LeagueDto | null>(null);
  const [archiveTarget, setArchiveTarget] = useState<LeagueDto | null>(null);
  const [archiving, setArchiving] = useState(false);
  const [recentlyArchived, setRecentlyArchived] = useState<string | null>(null);
  const navigate = useNavigate();

  const totalPages = Math.ceil(totalCount / PAGE_SIZE);

  const fetchLeagues = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const skip = (page - 1) * PAGE_SIZE;
      // archived=false → solo ligas no archivadas (comportamiento por defecto)
      const result = await getLeagues(skip, PAGE_SIZE, yearFilter, false);
      setLeagues(result.items);
      setTotalCount(result.totalCount);
    } catch {
      setError('Error al cargar las ligas.');
    } finally {
      setLoading(false);
    }
  }, [page, yearFilter]);

  useEffect(() => {
    fetchLeagues();
  }, [fetchLeagues]);

  const handleSearch = () => {
    const parsed = parseInt(searchYear, 10);
    setPage(1);
    setYearFilter(isNaN(parsed) ? undefined : parsed);
  };

  const handleSearchKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') handleSearch();
  };

  const handleActivate = async (league: LeagueDto) => {
    try {
      await activateLeague(league.id);
      notifications.show({
        title: 'Liga activada',
        message: `${league.name} (${league.year})`,
        color: 'green',
      });
      await fetchLeagues();
    } catch {
      notifications.show({
        title: 'Error',
        message: 'No se pudo activar la liga.',
        color: 'red',
      });
    }
  };

  const handleArchiveConfirm = async () => {
    if (!archiveTarget) return;
    setArchiving(true);
    try {
      await archiveLeague(archiveTarget.id);
      const archivedName = `${archiveTarget.name} (${archiveTarget.year})`;
      notifications.show({
        title: 'Liga archivada',
        message: `${archivedName} ha sido archivada.`,
        color: 'gray',
      });
      setRecentlyArchived(archivedName);
      setArchiveTarget(null);
      await fetchLeagues();
    } catch {
      notifications.show({
        title: 'Error',
        message: 'No se pudo archivar la liga.',
        color: 'red',
      });
    } finally {
      setArchiving(false);
    }
  };

  const handleModalClose = () => {
    setModalOpen(false);
    setEditLeague(null);
  };

  const handleSuccess = () => {
    handleModalClose();
    setPage(1);
    fetchLeagues();
  };

  return (
    <Container size="lg" py="md">
      <Group justify="space-between" mb="md">
        <Title order={3}>
          <Group gap={6} component="span">
            <IconTrophy size={22} />
            Gestión de ligas
          </Group>
        </Title>
        <Group gap="sm">
          <Button
            variant="light"
            color="gray"
            leftSection={<IconHistory size={18} />}
            onClick={() => navigate(Routes.ArchivedLeagues)}
          >
            Histórico archivadas
          </Button>
          <Button leftSection={<IconPlus size={18} />} onClick={() => setModalOpen(true)}>
            Crear liga
          </Button>
        </Group>
      </Group>

      <Group mb="md">
        <TextInput
          placeholder="Filtrar por año..."
          leftSection={<IconSearch size={16} />}
          value={searchYear}
          onChange={(e) => setSearchYear(e.currentTarget.value)}
          onKeyDown={handleSearchKeyDown}
          style={{ flex: 1 }}
        />
        <Button variant="light" onClick={handleSearch}>
          Buscar
        </Button>
        <ActionIcon
          variant="subtle"
          onClick={() => {
            setSearchYear('');
            setYearFilter(undefined);
            setPage(1);
          }}
          title="Limpiar filtro"
        >
          <IconRefresh size={18} />
        </ActionIcon>
      </Group>

      {error && (
        <Alert color="red" mb="md">
          {error}
        </Alert>
      )}

      {recentlyArchived && (
        <Alert
          color="gray"
          mb="md"
          withCloseButton
          icon={<IconArchive size={16} />}
          onClose={() => setRecentlyArchived(null)}
        >
          <Group justify="space-between" gap={0}>
            <Text size="sm">
              <Text span fw={600}>{recentlyArchived}</Text> ha sido archivada.
            </Text>
            <Button
              size="compact-xs"
              variant="subtle"
              color="gray"
              onClick={() => navigate(Routes.ArchivedLeagues)}
            >
              Ver histórico
            </Button>
          </Group>
        </Alert>
      )}

      {loading ? (
        <Center py="xl">
          <Loader size="lg" />
        </Center>
      ) : leagues.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">
          No se encontraron ligas activas o inactivas.
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
                <Table.Th>Min. puntos</Table.Th>
                <Table.Th>Descartes</Table.Th>
                <Table.Th style={{ width: 220 }}>Acciones</Table.Th>
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
                  <Table.Td>{leagueStatusBadge(l)}</Table.Td>
                  <Table.Td>
                    <Text size="sm">{l.competitionsCount}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm">{l.minPoints}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm">{l.worstResultsToDiscard}</Text>
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
                      <Tooltip label="Clasificación">
                        <ActionIcon
                          variant="subtle"
                          color="violet"
                          onClick={() => navigate(Routes.standingsFor(l.id))}
                        >
                          <IconChartBar size={18} />
                        </ActionIcon>
                      </Tooltip>
                      <Tooltip label="Editar">
                        <ActionIcon
                          variant="subtle"
                          color="blue"
                          onClick={() => setEditLeague(l)}
                        >
                          <IconEdit size={18} />
                        </ActionIcon>
                      </Tooltip>
                      {!l.isActive && (
                        <Tooltip label="Activar liga">
                          <ActionIcon
                            variant="subtle"
                            color="green"
                            onClick={() => handleActivate(l)}
                          >
                            <IconPlayerPlay size={18} />
                          </ActionIcon>
                        </Tooltip>
                      )}
                      <Tooltip label="Archivar">
                        <ActionIcon
                          variant="subtle"
                          color="gray"
                          onClick={() => setArchiveTarget(l)}
                        >
                          <IconArchive size={18} />
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
                {totalCount} liga{totalCount !== 1 ? 's' : ''} en total
              </Text>
            </Group>
          )}
        </>
      )}

      <CreateEditLeagueModal
        opened={modalOpen || editLeague !== null}
        onClose={handleModalClose}
        onSuccess={handleSuccess}
        league={editLeague}
      />

      <ConfirmationModal
        opened={archiveTarget !== null}
        title="Archivar liga"
        description={`¿Seguro que quieres archivar "${archiveTarget?.name} (${archiveTarget?.year})"? Podrás desarchivarla después desde el histórico.`}
        confirmLabel="Archivar"
        confirmColor="gray"
        isLoading={archiving}
        onConfirm={handleArchiveConfirm}
        onCancel={() => setArchiveTarget(null)}
      />
    </Container>
  );
}
