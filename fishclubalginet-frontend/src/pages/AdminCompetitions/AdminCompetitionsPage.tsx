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
  Tooltip,
  Loader,
  Center,
  Alert,
} from '@mantine/core';
import {
  IconPlus,
  IconChevronLeft,
  IconEye,
  IconCalendarEvent,
  IconLockOpen,
  IconLock,
  IconLockOpenOff,
  IconListNumbers,
  IconClipboardList,
  IconCircleCheck,
} from '@tabler/icons-react';
import {
  getCompetitionsByLeague,
  openRegistration,
  closeRegistration,
  reopenRegistration,
  assignSpots,
  moveToResultsDraft,
  validateResults,
} from '../../api/competitionsApi';
import { notifications } from '@mantine/notifications';
import type { CompetitionDto } from '../../types';
import { Routes } from '../../constants';
import CreateCompetitionModal from './CreateCompetitionModal';
import ConfirmationModal from '../../components/ConfirmationModal';

type ActionType =
  | 'openRegistration'
  | 'closeRegistration'
  | 'reopenRegistration'
  | 'assignSpots'
  | 'moveToResultsDraft'
  | 'validateResults';

interface PendingAction {
  competition: CompetitionDto;
  type: ActionType;
}

const ACTION_CONFIG: Record<ActionType, { title: string; description: (c: CompetitionDto) => string; confirmLabel: string; confirmColor: string }> = {
  openRegistration: {
    title: 'Abrir inscripción',
    description: (c) => `¿Abrir inscripción para el concurso #${c.competitionNumber} — ${c.venue}?`,
    confirmLabel: 'Abrir',
    confirmColor: 'blue',
  },
  closeRegistration: {
    title: 'Cerrar inscripción',
    description: (c) => `¿Cerrar inscripción para el concurso #${c.competitionNumber}? No se podrán añadir más participantes.`,
    confirmLabel: 'Cerrar',
    confirmColor: 'orange',
  },
  reopenRegistration: {
    title: 'Reabrir inscripción',
    description: (c) => `¿Reabrir inscripción para el concurso #${c.competitionNumber}? Solo es posible dentro de los 30 días siguientes al cierre.`,
    confirmLabel: 'Reabrir',
    confirmColor: 'blue',
  },
  assignSpots: {
    title: 'Asignar pesqueras',
    description: (c) => `¿Asignar números de pesquera a los participantes del concurso #${c.competitionNumber}? Los números actuales se sobreescribirán.`,
    confirmLabel: 'Asignar',
    confirmColor: 'violet',
  },
  moveToResultsDraft: {
    title: 'Pasar a borrador de resultados',
    description: (c) => `¿Mover el concurso #${c.competitionNumber} a estado "Resultados en borrador"? Podrás seguir editando resultados.`,
    confirmLabel: 'Confirmar',
    confirmColor: 'yellow',
  },
  validateResults: {
    title: 'Validar resultados',
    description: (c) => `¿Validar definitivamente los resultados del concurso #${c.competitionNumber}? Esta acción bloqueará la edición.`,
    confirmLabel: 'Validar',
    confirmColor: 'green',
  },
};

async function executeAction(type: ActionType, competitionId: string): Promise<void> {
  switch (type) {
    case 'openRegistration':    return openRegistration(competitionId);
    case 'closeRegistration':   return closeRegistration(competitionId);
    case 'reopenRegistration':  return reopenRegistration(competitionId);
    case 'assignSpots':         return assignSpots(competitionId);
    case 'moveToResultsDraft':  return moveToResultsDraft(competitionId);
    case 'validateResults':     return validateResults(competitionId);
  }
}

