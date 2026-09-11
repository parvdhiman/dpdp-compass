import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router-dom";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import IconButton from "@mui/material/IconButton";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemText from "@mui/material/ListItemText";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import DeleteIcon from "@mui/icons-material/Delete";
import { useAuth } from "../auth/AuthProvider";
import * as remediationApi from "./api";
import type { EvidenceReference } from "./types";

const NEXT_STATUS: Record<string, string | null> = {
  OPEN: "IN_PROGRESS",
};

export function RemediationTaskDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { hasPermission } = useAuth();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [evidenceDraft, setEvidenceDraft] = useState<EvidenceReference[]>([{ description: "", url: "" }]);
  const [commentDraft, setCommentDraft] = useState("");
  const [verificationNotes, setVerificationNotes] = useState("");

  const { data: task, isPending, isError } = useQuery({
    queryKey: ["remediation-task", id],
    queryFn: () => remediationApi.getRemediationTaskById(id!),
    enabled: !!id,
  });

  const invalidate = () => {
    queryClient.invalidateQueries({ queryKey: ["remediation-task", id] });
    queryClient.invalidateQueries({ queryKey: ["remediation-tasks"] });
  };

  const statusMutation = useMutation({ mutationFn: (status: string) => remediationApi.updateRemediationTaskStatus(id!, status), onSuccess: invalidate });
  const evidenceMutation = useMutation({
    mutationFn: () => remediationApi.addRemediationEvidence(id!, evidenceDraft.filter((e) => e.description || e.url)),
    onSuccess: invalidate,
  });
  const commentMutation = useMutation({
    mutationFn: () => remediationApi.addRemediationComment(id!, commentDraft),
    onSuccess: () => {
      setCommentDraft("");
      invalidate();
    },
  });
  const verifyMutation = useMutation({ mutationFn: () => remediationApi.verifyRemediationTask(id!, verificationNotes || null), onSuccess: invalidate });
  const closeMutation = useMutation({ mutationFn: () => remediationApi.closeRemediationTask(id!), onSuccess: invalidate });

  if (isPending) return <CircularProgress />;
  if (isError || !task) return <Alert severity="error">Could not load this remediation task.</Alert>;

  const canManage = hasPermission("remediation.manage");
  const nextStatus = NEXT_STATUS[task.status];

  return (
    <Stack spacing={3}>
      <Button size="small" onClick={() => navigate(`/findings/${task.findingId}`)} sx={{ alignSelf: "flex-start" }}>
        ← Back to Finding {task.findingNumber}
      </Button>

      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "flex-start", flexWrap: "wrap", gap: 2 }}>
        <Stack spacing={0.5}>
          <Typography variant="h4" component="h1">
            {task.title}
          </Typography>
          <Stack direction="row" spacing={1}>
            <Chip label={task.status.replace("_", " ")} size="small" />
            {task.isOverdue && <Chip label="Overdue" size="small" color="error" />}
          </Stack>
        </Stack>
        {canManage && (
          <Stack direction="row" spacing={1}>
            {nextStatus && (
              <Button variant="outlined" onClick={() => statusMutation.mutate(nextStatus)} disabled={statusMutation.isPending}>
                Mark {nextStatus.replace("_", " ")}
              </Button>
            )}
            {task.status === "VERIFIED" && (
              <Button variant="contained" color="success" onClick={() => closeMutation.mutate()} disabled={closeMutation.isPending}>
                Close
              </Button>
            )}
          </Stack>
        )}
      </Stack>

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack spacing={1}>
          <Typography variant="body2">{task.description}</Typography>
          <Typography variant="caption" color="text.secondary">
            Owner: {task.ownerName ?? "Unassigned"} · Due: {task.dueDate ?? "—"}
          </Typography>
          {task.verifiedAt && (
            <Typography variant="caption" color="success.main">
              Verified by {task.verifiedByName} on {new Date(task.verifiedAt).toLocaleString()}
              {task.verificationNotes ? ` — ${task.verificationNotes}` : ""}
            </Typography>
          )}
        </Stack>
      </Paper>

      <Typography variant="h6">Evidence</Typography>
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack spacing={1}>
          {task.evidence.map((e, i) => (
            <Typography key={i} variant="body2">
              {e.description} {e.url && <a href={e.url} target="_blank" rel="noreferrer">{e.url}</a>}
            </Typography>
          ))}
          {task.evidence.length === 0 && (
            <Typography variant="body2" color="text.secondary">
              No evidence attached yet.
            </Typography>
          )}
          {canManage && task.status !== "VERIFIED" && task.status !== "CLOSED" && (
            <Stack spacing={1}>
              {evidenceDraft.map((item, index) => (
                <Stack key={index} direction="row" spacing={1} sx={{ alignItems: "center" }}>
                  <TextField
                    size="small"
                    label="Description"
                    value={item.description ?? ""}
                    onChange={(e) => setEvidenceDraft((prev) => prev.map((it, i) => (i === index ? { ...it, description: e.target.value } : it)))}
                  />
                  <TextField
                    size="small"
                    label="URL"
                    sx={{ flex: 1 }}
                    value={item.url ?? ""}
                    onChange={(e) => setEvidenceDraft((prev) => prev.map((it, i) => (i === index ? { ...it, url: e.target.value } : it)))}
                  />
                  <IconButton size="small" onClick={() => setEvidenceDraft((prev) => prev.filter((_, i) => i !== index))}>
                    <DeleteIcon fontSize="small" />
                  </IconButton>
                </Stack>
              ))}
              <Stack direction="row" spacing={1}>
                <Button size="small" onClick={() => setEvidenceDraft((prev) => [...prev, { description: "", url: "" }])}>
                  + Add row
                </Button>
                <Button size="small" variant="contained" onClick={() => evidenceMutation.mutate()} disabled={evidenceMutation.isPending}>
                  Save Evidence
                </Button>
              </Stack>
            </Stack>
          )}
        </Stack>
      </Paper>

      {canManage && task.status === "PENDING_VERIFICATION" && (
        <Paper variant="outlined" sx={{ p: 2 }}>
          <Stack spacing={2}>
            <Typography variant="h6">Verify</Typography>
            <TextField label="Verification notes" fullWidth multiline rows={2} value={verificationNotes} onChange={(e) => setVerificationNotes(e.target.value)} />
            <Button variant="contained" sx={{ alignSelf: "flex-start" }} onClick={() => verifyMutation.mutate()} disabled={verifyMutation.isPending}>
              Mark Verified
            </Button>
          </Stack>
        </Paper>
      )}

      <Typography variant="h6">Comments</Typography>
      <List component={Paper} variant="outlined" disablePadding>
        {task.comments.map((c) => (
          <ListItem key={c.id} divider>
            <ListItemText primary={c.comment} secondary={`${c.authorName} — ${new Date(c.createdAt).toLocaleString()}`} />
          </ListItem>
        ))}
        {task.comments.length === 0 && (
          <ListItem>
            <ListItemText primary="No comments yet." />
          </ListItem>
        )}
      </List>
      <Stack direction="row" spacing={1}>
        <TextField label="Add a comment" fullWidth size="small" value={commentDraft} onChange={(e) => setCommentDraft(e.target.value)} />
        <Button variant="outlined" disabled={!commentDraft.trim() || commentMutation.isPending} onClick={() => commentMutation.mutate()}>
          Post
        </Button>
      </Stack>
    </Stack>
  );
}
