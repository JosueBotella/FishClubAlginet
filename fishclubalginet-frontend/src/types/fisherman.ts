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
  isDeleted: boolean;
}

export const DocumentTypeLabels: Record<number, strin