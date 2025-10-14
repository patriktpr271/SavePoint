import React from 'react';
import { useError } from '../contexts/ErrorContext';
import { ErrorHandler, ErrorType } from '../services/errorHandler';

const GlobalErrorDisplay: React.FC = () => {
  const { error, clearError, retry, isLoading } = useError();

  if (!error) return null;

  const userFriendlyMessage = ErrorHandler.getUserFriendlyMessage(error);
  const validationErrors = ErrorHandler.getValidationErrors(error);
  const isRetryable = ErrorHandler.isRetryable(error);

  const handleRetry = () => {
    if (retry) {
      clearError();
      retry();
    }
  };

  return (
    <div className="fixed top-4 right-4 z-50 w-96">
      <div className={`alert ${getAlertClass(error.type)} shadow-lg`}>
        <div>
          <svg
            className="stroke-current shrink-0 h-6 w-6"
            fill="none"
            viewBox="0 0 24 24"
          >
            {error.type === ErrorType.VALIDATION ? (
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
                d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"
              />
            ) : (
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth="2"
                d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z"
              />
            )}
          </svg>
          <div>
            <h3 className="font-bold">{getErrorTitle(error.type)}</h3>
            <div className="text-xs">{userFriendlyMessage}</div>
            
            {/* Display validation errors if present */}
            {validationErrors && (
              <div className="mt-2 text-xs">
                <div className="font-semibold">Validation errors:</div>
                <ul className="list-disc list-inside mt-1 space-y-1">
                  {Object.entries(validationErrors).map(([field, errors]) =>
                    errors.map((errorMsg, index) => (
                      <li key={`${field}-${index}`}>
                        <span className="font-medium">{field}:</span> {errorMsg}
                      </li>
                    ))
                  )}
                </ul>
              </div>
            )}

            {/* Display trace ID for debugging */}
            {error.traceId && (
              <div className="text-xs opacity-70 mt-1">
                Trace ID: {error.traceId}
              </div>
            )}
          </div>
        </div>
        
        <div className="flex-none">
          {/* Retry button for retryable errors */}
          {isRetryable && retry && (
            <button
              className="btn btn-ghost btn-sm"
              onClick={handleRetry}
              disabled={isLoading}
            >
              {isLoading ? (
                <span className="loading loading-spinner loading-sm"></span>
              ) : (
                'Retry'
              )}
            </button>
          )}
          
          {/* Close button */}
          <button
            className="btn btn-ghost btn-sm"
            onClick={clearError}
          >
            <svg
              className="w-4 h-4"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </button>
        </div>
      </div>
    </div>
  );
};

function getAlertClass(errorType: ErrorType): string {
  switch (errorType) {
    case ErrorType.VALIDATION:
      return 'alert-warning';
    case ErrorType.NOT_FOUND:
      return 'alert-info';
    case ErrorType.UNAUTHORIZED:
    case ErrorType.FORBIDDEN:
      return 'alert-warning';
    case ErrorType.CONFLICT:
      return 'alert-warning';
    case ErrorType.BUSINESS_RULE:
      return 'alert-info';
    case ErrorType.NETWORK:
    case ErrorType.TIMEOUT:
      return 'alert-warning';
    default:
      return 'alert-error';
  }
}

function getErrorTitle(errorType: ErrorType): string {
  switch (errorType) {
    case ErrorType.VALIDATION:
      return 'Validation Error';
    case ErrorType.NOT_FOUND:
      return 'Not Found';
    case ErrorType.UNAUTHORIZED:
      return 'Authentication Required';
    case ErrorType.FORBIDDEN:
      return 'Access Denied';
    case ErrorType.CONFLICT:
      return 'Conflict';
    case ErrorType.BUSINESS_RULE:
      return 'Invalid Operation';
    case ErrorType.NETWORK:
      return 'Network Error';
    case ErrorType.TIMEOUT:
      return 'Timeout';
    default:
      return 'Error';
  }
}

export default GlobalErrorDisplay;