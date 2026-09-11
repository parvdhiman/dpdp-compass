export interface PermissionDto {
  id: string;
  key: string;
  description: string;
  module: string;
}

export interface RoleDto {
  id: string;
  name: string;
  description: string | null;
  isSystemRole: boolean;
  permissions: PermissionDto[];
}
