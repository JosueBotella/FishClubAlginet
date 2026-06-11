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
  Paper,
  Divider,
  Stack,
  ThemeIcon,
} from '@mantine/core';
import {
  IconChevronLeft,
  IconChartBar,
  IconWeight,
  IconMedal,
  IconTrophy,
  IconCrown,
  IconCalendar,
  IconAnchor,
} from '@tabler/icons-react';
import { getLeagueStandingsMatrix, getSeasonBiggestCatch } from '../../api/leaguesApi';
import type {
  LeagueStandingsMatrixDto,
  FishermanMatrixRowDto,
  CompetitionHeaderDto,
  SeasonBiggestCatchDto,
} from '../../types';

function podiumBadge(position: number) {
  if (position === 1) return <Badge color="yellow" variant="filled" size="sm">🥇 1º</Badge>;
  if (position === 2) return <Badge color="gray" variant="filled" size="sm">🥈 2º</Badge>;
  if (position === 3) return <Badge color="orange" variant="filled" size="sm">🥉 3º</Badge>;
  return <Text size="sm" c="dimmed" fw={500}>{position}º</Text>;
}

interface CompetitionAggregates {
  total: number;
  average: number;
  attendeesCount: number;
}

function getCompetitionAggregates(
  rows: FishermanMatrixRowDto[],
  cId: string,
  mode: 'weight' | 'points'
): CompetitionAggregates {
  let total = 0;
  let attendeesCount = 0;

  rows.forEach((r) => {
    const res = r.resultsPerCompetition[cId];
    if (res && res.didAttend) {
      attendeesCount++;
      total += mode === 'weight' ? res.weightInGrams : Number(res.points);
    }
  });

  const average = attendeesCount > 0 ? total / attendeesCount : 0;
  return { total, average, attendeesCount };
}

interface StandingsMatrixTableProps {
  competitions: CompetitionHeaderDto[];
  rows: FishermanMatrixRowDto[];
  mode: 'weight' | 'points';
  worstResultsToDiscard: number;
}

