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
import Select from "@mui/material/Select";
import InputLabel from "@mui/material/InputLabel";
import FormControl from "@mui/material/FormControl";
import OutlinedInput from "@mui/material/OutlinedInput";
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
import * as api from "./api";
import { dataCategoriesApi, itSystemsApi, dataCollectionSourcesApi, recipientsApi, processorsApi, retentionPoliciesApi } from "./api";
import { DATA_SUBJECT_CATEGORIES, PROCESSING_ACTIVITY_STATUSES, type ProcessingActivityDetail, type UpsertProcessingActivityPayload } from "./types";

const PAGE_SIZE = 20;

const STATUS_COLORS: Record<string, "default" | "warning" | "success" | "error"> = {
  DRAFT: "default", IN_REVIEW: "warning", APPROVED: "success", ARCHIVED: "error",
};

function MultiSelectField({ label, options, getLabel, value, onChange }: {
  label: string;
  options: { id: string; name: string }[];
  getLabel: (id: string) => string;
  value: string[];
  onChange: (value: string[]) => void;
}) {
  return (
    <FormControl fullWidth>
      <InputLabel>{label}</InputLabel>
      <Select
        multiple
        input={<OutlinedInput label={label} />}
        value={value}
        onChange={(e) => onChange(typeof e.target.value === "string" ? e.target.value.split(",") : e.target.value)}
        renderValue={(selected) => (selected as string[]).map(getLabel).join(", ")}
      >
        {options.map((o) => (
          <MenuItem key={o.id} value={o.id}>
            {o.name}
          </MenuItem>
        ))}
      </Select>
    </FormControl>
  );
}

