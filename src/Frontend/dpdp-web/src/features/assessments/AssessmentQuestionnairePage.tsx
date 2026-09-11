import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router-dom";
import Accordion from "@mui/material/Accordion";
import AccordionDetails from "@mui/material/AccordionDetails";
import AccordionSummary from "@mui/material/AccordionSummary";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import Divider from "@mui/material/Divider";
import IconButton from "@mui/material/IconButton";
import MenuItem from "@mui/material/MenuItem";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import DeleteIcon from "@mui/icons-material/Delete";
import * as assessmentsApi from "./api";
import { ANSWER_STATUSES, CONFIDENCE_LEVELS, type AssessmentAnswer, type EvidenceReference } from "./types";

const STATUS_COLORS: Record<string, "default" | "success" | "warning" | "error" | "info"> = {
  PASS: "success",
  PARTIAL: "warning",
  FAIL: "error",
  NOT_APPLICABLE: "default",
  NOT_ASSESSED: "default",
  NEEDS_REVIEW: "info",
};

function QuestionEditor({ question, assessmentId, readOnly }: { question: AssessmentAnswer; assessmentId: string; readOnly: boolean }) {
  const queryClient = useQueryClient();
  const [status, setStatus] = useState(question.status);
  const [answerValue, setAnswerValue] = useState(question.answerValue ?? "");
  const [comment, setComment] = useState(question.comment ?? "");
  const [confidence, setConfidence] = useState(question.confidence ?? "");
  const [remediationNotes, setRemediationNotes] = useState(question.remediationNotes ?? "");
  const [evidence, setEvidence] = useState<EvidenceReference[]>(question.evidence ?? []);
  const [error, setError] = useState<string | null>(null);

  const saveMutation = useMutation({
    mutationFn: () =>
      assessmentsApi.saveAssessmentAnswer(question.assessmentControlQuestionId, {
        status,
        answerValue: answerValue || null,
        comment: comment || null,
        evidence: evidence.filter((e) => e.description || e.url),
        confidence: confidence || null,
        remediationNotes: remediationNotes || null,
      }),
    onSuccess: () => {
      setError(null);
      queryClient.invalidateQueries({ queryKey: ["assessment-questionnaire", assessmentId] });
      queryClient.invalidateQueries({ queryKey: ["assessment", assessmentId] });
      queryClient.invalidateQueries({ queryKey: ["assessment-score", assessmentId] });
    },
    onError: () => setError("Could not save this answer — mandatory evidence may be required."),
  });

  const hasMandatoryEvidence = question.evidenceRequirements.some((e) => e.isMandatory);

  return (
    <Paper variant="outlined" sx={{ p: 2 }}>
      <Stack spacing={1.5}>
        <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
          <Typography variant="subtitle1">{question.questionText}</Typography>
          {question.isRequired && <Chip label="Required" size="small" />}
          <Chip label={status.replace("_", " ")} size="small" color={STATUS_COLORS[status] ?? "default"} />
        </Stack>
        {question.helpText && (
          <Typography variant="body2" color="text.secondary">
            {question.helpText}
          </Typography>
        )}

        {error && <Alert severity="error">{error}</Alert>}

        <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap" }}>
          <TextField select label="Status" size="small" sx={{ minWidth: 180 }} value={status} onChange={(e) => setStatus(e.target.value)} disabled={readOnly}>
            {ANSWER_STATUSES.map((s) => (
              <MenuItem key={s} value={s}>
                {s.replace("_", " ")}
              </MenuItem>
            ))}
          </TextField>
          <TextField select label="Confidence" size="small" sx={{ minWidth: 150 }} value={confidence} onChange={(e) => setConfidence(e.target.value)} disabled={readOnly}>
            <MenuItem value="">Not set</MenuItem>
            {CONFIDENCE_LEVELS.map((c) => (
              <MenuItem key={c} value={c}>
                {c}
              </MenuItem>
            ))}
          </TextField>
        </Stack>

        {question.questionType === "YES_NO" ? (
          <TextField select label="Answer" size="small" sx={{ maxWidth: 200 }} value={answerValue} onChange={(e) => setAnswerValue(e.target.value)} disabled={readOnly}>
            <MenuItem value="true">Yes</MenuItem>
            <MenuItem value="false">No</MenuItem>
          </TextField>
        ) : question.questionType === "MULTIPLE_CHOICE" && question.options ? (
          <TextField select label="Answer" size="small" sx={{ maxWidth: 300 }} value={answerValue} onChange={(e) => setAnswerValue(e.target.value)} disabled={readOnly}>
            {question.options.map((opt) => (
              <MenuItem key={opt} value={opt}>
                {opt}
              </MenuItem>
            ))}
          </TextField>
        ) : (
          <TextField
            label="Answer"
            size="small"
            fullWidth
            multiline={question.questionType === "TEXT"}
            rows={question.questionType === "TEXT" ? 2 : undefined}
            type={question.questionType === "NUMBER" ? "number" : question.questionType === "DATE" ? "date" : "text"}
            slotProps={question.questionType === "DATE" ? { inputLabel: { shrink: true } } : undefined}
            value={answerValue}
            onChange={(e) => setAnswerValue(e.target.value)}
            disabled={readOnly}
          />
        )}

        <TextField label="Comment" size="small" fullWidth multiline rows={2} value={comment} onChange={(e) => setComment(e.target.value)} disabled={readOnly} />
        <TextField label="Remediation notes" size="small" fullWidth value={remediationNotes} onChange={(e) => setRemediationNotes(e.target.value)} disabled={readOnly} />

        <Stack spacing={1}>
          <Typography variant="caption" color="text.secondary">
            Evidence {hasMandatoryEvidence && "(mandatory before marking Pass/Partial)"}
          </Typography>
          {evidence.map((item, index) => (
            <Stack key={index} direction="row" spacing={1} sx={{ alignItems: "center" }}>
              <TextField
                size="small"
                label="Description"
                value={item.description ?? ""}
                onChange={(e) => setEvidence((prev) => prev.map((it, i) => (i === index ? { ...it, description: e.target.value } : it)))}
                disabled={readOnly}
              />
              <TextField
                size="small"
                label="URL"
                sx={{ flex: 1 }}
                value={item.url ?? ""}
                onChange={(e) => setEvidence((prev) => prev.map((it, i) => (i === index ? { ...it, url: e.target.value } : it)))}
                disabled={readOnly}
              />
              {!readOnly && (
                <IconButton size="small" onClick={() => setEvidence((prev) => prev.filter((_, i) => i !== index))}>
                  <DeleteIcon fontSize="small" />
                </IconButton>
              )}
            </Stack>
          ))}
          {!readOnly && (
            <Button size="small" onClick={() => setEvidence((prev) => [...prev, { description: "", url: "" }])} sx={{ alignSelf: "flex-start" }}>
              + Add evidence
            </Button>
          )}
        </Stack>

        {question.reviewComment && (
          <Alert severity="info">
            Reviewer note ({question.reviewerName}): {question.reviewComment}
          </Alert>
        )}

        {!readOnly && (
          <Button variant="contained" size="small" sx={{ alignSelf: "flex-start" }} onClick={() => saveMutation.mutate()} disabled={saveMutation.isPending}>
            {saveMutation.isPending ? "Saving…" : "Save Answer"}
          </Button>
        )}
      </Stack>
    </Paper>
  );
}

