export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiration: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface User {
  email: string;
  name: string;
  profile: string;
}
