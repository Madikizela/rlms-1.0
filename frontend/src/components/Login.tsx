import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import logoImage from '../assets/logo.png';
import backgroundImage from '../assets/background.jpeg';
import {
  EmailIcon,
  LockIcon,
  EyeIcon,
  EyeOffIcon,
  CheckCircleIcon,
  ExclamationCircleIcon,
  ExclamationTriangleIcon,
  SignInIcon,
  InfoCircleIcon
} from './CustomIcons';

const Login: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [emailError, setEmailError] = useState('');
  const [passwordError, setPasswordError] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const navigate = useNavigate();

  const validateEmail = (email: string) => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!email) {
      return 'Email is required';
    }
    if (!emailRegex.test(email)) {
      return 'Please enter a valid email address';
    }
    return '';
  };

  const validatePassword = (password: string) => {
    if (!password) {
      return 'Password is required';
    }
    if (password.length < 6) {
      return 'Password must be at least 6 characters long';
    }
    return '';
  };

  const handleEmailChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setEmail(value);
    setEmailError(validateEmail(value));
  };

  const handlePasswordChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setPassword(value);
    setPasswordError(validatePassword(value));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Clear previous errors
    setError('');
    
    // Validate form
    const emailValidationError = validateEmail(email);
    const passwordValidationError = validatePassword(password);
    
    setEmailError(emailValidationError);
    setPasswordError(passwordValidationError);
    
    if (emailValidationError || passwordValidationError) {
      return;
    }
    
    setIsLoading(true);

    try {
      const response = await fetch('http://localhost:5213/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
      });

      if (response.ok) {
        const data = await response.json();
        
        // Store authentication data
        localStorage.setItem('token', data.token);
        localStorage.setItem('user', JSON.stringify(data.user));
        
        // Navigate to dashboard
        navigate('/dashboard');
      } else {
        const errorData = await response.json();
        setError(errorData.message || 'Login failed');
      }
    } catch (error) {
      setError('Network error. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div 
      className="position-relative"
      style={{
        backgroundImage: `url(${backgroundImage})`,
        backgroundSize: 'cover',
        backgroundPosition: 'center right',
        backgroundRepeat: 'no-repeat',
        width: '100vw',
        height: '100vh',
        margin: 0,
        padding: 0,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center'
      }}
    >

      <div className="position-relative" style={{ width: '100%', maxWidth: '380px', padding: '0 20px' }}>
        <div style={{ width: '100%' }}>
          <div style={{ width: '100%' }}>
            {/* Login Card */}
            <div className="card shadow-lg border-0" style={{ borderRadius: '20px', backgroundColor: 'rgba(255, 255, 255, 0.98)', width: '100%' }}>
              <div className="card-body p-5">
                {/* Logo and Header */}
                <div className="text-center mb-4">
                   <img 
                     src={logoImage} 
                     alt="RLMS Logo" 
                     className="mb-3"
                     style={{ width: '80px', height: '80px' }}
                   />
                  <h2 className="fw-bold text-primary mb-2">RLMS</h2>
                  <p className="text-muted">Remote Learning Management System</p>
                </div>

                {/* Error Alert */}
                {error && (
                  <div className="alert alert-danger" role="alert">
                    <ExclamationTriangleIcon className="me-2" size={18} />
                    {error}
                  </div>
                )}

                {/* Login Form */}
                <form onSubmit={handleSubmit} noValidate>
                  {/* Email Field */}
                  <div className="mb-3">
                    <label htmlFor="email" className="form-label fw-semibold text-dark">
                      <EmailIcon className="me-2" size={16} />
                      Email Address
                    </label>
                    <div className="position-relative">
                      <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                        <EmailIcon className="text-gray-400" size={18} />
                      </div>
                      <input
                        type="email"
                        id="email"
                        className="form-control"
                        placeholder="Enter your email"
                        value={email}
                        onChange={handleEmailChange}
                        style={{
                          borderRadius: '12px',
                          border: emailError ? '2px solid #dc3545' : email && !emailError ? '2px solid #198754' : '2px solid #e0e0e0',
                          backgroundColor: '#f8f9fa',
                          paddingLeft: '50px',
                          fontSize: '16px',
                          transition: 'all 0.3s ease',
                          height: '56px'
                        }}
                        onFocus={(e) => {
                          e.target.style.borderColor = '#4F46E5';
                          e.target.style.boxShadow = '0 0 0 0.2rem rgba(79, 70, 229, 0.25)';
                        }}
                        onBlur={(e) => {
                          e.target.style.borderColor = emailError ? '#dc3545' : email && !emailError ? '#198754' : '#e0e0e0';
                          e.target.style.boxShadow = emailError ? '0 0 0 0.2rem rgba(220, 53, 69, 0.25)' : email && !emailError ? '0 0 0 0.2rem rgba(25, 135, 84, 0.25)' : 'none';
                        }}
                      />
                      {email && !emailError && (
                        <CheckCircleIcon className="position-absolute text-success" 
                           style={{ right: '12px', top: '50%', transform: 'translateY(-50%)', fontSize: '18px' }} size={18} />
                      )}
                      {emailError && (
                        <ExclamationCircleIcon className="position-absolute text-danger" 
                           style={{ right: '12px', top: '50%', transform: 'translateY(-50%)', fontSize: '18px' }} size={18} />
                      )}
                    </div>
                    {emailError && (
                      <div id="email-error" className="invalid-feedback d-block mt-2">
                        <ExclamationTriangleIcon className="me-1" size={14} />
                        {emailError}
                      </div>
                    )}
                  </div>

                  {/* Password Field */}
                  <div className="mb-4">
                    <label htmlFor="password" className="form-label fw-semibold text-dark">
                      <LockIcon className="me-2" size={16} />
                      Password
                    </label>
                    <div className="position-relative">
                      <input
                        type={showPassword ? "text" : "password"}
                        className={`form-control form-control-lg ${passwordError ? 'is-invalid' : password ? 'is-valid' : ''}`}
                        id="password"
                        placeholder="Enter your password"
                        value={password}
                        onChange={handlePasswordChange}
                        required
                        aria-describedby={passwordError ? "password-error" : undefined}
                        style={{ 
                          borderRadius: '12px',
                          border: passwordError ? '2px solid #dc3545' : password && !passwordError ? '2px solid #198754' : '2px solid #e0e0e0',
                          backgroundColor: '#f8f9fa',
                          paddingLeft: '16px',
                          paddingRight: '50px',
                          fontSize: '16px',
                          transition: 'all 0.3s ease',
                          boxShadow: passwordError ? '0 0 0 0.2rem rgba(220, 53, 69, 0.25)' : password && !passwordError ? '0 0 0 0.2rem rgba(25, 135, 84, 0.25)' : 'none'
                        }}
                        onFocus={(e) => {
                          e.target.style.borderColor = '#4F46E5';
                          e.target.style.boxShadow = '0 0 0 0.2rem rgba(79, 70, 229, 0.25)';
                        }}
                        onBlur={(e) => {
                          e.target.style.borderColor = passwordError ? '#dc3545' : password && !passwordError ? '#198754' : '#e0e0e0';
                          e.target.style.boxShadow = passwordError ? '0 0 0 0.2rem rgba(220, 53, 69, 0.25)' : password && !passwordError ? '0 0 0 0.2rem rgba(25, 135, 84, 0.25)' : 'none';
                        }}
                      />
                      <button
                        type="button"
                        className="btn position-absolute"
                        style={{ 
                          right: '8px', 
                          top: '50%', 
                          transform: 'translateY(-50%)',
                          border: 'none',
                          background: 'transparent',
                          color: '#6c757d',
                          fontSize: '18px',
                          padding: '4px 8px'
                        }}
                        onClick={() => setShowPassword(!showPassword)}
                        aria-label={showPassword ? "Hide password" : "Show password"}
                      >
                        {showPassword ? <EyeOffIcon size={18} /> : <EyeIcon size={18} />}
                      </button>
                    </div>
                    {passwordError && (
                      <div id="password-error" className="invalid-feedback d-block mt-2">
                        <ExclamationTriangleIcon className="me-1" size={14} />
                        {passwordError}
                      </div>
                    )}
                  </div>

                  {/* Submit Button */}
                  <div className="d-grid">
                    <button
                      type="submit"
                      className="btn btn-primary btn-lg position-relative"
                      disabled={isLoading || emailError !== '' || passwordError !== ''}
                      style={{
                        borderRadius: '12px',
                        background: isLoading || emailError !== '' || passwordError !== '' 
                          ? 'linear-gradient(135deg, #6c757d 0%, #495057 100%)' 
                          : 'linear-gradient(135deg, #4F46E5 0%, #06B6D4 100%)',
                        border: 'none',
                        fontWeight: '600',
                        fontSize: '16px',
                        padding: '14px 24px',
                        transition: 'all 0.3s ease',
                        boxShadow: '0 4px 15px rgba(79, 70, 229, 0.3)',
                        transform: 'translateY(0)'
                      }}
                      onMouseEnter={(e) => {
                        if (!isLoading && emailError === '' && passwordError === '') {
                          e.currentTarget.style.transform = 'translateY(-2px)';
                          e.currentTarget.style.boxShadow = '0 6px 20px rgba(79, 70, 229, 0.4)';
                        }
                      }}
                      onMouseLeave={(e) => {
                        e.currentTarget.style.transform = 'translateY(0)';
                        e.currentTarget.style.boxShadow = '0 4px 15px rgba(79, 70, 229, 0.3)';
                      }}
                    >
                      {isLoading ? (
                        <>
                          <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
                          Signing In...
                        </>
                      ) : (
                        <>
                          <SignInIcon className="me-2" size={16} />
                          Sign In
                        </>
                      )}
                    </button>
                  </div>
                </form>

                {/* Demo Credentials */}
                <div className="mt-4 p-3 bg-light rounded" style={{ borderRadius: '10px' }}>
                  <h6 className="text-primary mb-2">
                    <InfoCircleIcon className="me-2" size={16} />
                    Demo Credentials
                  </h6>
                  <small className="text-muted d-block">
                    <strong>Email:</strong> admin@system.local
                  </small>
                  <small className="text-muted d-block">
                    <strong>Password:</strong> Admin@123!System
                  </small>
                </div>
              </div>
            </div>

            {/* Footer */}
            <div className="text-center mt-4">
              <p className="text-dark mb-0" style={{ textShadow: '1px 1px 2px rgba(255,255,255,0.8)' }}>
                Don't have an account? 
                <a href="#" className="text-primary ms-1 text-decoration-none fw-bold">Contact administrator</a>
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;