function statusBadge(status: CompetitionDto['status']) {
  const map: Record<CompetitionDto['status'], { label: string; color: string }> = {
    Planned: { label: 'Planificado', color: 'gray' },
    RegistrationOpen: { label: 'Inscripción abierta', color: 'blue' },
    Closed: { label: 'Cerrado', color: 'orange' },
    ResultsDraft: { label: 'Resultados borrador', color: 'yellow' },
    ResultsValidated: { label: 'Resultados validados', color: 'green' },
  };
  const { label, color } = map[status] ?? { label: status, color: 'gray' };
  return <Badge color={color} variant="light">{label}</Badge>;
}

function formatTime(t: string) {
  return t.slice(0, 5);
}

export default function AdminCompetitionsPage() {
  const { leagueId } = useParams<{ leagueId: string }>();
  const navigate = useNavigate();

  const [competitions, setCompetitions] = useState<CompetitionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [pendingAction, setPendingAction] = useState<PendingAction | null>(null);
  const [actionLoading, setActionLoading] = useState(false);

  const fetchCompetitions = useCallback(async () => {
    if (!leagueId) return;
    setLoading(true);
    setError(null);
    try {
      const data = await getCompetitionsByLeague(leagueId);
      setCompetitions(data);
    } catch {
      setError('Error al cargar los concursos.');
    } finally {
      setLoading(false);
    }
  }, [leagueId]);

  useEffect(() => {
    fetchCompetitions();
  }, [fetchCompetitions]);

  const requestAction = (competition: CompetitionDto, type: ActionType) => {
    setPendingAction({ competition, type });
  };

  const handleConfirmAction = async () => {
    if (!pendingAction) return;
    setActionLoading(true);
    const { competition, type } = pendingAction;
    const config = ACTION_CONFIG[type];
    try {
      await executeAction(type, competition.id);
      notifications.show({
        title: config.title,
        message: `Concurso #${competition.competitionNumber} actualizado.`,
        color: config.confirmColor,
      });
      setPendingAction(null);
      fetchCompetitions();
    } catch {
      notifications.show({
        title: 'Error',
        message: `No se pudo ejecutar la acción: ${config.title.toLowerCase()}.`,
        color: 'red',
      });
    } finally {
      setActionLoading(false);
    }
  };

  const handleModalSuccess = () => {
    setModalOpen(false);
    fetchCompetitions();
  };

  const confirmConfig = pendingAction ? ACTION_CONFIG[pendingAction.type] : null;

  return (
    <Container size="xl" py="md">
      <Group mb="md">
        <ActionIcon variant="subtle" onClick={() => navigate(Routes.Leagues)} title="Volver a ligas">
          <IconChevronLeft size={20} />
        </ActionIcon>
        <Title order={3}>
          <Group gap={6} component="span">
            <IconCalendarEvent size={22} />
            Concursos de la liga
          </Group>
        </Title>
        <Button
          leftSection={<IconPlus size={18} />}
          ml="auto"
          onClick={() => setModalOpen(true)}
        >
          Nuevo concurso
        </Button>
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
      ) : competitions.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">
          No hay concursos en esta liga.
        </Text>
      ) : (
        <Table striped highlightOnHover withTableBorder>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>#</Table.Th>
              <Table.Th>Nombre</Table.Th>
              <Table.Th>Fecha</Table.Th>
              <Table.Th>Horario</Table.Th>
              <Table.Th>Lugar / Zona</Table.Th>
              <Table.Th>Modalidad</Table.Th>
              <Table.Th>Plazas</Table.Th>
              <Table.Th>Estado</Table.Th>
              <Table.Th style={{ width: 160 }}>Acciones</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {competitions.map((c) => (
              <Table.Tr key={c.id}>
                <Table.Td>
                  <Text size="sm" fw={600}>{c.competitionNumber}</Text>
                </Table.Td>
                <Table.Td>
                  <Text size="sm">{c.name ?? '—'}</Text>
                </Table.Td>
                <Table.Td>
                  <Text size="sm">{new Date(c.date).toLocaleDateString('es-ES')}</Text>
                </Table.Td>
                <Table.Td>
                  <Text size="sm">{formatTime(c.startTime)} – {formatTime(c.endTime)}</Text>
                </Table.Td>
                <Table.Td>
                  <Text size="sm">{c.venue} / {c.zone}</Text>
                </Table.Td>
                <Table.Td>
                  <Text size="sm">
                    {c.subspecialty === 'AguaDulce' ? 'Agua dulce' : c.subspecialty} · {c.category}
                  </Text>
                </Table.Td>
                <Table.Td>
                  <Text size="sm">{c.participantCount} / {c.maxSpots}</Text>
                </Table.Td>
                <Table.Td>{statusBadge(c.status)}</Table.Td>
                <Table.Td>
                  <Group gap={4}>
                    {/* Planned → RegistrationOpen */}
                    {c.status === 'Planned' && (
                      <Tooltip label="Abrir inscripción">
                        <ActionIcon variant="subtle" color="blue" onClick={() => requestAction(c, 'openRegistration')}>
                          <IconLockOpen size={18} />
                        </ActionIcon>
                      </Tooltip>
                    )}

                    {/* RegistrationOpen → Closed */}
                    {c.status === 'RegistrationOpen' && (
                      <>
                        <Tooltip label="Cerrar inscripción">
                          <ActionIcon variant="subtle" color="orange" onClick={() => requestAction(c, 'closeRegistration')}>
                            <IconLock size={18} />
                          </ActionIcon>
                        </Tooltip>
                        <Tooltip label="Asignar pesqueras">
                          <ActionIcon variant="subtle" color="violet" onClick={() => requestAction(c, 'assignSpots')}>
                            <IconListNumbers size={18} />
                          </ActionIcon>
                        </Tooltip>
                      </>
                    )}

                    {/* Closed → ReopenRegistration | ResultsDraft */}
                    {c.status === 'Closed' && (
                      <>
                        <Tooltip label="Reabrir inscripción (≤30 días)">
                          <ActionIcon variant="subtle" color="blue" onClick={() => requestAction(c, 'reopenRegistration')}>
                            <IconLockOpenOff size={18} />
                          </ActionIcon>
                        </Tooltip>
                        <Tooltip label="Pasar a borrador de resultados">
                          <ActionIcon variant="subtle" color="yellow" onClick={() => requestAction(c, 'moveToResultsDraft')}>
                            <IconClipboardList size={18} />
                          </ActionIcon>
                        </Tooltip>
                      </>
                    )}

                    {/* ResultsDraft → ResultsValidated */}
                    {c.status === 'ResultsDraft' && (
                      <Tooltip label="Validar resultados">
                        <ActionIcon variant="subtle" color="green" onClick={() => requestAction(c, 'validateResults')}>
                          <IconCircleCheck size={18} />
                        </ActionIcon>
                      </Tooltip>
                    )}

                    {/* Siempre visible: ver resultados */}
                    <Tooltip label="Ver resultados / inscripciones">
                      <ActionIcon
                        variant="subtle"
                        color="teal"
                        onClick={() => navigate(Routes.competitionResultsFor(c.id))}
                      >
                        <IconEye size={18} />
                      </ActionIcon>
                    </Tooltip>
                  </Group>
                </Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      )}

      {leagueId && (
        <CreateCompetitionModal
          opened={modalOpen}
          leagueId={leagueId}
          onClose={() => setModalOpen(false)}
          onSuccess={handleModalSuccess}
        />
      )}

      {pendingAction && confirmConfig && (
        <ConfirmationModal
          opened={pendingAction !== null}
          title={confirmConfig.title}
          description={confirmConfig.description(pendingAction.competition)}
          confirmLabel={confirmConfig.confirmLabel}
          confirmColor={confirmConfig.confirmColor}
          isLoading={actionLoading}
          onConfirm={handleConfirmAction}
          onCancel={() => setPendingAction(null)}
        />
      )}
    </Container>
  );
}
