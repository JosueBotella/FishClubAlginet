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
      let url = `api/users?sk