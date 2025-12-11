export interface ApiErrorResponse {
  message: string;
  errorCode: string;
  statusCode: number;
  timestamp: string;
  traceId?: string;
  details?: any;
  stackTrace?: string;
  innerException?: ApiErrorResponse;
}

export interface ValidationErrorResponse extends ApiErrorResponse {
  validationErrors: Record<string, string[]>;
}

export enum ErrorType {
  VALIDATION = 'VALIDATION_ERROR',
  NOT_FOUND = 'RESOURCE_NOT_FOUND',
  UNAUTHORIZED = 'AUTHENTICATION_REQUIRED',
  FORBIDDEN = 'ACCESS_FORBIDDEN',
  CONFLICT = 'RESOURCE_CONFLICT',
  BUSINESS_RULE = 'BUSINESS_RULE_VIOLATION',
  NETWORK = 'NETWORK_ERROR',
  TIMEOUT = 'TIMEOUT',
  UNKNOWN = 'UNKNOWN_ERROR'
}

export class AppError extends Error {
  public readonly type: ErrorType;
  public readonly statusCode: number;
  public readonly details?: any;
  public readonly traceId?: string;

  constructor(
    message: string, 
    type: ErrorType = ErrorType.UNKNOWN, 
    statusCode: number = 500,
    details?: any,
    traceId?: string
  ) {
    super(message);
    this.name = 'AppError';
    this.type = type;
    this.statusCode = statusCode;
    this.details = details;
    this.traceId = traceId;
  }

  static fromApiResponse(apiError: ApiErrorResponse): AppError {
    const errorType = Object.values(ErrorType).find(type => type === apiError.errorCode) || ErrorType.UNKNOWN;
    return new AppError(
      apiError.message,
      errorType,
      apiError.statusCode,
      apiError.details,
      apiError.traceId
    );
  }
}

export class ErrorHandler {
  static async handleApiError(response: Response): Promise<never> {
    let errorData: ApiErrorResponse;

    try {
      errorData = await response.json();
    } catch {
      errorData = {
        message: 'An unexpected error occurred',
        errorCode: ErrorType.UNKNOWN,
        statusCode: response.status,
        timestamp: new Date().toISOString()
      };
    }

    throw AppError.fromApiResponse(errorData);
  }

  static getUserFriendlyMessage(error: AppError): string {
    switch (error.type) {
      case ErrorType.VALIDATION:
        return 'Please check your input and try again.';
      case ErrorType.NOT_FOUND:
        return 'The requested item could not be found.';
      case ErrorType.UNAUTHORIZED:
        return 'Please log in to continue.';
      case ErrorType.FORBIDDEN:
        return 'You do not have permission to perform this action.';
      case ErrorType.CONFLICT:
        return 'This action conflicts with existing data.';
      case ErrorType.BUSINESS_RULE:
        return error.message;
      case ErrorType.NETWORK:
        return 'Network error. Please check your connection and try again.';
      case ErrorType.TIMEOUT:
        return 'The request timed out. Please try again.';
      default:
        return 'An unexpected error occurred. Please try again later.';
    }
  }

  static getValidationErrors(error: AppError): Record<string, string[]> | null {
    if (error.type === ErrorType.VALIDATION && error.details?.validationErrors) {
      return error.details.validationErrors;
    }
    return null;
  }

  static isRetryable(error: AppError): boolean {
    return [
      ErrorType.NETWORK,
      ErrorType.TIMEOUT,
      ErrorType.UNKNOWN
    ].includes(error.type) && error.statusCode >= 500;
  }

  static async fetchWithErrorHandling(
    url: string, 
    options: RequestInit = {}
  ): Promise<Response> {
    try {
      const response = await fetch(url, {
        credentials: 'include',
        headers: {
          'Content-Type': 'application/json',
          ...options.headers,
        },
        ...options,
      });

      if (!response.ok) {
        await this.handleApiError(response);
      }

      return response;
    } catch (error) {
      if (error instanceof AppError) {
        throw error;
      }

      if (error instanceof TypeError && error.message.includes('fetch')) {
        throw new AppError(
          'Network error. Please check your connection.',
          ErrorType.NETWORK,
          0
        );
      }

      if (error instanceof Error && error.name === 'AbortError') {
        throw new AppError(
          'Request timed out.',
          ErrorType.TIMEOUT,
          408
        );
      }

      throw new AppError(
        error instanceof Error ? error.message : 'Unknown error occurred',
        ErrorType.UNKNOWN,
        500
      );
    }
  }

  static logError(error: AppError, context?: string): void {
    const logData = {
      message: error.message,
      type: error.type,
      statusCode: error.statusCode,
      traceId: error.traceId,
      context,
      timestamp: new Date().toISOString(),
      stack: error.stack
    };

    console.error('[ErrorHandler]', logData);
  }
}