function StandingsMatrixTable({
  competitions,
  rows,
  mode,
  worstResultsToDiscard,
}: StandingsMatrixTableProps) {
  if (rows.length === 0) {
    return (
      <Text c="dimmed" ta="center" py="xl">
        Sin datos de clasificación detallada todavía.
      </Text>
    );
  }

  // Ancho estimado basado en el número de columnas: 320px fijos + 90px por cada concurso
  const tableMinWidth = 320 + competitions.length * 90 + (mode === 'points' ? 160 : 80);

  return (
    <Table.ScrollContainer minWidth={tableMinWidth} mt="md">
      <Table striped highlightOnHover withTableBorder withColumnBorders>
        <Table.Thead>
          <Table.Tr>
            <Table.Th style={{ width: 70, textAlign: 'center' }}>Pos.</Table.Th>
            <Table.Th style={{ width: 180 }}>Pescador</Table.Th>
            <Table.Th style={{ width: 70, textAlign: 'center' }}>Asist.</Table.Th>
            {competitions.map((c) => (
              <Tooltip
                key={c.id}
                label={
                  <Stack gap={2}>
                    <Text fw={600} size="xs">{c.name || `Concurso ${c.competitionNumber}`}</Text>
                    <Text size="xs" c="dimmed">
                      Fecha: {new Date(c.date).toLocaleDateString('es-ES')}
                    </Text>
                  </Stack>
                }
                withArrow
                position="top"
              >
                <Table.Th style={{ textAlign: 'center', minWidth: 80, cursor: 'help' }}>
                  {`C${c.competitionNumber}`}
                </Table.Th>
              </Tooltip>
            ))}
            <Table.Th style={{ textAlign: 'center', width: 90 }}>Total</Table.Th>
            {mode === 'points' && (
              <Table.Th style={{ textAlign: 'center', width: 90 }}>Final</Table.Th>
            )}
          </Table.Tr>
        </Table.Thead>
        <Table.Tbody>
          {rows.map((r, idx) => (
            <Table.Tr key={r.fishermanId}>
              <Table.Td style={{ textAlign: 'center' }}>{podiumBadge(idx + 1)}</Table.Td>
              <Table.Td>
                <Text size="sm" fw={500}>
                  {r.fullName}
                </Text>
              </Table.Td>
              <Table.Td style={{ textAlign: 'center' }}>
                <Text size="sm">{r.competitionsAttended}</Text>
              </Table.Td>
              {competitions.map((c) => {
                const res = r.resultsPerCompetition[c.id];
                if (!res || !res.didAttend) {
                  return (
                    <Table.Td key={c.id} style={{ textAlign: 'center' }}>
                      <Text size="xs" c="gray.4">-</Text>
                    </Table.Td>
                  );
                }

                const valueStr =
                  mode === 'weight'
                    ? res.weightInGrams.toLocaleString('es-ES')
                    : Number(res.points).toFixed(1);

                if (mode === 'points' && res.isDiscarded) {
                  return (
                    <Tooltip
                      key={c.id}
                      label={`Resultado de ${valueStr} pts descartado por ser uno de los ${worstResultsToDiscard} peores`}
                      withArrow
                      position="top"
                    >
                      <Table.Td style={{ textAlign: 'center', cursor: 'help' }}>
                        <Text
                          size="sm"
                          c="dimmed"
                          style={{ textDecoration: 'line-through' }}
                        >
                          {valueStr}
                        </Text>
                      </Table.Td>
                    </Tooltip>
                  );
                }

                return (
                  <Table.Td key={c.id} style={{ textAlign: 'center' }}>
                    <Text size="sm" fw={res.ranking <= 3 ? 600 : 400}>
                      {valueStr}
                    </Text>
                  </Table.Td>
                );
              })}
              <Table.Td style={{ textAlign: 'center' }}>
                <Text size="sm" fw={mode === 'weight' ? 600 : 400}>
                  {mode === 'weight'
                    ? `${r.totalWeightGrams.toLocaleString('es-ES')}`
                    : Number(r.totalPoints).toFixed(1)}
                </Text>
              </Table.Td>
              {mode === 'points' && (
                <Table.Td style={{ textAlign: 'center' }}>
                  <Text size="sm" fw={600} c="blue">
                    {Number(r.pointsAfterDiscard).toFixed(1)}
                  </Text>
                </Table.Td>
              )}
            </Table.Tr>
          ))}
        </Table.Tbody>
        <Table.Tfoot>
          {/* Fila de Asistentes */}
          <Table.Tr bg="var(--mantine-color-gray-0)">
            <Table.Td colSpan={3}>
              <Text size="xs" fw={700} c="dimmed" ta="right">
                Asistentes
              </Text>
            </Table.Td>
            {competitions.map((c) => {
              const aggs = getCompetitionAggregates(rows, c.id, mode);
              return (
                <Table.Td key={c.id} style={{ textAlign: 'center' }}>
                  <Text size="xs" fw={700}>
                    {aggs.attendeesCount}
                  </Text>
                </Table.Td>
              );
            })}
            <Table.Td />
            {mode === 'points' && <Table.Td />}
          </Table.Tr>

          {/* Fila de Totales */}
          <Table.Tr bg="var(--mantine-color-gray-0)">
            <Table.Td colSpan={3}>
              <Text size="xs" fw={700} c="dimmed" ta="right">
                {mode === 'weight' ? 'Peso Total (g)' : 'Puntos Totales'}
              </Text>
            </Table.Td>
            {competitions.map((c) => {
              const aggs = getCompetitionAggregates(rows, c.id, mode);
              return (
                <Table.Td key={c.id} style={{ textAlign: 'center' }}>
                  <Text size="xs" fw={700}>
                    {mode === 'weight'
                      ? aggs.total.toLocaleString('es-ES')
                      : aggs.total.toFixed(1)}
                  </Text>
                </Table.Td>
              );
            })}
            <Table.Td style={{ textAlign: 'center' }}>
              <Text size="xs" fw={700} c="teal">
                {mode === 'weight'
                  ? rows.reduce((acc, r) => acc + r.totalWeightGrams, 0).toLocaleString('es-ES')
                  : rows.reduce((acc, r) => acc + Number(r.totalPoints), 0).toFixed(1)}
              </Text>
            </Table.Td>
            {mode === 'points' && (
              <Table.Td style={{ textAlign: 'center' }}>
                <Text size="xs" fw={700} c="teal">
                  {rows.reduce((acc, r) => acc + Number(r.pointsAfterDiscard), 0).toFixed(1)}
                </Text>
              </Table.Td>
            )}
          </Table.Tr>

          {/* Fila de Promedios */}
          <Table.Tr bg="var(--mantine-color-gray-0)">
            <Table.Td colSpan={3}>
              <Text size="xs" fw={700} c="dimmed" ta="right">
                Promedio
              </Text>
            </Table.Td>
            {competitions.map((c) => {
              const aggs = getCompetitionAggregates(rows, c.id, mode);
              return (
                <Table.Td key={c.id} style={{ textAlign: 'center' }}>
                  <Text size="xs" fw={700}>
                    {mode === 'weight'
                      ? Math.round(aggs.average).toLocaleString('es-ES')
                      : aggs.average.toFixed(1)}
                  </Text>
                </Table.Td>
              );
            })}
            <Table.Td />
            {mode === 'points' && <Table.Td />}
          </Table.Tr>
        </Table.Tfoot>
      </Table>
    </Table.ScrollContainer>
  );
}

