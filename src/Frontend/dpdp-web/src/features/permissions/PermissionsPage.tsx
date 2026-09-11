import { useQuery } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import CircularProgress from "@mui/material/CircularProgress";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableContainer from "@mui/material/TableContainer";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import * as rolesApi from "../roles/api";
import type { PermissionDto } from "../roles/types";

export function PermissionsPage() {
  const { data: permissions, isPending, isError } = useQuery({
    queryKey: ["permissions"],
    queryFn: rolesApi.getPermissions,
  });

  if (isPending) return <CircularProgress />;
  if (isError) return <Alert severity="error">Could not load permissions.</Alert>;

  const byModule = new Map<string, PermissionDto[]>();
  for (const permission of permissions!) {
    const bucket = byModule.get(permission.module) ?? [];
    bucket.push(permission);
    byModule.set(permission.module, bucket);
  }

  return (
    <Stack spacing={3}>
      <Typography variant="h4" component="h1">
        Permissions
      </Typography>
      <Typography variant="body2" color="text.secondary">
        The full permission catalogue. Only a Super Administrator can change which roles hold which
        permissions — see the Roles page.
      </Typography>

      {Array.from(byModule.entries()).map(([module, modulePermissions]) => (
        <Paper key={module} variant="outlined">
          <Typography variant="subtitle1" sx={{ fontWeight: 600, p: 2, pb: 0 }}>
            {module}
          </Typography>
          <TableContainer>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Key</TableCell>
                  <TableCell>Description</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {modulePermissions.map((permission) => (
                  <TableRow key={permission.id}>
                    <TableCell>
                      <code>{permission.key}</code>
                    </TableCell>
                    <TableCell>{permission.description}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </Paper>
      ))}
    </Stack>
  );
}
