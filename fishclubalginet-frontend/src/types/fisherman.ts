export interface FishermanDto {
  id: number;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  documentType: number;
  documentNumber: string;
  federationLicense: string | null;
  addressCity: string;
  addressProvince: string;
}

export const DocumentTypeLabels: Record<number, string> = {
  1: 'DNI',
  2: 'NIE',
  3: 'Pasaporte',
};
