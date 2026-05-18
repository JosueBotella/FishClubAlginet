import { useCallback, useEffect, useState } from 'react';
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
  Modal,
  Stack,
  Switch,
} from '@mantine/core';
import {
  IconSearch,
  IconTrash,
  IconRefresh,
  IconHistory,
  IconPencil,
} from '@tabler/icons-react';
import { notifications } from '@mantine/notifications';
import { getFishermen, deleteFisherman, updateFisherman } from '../../api/fishermenApi';
import type { UpdateFishermanRequest } from '../../api/fishermenApi';
import { DocumentTypeLabels } from '../../types';
import type { FishermanDto } from '../../types';

const PAGE_SIZE = 15;

interface EditState {
  id: number;
  firstName: string;
  lastName: string;
  federationLicense: string;
  addressStreet: string;
  addressCity: string;
  addressZipCode: string;
  addressProvince: string;
}

function buildEditState(f: FishermanDto): EditState {
  return {
    id: f.id,
    firstName: f.firstName,
    lastName: f.lastName,
    federationLicense: f.federationLicense ?? '',
    addressStreet: f.addressStreet ?? '',
    addressCity: f.addressCity ?? '',
    addressZipCode: f.addressZipCode ?? '',
    addressProvince: f.addressProvince ?? '',
  };
}