export function AssessmentQuestionnairePage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

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

  const readOnly = assessment ? !(assessment.status === "DRAFT" || assessment.status === "IN_PROGRESS") : true;

  if (isPending) return <CircularProgress />;
  if (isError || !controls) return <Alert severity="error">Could not load the questionnaire.</Alert>;

  return (
    <Stack spacing={3}>
      <Button size="small" onClick={() => navigate(`/assessments/${id}`)} sx={{ alignSelf: "flex-start" }}>
        ← Back to Assessment Summary
      </Button>

      <Typography variant="h4" component="h1">
        Questionnaire — {assessment?.name}
      </Typography>

      {readOnly && (
        <Alert severity="info">
          This assessment is {assessment?.status.replace("_", " ").toLowerCase()} — answers can no longer be edited here.
        </Alert>
      )}

      {controls.map((control) => (
        <Accordion key={control.assessmentControlId} defaultExpanded={controls.length <= 3}>
          <AccordionSummary expandIcon={<ExpandMoreIcon />}>
            <Stack direction="row" spacing={1} sx={{ alignItems: "center", width: "100%" }}>
              <Typography sx={{ flex: 1 }}>
                {control.controlBusinessId} — {control.controlName}
              </Typography>
              <Chip label={control.riskLevel} size="small" />
              <Chip label={control.status.replace("_", " ")} size="small" color={STATUS_COLORS[control.status] ?? "default"} />
            </Stack>
          </AccordionSummary>
          <AccordionDetails>
            <Stack spacing={2}>
              {control.questions.map((q, index) => (
                <div key={q.assessmentControlQuestionId}>
                  {index > 0 && <Divider sx={{ mb: 2 }} />}
                  <QuestionEditor question={q} assessmentId={id!} readOnly={readOnly} />
                </div>
              ))}
            </Stack>
          </AccordionDetails>
        </Accordion>
      ))}
    </Stack>
  );
}
