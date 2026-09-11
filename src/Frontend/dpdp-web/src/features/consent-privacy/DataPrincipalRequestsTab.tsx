import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Checkbox from "@mui/material/Checkbox";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import FormControlLabel from "@mui/material/FormControlLabel";
import Grid from "@mui/material/Grid";
import MenuItem from "@mui/material/MenuItem";
import Pagination from "@mui/material/Pagination";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { useAuth } from "../auth/AuthProvider";
import { getUsers } from "../users/api";
import * as api from "./api";
import { DATA_PRINCIPAL_REQUEST_STATUSES, DATA_PRINCIPAL_REQUEST_TYPES, type CreateDataPrincipalRequestPayload } from "./types";

const PAGE_SIZE = 20;

const STATUS_COLORS: Record<string, "default" | "info" | "warning" | "success" | "error"> = {
  REQUESTED: "default", IDENTITY_VERIFICATION: "info", IN_PROGRESS: "info",
  AWAITING_INFORMATION: "warning", COMPLETED: "success", REJECTED: "error", CLOSED: "default",
};

function CreateRequestDialog({ onClose, onSave, saving }: { onClose: () => void; onSave: (payload: CreateDataPrincipalRequestPayload) => void; saving: boolean }) {
  const [requestType, setRequestType] = useState<string>("ACCESS");
  const [requesterName, setRequesterName] = useState("");
  const [requesterContactEmail, setRequesterContactEmail] = useState("");
  const [requesterContactPhone, setRequesterContactPhone] = useState("");
  const [description, setDescription] = useState("");

  const canSave = requestType.length > 0 && requesterName.trim().length > 0;

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>New Data Principal Request</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <TextField select label="Request Type" fullWidth required value={requestType} onChange={(e) => setRequestType(e.target.value)}>
            {DATA_PRINCIPAL_REQUEST_TYPES.map((t) => (
              <MenuItem key={t} value={t}>
                {t.replace(/_/g, " ")}
              </MenuItem>
            ))}
          </TextField>
          <TextField label="Requester Name" fullWidth required value={requesterName} onChange={(e) => setRequesterName(e.target.value)} />
          <TextField label="Requester Contact Email" fullWidth value={requesterContactEmail} onChange={(e) => setRequesterContactEmail(e.target.value)} />
          <TextField label="Requester Contact Phone" fullWidth value={requesterContactPhone} onChange={(e) => setRequesterContactPhone(e.target.value)} />
          <TextField label="Description" fullWidth multiline rows={3} value={description} onChange={(e) => setDescription(e.target.value)} />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          disabled={!canSave || saving}
          onClick={() =>
            onSave({
              requestType,
              requesterName,
              requesterContactEmail: requesterContactEmail || null,
              requesterContactPhone: requesterContactPhone || null,
              description: description || null,
            })
          }
        >
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
}