function ActivityFormDialog({
  activity, onClose, onSave, saving,
}: {
  activity: ProcessingActivityDetail | null;
  onClose: () => void;
  onSave: (payload: UpsertProcessingActivityPayload) => void;
  saving: boolean;
}) {
  const { data: categories } = useQuery({ queryKey: ["data-categories", ""], queryFn: () => dataCategoriesApi.list() });
  const { data: systems } = useQuery({ queryKey: ["it-systems", ""], queryFn: () => itSystemsApi.list() });
  const { data: sources } = useQuery({ queryKey: ["data-collection-sources", ""], queryFn: () => dataCollectionSourcesApi.list() });
  const { data: recipients } = useQuery({ queryKey: ["recipients", ""], queryFn: () => recipientsApi.list() });
  const { data: processors } = useQuery({ queryKey: ["processors", ""], queryFn: () => processorsApi.list() });
  const { data: retentionPolicies } = useQuery({ queryKey: ["retention-policies", ""], queryFn: () => retentionPoliciesApi.list() });

  const [name, setName] = useState(activity?.name ?? "");
  const [purpose, setPurpose] = useState(activity?.purpose ?? "");
  const [dataSubjectCategories, setDataSubjectCategories] = useState<string[]>(activity?.dataSubjectCategories ?? []);
  const [securityControlsText, setSecurityControlsText] = useState((activity?.securityControls ?? []).join(", "));
  const [retentionPolicyId, setRetentionPolicyId] = useState(activity?.retentionPolicyId ?? "");
  const [reviewDate, setReviewDate] = useState(activity?.reviewDate ?? "");
  const [dataCategoryIds, setDataCategoryIds] = useState<string[]>(activity?.dataCategories.map((c) => c.id) ?? []);
  const [itSystemIds, setItSystemIds] = useState<string[]>(activity?.itSystems.map((s) => s.id) ?? []);
  const [dataCollectionSourceIds, setDataCollectionSourceIds] = useState<string[]>(activity?.dataCollectionSources.map((s) => s.id) ?? []);
  const [recipientIds, setRecipientIds] = useState<string[]>(activity?.recipients.map((r) => r.id) ?? []);
  const [processorIds, setProcessorIds] = useState<string[]>(activity?.processors.map((p) => p.id) ?? []);

  const nameOf = (list: { id: string; name: string }[] | undefined) => (id: string) => list?.find((x) => x.id === id)?.name ?? id;

  const canSave = name.trim().length > 0 && purpose.trim().length > 0;

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{activity ? "Edit Processing Activity" : "New Processing Activity"}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <TextField label="Name" fullWidth required value={name} onChange={(e) => setName(e.target.value)} />
          <TextField label="Purpose" fullWidth required multiline rows={2} value={purpose} onChange={(e) => setPurpose(e.target.value)} />
          <MultiSelectField
            label="Data Subject Categories"
            options={DATA_SUBJECT_CATEGORIES.map((c) => ({ id: c, name: c.replace(/_/g, " ") }))}
            getLabel={(id) => id.replace(/_/g, " ")}
            value={dataSubjectCategories}
            onChange={setDataSubjectCategories}
          />
          <TextField
            label="Security Controls (comma-separated)"
            fullWidth
            value={securityControlsText}
            onChange={(e) => setSecurityControlsText(e.target.value)}
            helperText="e.g. Encryption at rest, Access control, MFA"
          />
          <TextField select label="Retention Policy" fullWidth value={retentionPolicyId} onChange={(e) => setRetentionPolicyId(e.target.value)}>
            <MenuItem value="">—</MenuItem>
            {retentionPolicies?.map((p) => (
              <MenuItem key={String(p.id)} value={String(p.id)}>
                {String(p.name)}
              </MenuItem>
            ))}
          </TextField>
          <TextField label="Review Date" type="date" fullWidth slotProps={{ inputLabel: { shrink: true } }} value={reviewDate} onChange={(e) => setReviewDate(e.target.value)} />
          <MultiSelectField label="Data Categories" options={(categories ?? []).map((c) => ({ id: String(c.id), name: String(c.name) }))} getLabel={nameOf(categories)} value={dataCategoryIds} onChange={setDataCategoryIds} />
          <MultiSelectField label="Systems" options={(systems ?? []).map((s) => ({ id: String(s.id), name: String(s.name) }))} getLabel={nameOf(systems)} value={itSystemIds} onChange={setItSystemIds} />
          <MultiSelectField label="Data Sources" options={(sources ?? []).map((s) => ({ id: String(s.id), name: String(s.name) }))} getLabel={nameOf(sources)} value={dataCollectionSourceIds} onChange={setDataCollectionSourceIds} />
          <MultiSelectField label="Recipients" options={(recipients ?? []).map((r) => ({ id: String(r.id), name: String(r.name) }))} getLabel={nameOf(recipients)} value={recipientIds} onChange={setRecipientIds} />
          <MultiSelectField label="Processors" options={(processors ?? []).map((p) => ({ id: String(p.id), name: String(p.name) }))} getLabel={nameOf(processors)} value={processorIds} onChange={setProcessorIds} />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          disabled={!canSave || saving}
          onClick={() =>
            onSave({
              name,
              purpose,
              dataSubjectCategories,
              securityControls: securityControlsText.split(",").map((s) => s.trim()).filter(Boolean),
              retentionPolicyId: retentionPolicyId || null,
              reviewDate: reviewDate || null,
              dataCategoryIds,
              itSystemIds,
              dataCollectionSourceIds,
              recipientIds,
              processorIds,
            })
          }
        >
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
}

function SendBackDialog({ onClose, onSend }: { onClose: () => void; onSend: (comments: string) => void }) {
  const [comments, setComments] = useState("");
  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Send Back to Draft</DialogTitle>
      <DialogContent>
        <TextField label="Review Comments" fullWidth required multiline rows={2} sx={{ mt: 1 }} value={comments} onChange={(e) => setComments(e.target.value)} />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" color="warning" disabled={!comments.trim()} onClick={() => onSend(comments)}>
          Send Back
        </Button>
      </DialogActions>
    </Dialog>
  );
}

