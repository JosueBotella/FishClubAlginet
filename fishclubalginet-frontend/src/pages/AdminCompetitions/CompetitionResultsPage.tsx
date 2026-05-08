import { useCallback, useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Container,
  Title,
  Table,
  Group,
  Badge,
  ActionIcon,
  Button,
  Text,
  Loader,
  Center,
  Alert,
  Modal,
  Stack,
  TextInput,
  Checkbox,
  Pagination,
  Divider,
} from '@mantine/core';
import {
  IconChevronLeft,
  IconFish,
  IconMedal,
  IconUserPlus,
  IconSearch,
} from '@tabler/icons-react';
import { notifications } from '@mantine/notifications';
import { getCompetitionResults, registerFisherman } from '../../api/competitionsApi';
import { getFishermen } from '../../api/fishermenApi';
import type { CompetitionResultDto, FishermanDto } from '../../types';

const PAGE_SIZE = 10;

export default function CompetitionResultsPage() {
  const { competitionId } = useParams<{ competitionId: string }>();
  const navigate = useNavigate();

  const [results, setResults] = useState<CompetitionResultDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Modal state
  const [registerOpen, setRegisterOpen] = useState(false);
  const [fishermen, setFishermen] = useState<FishermanDto[]>([]);
  const [fishermenTotal, setFishermenTotal] = useState(0);
  const [fishermenPage, setFishermenPage] = useState(1);
  const [search, setSearch] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [selectedIds, setSelectedIds] = useState<Set<number>>(new Set());
  const [loadingFishermen, setLoadingFishermen] = useState(false);
  const [registering, setRegistering] = useState(false);

  const alreadyRegistered = new Set(results.map((r) => r.fishermanId));
  const fishermenPages = Math.ceil(fishermenTotal / PAGE_SIZE);

  // ── Results ──────────────────────────────────────────────────────────────
  const fetchResults = useCallback(async () => {
    if (!competitionId) return;
    setLoading(true);
    setError(null);
    try {
      const data = await getCompetitionResults(competitionId);
      setResults(data);
    } catch {
      setError('Error al cargar los resultados.');
    } finally {
      setLoading(false);
    }
  }, [competitionId]);

  useEffect(() => {
    fetchResults();
  }, [fetchResults]);

  // ── Fishermen list ────────────────────────────────────────────────────────
  const fetchFishermen = useCallback(async () => {
    setLoadingFishermen(true);
    try {
      const skip = (fishermenPage - 1) * PAGE_SIZE;
      const result = await getFishermen(skip, PAGE_SIZE, search || undefined, false);
      setFishermen(result.items);
      setFishermenTotal(result.totalCount);
    } catch {
      notifications.show({ title: 'Error', message: 'No se pudieron cargar los pescadores.', color: 'red' });
    } finally {
      setLoadingFishermen(false);
    }
  }, [fishermenPage, search]);

  useEffect(() => {
    if (registerOpen) fetchFishermen();
  }, [registerOpen, fetchFishermen]);

  const openModal = () => {
    setSelectedIds(new Set());
    setSearchInput('');
    setSearch('');
    setFishermenPage(1);
    setRegisterOpen(true);
  };

  const handleSearch = () => {
    setFishermenPage(1);
    setSearch(searchInput);
  };

  const toggleSelect = (id: number) => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });
  };

  const selectablePage = fishermen.filter((f) => !alreadyRegistered.has(f.id));
  const allPageSelected =
    selectablePage.length > 0 && selectablePage.every((f) => selectedIds.has(f.id));

  const toggleSelectAll = () => {
    setSelectedIds((prev) => {
      const next = new Set(prev);
      if (allPageSelected) {
        selectablePage.forEach((f) => next.delete(f.id));
      } else {
        selectablePage.forEach((f) => next.add(f.id));
      }
      return next;
    });
  };

  const handleRegister = async () => {
    if (!competitionId || selectedIds.size === 0) return;
    setRegistering(true);
    let ok = 0;
    let fail = 0;
    for (const id of selectedIds) {
      try {
        await registerFisherman(competitionId, id);
        ok++;
      } catch {
        fail++;
      }
    }
    setRegistering(false);
    if (ok > 0) {
      notifications.show({
        title: `${ok} pescador${ok > 1 ? 'es' : ''} inscrito${ok > 1 ? 's' : ''}`,
        message: fail > 0 ? `${fail} no pudieron inscribirse (ya inscritos o concurso cerrado).` : '',
        color: ok > 0 ? 'green' : 'red',
      });
    } else {
      notifications.show({ title: 'Error', message: 'Ningún pescador pudo inscribirse.', color: 'red' });
    }
    setRegisterOpen(false);
    fetchResults();
  };

  // ── Ranking badge ─────────────────────────────────────────────────────────
  const rankBadge = (ranking: number) => {
    if (ranking === 1) return <Badge color="yellow" variant="filled">1</Badge>;
    if (ranking === 2) return <Badge color="gray" variant="filled">2</Badge>;
    if (ranking === 3) return <Badge color="orange" variant="filled">3</Badge>;
    return <Text size="sm">{ranking}</Text>;
  };

  return (
    <Container size="lg" py="md">
      <Group mb="md">
        <ActionIcon variant="subtle" onClick={() => navigate(-1)} title="Volver">
          <IconChevronLeft size={20} />
        </ActionIcon>
        <Title order={3}>
          <Group gap={6} component="span">
            <IconMedal size={22} />
            Resultados del concurso
          </Group>
        </Title>
        <Button leftSection={<IconUserPlus size={18} />} ml="auto" variant="light" onClick={openModal}>
          Inscribir pescadores
        </Button>
      </Group>

      {error && <Alert color="red" mb="md">{error}</Alert>}

      {loading ? (
        <Center py="xl"><Loader size="lg" /></Center>
      ) : results.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">No hay inscripciones en este concurso.</Text>
      ) : (
        <Table striped highlightOnHover withTableBorder>
          <Table.Thead>
            <Table.Tr>
              <Table.Th style={{ width: 70 }}>Posición</Table.Th>
              <Table.Th>Pescador</Table.Th>
              <Table.Th>Puesto sorteo</Table.Th>
              <Table.Th>Asistió</Table.Th>
              <Table.Th>Peso total (g)</Table.Th>
              <Table.Th>Mayor captura (g)</Table.Th>
              <Table.Th>Puntos</Table.Th>
              <Table.Th>Validado</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {results.map((r) => (
              <Table.Tr key={r.id}>
                <Table.Td>{rankBadge(r.ranking)}</Table.Td>
                <Table.Td>
                  <Group gap={4}>
                    <IconFish size={14} />
                    <Text size="sm">#{r.fishermanId}</Text>
                  </Group>
                </Table.Td>
                <Table.Td><Text size="sm">{r.assignedSpotNumber ?? '—'}</Text></Table.Td>
                <Table.Td>
                  <Badge color={r.didAttend ? 'green' : 'red'} variant="light">
                    {r.didAttend ? 'Sí' : 'No'}
                  </Badge>
                </Table.Td>
                <Table.Td><Text size="sm">{r.weightInGrams}</Text></Table.Td>
                <Table.Td><Text size="sm">{r.biggestCatchWeight ?? '—'}</Text></Table.Td>
                <Table.Td><Text size="sm" fw={600}>{r.points}</Text></Table.Td>
                <Table.Td>
                  <Badge color={r.isValidated ? 'green' : 'gray'} variant="light">
                    {r.isValidated ? 'Sí' : 'No'}
                  </Badge>
                </Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      )}

      {/* ── Modal inscripción masiva ── */}
      <Modal
        opened={registerOpen}
        onClose={() => setRegisterOpen(false)}
        title="Inscribir pescadores"
        centered
        size="lg"
      >
        <Stack gap="sm">
          {/* Buscador */}
          <Group>
            <TextInput
              placeholder="Buscar por nombre..."
              leftSection={<IconSearch size={16} />}
              value={searchInput}
              onChange={(e) => setSearchInput(e.currentTarget.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
              style={{ flex: 1 }}
            />
            <Button variant="light" onClick={handleSearch}>Buscar</Button>
          </Group>

          {/* Toolbar selección */}
          <Group justify="space-between">
            <Group gap="xs">
              <Button
                size="xs"
                variant="light"
                disabled={selectablePage.length === 0}
                onClick={toggleSelectAll}
              >
                {allPageSelected ? 'Deseleccionar pescadores' : 'Seleccionar pescadores'}
              </Button>
              {selectedIds.size > 0 && (
                <Button size="xs" variant="subtle" color="gray" onClick={() => setSelectedIds(new Set())}>
                  Limpiar selección
                </Button>
              )}
            </Group>
            {selectedIds.size > 0 && (
              <Text size="sm" c="blue" fw={500}>
                {selectedIds.size} seleccionado{selectedIds.size > 1 ? 's' : ''}
              </Text>
            )}
          </Group>

          <Divider />

          {/* Lista */}
          {loadingFishermen ? (
            <Center py="md"><Loader size="sm" /></Center>
          ) : fishermen.length === 0 ? (
            <Text c="dimmed" ta="center" py="md">No se encontraron pescadores.</Text>
          ) : (
            <Table highlightOnHover withRowBorders>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th style={{ width: 40 }}>
                    <Checkbox
                      checked={allPageSelected}
                      indeterminate={!allPageSelected && selectablePage.some((f) => selectedIds.has(f.id))}
                      disabled={selectablePage.length === 0}
                      onChange={toggleSelectAll}
                    />
                  </Table.Th>
                  <Table.Th>Pescador</Table.Th>
                  <Table.Th>Licencia</Table.Th>
                  <Table.Th>Ciudad</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {fishermen.map((f) => {
                  const enrolled = alreadyRegistered.has(f.id);
                  return (
                    <Table.Tr
                      key={f.id}
                      style={{ opacity: enrolled ? 0.45 : 1, cursor: enrolled ? 'default' : 'pointer' }}
                      onClick={() => !enrolled && toggleSelect(f.id)}
                    >
                      <Table.Td>
                        <Checkbox
                          checked={enrolled || selectedIds.has(f.id)}
                          disabled={enrolled}
                          onChange={() => !enrolled && toggleSelect(f.id)}
                          onClick={(e) => e.stopPropagation()}
                        />
                      </Table.Td>
                      <Table.Td>
                        <Text size="sm" fw={500}>
                          {f.firstName} {f.lastName}
                          {enrolled && (
                            <Badge size="xs" color="green" variant="light" ml={6}>Ya inscrito</Badge>
                          )}
                        </Text>
                      </Table.Td>
                      <Table.Td><Text size="xs" c="dimmed">{f.federationLicense ?? '—'}</Text></Table.Td>
                      <Table.Td><Text size="xs" c="dimmed">{f.addressCity}</Text></Table.Td>
                    </Table.Tr>
                  );
                })}
              </Table.Tbody>
            </Table>
          )}

          {/* Paginación */}
          {fishermenPages > 1 && (
            <Group justify="center">
              <Pagination
                total={fishermenPages}
                value={fishermenPage}
                onChange={setFishermenPage}
                size="sm"
              />
            </Group>
          )}

          <Divider />

          <Group justify="flex-end">
            <Button variant="default" onClick={() => setRegisterOpen(false)}>Cancelar</Button>
            <Button
              onClick={handleRegister}
              loading={registering}
              disabled={selectedIds.size === 0}
            >
              Inscribir {selectedIds.size > 0 ? `(${selectedIds.size})` : ''}
            </Button>
          </Group>
        </Stack>
      </Modal>
    </Container>
  );
}
