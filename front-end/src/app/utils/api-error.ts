/**
 * Extracts a human-readable message from a failed HTTP call.
 *
 * The API answers with two shapes:
 *  - domain errors: `{ title, notificationType, errors: ["message"] }`
 *  - validation errors: `{ title, status, errors: { Field: ["message"] } }`
 */
export function extractApiError(error: any, fallback: string): string {
  const body = error?.error;
  const errors = body?.errors ?? body?.Errors;

  if (Array.isArray(errors) && errors.length > 0) {
    return String(errors[0]);
  }

  if (errors && typeof errors === 'object') {
    const first = Object.values(errors).flat().find(v => typeof v === 'string');
    if (first) {
      return String(first);
    }
  }

  if (typeof body?.message === 'string') {
    return body.message;
  }

  if (error?.status === 429) {
    return 'Too many requests. Please try again later.';
  }

  return fallback;
}
