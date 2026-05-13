export interface ApiErrorResponseDto {
  message?: string;
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}
