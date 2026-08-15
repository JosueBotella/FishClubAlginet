import { apiClient } from './apiClient';
import { Endpoints } from '../constants';
import type { LoginRequest, LoginResponse, ChangePasswordRequest } from '../types';

export const authApi = {
  login: (data: LoginRequest | { email?: string; userName?: string; password: string }) => {
    const userIdentifier = ('userName' in data && data.userName) ? data.userName : (data.email ?? '');
    return apiClient.post<LoginResponse>(Endpoints.Account.Login, {
      userName: userIdentifier,
      email: userIdentifier,
      password: data.password,
    });
  },

  changePassword: (data: ChangePasswordRequest) =>
    apiClient.post(Endpoints.Account.ChangePassword, data),
};
