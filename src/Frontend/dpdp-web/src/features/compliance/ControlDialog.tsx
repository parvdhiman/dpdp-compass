import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { useMutation } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import * as complianceApi from "./api";
import { RISK_LEVELS, REVIEW_STATUSES, type ControlDetail } from "./types";

const schema = z.object({
  controlId: z.string().min(1, "Control ID is required").max(50),
  name: z.string().min(1, "Name is required").max(300),
  description: z.string().min(1, "Description is required"),
  objective: z.string().min(1, "Objective is required"),
  controlCategoryId: z.string().min(1, "Category is required"),
  riskLevel: z.string().min(1),
  applicableConditions: z.string().optional(),
  evidenceRequirementsSummary: z.string().optional(),
  guidance: z.string().optional(),
  sourceReference: z.string().min(1, "Source reference is required"),
  reviewStatus: z.string().min(1),
});

type FormValues = z.infer<typeof schema>;

export function ControlDialog({
  control,
  onClose,
  onSaved,
}: {
  control: ControlDetail | null;
  onClose: () => void;
  onSaved: () => void;
}) {
  const [serverError, setServerError] = useState<string | null>(null);

  const { data: categories } = useQuery({
    queryKey: ["compliance", "control-categories"],
    queryFn: complianceApi.getControlCategories,
  });

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: control
      ? {
          controlId: control.controlId,
          name: control.name,
          description: control.description,
          objective: control.objective,
          controlCategoryId: control.controlCategoryId,
          riskLevel: control.riskLevel,
          applicableConditions: control.applicableConditions ?? "",
          evidenceRequirementsSummary: control.evidenceRequirementsSummary ?? "",
          guidance: control.guidance ?? "",
          sourceReference: control.sourceReference,
          reviewStatus: control.reviewStatus,
        }
      : {
          controlId: "",
          name: "",
          description: "",
          objective: "",
          controlCategoryId: "",
          riskLevel: "LOW",
          sourceReference: "",
          reviewStatus: "DRAFT",
        },
  });

  const saveMutation = useMutation({
    mutationFn: (values: FormValues) =>
      control
        ? complianceApi.updateControl(control.id, values)
        : complianceApi.createControl(values),
  });

  const onSubmit = async (values: FormValues) => {
    setServerError(null);
    try {
      await saveMutation.mutateAsync(values);
      onSaved();
    } catch {
      setServerError("Could not save this control.");
    }
  };

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{control ? "Edit Control" : "Create Control"}</DialogTitle>
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            {serverError && <Alert severity="error">{serverError}</Alert>}
            <TextField
              label="Control ID"
              fullWidth
              disabled={!!control}
              error={!!errors.controlId}
              helperText={errors.controlId?.message}
              {...register("controlId")}
            />
            <TextField label="Name" fullWidth error={!!errors.name} helperText={errors.name?.message} {...register("name")} />
            <TextField
              label="Description"
              fullWidth
              multiline
              rows={3}
              error={!!errors.description}
              helperText={errors.description?.message}
              {...register("description")}
            />
            <TextField
              label="Objective"
              fullWidth
              multiline
              rows={2}
              error={!!errors.objective}
              helperText={errors.objective?.message}
              {...register("objective")}
            />
            <TextField
              select
              label="Category"
              fullWidth
              error={!!errors.controlCategoryId}
              helperText={errors.controlCategoryId?.message}
              defaultValue={control?.controlCategoryId ?? ""}
              {...register("controlCategoryId")}
            >
              {categories?.map((category) => (
                <MenuItem key={category.id} value={category.id}>
                  {category.name}
                </MenuItem>
              ))}
            </TextField>
            <TextField select label="Risk level" fullWidth defaultValue={control?.riskLevel ?? "LOW"} {...register("riskLevel")}>
              {RISK_LEVELS.map((level) => (
                <MenuItem key={level} value={level}>
                  {level}
                </MenuItem>
              ))}
            </TextField>
            <TextField label="Applicable conditions" fullWidth {...register("applicableConditions")} />
            <TextField label="Evidence requirements summary" fullWidth {...register("evidenceRequirementsSummary")} />
            <TextField label="Guidance" fullWidth multiline rows={2} {...register("guidance")} />
            <TextField
              label="Source reference"
              fullWidth
              error={!!errors.sourceReference}
              helperText={errors.sourceReference?.message}
              {...register("sourceReference")}
            />
            {control && (
              <TextField select label="Legal review status" fullWidth defaultValue={control.reviewStatus} {...register("reviewStatus")}>
                {REVIEW_STATUSES.map((status) => (
                  <MenuItem key={status} value={status}>
                    {status}
                  </MenuItem>
                ))}
              </TextField>
            )}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Cancel</Button>
          <Button type="submit" variant="contained" disabled={isSubmitting}>
            {isSubmitting ? "Saving…" : "Save"}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
