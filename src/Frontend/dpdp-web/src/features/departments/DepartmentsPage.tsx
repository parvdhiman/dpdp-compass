import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
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
import { useAuth } from "../auth/AuthProvider";
import * as businessUnitsApi from "../business-units/api";
import * as departmentsApi from "./api";
import { DepartmentDialog } from "./DepartmentDialog";
import type { Department } from "./types";

const PAGE_SIZE = 25;

export function DepartmentsPage() {
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [businessUnitId, setBusinessUnitId] = useState("");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<Department | null>(null);

  const { data: businessUnits } = useQuery({
    queryKey: ["business-units", "all-for-select"],
    queryFn: () => businessUnitsApi.getBusinessUnits(1, 100),
  });

  const { data, isPending, isError } = useQuery({
    queryKey: ["departments", page, search, businessUnitId],
    queryFn: () => departmentsApi.getDepartments(page, PAGE_SIZE, search || undefined, businessUnitId || undefined),
  });

  const deleteMutation = useMutation({
    mutationFn: departmentsApi.deleteDepartment,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["departments"] }),
  });

  const handleSaved = () => {
    setDialogOpen(false);
    queryClient.invalidateQueries({ queryKey: ["departments"] });
  };

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
        <Typography variant="h4" component="h1">
          Departments
        </Typography>
        {hasPermission("departments.create") && (
          <Button
            variant="contained"
            onClick={() => {
              setEditing(null);
              setDialogOpen(true);
            }}
          >
            Create Department
          </Button>
        )}
      </Stack>

      <Stack direction="row" spacing={2}>
        <TextField
          label="Search"
          size="small"
          sx={{ maxWidth: 300 }}
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(1);
          }}
        />
        <TextField
          label="Business Unit"
          select
          size="small"
          sx={{ minWidth: 220 }}
          value={businessUnitId}
          onChange={(e) => {
            setBusinessUnitId(e.target.value);
            setPage(1);
          }}
        >
          <MenuItem value="">All business units</MenuItem>
          {(businessUnits?.items ?? []).map((bu) => (
            <MenuItem key={bu.id} value={bu.id}>
              {bu.name}
            </MenuItem>
          ))}
        </TextField>
      </Stack>

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load departments.</Alert>}

      {data && (
        <>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Business Unit</TableCell>
                  <TableCell>Head</TableCell>
                  <TableCell>Status</TableCell>
                  {(hasPermission("departments.update") || hasPermission("departments.delete")) && (
                    <TableCell align="right">Actions</TableCell>
                  )}
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((dept) => (
                  <TableRow key={dept.id}>
                    <TableCell>{dept.name}</TableCell>
                    <TableCell>{dept.businessUnitName}</TableCell>
                    <TableCell>{dept.head.name ?? "—"}</TableCell>
                    <TableCell>
                      <Chip label={dept.isActive ? "Active" : "Inactive"} color={dept.isActive ? "success" : "default"} size="small" />
                    </TableCell>
                    <TableCell align="right">
                      {hasPermission("departments.update") && (
                        <Button
                          size="small"
                          onClick={() => {
                            setEditing(dept);
                            setDialogOpen(true);
                          }}
                        >
                          Edit
                        </Button>
                      )}
                      {hasPermission("departments.delete") && (
                        <Button size="small" color="error" onClick={() => deleteMutation.mutate(dept.id)}>
                          Delete
                        </Button>
                      )}
                    </TableCell>
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={5} align="center">
                      No departments found.
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

      {dialogOpen && (
        <DepartmentDialog
          department={editing}
          defaultBusinessUnitId={businessUnitId || undefined}
          onClose={() => setDialogOpen(false)}
          onSaved={handleSaved}
        />
      )}
    </Stack>
  );
}
