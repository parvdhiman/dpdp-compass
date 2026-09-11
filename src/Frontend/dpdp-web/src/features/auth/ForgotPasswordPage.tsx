import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { useNavigate, useSearchParams } from "react-router-dom";
import { ApiError } from "../../lib/apiClient";
import * as authApi from "./api";

const requestSchema = z.object({
  email: z.string().min(1, "Email is required").email("Enter a valid email address"),
});
type RequestFormValues = z.infer<typeof requestSchema>;

const resetSchema = z
  .object({
    newPassword: z.string().min(12, "Password must be at least 12 characters long"),
    confirmPassword: z.string().min(1, "Please confirm your new password"),
  })
  .refine((data) => data.newPassword === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });
type ResetFormValues = z.infer<typeof resetSchema>;

function RequestResetForm() {
  const [devOnlyResetToken, setDevOnlyResetToken] = useState<string | null>(null);
  const [submitted, setSubmitted] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<RequestFormValues>({ resolver: zodResolver(requestSchema) });

  const onSubmit = async (values: RequestFormValues) => {
    const result = await authApi.forgotPassword(values.email);
    setDevOnlyResetToken(result.devOnlyResetToken);
    setSubmitted(true);
  };

  if (submitted) {
    return (
      <Stack spacing={2}>
        <Alert severity="success">
          If an account exists for that email, a password reset link has been issued.
        </Alert>
        {devOnlyResetToken && (
          <Alert severity="info">
            Development only — no email delivery module exists yet. Reset token:
            <br />
            <code style={{ wordBreak: "break-all" }}>{devOnlyResetToken}</code>
          </Alert>
        )}
      </Stack>
    );
  }

  return (
    <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Stack spacing={2}>
        <TextField
          label="Email"
          type="email"
          fullWidth
          error={!!errors.email}
          helperText={errors.email?.message}
          {...register("email")}
        />
        <Button type="submit" variant="contained" fullWidth disabled={isSubmitting}>
          {isSubmitting ? "Sending…" : "Send reset link"}
        </Button>
      </Stack>
    </Box>
  );
}

function ResetPasswordForm({ token }: { token: string }) {
  const navigate = useNavigate();
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ResetFormValues>({ resolver: zodResolver(resetSchema) });

  const onSubmit = async (values: ResetFormValues) => {
    setServerError(null);
    try {
      await authApi.resetPassword(token, values.newPassword);
      navigate("/login", { replace: true });
    } catch (error) {
      setServerError(error instanceof ApiError ? error.message : "Could not reset password.");
    }
  };

  return (
    <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
      <Stack spacing={2}>
        {serverError && <Alert severity="error">{serverError}</Alert>}
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
        <Button type="submit" variant="contained" fullWidth disabled={isSubmitting}>
          {isSubmitting ? "Resetting…" : "Reset password"}
        </Button>
      </Stack>
    </Box>
  );
}

export function ForgotPasswordPage() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get("token");

  return (
    <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", minHeight: "80vh" }}>
      <Card variant="outlined" sx={{ width: 420 }}>
        <CardContent>
          <Typography variant="h5" component="h1" gutterBottom>
            {token ? "Set a new password" : "Forgot password"}
          </Typography>
          <Box sx={{ mt: 2 }}>{token ? <ResetPasswordForm token={token} /> : <RequestResetForm />}</Box>
        </CardContent>
      </Card>
    </Box>
  );
}
