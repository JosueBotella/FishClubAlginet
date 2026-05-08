export interface CompetitionDto {
  id: string;
  leagueId: string;
  competitionNumber: number;
  name: string | null;
  date: string;
  startTime: string;
  endTime: string;
  venue: string;
  zone: string;
  subspecialty: 'Mar' | 'AguaDulce';
  category: 'Seniors' | 'Juvenil';
  status: 'Planned' | 'RegistrationOpen' | 'Closed' | 'ResultsDraft' | 'ResultsValidated';
  maxSpots: number;
  participantCount: number;
  lastUpdateUtc: string;
}

export interface CompetitionResultDto {
  id: string;
  competitionId: string;
  fishermanId: number;
  assignedSpotNumber: number | null;
  didAttend: boolean;
  weightInGrams: number;
  biggestCatchWeight: number | null;
  points: number;
  ranking: number;
  isValidated: boolean;
  registrationDate: string;
}

export interface CreateCompetitionRequest {
  leagueId: string;
  competitionNumber: number;
  name: string | null;
  date: string;
  startTime: string;
  endTime: string;
  venue: string;
  zone: string;
  subspecialty: 'Mar' | 'AguaDulce';
  category: 'Seniors' | 'Juvenil';
  maxSpots: number;
}

export interface RegisterFishermanRequest {
  competitionId: string;
  fishermanId: number;
}

export type CreateCompetitionFormData = Omit<CreateCompetitionRequest, 'leagueId'>;
