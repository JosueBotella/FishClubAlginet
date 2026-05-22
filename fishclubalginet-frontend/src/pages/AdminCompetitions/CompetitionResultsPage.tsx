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
  NumberInput,
  Switch,
  Tooltip,
} from '@mantine/core';
import {
  IconChevronLeft,
  IconFish,
  IconMedal,
  IconUserPlus,
  IconSearch,
  IconTrash,
  IconPencil,
  IconInfoCircle,
} from '@tabler/icons-react';
import { notifications } from '@mantine/notifications';
import {
  getCompetitionById,
  getCompetitionResults,
  registerFisherman,
  removeRegistration,
  updateCompetitionResult,
  updateBiggestCatchConfig,
} from '../../api/competitionsApi';
import { getFishermen } from '../../api/fishermenApi';
import type { CompetitionDto, CompetitionResultDto, FishermanDto } from '../../types';

const PAGE_SIZE = 10;
const RESULTS_PAGE_SIZE = 10;

// Statuses that allow editing weight/attendance data
const EDITABLE_STATUSES: CompetitionDto['status'][] = ['Closed', 'ResultsDraft'];

interface EditState {
  resultId: string;
  fishermanId: number;
  didAttend: boolean;
  weightInGrams: number;
  biggestCatchWeight: number | null;
}

