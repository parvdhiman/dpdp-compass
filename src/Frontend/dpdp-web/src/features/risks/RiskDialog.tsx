import { useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import * as usersApi from "../users/api";
import * as risksApi from "./api";
import { IMPACT_LEVELS, LIKELIHOOD_LEVELS, RISK_LEVELS, RISK_STATUSES, type RiskDetail } from "./types";

export function RiskDialog({ risk, onClose, onSaved }: { risk: RiskDetail | null; onClose: () => void; onSaved: () => void }) {
  const [serverError, setServerError] = useState<string | null>(null);
  const { data: users } = useQuery({ queryKey: ["users-for-risk"], queryFn: () => usersApi.getUsers(1, 100) });

  const [title, setTitle] = useState(risk?.title ?? "");
  const [description, setDescription] = useState(risk?.description ?? "");
  const [likelihood, setLikelihood] = useState(risk?.likelihood ?? "POSSIBLE");
  const [impact, setImpact] = useState(risk?.impact ?? "MODERATE");
  const [dataSensitivity, setDataSensitivity] = useState(risk?.dataSensitivity ?? "MEDIUM");
  const [exposure, setExposure] = useState(risk?.exposure ?? "MEDIUM");
  const [ownerUserId, setOwnerUserId] = useState(risk?.ownerUserId ?? "");
  const [status, setStatus] = useState(risk?.status ?? "OPEN");
  const [treatmentPlan, setTreatmentPlan] = useState(risk?.treatmentPlan ?? "");
  const [submitting, setSubmitting] = useState(false);

  const saveMutation = useMutation({
    mutationFn: () => {
      const payload = { title, description, likelihood, impact, dataSensitivity, exposure, ownerUserId: ownerUserId || null, treatmentPlan: treatmentPlan || null };
      return risk ? risksApi.updateRisk(risk.id, { ...payload, status }) : risksApi.createRisk(payload);
    },
  });

  const onSubmit = async () => {
    setServerError(null);
    setSubmitting(true);
    try {
      await saveMutation.mutateAsync();
      onSaved();
    } catch {
      setServerError("Could not save this risk.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{risk ? "Edit Risk" : "New Risk"}</DialogTitle>
      <DialogContent>
        <Stack spacing={2} sx={{ pt: 1 }}>
          {serverError && <Alert severity="error">{serverError}</Alert>}
          <TextField label="Title" fullWidth value={title} onChange={(e) => setTitle(e.target.value)} />
          <TextField label="Description" fullWidth multiline rows={3} value={description} onChange={(e) => setDescription(e.target.value)} />
          <Stack direction="row" spacing={2}>
            <TextField select label="Likelihood" fullWidth value={likelihood} onChange={(e) => setLikelihood(e.target.value)}>
              {LIKELIHOOD_LEVELS.map((l) => (
                <MenuItem key={l} value={l}>
                  {l.replace("_", " ")}
                </MenuItem>
              ))}
            </TextField>
            <TextField select label="Impact" fullWidth value={impact} onChange={(e) => setImpact(e.target.value)}>
              {IMPACT_LEVELS.map((i) => (
                <MenuItem key={i} value={i}>
                  {i}
                </MenuItem>
              ))}
            </TextField>
          </Stack>
          <Stack direction="row" spacing={2}>
            <TextField select label="Data Sensitivity" fullWidth value={dataSensitivity} onChange={(e) => setDataSensitivity(e.target.value)}>
              {RISK_LEVELS.map((l) => (
                <MenuItem key={l} value={l}>
                  {l}
                </MenuItem>
              ))}
            </TextField>
            <TextField select label="Exposure" fullWidth value={exposure} onChange={(e) => setExposure(e.target.value)}>
              {RISK_LEVELS.map((l) => (
                <MenuItem key={l} value={l}>
                  {l}
                </MenuItem>
              ))}
            </TextField>
          </Stack>
          <TextField select label="Owner" fullWidth value={ownerUserId} onChange={(e) => setOwnerUserId(e.target.value)}>
            <MenuItem value="">Unassigned</MenuItem>
            {users?.items.map((u) => (
              <MenuItem key={u.id} value={u.id}>
                {u.fullName}
              </MenuItem>
            ))}
          </TextField>
          {risk && (
            <TextField select label="Status" fullWidth value={status} onChange={(e) => setStatus(e.target.value)}>
              {RISK_STATUSES.map((s) => (
                <MenuItem key={s} value={s}>
                  {s}
                </MenuItem>
              ))}
            </TextField>
          )}
          <TextField label="Treatment plan" fullWidth multiline rows={2} value={treatmentPlan} onChange={(e) => setTreatmentPlan(e.target.value)} />
        </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button variant="contained" disabled={!title || !description || submitting} onClick={onSubmit}>
          {submitting ? "Saving…" : "Save"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
