import { useEffect } from 'react';
import {
  Modal,
  TextInput,
  NumberInput,
  Button,
  Group,
  Stack,
} from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import { createLeague, updateLeague } from '../../api/leaguesApi';
import type { LeagueDto, LeagueFormData } from '../../types';

interface Props {
  opened: boolean;
  onClose: () => void;
  onSuccess: () => void;
  league: LeagueDto | null;
}

const DEFAULT_FORM: LeagueFormData = {
  name: '',
  year: new Date().getFullYear(),
  minPoints: 5,
  worstResultsToDiscard: 0,
};

export default function CreateEditLeagueModal({ opened, onClose, onSuccess, league }: Props) {
  const isEditing = league !== null;

  const form = useForm<LeagueFormData>({
    initialValues: DEFAULT_FORM,
    validate: {
      name: (value) => (value.trim().length > 0 ? null : 'El nombre es obligatorio'),
      year: (value) => (value >= 2000 ? null : 'El año debe ser 2000 o posterior'),
      minPoints: (value) => (value >= 0 ? null : 'Debe ser >= 0'),
      worstResultsToDiscard: (value) => (value >= 0 ? null : 'Debe ser >= 0'),
    },
  });

  useEffect(() => {
    if (opened) {
      if (isEditing) {
        form.setValues({
          name: league.name,
          year: league.year,
          minPoints: league.minPoints,
          worstResultsToDiscard: league.worstResultsToDiscard,
        });
      } else {
        form.setValues(DEFAULT_FORM);
      }
      form.resetDirty();
    }
  }, [opened, league]);

  const handleSubmit = async (values: LeagueFormData) => {
    try {
      if (isEditing) {
        await updateLeague(league!.id, {
          name: values.name,
          minPoints: values.minPoints,
          worstResultsToDiscard: values.worstResultsToDiscard,
        });
        notifications.show({
          title: 'Liga actualizada',
          message: values.name,
          color: 'green',
        });
      } else {
        await createLeague(values);
        notifications.show({
          title: 'Liga creada',
          message: values.name,
          color: 'green',
        });
      }
      onSuccess();
    } catch {
      notifications.show({
        title: 'Error',
        message: isEditing ? 'No se pudo actualizar la liga.' : 'No se pudo crear la liga.',
        color: 'red',
      });
    }
  };

  return (
    <Modal
      opened={opened}
      onClose={onClose}
      title={isEditing ? 'Editar liga' : 'Crear liga'}
      centered
    >
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <TextInput
            label="Nombre"
            placeholder="Liga 2026"
            required
            {...form.getInputProps('name')}
          />

          <NumberInput
            label="Año"
            placeholder={new Date().getFullYear().toString()}
            required
            min={2000}
            {...form.getInputProps('year')}
            disabled={isEditing}
          />

          <NumberInput
            label="Puntos minimos"
            description="Puntos que recibe cada participante (por defecto 5)"
            required
            min={0}
            {...form.getInputProps('minPoints')}
          />

          <NumberInput
            label="Peores resultados a descartar"
            description="N de peores concursos a restar en clasificacion por puntos"
            required
            min={0}
            {...form.getInputProps('worstResultsToDiscard')}
          />

          <Group justify="flex-end" mt="md">
            <Button variant="default" onClick={onClose}>
              Cancelar
            </Button>
            <Button type="submit" disabled={!form.isDirty()}>
              {isEditing ? 'Guardar cambios' : 'Crear'}
            </Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  );
}
