import type { PagedResult } from "../users/types";

export const RISK_LEVELS = ["LOW", "MEDIUM", "HIGH", "CRITICAL"] as const;
export const RISK_STATUSES = ["OPEN", "MITIGATING", "ACCEPTED", "CLOSED"] as const;
export const LIKELIHOOD_LEVELS = ["RARE", "UNLIKELY", "POSSIBLE", "LIKELY", "ALMOST_CERTAIN"] as const;
export const IMPACT_LEVELS = ["NEGLIGIBLE", "MINOR", "MODERATE", "MAJOR", "SEVERE"] as const;

export interface RiskSummary {
  id: string;
  riskNumber: string;
  title: string;
  likelihood: string;
  impact: string;
  dataSensitivity: string;
  exposure: string;
  calculatedRiskLevel: string;
  calculatedRiskScore: number;
  status: string;
  ownerUserId: string | null;
  ownerName: string | null;
  createdAt: string;
  findingCount: number;
}

export interface LinkedFinding {
  id: string;
  findingNumber: string;
  title: string;
  severity: string;
  status: string;
}

export interface RiskDetail extends RiskSummary {
  description: string;
  treatmentPlan: string | null;
  updatedAt: string;
  findings: LinkedFinding[];
}

export type PagedRisks = PagedResult<RiskSummary>;

export interface RiskPayload {
  title: string;
  description: string;
  likelihood: string;
  impact: string;
  dataSensitivity: string;
  exposure: string;
  ownerUserId?: string | null;
  treatmentPlan?: string | null;
}
