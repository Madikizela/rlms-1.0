import React, { useState } from 'react';
import { CheckCircleIcon, ExclamationCircleIcon, ExclamationTriangleIcon } from './CustomIcons';

interface ClientFormData {
  clientName: string;
  vatNumber: string;
  businessDescription: string;
  businessSector: string;
  contractNumber: string;
  clientAddress: string;
  emailAddress: string;
  websiteLink: string;
  attendanceType: string;
  logo?: File;
}

interface ClientFormErrors {
  clientName?: string;
  vatNumber?: string;
  businessDescription?: string;
  businessSector?: string;
  contractNumber?: string;
  clientAddress?: string;
  emailAddress?: string;
  websiteLink?: string;
  attendanceType?: string;
  logo?: string;
}

interface ClientFormProps {
  onCancel: () => void;
  onSubmit: (data: ClientFormData) => void;
}

const ClientForm: React.FC<ClientFormProps> = ({ onCancel, onSubmit }) => {
  const [formData, setFormData] = useState<ClientFormData>({
    clientName: '',
    vatNumber: '',
    businessDescription: '',
    businessSector: '',
    contractNumber: '',
    clientAddress: '',
    emailAddress: '',
    websiteLink: '',
    attendanceType: '',
  });

  const [errors, setErrors] = useState<ClientFormErrors>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [logoPreview, setLogoPreview] = useState<string | null>(null);

  const validateField = (name: keyof ClientFormData, value: string | File | undefined): string => {
    switch (name) {
      case 'clientName':
        if (!value || (typeof value === 'string' && value.trim().length < 2)) {
          return 'Client name must be at least 2 characters long';
        }
        if (typeof value === 'string' && value.trim().length > 100) {
          return 'Client name cannot exceed 100 characters';
        }
        break;

      case 'emailAddress':
        if (!value || typeof value !== 'string') {
          return 'Email address is required';
        }
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(value)) {
          return 'Please enter a valid email address';
        }
        break;

      case 'vatNumber':
        if (value && typeof value === 'string' && value.trim()) {
          const vatRegex = /^[A-Z0-9]{8,15}$/;
          if (!vatRegex.test(value.replace(/\s/g, ''))) {
            return 'VAT number should be 8-15 alphanumeric characters';
          }
        }
        break;

      case 'websiteLink':
        if (value && typeof value === 'string' && value.trim()) {
          try {
            new URL(value);
          } catch {
            return 'Please enter a valid website URL';
          }
        }
        break;

      case 'businessDescription':
        if (value && typeof value === 'string' && value.trim().length > 500) {
          return 'Business description cannot exceed 500 characters';
        }
        break;

      case 'clientAddress':
        if (value && typeof value === 'string' && value.trim().length > 200) {
          return 'Address cannot exceed 200 characters';
        }
        break;

      case 'logo':
        if (value && value instanceof File) {
          if (value.size > 10 * 1024 * 1024) { // 10MB
            return 'Logo file size cannot exceed 10MB';
          }
          if (!value.type.startsWith('image/')) {
            return 'Logo must be an image file';
          }
        }
        break;
    }
    return '';
  };

  const handleInputChange = (name: keyof ClientFormData, value: string) => {
    setFormData(prev => ({ ...prev, [name]: value }));
    
    // Clear error when user starts typing
    if (errors[name]) {
      setErrors(prev => ({ ...prev, [name]: undefined }));
    }

    // Validate field on change
    const error = validateField(name, value);
    if (error) {
      setErrors(prev => ({ ...prev, [name]: error }));
    }
  };

  const handleFileChange = (file: File | null) => {
    if (file) {
      const error = validateField('logo', file);
      if (error) {
        setErrors(prev => ({ ...prev, logo: error }));
        return;
      }

      setFormData(prev => ({ ...prev, logo: file }));
      setErrors(prev => ({ ...prev, logo: undefined }));

      // Create preview
      const reader = new FileReader();
      reader.onload = (e) => {
        setLogoPreview(e.target?.result as string);
      };
      reader.readAsDataURL(file);
    } else {
      setFormData(prev => ({ ...prev, logo: undefined }));
      setLogoPreview(null);
    }
  };

  const validateForm = (): boolean => {
    const newErrors: ClientFormErrors = {};
    
    // Validate required fields
    Object.entries(formData).forEach(([key, value]) => {
      if (key === 'clientName' || key === 'emailAddress') {
        const error = validateField(key as keyof ClientFormData, value);
        if (error) {
          newErrors[key as keyof ClientFormErrors] = error;
        }
      }
    });

    // Validate optional fields that have values
    Object.entries(formData).forEach(([key, value]) => {
      if (key !== 'clientName' && key !== 'emailAddress' && value) {
        const error = validateField(key as keyof ClientFormData, value);
        if (error) {
          newErrors[key as keyof ClientFormErrors] = error;
        }
      }
    });

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!validateForm()) {
      return;
    }

    setIsSubmitting(true);
    
    try {
      await onSubmit(formData);
    } catch (error) {
      console.error('Error submitting form:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  const getFieldClassName = (fieldName: keyof ClientFormErrors, hasValue: boolean) => {
    const baseClass = "w-full pl-12 pr-5 py-4 border-2 rounded-2xl focus:ring-3 focus:ring-blue-200 focus:border-blue-500 transition-all duration-300 bg-white text-gray-800 font-medium placeholder-gray-500 hover:border-gray-400";
    
    if (errors[fieldName]) {
      return `${baseClass} border-red-500 focus:border-red-500 focus:ring-red-200`;
    }
    
    if (hasValue && !errors[fieldName]) {
      return `${baseClass} border-green-500 focus:border-green-500 focus:ring-green-200`;
    }
    
    return `${baseClass} border-gray-300`;
  };

  return (
    <div className="bg-white rounded-2xl shadow-xl border border-gray-100 overflow-hidden">
      <div className="bg-gradient-to-r from-blue-50 to-indigo-50 px-8 py-6 border-b border-gray-200">
        <h3 className="text-2xl font-bold text-gray-800">Client Information</h3>
        <p className="text-gray-600 mt-1">Enter all client details and information</p>
      </div>
      
      <form onSubmit={handleSubmit} className="p-8" noValidate>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
          {/* Client Name */}
          <div className="space-y-2">
            <label htmlFor="clientName" className="block text-sm font-bold text-gray-800 mb-2 flex items-center">
              <svg className="w-4 h-4 mr-2 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M4 4a2 2 0 00-2 2v8a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2H4zm0 2h12v8H4V6z" clipRule="evenodd"/>
              </svg>
              Client Name *
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                <svg className="w-5 h-5 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M4 4a2 2 0 00-2 2v8a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2H4zm0 2h12v8H4V6z" clipRule="evenodd"/>
                </svg>
              </div>
              <input
                type="text"
                id="clientName"
                className={getFieldClassName('clientName', !!formData.clientName)}
                placeholder="Enter client name"
                value={formData.clientName}
                onChange={(e) => handleInputChange('clientName', e.target.value)}
                required
                aria-describedby={errors.clientName ? "clientName-error" : undefined}
              />
              {formData.clientName && !errors.clientName && (
                <CheckCircleIcon className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-green-500" />
              )}
              {errors.clientName && (
                <ExclamationCircleIcon className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-red-500" />
              )}
            </div>
            {errors.clientName && (
              <div id="clientName-error" className="text-red-600 text-sm mt-2 flex items-center">
                <ExclamationTriangleIcon className="w-4 h-4 mr-1" />
                {errors.clientName}
              </div>
            )}
          </div>

          {/* VAT Registration No */}
          <div className="space-y-2">
            <label htmlFor="vatNumber" className="block text-sm font-bold text-gray-800 mb-2 flex items-center">
              <svg className="w-4 h-4 mr-2 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
                <path d="M4 4a2 2 0 00-2 2v1h16V6a2 2 0 00-2-2H4zM18 9H2v5a2 2 0 002 2h12a2 2 0 002-2V9zM4 13a1 1 0 011-1h1a1 1 0 110 2H5a1 1 0 01-1-1zm5-1a1 1 0 100 2h1a1 1 0 100-2H9z"/>
              </svg>
              VAT Registration No.
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                <svg className="w-5 h-5 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                  <path d="M4 4a2 2 0 00-2 2v1h16V6a2 2 0 00-2-2H4zM18 9H2v5a2 2 0 002 2h12a2 2 0 002-2V9zM4 13a1 1 0 011-1h1a1 1 0 110 2H5a1 1 0 01-1-1zm5-1a1 1 0 100 2h1a1 1 0 100-2H9z"/>
                </svg>
              </div>
              <input
                type="text"
                id="vatNumber"
                className={getFieldClassName('vatNumber', !!formData.vatNumber)}
                placeholder="Enter VAT number"
                value={formData.vatNumber}
                onChange={(e) => handleInputChange('vatNumber', e.target.value)}
                aria-describedby={errors.vatNumber ? "vatNumber-error" : undefined}
              />
              {formData.vatNumber && !errors.vatNumber && (
                <CheckCircleIcon className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-green-500" />
              )}
              {errors.vatNumber && (
                <ExclamationCircleIcon className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-red-500" />
              )}
            </div>
            {errors.vatNumber && (
              <div id="vatNumber-error" className="text-red-600 text-sm mt-2 flex items-center">
                <ExclamationTriangleIcon className="w-4 h-4 mr-1" />
                {errors.vatNumber}
              </div>
            )}
          </div>

          {/* Email Address */}
          <div className="space-y-2">
            <label htmlFor="emailAddress" className="block text-sm font-bold text-gray-800 mb-2 flex items-center">
              <svg className="w-4 h-4 mr-2 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
                <path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z"/>
                <path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z"/>
              </svg>
              Email Address *
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                <svg className="w-5 h-5 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                  <path d="M2.003 5.884L10 9.882l7.997-3.998A2 2 0 0016 4H4a2 2 0 00-1.997 1.884z"/>
                  <path d="M18 8.118l-8 4-8-4V14a2 2 0 002 2h12a2 2 0 002-2V8.118z"/>
                </svg>
              </div>
              <input
                type="email"
                id="emailAddress"
                className={getFieldClassName('emailAddress', !!formData.emailAddress)}
                placeholder="Enter email address"
                value={formData.emailAddress}
                onChange={(e) => handleInputChange('emailAddress', e.target.value)}
                required
                aria-describedby={errors.emailAddress ? "emailAddress-error" : undefined}
              />
              {formData.emailAddress && !errors.emailAddress && (
                <CheckCircleIcon className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-green-500" />
              )}
              {errors.emailAddress && (
                <ExclamationCircleIcon className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-red-500" />
              )}
            </div>
            {errors.emailAddress && (
              <div id="emailAddress-error" className="text-red-600 text-sm mt-2 flex items-center">
                <ExclamationTriangleIcon className="w-4 h-4 mr-1" />
                {errors.emailAddress}
              </div>
            )}
          </div>

          {/* Business Sector */}
          <div className="space-y-2">
            <label htmlFor="businessSector" className="block text-sm font-bold text-gray-800 mb-2 flex items-center">
              <svg className="w-4 h-4 mr-2 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M6 2a2 2 0 00-2 2v12a2 2 0 002 2h8a2 2 0 002-2V4a2 2 0 00-2-2H6zm1 2a1 1 0 000 2h6a1 1 0 100-2H7z" clipRule="evenodd"/>
              </svg>
              Business Sector
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                <svg className="w-5 h-5 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M6 2a2 2 0 00-2 2v12a2 2 0 002 2h8a2 2 0 002-2V4a2 2 0 00-2-2H6zm1 2a1 1 0 000 2h6a1 1 0 100-2H7z" clipRule="evenodd"/>
                </svg>
              </div>
              <input
                type="text"
                id="businessSector"
                className={getFieldClassName('businessSector', !!formData.businessSector)}
                placeholder="Enter business sector"
                value={formData.businessSector}
                onChange={(e) => handleInputChange('businessSector', e.target.value)}
                aria-describedby={errors.businessSector ? "businessSector-error" : undefined}
              />
              {formData.businessSector && !errors.businessSector && (
                <CheckCircleIcon className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-green-500" />
              )}
            </div>
            {errors.businessSector && (
              <div id="businessSector-error" className="text-red-600 text-sm mt-2 flex items-center">
                <ExclamationTriangleIcon className="w-4 h-4 mr-1" />
                {errors.businessSector}
              </div>
            )}
          </div>

          {/* Contract Number */}
          <div className="space-y-2">
            <label htmlFor="contractNumber" className="block text-sm font-bold text-gray-800 mb-2 flex items-center">
              <svg className="w-4 h-4 mr-2 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4zm2 6a1 1 0 011-1h6a1 1 0 110 2H7a1 1 0 01-1-1zm1 3a1 1 0 100 2h6a1 1 0 100-2H7z" clipRule="evenodd"/>
              </svg>
              Contract Number
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                <svg className="w-5 h-5 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M4 4a2 2 0 012-2h4.586A2 2 0 0112 2.586L15.414 6A2 2 0 0116 7.414V16a2 2 0 01-2 2H6a2 2 0 01-2-2V4z" clipRule="evenodd"/>
                </svg>
              </div>
              <input
                type="text"
                id="contractNumber"
                className={getFieldClassName('contractNumber', !!formData.contractNumber)}
                placeholder="Enter contract number"
                value={formData.contractNumber}
                onChange={(e) => handleInputChange('contractNumber', e.target.value)}
                aria-describedby={errors.contractNumber ? "contractNumber-error" : undefined}
              />
              {formData.contractNumber && !errors.contractNumber && (
                <CheckCircleIcon className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-green-500" />
              )}
            </div>
            {errors.contractNumber && (
              <div id="contractNumber-error" className="text-red-600 text-sm mt-2 flex items-center">
                <ExclamationTriangleIcon className="w-4 h-4 mr-1" />
                {errors.contractNumber}
              </div>
            )}
          </div>

          {/* Website Link */}
          <div className="space-y-2">
            <label htmlFor="websiteLink" className="block text-sm font-bold text-gray-800 mb-2 flex items-center">
              <svg className="w-4 h-4 mr-2 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M4.083 9h1.946c.089-1.546.383-2.97.837-4.118A6.004 6.004 0 004.083 9zM10 2a8 8 0 100 16 8 8 0 000-16z" clipRule="evenodd"/>
              </svg>
              Website Link
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                <svg className="w-5 h-5 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M4.083 9h1.946c.089-1.546.383-2.97.837-4.118A6.004 6.004 0 004.083 9zM10 2a8 8 0 100 16 8 8 0 000-16z" clipRule="evenodd"/>
                </svg>
              </div>
              <input
                type="url"
                id="websiteLink"
                className={getFieldClassName('websiteLink', !!formData.websiteLink)}
                placeholder="https://example.com"
                value={formData.websiteLink}
                onChange={(e) => handleInputChange('websiteLink', e.target.value)}
                aria-describedby={errors.websiteLink ? "websiteLink-error" : undefined}
              />
              {formData.websiteLink && !errors.websiteLink && (
                <CheckCircleIcon className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-green-500" />
              )}
              {errors.websiteLink && (
                <ExclamationCircleIcon className="absolute right-3 top-1/2 transform -translate-y-1/2 w-5 h-5 text-red-500" />
              )}
            </div>
            {errors.websiteLink && (
              <div id="websiteLink-error" className="text-red-600 text-sm mt-2 flex items-center">
                <ExclamationTriangleIcon className="w-4 h-4 mr-1" />
                {errors.websiteLink}
              </div>
            )}
          </div>

          {/* Business Description */}
          <div className="md:col-span-2 space-y-2">
            <label htmlFor="businessDescription" className="block text-sm font-bold text-gray-800 mb-2 flex items-center">
              <svg className="w-4 h-4 mr-2 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M3 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1z" clipRule="evenodd"/>
              </svg>
              Business Description
              <span className="ml-2 text-xs text-gray-500">({formData.businessDescription.length}/500)</span>
            </label>
            <div className="relative">
              <div className="absolute top-4 left-4 pointer-events-none">
                <svg className="w-5 h-5 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M3 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zm0 4a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1z" clipRule="evenodd"/>
                </svg>
              </div>
              <textarea
                id="businessDescription"
                rows={4}
                className={`w-full pl-12 pr-5 py-4 border-2 rounded-2xl focus:ring-3 focus:ring-blue-200 focus:border-blue-500 transition-all duration-300 bg-white text-gray-800 font-medium placeholder-gray-500 hover:border-gray-400 resize-none ${
                  errors.businessDescription ? 'border-red-500 focus:border-red-500 focus:ring-red-200' : 
                  formData.businessDescription && !errors.businessDescription ? 'border-green-500 focus:border-green-500 focus:ring-green-200' : 
                  'border-gray-300'
                }`}
                placeholder="Describe the client's business..."
                value={formData.businessDescription}
                onChange={(e) => handleInputChange('businessDescription', e.target.value)}
                maxLength={500}
                aria-describedby={errors.businessDescription ? "businessDescription-error" : undefined}
              />
            </div>
            {errors.businessDescription && (
              <div id="businessDescription-error" className="text-red-600 text-sm mt-2 flex items-center">
                <ExclamationTriangleIcon className="w-4 h-4 mr-1" />
                {errors.businessDescription}
              </div>
            )}
          </div>

          {/* Client Address */}
          <div className="md:col-span-2 space-y-2">
            <label htmlFor="clientAddress" className="block text-sm font-bold text-gray-800 mb-2 flex items-center">
              <svg className="w-4 h-4 mr-2 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clipRule="evenodd"/>
              </svg>
              Client Address
              <span className="ml-2 text-xs text-gray-500">({formData.clientAddress.length}/200)</span>
            </label>
            <div className="relative">
              <div className="absolute top-4 left-4 pointer-events-none">
                <svg className="w-5 h-5 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M5.05 4.05a7 7 0 119.9 9.9L10 18.9l-4.95-4.95a7 7 0 010-9.9zM10 11a2 2 0 100-4 2 2 0 000 4z" clipRule="evenodd"/>
                </svg>
              </div>
              <textarea
                id="clientAddress"
                rows={3}
                className={`w-full pl-12 pr-5 py-4 border-2 rounded-2xl focus:ring-3 focus:ring-blue-200 focus:border-blue-500 transition-all duration-300 bg-white text-gray-800 font-medium placeholder-gray-500 hover:border-gray-400 resize-none ${
                  errors.clientAddress ? 'border-red-500 focus:border-red-500 focus:ring-red-200' : 
                  formData.clientAddress && !errors.clientAddress ? 'border-green-500 focus:border-green-500 focus:ring-green-200' : 
                  'border-gray-300'
                }`}
                placeholder="Enter client address..."
                value={formData.clientAddress}
                onChange={(e) => handleInputChange('clientAddress', e.target.value)}
                maxLength={200}
                aria-describedby={errors.clientAddress ? "clientAddress-error" : undefined}
              />
            </div>
            {errors.clientAddress && (
              <div id="clientAddress-error" className="text-red-600 text-sm mt-2 flex items-center">
                <ExclamationTriangleIcon className="w-4 h-4 mr-1" />
                {errors.clientAddress}
              </div>
            )}
          </div>

          {/* Attendance Type */}
          <div className="space-y-2">
            <label htmlFor="attendanceType" className="block text-sm font-bold text-gray-800 mb-2 flex items-center">
              <svg className="w-4 h-4 mr-2 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" clipRule="evenodd"/>
              </svg>
              Attendance Type
            </label>
            <div className="relative">
              <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                <svg className="w-5 h-5 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1z" clipRule="evenodd"/>
                </svg>
              </div>
              <select 
                id="attendanceType"
                className={`w-full pl-12 pr-12 py-4 border-2 rounded-2xl focus:ring-3 focus:ring-blue-200 focus:border-blue-500 transition-all duration-300 bg-white text-gray-800 font-medium hover:border-gray-400 appearance-none ${
                  errors.attendanceType ? 'border-red-500 focus:border-red-500 focus:ring-red-200' : 
                  formData.attendanceType && !errors.attendanceType ? 'border-green-500 focus:border-green-500 focus:ring-green-200' : 
                  'border-gray-300'
                }`}
                value={formData.attendanceType}
                onChange={(e) => handleInputChange('attendanceType', e.target.value)}
                aria-describedby={errors.attendanceType ? "attendanceType-error" : undefined}
              >
                <option value="">Select attendance type</option>
                <option value="full-time">Full Time</option>
                <option value="part-time">Part Time</option>
                <option value="remote">Remote</option>
                <option value="hybrid">Hybrid</option>
                <option value="contract">Contract</option>
              </select>
              <div className="absolute inset-y-0 right-0 pr-4 flex items-center pointer-events-none">
                <svg className="w-5 h-5 text-gray-400" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z" clipRule="evenodd"/>
                </svg>
              </div>
            </div>
            {errors.attendanceType && (
              <div id="attendanceType-error" className="text-red-600 text-sm mt-2 flex items-center">
                <ExclamationTriangleIcon className="w-4 h-4 mr-1" />
                {errors.attendanceType}
              </div>
            )}
          </div>

          {/* Client Logo */}
          <div className="space-y-2">
            <label className="block text-sm font-bold text-gray-800 mb-2 flex items-center">
              <svg className="w-4 h-4 mr-2 text-blue-600" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M4 3a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V5a2 2 0 00-2-2H4zm12 12H4l4-8 3 6 2-4 3 6z" clipRule="evenodd"/>
              </svg>
              Client Logo
            </label>
            <div className="relative">
              <div className={`border-2 border-dashed rounded-2xl p-6 text-center transition-colors duration-300 ${
                errors.logo ? 'border-red-300 bg-red-50' : 'border-gray-300 hover:border-blue-400'
              }`}>
                {logoPreview ? (
                  <div className="space-y-4">
                    <img src={logoPreview} alt="Logo preview" className="mx-auto h-20 w-20 object-contain rounded-lg" />
                    <div className="flex justify-center space-x-2">
                      <button
                        type="button"
                        onClick={() => handleFileChange(null)}
                        className="px-3 py-1 text-sm bg-red-100 text-red-700 rounded-lg hover:bg-red-200 transition-colors"
                      >
                        Remove
                      </button>
                      <label htmlFor="logo-upload" className="px-3 py-1 text-sm bg-blue-100 text-blue-700 rounded-lg hover:bg-blue-200 transition-colors cursor-pointer">
                        Change
                      </label>
                    </div>
                  </div>
                ) : (
                  <>
                    <svg className="mx-auto h-12 w-12 text-gray-400" stroke="currentColor" fill="none" viewBox="0 0 48 48">
                      <path d="M28 8H12a4 4 0 00-4 4v20m32-12v8m0 0v8a4 4 0 01-4 4H12a4 4 0 01-4-4v-4m32-4l-3.172-3.172a4 4 0 00-5.656 0L28 28M8 32l9.172-9.172a4 4 0 015.656 0L28 28m0 0l4 4m4-24h8m-4-4v8m-12 4h.02" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
                    </svg>
                    <div className="mt-4">
                      <label htmlFor="logo-upload" className="cursor-pointer">
                        <span className="mt-2 block text-sm font-medium text-gray-900">
                          Click to upload logo or drag and drop
                        </span>
                        <span className="mt-1 block text-xs text-gray-500">
                          PNG, JPG, GIF up to 10MB
                        </span>
                      </label>
                    </div>
                  </>
                )}
                <input 
                  id="logo-upload" 
                  name="logo-upload" 
                  type="file" 
                  className="sr-only" 
                  accept="image/*"
                  onChange={(e) => handleFileChange(e.target.files?.[0] || null)}
                />
              </div>
            </div>
            {errors.logo && (
              <div className="text-red-600 text-sm mt-2 flex items-center">
                <i className="bi bi-exclamation-triangle me-1"></i>
                {errors.logo}
              </div>
            )}
          </div>
        </div>

        {/* Action Buttons */}
        <div className="flex justify-end space-x-4 mt-8 pt-6 border-t border-gray-200">
          <button
            type="button"
            onClick={onCancel}
            className="px-8 py-3 bg-gray-500 hover:bg-gray-600 text-white font-medium rounded-xl transition-all duration-300 shadow-lg hover:shadow-xl transform hover:-translate-y-1"
            disabled={isSubmitting}
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={isSubmitting || Object.keys(errors).length > 0}
            className={`px-8 py-3 font-medium rounded-xl transition-all duration-300 shadow-lg hover:shadow-xl transform hover:-translate-y-1 flex items-center space-x-2 ${
              isSubmitting || Object.keys(errors).length > 0
                ? 'bg-gray-400 cursor-not-allowed text-white'
                : 'bg-gradient-to-r from-green-500 to-green-600 hover:from-green-600 hover:to-green-700 text-white'
            }`}
          >
            {isSubmitting ? (
              <>
                <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                </svg>
                Adding Client...
              </>
            ) : (
              <>
                <svg className="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
                  <path fillRule="evenodd" d="M10 3a1 1 0 011 1v5h5a1 1 0 110 2h-5v5a1 1 0 11-2 0v-5H4a1 1 0 110-2h5V4a1 1 0 011-1z" clipRule="evenodd"/>
                </svg>
                <span>Add Client</span>
              </>
            )}
          </button>
        </div>
      </form>
    </div>
  );
};

export default ClientForm;