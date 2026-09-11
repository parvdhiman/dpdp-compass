import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
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
import * as remediationApi from "./api";
import { REMEDIATION_STATUSES } from "./types";

const PAGE_SIZE = 20;

const STATUS_COLORS: Record<string, "default" | "info" | "warning" | "success"> = {
  OPEN: "default",
  IN_PROGRESS: "info",
  PENDING_VERIFICATION: "warning",
  VERIFIED: "success",
  CLOSED: "default",
};

// Shared by the Remediation Dashboard (overdueOnly=false) and Overdue Tasks (overdueOnly=true) routes — the same list, filtered differently, per docs/ARCHITECTURE.md Module 6 section.
export function RemediationTaskListPage({ title, overdueOnly }: { title: string; overdueOnly: boolean }) {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");

  const { data, isPending, isError } = useQuery({
    queryKey: ["remediation-tasks", overdueOnly, page, search, status],
    queryFn: () => remediationApi.getRemediationTasks({ page, pageSize: PAGE_SIZE, search: search || undefined, status: status || undefined, overdueOnly }),
  });

  return (
    <Stack spacing={3}>
      <Typography variant="h4" component="h1">
        {title}
      </Typography>

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
        {!overdueOnly && (
          <TextField
            select
            label="Status"
            size="small"
            sx={{ minWidth: 200 }}
            value={status}
            onChange={(e) => {
              setStatus(e.target.value);
              setPage(1);
            }}
          >
            <MenuItem value="">All statuses</MenuItem>
            {REMEDIATION_STATUSES.map((s) => (
              <MenuItem key={s} value={s}>
                {s.replace("_", " ")}
              </MenuItem>
            ))}
          </TextField>
        )}
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load remediation tasks.</Alert>}

      {data && (
        <>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Finding</TableCell>
                  <TableCell>Task</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Owner</TableCell>
                  <TableCell>Due Date</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((t) => (
                  <TableRow key={t.id} hover sx={{ cursor: "pointer" }} onClick={() => navigate(`/remediation-tasks/${t.id}`)}>
                    <TableCell>{t.findingNumber}</TableCell>
                    <TableCell>{t.title}</TableCell>
                    <TableCell>
                      <Chip label={t.status.replace("_", " ")} size="small" color={STATUS_COLORS[t.status] ?? "default"} />
                    </TableCell>
                    <TableCell>{t.ownerName ?? "Unassigned"}</TableCell>
                    <TableCell>
                      {t.dueDate ?? "—"} {t.isOverdue && <Chip label="Overdue" size="small" color="error" />}
                    </TableCell>
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={5} align="center">
                      {overdueOnly ? "No overdue tasks — nice work." : "No remediation tasks found."}
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
    </Stack>
  );
}
