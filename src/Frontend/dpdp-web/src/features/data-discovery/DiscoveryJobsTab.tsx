import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import Collapse from "@mui/material/Collapse";
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
import * as api from "./api";
import { DISCOVERY_JOB_STATUSES } from "./types";

const PAGE_SIZE = 20;
const ACTIVE_STATUSES = new Set(["PENDING", "RUNNING"]);

const STATUS_COLORS: Record<string, "success" | "warning" | "error" | "info" | "default"> = {
  PENDING: "default",
  RUNNING: "info",
  COMPLETED: "success",
  FAILED: "error",
  CANCELLED: "warning",
};

function JobResults({ jobId }: { jobId: string }) {
  const { data, isPending } = useQuery({ queryKey: ["discovery-job", jobId], queryFn: () => api.getDiscoveryJobById(jobId) });

  if (isPending) return <CircularProgress size={20} />;
  if (!data || data.results.length === 0) return <Typography variant="body2">No assets processed yet.</Typography>;

  return (
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell>Asset</TableCell>
          <TableCell>Row Count</TableCell>
          <TableCell>Columns</TableCell>
          <TableCell>Scanned At</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {data.results.map((r) => (
          <TableRow key={r.id}>
            <TableCell>{r.assetName}</TableCell>
            <TableCell>{r.rowCountAtScan ?? "—"}</TableCell>
            <TableCell>{r.columnsDiscovered}</TableCell>
            <TableCell>{new Date(r.scannedAt).toLocaleString()}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}

export function DiscoveryJobsTab({ dataSourceId }: { dataSourceId?: string }) {
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [status, setStatus] = useState("");
  const [expandedJobId, setExpandedJobId] = useState<string | null>(null);

  const canManage = hasPermission("discoveryjobs.manage");

  const { data, isPending, isError } = useQuery({
    queryKey: ["discovery-jobs", page, status, dataSourceId],
    queryFn: () => api.getDiscoveryJobs({ page, pageSize: PAGE_SIZE, status: status || undefined, dataSourceId }),
    refetchInterval: (query) => (query.state.data?.items.some((j) => ACTIVE_STATUSES.has(j.status)) ? 3000 : false),
  });

  const cancelMutation = useMutation({
    mutationFn: (id: string) => api.cancelDiscoveryJob(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["discovery-jobs"] }),
  });

  return (
    <Stack spacing={2}>
      <Stack direction="row" spacing={2}>
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
          {DISCOVERY_JOB_STATUSES.map((s) => (
            <MenuItem key={s} value={s}>
              {s}
            </MenuItem>
          ))}
        </TextField>
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load discovery jobs.</Alert>}

      {data && (
        <Stack spacing={2}>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell />
                  <TableCell>Data Source</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Triggered By</TableCell>
                  <TableCell>Assets</TableCell>
                  <TableCell>Elements</TableCell>
                  <TableCell>Started</TableCell>
                  <TableCell>Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((job) => (
                  <>
                    <TableRow key={job.id} hover>
                      <TableCell>
                        <Button size="small" onClick={() => setExpandedJobId(expandedJobId === job.id ? null : job.id)}>
                          {expandedJobId === job.id ? "▾" : "▸"}
                        </Button>
                      </TableCell>
                      <TableCell>{job.dataSourceName}</TableCell>
                      <TableCell>
                        <Chip size="small" label={job.status} color={STATUS_COLORS[job.status] ?? "default"} />
                      </TableCell>
                      <TableCell>{job.triggeredByName}</TableCell>
                      <TableCell>{job.assetsDiscoveredCount}</TableCell>
                      <TableCell>{job.elementsDiscoveredCount}</TableCell>
                      <TableCell>{job.startedAt ? new Date(job.startedAt).toLocaleString() : "—"}</TableCell>
                      <TableCell>
                        {canManage && ACTIVE_STATUSES.has(job.status) && (
                          <Button size="small" color="warning" onClick={() => cancelMutation.mutate(job.id)}>
                            Cancel
                          </Button>
                        )}
                      </TableCell>
                    </TableRow>
                    {expandedJobId === job.id && (
                      <TableRow key={`${job.id}-detail`}>
                        <TableCell colSpan={8} sx={{ py: 0 }}>
                          <Collapse in>
                            <Box sx={{ p: 2 }}>
                              {job.errorMessage && <Alert severity="error" sx={{ mb: 1 }}>{job.errorMessage}</Alert>}
                              <JobResults jobId={job.id} />
                            </Box>
                          </Collapse>
                        </TableCell>
                      </TableRow>
                    )}
                  </>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={8} align="center">
                      No discovery jobs found.
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
    </Stack>
  );
}
