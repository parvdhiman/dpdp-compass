import { apiFetch } from "../../lib/apiClient";
import type {
  ControlCategory,
  ControlDetail,
  CreateControlPayload,
  Framework,
  FrameworkVersion,
  LegalReference,
  PagedControls,
  PagedEvidenceRequirements,
  PagedQuestions,
  RequirementDetail,
  UpdateControlPayload,
} from "./types";

export function getFrameworks(): Promise<Framework[]> {
  return apiFetch<Framework[]>("/api/v1/compliance/frameworks");
}

export function getFrameworkVersion(id: string): Promise<FrameworkVersion> {
  return apiFetch<FrameworkVersion>(`/api/v1/compliance/framework-versions/${id}`);
}

export function createFrameworkVersion(
  frameworkId: string,
  payload: { versionLabel: string; officialCitation?: string; publicationDate?: string; effectiveDate?: string; sourceUrl?: string; changeSummary?: string },
): Promise<FrameworkVersion> {
  return apiFetch<FrameworkVersion>(`/api/v1/compliance/frameworks/${frameworkId}/versions`, {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export function activateFrameworkVersion(id: string): Promise<void> {
  return apiFetch<void>(`/api/v1/compliance/framework-versions/${id}/activate`, { method: "POST" });
}

export function getLegalReferences(frameworkVersionId?: string): Promise<LegalReference[]> {
  const params = frameworkVersionId ? `?frameworkVersionId=${frameworkVersionId}` : "";
  return apiFetch<LegalReference[]>(`/api/v1/compliance/legal-references${params}`);
}

export function getRequirements(legalReferenceId?: string): Promise<RequirementDetail[]> {
  const params = legalReferenceId ? `?legalReferenceId=${legalReferenceId}` : "";
  return apiFetch<RequirementDetail[]>(`/api/v1/compliance/requirements${params}`);
}

export function getControlCategories(): Promise<ControlCategory[]> {
  return apiFetch<ControlCategory[]>("/api/v1/compliance/control-categories");
}

export interface GetControlsParams {
  page: number;
  pageSize: number;
  search?: string;
  categoryId?: string;
  riskLevel?: string;
  status?: string;
  frameworkVersionId?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export function getControls(params: GetControlsParams): Promise<PagedControls> {
  const query = new URLSearchParams({ page: String(params.page), pageSize: String(params.pageSize) });
  if (params.search) query.set("search", params.search);
  if (params.categoryId) query.set("categoryId", params.categoryId);
  if (params.riskLevel) query.set("riskLevel", params.riskLevel);
  if (params.status) query.set("status", params.status);
  if (params.frameworkVersionId) query.set("frameworkVersionId", params.frameworkVersionId);
  if (params.sortBy) query.set("sortBy", params.sortBy);
  if (params.sortDescending) query.set("sortDescending", "true");
  return apiFetch<PagedControls>(`/api/v1/compliance/controls?${query.toString()}`);
}

export function getControlById(id: string): Promise<ControlDetail> {
  return apiFetch<ControlDetail>(`/api/v1/compliance/controls/${id}`);
}

export function createControl(payload: CreateControlPayload): Promise<ControlDetail> {
  return apiFetch<ControlDetail>("/api/v1/compliance/controls", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export function updateControl(id: string, payload: UpdateControlPayload): Promise<ControlDetail> {
  return apiFetch<ControlDetail>(`/api/v1/compliance/controls/${id}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export function activateControl(id: string): Promise<void> {
  return apiFetch<void>(`/api/v1/compliance/controls/${id}/activate`, { method: "POST" });
}

export function retireControl(id: string): Promise<void> {
  return apiFetch<void>(`/api/v1/compliance/controls/${id}/retire`, { method: "POST" });
}

export interface GetQuestionsParams {
  page: number;
  pageSize: number;
  search?: string;
  controlId?: string;
  questionType?: string;
}

export function getQuestions(params: GetQuestionsParams): Promise<PagedQuestions> {
  const query = new URLSearchParams({ page: String(params.page), pageSize: String(params.pageSize) });
  if (params.search) query.set("search", params.search);
  if (params.controlId) query.set("controlId", params.controlId);
  if (params.questionType) query.set("questionType", params.questionType);
  return apiFetch<PagedQuestions>(`/api/v1/compliance/questions?${query.toString()}`);
}

export interface GetEvidenceRequirementsParams {
  page: number;
  pageSize: number;
  search?: string;
  assessmentQuestionId?: string;
}

export function getEvidenceRequirements(params: GetEvidenceRequirementsParams): Promise<PagedEvidenceRequirements> {
  const query = new URLSearchParams({ page: String(params.page), pageSize: String(params.pageSize) });
  if (params.search) query.set("search", params.search);
  if (params.assessmentQuestionId) query.set("assessmentQuestionId", params.assessmentQuestionId);
  return apiFetch<PagedEvidenceRequirements>(`/api/v1/compliance/evidence-requirements?${query.toString()}`);
}
