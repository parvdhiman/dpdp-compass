export interface ContactInfo {
  name: string | null;
  email: string | null;
  phone: string | null;
}

export interface OrganisationLocation {
  id: string;
  label: string;
  addressLine1: string | null;
  addressLine2: string | null;
  city: string | null;
  state: string | null;
  postalCode: string | null;
  country: string | null;
  isPrimary: boolean;
}

export interface OrganisationProfile {
  id: string;
  name: string;
  legalName: string | null;
  status: string;
  industry: string | null;
  size: string | null;
  country: string | null;
  website: string | null;
  primaryContact: ContactInfo;
  privacyContact: ContactInfo;
  dpoContact: ContactInfo;
  locations: OrganisationLocation[];
  createdAt: string;
}

export interface OrganisationDashboard {
  organisationId: string;
  name: string;
  industry: string | null;
  size: string | null;
  businessUnitCount: number;
  departmentCount: number;
  activeUserCount: number;
  primaryLocation: OrganisationLocation | null;
  hasDpoConfigured: boolean;
}

export interface OrganisationSummary {
  id: string;
  name: string;
  legalName: string | null;
  status: string;
  industry: string | null;
  size: string | null;
  country: string | null;
  createdAt: string;
}

export const ORGANISATION_SIZES = ["Micro", "Small", "Medium", "Large", "Enterprise"] as const;
