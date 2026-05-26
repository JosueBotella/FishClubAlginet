import { AxiosError } from 'axios';

interface ApiProblemDetails {
  title?: string;
  errors?: Record<string, string[] | string>;
}

/**
 * Extracts a user-friendly error message from an API AxiosError or standard Error.
 * Supports ASP.NET Core ValidationProblem (ModelStateDictionary) and ProblemDetails.
 */
export function getApiErrorMessage(error: unknown, defaultMessage: string): string {
  if (error && typeof error === 'object' && 'response' in error) {
    const axiosError = error as AxiosError<ApiProblemDetails>;
    const data = axiosError.response?.data;

    if (data) {
      // 1. Validation errors (Model State Dictionary)
      if (data.errors && typeof data.errors === 'object') {
        const messages: string[] = [];
        for (const key in data.errors) {
          const val = data.errors[key];
          if (Array.isArray(val)) {
            messages.push(...val);
          } else if (typeof val === 'string') {
            messages.push(val);
          }
        }
        if (messages.length > 0) {
          return messages.join('\n');
        }
      }

      // 2. Custom problem detail (Conflict, NotFound, etc.)
      if (data.title && typeof data.title === 'string') {
        return data.title;
      }
    }
  }

  // 3. Network or other JavaScript error
  if (error instanceof Error) {
    return error.message;
  }

  return defaultMessage;
}
