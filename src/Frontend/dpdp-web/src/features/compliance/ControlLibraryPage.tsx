import { useState } from "react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
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
import { Link as RouterLink } from "react-router-dom";
import { useAuth } from "../auth/AuthProvider";
import * as complianceApi from "./api";
import { ControlDialog } from "./ControlDialog";
import { CONTROL_STATUSES, RISK_LEVELS } from "./types";

const PAGE_SIZE = 25;

const RISK_COLORS: Record<string, "success" | "warning" | "error" | "default"> = {
  LOW: "success",
  MEDIUM: "warning",
  HIGH: "error",
  CRITICAL: "error",
};

export function ControlLibraryPage() {
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [riskLevel, setRiskLevel] = useState("");
  const [status, setStatus] = useState("");
  const [createOpen, setCreateOpen] = useState(false);

  const canManage = hasPermission("controls.manage");

  const { data: categories } = useQuery({
    queryKey: ["compliance", "control-categories"],
    queryFn: complianceApi.getControlCategories,
  });

  const { data, isPending, isError } = useQuery({
    queryKey: ["compliance", "controls", page, search, categoryId, riskLevel, status],
    queryFn: () =>
      complianceApi.getControls({
        page,
        pageSize: PAGE_SIZE,
        search: search || undefined,
        categoryId: categoryId || undefined,
        riskLevel: riskLevel || undefined,
        status: status || undefined,
      }),
  });

  const resetPageAnd = (setter: (value: string) => void) => (value: string) => {
    setter(value);
    setPage(1);
  };

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
        <Typography variant="h4" component="h1">
          Control Library
        </Typography>
        {canManage && <Button variant="contained" onClick={() => setCreateOpen(true)}>Create Control</Button>}
      </Stack>

      <Stack direction="row" spacing={2} sx={{ flexWrap: "wrap" }}>
        <TextField
          label="Search"
          size="small"
          sx={{ minWidth: 220 }}
          value={search}
          onChange={(e) => resetPageAnd(setSearch)(e.target.value)}
        />
        <TextField
          select
          label="Category"
          size="small"
          sx={{ minWidth: 200 }}
          value={categoryId}
          onChange={(e) => resetPageAnd(setCategoryId)(e.target.value)}
        >
          <MenuItem value="">All categories</MenuItem>
          {categories?.map((category) => (
            <MenuItem key={category.id} value={category.id}>
              {category.name} ({category.controlCount})
            </MenuItem>
          ))}
        </TextField>
        <TextField
          select
          label="Risk level"
          size="small"
          sx={{ minWidth: 160 }}
          value={riskLevel}
          onChange={(e) => resetPageAnd(setRiskLevel)(e.target.value)}
        >
          <MenuItem value="">All risk levels</MenuItem>
          {RISK_LEVELS.map((level) => (
            <MenuItem key={level} value={level}>
              {level}
            </MenuItem>
          ))}
        </TextField>
        {canManage && (
          <TextField
            select
            label="Status"
            size="small"
            sx={{ minWidth: 160 }}
            value={status}
            onChange={(e) => resetPageAnd(setStatus)(e.target.value)}
          >
            <MenuItem value="">All statuses</MenuItem>
            {CONTROL_STATUSES.map((s) => (
              <MenuItem key={s} value={s}>
                {s}
              </MenuItem>
            ))}
          </TextField>
        )}
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load controls.</Alert>}

      {data && (
        <>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Control ID</TableCell>
                  <TableCell>Name</TableCell>
                  <TableCell>Category</TableCell>
                  <TableCell>Risk</TableCell>
                  {canManage && <TableCell>Status</TableCell>}
                  <TableCell align="right">Questions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((c) => (
                  <TableRow key={c.id} hover>
                    <TableCell>
                      <RouterLink to={`/compliance/controls/${c.id}`}>{c.controlId}</RouterLink>
                    </TableCell>
                    <TableCell>{c.name}</TableCell>
                    <TableCell>{c.categoryName}</TableCell>
                    <TableCell>
                      <Chip label={c.riskLevel} color={RISK_COLORS[c.riskLevel] ?? "default"} size="small" />
                    </TableCell>
                    {canManage && (
                      <TableCell>
                        <Chip label={c.status} size="small" variant="outlined" />
                      </TableCell>
                    )}
                    <TableCell align="right">{c.questionCount}</TableCell>
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={canManage ? 6 : 5} align="center">
                      No controls found.
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
        <ControlDialog
          control={null}
          onClose={() => setCreateOpen(false)}
          onSaved={() => {
            setCreateOpen(false);
            queryClient.invalidateQueries({ queryKey: ["compliance", "controls"] });
          }}
        />
      )}
    </Stack>
  );
}
