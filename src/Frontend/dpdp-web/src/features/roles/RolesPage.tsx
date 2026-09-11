import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Accordion from "@mui/material/Accordion";
import AccordionDetails from "@mui/material/AccordionDetails";
import AccordionSummary from "@mui/material/AccordionSummary";
import Alert from "@mui/material/Alert";
import Checkbox from "@mui/material/Checkbox";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import FormControlLabel from "@mui/material/FormControlLabel";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { useAuth } from "../auth/AuthProvider";
import * as rolesApi from "./api";

export function RolesPage() {
  const { user } = useAuth();
  const queryClient = useQueryClient();

  const { data: roles, isPending, isError } = useQuery({ queryKey: ["roles"], queryFn: rolesApi.getRoles });
  const { data: permissions } = useQuery({ queryKey: ["permissions"], queryFn: rolesApi.getPermissions });

  const assignMutation = useMutation({
    mutationFn: ({ roleId, permissionId }: { roleId: string; permissionId: string }) =>
      rolesApi.assignRolePermission(roleId, permissionId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["roles"] }),
  });
  const revokeMutation = useMutation({
    mutationFn: ({ roleId, permissionId }: { roleId: string; permissionId: string }) =>
      rolesApi.revokeRolePermission(roleId, permissionId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["roles"] }),
  });

  if (isPending) return <CircularProgress />;
  if (isError) return <Alert severity="error">Could not load roles.</Alert>;

  const canEditTemplates = user?.isSuperAdministrator === true;

  return (
    <Stack spacing={3}>
      <Typography variant="h4" component="h1">
        Roles
      </Typography>
      {!canEditTemplates && (
        <Alert severity="info">
          Only a Super Administrator can change a role's permission template — see docs/ARCHITECTURE.md.
        </Alert>
      )}

      {roles!.map((role) => {
        const assignedIds = new Set(role.permissions.map((p) => p.id));

        return (
          <Accordion key={role.id}>
            <AccordionSummary expandIcon={<ExpandMoreIcon />}>
              <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
                <Typography sx={{ fontWeight: 600 }}>{role.name}</Typography>
                <Chip size="small" label={`${role.permissions.length} permissions`} />
              </Stack>
            </AccordionSummary>
            <AccordionDetails>
              {role.description && (
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  {role.description}
                </Typography>
              )}
              <Stack>
                {(permissions ?? []).map((permission) => (
                  <FormControlLabel
                    key={permission.id}
                    disabled={!canEditTemplates || assignMutation.isPending || revokeMutation.isPending}
                    control={
                      <Checkbox
                        checked={assignedIds.has(permission.id)}
                        onChange={(e) => {
                          const args = { roleId: role.id, permissionId: permission.id };
                          if (e.target.checked) assignMutation.mutate(args);
                          else revokeMutation.mutate(args);
                        }}
                      />
                    }
                    label={`${permission.key} — ${permission.description}`}
                  />
                ))}
              </Stack>
            </AccordionDetails>
          </Accordion>
        );
      })}
    </Stack>
  );
}
