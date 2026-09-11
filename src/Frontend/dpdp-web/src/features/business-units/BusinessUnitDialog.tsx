import { useState } from "react";
import { zodResolver } from "@hookform/resolvers/zod";
import { Controller, useForm } from "react-hook-form";
import { z } from "zod";
import { useMutation } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Checkbox from "@mui/material/Checkbox";
import Dialog from "@mui/material/Dialog";
import DialogActions from "@mui/material/DialogActions";
import DialogContent from "@mui/material/DialogContent";
import DialogTitle from "@mui/material/DialogTitle";
import FormControlLabel from "@mui/material/FormControlLabel";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import * as businessUnitsApi from "./api";
import type { BusinessUnit } from "./types";

const schema = z.object({
  name: z.string().min(1, "Name is required").max(200),
  description: z.string().optional(),
  headName: z.string().optional(),
  headEmail: z.string().email("Enter a valid email").optional().or(z.literal("")),
  headPhone: z.string().optional(),
  isActive: z.boolean(),
});

type FormValues = z.infer<typeof schema>;

export function BusinessUnitDialog({
  organisationId,
  businessUnit,
  onClose,
  onSaved,
}: {
  organisationId: string;
  businessUnit: BusinessUnit | null;
  onClose: () => void;
  onSaved: () => void;
}) {
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: businessUnit
      ? {
          name: businessUnit.name,
          description: businessUnit.description ?? "",
          headName: businessUnit.head.name ?? "",
          headEmail: businessUnit.head.email ?? "",
          headPhone: businessUnit.head.phone ?? "",
          isActive: businessUnit.isActive,
        }
      : { name: "", isActive: true },
  });

  const saveMutation = useMutation({
    mutationFn: (values: FormValues) =>
      businessUnit
        ? businessUnitsApi.updateBusinessUnit(businessUnit.id, values)
        : businessUnitsApi.createBusinessUnit(organisationId, values),
  });

  const onSubmit = async (values: FormValues) => {
    setServerError(null);
    try {
      await saveMutation.mutateAsync(values);
      onSaved();
    } catch {
      setServerError("Could not save this business unit.");
    }
  };

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{businessUnit ? "Edit Business Unit" : "Create Business Unit"}</DialogTitle>
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            {serverError && <Alert severity="error">{serverError}</Alert>}
            <TextField label="Name" fullWidth error={!!errors.name} helperText={errors.name?.message} {...register("name")} />
            <TextField label="Description" fullWidth multiline rows={2} {...register("description")} />
            <TextField label="Head — name" fullWidth {...register("headName")} />
            <TextField label="Head — email" fullWidth error={!!errors.headEmail} helperText={errors.headEmail?.message} {...register("headEmail")} />
            <TextField label="Head — phone" fullWidth {...register("headPhone")} />
            {businessUnit && (
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
