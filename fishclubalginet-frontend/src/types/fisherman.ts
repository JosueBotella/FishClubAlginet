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

// Devuelto por GET /api/fishermen/my-profile.
// Nota: documentType viene como string ("Dni", "Nie", "Passport"), no como número.
export interface FishermanProfileDto {
  id: number;
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  documentType: string;
  documentNumber: string;
  federationLicense: string | null;
  regionalLicense: string | null;
  street: string;
  number: string;
  floorDoor: string;
  zipCode: string;
  city: string;
  province: string;
}

// Mapea el valor numérico del enum TypeNationalIdentifier (backend)
// a una etiqueta legible para mostrar en la UI.
//   1 -> Dni
//   2 -> Nie
//   3 -> Passport
export const DocumentTypeLabels: Record<number, string> = {
  1: 'DNI',
  2: 'NIE',
  3: 'Pasaporte',
};
