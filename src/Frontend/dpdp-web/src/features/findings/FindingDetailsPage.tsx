import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Link as RouterLink, useNavigate, useParams } from "react-router-dom";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemButton from "@mui/material/ListItemButton";
import ListItemText from "@mui/material/ListItemText";
import MenuItem from "@mui/material/MenuItem";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { useAuth } from "../auth/AuthProvider";
import * as usersApi from "../users/api";
import * as findingsApi from "./api";
import { IMPACT_LEVELS, LIKELIHOOD_LEVELS } from "./types";

const NEXT_STATUS: Record<string, string | null> = {
  ASSIGNED: "IN_PROGRESS",
  IN_PROGRESS: "PENDING_VERIFICATION",
  PENDING_VERIFICATION: "RESOLVED",
};

function AssignDialog({ onClose, onAssign }: { onClose: () => void; onAssign: (userId: string) => void }) {
  const { data: users } = useQuery({ queryKey: ["users-for-assign"], queryFn: () => usersApi.getUsers(1, 100) });
  const [userId, setUserId] = useState("");

  return (
    <Dialog open onClose={onClose}>
      <DialogTitle>Assign Finding</DialogTitle>
      <DialogContent>
        <TextField select label="Owner" fullWidth sx={{ mt: 1, minWidth: 300 }} value={userId} onChange={(e) => setUserId(e.target.value)}>
          {users?.items.map((u) => (
            <MenuItem key={u.id} value={u.id}>
              {u.fullName}
            </MenuItem>
          ))}
        </TextField>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" disabled={!userId} onClick={() => onAssign(userId)}>
          Assign
        </Button>
      </DialogActions>
    </Dialog>
  );
}

function CreateRiskDialog({ onClose, onCreate }: { onClose: () => void; onCreate: (likelihood: string, impact: string) => void }) {
  const [likelihood, setLikelihood] = useState("POSSIBLE");
  const [impact, setImpact] = useState("MODERATE");

  return (
    <Dialog open onClose={onClose}>
      <DialogTitle>Create Risk from Finding</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ mt: 1, minWidth: 300 }}>
          <TextField select label="Likelihood" value={likelihood} onChange={(e) => setLikelihood(e.target.value)}>
            {LIKELIHOOD_LEVELS.map((l) => (
              <MenuItem key={l} value={l}>
                {l.replace("_", " ")}
              </MenuItem>
            ))}
          </TextField>
          <TextField select label="Impact" value={impact} onChange={(e) => setImpact(e.target.value)}>
            {IMPACT_LEVELS.map((i) => (
              <MenuItem key={i} value={i}>
                {i}
              </MenuItem>
            ))}
          </TextField>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" onClick={() => onCreate(likelihood, impact)}>
          Create
        </Button>
      </DialogActions>
    </Dialog>
  );
}

