export interface CompetitionDto {
  id: string;
  leagueId: string;
  competitionNumber: number;
  name: string | null;
  date: string;
  startTime: string;
  endTime: string;
  venue: string;
  zone: string | null;
  subspecialty: 'Mar' | 'AguaDulce';
  category: 'Seniors' | 'Juvenil';
  status: 'Planned' | 'RegistrationOpen' | 'Closed' | 'ResultsDraft' | 'ResultsValidated';
  maxSpots: number;
  participantCount: number;
  lastUpdateUtc: string;
  biggestCatchMinWeightInGrams: number | null;
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
  zone: string | null;
  subspecialty: 'Mar' | 'AguaDulce';
  category: 'Seniors' | 'Juvenil';
  maxSpots: number;
  biggestCatchMinWeightInGrams: number | null;
}

export interface UpdateBiggestCatchConfigRequest {
  minWeightInGrams: number | null;
}

export interface RegisterFishermanRequest {
  competitionId: string;
  fishermanId: number;
}

export type CreateCompetitionFormData = Omit<CreateCompetitionRequest, 'leagueId'>;

export interface LeagueFishermanStandingDto {
  fishermanId: number;
  fullName: string;
  totalWeightGrams: number;
  totalPoints: number;
  pointsAfterDiscard: number;
  competitionsAttended: number;
}

export interface LeagueStandingsDto {
  leagueId: string;
  leagueName: string;
  year: number;
  worstResultsToDiscard: number;
  byWeight: LeagueFishermanStandingDto[];
  byPoints: LeagueFishermanStandingDto[];
}
