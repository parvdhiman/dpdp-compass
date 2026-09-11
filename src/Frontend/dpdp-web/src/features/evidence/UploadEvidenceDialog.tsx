import { useRef, useState } from "react";
import { useMutation } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import * as evidenceApi from "./api";
import { EVIDENCE_TYPES } from "./types";

/**
 * Standalone upload dialog. The preset* props let a future "Evidence" tab
 * on Assessment/Control/Finding detail pages pre-link a new item to that
 * record without duplicating this form — see docs/EVIDENCE_STORAGE.md.
 */
export function UploadEvidenceDialog({
  onClose,
  onUploaded,
  presetAssessmentId,
  presetControlId,
  presetFindingId,
}: {
  onClose: () => void;
  onUploaded: (id: string) => void;
  presetAssessmentId?: string;
  presetControlId?: string;
  presetFindingId?: string;
}) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [evidenceType, setEvidenceType] = useState("PDF");
  const [externalUrl, setExternalUrl] = useState("");
  const [expiryDate, setExpiryDate] = useState("");
  const [file, setFile] = useState<File | null>(null);
  const [serverError, setServerError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const isUrlType = evidenceType === "URL";

  const mutation = useMutation({ mutationFn: evidenceApi.uploadEvidence });

  const canSubmit = title.trim().length > 0 && (isUrlType ? externalUrl.trim().length > 0 : !!file);

  const onSubmit = async () => {
    setServerError(null);
    try {
      const created = await mutation.mutateAsync({
        title,
        description: description || null,
        evidenceType,
        assessmentId: presetAssessmentId ?? null,
        controlId: presetControlId ?? null,
        findingId: presetFindingId ?? null,
        expiryDate: expiryDate || null,
        externalUrl: isUrlType ? externalUrl : null,
        file: isUrlType ? null : file,
      });
      onUploaded(created.id);
    } catch {
      setServerError("Could not upload this evidence. Check the file type and size and try again.");
    }
  };

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Upload Evidence</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          {serverError && <Alert severity="error">{serverError}</Alert>}
          <TextField label="Title" fullWidth required value={title} onChange={(e) => setTitle(e.target.value)} />
          <TextField label="Description" fullWidth multiline rows={2} value={description} onChange={(e) => setDescription(e.target.value)} />
          <TextField
            select
            label="Evidence Type"
            fullWidth
            value={evidenceType}
            onChange={(e) => {
              setEvidenceType(e.target.value);
              setFile(null);
              setExternalUrl("");
            }}
          >
            {EVIDENCE_TYPES.map((t) => (
              <MenuItem key={t} value={t}>
                {t.replace("_", " ")}
              </MenuItem>
            ))}
          </TextField>

          {isUrlType ? (
            <TextField
              label="URL"
              fullWidth
              required
              placeholder="https://…"
              value={externalUrl}
              onChange={(e) => setExternalUrl(e.target.value)}
            />
          ) : (
            <Stack spacing={1}>
              <Button variant="outlined" onClick={() => fileInputRef.current?.click()}>
                {file ? "Change File" : "Choose File"}
              </Button>
              <input
                ref={fileInputRef}
                type="file"
                hidden
                onChange={(e) => setFile(e.target.files?.[0] ?? null)}
                accept=".pdf,.png,.jpg,.jpeg,.gif,.docx,.xlsx,.txt,.csv,.json,.xml,.yaml,.yml"
              />
              {file && (
                <Typography variant="body2" color="text.secondary">
                  {file.name} ({(file.size / 1024).toFixed(1)} KB)
                </Typography>
              )}
            </Stack>
          )}

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
        <Button variant="contained" disabled={!canSubmit || mutation.isPending} onClick={onSubmit}>
          {mutation.isPending ? "Uploading…" : "Upload"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
