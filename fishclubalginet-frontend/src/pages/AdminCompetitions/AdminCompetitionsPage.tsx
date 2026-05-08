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
} from '@tabler/icons-react';
import { getCompetitionsByLeague } from '../../api/competitionsApi';
import type { CompetitionDto } from '../../types';
import { Routes } from '../../constants';
import CreateCompetitionModal from './CreateCompetitionModal';

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

  const handleModalSuccess = () => {
    setModalOpen(false);
    fetchCompetitions();
  };

  return (
    <Container size="lg" py="md">
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
              <Table.Th style={{ width: 80 }}>Acciones</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {competitions.map((c) => (
              <Table.Tr key={c.id}>
                <Table.Td>
                  <Text size="sm" fw={600}>
                    {c.competitionNumber}
                  </Text>
                </Table.Td>
                <Table.Td>
                  <Text size="sm">{c.name ?? '—'}</Text>
                </Table.Td>
                <Table.Td>
                  <Text size="sm">{new Date(c.date).toLocaleDateString('es-ES')}</Text>
                </Table.Td>
                <Table.Td>
                  <Text size="sm">
                    {formatTime(c.startTime)} – {formatTime(c.endTime)}
                  </Text>
                </Table.Td>
                <Table.Td>
                  <Text size="sm">
                    {c.venue} / {c.zone}
                  </Text>
                </Table.Td>
                <Table.Td>
                  <Text size="sm">
                    {c.subspecialty === 'AguaDulce' ? 'Agua dulce' : c.subspecialty} ·{' '}
                    {c.category}
                  </Text>
                </Table.Td>
                <Table.Td>
                  <Text size="sm">
                    {c.participantCount} / {c.maxSpots}
                  </Text>
                </Table.Td>
                <Table.Td>{statusBadge(c.status)}</Table.Td>
                <Table.Td>
                  <Tooltip label="Ver resultados / inscripciones">
                    <ActionIcon
                      variant="subtle"
                      color="blue"
                      onClick={() =>
                        navigate(Routes.competitionResultsFor(c.id))
                      }
                    >
                      <IconEye size={18} />
                    </ActionIcon>
                  </Tooltip>
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
    </Container>
  );
}
