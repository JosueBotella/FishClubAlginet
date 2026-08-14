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
  Competitions: '/leagues/:leagueId/competitions',
  AdminCompetitions: '/admin/leagues/:leagueId/competitions',
  CompetitionResults: '/competitions/:competitionId/results',
  AdminCompetitionResults: '/admin/competitions/:competitionId/results',
  LeagueStandings: '/leagues/:leagueId/standings',
  AdminLeagueStandings: '/admin/leagues/:leagueId/standings',
  competitionsFor: (leagueId: string) => `/leagues/${leagueId}/competitions`,
  competitionResultsFor: (competitionId: string) =>
    `/competitions/${competitionId}/results`,
  standingsFor: (leagueId: string) => `/leagues/${leagueId}/standings`,
} as const;


