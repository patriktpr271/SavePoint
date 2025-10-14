import React, { createContext, useContext, useState, useCallback, ReactNode } from 'react';
import { AppError, ErrorType, ErrorHandler } from '../services/errorHandler';

interface ErrorContextType {
  // Current error state
  error: AppError | null;
  // Show/hide error
  showError: (error: AppError | string, context?: string) => void;
  clearError: () => void;
  // Global loading state for operations that might error
  isLoading: boolean;
  setLoading: (loading: boolean) => void;
  // Retry functionality
  retry?: () => void;
  setRetry: (retryFn?: () => void) => void;
}

const ErrorContext = createContext<ErrorContextType | undefined>(undefined);

export const useError = () => {
  const context = useContext(ErrorContext);
  if (context === undefined) {
    throw new Error('useError must be used within an ErrorProvider');
  }
  return context;
};

interface ErrorProviderProps {
  children: ReactNode;
}

export const ErrorProvider: React.FC<ErrorProviderProps> = ({ children }) => {
  const [error, setError] = useState<AppError | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [retry, setRetry] = useState<(() => void) | undefined>(undefined);

  const showError = useCallback((error: AppError | string, context?: string) => {
    const appError = error instanceof AppError 
      ? error 
      : new AppError(error, ErrorType.UNKNOWN);
    
    setError(appError);
    ErrorHandler.logError(appError, context);
  }, []);

  const clearError = useCallback(() => {
    setError(null);
    setRetry(undefined);
  }, []);

  const setLoading = useCallback((loading: boolean) => {
    setIsLoading(loading);
  }, []);

  const setRetryFunction = useCallback((retryFn?: () => void) => {
    setRetry(() => retryFn);
  }, []);

  return (
    <ErrorContext.Provider value={{
      error,
      showError,
      clearError,
      isLoading,
      setLoading,
      retry,
      setRetry: setRetryFunction,
    }}>
      {children}
    </ErrorContext.Provider>
  );
};