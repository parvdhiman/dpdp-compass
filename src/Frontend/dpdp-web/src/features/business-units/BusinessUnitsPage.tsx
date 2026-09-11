import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
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
import * as businessUnitsApi from "./api";
import { BusinessUnitDialog } from "./BusinessUnitDialog";
import type { BusinessUnit } from "./types";

const PAGE_SIZE = 25;

export function BusinessUnitsPage() {
  const { user, hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<BusinessUnit | null>(null);

  const { data, isPending, isError } = useQuery({
    queryKey: ["business-units", page, search],
    queryFn: () => businessUnitsApi.getBusinessUnits(page, PAGE_SIZE, search || undefined),
  });

  const deleteMutation = useMutation({
    mutationFn: businessUnitsApi.deleteBusinessUnit,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["business-units"] }),
  });

  const handleSaved = () => {
    setDialogOpen(false);
    queryClient.invalidateQueries({ queryKey: ["business-units"] });
  };

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
        <Typography variant="h4" component="h1">
          Business Units
        </Typography>
        {hasPermission("businessunits.create") && (
          <Button
            variant="contained"
            onClick={() => {
              setEditing(null);
              setDialogOpen(true);
            }}
          >
            Create Business Unit
          </Button>
        )}
      </Stack>

      <TextField
        label="Search"
        size="small"
        sx={{ maxWidth: 360 }}
        value={search}
        onChange={(e) => {
          setSearch(e.target.value);
          setPage(1);
        }}
      />

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load business units.</Alert>}

      {data && (
        <>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Head</TableCell>
                  <TableCell>Departments</TableCell>
                  <TableCell>Status</TableCell>
                  {(hasPermission("businessunits.update") || hasPermission("businessunits.delete")) && (
                    <TableCell align="right">Actions</TableCell>
                  )}
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((bu) => (
                  <TableRow key={bu.id}>
                    <TableCell>{bu.name}</TableCell>
                    <TableCell>{bu.head.name ?? "—"}</TableCell>
                    <TableCell>{bu.departmentCount}</TableCell>
                    <TableCell>
                      <Chip label={bu.isActive ? "Active" : "Inactive"} color={bu.isActive ? "success" : "default"} size="small" />
                    </TableCell>
                    <TableCell align="right">
                      {hasPermission("businessunits.update") && (
                        <Button
                          size="small"
                          onClick={() => {
                            setEditing(bu);
                            setDialogOpen(true);
                          }}
                        >
                          Edit
                        </Button>
                      )}
                      {hasPermission("businessunits.delete") && (
                        <Button size="small" color="error" onClick={() => deleteMutation.mutate(bu.id)}>
                          Delete
                        </Button>
                      )}
                    </TableCell>
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={5} align="center">
                      No business units found.
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

      {dialogOpen && user?.organisationId && (
        <BusinessUnitDialog
          organisationId={user.organisationId}
          businessUnit={editing}
          onClose={() => setDialogOpen(false)}
          onSaved={handleSaved}
        />
      )}
    </Stack>
  );
}
