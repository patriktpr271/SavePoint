import React, { useState } from 'react';
import { Eye, EyeOff } from 'lucide-react';

interface RegisterModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSwitchToLogin?: () => void;
}

const RegisterModal: React.FC<RegisterModalProps> = ({ isOpen, onClose, onSwitchToLogin }) => {
  const initialFormState = {
    username: '',
    displayName: '',
    email: '',
    password: '',
    confirmPassword: ''
  };

  const [formData, setFormData] = useState(initialFormState);
  const [errors, setErrors] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const handleClose = () => {
    setFormData(initialFormState);
    setErrors([]);
    onClose();
  };

  const validatePassword = (password: string): string[] => {
    const validationErrors: string[] = [];
    
    if (password.length < 6) {
      validationErrors.push("Password must be at least 6 characters long");
    }
    if (!/[A-Z]/.test(password)) {
      validationErrors.push("Password must contain at least one uppercase letter");
    }
    if (!/[0-9]/.test(password)) {
      validationErrors.push("Password must contain at least one number");
    }
    if (!/[^A-Za-z0-9]/.test(password)) {
      validationErrors.push("Password must contain at least one special character");
    }
    
    return validationErrors;
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
    setErrors([]); // Clear errors when user types
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    
    // Basic validation
    const newErrors: string[] = [];

    if (!formData.username || !formData.displayName || !formData.email || !formData.password || !formData.confirmPassword) {
      newErrors.push('All fields are required');
    }

    if (formData.password !== formData.confirmPassword) {
      newErrors.push('Passwords do not match');
    }

    // Validate password requirements
    if (formData.password) {
      const passwordErrors = validatePassword(formData.password);
      newErrors.push(...passwordErrors);
    }

    if (newErrors.length > 0) {
      setErrors(newErrors);
      setLoading(false);
      return;
    }

    try {
      const response = await fetch('/api/auth/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        credentials: 'include', // Important for cookie-based auth
        body: JSON.stringify({
          userName: formData.username, // Match backend property name
          displayName: formData.displayName,
          email: formData.email,
          password: formData.password
        }),
      });

      if (response.ok) {
        handleClose();
        // TODO: Show success message
      } else {
        const data = await response.json();
        // Handle ModelState errors from backend
        if (data.errors) {
          const modelStateErrors: string[] = [];
          Object.values(data.errors).forEach((errorArray: any) => {
            if (Array.isArray(errorArray)) {
              modelStateErrors.push(...errorArray);
            }
          });
          setErrors(modelStateErrors);
        } else if (data.message) {
          setErrors([data.message]);
        } else {
          setErrors(['Registration failed. Please try again.']);
        }
      }
    } catch (err) {
      console.error('Registration error:', err);
      setErrors(['An unexpected error occurred. Please try again.']);
    } finally {
      setLoading(false);
    }
  };

  if (!isOpen) return null;

  return (
    <div className="modal modal-open">
      <div className="modal-box max-w-md mx-auto bg-gradient-to-br from-base-100 to-base-200 shadow-2xl border border-base-300">
        {/* Header */}
        <div className="text-center mb-8">
          <div className="w-16 h-16 mx-auto mb-4 bg-gradient-to-r from-orange-500 to-emerald-500 rounded-full flex items-center justify-center shadow-lg">
            <span className="text-2xl font-bold text-white">SP</span>
          </div>
          <h2 className="text-3xl font-bold bg-gradient-to-r from-orange-500 to-emerald-500 bg-clip-text text-transparent mb-2">Join SavePoint</h2>
          <p className="text-base-content/70">Create your account to start your gaming journey</p>
        </div>
        
        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Username Field */}
          <div className="form-control">
            <label className="label">
              <span className="label-text font-semibold text-base-content/80">Username</span>
            </label>
            <input
              type="text"
              name="username"
              value={formData.username}
              onChange={handleInputChange}
              className="input input-bordered w-full focus:border-orange-500 focus:ring-2 focus:ring-orange-200 bg-gray-700 transition-all duration-200 hover:bg-gray-600"
              placeholder="Choose a username"
              required
            />
          </div>

          {/* Display Name Field */}
          <div className="form-control">
            <label className="label">
              <span className="label-text font-semibold text-base-content/80">Display Name</span>
            </label>
            <input
              type="text"
              name="displayName"
              value={formData.displayName}
              onChange={handleInputChange}
              className="input input-bordered w-full focus:border-orange-500 focus:ring-2 focus:ring-orange-200 bg-gray-700 transition-all duration-200 hover:bg-gray-600"
              placeholder="Your display name"
              required
            />
          </div>

          {/* Email Field */}
          <div className="form-control">
            <label className="label">
              <span className="label-text font-semibold text-base-content/80">Email Address</span>
            </label>
            <input
              type="email"
              name="email"
              value={formData.email}
              onChange={handleInputChange}
              className="input input-bordered w-full focus:border-orange-500 focus:ring-2 focus:ring-orange-200 bg-gray-700 transition-all duration-200 hover:bg-gray-600"
              placeholder="Enter your email"
              required
            />
          </div>

          {/* Password Field */}
          <div className="form-control">
            <label className="label">
              <span className="label-text font-semibold text-base-content/80">Password</span>
            </label>
            <div className="relative">
              <input
                type={showPassword ? "text" : "password"}
                name="password"
                value={formData.password}
                onChange={handleInputChange}
                className="input input-bordered w-full pr-12 focus:border-orange-500 focus:ring-2 focus:ring-orange-200 bg-gray-700 transition-all duration-200 hover:bg-gray-600"
                placeholder="Create a strong password"
                required
              />
              <span
                onClick={() => setShowPassword(!showPassword)}
                className="absolute right-3 top-1/2 transform -translate-y-1/2 text-base-content/60 hover:text-base-content transition-colors cursor-pointer"
              >
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </span>
            </div>
          </div>

          {/* Confirm Password Field */}
          <div className="form-control">
            <label className="label">
              <span className="label-text font-semibold text-base-content/80">Confirm Password</span>
            </label>
            <div className="relative">
              <input
                type={showConfirmPassword ? "text" : "password"}
                name="confirmPassword"
                value={formData.confirmPassword}
                onChange={handleInputChange}
                className="input input-bordered w-full pr-12 focus:border-orange-500 focus:ring-2 focus:ring-orange-200 bg-gray-700 transition-all duration-200 hover:bg-gray-600"
                placeholder="Confirm your password"
                required
              />
              <span
                onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                className="absolute right-3 top-1/2 transform -translate-y-1/2 text-base-content/60 hover:text-base-content transition-colors cursor-pointer"
              >
                {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </span>
            </div>
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
            className="btn bg-gradient-to-r from-orange-500 to-emerald-500 hover:from-orange-600 hover:to-emerald-600 border-0 text-white w-full py-3 text-lg font-semibold shadow-lg hover:shadow-xl transition-all duration-200 transform hover:-translate-y-0.5" 
            disabled={loading}
          >
            {loading ? (
              <>
                <span className="loading loading-spinner loading-sm"></span>
                Creating Account...
              </>
            ) : (
              'Create Account'
            )}
          </button>

          {/* Divider */}
          <div className="divider my-6 text-base-content/50">or</div>

          {/* Sign In Link */}
          <div className="text-center bg-base-200/50 rounded-lg p-4 border border-base-300/50">
            <p className="text-base-content/70">
              Already have an account?{' '}
              <button
                type="button"
                onClick={() => {
                  if (onSwitchToLogin) {
                    handleClose();
                    onSwitchToLogin();
                  }
                }}
                className="text-orange-600 hover:text-orange-700 font-semibold underline decoration-transparent hover:decoration-current transition-all"
              >
                Sign in here
              </button>
            </p>
          </div>

          {/* Cancel Button */}
          <div className="modal-action mt-8">
            <button 
              type="button" 
              className="btn btn-ghost w-full hover:bg-base-200 transition-colors" 
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

export default RegisterModal;