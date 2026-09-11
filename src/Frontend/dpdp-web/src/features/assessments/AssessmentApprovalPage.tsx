import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router-dom";
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
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import * as assessmentsApi from "./api";

export function AssessmentApprovalPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [comments, setComments] = useState("");
  const [rejectComments, setRejectComments] = useState("");
  const [validationError, setValidationError] = useState<string | null>(null);

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

  const afterDecision = () => {
    queryClient.invalidateQueries({ queryKey: ["assessment", id] });
    queryClient.invalidateQueries({ queryKey: ["assessments"] });
    navigate(`/assessments/${id}`);
  };

  const approveMutation = useMutation({
    mutationFn: () => assessmentsApi.approveAssessment(id!, comments || null),
    onSuccess: afterDecision,
  });

  const rejectMutation = useMutation({
    mutationFn: () => assessmentsApi.rejectAssessment(id!, rejectComments),
    onSuccess: afterDecision,
  });

  const handleReject = () => {
    if (!rejectComments.trim()) {
      setValidationError("Rejection requires a comment explaining what needs to change.");
      return;
    }
    setValidationError(null);
    rejectMutation.mutate();
  };

  if (isPending) return <CircularProgress />;
  if (isError || !assessment) return <Alert severity="error">Could not load this assessment for approval.</Alert>;

  return (
    <Stack spacing={3} sx={{ maxWidth: 800 }}>
      <Button size="small" onClick={() => navigate(`/assessments/${id}`)} sx={{ alignSelf: "flex-start" }}>
        ← Back to Assessment Summary
      </Button>

      <Typography variant="h4" component="h1">
        Approve or Reject — {assessment.name}
      </Typography>
      <Chip label={assessment.status.replace("_", " ")} size="small" sx={{ alignSelf: "flex-start" }} />

      <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap" }}>
        <Card variant="outlined" sx={{ minWidth: 160 }}>
          <CardContent>
            <Typography variant="body2" color="text.secondary">
              Overall Score
            </Typography>
            <Typography variant="h4">{score?.overallScore === undefined || score.overallScore === null ? "—" : `${score.overallScore.toFixed(0)}%`}</Typography>
          </CardContent>
        </Card>
        <Card variant="outlined" sx={{ minWidth: 160 }}>
          <CardContent>
            <Typography variant="body2" color="text.secondary">
              Evidence Coverage
            </Typography>
            <Typography variant="h4">
              {score?.evidenceCoveragePercent === undefined || score.evidenceCoveragePercent === null ? "—" : `${score.evidenceCoveragePercent.toFixed(0)}%`}
            </Typography>
          </CardContent>
        </Card>
      </Stack>

      {assessment.reviews.length > 0 && (
        <>
          <Typography variant="h6">Reviewer Feedback</Typography>
          <List component={Paper} variant="outlined" disablePadding>
            {assessment.reviews.map((r) => (
              <ListItem key={r.id} divider>
                <ListItemText primary={`${r.reviewerName} — ${r.decision.replace("_", " ")}`} secondary={r.comments} />
              </ListItem>
            ))}
          </List>
        </>
      )}

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack spacing={2}>
          <Typography variant="h6" color="success.main">
            Approve
          </Typography>
          {approveMutation.isError && <Alert severity="error">Could not approve this assessment.</Alert>}
          <TextField label="Approval comments (optional)" fullWidth multiline rows={2} value={comments} onChange={(e) => setComments(e.target.value)} />
          <Button variant="contained" color="success" sx={{ alignSelf: "flex-start" }} onClick={() => approveMutation.mutate()} disabled={approveMutation.isPending}>
            {approveMutation.isPending ? "Approving…" : "Approve Assessment"}
          </Button>
        </Stack>
      </Paper>

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack spacing={2}>
          <Typography variant="h6" color="error.main">
            Reject
          </Typography>
          {validationError && <Alert severity="warning">{validationError}</Alert>}
          {rejectMutation.isError && <Alert severity="error">Could not reject this assessment.</Alert>}
          <TextField
            label="Rejection comments (required)"
            fullWidth
            multiline
            rows={2}
            value={rejectComments}
            onChange={(e) => setRejectComments(e.target.value)}
          />
          <Button variant="outlined" color="error" sx={{ alignSelf: "flex-start" }} onClick={handleReject} disabled={rejectMutation.isPending}>
            {rejectMutation.isPending ? "Rejecting…" : "Reject Assessment"}
          </Button>
        </Stack>
      </Paper>
    </Stack>
  );
}