export default function CompetitionResultsPage() {
  const { competitionId } = useParams<{ competitionId: string }>();
  const navigate = useNavigate();

  // Competition metadata (needed for status guard)
  const [competition, setCompetition] = useState<CompetitionDto | null>(null);

  // Results + pagination
  const [results, setResults] = useState<CompetitionResultDto[]>([]);
  const [resultsPage, setResultsPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Delete confirmation
  const [deleteResultId, setDeleteResultId] = useState<string | null>(null);
  const [deleting, setDeleting] = useState(false);

  // Edit modal
  const [editState, setEditState] = useState<EditState | null>(null);
  const [saving, setSaving] = useState(false);

  // Biggest catch config
  const [biggestCatchMinWeight, setBiggestCatchMinWeight] = useState<number | null>(null);
  const [savingConfig, setSavingConfig] = useState(false);

  // Register modal
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

  // Paginated results slice
  const totalResultsPages = Math.ceil(results.length / RESULTS_PAGE_SIZE);
  const pagedResults = results.slice(
    (resultsPage - 1) * RESULTS_PAGE_SIZE,
    resultsPage * RESULTS_PAGE_SIZE
  );

  // Derived: can we edit weights?
  const canEditResults =
    competition !== null && EDITABLE_STATUSES.includes(competition.status);

  // ── Fetch competition + results ──────────────────────────────────────────
  const fetchAll = useCallback(async () => {
    if (!competitionId) return;
    setLoading(true);
    setError(null);
    try {
      const [comp, data] = await Promise.all([
        getCompetitionById(competitionId),
        getCompetitionResults(competitionId),
      ]);
      setCompetition(comp);
      setBiggestCatchMinWeight(comp.biggestCatchMinWeightInGrams);
      setResults(data);
    } catch {
      setError('Error al cargar los datos del concurso.');
    } finally {
      setLoading(false);
    }
  }, [competitionId]);

  useEffect(() => {
    fetchAll();
  }, [fetchAll]);

  // ── Delete ────────────────────────────────────────────────────────────────
  const handleDelete = async () => {
    if (!deleteResultId) return;
    setDeleting(true);
    try {
      await removeRegistration(deleteResultId);
      notifications.show({ title: 'Inscripción eliminada', message: '', color: 'orange' });
      setDeleteResultId(null);
      fetchAll();
    } catch {
      notifications.show({ title: 'Error', message: 'No se pudo eliminar la inscripción.', color: 'red' });
    } finally {
      setDeleting(false);
    }
  };

  // ── Biggest catch config ──────────────────────────────────────────────────
  const handleSaveBiggestCatchConfig = async () => {
    if (!competitionId) return;
    setSavingConfig(true);
    try {
      await updateBiggestCatchConfig(competitionId, { minWeightInGrams: biggestCatchMinWeight });
      notifications.show({ title: 'Configuración guardada', message: 'Mínimo pieza mayor actualizado.', color: 'green' });
    } catch {
      notifications.show({ title: 'Error', message: 'No se pudo guardar la configuración.', color: 'red' });
    } finally {
      setSavingConfig(false);
    }
  };

  // ── Edit ──────────────────────────────────────────────────────────────────
  const openEdit = (r: CompetitionResultDto) => {
    if (!canEditResults) return;
    setEditState({
      resultId: r.id,
      fishermanId: r.fishermanId,
      didAttend: r.didAttend,
      weightInGrams: r.weightInGrams,
      biggestCatchWeight: r.biggestCatchWeight ?? null,
    });
  };

  const handleSave = async () => {
    if (!editState) return;
    setSaving(true);
    try {
      await updateCompetitionResult(
        editState.resultId,
        editState.didAttend,
        editState.weightInGrams,
        editState.biggestCatchWeight
      );
      notifications.show({ title: 'Resultado actualizado', message: '', color: 'green' });
      setEditState(null);
      fetchAll();
    } catch {
      notifications.show({ title: 'Error', message: 'No se pudo actualizar el resultado.', color: 'red' });
    } finally {
      setSaving(false);
    }
  };

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
      if (allPageSelected) selectablePage.forEach((f) => next.delete(f.id));
      else selectablePage.forEach((f) => next.add(f.id));
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
        message: fail > 0 ? `${fail} no pudieron inscribirse.` : '',
        color: 'green',
      });
    } else {
      notifications.show({ title: 'Error', message: 'Ningún pescador pudo inscribirse.', color: 'red' });
    }
    setRegisterOpen(false);
    fetchAll();
  };

  // ── Ranking badge ─────────────────────────────────────────────────────────
  const rankBadge = (ranking: number) => {
    if (ranking === 1) return <Badge color="yellow" variant="filled">1</Badge>;
    if (ranking === 2) return <Badge color="gray" variant="filled">2</Badge>;
    if (ranking === 3) return <Badge color="orange" variant="filled">3</Badge>;
    return <Text size="sm">{ranking}</Text>;
  };

  const editTooltip = !canEditResults
    ? 'Solo disponible en estado Cerrado o Resultados en borrador'
    : 'Editar resultado';

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
            {competition && (
              <Badge color="gray" variant="outline" ml={4}>
                #{competition.competitionNumber}
              </Badge>
            )}
          </Group>
        </Title>
        <Button leftSection={<IconUserPlus size={18} />} ml="auto" variant="light" onClick={openModal}>
          Inscribir pescadores
        </Button>
      </Group>

      {/* Status guard informational alert */}
      {competition && !canEditResults && results.length > 0 && (
        <Alert
          icon={<IconInfoCircle size={18} />}
          color="blue"
          variant="light"
          mb="md"
        >
          La edición de pesos y asistencia solo está disponible cuando el concurso está en estado{' '}
          <strong>Cerrado</strong> o <strong>Resultados en borrador</strong>. Estado actual:{' '}
          <strong>
            {competition.status === 'Planned' && 'Planificado'}
            {competition.status === 'RegistrationOpen' && 'Inscripción abierta'}
            {competition.status === 'ResultsValidated' && 'Resultados validados'}
          </strong>
          .
        </Alert>
      )}

      {/* Biggest catch minimum weight config */}
      {competition && (
        <Group mb="md" align="flex-end" gap="xs">
          <Text size="sm" c="dimmed" fw={500}>Mínimo pieza mayor (g):</Text>
          <NumberInput
            size="xs"
            placeholder="Sin mínimo"
            min={1}
            value={biggestCatchMinWeight ?? ''}
            onChange={(v) => setBiggestCatchMinWeight(typeof v === 'number' ? v : null)}
            style={{ width: 130 }}
          />
          <Button
            size="xs"
            variant="light"
            loading={savingConfig}
            onClick={handleSaveBiggestCatchConfig}
          >
            Guardar
          </Button>
          {biggestCatchMinWeight === null && (
            <Text size="xs" c="dimmed">(sin mínimo configurado)</Text>
          )}
        </Group>
      )}

      {error && <Alert color="red" mb="md">{error}</Alert>}

      {loading ? (
        <Center py="xl"><Loader size="lg" /></Center>
      ) : results.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">No hay inscripciones en este concurso.</Text>
      ) : (
        <>
          <Table striped highlightOnHover withTableBorder>
            <Table.Thead>
              <Table.Tr>
                <Table.Th style={{ width: 70 }}>Pos.</Table.Th>
                <Table.Th>Pescador</Table.Th>
                <Table.Th>Puesto</Table.Th>
                <Table.Th>Asistió</Table.Th>
                <Table.Th>Peso (g)</Table.Th>
                <Table.Th>Mayor (g)</Table.Th>
                <Table.Th>Puntos</Table.Th>
                <Table.Th>Validado</Table.Th>
                <Table.Th style={{ width: 80 }}>Acciones</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {pagedResults.map((r) => (
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
                  <Table.Td>
                    <Group gap={4}>
                      <Tooltip label={editTooltip}>
                        <ActionIcon
                          variant="subtle"
                          color={canEditResults ? 'blue' : 'gray'}
                          title="Editar resultado"
                          disabled={!canEditResults}
                          onClick={() => openEdit(r)}
                        >
                          <IconPencil size={16} />
                        </ActionIcon>
                      </Tooltip>
                      <ActionIcon
                        variant="subtle"
                        color="red"
                        title="Desinscribir"
                        onClick={() => setDeleteResultId(r.id)}
                      >
                        <IconTrash size={16} />
                      </ActionIcon>
                    </Group>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>

          {totalResultsPages > 1 && (
            <Group justify="center" mt="md">
              <Pagination
                total={totalResultsPages}
                value={resultsPage}
                onChange={setResultsPage}
                size="sm"
              />
            </Group>
          )}
        </>
      )}

      {/* ── Modal confirmación eliminar ── */}
      <Modal
        opened={deleteResultId !== null}
        onClose={() => setDeleteResultId(null)}
        title="Confirmar desinscripción"
        centered
        size="sm"
      >
        <Stack gap="md">
          <Text size="sm">¿Seguro que quieres eliminar esta inscripción? Esta acción no se puede deshacer.</Text>
          <Group justify="flex-end">
            <Button variant="default" onClick={() => setDeleteResultId(null)}>Cancelar</Button>
            <Button color="red" loading={deleting} onClick={handleDelete}>Eliminar</Button>
          </Group>
        </Stack>
      </Modal>

      {/* ── Modal editar resultado ── */}
      <Modal
        opened={editState !== null}
        onClose={() => setEditState(null)}
        title={editState ? `Editar resultado — Pescador #${editState.fishermanId}` : ''}
        centered
        size="sm"
      >
        {editState && (
          <Stack gap="md">
            <Switch
              label="Asistió al concurso"
              checked={editState.didAttend}
              onChange={(e) =>
                setEditState((prev) => prev && { ...prev, didAttend: e.currentTarget.checked })
              }
            />
            <NumberInput
              label="Peso total (gramos)"
              min={0}
              value={editState.weightInGrams}
              onChange={(v) =>
                setEditState((prev) => prev && { ...prev, weightInGrams: typeof v === 'number' ? v : 0 })
              }
            />
            <NumberInput
              label="Mayor captura (gramos)"
              description="Déjalo vacío si no aplica"
              min={0}
              value={editState.biggestCatchWeight ?? undefined}
              onChange={(v) =>
                setEditState((prev) =>
                  prev && { ...prev, biggestCatchWeight: typeof v === 'number' ? v : null }
                )
              }
            />
            <Group justify="flex-end">
              <Button variant="default" onClick={() => setEditState(null)}>Cancelar</Button>
              <Button loading={saving} onClick={handleSave}>Guardar</Button>
            </Group>
          </Stack>
        )}
      </Modal>

      {/* ── Modal inscripción masiva ── */}
      <Modal
        opened={registerOpen}
        onClose={() => setRegisterOpen(false)}
        title="Inscribir pescadores"
        centered
        size="lg"
      >
        <Stack gap="sm">
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

          <Group justify="space-between">
            <Group gap="xs">
              <Button
                size="xs"
                variant="light"
                disabled={selectablePage.length === 0}
                onClick={toggleSelectAll}
              >
                {allPageSelected ? 'Deseleccionar página' : 'Seleccionar página'}
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
            <Button onClick={handleRegister} loading={registering} disabled={selectedIds.size === 0}>
              Inscribir {selectedIds.size > 0 ? `(${selectedIds.size})` : ''}
            </Button>
          </Group>
        </Stack>
      </Modal>
    </Container>
  );
}
