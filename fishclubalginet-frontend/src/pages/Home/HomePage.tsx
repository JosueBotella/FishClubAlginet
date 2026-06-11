import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import {
  Container,
  Title,
  Text,
  SimpleGrid,
  Paper,
  Group,
  Badge,
  Loader,
  Center,
  Stack,
  ThemeIcon,
  Anchor,
  Divider,
} from '@mantine/core';
import { IconMedal, IconWeight, IconCrown, IconCalendar } from '@tabler/icons-react';
import { useAuth } from '../../hooks';
import {
  getActiveLeague,
  getLeagueStandings,
  getSeasonBiggestCatch,
} from '../../api/leaguesApi';
import type {
  LeagueDto,
  LeagueStandingsDto,
  LeagueFishermanStandingDto,
  SeasonBiggestCatchDto,
} from '../../types';

function podiumBadge(position: number) {
  if (position === 1) return <Badge color="yellow" variant="filled" size="sm">🥇 1º</Badge>;
  if (position === 2) return <Badge color="gray" variant="filled" size="sm">🥈 2º</Badge>;
  return <Badge color="orange" variant="filled" size="sm">🥉 3º</Badge>;
}

interface Top3CardProps {
  title: string;
  icon: React.ReactNode;
  color: string;
  rows: LeagueFishermanStandingDto[];
  renderValue: (row: LeagueFishermanStandingDto) => string;
}

function Top3Card({ title, icon, color, rows, renderValue }: Top3CardProps) {
  return (
    <Paper withBorder p="md" radius="md">
      <Group gap="xs" mb="sm">
        <ThemeIcon variant="light" color={color} size="md">
          {icon}
        </ThemeIcon>
        <Text fw={600} size="sm">
          {title}
        </Text>
      </Group>
      {rows.length === 0 ? (
        <Text size="sm" c="dimmed">
          Sin resultados todavía.
        </Text>
      ) : (
        <Stack gap="xs">
          {rows.slice(0, 3).map((r, idx) => (
            <Group key={r.fishermanId} justify="space-between" wrap="nowrap">
              <Group gap="xs" wrap="nowrap">
                {podiumBadge(idx + 1)}
                <Text size="sm" fw={500} lineClamp={1}>
                  {r.fullName}
                </Text>
              </Group>
              <Text size="sm" fw={600} c={color}>
                {renderValue(r)}
              </Text>
            </Group>
          ))}
        </Stack>
      )}
    </Paper>
  );
}

interface BiggestCatchCardProps {
  biggestCatch: SeasonBiggestCatchDto | null;
}

function BiggestCatchCard({ biggestCatch }: BiggestCatchCardProps) {
  return (
    <Paper withBorder p="md" radius="md">
      <Group gap="xs" mb="sm">
        <ThemeIcon variant="light" color="grape" size="md">
          <IconCrown size={16} />
        </ThemeIcon>
        <Text fw={600} size="sm">
          Pieza Mayor del Año
        </Text>
      </Group>
      {!biggestCatch ? (
        <Text size="sm" c="dimmed">
          Sin pieza mayor registrada todavía.
        </Text>
      ) : (
        <Stack gap={4}>
          <Text size="lg" fw={700}>
            {biggestCatch.fishermanName}
          </Text>
          <Text size="xl" fw={700} c="grape">
            {(biggestCatch.weightInGrams / 1000).toLocaleString('es-ES', {
              minimumFractionDigits: 3,
            })}{' '}
            kg
          </Text>
          <Group gap={6} mt={2}>
            <IconCalendar size={14} />
            <Text size="xs" c="dimmed">
              {biggestCatch.competitionName || `Concurso ${biggestCatch.competitionNumber}`}
              {' · '}
              {new Date(biggestCatch.competitionDate).toLocaleDateString('es-ES')}
            </Text>
          </Group>
        </Stack>
      )}
    </Paper>
  );
}

export default function HomePage() {
  const { user } = useAuth();
  const [loading, setLoading] = useState(true);
  const [league, setLeague] = useState<LeagueDto | null>(null);
  const [standings, setStandings] = useState<LeagueStandingsDto | null>(null);
  const [biggestCatch, setBiggestCatch] = useState<SeasonBiggestCatchDto | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadSummary() {
      try {
        const activeLeague = await getActiveLeague();
        if (cancelled || !activeLeague) return;
        setLeague(activeLeague);

        const [standingsResult, biggestCatchResult] = await Promise.allSettled([
          getLeagueStandings(activeLeague.id),
          getSeasonBiggestCatch(activeLeague.id),
        ]);
        if (cancelled) return;
        if (standingsResult.status === 'fulfilled') setStandings(standingsResult.value);
        if (biggestCatchResult.status === 'fulfilled') setBiggestCatch(biggestCatchResult.value);
      } catch {
        // Silencioso: la home no debe romperse si falla el resumen
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    loadSummary();
    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <Container py="xl">
      <Title order={2}>Bienvenido, {user?.email}</Title>
      <Text c="dimmed" size="sm" mt="xs">
        Roles: {user?.roles.join(', ') || 'Ninguno'}
      </Text>

      <Divider my="lg" />

      {loading ? (
        <Center py="xl">
          <Loader size="sm" />
        </Center>
      ) : !league ? (
        <Text size="sm" c="dimmed">
          No hay ninguna liga activa en este momento.
        </Text>
      ) : (
        <>
          <Group justify="space-between" mb="md">
            <Title order={4}>Resumen de {league.name}</Title>
            <Anchor
              component={Link}
              to={`/admin/leagues/${league.id}/standings`}
              size="sm"
            >
              Ver clasificación completa →
            </Anchor>
          </Group>
          <SimpleGrid cols={{ base: 1, sm: 3 }} spacing="md">
            <Top3Card
              title="Top 3 — Puntos"
              icon={<IconMedal size={16} />}
              color="blue"
              rows={standings?.byPoints ?? []}
              renderValue={(r) => `${Number(r.pointsAfterDiscard).toFixed(1)} pts`}
            />
            <Top3Card
              title="Top 3 — Peso"
              icon={<IconWeight size={16} />}
              color="teal"
              rows={standings?.byWeight ?? []}
              renderValue={(r) =>
                `${(r.totalWeightGrams / 1000).toLocaleString('es-ES', {
                  minimumFractionDigits: 3,
                })} kg`
              }
            />
            <BiggestCatchCard biggestCatch={biggestCatch} />
          </SimpleGrid>
        </>
      )}
    </Container>
  );
}
