export interface UserDto {
  id: string;
  email: string;
  isLockedOut: boolean;
  roles: string[];
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
}

export interface CreateUserRequest {
  email: string;
  password: string;
  role: string;
}

export interface AssignRoleRequest {
  role: string;
}

export interface RemoveRoleRequest {
  role: string;
}
