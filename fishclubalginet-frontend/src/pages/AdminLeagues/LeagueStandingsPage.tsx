import { useCallback, useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Container,
  Title,
  Table,
  Group,
  Badge,
  ActionIcon,
  Text,
  Loader,
  Center,
  Alert,
  Tabs,
  Tooltip,
} from '@mantine/core';
import {
  IconChevronLeft,
  IconChartBar,
  IconWeight,
  IconMedal,
  IconTrophy,
} from '@tabler/icons-react';
import { getLeagueStandings } from '../../api/leaguesApi';
import type { LeagueStandingsDto, LeagueFishermanStandingDto } from '../../types';

function podiumBadge(position: number) {
  if (position === 1) return <Badge color="yellow" variant="filled" size="sm">🥇 1º</Badge>;
  if (position === 2) return <Badge color="gray" variant="filled" size="sm">🥈 2º</Badge>;
  if (position === 3) return <Badge color="orange" variant="filled" size="sm">🥉 3º</Badge>;
  return <Text size="sm" c="dimmed">{position}º</Text>;
}

interface StandingsTableProps {
  rows: LeagueFishermanStandingDto[];
  mode: 'weight' | 'points';
  worstResultsToDiscard: number;
}

function StandingsTable({ rows, mode, worstResultsToDiscard }: StandingsTableProps) {
  if (rows.length === 0) {
    return (
      <Text c="dimmed" ta="center" py="xl">
        Sin datos de clasificación todavía.
      </Text>
    );
  }

  return (
    <Table striped highlightOnHover withTableBorder mt="sm">
      <Table.Thead>
        <Table.Tr>
          <Table.Th style={{ width: 70 }}>Pos.</Table.Th>
          <Table.Th>Pescador</Table.Th>
          <Table.Th>Concursos</Table.Th>
          {mode === 'weight' ? (
            <Table.Th>Peso total (g)</Table.Th>
          ) : (
            <>
              <Table.Th>Puntos totales</Table.Th>
              <Tooltip
                label={
                  worstResultsToDiscard > 0
                    ? `Se descartan los ${worstResultsToDiscard} peores resultados`
                    : 'Sin descartes configurados'
                }
                withArrow
              >
                <Table.Th style={{ cursor: 'help' }}>
                  Puntos (con descarte){worstResultsToDiscard > 0 ? ' ⓘ' : ''}
                </Table.Th>
              </Tooltip>
            </>
          )}
          <Table.Th>Peso total (g)</Table.Th>
        </Table.Tr>
      </Table.Thead>
      <Table.Tbody>
        {rows.map((r, idx) => (
          <Table.Tr key={r.fishermanId}>
            <Table.Td>{podiumBadge(idx + 1)}</Table.Td>
            <Table.Td>
              <Text size="sm" fw={500}>{r.fullName}</Text>
            </Table.Td>
            <Table.Td>
              <Text size="sm">{r.competitionsAttended}</Text>
            </Table.Td>
            {mode === 'weight' ? (
              <Table.Td>
                <Text size="sm" fw={600}>{r.totalWeightGrams.toLocaleString('es-ES')} g</Text>
              </Table.Td>
            ) : (
              <>
                <Table.Td>
                  <Text size="sm">{r.totalPoints}</Text>
                </Table.Td>
                <Table.Td>
                  <Text size="sm" fw={600}>{r.pointsAfterDiscard}</Text>
                </Table.Td>
              </>
            )}
            <Table.Td>
              <Text size="sm" c="dimmed">{r.totalWeightGrams.toLocaleString('es-ES')} g</Text>
            </Table.Td>
          </Table.Tr>
        ))}
      </Table.Tbody>
    </Table>
  );
}

export default function LeagueStandingsPage() {
  const { leagueId } = useParams<{ leagueId: string }>();
  const navigate = useNavigate();

  const [standings, setStandings] = useState<LeagueStandingsDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchStandings = useCallback(async () => {
    if (!leagueId) return;
    setLoading(true);
    setError(null);
    try {
      const data = await getLeagueStandings(leagueId);
      setStandings(data);
    } catch {
      setError('Error al cargar la clasificación.');
    } finally {
      setLoading(false);
    }
  }, [leagueId]);

  useEffect(() => {
    fetchStandings();
  }, [fetchStandings]);

  return (
    <Container size="lg" py="md">
      <Group mb="md">
        <ActionIcon variant="subtle" onClick={() => navigate(-1)} title="Volver">
          <IconChevronLeft size={20} />
        </ActionIcon>
        <Title order={3}>
          <Group gap={6} component="span">
            <IconChartBar size={22} />
            Clasificación
            {standings && (
              <Text span c="dimmed" fw={400} size="lg">
                — {standings.leagueName} {standings.year}
              </Text>
            )}
          </Group>
        </Title>
      </Group>

      {standings && standings.worstResultsToDiscard > 0 && (
        <Alert color="blue" variant="light" mb="md" icon={<IconTrophy size={18} />}>
          Esta liga descarta los <strong>{standings.worstResultsToDiscard}</strong> peores
          resultado{standings.worstResultsToDiscard > 1 ? 's' : ''} de cada pescador para la
          clasificación por puntos.
        </Alert>
      )}

      {error && <Alert color="red" mb="md">{error}</Alert>}

      {loading ? (
        <Center py="xl"><Loader size="lg" /></Center>
      ) : standings ? (
        <Tabs defaultValue="points">
          <Tabs.List>
            <Tabs.Tab value="points" leftSection={<IconMedal size={16} />}>
              Por puntos
            </Tabs.Tab>
            <Tabs.Tab value="weight" leftSection={<IconWeight size={16} />}>
              Por peso
            </Tabs.Tab>
          </Tabs.List>

          <Tabs.Panel value="points">
            <StandingsTable
              rows={standings.byPoints}
              mode="points"
              worstResultsToDiscard={standings.worstResultsToDiscard}
            />
          </Tabs.Panel>

          <Tabs.Panel value="weight">
            <StandingsTable
              rows={standings.byWeight}
              mode="weight"
              worstResultsToDiscard={standings.worstResultsToDiscard}
            />
          </Tabs.Panel>
        </Tabs>
      ) : null}
    </Container>
  );
}
