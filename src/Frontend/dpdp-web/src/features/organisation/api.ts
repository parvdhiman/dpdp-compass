import { apiFetch } from "../../lib/apiClient";
import type { OrganisationDashboard, OrganisationLocation, OrganisationProfile } from "./types";

export function getOrganisationProfile(organisationId: string): Promise<OrganisationProfile> {
  return apiFetch<OrganisationProfile>(`/api/v1/organisations/${organisationId}`);
}

export function getOrganisationDashboard(organisationId: string): Promise<OrganisationDashboard> {
  return apiFetch<OrganisationDashboard>(`/api/v1/organisations/${organisationId}/dashboard`);
}

export interface UpdateOrganisationProfilePayload {
  name: string;
  legalName?: string;
  industry?: string;
  size?: string;
  country?: string;
  website?: string;
  primaryContactName?: string;
  primaryContactEmail?: string;
  primaryContactPhone?: string;
  privacyContactName?: string;
  privacyContactEmail?: string;
  privacyContactPhone?: string;
  dpoName?: string;
  dpoEmail?: string;
  dpoPhone?: string;
}

export function updateOrganisationProfile(
  organisationId: string,
  payload: UpdateOrganisationProfilePayload,
): Promise<OrganisationProfile> {
  return apiFetch<OrganisationProfile>(`/api/v1/organisations/${organisationId}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export interface LocationPayload {
  label: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  isPrimary: boolean;
}

export function createLocation(organisationId: string, payload: LocationPayload): Promise<OrganisationLocation> {
  return apiFetch<OrganisationLocation>(`/api/v1/organisations/${organisationId}/locations`, {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export function updateLocation(locationId: string, payload: LocationPayload): Promise<OrganisationLocation> {
  return apiFetch<OrganisationLocation>(`/api/v1/organisations/locations/${locationId}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export function deleteLocation(locationId: string): Promise<void> {
  return apiFetch<void>(`/api/v1/organisations/locations/${locationId}`, { method: "DELETE" });
}
