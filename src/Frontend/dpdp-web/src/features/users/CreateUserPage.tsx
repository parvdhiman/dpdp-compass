import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";
import { z } from "zod";
import { useMutation, useQuery } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Checkbox from "@mui/material/Checkbox";
import CircularProgress from "@mui/material/CircularProgress";
import FormControlLabel from "@mui/material/FormControlLabel";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { useNavigate } from "react-router-dom";
import { ApiError } from "../../lib/apiClient";
import { useAuth } from "../auth/AuthProvider";
import * as rolesApi from "../roles/api";
import * as usersApi from "./api";

const createUserSchema = z.object({
  organisationId: z.string().min(1, "Organisation is required").uuid("Must be a valid organisation id"),
  email: z.string().min(1, "Email is required").email("Enter a valid email address"),
  fullName: z.string().min(1, "Full name is required").max(200),
  phoneNumber: z.string().optional(),
  password: z.string().min(12, "Password must be at least 12 characters long"),
  roleIds: z.array(z.string()),
});

type CreateUserFormValues = z.infer<typeof createUserSchema>;

export function CreateUserPage() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const [serverError, setServerError] = useState<string | null>(null);

  const { data: roles, isPending: rolesLoading } = useQuery({
    queryKey: ["roles"],
    queryFn: rolesApi.getRoles,
  });

  const {
    register,
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CreateUserFormValues>({
    resolver: zodResolver(createUserSchema),
    defaultValues: { organisationId: user?.organisationId ?? "", roleIds: [] },
  });

  const createMutation = useMutation({ mutationFn: usersApi.createUser });

  const onSubmit = async (values: CreateUserFormValues) => {
    setServerError(null);
    try {
      await createMutation.mutateAsync(values);
      navigate("/users");
    } catch (error) {
      setServerError(error instanceof ApiError ? error.message : "Could not create user.");
    }
  };

  const assignableRoles = (roles ?? []).filter((r) => r.name !== "Super Administrator");

  return (
    <Stack spacing={3} sx={{ maxWidth: 520 }}>
      <Typography variant="h4" component="h1">
        Create User
      </Typography>

      <Paper variant="outlined" sx={{ p: 3 }}>
        <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <Stack spacing={2}>
            {serverError && <Alert severity="error">{serverError}</Alert>}

            {user?.isSuperAdministrator && (
              <TextField
                label="Organisation ID"
                fullWidth
                helperText={
                  errors.organisationId?.message ??
                  "As Super Administrator you must specify the target organisation's id — there is no organisation picker yet (see docs/MODULE_ROADMAP.md)."
                }
                error={!!errors.organisationId}
                {...register("organisationId")}
              />
            )}

            <TextField
              label="Full name"
              fullWidth
              error={!!errors.fullName}
              helperText={errors.fullName?.message}
              {...register("fullName")}
            />

            <TextField
              label="Email"
              type="email"
              fullWidth
              error={!!errors.email}
              helperText={errors.email?.message}
              {...register("email")}
            />

            <TextField label="Phone number (optional)" fullWidth {...register("phoneNumber")} />

            <TextField
              label="Temporary password"
              type="password"
              fullWidth
              error={!!errors.password}
              helperText={errors.password?.message ?? "The user will be required to change this on first login."}
              {...register("password")}
            />

            <Typography variant="subtitle2">Roles</Typography>
            {rolesLoading && <CircularProgress size={20} />}
            <Controller
              name="roleIds"
              control={control}
              render={({ field }) => (
                <Stack>
                  {assignableRoles.map((role) => (
                    <FormControlLabel
                      key={role.id}
                      control={
                        <Checkbox
                          checked={field.value.includes(role.id)}
                          onChange={(e) => {
                            field.onChange(
                              e.target.checked
                                ? [...field.value, role.id]
                                : field.value.filter((id) => id !== role.id),
                            );
                          }}
                        />
                      }
                      label={role.name}
                    />
                  ))}
                </Stack>
              )}
            />

            <Stack direction="row" spacing={2}>
              <Button type="submit" variant="contained" disabled={isSubmitting}>
                {isSubmitting ? "Creating…" : "Create User"}
              </Button>
              <Button onClick={() => navigate("/users")}>Cancel</Button>
            </Stack>
          </Stack>
        </Box>
      </Paper>
    </Stack>
  );
}
