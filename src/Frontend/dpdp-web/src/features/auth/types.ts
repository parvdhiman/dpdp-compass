export interface MeDto {
  id: string;
  organisationId: string | null;
  email: string;
  fullName: string;
  isSuperAdministrator: boolean;
  roles: string[];
  permissions: string[];
}

export interface AuthResult {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
  user: MeDto;
}
