import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router-dom";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Checkbox from "@mui/material/Checkbox";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import FormControlLabel from "@mui/material/FormControlLabel";
import MenuItem from "@mui/material/MenuItem";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import * as assessmentsApi from "./api";
import { REVIEW_DECISIONS, type AssessmentAnswer } from "./types";

function ReviewRow({ question, assessmentId }: { question: AssessmentAnswer; assessmentId: string }) {
  const queryClient = useQueryClient();
  const [reviewComment, setReviewComment] = useState(question.reviewComment ?? "");
  const [flag, setFlag] = useState(question.status === "NEEDS_REVIEW");

  const mutation = useMutation({
    mutationFn: () => assessmentsApi.reviewAssessmentAnswer(question.assessmentControlQuestionId, reviewComment || null, flag),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["assessment-questionnaire", assessmentId] });
      queryClient.invalidateQueries({ queryKey: ["assessment", assessmentId] });
    },
  });

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack spacing={1}>
        <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
          <Typography variant="subtitle2" sx={{ flex: 1 }}>
            {question.questionText}
          </Typography>
          <Chip label={question.status.replace("_", " ")} size="small" />
        </Stack>
        <Typography variant="body2" color="text.secondary">
          Answer: {question.answerValue ?? ((question.answerValues ?? []).join(", ") || "—")}
          {question.comment ? ` — "${question.comment}"` : ""}
        </Typography>
        {question.evidence.length > 0 && (
          <Typography variant="caption" color="text.secondary">
            Evidence: {question.evidence.map((e) => e.description || e.url).join("; ")}
          </Typography>
        )}
        <TextField
          label="Reviewer comment"
          size="small"
          fullWidth
          value={reviewComment}
          onChange={(e) => setReviewComment(e.target.value)}
        />
        <FormControlLabel
          control={<Checkbox checked={flag} onChange={(e) => setFlag(e.target.checked)} />}
          label="Flag this answer for review (NEEDS_REVIEW)"
        />
        <Button size="small" variant="outlined" sx={{ alignSelf: "flex-start" }} onClick={() => mutation.mutate()} disabled={mutation.isPending}>
          {mutation.isPending ? "Saving…" : "Save Review Note"}
        </Button>
      </Stack>
    </Paper>
  );
}

export function AssessmentReviewerPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [decision, setDecision] = useState("READY_FOR_APPROVAL");
  const [comments, setComments] = useState("");

  const { data: assessment } = useQuery({
    queryKey: ["assessment", id],
    queryFn: () => assessmentsApi.getAssessmentById(id!),
    enabled: !!id,
  });

  const { data: controls, isPending, isError } = useQuery({
    queryKey: ["assessment-questionnaire", id],
    queryFn: () => assessmentsApi.getAssessmentQuestionnaire(id!),
    enabled: !!id,
  });

  const reviewMutation = useMutation({
    mutationFn: () => assessmentsApi.reviewAssessment(id!, decision, comments || null),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["assessment", id] });
      queryClient.invalidateQueries({ queryKey: ["assessments"] });
      navigate(`/assessments/${id}`);
    },
  });

  if (isPending) return <CircularProgress />;
  if (isError || !controls) return <Alert severity="error">Could not load this assessment for review.</Alert>;

  return (
    <Stack spacing={3}>
      <Button size="small" onClick={() => navigate(`/assessments/${id}`)} sx={{ alignSelf: "flex-start" }}>
        ← Back to Assessment Summary
      </Button>

      <Typography variant="h4" component="h1">
        Reviewing — {assessment?.name}
      </Typography>

      <Stack spacing={2}>
        {controls.flatMap((c) => c.questions).map((q) => (
          <ReviewRow key={q.assessmentControlQuestionId} question={q} assessmentId={id!} />
        ))}
      </Stack>

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack spacing={2}>
          <Typography variant="h6">Submit Review Decision</Typography>
          {reviewMutation.isError && <Alert severity="error">Could not submit this review.</Alert>}
          <TextField select label="Decision" value={decision} onChange={(e) => setDecision(e.target.value)} sx={{ maxWidth: 300 }}>
            {REVIEW_DECISIONS.map((d) => (
              <MenuItem key={d} value={d}>
                {d.replace("_", " ")}
              </MenuItem>
            ))}
          </TextField>
          <TextField label="Comments" fullWidth multiline rows={3} value={comments} onChange={(e) => setComments(e.target.value)} />
          <Button variant="contained" sx={{ alignSelf: "flex-start" }} onClick={() => reviewMutation.mutate()} disabled={reviewMutation.isPending}>
            {reviewMutation.isPending ? "Submitting…" : "Submit Review"}
          </Button>
        </Stack>
      </Paper>
    </Stack>
  );
}
