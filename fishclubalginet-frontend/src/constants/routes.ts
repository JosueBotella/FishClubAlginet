export const Routes = {
  Home: '/',
  Login: '/login',
  Fishermen: '/admin/fishermen',
  AddFisherman: '/fishermen/add',
  Users: '/admin/users',
  Leagues: '/admin/leagues',
  ArchivedLeagues: '/admin/leagues/archived',
  Profile: '/profile',
  Competitions: '/admin/leagues/:leagueId/competitions',
  CompetitionResults: '/admin/competitions/:competitionId/results',
  LeagueStandings: '/admin/leagues/:leagueId/standings',
  competitionsFor: (leagueId: string) => `/admin/leagues/${leagueId}/competitions`,
  competitionResultsFor: (competitionId: string) =>
    `/admin/competitions/${competitionId}/results`,
  standingsFor: (leagueId: string) => `/admin/leagues/${leagueId}/standings`,
} as const;
