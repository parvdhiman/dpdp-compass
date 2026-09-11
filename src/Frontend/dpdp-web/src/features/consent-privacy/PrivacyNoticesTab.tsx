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
import FormControl from "@mui/material/FormControl";
import InputLabel from "@mui/material/InputLabel";
import MenuItem from "@mui/material/MenuItem";
import OutlinedInput from "@mui/material/OutlinedInput";
import Pagination from "@mui/material/Pagination";
import Paper from "@mui/material/Paper";
import Select from "@mui/material/Select";
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
import { dataCategoriesApi } from "../data-inventory/api";
import * as api from "./api";
import { PRIVACY_NOTICE_STATUSES, type CreatePrivacyNoticePayload, type PrivacyNoticeDetail, type UpsertPrivacyNoticePayload } from "./types";

const PAGE_SIZE = 20;

const STATUS_COLORS: Record<string, "default" | "info" | "success" | "error"> = {
  DRAFT: "default", APPROVED: "info", PUBLISHED: "success", ARCHIVED: "error",
};

function NoticeFormDialog({
  notice, onClose, onSave, saving,
}: {
  notice: PrivacyNoticeDetail | null;
  onClose: () => void;
  onSave: (payload: CreatePrivacyNoticePayload) => void;
  saving: boolean;
}) {
  const { data: categories } = useQuery({ queryKey: ["data-categories", ""], queryFn: () => dataCategoriesApi.list() });

  const [code, setCode] = useState(notice?.code ?? "");
  const [title, setTitle] = useState(notice?.title ?? "");
  const [version, setVersion] = useState(notice?.version ?? "1.0");
  const [language, setLanguage] = useState(notice?.language ?? "en");
  const [purpose, setPurpose] = useState(notice?.purpose ?? "");
  const [publishedDate, setPublishedDate] = useState(notice?.publishedDate ?? "");
  const [effectiveDate, setEffectiveDate] = useState(notice?.effectiveDate ?? "");
  const [dataCategoryIds, setDataCategoryIds] = useState<string[]>(notice?.dataCategories.map((c) => c.id) ?? []);

  const canSave = code.trim().length > 0 && title.trim().length > 0 && version.trim().length > 0 && language.trim().length > 0 && purpose.trim().length > 0;

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{notice ? "Edit Privacy Notice" : "New Privacy Notice"}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <TextField label="Code" fullWidth required disabled={!!notice} helperText="Identifies this notice across all its versions (e.g. PRIVACY-POLICY)" value={code} onChange={(e) => setCode(e.target.value)} />
          <TextField label="Title" fullWidth required value={title} onChange={(e) => setTitle(e.target.value)} />
          <TextField label="Version" fullWidth required disabled={!!notice} value={version} onChange={(e) => setVersion(e.target.value)} />
          <TextField label="Language" fullWidth required value={language} onChange={(e) => setLanguage(e.target.value)} />
          <TextField label="Purpose" fullWidth required multiline rows={3} value={purpose} onChange={(e) => setPurpose(e.target.value)} />
          <TextField label="Published Date" type="date" fullWidth slotProps={{ inputLabel: { shrink: true } }} value={publishedDate} onChange={(e) => setPublishedDate(e.target.value)} />
          <TextField label="Effective Date" type="date" fullWidth slotProps={{ inputLabel: { shrink: true } }} value={effectiveDate} onChange={(e) => setEffectiveDate(e.target.value)} />
          <FormControl fullWidth>
            <InputLabel>Data Categories</InputLabel>
            <Select
              multiple
              input={<OutlinedInput label="Data Categories" />}
              value={dataCategoryIds}
              onChange={(e) => setDataCategoryIds(typeof e.target.value === "string" ? e.target.value.split(",") : e.target.value)}
              renderValue={(selected) => (selected as string[]).map((id) => categories?.find((c) => c.id === id)?.name ?? id).join(", ")}
            >
              {(categories ?? []).map((c) => (
                <MenuItem key={c.id} value={c.id}>
                  {c.name}
                </MenuItem>
              ))}
            </Select>
          </FormControl>
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          disabled={!canSave || saving}
          onClick={() =>
            onSave({
              code, title, version, language, purpose,
              publishedDate: publishedDate || null,
              effectiveDate: effectiveDate || null,
              dataCategoryIds,
            })
          }
        >
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
}

