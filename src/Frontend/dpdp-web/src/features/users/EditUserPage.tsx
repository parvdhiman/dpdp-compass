import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import MenuItem from "@mui/material/MenuItem";
import Paper from "@mui/material/Paper";
import Select from "@mui/material/Select";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { useNavigate, useParams } from "react-router-dom";
import { ApiError } from "../../lib/apiClient";
import { useAuth } from "../auth/AuthProvider";
import * as rolesApi from "../roles/api";
import * as usersApi from "./api";

const updateUserSchema = z.object({
  fullName: z.string().min(1, "Full name is required").max(200),
  phoneNumber: z.string().optional(),
});

type UpdateUserFormValues = z.infer<typeof updateUserSchema>;

export function EditUserPage() {
  const { id } = useParams<{ id: string }>();
  const userId = id!;
  const { hasPermission } = useAuth();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [serverError, setServerError] = useState<string | null>(null);
  const [selectedRoleId, setSelectedRoleId] = useState("");

  const { data: targetUser, isPending } = useQuery({
    queryKey: ["users", userId],
    queryFn: () => usersApi.getUser(userId),
  });

  const { data: roles } = useQuery({ queryKey: ["roles"], queryFn: rolesApi.getRoles });

  const {
    register,
    reset,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<UpdateUserFormValues>({
    resolver: zodResolver(updateUserSchema),
    values: targetUser ? { fullName: targetUser.fullName, phoneNumber: targetUser.phoneNumber ?? "" } : undefined,
  });

  const updateMutation = useMutation({ mutationFn: (v: UpdateUserFormValues) => usersApi.updateUser(userId, v) });
  const assignRoleMutation = useMutation({
    mutationFn: (roleId: string) => usersApi.assignUserRole(userId, roleId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["users", userId] });
      setSelectedRoleId("");
    },
  });
  const revokeRoleMutation = useMutation({
    mutationFn: (roleId: string) => usersApi.revokeUserRole(userId, roleId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["users", userId] }),
  });

  const onSubmit = async (values: UpdateUserFormValues) => {
    setServerError(null);
    try {
      await updateMutation.mutateAsync(values);
      navigate("/users");
    } catch (error) {
      setServerError(error instanceof ApiError ? error.message : "Could not update user.");
      reset(values);
    }
  };

  if (isPending) return <CircularProgress />;
  if (!targetUser) return <Alert severity="error">User not found.</Alert>;

  const assignedRoleIds = new Set(targetUser.roles.map((r) => r.roleId));
  const availableRoles = (roles ?? []).filter(
    (r) => r.name !== "Super Administrator" && !assignedRoleIds.has(r.id),
  );

  return (
    <Stack spacing={3} sx={{ maxWidth: 520 }}>
      <Typography variant="h4" component="h1">
        Edit User
      </Typography>

      <Paper variant="outlined" sx={{ p: 3 }}>
        <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <Stack spacing={2}>
            {serverError && <Alert severity="error">{serverError}</Alert>}

            <TextField label="Email" value={targetUser.email} disabled fullWidth />

            <TextField
              label="Full name"
              fullWidth
              error={!!errors.fullName}
              helperText={errors.fullName?.message}
              {...register("fullName")}
            />

            <TextField label="Phone number (optional)" fullWidth {...register("phoneNumber")} />

            <Stack direction="row" spacing={2}>
              <Button type="submit" variant="contained" disabled={isSubmitting}>
                {isSubmitting ? "Saving…" : "Save Changes"}
              </Button>
              <Button onClick={() => navigate("/users")}>Cancel</Button>
            </Stack>
          </Stack>
        </Box>
      </Paper>

      {hasPermission("roles.manage") && (
        <Paper variant="outlined" sx={{ p: 3 }}>
          <Typography variant="h6" gutterBottom>
            Roles
          </Typography>
          <Stack direction="row" spacing={1} sx={{ flexWrap: "wrap", mb: 2 }}>
            {targetUser.roles.map((role) => (
              <Chip
                key={role.roleId}
                label={role.roleName}
                onDelete={
                  role.roleName === "Super Administrator"
                    ? undefined
                    : () => revokeRoleMutation.mutate(role.roleId)
                }
              />
            ))}
          </Stack>

          <Stack direction="row" spacing={2}>
            <Select
              size="small"
              displayEmpty
              value={selectedRoleId}
              onChange={(e) => setSelectedRoleId(e.target.value)}
              sx={{ minWidth: 220 }}
            >
              <MenuItem value="" disabled>
                Add a role…
              </MenuItem>
              {availableRoles.map((role) => (
                <MenuItem key={role.id} value={role.id}>
                  {role.name}
                </MenuItem>
              ))}
            </Select>
            <Button
              variant="outlined"
              disabled={!selectedRoleId || assignRoleMutation.isPending}
              onClick={() => assignRoleMutation.mutate(selectedRoleId)}
            >
              Assign
            </Button>
          </Stack>
        </Paper>
      )}
    </Stack>
  );
}
