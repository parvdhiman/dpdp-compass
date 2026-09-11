import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link as RouterLink, useNavigate, useParams } from "react-router-dom";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemText from "@mui/material/ListItemText";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import { useAuth } from "../auth/AuthProvider";
import * as assessmentsApi from "./api";

function ScoreCard({ label, value }: { label: string; value: number | null }) {
  return (
    <Card variant="outlined" sx={{ minWidth: 160, flex: "1 1 160px" }}>
      <CardContent>
        <Typography variant="body2" color="text.secondary">
          {label}
        </Typography>
        <Typography variant="h4">{value === null ? "—" : `${value.toFixed(0)}%`}</Typography>
      </CardContent>
    </Card>
  );
}

export function AssessmentSummaryPage() {
  const { id } = useParams<{ id: string }>();
  const { hasPermission } = useAuth();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const { data: assessment, isPending, isError } = useQuery({
    queryKey: ["assessment", id],
    queryFn: () => assessmentsApi.getAssessmentById(id!),
    enabled: !!id,
  });

  const { data: score } = useQuery({
    queryKey: ["assessment-score", id],
    queryFn: () => assessmentsApi.getAssessmentScore(id!),
    enabled: !!id,
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["assessment", id] });
    queryClient.invalidateQueries({ queryKey: ["assessments"] });
    queryClient.invalidateQueries({ queryKey: ["assessment-score", id] });
  };

  const submitMutation = useMutation({ mutationFn: () => assessmentsApi.submitAssessment(id!), onSuccess: invalidate });
  const reopenMutation = useMutation({ mutationFn: () => assessmentsApi.reopenAssessment(id!), onSuccess: invalidate });
  const archiveMutation = useMutation({ mutationFn: () => assessmentsApi.archiveAssessment(id!), onSuccess: invalidate });

  if (isPending) return <CircularProgress />;
  if (isError || !assessment) return <Alert severity="error">Could not load this assessment.</Alert>;

  const canEdit = hasPermission("assessments.create");

  return (
    <Stack spacing={3}>
      <Button size="small" onClick={() => navigate("/assessments")} sx={{ alignSelf: "flex-start" }}>
        ← Back to Assessments
      </Button>

      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "flex-start", flexWrap: "wrap", gap: 2 }}>
        <Stack spacing={0.5}>
          <Typography variant="h4" component="h1">
            {assessment.name}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {assessment.frameworkName} ({assessment.frameworkVersionLabel})
          </Typography>
          <Stack direction="row" spacing={1}>
            <Chip label={assessment.status.replace("_", " ")} color="primary" size="small" />
            {assessment.assignedToUserName && <Chip label={`Assigned: ${assessment.assignedToUserName}`} size="small" variant="outlined" />}
            {assessment.dueDate && <Chip label={`Due ${assessment.dueDate}`} size="small" variant="outlined" />}
          </Stack>
        </Stack>

        <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap" }}>
          <Button component={RouterLink} to={`/assessments/${id}/questionnaire`} variant="outlined">
            {canEdit && (assessment.status === "DRAFT" || assessment.status === "IN_PROGRESS") ? "Answer Questions" : "View Questionnaire"}
          </Button>
          {canEdit && (assessment.status === "DRAFT" || assessment.status === "IN_PROGRESS") && (
            <Button variant="contained" onClick={() => submitMutation.mutate()} disabled={submitMutation.isPending}>
              Submit
            </Button>
          )}
          {canEdit && assessment.status === "REJECTED" && (
            <Button variant="outlined" color="warning" onClick={() => reopenMutation.mutate()} disabled={reopenMutation.isPending}>
              Reopen
            </Button>
          )}
          {hasPermission("assessments.review") && (assessment.status === "SUBMITTED" || assessment.status === "UNDER_REVIEW") && (
            <Button component={RouterLink} to={`/assessments/${id}/review`} variant="outlined">
              Review
            </Button>
          )}
          {hasPermission("assessments.approve") && assessment.status === "UNDER_REVIEW" && (
            <Button component={RouterLink} to={`/assessments/${id}/approve`} variant="contained" color="success">
              Approve / Reject
            </Button>
          )}
          {hasPermission("assessments.approve") && assessment.status === "APPROVED" && (
            <Button variant="outlined" onClick={() => archiveMutation.mutate()} disabled={archiveMutation.isPending}>
              Archive
            </Button>
          )}
        </Stack>
      </Stack>

      {submitMutation.isError && <Alert severity="error">Could not submit — check that every required question has been answered.</Alert>}

      <Typography variant="h5" component="h2">
        Compliance Score
      </Typography>
      <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap" }}>
        <ScoreCard label="Overall Score" value={score?.overallScore ?? null} />
        <ScoreCard label="Control Score" value={score?.controlScore ?? null} />
        <ScoreCard label="Risk-Adjusted Score" value={score?.riskAdjustedScore ?? null} />
        <ScoreCard label="Evidence Coverage" value={score?.evidenceCoveragePercent ?? null} />
        <ScoreCard label="Assessment Coverage" value={score?.assessmentCoveragePercent ?? null} />
      </Stack>
      {score && (
        <Typography variant="caption" color="text.secondary">
          {score.answeredRequiredQuestions}/{score.totalRequiredQuestions} required questions answered across {score.applicableControls}/{score.totalControls} applicable controls.
          Scores are figures, not a legal compliance certification — see the platform's compliance framing.
        </Typography>
      )}

      {assessment.scopes.length > 0 && (
        <>
          <Typography variant="h5" component="h2">
            Scope
          </Typography>
          <List component={Paper} variant="outlined" disablePadding>
            {assessment.scopes.map((s) => (
              <ListItem key={s.id} divider>
                <ListItemText primary={[s.businessUnitName, s.departmentName].filter(Boolean).join(" / ") || "Whole organisation"} secondary={s.notes} />
              </ListItem>
            ))}
          </List>
        </>
      )}

      <Typography variant="h5" component="h2">
        Controls
      </Typography>
      <TableContainer component={Paper} variant="outlined">
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Control</TableCell>
              <TableCell>Category</TableCell>
              <TableCell>Risk</TableCell>
              <TableCell>Status</TableCell>
              <TableCell align="right">Questions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {assessment.controls.map((c) => (
              <TableRow key={c.id}>
                <TableCell>
                  {c.controlBusinessId} — {c.controlName}
                </TableCell>
                <TableCell>{c.categoryName}</TableCell>
                <TableCell>{c.riskLevel}</TableCell>
                <TableCell>
                  <Chip label={c.status.replace("_", " ")} size="small" />
                </TableCell>
                <TableCell align="right">
                  {c.answeredQuestionCount}/{c.questionCount}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {assessment.reviews.length > 0 && (
        <>
          <Typography variant="h5" component="h2">
            Review History
          </Typography>
          <List component={Paper} variant="outlined" disablePadding>
            {assessment.reviews.map((r) => (
              <ListItem key={r.id} divider>
                <ListItemText primary={`${r.reviewerName} — ${r.decision.replace("_", " ")}`} secondary={`${r.comments ?? ""} (${new Date(r.createdAt).toLocaleString()})`} />
              </ListItem>
            ))}
          </List>
        </>
      )}

      {assessment.approvals.length > 0 && (
        <>
          <Typography variant="h5" component="h2">
            Approval History
          </Typography>
          <List component={Paper} variant="outlined" disablePadding>
            {assessment.approvals.map((a) => (
              <ListItem key={a.id} divider>
                <ListItemText primary={`${a.decidedByUserName} — ${a.decision}`} secondary={`${a.comments ?? ""} (${new Date(a.createdAt).toLocaleString()})`} />
              </ListItem>
            ))}
          </List>
        </>
      )}
    </Stack>
  );
}