export default function AdminFishermenPage() {
  const [fishermen, setFishermen] = useState<FishermanDto[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Delete confirmation
  const [deleteTarget, setDeleteTarget] = useState<FishermanDto | null>(null);
  const [deleting, setDeleting] = useState(false);

  // Edit modal
  const [editState, setEditState] = useState<EditState | null>(null);
  const [saving, setSaving] = useState(false);

  // Show deleted toggle
  const [showDeleted, setShowDeleted] = useState(false);

  const totalPages = Math.ceil(totalCount / PAGE_SIZE);

  const fetchFishermen = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const skip = (page - 1) * PAGE_SIZE;
      const result = await getFishermen(skip, PAGE_SIZE, search || undefined, showDeleted);
      setFishermen(result.items);
      setTotalCount(result.totalCount);
    } catch {
      setError('Error al cargar los pescadores.');
    } finally {
      setLoading(false);
    }
  }, [page, search, showDeleted]);

  useEffect(() => {
    fetchFishermen();
  }, [fetchFishermen]);

  const handleSearch = () => {
    setPage(1);
    setSearch(searchInput);
  };

  const handleSearchKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') handleSearch();
  };

  // ── Delete ────────────────────────────────────────────────────────────────
  const handleConfirmDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await deleteFisherman(deleteTarget.id);
      notifications.show({
        title: 'Pescador eliminado',
        message: `${deleteTarget.firstName} ${deleteTarget.lastName}`,
        color: 'orange',
      });
      setDeleteTarget(null);
      await fetchFishermen();
    } catch {
      notifications.show({
        title: 'Error',
        message: 'No se pudo eliminar el pescador.',
        color: 'red',
      });
    } finally {
      setDeleting(false);
    }
  };

  // ── Edit ──────────────────────────────────────────────────────────────────
  const handleSave = async () => {
    if (!editState) return;
    setSaving(true);
    try {
      const request: UpdateFishermanRequest = {
        firstName: editState.firstName,
        lastName: editState.lastName,
        federationLicense: editState.federationLicense || null,
        addressStreet: editState.addressStreet,
        addressCity: editState.addressCity,
        addressZipCode: editState.addressZipCode,
        addressProvince: editState.addressProvince,
      };
      await updateFisherman(editState.id, request);
      notifications.show({
        title: 'Pescador actualizado',
        message: `${editState.firstName} ${editState.lastName}`,
        color: 'green',
      });
      setEditState(null);
      await fetchFishermen();
    } catch {
      notifications.show({
        title: 'Error',
        message: 'No se pudo actualizar el pescador.',
        color: 'red',
      });
    } finally {
      setSaving(false);
    }
  };

  const set = (field: keyof EditState) => (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setEditState((prev) => prev && { ...prev, [field]: value });
  };

  // ── Utils ─────────────────────────────────────────────────────────────────
  const formatDate = (iso: string) =>
    new Date(iso).toLocaleDateString('es-ES', {
      day: '2-digit', month: '2-digit', year: 'numeric',
    });

  return (
    <Container size="lg" py="md">
      <Group justify="space-between" mb="md">
        <Title order={3}>
          {showDeleted ? (
            <Group gap={6} component="span">
              <IconHistory size={20} />
              Histórico de pescadores eliminados
            </Group>
          ) : (
            'Gestión de pescadores'
          )}
        </Title>
        <Switch
          label={showDeleted ? 'Ver activos' : 'Ver eliminados'}
          checked={showDeleted}
          onChange={(e) => { setShowDeleted(e.currentTarget.checked); setPage(1); }}
        />
      </Group>

      <Group mb="md">
        <TextInput
          placeholder="Buscar por nombre, documento, licencia..."
          leftSection={<IconSearch size={16} />}
          value={searchInput}
          onChange={(e) => setSearchInput(e.currentTarget.value)}
          onKeyDown={handleSearchKeyDown}
          style={{ flex: 1 }}
        />
        <Button variant="light" onClick={handleSearch}>Buscar</Button>
        <ActionIcon
          variant="subtle"
          onClick={() => { setSearchInput(''); setSearch(''); setPage(1); }}
          title="Limpiar filtro"
        >
          <IconRefresh size={18} />
        </ActionIcon>
      </Group>

      {error && <Alert color="red" mb="md">{error}</Alert>}

      {loading ? (
        <Center py="xl"><Loader size="lg" /></Center>
      ) : fishermen.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">No se encontraron pescadores.</Text>
      ) : (
        <>
          <Table striped highlightOnHover withTableBorder>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>Nombre</Table.Th>
                <Table.Th>Documento</Table.Th>
                <Table.Th>Licencia Fed.</Table.Th>
                <Table.Th>Fecha nac.</Table.Th>
                <Table.Th>Ciudad</Table.Th>
                {showDeleted && <Table.Th>Estado</Table.Th>}
                <Table.Th style={{ width: 90 }}>Acciones</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {fishermen.map((f) => (
                <Table.Tr key={f.id}>
                  <Table.Td>
                    <Text size="sm" fw={500}>{f.lastName}, {f.firstName}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Group gap={4}>
                      <Badge size="xs" variant="light" color="gray">
                        {DocumentTypeLabels[f.documentType] || f.documentType}
                      </Badge>
                      <Text size="sm">{f.documentNumber}</Text>
                    </Group>
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm">{f.federationLicense || '—'}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm">{formatDate(f.dateOfBirth)}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm">{f.addressCity}</Text>
                  </Table.Td>
                  {showDeleted && (
                    <Table.Td>
                      <Badge size="sm" color="red" variant="light">Eliminado</Badge>
                    </Table.Td>
                  )}
                  <Table.Td>
                    {!f.isDeleted && (
                      <Group gap={4}>
                        <Tooltip label="Editar pescador">
                          <ActionIcon
                            variant="subtle"
                            color="blue"
                            onClick={() => setEditState(buildEditState(f))}
                          >
                            <IconPencil size={16} />
                          </ActionIcon>
                        </Tooltip>
                        <Tooltip label="Eliminar pescador">
                          <ActionIcon
                            variant="subtle"
                            color="red"
                            onClick={() => setDeleteTarget(f)}
                          >
                            <IconTrash size={16} />
                          </ActionIcon>
                        </Tooltip>
                      </Group>
                    )}
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>

          {totalPages > 1 && (
            <Group justify="center" mt="md">
              <Pagination total={totalPages} value={page} onChange={setPage} size="sm" />
              <Text size="xs" c="dimmed">
                {totalCount} pescador{totalCount !== 1 ? 'es' : ''} en total
              </Text>
            </Group>
          )}
        </>
      )}

      {/* ── Modal editar ── */}
      <Modal
        opened={editState !== null}
        onClose={() => setEditState(null)}
        title={editState ? `Editar — ${editState.firstName} ${editState.lastName}` : ''}
        centered
        size="md"
      >
        {editState && (
          <Stack gap="sm">
            <Group grow>
              <TextInput
                label="Nombre"
                required
                value={editState.firstName}
                onChange={set('firstName')}
              />
              <TextInput
                label="Apellidos"
                required
                value={editState.lastName}
                onChange={set('lastName')}
              />
            </Group>
            <TextInput
              label="Licencia federativa"
              placeholder="Ej: 12345"
              value={editState.federationLicense}
              onChange={set('federationLicense')}
            />
            <TextInput
              label="Calle"
              value={editState.addressStreet}
              onChange={set('addressStreet')}
            />
            <Group grow>
              <TextInput
                label="Ciudad"
                value={editState.addressCity}
                onChange={set('addressCity')}
              />
              <TextInput
                label="Código postal"
                value={editState.addressZipCode}
                onChange={set('addressZipCode')}
              />
            </Group>
            <TextInput
              label="Provincia"
              value={editState.addressProvince}
              onChange={set('addressProvince')}
            />
            <Group justify="flex-end" mt="xs">
              <Button variant="default" onClick={() => setEditState(null)}>Cancelar</Button>
              <Button
                loading={saving}
                disabled={!editState.firstName.trim() || !editState.lastName.trim()}
                onClick={handleSave}
              >
                Guardar
              </Button>
            </Group>
          </Stack>
        )}
      </Modal>

      {/* ── Modal confirmar eliminar ── */}
      <Modal
        opened={deleteTarget !== null}
        onClose={() => setDeleteTarget(null)}
        title="Confirmar eliminación"
        centered
        size="sm"
      >
        <Text size="sm">
          Vas a eliminar a{' '}
          <Text span fw={700}>{deleteTarget?.firstName} {deleteTarget?.lastName}</Text>.
          {' '}Esta acción marca al pescador como eliminado (soft delete).
        </Text>
        <Group justify="flex-end" mt="lg">
          <Button variant="default" onClick={() => setDeleteTarget(null)}>Cancelar</Button>
          <Button color="red" loading={deleting} onClick={handleConfirmDelete}>Eliminar</Button>
        </Group>
      </Modal>
    </Container>
  );
}
