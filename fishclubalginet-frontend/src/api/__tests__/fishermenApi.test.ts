import { describe, it, expect, vi, beforeEach } from 'vitest';
import { getMyProfile } from '../fishermenApi';
import type { FishermanProfileDto } from '../../types';

// Mockeamos el cliente axios subyacente. apiClient es una instancia singleton
// con interceptors, así que solo nos interesa controlar su método get.
vi.mock('../apiClient', () => ({
  apiClient: {
    get: vi.fn(),
  },
}));

import { apiClient } from '../apiClient';

const mockedGet = apiClient.get as unknown as ReturnType<typeof vi.fn>;

const sampleProfile: FishermanProfileDto = {
  id: 1,
  firstName: 'Josue',
  lastName: 'Botella',
  dateOfBirth: '1990-01-01T00:00:00Z',
  documentType: 'Dni',
  documentNumber: '12345678A',
  federationLicense: 'FED-1',
  regionalLicense: null,
  street: 'C/ Mayor',
  number: '12',
  floorDoor: '3 izq',
  zipCode: '46230',
  city: 'Alginet',
  province: 'Valencia',
};

describe('getMyProfile', () => {
  beforeEach(() => {
    mockedGet.mockReset();
  });

  it('devuelve el perfil cuando el endpoint responde 200', async () => {
    mockedGet.mockResolvedValueOnce({ data: sampleProfile });

    const result = await getMyProfile();

    expect(result).toEqual(sampleProfile);
    expect(mockedGet).toHaveBeenCalledWith('api/fishermen/my-profile');
  });

  it('devuelve null cuando el endpoint responde 404 (Admin sin Fisherman asociado)', async () => {
    // Simulamos el shape exacto de un error de axios con response.status = 404
    const axiosError = {
      response: { status: 404, data: { detail: 'Fisherman not found' } },
      isAxiosError: true,
    };
    mockedGet.mockRejectedValueOnce(axiosError);

    const result = await getMyProfile();

    expect(result).toBeNull();
  });

  it('relanza el error cuando el código no es 404 (p.ej. 500)', async () => {
    const axiosError = {
      response: { status: 500 },
      isAxiosError: true,
    };
    mockedGet.mockRejectedValueOnce(axiosError);

    await expect(getMyProfile()).rejects.toMatchObject({
      response: { status: 500 },
    });
  });

  it('relanza errores de red (sin response)', async () => {
    const networkError = new Error('Network Error');
    mockedGet.mockRejectedValueOnce(networkError);

    await expect(getMyProfile()).rejects.toThrow('Network Error');
  });
});
