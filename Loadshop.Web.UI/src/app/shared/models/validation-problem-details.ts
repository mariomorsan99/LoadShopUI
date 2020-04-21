export interface ValidationProblemDetails {
  title: string;
  status: number;
  detail: string;
  instance: string;
  errors: object;
}
