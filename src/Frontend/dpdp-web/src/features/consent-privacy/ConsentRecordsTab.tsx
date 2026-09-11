import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
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
import { useAuth } from "../auth/AuthProvider";
import { dataPrincipalsApi, consentPurposesApi } from "./api";
import * as api from "./api";
import { CONSENT_CHANNELS, CONSENT_STATUSES, type CreateConsentPayload } from "./types";

const PAGE_SIZE = 20;

const STATUS_COLORS: Record<string, "success" | "default" | "error" | "warning"> = {
  GRANTED: "success", WITHDRAWN: "default", EXPIRED: "warning", REVOKED: "error",
};

function CreateConsentDialog({ onClose, onSave, saving }: { onClose: () => void; onSave: (payload: CreateConsentPayload) => void; saving: boolean }) {
  const { data: principals } = useQuery({ queryKey: ["data-principals", ""], queryFn: () => dataPrincipalsApi.list() });
  const { data: purposes } = useQuery({ queryKey: ["consent-purposes", ""], queryFn: () => consentPurposesApi.list() });

  const [dataPrincipalId, setDataPrincipalId] = useState("");
  const [consentPurposeId, setConsentPurposeId] = useState("");
  const [channel, setChannel] = useState<string>("WEB");
  const [expiresAt, setExpiresAt] = useState("");
  const [sourceSystem, setSourceSystem] = useState("");

  const canSave = dataPrincipalId.length > 0 && consentPurposeId.length > 0 && channel.length > 0;

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Capture Consent</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <TextField select label="Data Principal" fullWidth required value={dataPrincipalId} onChange={(e) => setDataPrincipalId(e.target.value)}>
            {(principals ?? []).map((p) => (
              <MenuItem key={p.id} value={p.id}>
                {p.externalReferenceId}
              </MenuItem>
            ))}
          </TextField>
          <TextField select label="Consent Purpose" fullWidth required value={consentPurposeId} onChange={(e) => setConsentPurposeId(e.target.value)}>
            {(purposes ?? []).map((p) => (
              <MenuItem key={p.id} value={p.id}>
                {p.name}
              </MenuItem>
            ))}
          </TextField>
          <TextField select label="Channel" fullWidth required value={channel} onChange={(e) => setChannel(e.target.value)}>
            {CONSENT_CHANNELS.map((c) => (
              <MenuItem key={c} value={c}>
                {c.replace(/_/g, " ")}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            label="Expires At"
            type="datetime-local"
            fullWidth
            slotProps={{ inputLabel: { shrink: true } }}
            value={expiresAt}
            onChange={(e) => setExpiresAt(e.target.value)}
          />
          <TextField label="Source System" fullWidth value={sourceSystem} onChange={(e) => setSourceSystem(e.target.value)} />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          disabled={!canSave || saving}
          onClick={() =>
            onSave({
              dataPrincipalId,
              consentPurposeId,
              channel,
              expiresAt: expiresAt ? new Date(expiresAt).toISOString() : null,
              sourceSystem: sourceSystem || null,
            })
          }
        >
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
}

function RevokeConsentDialog({ onClose, onRevoke }: { onClose: () => void; onRevoke: (reason: string) => void }) {
  const [reason, setReason] = useState("");
  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Revoke Consent</DialogTitle>
      <DialogContent>
        <TextField label="Reason" fullWidth required multiline rows={2} sx={{ mt: 1 }} value={reason} onChange={(e) => setReason(e.target.value)} />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" color="error" disabled={!reason.trim()} onClick={() => onRevoke(reason)}>
          Revoke
        </Button>
      </DialogActions>
    </Dialog>
  );
}

export function ConsentRecordsTab() {
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [status, setStatus] = useState("");
  const [createOpen, setCreateOpen] = useState(false);
  const [revokeId, setRevokeId] = useState<string | null>(null);

  const canManage = hasPermission("consent.manage");

  const { data, isPending, isError } = useQuery({
    queryKey: ["consent-records", page, status],
    queryFn: () => api.getConsentRecords({ page, pageSize: PAGE_SIZE, status: status || undefined }),
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["consent-records"] });
  const createMutation = useMutation({ mutationFn: api.createConsentRecord, onSuccess: invalidate });
  const withdrawMutation = useMutation({ mutationFn: api.withdrawConsent, onSuccess: invalidate });
  const revokeMutation = useMutation({ mutationFn: (vars: { id: string; reason: string }) => api.revokeConsent(vars.id, vars.reason), onSuccess: invalidate });
  const markExpiredMutation = useMutation({ mutationFn: api.markConsentExpired, onSuccess: invalidate });

  return (
    <Stack spacing={2}>
      <Stack direction="row" sx={{ justifyContent: "space-between", flexWrap: "wrap", gap: 2 }}>
        <TextField
          select
          label="Status"
          size="small"
          sx={{ minWidth: 160 }}
          value={status}
          onChange={(e) => {
            setStatus(e.target.value);
            setPage(1);
          }}
        >
          <MenuItem value="">All statuses</MenuItem>
          {CONSENT_STATUSES.map((s) => (
            <MenuItem key={s} value={s}>
              {s}
            </MenuItem>
          ))}
        </TextField>
        {canManage && (
          <Stack direction="row" spacing={1}>
            <Button variant="outlined" onClick={() => markExpiredMutation.mutate()} disabled={markExpiredMutation.isPending}>
              Mark Expired
            </Button>
            <Button variant="contained" onClick={() => setCreateOpen(true)}>
              Capture Consent
            </Button>
          </Stack>
        )}
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load consent records.</Alert>}

      {data && (
        <Stack spacing={2}>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Consent #</TableCell>
                  <TableCell>Data Principal</TableCell>
                  <TableCell>Purpose</TableCell>
                  <TableCell>Channel</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Granted At</TableCell>
                  {canManage && <TableCell>Actions</TableCell>}
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((c) => (
                  <TableRow key={c.id} hover>
                    <TableCell>{c.consentNumber}</TableCell>
                    <TableCell>{c.dataPrincipalReference}</TableCell>
                    <TableCell>{c.consentPurposeName}</TableCell>
                    <TableCell>{c.channel.replace(/_/g, " ")}</TableCell>
                    <TableCell>
                      <Chip size="small" label={c.status} color={STATUS_COLORS[c.status]} />
                    </TableCell>
                    <TableCell>{new Date(c.grantedAt).toLocaleString()}</TableCell>
                    {canManage && (
                      <TableCell>
                        {c.status === "GRANTED" && (
                          <Stack direction="row" spacing={1}>
                            <Button size="small" onClick={() => withdrawMutation.mutate(c.id)}>
                              Withdraw
                            </Button>
                            <Button size="small" color="error" onClick={() => setRevokeId(c.id)}>
                              Revoke
                            </Button>
                          </Stack>
                        )}
                      </TableCell>
                    )}
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={canManage ? 7 : 6} align="center">
                      No consent records found.
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
        <CreateConsentDialog
          saving={createMutation.isPending}
          onClose={() => setCreateOpen(false)}
          onSave={(payload) => {
            createMutation.mutate(payload);
            setCreateOpen(false);
          }}
        />
      )}
      {revokeId && (
        <RevokeConsentDialog
          onClose={() => setRevokeId(null)}
          onRevoke={(reason) => {
            revokeMutation.mutate({ id: revokeId, reason });
            setRevokeId(null);
          }}
        />
      )}
    </Stack>
  );
}
