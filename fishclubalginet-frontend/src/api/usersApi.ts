import { apiClient } from './apiClient';
import { Endpoints } from '../constants';
import type {
  UserDto,
  PaginatedResult,
  CreateUserRequest,
  AssignRoleRequest,
  RemoveRoleRequest,
} from '../types';

export async function getUsers(
  skip: number,
  take: number,
  search?: string
): Promise<PaginatedResult<UserDto>> {
  const url = Endpoints.Users.GetAllPaged(skip, take, search);
  const { data } = await apiClient.get<PaginatedResult<UserDto>>(url);
  return data;
}

export async function createUser(request: CreateUserRequest): Promise<string> {
  const { data } = await apiClient.post<{ id: string }>(
    Endpoints.Users.Create,
    request
  );
  return data.id;
}

export async function blockUser(userId: string): Promise<void> {
  await apiClient.post(Endpoints.Users.Block(userId));
}

export async function unblockUser(userId: string): Promise<void> {
  await apiClient.post(Endpoints.Users.Unblock(userId));
}

export async function assignRole(
  userId: string,
  request: AssignRoleRequest
): Promise<void> {
  await apiClient.post(Endpoints.Users.AssignRole(userId), request);
}

export async function removeRole(
  userId: string,
  request: RemoveRoleRequest
): Promise<void> {
  await apiClient.post(Endpoints.Users.RemoveRole(userId), request);
}
