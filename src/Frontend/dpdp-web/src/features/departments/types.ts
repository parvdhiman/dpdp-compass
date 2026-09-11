import type { ContactInfo } from "../organisation/types";
import type { PagedResult } from "../users/types";

export interface Department {
  id: string;
  organisationId: string;
  businessUnitId: string;
  businessUnitName: string;
  name: string;
  description: string | null;
  head: ContactInfo;
  isActive: boolean;
  createdAt: string;
}

export type PagedDepartments = PagedResult<Department>;

export interface DepartmentPayload {
  name: string;
  description?: string;
  headName?: string;
  headEmail?: string;
  headPhone?: string;
}