function ActivityDetailDialog({ id, onClose }: { id: string; onClose: () => void }) {
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const [editOpen, setEditOpen] = useState(false);
  const [sendBackOpen, setSendBackOpen] = useState(false);

  const { data: activity, isPending } = useQuery({ queryKey: ["processing-activity", id], queryFn: () => api.getProcessingActivityById(id) });

  const invalidateAll = () => {
    queryClient.invalidateQueries({ queryKey: ["processing-activity", id] });
    queryClient.invalidateQueries({ queryKey: ["processing-activities"] });
  };

  const updateMutation = useMutation({ mutationFn: (payload: UpsertProcessingActivityPayload) => api.updateProcessingActivity(id, payload), onSuccess: invalidateAll });
  const submitMutation = useMutation({ mutationFn: () => api.submitProcessingActivityForReview(id), onSuccess: invalidateAll });
  const approveMutation = useMutation({ mutationFn: () => api.approveProcessingActivity(id, null), onSuccess: invalidateAll });
  const sendBackMutation = useMutation({ mutationFn: (comments: string) => api.sendProcessingActivityBackToDraft(id, comments), onSuccess: invalidateAll });
  const archiveMutation = useMutation({ mutationFn: () => api.archiveProcessingActivity(id), onSuccess: invalidateAll });
  const reopenMutation = useMutation({ mutationFn: () => api.reopenProcessingActivity(id), onSuccess: invalidateAll });

  const canManage = hasPermission("processingactivities.manage");
  const canApprove = hasPermission("processingactivities.approve");
  const canReview = hasPermission("processingactivities.review");

  if (isPending || !activity) {
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
        {activity.activityNumber} — {activity.name}{" "}
        <Chip size="small" label={activity.status.replace("_", " ")} color={STATUS_COLORS[activity.status]} sx={{ ml: 1 }} />
      </DialogTitle>
      <DialogContent>
        <Stack spacing={1.5}>
          <Typography variant="body2">{activity.purpose}</Typography>
          <Typography variant="caption" color="text.secondary">
            Owner: {activity.ownerName ?? "Unassigned"} · Review date: {activity.reviewDate ?? "—"}
          </Typography>
          {activity.reviewComments && <Alert severity="info">Review comments: {activity.reviewComments}</Alert>}
          <Typography variant="body2">Data Subjects: {activity.dataSubjectCategories.join(", ") || "—"}</Typography>
          <Typography variant="body2">Data Categories: {activity.dataCategories.map((c) => c.name).join(", ") || "—"}</Typography>
          <Typography variant="body2">Systems: {activity.itSystems.map((s) => s.name).join(", ") || "—"}</Typography>
          <Typography variant="body2">Data Sources: {activity.dataCollectionSources.map((s) => s.name).join(", ") || "—"}</Typography>
          <Typography variant="body2">Recipients: {activity.recipients.map((r) => r.name).join(", ") || "—"}</Typography>
          <Typography variant="body2">Processors: {activity.processors.map((p) => p.name).join(", ") || "—"}</Typography>
          <Typography variant="body2">Retention Policy: {activity.retentionPolicyName ?? "—"}</Typography>
          <Typography variant="body2">Security Controls: {activity.securityControls.join(", ") || "—"}</Typography>
        </Stack>
      </DialogContent>
      <DialogActions sx={{ flexWrap: "wrap", gap: 1, px: 3, pb: 2 }}>
        {canManage && activity.status !== "ARCHIVED" && <Button onClick={() => setEditOpen(true)}>Edit</Button>}
        {canManage && activity.status === "DRAFT" && (
          <Button variant="contained" onClick={() => submitMutation.mutate()} disabled={submitMutation.isPending}>
            Submit for Review
          </Button>
        )}
        {canApprove && activity.status === "IN_REVIEW" && (
          <Button variant="contained" color="success" onClick={() => approveMutation.mutate()} disabled={approveMutation.isPending}>
            Approve
          </Button>
        )}
        {canReview && activity.status === "IN_REVIEW" && (
          <Button variant="outlined" color="warning" onClick={() => setSendBackOpen(true)}>
            Send Back
          </Button>
        )}
        {canManage && activity.status === "APPROVED" && (
          <Button variant="outlined" onClick={() => archiveMutation.mutate()} disabled={archiveMutation.isPending}>
            Archive
          </Button>
        )}
        {canManage && activity.status === "APPROVED" && (
          <Button variant="outlined" onClick={() => reopenMutation.mutate()} disabled={reopenMutation.isPending}>
            Reopen
          </Button>
        )}
        <Button onClick={onClose}>Close</Button>
      </DialogActions>

      {editOpen && (
        <ActivityFormDialog
          activity={activity}
          saving={updateMutation.isPending}
          onClose={() => setEditOpen(false)}
          onSave={(payload) => {
            updateMutation.mutate(payload);
            setEditOpen(false);
          }}
        />
      )}
      {sendBackOpen && (
        <SendBackDialog
          onClose={() => setSendBackOpen(false)}
          onSend={(comments) => {
            sendBackMutation.mutate(comments);
            setSendBackOpen(false);
          }}
        />
      )}
    </Dialog>
  );
}

