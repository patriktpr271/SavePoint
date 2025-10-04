import React, { useState } from 'react';
import { useAuth } from '../contexts/AuthContext';

interface LoginModalProps {
  isOpen: boolean;
  onClose: () => void;
  onLoginSuccess?: (user: any) => void;
  onSwitchToRegister?: () => void;
}

const LoginModal: React.FC<LoginModalProps> = ({ isOpen, onClose, onLoginSuccess, onSwitchToRegister }) => {
  const { login } = useAuth();
  const initialFormState = {
    email: '',
    password: '',
    rememberMe: false
  };

  const [formData, setFormData] = useState(initialFormState);
  const [errors, setErrors] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);

  const handleClose = () => {
    setFormData(initialFormState);
    setErrors([]);
    onClose();
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type, checked } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? checked : value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setErrors([]);

    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include',
        body: JSON.stringify(formData),
      });

      const data = await response.json();

      if (response.ok) {
        login(data.user);
        handleClose();
        if (onLoginSuccess) {
          onLoginSuccess(data.user);
        }
      } else {
        if (data.message) {
          setErrors([data.message]);
        } else if (data.errors) {
          const modelStateErrors: string[] = [];
          Object.values(data.errors).forEach((errorArray: any) => {
            if (Array.isArray(errorArray)) {
              modelStateErrors.push(...errorArray);
            }
          });
          setErrors(modelStateErrors);
        }
      }
    } catch (error) {
      console.error('Login error:', error);
      setErrors(['An unexpected error occurred. Please try again.']);
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="modal modal-open">
      <div className="modal-box max-w-md mx-auto bg-base-100 shadow-2xl">
        {/* Header */}
        <div className="text-center mb-8">
          <h2 className="text-3xl font-bold text-base-content mb-2">Welcome Back</h2>
          <p className="text-base-content/70">Sign in to your SavePoint account</p>
        </div>
        
        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Email Field */}
          <div className="form-control">
            <label className="label">
              <span className="label-text font-medium">Email Address</span>
            </label>
            <input
              type="email"
              name="email"
              value={formData.email}
              onChange={handleInputChange}
              className="input input-bordered w-full focus:input-primary transition-colors"
              placeholder="Enter your email"
              required
            />
          </div>

          {/* Password Field */}
          <div className="form-control">
            <label className="label">
              <span className="label-text font-medium">Password</span>
            </label>
            <input
              type="password"
              name="password"
              value={formData.password}
              onChange={handleInputChange}
              className="input input-bordered w-full focus:input-primary transition-colors"
              placeholder="Enter your password"
              required
            />
          </div>

          {/* Remember Me */}
          <div className="flex items-center justify-between">
            <label className="cursor-pointer flex items-center gap-2">
              <input
                type="checkbox"
                name="rememberMe"
                checked={formData.rememberMe}
                onChange={handleInputChange}
                className="checkbox checkbox-primary checkbox-sm"
              />
              <span className="label-text">Remember me</span>
            </label>
            <a className="text-primary hover:text-primary-focus text-sm cursor-pointer">
              Forgot password?
            </a>
          </div>

          {/* Error Display */}
          {errors.length > 0 && (
            <div className="alert alert-error">
              <svg xmlns="http://www.w3.org/2000/svg" className="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              <div className="flex flex-col">
                {errors.map((error, index) => (
                  <span key={index}>{error}</span>
                ))}
              </div>
            </div>
          )}

          {/* Submit Button */}
          <button 
            type="submit" 
            className="btn btn-primary w-full py-3 text-lg font-semibold" 
            disabled={loading}
          >
            {loading ? (
              <>
                <span className="loading loading-spinner loading-sm"></span>
                Signing In...
              </>
            ) : (
              'Sign In'
            )}
          </button>

          {/* Divider */}
          <div className="divider my-6">or</div>

          {/* Sign Up Link */}
          <div className="text-center">
            <p className="text-base-content/70">
              Don't have an account?{' '}
              <button
                type="button"
                onClick={() => {
                  if (onSwitchToRegister) {
                    handleClose();
                    onSwitchToRegister();
                  }
                }}
                className="text-primary hover:text-primary-focus font-semibold underline"
              >
                Create one here
              </button>
            </p>
          </div>

          {/* Cancel Button */}
          <div className="modal-action mt-8">
            <button 
              type="button" 
              className="btn btn-ghost w-full" 
              onClick={handleClose} 
              disabled={loading}
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
      <form method="dialog" className="modal-backdrop">
        <button onClick={handleClose}>close</button>
      </form>
    </div>
  );
};

export default LoginModal;
