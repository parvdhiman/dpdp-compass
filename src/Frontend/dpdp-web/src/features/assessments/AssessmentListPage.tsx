import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import LinearProgress from "@mui/material/LinearProgress";
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
import { Link as RouterLink, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthProvider";
import * as assessmentsApi from "./api";
import { ASSESSMENT_STATUSES } from "./types";

const PAGE_SIZE = 20;

const STATUS_COLORS: Record<string, "default" | "info" | "warning" | "success" | "error"> = {
  DRAFT: "default",
  IN_PROGRESS: "info",
  SUBMITTED: "warning",
  UNDER_REVIEW: "warning",
  APPROVED: "success",
  REJECTED: "error",
  ARCHIVED: "default",
};

export function AssessmentListPage() {
  const { hasPermission } = useAuth();
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");

  const { data, isPending, isError } = useQuery({
    queryKey: ["assessments", page, search, status],
    queryFn: () => assessmentsApi.getAssessments({ page, pageSize: PAGE_SIZE, search: search || undefined, status: status || undefined }),
  });

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
        <Typography variant="h4" component="h1">
          Compliance Assessments
        </Typography>
        {hasPermission("assessments.create") && (
          <Stack direction="row" spacing={1}>
            <Button component={RouterLink} to="/assessments/wizard" variant="outlined">
              Guided Setup
            </Button>
            <Button component={RouterLink} to="/assessments/new" variant="contained">
              New Assessment
            </Button>
          </Stack>
        )}
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
          {ASSESSMENT_STATUSES.map((s) => (
            <MenuItem key={s} value={s}>
              {s.replace("_", " ")}
            </MenuItem>
          ))}
        </TextField>
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load assessments.</Alert>}

      {data && (
        <>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Framework</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Assigned To</TableCell>
                  <TableCell>Due Date</TableCell>
                  <TableCell sx={{ minWidth: 140 }}>Progress</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((a) => (
                  <TableRow key={a.id} hover sx={{ cursor: "pointer" }} onClick={() => navigate(`/assessments/${a.id}`)}>
                    <TableCell>{a.name}</TableCell>
                    <TableCell>
                      {a.frameworkName} ({a.frameworkVersionLabel})
                    </TableCell>
                    <TableCell>
                      <Chip label={a.status.replace("_", " ")} size="small" color={STATUS_COLORS[a.status] ?? "default"} />
                    </TableCell>
                    <TableCell>{a.assignedToUserName ?? "Unassigned"}</TableCell>
                    <TableCell>{a.dueDate ?? "—"}</TableCell>
                    <TableCell>
                      <Stack spacing={0.5}>
                        <LinearProgress
                          variant="determinate"
                          value={a.totalControls === 0 ? 0 : (a.completedControls / a.totalControls) * 100}
                        />
                        <Typography variant="caption" color="text.secondary">
                          {a.completedControls}/{a.totalControls} controls
                        </Typography>
                      </Stack>
                    </TableCell>
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={6} align="center">
                      No assessments found.
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
