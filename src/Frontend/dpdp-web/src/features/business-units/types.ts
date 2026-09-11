import type { ContactInfo } from "../organisation/types";
import type { PagedResult } from "../users/types";

export interface BusinessUnit {
  id: string;
  organisationId: string;
  name: string;
  description: string | null;
  head: ContactInfo;
  isActive: boolean;
  departmentCount: number;
  createdAt: string;
}

export type PagedBusinessUnits = PagedResult<BusinessUnit>;

export interface BusinessUnitPayload {
  name: string;
  description?: string;
  headName?: string;
  headEmail?: string;
  headPhone?: string;
}
