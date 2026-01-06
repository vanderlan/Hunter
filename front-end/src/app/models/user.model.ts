export enum UserProfile {
  Admin = 1,
  Partner = 2
}

export interface User {
  publicId?: string;
  name: string;
  email: string;
  profile: UserProfile;
  createdAt?: Date;
  lastUpdated?: Date;
}

export interface UserCreateRequest {
  name: string;
  email: string;
  password: string;
  profile: UserProfile;
}

export interface UserUpdateRequest {
  publicId?: string;
  name: string;
  email: string;
  profile: UserProfile;
}

export interface PagedUsers {
  items: User[];
  count: number;
}
