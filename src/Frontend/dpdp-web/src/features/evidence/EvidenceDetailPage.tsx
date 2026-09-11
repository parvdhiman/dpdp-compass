import { useEffect, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router-dom";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import MenuItem from "@mui/material/MenuItem";
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
import * as usersApi from "../users/api";
import * as evidenceApi from "./api";
import type { EvidenceDetail, UpdateEvidencePayload } from "./types";

const STATUS_COLORS: Record<string, "success" | "warning" | "error" | "info" | "default"> = {
  UPLOADED: "info",
  UNDER_REVIEW: "warning",
  APPROVED: "success",
  REJECTED: "error",
  EXPIRED: "error",
  ARCHIVED: "default",
};

function EditMetadataDialog({ evidence, onClose, onSave }: { evidence: EvidenceDetail; onClose: () => void; onSave: (payload: UpdateEvidencePayload) => void }) {
  const { data: users } = useQuery({ queryKey: ["users-for-evidence"], queryFn: () => usersApi.getUsers(1, 100) });
  const [title, setTitle] = useState(evidence.title);
  const [description, setDescription] = useState(evidence.description ?? "");
  const [ownerUserId, setOwnerUserId] = useState(evidence.ownerUserId ?? "");
  const [reviewerUserId, setReviewerUserId] = useState(evidence.reviewerUserId ?? "");
  const [expiryDate, setExpiryDate] = useState(evidence.expiryDate ?? "");

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Edit Evidence</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          <TextField label="Title" fullWidth value={title} onChange={(e) => setTitle(e.target.value)} />
          <TextField label="Description" fullWidth multiline rows={2} value={description} onChange={(e) => setDescription(e.target.value)} />
          <TextField select label="Owner" fullWidth value={ownerUserId} onChange={(e) => setOwnerUserId(e.target.value)}>
            <MenuItem value="">Unassigned</MenuItem>
            {users?.items.map((u) => (
              <MenuItem key={u.id} value={u.id}>
                {u.fullName}
              </MenuItem>
            ))}
          </TextField>
          <TextField select label="Reviewer" fullWidth value={reviewerUserId} onChange={(e) => setReviewerUserId(e.target.value)}>
            <MenuItem value="">Unassigned</MenuItem>
            {users?.items.map((u) => (
              <MenuItem key={u.id} value={u.id}>
                {u.fullName}
              </MenuItem>
            ))}
          </TextField>
          <TextField
            label="Expiry Date"
            type="date"
            fullWidth
            slotProps={{ inputLabel: { shrink: true } }}
            value={expiryDate}
            onChange={(e) => setExpiryDate(e.target.value)}
          />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          disabled={!title.trim()}
          onClick={() =>
            onSave({
              title,
              description: description || null,
              vendorReference: evidence.vendorReference,
              processingActivityReference: evidence.processingActivityReference,
              ownerUserId: ownerUserId || null,
              reviewerUserId: reviewerUserId || null,
              expiryDate: expiryDate || null,
            })
          }
        >
          Save
        </Button>
      </DialogActions>
    </Dialog>
  );
}

function NewVersionDialog({ evidenceType, onClose, onUpload }: { evidenceType: string; onClose: () => void; onUpload: (file: File | null, externalUrl: string | null) => void }) {
  const [file, setFile] = useState<File | null>(null);
  const [externalUrl, setExternalUrl] = useState("");
  const isUrlType = evidenceType === "URL";

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Upload New Version</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          {isUrlType ? (
            <TextField label="URL" fullWidth value={externalUrl} onChange={(e) => setExternalUrl(e.target.value)} />
          ) : (
            <input type="file" onChange={(e) => setFile(e.target.files?.[0] ?? null)} />
          )}
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          variant="contained"
          disabled={isUrlType ? !externalUrl.trim() : !file}
          onClick={() => onUpload(file, isUrlType ? externalUrl : null)}
        >
          Upload
        </Button>
      </DialogActions>
    </Dialog>
  );
}

function RejectDialog({ onClose, onReject }: { onClose: () => void; onReject: (reason: string) => void }) {
  const [reason, setReason] = useState("");
  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Reject Evidence</DialogTitle>
      <DialogContent>
        <TextField
          label="Rejection Reason"
          fullWidth
          required
          multiline
          rows={2}
          sx={{ mt: 1 }}
          value={reason}
          onChange={(e) => setReason(e.target.value)}
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" color="error" disabled={!reason.trim()} onClick={() => onReject(reason)}>
          Reject
        </Button>
      </DialogActions>
    </Dialog>
  );
}