export function ProcessingActivitiesTab() {
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");
  const [createOpen, setCreateOpen] = useState(false);
  const [detailId, setDetailId] = useState<string | null>(null);

  const canManage = hasPermission("processingactivities.manage");

  const { data, isPending, isError } = useQuery({
    queryKey: ["processing-activities", page, search, status],
    queryFn: () => api.getProcessingActivities({ page, pageSize: PAGE_SIZE, search: search || undefined, status: status || undefined }),
  });

  const createMutation = useMutation({
    mutationFn: api.createProcessingActivity,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["processing-activities"] }),
  });

  return (
    <Stack spacing={2}>
      <Stack direction="row" sx={{ justifyContent: "space-between", flexWrap: "wrap", gap: 2 }}>
        <Stack direction="row" spacing={2}>
          <TextField
            label="Search"
            size="small"
            sx={{ minWidth: 220 }}
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
          />
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
            {PROCESSING_ACTIVITY_STATUSES.map((s) => (
              <MenuItem key={s} value={s}>
                {s.replace("_", " ")}
              </MenuItem>
            ))}
          </TextField>
        </Stack>
        <Stack direction="row" spacing={1}>
          <Button variant="outlined" onClick={() => api.exportProcessingActivities({ search: search || undefined, status: status || undefined })}>
            Export CSV
          </Button>
          {canManage && (
            <Button variant="contained" onClick={() => setCreateOpen(true)}>
              New Processing Activity
            </Button>
          )}
        </Stack>
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load processing activities.</Alert>}

      {data && (
        <Stack spacing={2}>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Activity #</TableCell>
                  <TableCell>Name</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Owner</TableCell>
                  <TableCell>Review Date</TableCell>
                  <TableCell>Data Categories</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((a) => (
                  <TableRow key={a.id} hover sx={{ cursor: "pointer" }} onClick={() => setDetailId(a.id)}>
                    <TableCell>{a.activityNumber}</TableCell>
                    <TableCell>{a.name}</TableCell>
                    <TableCell>
                      <Chip size="small" label={a.status.replace("_", " ")} color={STATUS_COLORS[a.status]} />
                    </TableCell>
                    <TableCell>{a.ownerName ?? "Unassigned"}</TableCell>
                    <TableCell>{a.reviewDate ?? "—"}</TableCell>
                    <TableCell>{a.dataCategoryCount}</TableCell>
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={6} align="center">
                      No processing activities found.
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
        <ActivityFormDialog
          activity={null}
          saving={createMutation.isPending}
          onClose={() => setCreateOpen(false)}
          onSave={(payload) => {
            createMutation.mutate(payload);
            setCreateOpen(false);
          }}
        />
      )}
      {detailId && <ActivityDetailDialog id={detailId} onClose={() => setDetailId(null)} />}
    </Stack>
  );
}
