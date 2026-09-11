export interface RoleSummary {
  roleId: string;
  roleName: string;
}

export interface UserDto {
  id: string;
  organisationId: string | null;
  email: string;
  fullName: string;
  phoneNumber: string | null;
  isActive: boolean;
  mustChangePassword: boolean;
  lastLoginAt: string | null;
  createdAt: string;
  roles: RoleSummary[];
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface CreateUserPayload {
  organisationId: string;
  email: string;
  fullName: string;
  phoneNumber?: string;
  password: string;
  roleIds: string[];
}

export interface UpdateUserPayload {
  fullName: string;
  phoneNumber?: string;
}
