import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
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
import Typography from "@mui/material/Typography";
import { useAuth } from "../auth/AuthProvider";
import * as findingsApi from "./api";
import { FINDING_SEVERITIES, FINDING_STATUSES } from "./types";

const PAGE_SIZE = 20;

const SEVERITY_COLORS: Record<string, "error" | "warning" | "info" | "default"> = {
  CRITICAL: "error",
  HIGH: "error",
  MEDIUM: "warning",
  LOW: "info",
  INFORMATIONAL: "default",
};

const schema = z.object({
  title: z.string().min(1, "Title is required").max(300),
  description: z.string().min(1, "Description is required"),
  severity: z.string().min(1),
});
type FormValues = z.infer<typeof schema>;

function CreateFindingDialog({ onClose, onCreated }: { onClose: () => void; onCreated: (id: string) => void }) {
  const [serverError, setServerError] = useState<string | null>(null);
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema), defaultValues: { title: "", description: "", severity: "MEDIUM" } });

  const mutation = useMutation({ mutationFn: (values: FormValues) => findingsApi.createFinding(values) });

  const onSubmit = async (values: FormValues) => {
    setServerError(null);
    try {
      const created = await mutation.mutateAsync(values);
      onCreated(created.id);
    } catch {
      setServerError("Could not create this finding.");
    }
  };

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>New Finding</DialogTitle>
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            {serverError && <Alert severity="error">{serverError}</Alert>}
            <TextField label="Title" fullWidth error={!!errors.title} helperText={errors.title?.message} {...register("title")} />
            <TextField label="Description" fullWidth multiline rows={3} error={!!errors.description} helperText={errors.description?.message} {...register("description")} />
            <TextField select label="Severity" fullWidth defaultValue="MEDIUM" {...register("severity")}>
              {FINDING_SEVERITIES.map((s) => (
                <MenuItem key={s} value={s}>
                  {s}
                </MenuItem>
              ))}
            </TextField>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Cancel</Button>
          <Button type="submit" variant="contained" disabled={isSubmitting}>
            {isSubmitting ? "Creating…" : "Create"}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}

export function FindingDashboardPage() {
  const { hasPermission } = useAuth();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");
  const [severity, setSeverity] = useState("");
  const [createOpen, setCreateOpen] = useState(false);

  const { data, isPending, isError } = useQuery({
    queryKey: ["findings", page, search, status, severity],
    queryFn: () => findingsApi.getFindings({ page, pageSize: PAGE_SIZE, search: search || undefined, status: status || undefined, severity: severity || undefined }),
  });

  const openCount = data?.items.filter((f) => !["CLOSED", "ACCEPTED_RISK"].includes(f.status)).length ?? 0;
  const overdueCount = data?.items.filter((f) => f.isOverdue).length ?? 0;
  const criticalCount = data?.items.filter((f) => f.severity === "CRITICAL").length ?? 0;

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
        <Typography variant="h4" component="h1">
          Finding Dashboard
        </Typography>
        {hasPermission("findings.create") && (
          <Button variant="contained" onClick={() => setCreateOpen(true)}>
            New Finding
          </Button>
        )}
      </Stack>

      <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap" }}>
        <Card variant="outlined" sx={{ minWidth: 160 }}>
          <CardContent>
            <Typography variant="body2" color="text.secondary">
              Open (this page)
            </Typography>
            <Typography variant="h4">{openCount}</Typography>
          </CardContent>
        </Card>
        <Card variant="outlined" sx={{ minWidth: 160 }}>
          <CardContent>
            <Typography variant="body2" color="text.secondary">
              Overdue (this page)
            </Typography>
            <Typography variant="h4" color={overdueCount > 0 ? "error.main" : undefined}>
              {overdueCount}
            </Typography>
          </CardContent>
        </Card>
        <Card variant="outlined" sx={{ minWidth: 160 }}>
          <CardContent>
            <Typography variant="body2" color="text.secondary">
              Critical (this page)
            </Typography>
            <Typography variant="h4">{criticalCount}</Typography>
          </CardContent>
        </Card>
      </Stack>

      <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap" }}>
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
          sx={{ minWidth: 180 }}
          value={status}
          onChange={(e) => {
            setStatus(e.target.value);
            setPage(1);
          }}
        >
          <MenuItem value="">All statuses</MenuItem>
          {FINDING_STATUSES.map((s) => (
            <MenuItem key={s} value={s}>
              {s.replace("_", " ")}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          label="Severity"
          size="small"
          sx={{ minWidth: 160 }}
          value={severity}
          onChange={(e) => {
            setSeverity(e.target.value);
            setPage(1);
          }}
        >
          <MenuItem value="">All severities</MenuItem>
          {FINDING_SEVERITIES.map((s) => (
            <MenuItem key={s} value={s}>
              {s}
            </MenuItem>
          ))}
        </TextField>
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load findings.</Alert>}

      {data && (
        <>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>ID</TableCell>
                  <TableCell>Title</TableCell>
                  <TableCell>Severity</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Owner</TableCell>
                  <TableCell>Due Date</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((f) => (
                  <TableRow key={f.id} hover sx={{ cursor: "pointer" }} onClick={() => navigate(`/findings/${f.id}`)}>
                    <TableCell>{f.findingNumber}</TableCell>
                    <TableCell>{f.title}</TableCell>
                    <TableCell>
                      <Chip label={f.severity} size="small" color={SEVERITY_COLORS[f.severity] ?? "default"} />
                    </TableCell>
                    <TableCell>
                      <Chip label={f.status.replace("_", " ")} size="small" variant="outlined" />
                    </TableCell>
                    <TableCell>{f.ownerName ?? "Unassigned"}</TableCell>
                    <TableCell>
                      {f.dueDate ?? "—"} {f.isOverdue && <Chip label="Overdue" size="small" color="error" />}
                    </TableCell>
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={6} align="center">
                      No findings found.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>

          <Box sx={{ display: "flex", justifyContent: "center" }}>
            <Pagination count={data.totalPages} page={data.page} onChange={(_, value) => setPage(value)} color="primary" />
          </Box>
        </>
      )}

      {createOpen && (
        <CreateFindingDialog
          onClose={() => setCreateOpen(false)}
          onCreated={(id) => {
            setCreateOpen(false);
            queryClient.invalidateQueries({ queryKey: ["findings"] });
            navigate(`/findings/${id}`);
          }}
        />
      )}
    </Stack>
  );
}
