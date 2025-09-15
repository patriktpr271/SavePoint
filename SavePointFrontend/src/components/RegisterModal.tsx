import React, { useState } from 'react';

interface RegisterModalProps {
  isOpen: boolean;
  onClose: () => void;
}

const RegisterModal: React.FC<RegisterModalProps> = ({ isOpen, onClose }) => {
  const initialFormState = {
    username: '',
    displayName: '',
    email: '',
    password: '',
    confirmPassword: ''
  };

  const [formData, setFormData] = useState(initialFormState);
  const [errors, setErrors] = useState<string[]>([]);

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
      return;
    }

    try {
      const response = await fetch('/api/user/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          username: formData.username,
          displayName: formData.displayName,
          email: formData.email,
          password: formData.password
        }),
      });

      if (response.ok) {
        // Registration successful
        handleClose();
        // You might want to show a success message or automatically log the user in
      } else {
        const data = await response.json();
        // Handle various error formats from the API
        if (data.errors && Array.isArray(data.errors)) {
          // Handle ASP.NET Core Identity error format
          const formattedErrors = data.errors.map((error: string) => {
            // Convert error codes to user-friendly messages
            switch (error) {
              case 'PasswordTooShort':
                return 'Password must be at least 6 characters long';
              case 'PasswordRequiresNonAlphanumeric':
                return 'Password must contain at least one special character';
              case 'PasswordRequiresDigit':
                return 'Password must contain at least one number';
              case 'PasswordRequiresUpper':
                return 'Password must contain at least one uppercase letter';
              default:
                return error;
            }
          });
          setErrors(formattedErrors);
        } else if (typeof data === 'string') {
          setErrors([data]);
        } else if (Array.isArray(data)) {
          setErrors(data);
        } else if (data.message) {
          setErrors([data.message]);
        } else {
          setErrors(['Registration failed. Please try again.']);
        }
      }
    } catch (err) {
      setErrors(['Network error occurred. Please check your connection and try again.']);
    }
  };

  if (!isOpen) return null;

  return (
    <dialog className="modal modal-open modal-backdrop:bg-gray-800/50 modal-backdrop:backdrop-blur-sm">
      <div className="modal-box">
        <h3 className="font-bold text-lg mb-4">Create Account</h3>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="form-control">
            <label className="label">
              <span className="label-text">Username</span>
            </label>
            <input
              type="text"
              name="username"
              value={formData.username}
              onChange={handleInputChange}
              className="input input-bordered w-full"
              placeholder="Enter username"
            />
          </div>

          <div className="form-control">
            <label className="label">
              <span className="label-text">Display Name</span>
            </label>
            <input
              type="text"
              name="displayName"
              value={formData.displayName}
              onChange={handleInputChange}
              className="input input-bordered w-full"
              placeholder="Enter display name"
            />
          </div>

          <div className="form-control">
            <label className="label">
              <span className="label-text">Email</span>
            </label>
            <input
              type="email"
              name="email"
              value={formData.email}
              onChange={handleInputChange}
              className="input input-bordered w-full"
              placeholder="Enter email"
            />
          </div>

          <div className="form-control">
            <label className="label">
              <span className="label-text">Password</span>
            </label>
            <input
              type="password"
              name="password"
              value={formData.password}
              onChange={handleInputChange}
              className="input input-bordered w-full"
              placeholder="Enter password"
            />
          </div>

          <div className="form-control">
            <label className="label">
              <span className="label-text">Confirm Password</span>
            </label>
            <input
              type="password"
              name="confirmPassword"
              value={formData.confirmPassword}
              onChange={handleInputChange}
              className="input input-bordered w-full"
              placeholder="Confirm password"
            />
          </div>

          {errors.length > 0 && (
            <div className="alert alert-error shadow-lg">
              <svg xmlns="http://www.w3.org/2000/svg" className="stroke-current shrink-0 h-6 w-6" fill="none" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
              <div className="flex flex-col">
                {errors.map((error, index) => (
                  <span key={index}>{error}</span>
                ))}
              </div>
            </div>
          )}

          <div className="modal-action">
            <button type="button" className="btn" onClick={handleClose}>
              Cancel
            </button>
            <button type="submit" className="btn btn-primary">
              Register
            </button>
          </div>
        </form>
      </div>
      <form method="dialog" className="modal-backdrop">
        <button onClick={handleClose}>close</button>
      </form>
    </dialog>
  );
};

export default RegisterModal;