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
  Switch,
} from '@mantine/core';
import {
  IconSearch,
  IconTrash,
  IconRefresh,
  IconHistory,
} from '@tabler/icons-react';
import { notifications } from '@mantine/notifications';
import { getFishermen, deleteFisherman } from '../../api/fishermenApi';
import { DocumentTypeLabels } from '../../types';
import type { FishermanDto } from '../../types';

const PAGE_SIZE = 15;

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

  const handleConfirmDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await deleteFisherman(deleteTarget.id);
      notifications.show({
        title: 'Pescador eliminado',
        message: deleteTarget.firstName + ' ' + deleteTarget.lastName,
        color: 'green',
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

  const formatDate = (iso: string) => {
    const d = new Date(iso);
    return d.toLocaleDateString('es-ES', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    });
  };

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
          label={showDeleted ? "Ver activos" : "Ver eliminados"}
          checked={showDeleted}
          onChange={(e) => {
            setShowDeleted(e.currentTarget.checked);
            setPage(1);
          }}
        />
      </Group>

      {/* Barra de busqueda */}
      <Group mb="md">
        <TextInput
          placeholder="Buscar por nombre, documento, licencia..."
          leftSection={<IconSearch size={16} />}
          value={searchInput}
          onChange={(e) => setSearchInput(e.currentTarget.value)}
          onKeyDown={handleSearchKeyDown}
          style={{ flex: 1 }}
        />
        <Button variant="light" onClick={handleSearch}>
          Buscar
        </Button>
        <ActionIcon
          variant="subtle"
          onClick={() => {
            setSearchInput('');
            setSearch('');
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

      {loading ? (
        <Center py="xl">
          <Loader size="lg" />
        </Center>
      ) : fishermen.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">
          No se encontraron pescadores.
        </Text>
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
                {showDeleted && <Table.Th>Eliminado</Table.Th>}
                <Table.Th style={{ width: 80 }}>Acciones</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {fishermen.map((f) => (
                <Table.Tr key={f.id}>
                  <Table.Td>
                    <Text size="sm" fw={500}>
                      {f.lastName}, {f.firstName}
                    </Text>
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
                    <Text size="sm">{f.federationLicense || '-'}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm">{formatDate(f.dateOfBirth)}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm">{f.addressCity}</Text>
                  </Table.Td>
                  {showDeleted && (
                    <Table.Td>
                      <Badge size="sm" color="red" variant="light">
                        Eliminado
                      </Badge>
                    </Table.Td>
                  )}
                  <Table.Td>
                    {!f.isDeleted && (
                      <Tooltip label="Eliminar pescador">
                        <ActionIcon
                          variant="subtle"
                          color="red"
                          onClick={() => setDeleteTarget(f)}
                        >
                          <IconTrash size={18} />
                        </ActionIcon>
                      </Tooltip>
                    )}
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>

          {totalPages > 1 && (
            <Group justify="center" mt="md">
              <Pagination
                total={totalPages}
                value={page}
                onChange={setPage}
                size="sm"
              />
              <Text size="xs" c="dimmed">
                {totalCount} pescador{totalCount !== 1 ? 'es' : ''} en total
              </Text>
            </Group>
          )}
        </>
      )}

      {/* Modal confirmacion eliminar */}
      <Modal
        opened={deleteTarget !== null}
        onClose={() => setDeleteTarget(null)}
        title="Confirmar eliminacion"
        centered
      >
        <Text size="sm">
          Vas a eliminar a{' '}
          <Text span fw={700}>
            {deleteTarget?.firstName} {deleteTarget?.lastName}
          </Text>
          . Esta accion marca al pescador como eliminado (soft delete).
        </Text>
        <Group justify="flex-end" mt="lg">
          <Button variant="default" onClick={() => setDeleteTarget(null)}>
            Cancelar
          </Button>
          <Button
            color="red"
            loading={deleting}
            onClick={handleConfirmDelete}
          >
            Eliminar
          </Button>
        </Group>
      </Modal>
    </Container>
  );
}
