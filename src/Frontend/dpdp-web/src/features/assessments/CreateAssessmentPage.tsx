import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { z } from "zod";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import MenuItem from "@mui/material/MenuItem";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import * as complianceApi from "../compliance/api";
import * as assessmentsApi from "./api";

const schema = z.object({
  frameworkVersionId: z.string().min(1, "Select a framework version"),
  name: z.string().min(1, "Name is required").max(300),
  description: z.string().optional(),
  dueDate: z.string().optional(),
});

type FormValues = z.infer<typeof schema>;

export function CreateAssessmentPage() {
  const navigate = useNavigate();
  const [serverError, setServerError] = useState<string | null>(null);

  const { data: frameworks } = useQuery({ queryKey: ["compliance", "frameworks"], queryFn: complianceApi.getFrameworks });

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({ resolver: zodResolver(schema), defaultValues: { name: "", frameworkVersionId: "" } });

  const createMutation = useMutation({
    mutationFn: (values: FormValues) =>
      assessmentsApi.createAssessment({
        frameworkVersionId: values.frameworkVersionId,
        name: values.name,
        description: values.description || null,
        dueDate: values.dueDate || null,
      }),
  });

  const onSubmit = async (values: FormValues) => {
    setServerError(null);
    try {
      const created = await createMutation.mutateAsync(values);
      navigate(`/assessments/${created.id}`);
    } catch {
      setServerError("Could not create this assessment.");
    }
  };

  const versionOptions = frameworks?.flatMap((f) => f.versions.map((v) => ({ id: v.id, label: `${f.name} — ${v.versionLabel}` }))) ?? [];

  return (
    <Stack spacing={3} sx={{ maxWidth: 600 }}>
      <Typography variant="h4" component="h1">
        New Assessment
      </Typography>

      <Paper variant="outlined" sx={{ p: 3 }}>
        <form onSubmit={handleSubmit(onSubmit)} noValidate>
          <Stack spacing={2}>
            {serverError && <Alert severity="error">{serverError}</Alert>}
            <TextField
              select
              label="Framework Version"
              fullWidth
              error={!!errors.frameworkVersionId}
              helperText={errors.frameworkVersionId?.message}
              defaultValue=""
              {...register("frameworkVersionId")}
            >
              {versionOptions.map((v) => (
                <MenuItem key={v.id} value={v.id}>
                  {v.label}
                </MenuItem>
              ))}
            </TextField>
            <TextField label="Name" fullWidth error={!!errors.name} helperText={errors.name?.message} {...register("name")} />
            <TextField label="Description" fullWidth multiline rows={3} {...register("description")} />
            <TextField label="Due date" type="date" fullWidth slotProps={{ inputLabel: { shrink: true } }} {...register("dueDate")} />
            <Stack direction="row" spacing={1} sx={{ justifyContent: "flex-end" }}>
              <Button onClick={() => navigate("/assessments")}>Cancel</Button>
              <Button type="submit" variant="contained" disabled={isSubmitting}>
                {isSubmitting ? "Creating…" : "Create Assessment"}
              </Button>
            </Stack>
          </Stack>
        </form>
      </Paper>
    </Stack>
  );
}
