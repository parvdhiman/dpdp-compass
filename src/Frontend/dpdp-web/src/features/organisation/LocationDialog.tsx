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
import * as organisationApi from "./api";
import type { OrganisationLocation } from "./types";

const locationSchema = z.object({
  label: z.string().min(1, "Label is required").max(200),
  addressLine1: z.string().optional(),
  addressLine2: z.string().optional(),
  city: z.string().optional(),
  state: z.string().optional(),
  postalCode: z.string().optional(),
  country: z.string().optional(),
  isPrimary: z.boolean(),
});

type LocationFormValues = z.infer<typeof locationSchema>;

export function LocationDialog({
  organisationId,
  location,
  onClose,
  onSaved,
}: {
  organisationId: string;
  location: OrganisationLocation | null;
  onClose: () => void;
  onSaved: () => void;
}) {
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LocationFormValues>({
    resolver: zodResolver(locationSchema),
    defaultValues: location
      ? {
          label: location.label,
          addressLine1: location.addressLine1 ?? "",
          addressLine2: location.addressLine2 ?? "",
          city: location.city ?? "",
          state: location.state ?? "",
          postalCode: location.postalCode ?? "",
          country: location.country ?? "",
          isPrimary: location.isPrimary,
        }
      : { label: "", isPrimary: false },
  });

  const saveMutation = useMutation({
    mutationFn: (values: LocationFormValues) =>
      location
        ? organisationApi.updateLocation(location.id, values)
        : organisationApi.createLocation(organisationId, values),
  });

  const onSubmit = async (values: LocationFormValues) => {
    setServerError(null);
    try {
      await saveMutation.mutateAsync(values);
      onSaved();
    } catch {
      setServerError("Could not save this location.");
    }
  };

  return (
    <Dialog open onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{location ? "Edit Location" : "Add Location"}</DialogTitle>
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            {serverError && <Alert severity="error">{serverError}</Alert>}
            <TextField label="Label" fullWidth error={!!errors.label} helperText={errors.label?.message} {...register("label")} />
            <TextField label="Address line 1" fullWidth {...register("addressLine1")} />
            <TextField label="Address line 2" fullWidth {...register("addressLine2")} />
            <Stack direction="row" spacing={2}>
              <TextField label="City" fullWidth {...register("city")} />
              <TextField label="State" fullWidth {...register("state")} />
            </Stack>
            <Stack direction="row" spacing={2}>
              <TextField label="Postal code" fullWidth {...register("postalCode")} />
              <TextField label="Country" fullWidth {...register("country")} />
            </Stack>
            <Controller
              name="isPrimary"
              control={control}
              render={({ field }) => (
                <FormControlLabel
                  control={<Checkbox checked={field.value} onChange={(e) => field.onChange(e.target.checked)} />}
                  label="Primary location"
                />
              )}
            />
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
