export const Routes = {
  Home: '/',
  Login: '/login',
  Standings: '/standings',
  Fishermen: '/admin/fishermen',
  AddFisherman: '/fishermen/add',
  Users: '/admin/users',
  Leagues: '/admin/leagues',
  ArchivedLeagues: '/admin/leagues/archived',
  Profile: '/profile',
  Competitions: '/admin/leagues/:leagueId/competitions',
  CompetitionResults: '/admin/competitions/:competitionId/results',
  LeagueStandings: '/leagues/:leagueId/standings',
  AdminLeagueStandings: '/admin/leagues/:leagueId/standings',
  competitionsFor: (leagueId: string) => `/admin/leagues/${leagueId}/competitions`,
  competitionResultsFor: (competitionId: string) =>
    `/admin/competitions/${competitionId}/results`,
  standingsFor: (leagueId: string) => `/leagues/${leagueId}/standings`,
} as const;

