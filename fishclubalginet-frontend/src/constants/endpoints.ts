export const Endpoints = {
  Account: {
    Login: 'api/account/login',
    Register: 'api/account/register',
    ChangePassword: 'api/account/change-password',
  },
  Fishermen: {
    Add: 'api/fishermen/Add',
    GetAll: 'api/fishermen/GetAll',
    MyProfile: 'api/fishermen/my-profile',
    Update: (id: number) => `api/fishermen/${id}`,
    Delete: (id: number) => `api/fishermen/${id}`,
    GetAllPaged: (skip: number, take: number, search?: string, showDeleted?: boolean) => {
      let url = `api/fishermen/GetAll?skip=${skip}&take=${take}`;
      if (search) url += `&search=${encodeURIComponent(search)}`;
      if (showDeleted) url += `&showDeleted=true`;
      return url;
    },
  },
  Users: {
    GetAll: 'api/users',
    Create: 'api/users',
    Block: (userId: string) => `api/users/${userId}/block`,
    Unblock: (userId: string) => `api/users/${userId}/unblock`,
    AssignRole: (userId: string) => `api/users/${userId}/assign-role`,
    RemoveRole: (userId: string) => `api/users/${userId}/remove-role`,
    GetAllPaged: (skip: number, take: number, search?: string) => {
      let url = `api/users?skip=${skip}&take=${take}`;
      if (search) url += `&search=${encodeURIComponent(search)}`;
      return url;
    },
  },
  Leagues: {
    Base: 'api/leagues',
    GetActive: 'api/leagues/active',
    ById: (id: string) => `api/leagues/${id}`,
    Activate: (id: string) => `api/leagues/${id}/activate`,
    Archive: (id: string) => `api/leagues/${id}/archive`,
    Unarchive: (id: string) => `api/leagues/${id}/unarchive`,
    Standings: (id: string) => `api/leagues/${id}/standings`,
    GetAllPaged: (skip: number, take: number, year?: number, archived?: boolean) => {
      let url = `api/leagues?skip=${skip}&take=${take}`;
      if (year !== undefined) url += `&year=${year}`;
      if (archived !== undefined) url += `&archived=${archived}`;
      return url;
    },
  },
  Competitions: {
    Base: 'api/competitions',
    ById: (id: string) => `api/competitions/${id}`,
    ByLeague: (leagueId: string) => `api/competitions?leagueId=${leagueId}`,
    Register: (id: string) => `api/competitions/${id}/register`,
    Results: (id: string) => `api/competitions/${id}/results`,
    OpenRegistration: (id: string) => `api/competitions/${id}/open-registration`,
    CloseRegistration: (id: string) => `api/competitions/${id}/close-registration`,
    ReopenRegistration: (id: string) => `api/competitions/${id}/reopen-registration`,
    AssignSpots: (id: string) => `api/competitions/${id}/assign-spots`,
    MoveToResultsDraft: (id: string) => `api/competitions/${id}/results-draft`,
    ValidateResults: (id: string) => `api/competitions/${id}/validate-results`,
    RemoveResult: (resultId: string) => `api/competitions/results/${resultId}`,
    UpdateResult: (resultId: string) => `api/competitions/results/${resultId}`,
    BiggestCatchConfig: (id: string) => `api/competitions/${id}/biggest-catch-config`,
  },
} as const;