function NoticeDetailDialog({ id, onClose }: { id: string; onClose: () => void }) {
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const [editOpen, setEditOpen] = useState(false);

  const { data: notice, isPending } = useQuery({ queryKey: ["privacy-notice", id], queryFn: () => api.getPrivacyNoticeById(id) });

  const invalidateAll = () => {
    queryClient.invalidateQueries({ queryKey: ["privacy-notice", id] });
    queryClient.invalidateQueries({ queryKey: ["privacy-notices"] });
  };

  const updateMutation = useMutation({ mutationFn: (payload: UpsertPrivacyNoticePayload) => api.updatePrivacyNotice(id, payload), onSuccess: invalidateAll });
  const approveMutation = useMutation({ mutationFn: () => api.approvePrivacyNotice(id), onSuccess: invalidateAll });
  const publishMutation = useMutation({ mutationFn: () => api.publishPrivacyNotice(id), onSuccess: invalidateAll });
  const archiveMutation = useMutation({ mutationFn: () => api.archivePrivacyNotice(id), onSuccess: invalidateAll });
  const deleteMutation = useMutation({
    mutationFn: () => api.deletePrivacyNotice(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["privacy-notices"] });
      onClose();
    },
  });

  const canManage = hasPermission("privacynotices.manage");
  const canApprove = hasPermission("privacynotices.approve");

  if (isPending || !notice) {
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
        {notice.noticeNumber} — {notice.title} (v{notice.version}){" "}
        <Chip size="small" label={notice.status} color={STATUS_COLORS[notice.status]} sx={{ ml: 1 }} />
      </DialogTitle>
      <DialogContent>
        <Stack spacing={1.5}>
          <Typography variant="caption" color="text.secondary">
            Code: {notice.code} · Language: {notice.language}
          </Typography>
          <Typography variant="body2">{notice.purpose}</Typography>
          <Typography variant="body2">Data Categories: {notice.dataCategories.map((c) => c.name).join(", ") || "—"}</Typography>
          <Typography variant="caption" color="text.secondary">
            Published: {notice.publishedDate ?? "—"} · Effective: {notice.effectiveDate ?? "—"}
          </Typography>
        </Stack>
      </DialogContent>
      <DialogActions sx={{ flexWrap: "wrap", gap: 1, px: 3, pb: 2 }}>
        {canManage && notice.status === "DRAFT" && <Button onClick={() => setEditOpen(true)}>Edit</Button>}
        {canManage && notice.status === "DRAFT" && (
          <Button color="error" onClick={() => deleteMutation.mutate()} disabled={deleteMutation.isPending}>
            Delete
          </Button>
        )}
        {canApprove && notice.status === "DRAFT" && (
          <Button variant="contained" color="success" onClick={() => approveMutation.mutate()} disabled={approveMutation.isPending}>
            Approve
          </Button>
        )}
        {canManage && notice.status === "APPROVED" && (
          <Button variant="contained" onClick={() => publishMutation.mutate()} disabled={publishMutation.isPending}>
            Publish
          </Button>
        )}
        {canManage && notice.status === "PUBLISHED" && (
          <Button variant="outlined" onClick={() => archiveMutation.mutate()} disabled={archiveMutation.isPending}>
            Archive
          </Button>
        )}
        <Button onClick={onClose}>Close</Button>
      </DialogActions>

      {editOpen && (
        <NoticeFormDialog
          notice={notice}
          saving={updateMutation.isPending}
          onClose={() => setEditOpen(false)}
          onSave={(payload) => {
            updateMutation.mutate(payload);
            setEditOpen(false);
          }}
        />
      )}
    </Dialog>
  );
}

export function PrivacyNoticesTab() {
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");
  const [createOpen, setCreateOpen] = useState(false);
  const [detailId, setDetailId] = useState<string | null>(null);

  const canManage = hasPermission("privacynotices.manage");

  const { data, isPending, isError } = useQuery({
    queryKey: ["privacy-notices", page, search, status],
    queryFn: () => api.getPrivacyNotices({ page, pageSize: PAGE_SIZE, search: search || undefined, status: status || undefined }),
  });

  const createMutation = useMutation({
    mutationFn: api.createPrivacyNotice,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["privacy-notices"] }),
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
            {PRIVACY_NOTICE_STATUSES.map((s) => (
              <MenuItem key={s} value={s}>
                {s}
              </MenuItem>
            ))}
          </TextField>
        </Stack>
        {canManage && (
          <Button variant="contained" onClick={() => setCreateOpen(true)}>
            New Privacy Notice
          </Button>
        )}
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load privacy notices.</Alert>}

      {data && (
        <Stack spacing={2}>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Notice #</TableCell>
                  <TableCell>Code</TableCell>
                  <TableCell>Title</TableCell>
                  <TableCell>Version</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Effective Date</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((n) => (
                  <TableRow key={n.id} hover sx={{ cursor: "pointer" }} onClick={() => setDetailId(n.id)}>
                    <TableCell>{n.noticeNumber}</TableCell>
                    <TableCell>{n.code}</TableCell>
                    <TableCell>{n.title}</TableCell>
                    <TableCell>{n.version}</TableCell>
                    <TableCell>
                      <Chip size="small" label={n.status} color={STATUS_COLORS[n.status]} />
                    </TableCell>
                    <TableCell>{n.effectiveDate ?? "—"}</TableCell>
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={6} align="center">
                      No privacy notices found.
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
        <NoticeFormDialog
          notice={null}
          saving={createMutation.isPending}
          onClose={() => setCreateOpen(false)}
          onSave={(payload) => {
            createMutation.mutate(payload);
            setCreateOpen(false);
          }}
        />
      )}
      {detailId && <NoticeDetailDialog id={detailId} onClose={() => setDetailId(null)} />}
    </Stack>
  );
}