function AssignDialog({ onClose, onAssign }: { onClose: () => void; onAssign: (userId: string) => void }) {
  const { data: usersPage } = useQuery({ queryKey: ["users", 1, 100, ""], queryFn: () => getUsers(1, 100, undefined) });
  const [userId, setUserId] = useState("");

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Assign Request</DialogTitle>
      <DialogContent>
        <TextField select label="Assign To" fullWidth required sx={{ mt: 1 }} value={userId} onChange={(e) => setUserId(e.target.value)}>
          {(usersPage?.items ?? []).map((u) => (
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

function ReasonDialog({ title, label, onClose, onSubmit }: { title: string; label: string; onClose: () => void; onSubmit: (text: string) => void }) {
  const [text, setText] = useState("");
  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{title}</DialogTitle>
      <DialogContent>
        <TextField label={label} fullWidth required multiline rows={2} sx={{ mt: 1 }} value={text} onChange={(e) => setText(e.target.value)} />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" disabled={!text.trim()} onClick={() => onSubmit(text)}>
          Submit
        </Button>
      </DialogActions>
    </Dialog>
  );
}

function RequestDetailDialog({ id, onClose }: { id: string; onClose: () => void }) {
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const [assignOpen, setAssignOpen] = useState(false);
  const [completeOpen, setCompleteOpen] = useState(false);
  const [rejectOpen, setRejectOpen] = useState(false);

  const { data: request, isPending } = useQuery({ queryKey: ["data-principal-request", id], queryFn: () => api.getDataPrincipalRequestById(id) });

  const invalidateAll = () => {
    queryClient.invalidateQueries({ queryKey: ["data-principal-request", id] });
    queryClient.invalidateQueries({ queryKey: ["data-principal-requests"] });
    queryClient.invalidateQueries({ queryKey: ["dpr-sla-summary"] });
  };

  const assignMutation = useMutation({ mutationFn: (userId: string) => api.assignDataPrincipalRequest(id, userId), onSuccess: invalidateAll });
  const verifyMutation = useMutation({ mutationFn: () => api.verifyDataPrincipalRequestIdentity(id, request?.dataPrincipalId ?? null), onSuccess: invalidateAll });
  const progressMutation = useMutation({ mutationFn: (status: string) => api.updateDataPrincipalRequestStatus(id, status), onSuccess: invalidateAll });
  const completeMutation = useMutation({ mutationFn: (notes: string) => api.completeDataPrincipalRequest(id, notes || null), onSuccess: invalidateAll });
  const rejectMutation = useMutation({ mutationFn: (reason: string) => api.rejectDataPrincipalRequest(id, reason), onSuccess: invalidateAll });
  const closeMutation = useMutation({ mutationFn: () => api.closeDataPrincipalRequest(id), onSuccess: invalidateAll });

  const canManage = hasPermission("datarequests.manage");

  if (isPending || !request) {
    return (
      <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
        <DialogContent>
          <CircularProgress />
        </DialogContent>
      </Dialog>
    );
  }

  return (
    <Dialog open onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        {request.requestNumber} — {request.requestType.replace(/_/g, " ")}{" "}
        <Chip size="small" label={request.status.replace(/_/g, " ")} color={STATUS_COLORS[request.status]} sx={{ ml: 1 }} />
        {request.isOverdue && <Chip size="small" label="OVERDUE" color="error" sx={{ ml: 1 }} />}
      </DialogTitle>
      <DialogContent>
        <Stack spacing={1.5}>
          <Typography variant="body2">Requester: {request.requesterName} ({request.requesterContactEmail ?? "no email"})</Typography>
          {request.description && <Typography variant="body2">{request.description}</Typography>}
          <Typography variant="caption" color="text.secondary">
            Assigned to: {request.assignedToName ?? "Unassigned"} · SLA: {request.slaPolicyName ?? "None configured"} · Due:{" "}
            {request.dueAt ? new Date(request.dueAt).toLocaleString() : "—"}
          </Typography>
          {request.identityVerifiedAt && <Alert severity="info">Identity verified at {new Date(request.identityVerifiedAt).toLocaleString()}</Alert>}
          {request.resolutionNotes && <Alert severity="success">Resolution: {request.resolutionNotes}</Alert>}
          {request.rejectionReason && <Alert severity="error">Rejected: {request.rejectionReason}</Alert>}
        </Stack>
      </DialogContent>
      <DialogActions sx={{ flexWrap: "wrap", gap: 1, px: 3, pb: 2 }}>
        {canManage && !["COMPLETED", "REJECTED", "CLOSED"].includes(request.status) && (
          <Button onClick={() => setAssignOpen(true)}>Assign</Button>
        )}
        {canManage && ["REQUESTED", "IDENTITY_VERIFICATION"].includes(request.status) && (
          <Button variant="outlined" onClick={() => verifyMutation.mutate()} disabled={verifyMutation.isPending}>
            Verify Identity
          </Button>
        )}
        {canManage && request.status === "IN_PROGRESS" && (
          <Button variant="outlined" color="warning" onClick={() => progressMutation.mutate("AWAITING_INFORMATION")} disabled={progressMutation.isPending}>
            Await Information
          </Button>
        )}
        {canManage && request.status === "AWAITING_INFORMATION" && (
          <Button variant="outlined" onClick={() => progressMutation.mutate("IN_PROGRESS")} disabled={progressMutation.isPending}>
            Resume Progress
          </Button>
        )}
        {canManage && request.status === "IDENTITY_VERIFICATION" && (
          <Button variant="outlined" onClick={() => progressMutation.mutate("IN_PROGRESS")} disabled={progressMutation.isPending}>
            Start Progress
          </Button>
        )}
        {canManage && request.status === "IN_PROGRESS" && (
          <Button variant="contained" color="success" onClick={() => setCompleteOpen(true)}>
            Complete
          </Button>
        )}
        {canManage && !["COMPLETED", "REJECTED", "CLOSED"].includes(request.status) && (
          <Button variant="outlined" color="error" onClick={() => setRejectOpen(true)}>
            Reject
          </Button>
        )}
        {canManage && ["COMPLETED", "REJECTED"].includes(request.status) && (
          <Button variant="contained" onClick={() => closeMutation.mutate()} disabled={closeMutation.isPending}>
            Close
          </Button>
        )}
        <Button onClick={onClose}>Close Dialog</Button>
      </DialogActions>

      {assignOpen && (
        <AssignDialog
          onClose={() => setAssignOpen(false)}
          onAssign={(userId) => {
            assignMutation.mutate(userId);
            setAssignOpen(false);
          }}
        />
      )}
      {completeOpen && (
        <ReasonDialog
          title="Complete Request"
          label="Resolution Notes"
          onClose={() => setCompleteOpen(false)}
          onSubmit={(notes) => {
            completeMutation.mutate(notes);
            setCompleteOpen(false);
          }}
        />
      )}
      {rejectOpen && (
        <ReasonDialog
          title="Reject Request"
          label="Rejection Reason"
          onClose={() => setRejectOpen(false)}
          onSubmit={(reason) => {
            rejectMutation.mutate(reason);
            setRejectOpen(false);
          }}
        />
      )}
    </Dialog>
  );
}

function SlaSummaryCards() {
  const { data: summary } = useQuery({ queryKey: ["dpr-sla-summary"], queryFn: api.getDataPrincipalRequestSlaSummary });
  if (!summary) return null;

  const cards = [
    { label: "Open", value: summary.openCount, color: "text.primary" },
    { label: "Overdue", value: summary.overdueCount, color: "error.main" },
    { label: "Due within 48h", value: summary.dueWithin48HoursCount, color: "warning.main" },
    { label: "No SLA configured", value: summary.noSlaConfiguredCount, color: "text.secondary" },
  ];

  return (
    <Grid container spacing={2}>
      {cards.map((c) => (
        <Grid key={c.label} size={{ xs: 6, sm: 3 }}>
          <Card variant="outlined">
            <CardContent>
              <Typography variant="h5" sx={{ color: c.color }}>
                {c.value}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {c.label}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      ))}
    </Grid>
  );
}

export function DataPrincipalRequestsTab() {
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [status, setStatus] = useState("");
  const [requestType, setRequestType] = useState("");
  const [overdueOnly, setOverdueOnly] = useState(false);
  const [createOpen, setCreateOpen] = useState(false);
  const [detailId, setDetailId] = useState<string | null>(null);

  const canManage = hasPermission("datarequests.manage");

  const { data, isPending, isError } = useQuery({
    queryKey: ["data-principal-requests", page, status, requestType, overdueOnly],
    queryFn: () =>
      api.getDataPrincipalRequests({
        page, pageSize: PAGE_SIZE, status: status || undefined, requestType: requestType || undefined, overdueOnly: overdueOnly || undefined,
      }),
  });

  const createMutation = useMutation({
    mutationFn: api.createDataPrincipalRequest,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["data-principal-requests"] });
      queryClient.invalidateQueries({ queryKey: ["dpr-sla-summary"] });
    },
  });

  return (
    <Stack spacing={2}>
      <SlaSummaryCards />

      <Stack direction="row" sx={{ justifyContent: "space-between", flexWrap: "wrap", gap: 2 }}>
        <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap" }}>
          <TextField
            select
            label="Status"
            size="small"
            sx={{ minWidth: 180 }}
            value={status}
            onChange={(e) => {
              setStatus(e.target.value);
              setPage(1);
            }}
          >
            <MenuItem value="">All statuses</MenuItem>
            {DATA_PRINCIPAL_REQUEST_STATUSES.map((s) => (
              <MenuItem key={s} value={s}>
                {s.replace(/_/g, " ")}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            select
            label="Type"
            size="small"
            sx={{ minWidth: 160 }}
            value={requestType}
            onChange={(e) => {
              setRequestType(e.target.value);
              setPage(1);
            }}
          >
            <MenuItem value="">All types</MenuItem>
            {DATA_PRINCIPAL_REQUEST_TYPES.map((t) => (
              <MenuItem key={t} value={t}>
                {t.replace(/_/g, " ")}
              </MenuItem>
            ))}
          </TextField>
          <FormControlLabel
            control={<Checkbox checked={overdueOnly} onChange={(e) => { setOverdueOnly(e.target.checked); setPage(1); }} />}
            label="Overdue only"
          />
        </Stack>
        {canManage && (
          <Button variant="contained" onClick={() => setCreateOpen(true)}>
            New Request
          </Button>
        )}
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load data principal requests.</Alert>}

      {data && (
        <Stack spacing={2}>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Request #</TableCell>
                  <TableCell>Type</TableCell>
                  <TableCell>Requester</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Assigned To</TableCell>
                  <TableCell>Due At</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((r) => (
                  <TableRow key={r.id} hover sx={{ cursor: "pointer" }} onClick={() => setDetailId(r.id)}>
                    <TableCell>{r.requestNumber}</TableCell>
                    <TableCell>{r.requestType.replace(/_/g, " ")}</TableCell>
                    <TableCell>{r.requesterName}</TableCell>
                    <TableCell>
                      <Chip size="small" label={r.status.replace(/_/g, " ")} color={STATUS_COLORS[r.status]} />
                      {r.isOverdue && <Chip size="small" label="OVERDUE" color="error" sx={{ ml: 0.5 }} />}
                    </TableCell>
                    <TableCell>{r.assignedToName ?? "Unassigned"}</TableCell>
                    <TableCell>{r.dueAt ? new Date(r.dueAt).toLocaleDateString() : "—"}</TableCell>
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={6} align="center">
                      No data principal requests found.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
          <Stack direction="row" sx={{ justifyContent: "center" }}>
            <Pagination count={data.totalPages} page={data.page} onChange={(_, value) => setPage(value)} color="primary" />
          </Stack>
        </Stack>
      )}

      {createOpen && (
        <CreateRequestDialog
          saving={createMutation.isPending}
          onClose={() => setCreateOpen(false)}
          onSave={(payload) => {
            createMutation.mutate(payload);
            setCreateOpen(false);
          }}
        />
      )}
      {detailId && <RequestDetailDialog id={detailId} onClose={() => setDetailId(null)} />}
    </Stack>
  );
}
