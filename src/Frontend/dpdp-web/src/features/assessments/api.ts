import { apiFetch } from "../../lib/apiClient";
import type {
  AssessmentControlQuestionnaire,
  AssessmentDetail,
  AssessmentScore,
  CreateAssessmentPayload,
  PagedAssessments,
  SaveAnswerPayload,
} from "./types";

export interface GetAssessmentsParams {
  page: number;
  pageSize: number;
  search?: string;
  status?: string;
  frameworkVersionId?: string;
  assignedToUserId?: string;
}

export function getAssessments(params: GetAssessmentsParams): Promise<PagedAssessments> {
  const query = new URLSearchParams({ page: String(params.page), pageSize: String(params.pageSize) });
  if (params.search) query.set("search", params.search);
  if (params.status) query.set("status", params.status);
  if (params.frameworkVersionId) query.set("frameworkVersionId", params.frameworkVersionId);
  if (params.assignedToUserId) query.set("assignedToUserId", params.assignedToUserId);
  return apiFetch<PagedAssessments>(`/api/v1/assessments?${query.toString()}`);
}

export function getAssessmentById(id: string): Promise<AssessmentDetail> {
  return apiFetch<AssessmentDetail>(`/api/v1/assessments/${id}`);
}

export function createAssessment(payload: CreateAssessmentPayload): Promise<AssessmentDetail> {
  return apiFetch<AssessmentDetail>("/api/v1/assessments", { method: "POST", body: JSON.stringify(payload) });
}

export function updateAssessment(id: string, payload: { name: string; description?: string | null; dueDate?: string | null }): Promise<AssessmentDetail> {
  return apiFetch<AssessmentDetail>(`/api/v1/assessments/${id}`, { method: "PUT", body: JSON.stringify(payload) });
}

export function assignAssessment(id: string, assignedToUserId: string | null): Promise<AssessmentDetail> {
  return apiFetch<AssessmentDetail>(`/api/v1/assessments/${id}/assign`, {
    method: "POST",
    body: JSON.stringify({ assignedToUserId }),
  });
}

export function getAssessmentQuestionnaire(id: string): Promise<AssessmentControlQuestionnaire[]> {
  return apiFetch<AssessmentControlQuestionnaire[]>(`/api/v1/assessments/${id}/questionnaire`);
}

export function getAssessmentScore(id: string): Promise<AssessmentScore> {
  return apiFetch<AssessmentScore>(`/api/v1/assessments/${id}/score`);
}

export function saveAssessmentAnswer(assessmentControlQuestionId: string, payload: SaveAnswerPayload) {
  return apiFetch(`/api/v1/assessments/answers/${assessmentControlQuestionId}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export function reviewAssessmentAnswer(assessmentControlQuestionId: string, reviewComment: string | null, flagForReview: boolean) {
  return apiFetch(`/api/v1/assessments/answers/${assessmentControlQuestionId}/review`, {
    method: "POST",
    body: JSON.stringify({ reviewComment, flagForReview }),
  });
}

export function submitAssessment(id: string): Promise<AssessmentDetail> {
  return apiFetch<AssessmentDetail>(`/api/v1/assessments/${id}/submit`, { method: "POST" });
}

export function reviewAssessment(id: string, decision: string, comments?: string | null): Promise<AssessmentDetail> {
  return apiFetch<AssessmentDetail>(`/api/v1/assessments/${id}/review`, {
    method: "POST",
    body: JSON.stringify({ decision, comments }),
  });
}

export function approveAssessment(id: string, comments?: string | null): Promise<AssessmentDetail> {
  return apiFetch<AssessmentDetail>(`/api/v1/assessments/${id}/approve`, {
    method: "POST",
    body: JSON.stringify({ comments }),
  });
}

export function rejectAssessment(id: string, comments: string): Promise<AssessmentDetail> {
  return apiFetch<AssessmentDetail>(`/api/v1/assessments/${id}/reject`, {
    method: "POST",
    body: JSON.stringify({ comments }),
  });
}

export function reopenAssessment(id: string): Promise<AssessmentDetail> {
  return apiFetch<AssessmentDetail>(`/api/v1/assessments/${id}/reopen`, { method: "POST" });
}

export function archiveAssessment(id: string): Promise<AssessmentDetail> {
  return apiFetch<AssessmentDetail>(`/api/v1/assessments/${id}/archive`, { method: "POST" });
}
