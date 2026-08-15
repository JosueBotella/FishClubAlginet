import { apiClient } from './apiClient';
import { Endpoints } from '../constants';
import type { LoginRequest, LoginResponse, ChangePasswordRequest } from '../types';

export const authApi = {
  login: (data: LoginRequest) =>
    apiClient.post<LoginResponse>(Endpoints.Account.Login, {
      userName: data.email,
      email: data.email,
      password: data.password,
    }),

  changePassword: (data: ChangePasswordRequest) =>
    apiClient.post(Endpoints.Account.ChangePassword, data),
};
