import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";
import { z } from "zod";
import { useMutation, useQuery } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Checkbox from "@mui/material/Checkbox";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import FormControlLabel from "@mui/material/FormControlLabel";
import MenuItem from "@mui/material/MenuItem";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import * as businessUnitsApi from "../business-units/api";
import * as departmentsApi from "./api";
import type { Department } from "./types";

const schema = z.object({
  businessUnitId: z.string().min(1, "Business unit is required"),
  name: z.string().min(1, "Name is required").max(200),
  description: z.string().optional(),
  headName: z.string().optional(),
  headEmail: z.string().email("Enter a valid email").optional().or(z.literal("")),
  headPhone: z.string().optional(),
  isActive: z.boolean(),
});

type FormValues = z.infer<typeof schema>;

export function DepartmentDialog({
  department,
  defaultBusinessUnitId,
  onClose,
  onSaved,
}: {
  department: Department | null;
  defaultBusinessUnitId?: string;
  onClose: () => void;
  onSaved: () => void;
}) {
  const [serverError, setServerError] = useState<string | null>(null);

  const { data: businessUnits } = useQuery({
    queryKey: ["business-units", "all-for-select"],
    queryFn: () => businessUnitsApi.getBusinessUnits(1, 100),
  });

  const {
    register,
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: department
      ? {
          businessUnitId: department.businessUnitId,
          name: department.name,
          description: department.description ?? "",
          headName: department.head.name ?? "",
          headEmail: department.head.email ?? "",
          headPhone: department.head.phone ?? "",
          isActive: department.isActive,
        }
      : { businessUnitId: defaultBusinessUnitId ?? "", name: "", isActive: true },
  });

  const saveMutation = useMutation({
    mutationFn: (values: FormValues) =>
      department
        ? departmentsApi.updateDepartment(department.id, values)
        : departmentsApi.createDepartment(values.businessUnitId, values),
  });

  const onSubmit = async (values: FormValues) => {
    setServerError(null);
    try {
      await saveMutation.mutateAsync(values);
      onSaved();
    } catch {
      setServerError("Could not save this department.");
    }
  };

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{department ? "Edit Department" : "Create Department"}</DialogTitle>
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            {serverError && <Alert severity="error">{serverError}</Alert>}
            <TextField
              label="Business Unit"
              select
              fullWidth
              disabled={!!department}
              error={!!errors.businessUnitId}
              helperText={errors.businessUnitId?.message}
              {...register("businessUnitId")}
            >
              {(businessUnits?.items ?? []).map((bu) => (
                <MenuItem key={bu.id} value={bu.id}>
                  {bu.name}
                </MenuItem>
              ))}
            </TextField>
            <TextField label="Name" fullWidth error={!!errors.name} helperText={errors.name?.message} {...register("name")} />
            <TextField label="Description" fullWidth multiline rows={2} {...register("description")} />
            <TextField label="Head — name" fullWidth {...register("headName")} />
            <TextField label="Head — email" fullWidth error={!!errors.headEmail} helperText={errors.headEmail?.message} {...register("headEmail")} />
            <TextField label="Head — phone" fullWidth {...register("headPhone")} />
            {department && (
              <Controller
                name="isActive"
                control={control}
                render={({ field }) => (
                  <FormControlLabel
                    control={<Checkbox checked={field.value} onChange={(e) => field.onChange(e.target.checked)} />}
                    label="Active"
                  />
                )}
              />
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