interface SeasonBiggestCatchViewProps {
  biggestCatch: SeasonBiggestCatchDto | null;
}

function SeasonBiggestCatchView({ biggestCatch }: SeasonBiggestCatchViewProps) {
  if (!biggestCatch) {
    return (
      <Alert color="blue" variant="light" mt="lg" icon={<IconAnchor size={20} />}>
        Aún no se ha registrado ninguna captura que califique como pieza mayor en esta temporada.
      </Alert>
    );
  }

  return (
    <Container size="sm" py="xl">
      <Paper
        withBorder
        shadow="md"
        radius="lg"
        p="xl"
        style={{
          background: 'linear-gradient(135deg, #1e293b 0%, #0f172a 100%)',
          color: '#f8fafc',
          position: 'relative',
          overflow: 'hidden',
        }}
      >
        <div
          style={{
            position: 'absolute',
            top: -40,
            right: -40,
            width: 150,
            height: 150,
            borderRadius: '50%',
            background: 'radial-gradient(circle, rgba(245,158,11,0.2) 0%, rgba(245,158,11,0) 70%)',
            pointerEvents: 'none',
          }}
        />

        <Stack gap="xl" align="center" ta="center">
          <ThemeIcon
            size={64}
            radius="xl"
            color="yellow"
            variant="gradient"
            gradient={{ from: 'orange', to: 'yellow' }}
          >
            <IconCrown size={36} />
          </ThemeIcon>

          <Stack gap={4}>
            <Text size="xs" fw={700} c="yellow" lts={1.5} tt="uppercase">
              Trofeo Pieza Mayor
            </Text>
            <Title order={2} c="white">
              {biggestCatch.fishermanName}
            </Title>
          </Stack>

          <Paper bg="rgba(255,255,255,0.05)" radius="md" p="md" w="100%">
            <Stack gap={2}>
              <Text size="xs" c="gray.4">captura récord de la liga</Text>
              <Text fw={800} c="yellow.4" style={{ fontSize: '2.5rem', lineHeight: 1.2 }}>
                {biggestCatch.weightInGrams.toLocaleString('es-ES')} g
              </Text>
            </Stack>
          </Paper>

          <Divider color="gray.8" w="100%" />

          <Group justify="space-around" w="100%">
            <Stack gap={2} ta="left">
              <Group gap={6}>
                <IconAnchor size={14} color="yellow" />
                <Text size="xs" c="gray.4" fw={600}>
                  Escenario
                </Text>
              </Group>
              <Text size="sm" fw={600} style={{ color: 'white' }}>
                {biggestCatch.competitionName}
              </Text>
              <Text size="xs" c="dimmed">
                Concurso {biggestCatch.competitionNumber}
              </Text>
            </Stack>

            <Stack gap={2} ta="left">
              <Group gap={6}>
                <IconCalendar size={14} color="yellow" />
                <Text size="xs" c="gray.4" fw={600}>
                  Fecha
                </Text>
              </Group>
              <Text size="sm" fw={600} style={{ color: 'white' }}>
                {new Date(biggestCatch.competitionDate).toLocaleDateString('es-ES', {
                  day: 'numeric',
                  month: 'long',
                  year: 'numeric',
                })}
              </Text>
            </Stack>
          </Group>
        </Stack>
      </Paper>
    </Container>
  );
}

