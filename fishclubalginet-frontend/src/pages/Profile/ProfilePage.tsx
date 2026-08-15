import { useEffect, useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Container,
  Title,
  Paper,
  Stack,
  Group,
  Text,
  Badge,
  Divider,
  TextInput,
  PasswordInput,
  Button,
  Alert,
  Loader,
  Center,
  SimpleGrid,
  Tabs,
  Table,
  Card,
  Progress,
  ActionIcon,
  Tooltip,
} from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import {
  IconUser,
  IconKey,
  IconAddressBook,
  IconTrophy,
  IconCalendarPlus,
  IconEye,
  IconMapPin,
  IconClock,
  IconCheck,
  IconAlertCircle,
  IconTrash,
} from '@tabler/icons-react';
import { useAuth } from '../../hooks';
import { getMyProfile } from '../../api/fishermenApi';
import { authApi } from '../../api/authApi';
import {
  getMyRegistrations,
  getCompetitionsByLeague,
  registerFisherman,
  removeRegistration,
} from '../../api/competitionsApi';
import { leaguesApi } from '../../api';
import { Routes } from '../../constants';
import type {
  FishermanProfileDto,
  MyCompetitionRegistrationDto,
  CompetitionDto,
  LeagueDto,
} from '../../types';

export default function ProfilePage() {
  const { user } = useAuth();
  const navigate = useNavigate();

  const [activeTab, setActiveTab] = useState<string | null>('profile');

  // Profile data
  const [profile, setProfile] = useState<FishermanProfileDto | null>(null);
  const [loadingProfile, setLoadingProfile] = useState(true);
  const [profileError, setProfileError] = useState<string | null>(null);

  // My competitions data
  const [myRegistrations, setMyRegistrations] = useState<MyCompetitionRegistrationDto[]>([]);
  const [loadingRegistrations, setLoadingRegistrations] = useState(false);
  const [registrationsError, setRegistrationsError] = useState<string | null>(null);

  // Open competitions for self-registration
  const [openCompetitions, setOpenCompetitions] = useState<CompetitionDto[]>([]);
  const [loadingOpenCompetitions, setLoadingOpenCompetitions] = useState(false);
  const [registeringId, setRegisteringId] = useState<string | null>(null);
  const [unregisteringId, setUnregisteringId] = useState<string | null>(null);

  // Password change state
  const [changing, setChanging] = useState(false);
  const [pwdError, setPwdError] = useState<string | null>(null);

  const pwdForm = useForm({
    initialValues: {
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    },
    validate: {
      currentPassword: (v) =>
        v.length === 0 ? 'La contraseña actual es obligatoria' : null,
      newPassword: (v) =>
        v.length >= 6 ? null : 'Mínimo 6 caracteres',
      confirmPassword: (v, values) =>
        v === values.newPassword ? null : 'Las contraseñas no coinciden',
    },
  });

  // Load Profile
  const loadProfile = useCallback(async () => {
    setLoadingProfile(true);
    setProfileError(null);
    try {
      const data = await getMyProfile();
      setProfile(data);
    } catch {
      setProfileError('No se pudieron cargar los datos del pescador.');
    } finally {
      setLoadingProfile(false);
    }
  }, []);

  // Load Registrations
  const loadRegistrations = useCallback(async () => {
    setLoadingRegistrations(true);
    setRegistrationsError(null);
    try {
      const data = await getMyRegistrations();
      setMyRegistrations(data);
    } catch {
      setRegistrationsError('No se pudieron cargar tus inscripciones a concursos.');
    } finally {
      setLoadingRegistrations(false);
    }
  }, []);

  // Load Open Competitions
  const loadOpenCompetitions = useCallback(async () => {
    setLoadingOpenCompetitions(true);
    try {
      const leagues = await leaguesApi.getAllLeagues({ archived: false });
      const activeLeagues = leagues.filter((l: LeagueDto) => l.isActive);
      
      const compPromises = activeLeagues.map((l: LeagueDto) =>
        getCompetitionsByLeague(l.id).catch(() => [])
      );
      const results = await Promise.all(compPromises);
      const allComps = results.flat();
      const openOnes = allComps.filter((c) => c.status === 'RegistrationOpen');
      setOpenCompetitions(openOnes);
    } catch {
      // silently handle or fallback
    } finally {
      setLoadingOpenCompetitions(false);
    }
  }, []);

  useEffect(() => {
    loadProfile();
    loadRegistrations();
    loadOpenCompetitions();
  }, [loadProfile, loadRegistrations, loadOpenCompetitions]);

  const handleChangePassword = async (values: typeof pwdForm.values) => {
    setChanging(true);
    setPwdError(null);
    try {
      await authApi.changePassword({
        currentPassword: values.currentPassword,
        newPassword: values.newPassword,
      });
      notifications.show({
        title: 'Contraseña actualizada',
        message: 'Tu contraseña se ha cambiado correctamente.',
        color: 'green',
      });
      pwdForm.reset();
    } catch (err: unknown) {
      const msg =
        err instanceof Error
          ? err.message
          : 'No se pudo cambiar la contraseña. Verifica la actual.';
      setPwdError(msg);
    } finally {
      setChanging(false);
    }
  };

  const handleSelfRegister = async (competitionId: string) => {
    if (!profile) {
      notifications.show({
        title: 'Ficha requerida',
        message: 'Debes tener una ficha de pescador activa para inscribirte.',
        color: 'yellow',
      });
      return;
    }

    setRegisteringId(competitionId);
    try {
      await registerFisherman(competitionId, profile.id);
      notifications.show({
        title: 'Inscripción realizada',
        message: '¡Te has inscrito correctamente en el concurso!',
        color: 'green',
      });
      await Promise.all([loadRegistrations(), loadOpenCompetitions()]);
    } catch (err: unknown) {
      const msg =
        err instanceof Error ? err.message : 'No se pudo completar la inscripción.';
      notifications.show({
        title: 'Error de inscripción',
        message: msg,
        color: 'red',
      });
    } finally {
      setRegisteringId(null);
    }
  };

  const handleSelfUnregister = async (resultId: string) => {
    setUnregisteringId(resultId);
    try {
      await removeRegistration(resultId);
      notifications.show({
        title: 'Inscripción cancelada',
        message: 'Has cancelado tu inscripción en el concurso.',
        color: 'blue',
      });
      await Promise.all([loadRegistrations(), loadOpenCompetitions()]);
    } catch (err: unknown) {
      const msg =
        err instanceof Error ? err.message : 'No se pudo cancelar la inscripción.';
      notifications.show({
        title: 'Error',
        message: msg,
        color: 'red',
      });
    } finally {
      setUnregisteringId(null);
    }
  };

  const formatDate = (iso: string) => {
    const d = new Date(iso);
    return d.toLocaleDateString('es-ES', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  };

  const formatAddress = (p: FishermanProfileDto) => {
    const line1 = [p.street, p.number, p.floorDoor].filter(Boolean).join(' ');
    const line2 = [p.zipCode, p.city, p.province].filter(Boolean).join(' ');
    return [line1, line2].filter(Boolean).join(', ');
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'RegistrationOpen':
        return <Badge color="teal">Inscripción Abierta</Badge>;
      case 'Closed':
        return <Badge color="gray">Inscripción Cerrada</Badge>;
      case 'ResultsDraft':
        return <Badge color="yellow">Borrador de Pesaje</Badge>;
      case 'ResultsValidated':
        return <Badge color="blue">Resultados Validados</Badge>;
      default:
        return <Badge color="dark">Planificado</Badge>;
    }
  };

  const getRankingBadge = (ranking: number | null) => {
    if (!ranking) return <Text size="sm" c="dimmed">-</Text>;
    if (ranking === 1) return <Badge color="yellow" size="lg">🥇 1º Puesto</Badge>;
    if (ranking === 2) return <Badge color="gray" size="lg">🥈 2º Puesto</Badge>;
    if (ranking === 3) return <Badge color="orange" size="lg">🥉 3º Puesto</Badge>;
    return <Badge color="blue" variant="light">{ranking}º</Badge>;
  };

  return (
    <Container size="lg" py="md">
      <Group justify="space-between" mb="lg">
        <div>
          <Title order={2}>
            {profile ? `${profile.firstName} ${profile.lastName}` : 'Mi Área de Socio'}
          </Title>
          <Text c="dimmed" size="sm">
            Consulta tu ficha federativa, historial de concursos y gestiona tus inscripciones
          </Text>
        </div>
        <Group gap={6}>
          {user?.roles.map((role) => (
            <Badge key={role} size="md" color={role === 'Admin' ? 'blue' : 'teal'}>
              {role}
            </Badge>
          ))}
        </Group>
      </Group>

      <Tabs value={activeTab} onChange={setActiveTab}>
        <Tabs.List mb="lg">
          <Tabs.Tab value="profile" leftSection={<IconUser size={18} />}>
            Mi Perfil de Socio
          </Tabs.Tab>
          <Tabs.Tab
            value="my-competitions"
            leftSection={<IconTrophy size={18} />}
            rightSection={
              myRegistrations.length > 0 ? (
                <Badge size="xs" circle color="blue">
                  {myRegistrations.length}
                </Badge>
              ) : null
            }
          >
            Mis Concursos
          </Tabs.Tab>
          <Tabs.Tab
            value="open-registrations"
            leftSection={<IconCalendarPlus size={18} />}
            rightSection={
              openCompetitions.length > 0 ? (
                <Badge size="xs" circle color="teal">
                  {openCompetitions.length}
                </Badge>
              ) : null
            }
          >
            Inscripciones Abiertas
          </Tabs.Tab>
        </Tabs.List>

        {/* TAB 1: MI PERFIL DE SOCIO */}
        <Tabs.Panel value="profile">
          <SimpleGrid cols={{ base: 1, md: 2 }} spacing="lg">
            {/* Bloque 1: Datos de Cuenta */}
            <Paper p="md" withBorder>
              <Group mb="sm">
                <IconUser size={20} />
                <Title order={5}>Datos de cuenta</Title>
              </Group>
              <Divider mb="md" />
              <Stack gap="xs">
                <Group>
                  <Text fw={500} w={120}>
                    Email:
                  </Text>
                  <Text>{user?.email ?? '-'}</Text>
                </Group>
                <Group>
                  <Text fw={500} w={120}>
                    Roles:
                  </Text>
                  <Group gap={4}>
                    {user?.roles.map((role) => (
                      <Badge
                        key={role}
                        size="sm"
                        variant="light"
                        color={role === 'Admin' ? 'blue' : 'teal'}
                      >
                        {role}
                      </Badge>
                    ))}
                  </Group>
                </Group>
              </Stack>
            </Paper>

            {/* Bloque 2: Cambio de Contraseña */}
            <Paper p="md" withBorder>
              <Group mb="sm">
                <IconKey size={20} />
                <Title order={5}>Cambiar contraseña</Title>
              </Group>
              <Divider mb="md" />

              <form onSubmit={pwdForm.onSubmit(handleChangePassword)}>
                <Stack gap="xs">
                  {pwdError && <Alert color="red">{pwdError}</Alert>}

                  <PasswordInput
                    label="Contraseña actual"
                    size="xs"
                    required
                    {...pwdForm.getInputProps('currentPassword')}
                  />
                  <PasswordInput
                    label="Contraseña nueva"
                    size="xs"
                    required
                    {...pwdForm.getInputProps('newPassword')}
                  />
                  <PasswordInput
                    label="Confirmar contraseña nueva"
                    size="xs"
                    required
                    {...pwdForm.getInputProps('confirmPassword')}
                  />

                  <Group justify="flex-end" mt="xs">
                    <Button type="submit" size="xs" loading={changing} disabled={changing}>
                      Actualizar contraseña
                    </Button>
                  </Group>
                </Stack>
              </form>
            </Paper>

            {/* Bloque 3: Datos de Pescador */}
            <Paper p="md" withBorder style={{ gridColumn: '1 / -1' }}>
              <Group mb="sm">
                <IconAddressBook size={20} />
                <Title order={5}>Ficha del Pescador</Title>
              </Group>
              <Divider mb="md" />

              {loadingProfile ? (
                <Center py="md">
                  <Loader size="sm" />
                </Center>
              ) : profileError ? (
                <Alert color="yellow" icon={<IconAlertCircle size={16} />}>
                  {profileError}
                </Alert>
              ) : profile ? (
                <SimpleGrid cols={{ base: 1, sm: 2, md: 3 }} spacing="sm">
                  <ProfileField label="Nombre" value={profile.firstName} />
                  <ProfileField label="Apellidos" value={profile.lastName} />
                  <ProfileField
                    label="Fecha de nacimiento"
                    value={formatDate(profile.dateOfBirth)}
                  />
                  <ProfileField
                    label="Documento"
                    value={`${profile.documentType} - ${profile.documentNumber}`}
                  />
                  <ProfileField
                    label="Licencia federativa"
                    value={profile.federationLicense || '-'}
                  />
                  <ProfileField
                    label="Número federativo"
                    value={profile.federationNumber || '-'}
                  />
                  <ProfileField
                    label="Licencia regional"
                    value={profile.regionalLicense || '-'}
                  />
                  <ProfileField
                    label="Dirección"
                    value={formatAddress(profile) || '-'}
                    span={2}
                  />
                </SimpleGrid>
              ) : (
                <Text c="dimmed" size="sm">
                  No se ha vinculado aún una ficha de pescador a este usuario.
                </Text>
              )}
            </Paper>
          </SimpleGrid>
        </Tabs.Panel>

        {/* TAB 2: MIS CONCURSOS */}
        <Tabs.Panel value="my-competitions">
          {loadingRegistrations ? (
            <Center py="xl">
              <Loader />
            </Center>
          ) : registrationsError ? (
            <Alert color="red" icon={<IconAlertCircle size={16} />}>
              {registrationsError}
            </Alert>
          ) : myRegistrations.length === 0 ? (
            <Paper p="xl" withBorder style={{ textAlign: 'center' }}>
              <IconTrophy size={48} color="#228be6" style={{ margin: '0 auto 12px' }} />
              <Title order={4} mb="xs">
                Aún no estás inscrito en ningún concurso
              </Title>
              <Text c="dimmed" size="sm" mb="md">
                Consulta los concursos disponibles e inscríbete para empezar a puntuar en la liga.
              </Text>
              <Button onClick={() => setActiveTab('open-registrations')} variant="light">
                Ver concursos con inscripción abierta
              </Button>
            </Paper>
          ) : (
            <Paper p="md" withBorder>
              <Table.ScrollContainer minWidth={850}>
                <Table striped highlightOnHover verticalSpacing="sm">
                  <Table.Thead>
                    <Table.Tr>
                      <Table.Th>Concurso</Table.Th>
                      <Table.Th>Liga</Table.Th>
                      <Table.Th>Fecha y Horario</Table.Th>
                      <Table.Th>Sede / Zona</Table.Th>
                      <Table.Th>Estado</Table.Th>
                      <Table.Th>Pesquera</Table.Th>
                      <Table.Th>Peso Total</Table.Th>
                      <Table.Th>Puntos</Table.Th>
                      <Table.Th>Posición</Table.Th>
                      <Table.Th>Acciones</Table.Th>
                    </Table.Tr>
                  </Table.Thead>
                  <Table.Tbody>
                    {myRegistrations.map((r) => (
                      <Table.Tr key={r.resultId}>
                        <Table.Td fw={600}>
                          #{r.competitionNumber} {r.competitionName}
                        </Table.Td>
                        <Table.Td>{r.leagueName}</Table.Td>
                        <Table.Td>
                          <Group gap={4}>
                            <IconClock size={14} color="#868e96" />
                            <Text size="sm">{formatDate(r.date)}</Text>
                          </Group>
                        </Table.Td>
                        <Table.Td>
                          <Group gap={4}>
                            <IconMapPin size={14} color="#868e96" />
                            <Text size="sm">
                              {r.venue} {r.zone ? `(${r.zone})` : ''}
                            </Text>
                          </Group>
                        </Table.Td>
                        <Table.Td>{getStatusBadge(r.status)}</Table.Td>
                        <Table.Td>
                          {r.assignedSpotNumber ? (
                            <Badge color="cyan" variant="filled">
                              Pesquera #{r.assignedSpotNumber}
                            </Badge>
                          ) : (
                            <Text size="sm" c="dimmed">
                              Sin sortear
                            </Text>
                          )}
                        </Table.Td>
                        <Table.Td>
                          {r.weightInGrams !== null && r.weightInGrams !== undefined ? (
                            <Text fw={500}>{r.weightInGrams} g</Text>
                          ) : (
                            <Text size="sm" c="dimmed">-</Text>
                          )}
                        </Table.Td>
                        <Table.Td>
                          <Badge color="grape" variant="light">
                            {r.points} pts
                          </Badge>
                        </Table.Td>
                        <Table.Td>{getRankingBadge(r.ranking)}</Table.Td>
                        <Table.Td>
                          <Group gap={4}>
                            <Tooltip label="Ver acta y resultados del concurso">
                              <ActionIcon
                                variant="light"
                                color="blue"
                                onClick={() =>
                                  navigate(Routes.competitionResultsFor(r.competitionId))
                                }
                              >
                                <IconEye size={16} />
                              </ActionIcon>
                            </Tooltip>
                            {r.status === 'RegistrationOpen' && (
                              <Tooltip label="Cancelar mi inscripción">
                                <ActionIcon
                                  variant="light"
                                  color="red"
                                  loading={unregisteringId === r.resultId}
                                  onClick={() => handleSelfUnregister(r.resultId)}
                                >
                                  <IconTrash size={16} />
                                </ActionIcon>
                              </Tooltip>
                            )}
                          </Group>
                        </Table.Td>
                      </Table.Tr>
                    ))}

                  </Table.Tbody>
                </Table>
              </Table.ScrollContainer>
            </Paper>
          )}
        </Tabs.Panel>

        {/* TAB 3: INSCRIPCIONES ABIERTAS */}
        <Tabs.Panel value="open-registrations">
          {loadingOpenCompetitions ? (
            <Center py="xl">
              <Loader />
            </Center>
          ) : openCompetitions.length === 0 ? (
            <Paper p="xl" withBorder style={{ textAlign: 'center' }}>
              <IconCalendarPlus size={48} color="#868e96" style={{ margin: '0 auto 12px' }} />
              <Title order={4} mb="xs">
                No hay concursos con inscripción abierta en este momento
              </Title>
              <Text c="dimmed" size="sm">
                Las inscripciones se habilitan cuando el administrador abre el plazo de registro.
              </Text>
            </Paper>
          ) : (
            <SimpleGrid cols={{ base: 1, sm: 2, md: 3 }} spacing="md">
              {openCompetitions.map((comp) => {
                const isAlreadyRegistered = myRegistrations.some(
                  (r) => r.CompetitionId === comp.id
                );
                const isFull = comp.participantCount >= comp.maxSpots;
                const spotsPercentage = Math.round(
                  (comp.participantCount / comp.maxSpots) * 100
                );

                return (
                  <Card key={comp.id} withBorder shadow="sm" radius="md" p="md">
                    <Group justify="space-between" mb="xs">
                      <Badge color="teal" variant="light">
                        Inscripción Abierta
                      </Badge>
                      <Badge color="gray">#{comp.competitionNumber}</Badge>
                    </Group>

                    <Title order={4} mb="xs">
                      {comp.name || `Manga #${comp.competitionNumber}`}
                    </Title>

                    <Stack gap="xs" mb="md">
                      <Group gap={6}>
                        <IconClock size={16} color="#868e96" />
                        <Text size="sm">
                          {formatDate(comp.date)} ({comp.startTime.slice(0, 5)} - {comp.endTime.slice(0, 5)})
                        </Text>
                      </Group>
                      <Group gap={6}>
                        <IconMapPin size={16} color="#868e96" />
                        <Text size="sm">
                          {comp.venue} {comp.zone ? `(${comp.zone})` : ''}
                        </Text>
                      </Group>
                    </Stack>

                    <Divider mb="sm" />

                    <Group justify="space-between" mb={4}>
                      <Text size="xs" c="dimmed">
                        Plazas ocupadas
                      </Text>
                      <Text size="xs" fw={600}>
                        {comp.participantCount} / {comp.maxSpots}
                      </Text>
                    </Group>
                    <Progress
                      value={spotsPercentage}
                      color={isFull ? 'red' : spotsPercentage > 80 ? 'yellow' : 'teal'}
                      size="sm"
                      mb="md"
                    />

                    {isAlreadyRegistered ? (
                      <Button
                        fullWidth
                        variant="light"
                        color="teal"
                        leftSection={<IconCheck size={16} />}
                        disabled
                      >
                        Ya estás inscrito
                      </Button>
                    ) : (
                      <Button
                        fullWidth
                        color="blue"
                        disabled={isFull}
                        loading={registeringId === comp.id}
                        onClick={() => handleSelfRegister(comp.id)}
                      >
                        {isFull ? 'Plazas agotadas' : 'Inscribirme ahora'}
                      </Button>
                    )}
                  </Card>
                );
              })}
            </SimpleGrid>
          )}
        </Tabs.Panel>
      </Tabs>
    </Container>
  );
}

function ProfileField({
  label,
  value,
  span = 1,
}: {
  label: string;
  value: string;
  span?: number;
}) {
  return (
    <TextInput
      label={label}
      value={value}
      readOnly
      variant="filled"
      style={span === 2 ? { gridColumn: 'span 2' } : undefined}
    />
  );
}
