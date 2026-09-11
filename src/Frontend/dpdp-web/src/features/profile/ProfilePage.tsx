import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import Divider from "@mui/material/Divider";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { ApiError } from "../../lib/apiClient";
import * as authApi from "../auth/api";
import { useAuth } from "../auth/AuthProvider";

const changePasswordSchema = z
  .object({
    currentPassword: z.string().min(1, "Current password is required"),
    newPassword: z.string().min(12, "Password must be at least 12 characters long"),
    confirmPassword: z.string().min(1, "Please confirm your new password"),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

type ChangePasswordFormValues = z.infer<typeof changePasswordSchema>;

export function ProfilePage() {
  const { user } = useAuth();
  const [serverError, setServerError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ChangePasswordFormValues>({ resolver: zodResolver(changePasswordSchema) });

  const onSubmit = async (values: ChangePasswordFormValues) => {
    setServerError(null);
    setSuccess(false);
    try {
      await authApi.changePassword(values.currentPassword, values.newPassword);
      setSuccess(true);
      reset();
    } catch (error) {
      setServerError(error instanceof ApiError ? error.message : "Could not change password.");
    }
  };

  if (!user) return null;

  return (
    <Stack spacing={3} sx={{ maxWidth: 520 }}>
      <Typography variant="h4" component="h1">
        Profile
      </Typography>

      <Paper variant="outlined" sx={{ p: 3 }}>
        <Stack spacing={1}>
          <Typography>
            <strong>Name:</strong> {user.fullName}
          </Typography>
          <Typography>
            <strong>Email:</strong> {user.email}
          </Typography>
          <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
            <Typography component="span">
              <strong>Roles:</strong>
            </Typography>
            {user.roles.map((role) => (
              <Chip key={role} label={role} size="small" />
            ))}
          </Stack>
        </Stack>
      </Paper>

      <Paper variant="outlined" sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Change Password
        </Typography>
        <Divider sx={{ mb: 2 }} />

        <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <Stack spacing={2}>
            {serverError && <Alert severity="error">{serverError}</Alert>}
            {success && <Alert severity="success">Password changed successfully.</Alert>}

            <TextField
              label="Current password"
              type="password"
              fullWidth
              error={!!errors.currentPassword}
              helperText={errors.currentPassword?.message}
              {...register("currentPassword")}
            />
            <TextField
              label="New password"
              type="password"
              fullWidth
              error={!!errors.newPassword}
              helperText={errors.newPassword?.message}
              {...register("newPassword")}
            />
            <TextField
              label="Confirm new password"
              type="password"
              fullWidth
              error={!!errors.confirmPassword}
              helperText={errors.confirmPassword?.message}
              {...register("confirmPassword")}
            />

            <Button type="submit" variant="contained" disabled={isSubmitting} sx={{ alignSelf: "flex-start" }}>
              {isSubmitting ? "Saving…" : "Change Password"}
            </Button>
          </Stack>
        </Box>
      </Paper>
    </Stack>
  );
}
