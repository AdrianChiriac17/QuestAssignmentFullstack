export interface LoginUserRequestDto {
  email: string;
  password: string;
}

export interface RegisterUserRequestDto {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface UserResponseDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface LoginUserResponseDto {
  token: string;
  user: UserResponseDto;
}

export interface AuthSession {
  token: string;
  user: UserResponseDto;
}
