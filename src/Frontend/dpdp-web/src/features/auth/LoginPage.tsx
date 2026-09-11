import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Card from "@mui/material/Card";
import CardContent from "@mui/material/CardContent";
import Link from "@mui/material/Link";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { Link as RouterLink, useLocation, useNavigate } from "react-router-dom";
import { ApiError } from "../../lib/apiClient";
import { useAuth } from "./AuthProvider";

const loginSchema = z.object({
  email: z.string().min(1, "Email is required").email("Enter a valid email address"),
  password: z.string().min(1, "Password is required"),
});

type LoginFormValues = z.infer<typeof loginSchema>;

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({ resolver: zodResolver(loginSchema) });

  const onSubmit = async (values: LoginFormValues) => {
    setServerError(null);
    try {
      await login(values.email, values.password);
      const redirectTo = (location.state as { from?: Location })?.from?.pathname ?? "/";
      navigate(redirectTo, { replace: true });
    } catch (error) {
      setServerError(error instanceof ApiError ? error.message : "Login failed. Please try again.");
    }
  };

  return (
    <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", minHeight: "80vh" }}>
      <Card variant="outlined" sx={{ width: 400 }}>
        <CardContent>
          <Typography variant="h5" component="h1" gutterBottom>
            Sign in
          </Typography>
          <Typography variant="body2" color="text.secondary" gutterBottom>
            DPDP-COMPASS
          </Typography>

          <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate sx={{ mt: 2 }}>
            <Stack spacing={2}>
              {serverError && <Alert severity="error">{serverError}</Alert>}

              <TextField
                label="Email"
                type="email"
                autoComplete="username"
                fullWidth
                error={!!errors.email}
                helperText={errors.email?.message}
                {...register("email")}
              />

              <TextField
                label="Password"
                type="password"
                autoComplete="current-password"
                fullWidth
                error={!!errors.password}
                helperText={errors.password?.message}
                {...register("password")}
              />

              <Button type="submit" variant="contained" fullWidth disabled={isSubmitting}>
                {isSubmitting ? "Signing in…" : "Sign in"}
              </Button>

              <Link component={RouterLink} to="/forgot-password" variant="body2" sx={{ textAlign: "center" }}>
                Forgot your password?
              </Link>
            </Stack>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
}
