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
import Divider from "@mui/material/Divider";
import MenuItem from "@mui/material/MenuItem";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import { useAuth } from "../auth/AuthProvider";
import * as organisationApi from "./api";
import { LocationDialog } from "./LocationDialog";
import { ORGANISATION_SIZES, type OrganisationLocation } from "./types";

const profileSchema = z.object({
  name: z.string().min(1, "Name is required").max(200),
  legalName: z.string().optional(),
  industry: z.string().optional(),
  size: z.string().optional(),
  country: z.string().optional(),
  website: z.string().optional(),
  primaryContactName: z.string().optional(),
  primaryContactEmail: z.string().email("Enter a valid email").optional().or(z.literal("")),
  primaryContactPhone: z.string().optional(),
  privacyContactName: z.string().optional(),
  privacyContactEmail: z.string().email("Enter a valid email").optional().or(z.literal("")),
  privacyContactPhone: z.string().optional(),
  dpoName: z.string().optional(),
  dpoEmail: z.string().email("Enter a valid email").optional().or(z.literal("")),
  dpoPhone: z.string().optional(),
});

type ProfileFormValues = z.infer<typeof profileSchema>;

export function OrganisationProfilePage() {
  const { user, hasPermission } = useAuth();
  const organisationId = user?.organisationId;
  const queryClient = useQueryClient();
  const [serverError, setServerError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
  const [locationDialogOpen, setLocationDialogOpen] = useState(false);
  const [editingLocation, setEditingLocation] = useState<OrganisationLocation | null>(null);

  const { data: profile, isPending } = useQuery({
    queryKey: ["organisation-profile", organisationId],
    queryFn: () => organisationApi.getOrganisationProfile(organisationId!),
    enabled: !!organisationId,
  });

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ProfileFormValues>({
    resolver: zodResolver(profileSchema),
    values: profile
      ? {
          name: profile.name,
          legalName: profile.legalName ?? "",
          industry: profile.industry ?? "",
          size: profile.size ?? "",
          country: profile.country ?? "",
          website: profile.website ?? "",
          primaryContactName: profile.primaryContact.name ?? "",
          primaryContactEmail: profile.primaryContact.email ?? "",
          primaryContactPhone: profile.primaryContact.phone ?? "",
          privacyContactName: profile.privacyContact.name ?? "",
          privacyContactEmail: profile.privacyContact.email ?? "",
          privacyContactPhone: profile.privacyContact.phone ?? "",
          dpoName: profile.dpoContact.name ?? "",
          dpoEmail: profile.dpoContact.email ?? "",
          dpoPhone: profile.dpoContact.phone ?? "",
        }
      : undefined,
  });

  const updateMutation = useMutation({
    mutationFn: (values: ProfileFormValues) => organisationApi.updateOrganisationProfile(organisationId!, values),
    onSuccess: () => {
      setSuccess(true);
      queryClient.invalidateQueries({ queryKey: ["organisation-profile", organisationId] });
    },
  });

  const deleteLocationMutation = useMutation({
    mutationFn: organisationApi.deleteLocation,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["organisation-profile", organisationId] }),
  });

  const onSubmit = async (values: ProfileFormValues) => {
    setServerError(null);
    setSuccess(false);
    try {
      await updateMutation.mutateAsync(values);
    } catch {
      setServerError("Could not save the organisation profile.");
    }
  };

  const canEdit = hasPermission("organisation.write");

  if (!organisationId) {
    return <Alert severity="info">Super Administrator accounts aren't attached to an organisation.</Alert>;
  }
  if (isPending) return <CircularProgress />;

  return (
    <Stack spacing={3} sx={{ maxWidth: 720 }}>
      <Typography variant="h4" component="h1">
        Organisation Profile
      </Typography>

      <Paper variant="outlined" sx={{ p: 3 }}>
        <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <Stack spacing={2}>
            {serverError && <Alert severity="error">{serverError}</Alert>}
            {success && <Alert severity="success">Profile saved.</Alert>}

            <TextField label="Name" fullWidth disabled={!canEdit} error={!!errors.name} helperText={errors.name?.message} {...register("name")} />
            <TextField label="Legal name" fullWidth disabled={!canEdit} {...register("legalName")} />
            <Stack direction="row" spacing={2}>
              <TextField label="Industry" fullWidth disabled={!canEdit} {...register("industry")} />
              <TextField label="Size" select fullWidth disabled={!canEdit} defaultValue="" {...register("size")}>
                <MenuItem value="">Not set</MenuItem>
                {ORGANISATION_SIZES.map((size) => (
                  <MenuItem key={size} value={size}>
                    {size}
                  </MenuItem>
                ))}
              </TextField>
            </Stack>
            <Stack direction="row" spacing={2}>
              <TextField label="Country" fullWidth disabled={!canEdit} {...register("country")} />
              <TextField label="Website" fullWidth disabled={!canEdit} {...register("website")} />
            </Stack>

            <Divider textAlign="left">Primary Contact</Divider>
            <Stack direction="row" spacing={2}>
              <TextField label="Name" fullWidth disabled={!canEdit} {...register("primaryContactName")} />
              <TextField label="Email" fullWidth disabled={!canEdit} error={!!errors.primaryContactEmail} helperText={errors.primaryContactEmail?.message} {...register("primaryContactEmail")} />
              <TextField label="Phone" fullWidth disabled={!canEdit} {...register("primaryContactPhone")} />
            </Stack>

            <Divider textAlign="left">Privacy Contact</Divider>
            <Stack direction="row" spacing={2}>
              <TextField label="Name" fullWidth disabled={!canEdit} {...register("privacyContactName")} />
              <TextField label="Email" fullWidth disabled={!canEdit} error={!!errors.privacyContactEmail} helperText={errors.privacyContactEmail?.message} {...register("privacyContactEmail")} />
              <TextField label="Phone" fullWidth disabled={!canEdit} {...register("privacyContactPhone")} />
            </Stack>

            <Divider textAlign="left">Data Protection Officer</Divider>
            <Stack direction="row" spacing={2}>
              <TextField label="Name" fullWidth disabled={!canEdit} {...register("dpoName")} />
              <TextField label="Email" fullWidth disabled={!canEdit} error={!!errors.dpoEmail} helperText={errors.dpoEmail?.message} {...register("dpoEmail")} />
              <TextField label="Phone" fullWidth disabled={!canEdit} {...register("dpoPhone")} />
            </Stack>

            {canEdit && (
              <Button type="submit" variant="contained" disabled={isSubmitting} sx={{ alignSelf: "flex-start" }}>
                {isSubmitting ? "Saving…" : "Save Changes"}
              </Button>
            )}
          </Stack>
        </Box>
      </Paper>

      <Paper variant="outlined" sx={{ p: 3 }}>
        <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "center" }}>
          <Typography variant="h6">Locations</Typography>
          {canEdit && (
            <Button
              size="small"
              onClick={() => {
                setEditingLocation(null);
                setLocationDialogOpen(true);
              }}
            >
              Add Location
            </Button>
          )}
        </Stack>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Label</TableCell>
              <TableCell>City</TableCell>
              <TableCell>Country</TableCell>
              <TableCell>Primary</TableCell>
              {canEdit && <TableCell align="right">Actions</TableCell>}
            </TableRow>
          </TableHead>
          <TableBody>
            {profile?.locations.map((location) => (
              <TableRow key={location.id}>
                <TableCell>{location.label}</TableCell>
                <TableCell>{location.city}</TableCell>
                <TableCell>{location.country}</TableCell>
                <TableCell>{location.isPrimary && <Chip label="Primary" size="small" color="primary" />}</TableCell>
                {canEdit && (
                  <TableCell align="right">
                    <Button
                      size="small"
                      onClick={() => {
                        setEditingLocation(location);
                        setLocationDialogOpen(true);
                      }}
                    >
                      Edit
                    </Button>
                    <Button size="small" color="error" onClick={() => deleteLocationMutation.mutate(location.id)}>
                      Remove
                    </Button>
                  </TableCell>
                )}
              </TableRow>
            ))}
            {profile?.locations.length === 0 && (
              <TableRow>
                <TableCell colSpan={5} align="center">
                  No locations yet.
                </TableCell>
              </TableRow>
            )}
          </TableBody>
        </Table>
      </Paper>

      {locationDialogOpen && (
        <LocationDialog
          organisationId={organisationId}
          location={editingLocation}
          onClose={() => setLocationDialogOpen(false)}
          onSaved={() => {
            setLocationDialogOpen(false);
            queryClient.invalidateQueries({ queryKey: ["organisation-profile", organisationId] });
          }}
        />
      )}
    </Stack>
  );
}
