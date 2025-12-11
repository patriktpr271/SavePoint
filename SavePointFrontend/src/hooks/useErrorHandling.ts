import { useCallback } from 'react';
import { useError } from '../contexts/ErrorContext';
import { ErrorHandler, AppError } from '../services/errorHandler';

export const useAsyncOperation = () => {
  const { showError, setLoading, setRetry } = useError();

  const executeAsync = useCallback(async <T>(
    operation: () => Promise<T>,
    options?: {
      onSuccess?: (result: T) => void;
      onError?: (error: AppError) => void;
      context?: string;
      showLoading?: boolean;
      retryable?: boolean;
    }
  ): Promise<T | null> => {
    const {
      onSuccess,
      onError,
      context,
      showLoading = true,
      retryable = true
    } = options || {};

    if (showLoading) {
      setLoading(true);
    }

    try {
      const result = await operation();
      
      if (onSuccess) {
        onSuccess(result);
      }
      
      return result;
    } catch (error) {
      const appError = error instanceof AppError 
        ? error 
        : new AppError(
            error instanceof Error ? error.message : 'Unknown error occurred'
          );

      if (retryable && ErrorHandler.isRetryable(appError)) {
        setRetry(() => () => executeAsync(operation, options));
      }

      if (onError) {
        onError(appError);
      } else {
        showError(appError, context);
      }

      return null;
    } finally {
      if (showLoading) {
        setLoading(false);
      }
    }
  }, [showError, setLoading, setRetry]);

  return { executeAsync };
};

export const useApiCall = () => {
  const { executeAsync } = useAsyncOperation();

  const apiCall = useCallback(async <T>(
    url: string,
    options: RequestInit = {},
    config?: {
      onSuccess?: (result: T) => void;
      onError?: (error: AppError) => void;
      context?: string;
      showLoading?: boolean;
      retryable?: boolean;
      parseResponse?: boolean;
    }
  ): Promise<T | null> => {
    const { parseResponse = true, ...restConfig } = config || {};

    return executeAsync(async () => {
      const response = await ErrorHandler.fetchWithErrorHandling(url, options);
      
      if (parseResponse) {
        return await response.json() as T;
      } else {
        return response as unknown as T;
      }
    }, restConfig);
  }, [executeAsync]);

  return { apiCall };
};

export const useFormSubmission = () => {
  const { executeAsync } = useAsyncOperation();

  const submitForm = useCallback(async <T>(
    submitFn: () => Promise<T>,
    options?: {
      onSuccess?: (result: T) => void;
      onValidationError?: (errors: Record<string, string[]>) => void;
      onError?: (error: AppError) => void;
      context?: string;
    }
  ): Promise<T | null> => {
    return executeAsync(submitFn, {
      ...options,
      onError: (error) => {
        const validationErrors = ErrorHandler.getValidationErrors(error);
        if (validationErrors && options?.onValidationError) {
          options.onValidationError(validationErrors);
        } else if (options?.onError) {
          options.onError(error);
        }
      },
      retryable: false
    });
  }, [executeAsync]);

  return { submitForm };
};