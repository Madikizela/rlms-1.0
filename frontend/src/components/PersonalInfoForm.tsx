import React, { useState } from 'react';
import {
  EmailIcon,
  BuildingIcon,
  CalendarIcon,
  DocumentIcon,
  GlobeIcon,
  InfoCircleIcon,
  ExclamationCircleIcon,
  ExclamationTriangleIcon
} from './CustomIcons';

interface PersonalInfoFormData {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  dateOfBirth: string;
  address: string;
  department: string;
}

interface PersonalInfoFormProps {
  initialData?: Partial<PersonalInfoFormData>;
  onSubmit: (data: PersonalInfoFormData) => Promise<void>;
  onCancel?: () => void;
}

const PersonalInfoForm: React.FC<PersonalInfoFormProps> = ({
  initialData = {},
  onSubmit,
  onCancel
}) => {
  const [formData, setFormData] = useState<PersonalInfoFormData>({
    firstName: initialData.firstName || '',
    lastName: initialData.lastName || '',
    email: initialData.email || '',
    phoneNumber: initialData.phoneNumber || '',
    dateOfBirth: initialData.dateOfBirth || '',
    address: initialData.address || '',
    department: initialData.department || 'Information Technology'
  });

  const [errors, setErrors] = useState<Partial<PersonalInfoFormData>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isEditing, setIsEditing] = useState(false);

  // Validation functions
  const validateEmail = (email: string): string => {
    if (!email) return 'Email is required';
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) return 'Please enter a valid email address';
    return '';
  };

  const validatePhoneNumber = (phone: string): string => {
    if (!phone) return '';
    const phoneRegex = /^[\+]?[1-9][\d]{0,15}$/;
    if (!phoneRegex.test(phone.replace(/[\s\-\(\)]/g, ''))) {
      return 'Please enter a valid phone number';
    }
    return '';
  };

  const validateName = (name: string, fieldName: string): string => {
    if (!name.trim()) return `${fieldName} is required`;
    if (name.trim().length < 2) return `${fieldName} must be at least 2 characters`;
    if (!/^[a-zA-Z\s]+$/.test(name)) return `${fieldName} can only contain letters and spaces`;
    return '';
  };

  const validateDateOfBirth = (date: string): string => {
    if (!date) return '';
    const birthDate = new Date(date);
    const today = new Date();
    const age = today.getFullYear() - birthDate.getFullYear();
    
    if (birthDate > today) return 'Date of birth cannot be in the future';
    if (age < 16) return 'You must be at least 16 years old';
    if (age > 100) return 'Please enter a valid date of birth';
    return '';
  };

  const handleInputChange = (field: keyof PersonalInfoFormData, value: string) => {
    setFormData(prev => ({ ...prev, [field]: value }));
    
    // Clear error when user starts typing
    if (errors[field]) {
      setErrors(prev => ({ ...prev, [field]: '' }));
    }
  };

  const validateForm = (): boolean => {
    const newErrors: Partial<PersonalInfoFormData> = {};

    newErrors.firstName = validateName(formData.firstName, 'First name');
    newErrors.lastName = validateName(formData.lastName, 'Last name');
    newErrors.email = validateEmail(formData.email);
    newErrors.phoneNumber = validatePhoneNumber(formData.phoneNumber);
    newErrors.dateOfBirth = validateDateOfBirth(formData.dateOfBirth);

    // Remove empty error messages
    Object.keys(newErrors).forEach(key => {
      if (!newErrors[key as keyof PersonalInfoFormData]) {
        delete newErrors[key as keyof PersonalInfoFormData];
      }
    });

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) return;

    setIsSubmitting(true);
    try {
      await onSubmit(formData);
      setIsEditing(false);
    } catch (error) {
      console.error('Error submitting form:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  const toggleEdit = () => {
    if (isEditing && onCancel) {
      onCancel();
    }
    setIsEditing(!isEditing);
    setErrors({});
  };

  return (
    <div className="bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden">
      <div className="bg-gradient-to-r from-gray-50 to-blue-50 px-8 py-6 border-b border-gray-200">
        <div className="flex items-center justify-between">
          <div>
            <h3 className="text-2xl font-bold text-gray-800">Personal Information</h3>
            <p className="text-gray-600 mt-1">Update your personal details and contact information</p>
          </div>
          <div className="flex space-x-3">
            <button
              type="button"
              onClick={toggleEdit}
              className={`group relative px-6 py-3 rounded-xl transition-all duration-300 shadow-lg hover:shadow-xl transform hover:-translate-y-1 flex items-center space-x-2 font-medium border ${
                isEditing
                  ? 'bg-gradient-to-r from-gray-500 to-gray-600 hover:from-gray-600 hover:to-gray-700 text-white border-gray-400 hover:border-gray-300'
                  : 'bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800 text-white border-blue-500 hover:border-blue-400'
              }`}
            >
              <div className="absolute inset-0 bg-white opacity-0 group-hover:opacity-10 rounded-xl transition-opacity duration-300"></div>
              {isEditing ? (
                <>
                  <svg className="w-5 h-5 mr-2 relative z-10" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd"/>
                  </svg>
                  <span className="relative z-10">Cancel</span>
                </>
              ) : (
                <>
                  <svg className="w-5 h-5 mr-2 relative z-10" fill="currentColor" viewBox="0 0 20 20">
                    <path d="M13.586 3.586a2 2 0 112.828 2.828l-.793.793-2.828-2.828.793-.793zM11.379 5.793L3 14.172V17h2.828l8.38-8.379-2.83-2.828z"/>
                  </svg>
                  <span className="relative z-10">Edit Profile</span>
                </>
              )}
            </button>
          </div>
        </div>
      </div>

      <form onSubmit={handleSubmit} className="p-8">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
          {/* First Name */}
          <div className="space-y-2">
            <label className="block text-sm font-bold text-gray-800 mb-2 flex items-center">
              <DocumentIcon className="w-4 h-4 mr-2 text-blue-600" />
              First Name *
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                <DocumentIcon className="w-5 h-5 text-gray-400" />
              </div>
              <input
                type="text"
                value={formData.firstName}
                onChange={(e) => handleInputChange('firstName', e.target.value)}
                className={`w-full pl-12 pr-5 py-4 border-2 rounded-2xl focus:ring-3 focus:ring-blue-200 focus:border-blue-500 transition-all duration-300 text-gray-800 font-medium placeholder-gray-500 hover:border-gray-400 ${
                  isEditing 
                    ? errors.firstName 
                      ? 'border-red-300 bg-red-50 focus:border-red-500 focus:ring-red-200' 
                      : 'border-gray-300 bg-white'
                    : 'border-gray-300 bg-gray-50'
                }`}
                placeholder="Enter your first name"
                readOnly={!isEditing}
                disabled={!isEditing}
              />
            </div>
            {errors.firstName && (
              <p className="text-red-600 text-sm mt-1 flex items-center">
                <ExclamationCircleIcon className="w-4 h-4 mr-1" />
                {errors.firstName}
              </p>
            )}
          </div>

          {/* Last Name */}
          <div className="space-y-2">
            <label className="block text-sm font-bold text-gray-800 mb-2 flex items-center">
              <DocumentIcon className="w-4 h-4 mr-2 text-blue-600" />
              Last Name *
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                <DocumentIcon className="w-5 h-5 text-gray-400" />
              </div>
              <input
                type="text"
                value={formData.lastName}
                onChange={(e) => handleInputChange('lastName', e.target.value)}
                className={`w-full pl-12 pr-5 py-4 border-2 rounded-2xl focus:ring-3 focus:ring-blue-200 focus:border-blue-500 transition-all duration-300 text-gray-800 font-medium placeholder-gray-500 hover:border-gray-400 ${
                  isEditing 
                    ? errors.lastName 
                      ? 'border-red-300 bg-red-50 focus:border-red-500 focus:ring-red-200' 
                      : 'border-gray-300 bg-white'
                    : 'border-gray-300 bg-gray-50'
                }`}
                placeholder="Enter your last name"
                readOnly={!isEditing}
                disabled={!isEditing}
              />
            </div>
            {errors.lastName && (
              <p className="text-red-600 text-sm mt-1 flex items-center">
                <ExclamationCircleIcon className="w-4 h-4 mr-1" />
                {errors.lastName}
              </p>
            )}
          </div>

          {/* Email Address */}
          <div className="md:col-span-2 space-y-2">
            <label className="block text-sm font-bold text-gray-800 mb-2 flex items-center">
              <EmailIcon className="w-4 h-4 mr-2 text-blue-600" />
              Email Address *
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                <EmailIcon className="w-5 h-5 text-gray-400" />
              </div>
              <input
                type="email"
                value={formData.email}
                onChange={(e) => handleInputChange('email', e.target.value)}
                className={`w-full pl-12 pr-5 py-4 border-2 rounded-2xl focus:ring-3 focus:ring-blue-200 focus:border-blue-500 transition-all duration-300 text-gray-800 font-medium placeholder-gray-500 hover:border-gray-400 ${
                  isEditing 
                    ? errors.email 
                      ? 'border-red-300 bg-red-50 focus:border-red-500 focus:ring-red-200' 
                      : 'border-gray-300 bg-white'
                    : 'border-gray-300 bg-gray-50'
                }`}
                placeholder="Enter your email address"
                readOnly={!isEditing}
                disabled={!isEditing}
              />
            </div>
            {errors.email && (
              <p className="text-red-600 text-sm mt-1 flex items-center">
                <ExclamationCircleIcon className="w-4 h-4 mr-1" />
                {errors.email}
              </p>
            )}
          </div>

          {/* Phone Number */}
          <div className="space-y-2">
            <label className="block text-sm font-bold text-gray-800 mb-2 flex items-center">
              <GlobeIcon className="w-4 h-4 mr-2 text-blue-600" />
              Phone Number
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                <GlobeIcon className="w-5 h-5 text-gray-400" />
              </div>
              <input
                type="tel"
                value={formData.phoneNumber}
                onChange={(e) => handleInputChange('phoneNumber', e.target.value)}
                className={`w-full pl-12 pr-5 py-4 border-2 rounded-2xl focus:ring-3 focus:ring-blue-200 focus:border-blue-500 transition-all duration-300 text-gray-800 font-medium placeholder-gray-500 hover:border-gray-400 ${
                  isEditing 
                    ? errors.phoneNumber 
                      ? 'border-red-300 bg-red-50 focus:border-red-500 focus:ring-red-200' 
                      : 'border-gray-300 bg-white'
                    : 'border-gray-300 bg-gray-50'
                }`}
                placeholder="Enter your phone number"
                readOnly={!isEditing}
                disabled={!isEditing}
              />
            </div>
            {errors.phoneNumber && (
              <p className="text-red-600 text-sm mt-1 flex items-center">
                <ExclamationCircleIcon className="w-4 h-4 mr-1" />
                {errors.phoneNumber}
              </p>
            )}
          </div>

          {/* Date of Birth */}
          <div className="space-y-2">
            <label className="block text-sm font-bold text-gray-800 mb-2 flex items-center">
              <CalendarIcon className="w-4 h-4 mr-2 text-blue-600" />
              Date of Birth
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                <CalendarIcon className="w-5 h-5 text-gray-400" />
              </div>
              <input
                type="date"
                value={formData.dateOfBirth}
                onChange={(e) => handleInputChange('dateOfBirth', e.target.value)}
                className={`w-full pl-12 pr-5 py-4 border-2 rounded-2xl focus:ring-3 focus:ring-blue-200 focus:border-blue-500 transition-all duration-300 text-gray-800 font-medium placeholder-gray-500 hover:border-gray-400 ${
                  isEditing 
                    ? errors.dateOfBirth 
                      ? 'border-red-300 bg-red-50 focus:border-red-500 focus:ring-red-200' 
                      : 'border-gray-300 bg-white'
                    : 'border-gray-300 bg-gray-50'
                }`}
                readOnly={!isEditing}
                disabled={!isEditing}
              />
            </div>
            {errors.dateOfBirth && (
              <p className="text-red-600 text-sm mt-1 flex items-center">
                <ExclamationCircleIcon className="w-4 h-4 mr-1" />
                {errors.dateOfBirth}
              </p>
            )}
          </div>

          {/* Address */}
          <div className="md:col-span-2 space-y-2">
            <label className="block text-sm font-bold text-gray-800 mb-2 flex items-center">
              <GlobeIcon className="w-4 h-4 mr-2 text-blue-600" />
              Address
            </label>
            <div className="relative">
              <div className="absolute top-4 left-0 pl-4 flex items-start pointer-events-none">
                <GlobeIcon className="w-5 h-5 text-gray-400 mt-1" />
              </div>
              <textarea
                rows={4}
                value={formData.address}
                onChange={(e) => handleInputChange('address', e.target.value)}
                className={`w-full pl-12 pr-5 py-4 border-2 rounded-2xl focus:ring-3 focus:ring-blue-200 focus:border-blue-500 transition-all duration-300 text-gray-800 font-medium placeholder-gray-500 hover:border-gray-400 resize-none ${
                  isEditing 
                    ? 'border-gray-300 bg-white'
                    : 'border-gray-300 bg-gray-50'
                }`}
                placeholder="Enter your full address"
                readOnly={!isEditing}
                disabled={!isEditing}
              />
            </div>
          </div>

          {/* Department */}
          <div className="space-y-2">
            <label className="block text-sm font-bold text-gray-800 mb-2 flex items-center">
              <BuildingIcon className="w-4 h-4 mr-2 text-blue-600" />
              Department
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                <BuildingIcon className="w-5 h-5 text-gray-400" />
              </div>
              <select
                value={formData.department}
                onChange={(e) => handleInputChange('department', e.target.value)}
                className={`w-full pl-12 pr-5 py-4 border-2 rounded-2xl focus:ring-3 focus:ring-blue-200 focus:border-blue-500 transition-all duration-300 text-gray-800 font-medium hover:border-gray-400 appearance-none ${
                  isEditing 
                    ? 'border-gray-300 bg-white'
                    : 'border-gray-300 bg-gray-50'
                }`}
                disabled={!isEditing}
              >
                <option value="Information Technology">Information Technology</option>
                <option value="Business Administration">Business Administration</option>
                <option value="Engineering">Engineering</option>
                <option value="Healthcare">Healthcare</option>
                <option value="Finance">Finance</option>
                <option value="Marketing">Marketing</option>
                <option value="Human Resources">Human Resources</option>
              </select>
              <div className="absolute inset-y-0 right-0 pr-4 flex items-center pointer-events-none">
                <svg className="w-5 h-5 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clipRule="evenodd"/>
                </svg>
              </div>
            </div>
          </div>
        </div>

        {/* Save Button - Only visible when editing */}
        {isEditing && (
          <div className="flex justify-end mt-8 pt-6 border-t border-gray-200">
            <button
              type="submit"
              disabled={isSubmitting}
              className="px-8 py-3 bg-gradient-to-r from-green-500 to-green-600 hover:from-green-600 hover:to-green-700 disabled:from-gray-400 disabled:to-gray-500 text-white font-medium rounded-xl transition-all duration-300 shadow-lg hover:shadow-xl transform hover:-translate-y-1 disabled:transform-none flex items-center space-x-2"
            >
              {isSubmitting ? (
                <>
                  <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  Saving...
                </>
              ) : (
                <>
                  <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd"/>
                  </svg>
                  <span>Save Changes</span>
                </>
              )}
            </button>
          </div>
        )}
      </form>
    </div>
  );
};

export default PersonalInfoForm;