function PreviewPane({ evidenceId, versionNumber }: { evidenceId: string; versionNumber: number }) {
  const [url, setUrl] = useState<string | null>(null);
  const [failed, setFailed] = useState(false);

  useEffect(() => {
    let objectUrl: string | null = null;
    evidenceApi
      .previewEvidenceVersion(evidenceId, versionNumber)
      .then((blobUrl) => {
        objectUrl = blobUrl;
        setUrl(blobUrl);
      })
      .catch(() => setFailed(true));
    return () => {
      if (objectUrl) URL.revokeObjectURL(objectUrl);
    };
  }, [evidenceId, versionNumber]);

  if (failed) return <Alert severity="warning">This version cannot be previewed inline.</Alert>;
  if (!url) return <CircularProgress size={24} />;
  return <iframe src={url} title={`Evidence version ${versionNumber} preview`} style={{ width: "100%", height: 480, border: "none" }} />;
}

export function EvidenceDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { hasPermission } = useAuth();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [editOpen, setEditOpen] = useState(false);
  const [newVersionOpen, setNewVersionOpen] = useState(false);
  const [rejectOpen, setRejectOpen] = useState(false);
  const [previewVersion, setPreviewVersion] = useState<number | null>(null);

  const { data: evidence, isPending, isError } = useQuery({
    queryKey: ["evidence", id],
    queryFn: () => evidenceApi.getEvidenceById(id!),
    enabled: !!id,
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["evidence", id] });

  const updateMutation = useMutation({
    mutationFn: (payload: UpdateEvidencePayload) => evidenceApi.updateEvidence(id!, payload),
    onSuccess: invalidate,
  });
  const submitMutation = useMutation({ mutationFn: () => evidenceApi.submitEvidenceForReview(id!), onSuccess: invalidate });
  const approveMutation = useMutation({ mutationFn: () => evidenceApi.approveEvidence(id!, null), onSuccess: invalidate });
  const rejectMutation = useMutation({ mutationFn: (reason: string) => evidenceApi.rejectEvidence(id!, reason), onSuccess: invalidate });
  const archiveMutation = useMutation({ mutationFn: () => evidenceApi.archiveEvidence(id!), onSuccess: invalidate });
  const newVersionMutation = useMutation({
    mutationFn: (vars: { file: File | null; externalUrl: string | null }) => evidenceApi.uploadEvidenceVersion(id!, vars.file, vars.externalUrl),
    onSuccess: invalidate,
  });

  if (isPending) return <CircularProgress />;
  if (isError || !evidence) return <Alert severity="error">Could not load this evidence item.</Alert>;

  const canUpload = hasPermission("evidence.upload");
  const canReview = hasPermission("evidence.review");
  const isArchived = evidence.status === "ARCHIVED";

  return (
    <Stack spacing={3}>
      <Button size="small" onClick={() => navigate("/evidence")} sx={{ alignSelf: "flex-start" }}>
        ← Back to Evidence Dashboard
      </Button>

      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "flex-start", flexWrap: "wrap", gap: 2 }}>
        <Stack spacing={0.5}>
          <Typography variant="h4" component="h1">
            {evidence.evidenceNumber} — {evidence.title}
          </Typography>
          <Stack direction="row" spacing={1}>
            <Chip label={evidence.evidenceType.replace("_", " ")} size="small" />
            <Chip label={evidence.status.replace("_", " ")} size="small" color={STATUS_COLORS[evidence.status] ?? "default"} />
            {evidence.isExpired && <Chip label="Expired" size="small" color="error" />}
          </Stack>
        </Stack>

        {!isArchived && (
          <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap" }}>
            {canUpload && (
              <Button variant="outlined" onClick={() => setEditOpen(true)}>
                Edit
              </Button>
            )}
            {canUpload && (
              <Button variant="outlined" onClick={() => setNewVersionOpen(true)}>
                New Version
              </Button>
            )}
            {canUpload && evidence.status === "UPLOADED" && (
              <Button variant="contained" onClick={() => submitMutation.mutate()} disabled={submitMutation.isPending || !evidence.reviewerUserId}>
                Submit for Review
              </Button>
            )}
            {canReview && evidence.status === "UNDER_REVIEW" && (
              <Button variant="contained" color="success" onClick={() => approveMutation.mutate()} disabled={approveMutation.isPending}>
                Approve
              </Button>
            )}
            {canReview && evidence.status === "UNDER_REVIEW" && (
              <Button variant="outlined" color="error" onClick={() => setRejectOpen(true)}>
                Reject
              </Button>
            )}
            {canReview && ["APPROVED", "REJECTED", "EXPIRED"].includes(evidence.status) && (
              <Button variant="outlined" onClick={() => archiveMutation.mutate()} disabled={archiveMutation.isPending}>
                Archive
              </Button>
            )}
          </Stack>
        )}
      </Stack>

      {canUpload && evidence.status === "UPLOADED" && !evidence.reviewerUserId && (
        <Alert severity="info">Assign a reviewer (via Edit) before submitting this evidence for review.</Alert>
      )}

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack spacing={1.5}>
          {evidence.description && <Typography variant="body1">{evidence.description}</Typography>}
          {evidence.controlBusinessId && (
            <Typography variant="body2" color="text.secondary">
              Control: {evidence.controlBusinessId} — {evidence.controlName}
            </Typography>
          )}
          {evidence.findingNumber && <Typography variant="body2">Finding: {evidence.findingNumber}</Typography>}
          {evidence.vendorReference && <Typography variant="body2">Vendor: {evidence.vendorReference}</Typography>}
          {evidence.processingActivityReference && <Typography variant="body2">Processing Activity: {evidence.processingActivityReference}</Typography>}
          <Typography variant="caption" color="text.secondary">
            Owner: {evidence.ownerName ?? "Unassigned"} · Reviewer: {evidence.reviewerName ?? "Unassigned"} · Expiry: {evidence.expiryDate ?? "—"}
          </Typography>
          {evidence.rejectionReason && (
            <Alert severity="error">Rejected: {evidence.rejectionReason}</Alert>
          )}
        </Stack>
      </Paper>

      <Typography variant="h5" component="h2">
        Version History
      </Typography>
      <TableContainer component={Paper} variant="outlined">
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Version</TableCell>
              <TableCell>File / URL</TableCell>
              <TableCell>Size</TableCell>
              <TableCell>Checksum (SHA-256)</TableCell>
              <TableCell>Malware Scan</TableCell>
              <TableCell>Uploaded By</TableCell>
              <TableCell>Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {evidence.versions.map((v) => (
              <TableRow key={v.id}>
                <TableCell>v{v.versionNumber}</TableCell>
                <TableCell>{v.originalFileName ?? v.externalUrl ?? "—"}</TableCell>
                <TableCell>{v.sizeBytes ? `${(v.sizeBytes / 1024).toFixed(1)} KB` : "—"}</TableCell>
                <TableCell sx={{ fontFamily: "monospace", fontSize: "0.75rem" }}>{v.checksumSha256?.slice(0, 16) ?? "—"}…</TableCell>
                <TableCell>
                  <Chip label={v.malwareScanStatus} size="small" color={v.malwareScanStatus === "CLEAN" ? "success" : v.malwareScanStatus === "INFECTED" ? "error" : "default"} />
                </TableCell>
                <TableCell>{v.uploadedByName}</TableCell>
                <TableCell>
                  <Stack direction="row" spacing={1}>
                    {v.originalFileName && (
                      <Button size="small" onClick={() => evidenceApi.downloadEvidenceVersion(evidence.id, v.versionNumber, v.originalFileName!)}>
                        Download
                      </Button>
                    )}
                    {v.isPreviewSafe && (
                      <Button size="small" onClick={() => setPreviewVersion(previewVersion === v.versionNumber ? null : v.versionNumber)}>
                        {previewVersion === v.versionNumber ? "Hide Preview" : "Preview"}
                      </Button>
                    )}
                  </Stack>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {previewVersion !== null && <PreviewPane evidenceId={evidence.id} versionNumber={previewVersion} />}

      <Typography variant="h5" component="h2">
        Review History
      </Typography>
      <TableContainer component={Paper} variant="outlined">
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Version</TableCell>
              <TableCell>Reviewer</TableCell>
              <TableCell>Decision</TableCell>
              <TableCell>Comments</TableCell>
              <TableCell>Date</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {evidence.reviews.map((r) => (
              <TableRow key={r.id}>
                <TableCell>v{r.evidenceVersionNumber}</TableCell>
                <TableCell>{r.reviewerName}</TableCell>
                <TableCell>
                  <Chip label={r.decision} size="small" color={r.decision === "APPROVED" ? "success" : "error"} />
                </TableCell>
                <TableCell>{r.comments ?? "—"}</TableCell>
                <TableCell>{new Date(r.createdAt).toLocaleString()}</TableCell>
              </TableRow>
            ))}
            {evidence.reviews.length === 0 && (
              <TableRow>
                <TableCell colSpan={5} align="center">
                  No reviews yet.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {editOpen && (
        <EditMetadataDialog
          evidence={evidence}
          onClose={() => setEditOpen(false)}
          onSave={(payload) => {
            updateMutation.mutate(payload);
            setEditOpen(false);
          }}
        />
      )}
      {newVersionOpen && (
        <NewVersionDialog
          evidenceType={evidence.evidenceType}
          onClose={() => setNewVersionOpen(false)}
          onUpload={(file, externalUrl) => {
            newVersionMutation.mutate({ file, externalUrl });
            setNewVersionOpen(false);
          }}
        />
      )}
      {rejectOpen && (
        <RejectDialog
          onClose={() => setRejectOpen(false)}
          onReject={(reason) => {
            rejectMutation.mutate(reason);
            setRejectOpen(false);
          }}
        />
      )}
    </Stack>
  );
}
