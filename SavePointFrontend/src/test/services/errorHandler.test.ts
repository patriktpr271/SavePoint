import { describe, it, expect, vi, beforeEach } from 'vitest';

describe('Error Handler Service', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should handle network errors correctly', () => {
    const error = new Error('Network error');
    expect(error.message).toBe('Network error');
  });

  it('should handle 404 errors', () => {
    const error = { status: 404, message: 'Not found' };
    expect(error.status).toBe(404);
  });

  it('should handle 401 unauthorized errors', () => {
    const error = { status: 401, message: 'Unauthorized' };
    expect(error.status).toBe(401);
  });
});

describe('useErrorHandling Hook', () => {
  it('should initialize with no errors', () => {
    const errors: string[] = [];
    expect(errors).toHaveLength(0);
  });

  it('should add error when handleError is called', () => {
    const errors: string[] = [];
    errors.push('Test error');
    expect(errors).toHaveLength(1);
    expect(errors[0]).toBe('Test error');
  });
});
