import { useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useNavigate, useSearchParams } from "react-router-dom";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import MenuItem from "@mui/material/MenuItem";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import * as usersApi from "../users/api";
import * as remediationApi from "./api";

export function CreateRemediationTaskPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const findingId = searchParams.get("findingId") ?? "";

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [ownerUserId, setOwnerUserId] = useState("");
  const [dueDate, setDueDate] = useState("");
  const [serverError, setServerError] = useState<string | null>(null);

  const { data: users } = useQuery({ queryKey: ["users-for-remediation"], queryFn: () => usersApi.getUsers(1, 100) });

  const createMutation = useMutation({
    mutationFn: () =>
      remediationApi.createRemediationTask({
        findingId,
        title,
        description: description || null,
        ownerUserId: ownerUserId || null,
        dueDate: dueDate || null,
      }),
  });

  const onSubmit = async () => {
    setServerError(null);
    try {
      const created = await createMutation.mutateAsync();
      navigate(`/remediation-tasks/${created.id}`);
    } catch {
      setServerError("Could not create this remediation task.");
    }
  };

  if (!findingId) {
    return <Alert severity="error">A remediation task must be created from a finding — go to a Finding's details page first.</Alert>;
  }

  return (
    <Stack spacing={3} sx={{ maxWidth: 600 }}>
      <Typography variant="h4" component="h1">
        New Remediation Task
      </Typography>
      <Paper variant="outlined" sx={{ p: 3 }}>
        <Stack spacing={2}>
          {serverError && <Alert severity="error">{serverError}</Alert>}
          <TextField label="Title" fullWidth value={title} onChange={(e) => setTitle(e.target.value)} />
          <TextField label="Description" fullWidth multiline rows={3} value={description} onChange={(e) => setDescription(e.target.value)} />
          <TextField select label="Owner" fullWidth value={ownerUserId} onChange={(e) => setOwnerUserId(e.target.value)}>
            <MenuItem value="">Unassigned</MenuItem>
            {users?.items.map((u) => (
              <MenuItem key={u.id} value={u.id}>
                {u.fullName}
              </MenuItem>
            ))}
          </TextField>
          <TextField label="Due date" type="date" fullWidth slotProps={{ inputLabel: { shrink: true } }} value={dueDate} onChange={(e) => setDueDate(e.target.value)} />
          <Stack direction="row" spacing={1} sx={{ justifyContent: "flex-end" }}>
            <Button onClick={() => navigate(-1)}>Cancel</Button>
            <Button variant="contained" disabled={!title || createMutation.isPending} onClick={onSubmit}>
              {createMutation.isPending ? "Creating…" : "Create Task"}
            </Button>
          </Stack>
        </Stack>
      </Paper>
    </Stack>
  );
}