export default function LeagueStandingsPage() {
  const { leagueId } = useParams<{ leagueId: string }>();
  const navigate = useNavigate();

  const [matrix, setMatrix] = useState<LeagueStandingsMatrixDto | null>(null);
  const [biggestCatch, setBiggestCatch] = useState<SeasonBiggestCatchDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchStandingsData = useCallback(async () => {
    if (!leagueId) return;
    setLoading(true);
    setError(null);
    try {
      const [matrixData, biggestCatchData] = await Promise.all([
        getLeagueStandingsMatrix(leagueId),
        getSeasonBiggestCatch(leagueId),
      ]);
      setMatrix(matrixData);
      setBiggestCatch(biggestCatchData);
    } catch {
      setError('Error al cargar la clasificación detallada de la liga.');
    } finally {
      setLoading(false);
    }
  }, [leagueId]);

  useEffect(() => {
    fetchStandingsData();
  }, [fetchStandingsData]);

  return (
    <Container size="lg" py="md">
      <Group mb="md">
        <ActionIcon variant="subtle" onClick={() => navigate(-1)} title="Volver">
          <IconChevronLeft size={20} />
        </ActionIcon>
        <Title order={3}>
          <Group gap={6} component="span">
            <IconChartBar size={22} />
            Clasificación Detallada
            {matrix && (
              <Text span c="dimmed" fw={400} size="lg">
                — {matrix.leagueName} {matrix.year}
              </Text>
            )}
          </Group>
        </Title>
      </Group>

      {matrix && matrix.worstResultsToDiscard > 0 && (
        <Alert color="blue" variant="light" mb="md" icon={<IconTrophy size={18} />}>
          Esta liga descarta los <strong>{matrix.worstResultsToDiscard}</strong> peores
          resultado{matrix.worstResultsToDiscard > 1 ? 's' : ''} de cada pescador para la
          clasificación final por puntos.
        </Alert>
      )}

      {error && <Alert color="red" mb="md">{error}</Alert>}

      {loading ? (
        <Center py="xl">
          <Loader size="lg" />
        </Center>
      ) : matrix ? (
        <Tabs defaultValue="points">
          <Tabs.List>
            <Tabs.Tab value="points" leftSection={<IconMedal size={16} />}>
              Por puntos (Matriz)
            </Tabs.Tab>
            <Tabs.Tab value="weight" leftSection={<IconWeight size={16} />}>
              Por peso (Matriz)
            </Tabs.Tab>
            <Tabs.Tab value="biggestCatch" leftSection={<IconCrown size={16} />}>
              Pieza Mayor
            </Tabs.Tab>
          </Tabs.List>

          <Tabs.Panel value="points">
            <StandingsMatrixTable
              competitions={matrix.competitions}
              rows={matrix.byPoints}
              mode="points"
              worstResultsToDiscard={matrix.worstResultsToDiscard}
            />
          </Tabs.Panel>

          <Tabs.Panel value="weight">
            <StandingsMatrixTable
              competitions={matrix.competitions}
              rows={matrix.byWeight}
              mode="weight"
              worstResultsToDiscard={matrix.worstResultsToDiscard}
            />
          </Tabs.Panel>

          <Tabs.Panel value="biggestCatch">
            <SeasonBiggestCatchView biggestCatch={biggestCatch} />
          </Tabs.Panel>
        </Tabs>
      ) : null}
    </Container>
  );
}
