import { useEffect } from 'react';
import { Modal, TextInput, NumberInput, Button, Group, Stack, Select } from '@mantine/core';
import { useForm } from '@mantine/form';
import { notifications } from '@mantine/notifications';
import { createCompetition } from '../../api/competitionsApi';
import type { CreateCompetitionFormData } from '../../types';
import { getApiErrorMessage } from '../../utils/errorUtils';

interface Props {
  opened: boolean;
  leagueId: string;
  nextCompetitionNumber: number;
  onClose: () => void;
  onSuccess: () => void;
}

const DEFAULT_FORM: CreateCompetitionFormData = {
  competitionNumber: 1,
  name: null,
  date: new Date().toISOString().split('T')[0],
  startTime: '09:00:00',
  endTime: '14:00:00',
  venue: '',
  zone: null,
  subspecialty: 'Mar',
  category: 'Seniors',
  maxSpots: 30,
  biggestCatchMinWeightInGrams: null,
};

export default function CreateCompetitionModal({ opened, leagueId, nextCompetitionNumber, onClose, onSuccess }: Props) {
  const form = useForm<CreateCompetitionFormData>({
    initialValues: DEFAULT_FORM,
    validate: {
      competitionNumber: (v: number) => (v > 0 ? null : 'Debe ser > 0'),
      date: (v: string | null) => (v ? null : 'La fecha es obligatoria'),
      startTime: (v: string) => (v ? null : 'La hora de inicio es obligatoria'),
      endTime: (v: string) => (v ? null : 'La hora de fin es obligatoria'),
      venue: (v: string) => (v.trim().length > 0 ? null : 'El lugar es obligatorio'),
      maxSpots: (v: number) => (v > 0 ? null : 'Debe ser > 0'),
    },
  });

  useEffect(() => {
    if (opened) {
      form.setFieldValue('competitionNumber', nextCompetitionNumber);
    }
  }, [opened, nextCompetitionNumber]);

  const handleClose = () => {
    form.reset();
    onClose();
  };

  const handleSubmit = async (values: CreateCompetitionFormData) => {
    try {
      await createCompetition({
        ...values,
        leagueId,
        name: values.name?.trim() || null,
        zone: values.zone?.trim() || null,
      });
      notifications.show({
        title: 'Concurso creado',
        message: `Concurso #${values.competitionNumber}`,
        color: 'green',
      });
      form.reset();
      onSuccess();
    } catch (err) {
      notifications.show({
        title: 'Error',
        message: getApiErrorMessage(err, 'No se pudo crear el concurso.'),
        color: 'red',
      });
    }
  };

  return (
    <Modal opened={opened} onClose={handleClose} title="Crear concurso" centered size="md">
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <NumberInput
            label="Número de concurso"
            description="Ordinal dentro de la liga (1º, 2º...)"
            required
            min={1}
            {...form.getInputProps('competitionNumber')}
          />

          <TextInput
            label="Nombre (opcional)"
            placeholder="Ej: Gran Premio Bellus"
            value={form.values.name ?? ''}
            onChange={(e) => form.setFieldValue('name', e.currentTarget.value || null)}
          />

          <TextInput
            label="Fecha"
            type="date"
            required
            {...form.getInputProps('date')}
          />

          <Group grow>
            <TextInput
              label="Hora inicio"
              type="time"
              required
              value={form.values.startTime.slice(0, 5)}
              onChange={(e) => form.setFieldValue('startTime', `${e.currentTarget.value}:00`)}
              error={form.errors.startTime}
            />
            <TextInput
              label="Hora fin"
              type="time"
              required
              value={form.values.endTime.slice(0, 5)}
              onChange={(e) => form.setFieldValue('endTime', `${e.currentTarget.value}:00`)}
              error={form.errors.endTime}
            />
          </Group>

          <Group grow>
            <TextInput
              label="Lugar (Venue)"
              placeholder="BELLUS, PINEDO..."
              required
              {...form.getInputProps('venue')}
            />
            <TextInput
              label="Zona (opcional)"
              placeholder="C, B, SUR..."
              value={form.values.zone ?? ''}
              onChange={(e) => form.setFieldValue('zone', e.currentTarget.value || null)}
              error={form.errors.zone}
            />
          </Group>

          <Group grow>
            <Select
              label="Subespecialidad"
              data={[
                { value: 'Mar', label: 'Mar' },
                { value: 'AguaDulce', label: 'Agua dulce' },
              ]}
              {...form.getInputProps('subspecialty')}
            />
            <Select
              label="Categoría"
              data={[
                { value: 'Seniors', label: 'Seniors' },
                { value: 'Juvenil', label: 'Juvenil' },
              ]}
              {...form.getInputProps('category')}
            />
          </Group>

          <NumberInput
            label="Plazas máximas"
            required
            min={1}
            {...form.getInputProps('maxSpots')}
          />

          <NumberInput
            label="Peso mínimo pieza mayor (gramos, opcional)"
            description="Solo se considera pieza mayor si supera este peso. Déjalo vacío para no aplicar mínimo."
            min={1}
            value={form.values.biggestCatchMinWeightInGrams ?? ''}
            onChange={(val) =>
              form.setFieldValue(
                'biggestCatchMinWeightInGrams',
                typeof val === 'number' ? val : null
              )
            }
          />

          <Group justify="flex-end" mt="md">
            <Button variant="default" onClick={handleClose}>
              Cancelar
            </Button>
            <Button type="submit">Crear</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  );
}