export function FindingDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const { hasPermission } = useAuth();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [assignOpen, setAssignOpen] = useState(false);
  const [riskDialogOpen, setRiskDialogOpen] = useState(false);
  const [acceptRiskComments, setAcceptRiskComments] = useState("");
  const [showAcceptRisk, setShowAcceptRisk] = useState(false);

  const { data: finding, isPending, isError } = useQuery({
    queryKey: ["finding", id],
    queryFn: () => findingsApi.getFindingById(id!),
    enabled: !!id,
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["finding", id] });

  const assignMutation = useMutation({ mutationFn: (userId: string) => findingsApi.assignFinding(id!, userId), onSuccess: invalidate });
  const statusMutation = useMutation({ mutationFn: (status: string) => findingsApi.updateFindingStatus(id!, status), onSuccess: invalidate });
  const closeMutation = useMutation({ mutationFn: () => findingsApi.closeFinding(id!), onSuccess: invalidate });
  const acceptRiskMutation = useMutation({ mutationFn: (comments: string) => findingsApi.acceptFindingRisk(id!, comments), onSuccess: invalidate });
  const createRiskMutation = useMutation({
    mutationFn: (vars: { likelihood: string; impact: string }) => findingsApi.createRiskFromFinding(id!, vars.likelihood, vars.impact, null),
    onSuccess: invalidate,
  });

  if (isPending) return <CircularProgress />;
  if (isError || !finding) return <Alert severity="error">Could not load this finding.</Alert>;

  const canManage = hasPermission("findings.assign");
  const canClose = hasPermission("findings.close");
  const nextStatus = NEXT_STATUS[finding.status];
  const isActive = !["CLOSED", "ACCEPTED_RISK"].includes(finding.status);

  return (
    <Stack spacing={3}>
      <Button size="small" onClick={() => navigate("/findings")} sx={{ alignSelf: "flex-start" }}>
        ← Back to Finding Dashboard
      </Button>

      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "flex-start", flexWrap: "wrap", gap: 2 }}>
        <Stack spacing={0.5}>
          <Typography variant="h4" component="h1">
            {finding.findingNumber} — {finding.title}
          </Typography>
          <Stack direction="row" spacing={1}>
            <Chip label={finding.severity} size="small" color="error" />
            <Chip label={finding.status.replace("_", " ")} size="small" variant="outlined" />
            {finding.source === "ASSESSMENT" && <Chip label="From Assessment" size="small" />}
          </Stack>
        </Stack>

        {isActive && (
          <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap" }}>
            {canManage && (
              <Button variant="outlined" onClick={() => setAssignOpen(true)}>
                {finding.ownerName ? "Reassign" : "Assign"}
              </Button>
            )}
            {canManage && finding.status === "OPEN" && (
              <Alert severity="info" sx={{ py: 0 }}>
                Assign an owner to progress this finding.
              </Alert>
            )}
            {canManage && nextStatus && (
              <Button variant="outlined" onClick={() => statusMutation.mutate(nextStatus)} disabled={statusMutation.isPending}>
                Mark {nextStatus.replace("_", " ")}
              </Button>
            )}
            {canClose && finding.status === "RESOLVED" && (
              <Button variant="contained" color="success" onClick={() => closeMutation.mutate()} disabled={closeMutation.isPending}>
                Close
              </Button>
            )}
            {canClose && !finding.riskId && (
              <Button variant="outlined" onClick={() => setRiskDialogOpen(true)}>
                Create Risk
              </Button>
            )}
            {canClose && (
              <Button variant="outlined" color="warning" onClick={() => setShowAcceptRisk(true)}>
                Accept Risk
              </Button>
            )}
          </Stack>
        )}
      </Stack>

      {showAcceptRisk && (
        <Paper variant="outlined" sx={{ p: 2 }}>
          <Stack spacing={2}>
            <Typography variant="subtitle1">Accept Risk (terminal — this closes the finding without remediation)</Typography>
            <TextField label="Comments" fullWidth multiline rows={2} value={acceptRiskComments} onChange={(e) => setAcceptRiskComments(e.target.value)} />
            <Stack direction="row" spacing={1}>
              <Button onClick={() => setShowAcceptRisk(false)}>Cancel</Button>
              <Button
                variant="contained"
                color="warning"
                disabled={!acceptRiskComments.trim() || acceptRiskMutation.isPending}
                onClick={() => acceptRiskMutation.mutate(acceptRiskComments)}
              >
                Confirm Accept Risk
              </Button>
            </Stack>
          </Stack>
        </Paper>
      )}

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack spacing={1.5}>
          <Typography variant="body1">{finding.description}</Typography>
          {finding.controlBusinessId && (
            <Typography variant="body2" color="text.secondary">
              Control: {finding.controlBusinessId} — {finding.controlName}
            </Typography>
          )}
          {finding.assetReference && <Typography variant="body2">Asset: {finding.assetReference}</Typography>}
          {finding.recommendation && (
            <>
              <Typography variant="subtitle2">Recommendation</Typography>
              <Typography variant="body2" sx={{ whiteSpace: "pre-wrap" }}>
                {finding.recommendation}
              </Typography>
            </>
          )}
          <Typography variant="caption" color="text.secondary">
            Owner: {finding.ownerName ?? "Unassigned"} · Due: {finding.dueDate ?? "—"}
          </Typography>
          {finding.riskId && (
            <Typography variant="body2">
              Linked risk:{" "}
              <RouterLink to={`/risks/${finding.riskId}`}>
                {finding.riskNumber} ({finding.riskLevel})
              </RouterLink>
            </Typography>
          )}
        </Stack>
      </Paper>

      <Typography variant="h5" component="h2">
        Remediation Tasks
      </Typography>
      <List component={Paper} variant="outlined" disablePadding>
        {finding.remediationTasks.map((t) => (
          <ListItemButton key={t.id} divider component={RouterLink} to={`/remediation-tasks/${t.id}`}>
            <ListItemText primary={t.title} secondary={`${t.status.replace("_", " ")} · ${t.ownerName ?? "Unassigned"} · Due ${t.dueDate ?? "—"}`} />
          </ListItemButton>
        ))}
        {finding.remediationTasks.length === 0 && (
          <ListItem>
            <ListItemText primary="No remediation tasks yet." />
          </ListItem>
        )}
      </List>
      {hasPermission("remediation.manage") && (
        <Button component={RouterLink} to={`/remediation-tasks/new?findingId=${finding.id}`} variant="outlined" sx={{ alignSelf: "flex-start" }}>
          + Create Remediation Task
        </Button>
      )}

      {assignOpen && (
        <AssignDialog
          onClose={() => setAssignOpen(false)}
          onAssign={(userId) => {
            assignMutation.mutate(userId);
            setAssignOpen(false);
          }}
        />
      )}
      {riskDialogOpen && (
        <CreateRiskDialog
          onClose={() => setRiskDialogOpen(false)}
          onCreate={(likelihood, impact) => {
            createRiskMutation.mutate({ likelihood, impact });
            setRiskDialogOpen(false);
          }}
        />
      )}
    </Stack>
  );
}
