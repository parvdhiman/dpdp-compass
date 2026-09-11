import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import Pagination from "@mui/material/Pagination";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import Paper from "@mui/material/Paper";
import { Link as RouterLink } from "react-router-dom";
import { useAuth } from "../auth/AuthProvider";
import * as usersApi from "./api";

const PAGE_SIZE = 25;

export function UserListPage() {
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");

  const { data, isPending, isError } = useQuery({
    queryKey: ["users", page, search],
    queryFn: () => usersApi.getUsers(page, PAGE_SIZE, search || undefined),
  });

  const toggleActive = useMutation({
    mutationFn: ({ id, activate }: { id: string; activate: boolean }) =>
      activate ? usersApi.activateUser(id) : usersApi.deactivateUser(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["users"] }),
  });

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
        <Typography variant="h4" component="h1">
          Users
        </Typography>
        {hasPermission("users.create") && (
          <Button component={RouterLink} to="/users/new" variant="contained">
            Create User
          </Button>
        )}
      </Stack>

      <TextField
        label="Search by name or email"
        value={search}
        onChange={(e) => {
          setSearch(e.target.value);
          setPage(1);
        }}
        size="small"
        sx={{ maxWidth: 360 }}
      />

      {isPending && <CircularProgress />}
      {isError && <Alert severity="error">Could not load users.</Alert>}

      {data && (
        <>
          <TableContainer component={Paper} variant="outlined">
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Name</TableCell>
                  <TableCell>Email</TableCell>
                  <TableCell>Roles</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell align="right">Actions</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data.items.map((user) => (
                  <TableRow key={user.id}>
                    <TableCell>{user.fullName}</TableCell>
                    <TableCell>{user.email}</TableCell>
                    <TableCell>
                      {user.roles.map((r) => (
                        <Chip key={r.roleId} label={r.roleName} size="small" sx={{ mr: 0.5 }} />
                      ))}
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={user.isActive ? "Active" : "Inactive"}
                        color={user.isActive ? "success" : "default"}
                        size="small"
                      />
                    </TableCell>
                    <TableCell align="right">
                      <Stack direction="row" spacing={1} sx={{ justifyContent: "flex-end" }}>
                        {hasPermission("users.update") && (
                          <Button component={RouterLink} to={`/users/${user.id}/edit`} size="small">
                            Edit
                          </Button>
                        )}
                        {hasPermission("users.disable") && (
                          <Button
                            size="small"
                            color={user.isActive ? "warning" : "success"}
                            disabled={toggleActive.isPending}
                            onClick={() => toggleActive.mutate({ id: user.id, activate: !user.isActive })}
                          >
                            {user.isActive ? "Deactivate" : "Activate"}
                          </Button>
                        )}
                      </Stack>
                    </TableCell>
                  </TableRow>
                ))}
                {data.items.length === 0 && (
                  <TableRow>
                    <TableCell colSpan={5} align="center">
                      No users found.
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>

          <Box sx={{ display: "flex", justifyContent: "center" }}>
            <Pagination
              count={data.totalPages}
              page={data.page}
              onChange={(_, value) => setPage(value)}
              color="primary"
            />
          </Box>
        </>
      )}
    </Stack>
  );
}
