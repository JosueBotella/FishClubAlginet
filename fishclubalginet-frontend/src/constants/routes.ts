export const Routes = {
  Home: '/',
  Login: '/login',
  Fishermen: '/admin/fishermen',
  AddFisherman: '/fishermen/add',
  Users: '/admin/users',
  Leagues: '/admin/leagues',
  Profile: '/profile',
  Competitions: '/admin/leagues/:leagueId/competitions',
  CompetitionResults: '/admin/competitions/:competitionId/results',
  competitionsFor: (leagueId: string) => `/admin/leagues/${leagueId}/competitions`,
  competitionResultsFor: (competitionId: string) =>
    `/admin/competitions/${competitionId}/results`,
} as const;
