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
  receiveJobChangeNotifications?: boolean;
  lastLoginAt?: Date;
}

export interface UserCreateRequest {
  name: string;
  email: string;
  password: string;
  profile: UserProfile;
  sendWelcomeEmail?: boolean;
  receiveJobChangeNotifications?: boolean;
}

export interface UserUpdateRequest {
  publicId?: string;
  name: string;
  email: string;
  profile: UserProfile;
  receiveJobChangeNotifications?: boolean;
}

export interface UserPreferences {
  receiveJobChangeNotifications: boolean;
}

export interface PagedUsers {
  items: User[];
  count: number;
}
