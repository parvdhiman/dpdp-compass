import { useState } from "react";
import { useMutation, useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import Alert from "@mui/material/Alert";
import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Checkbox from "@mui/material/Checkbox";
import FormControlLabel from "@mui/material/FormControlLabel";
import MenuItem from "@mui/material/MenuItem";
import Paper from "@mui/material/Paper";
import Step from "@mui/material/Step";
import StepLabel from "@mui/material/StepLabel";
import Stepper from "@mui/material/Stepper";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import Typography from "@mui/material/Typography";
import * as businessUnitsApi from "../business-units/api";
import * as complianceApi from "../compliance/api";
import * as departmentsApi from "../departments/api";
import * as assessmentsApi from "./api";

const steps = ["Framework Version", "Scope (optional)", "Review & Create"];

export function AssessmentWizardPage() {
  const navigate = useNavigate();
  const [activeStep, setActiveStep] = useState(0);
  const [serverError, setServerError] = useState<string | null>(null);

  const [frameworkVersionId, setFrameworkVersionId] = useState("");
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [dueDate, setDueDate] = useState("");
  const [includeScope, setIncludeScope] = useState(false);
  const [businessUnitId, setBusinessUnitId] = useState("");
  const [departmentId, setDepartmentId] = useState("");
  const [scopeNotes, setScopeNotes] = useState("");

  const { data: frameworks } = useQuery({ queryKey: ["compliance", "frameworks"], queryFn: complianceApi.getFrameworks });
  const { data: businessUnits } = useQuery({
    queryKey: ["business-units-for-wizard"],
    queryFn: () => businessUnitsApi.getBusinessUnits(1, 100),
  });
  const { data: departments } = useQuery({
    queryKey: ["departments-for-wizard", businessUnitId],
    queryFn: () => departmentsApi.getDepartments(1, 100, undefined, businessUnitId || undefined),
    enabled: includeScope,
  });

  const versionOptions = frameworks?.flatMap((f) => f.versions.map((v) => ({ id: v.id, label: `${f.name} — ${v.versionLabel}`, frameworkName: f.name, versionLabel: v.versionLabel }))) ?? [];
  const selectedVersion = versionOptions.find((v) => v.id === frameworkVersionId);

  const createMutation = useMutation({
    mutationFn: () =>
      assessmentsApi.createAssessment({
        frameworkVersionId,
        name,
        description: description || null,
        dueDate: dueDate || null,
        scopes: includeScope && (businessUnitId || departmentId)
          ? [{ businessUnitId: businessUnitId || null, departmentId: departmentId || null, notes: scopeNotes || null }]
          : null,
      }),
  });

  const canProceedFromStep0 = frameworkVersionId !== "" && name.trim() !== "";

  const handleCreate = async () => {
    setServerError(null);
    try {
      const created = await createMutation.mutateAsync();
      navigate(`/assessments/${created.id}`);
    } catch {
      setServerError("Could not create this assessment.");
    }
  };

  return (
    <Stack spacing={3} sx={{ maxWidth: 700 }}>
      <Typography variant="h4" component="h1">
        Guided Assessment Setup
      </Typography>

      <Stepper activeStep={activeStep}>
        {steps.map((label) => (
          <Step key={label}>
            <StepLabel>{label}</StepLabel>
          </Step>
        ))}
      </Stepper>

      <Paper variant="outlined" sx={{ p: 3 }}>
        {serverError && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {serverError}
          </Alert>
        )}

        {activeStep === 0 && (
          <Stack spacing={2}>
            <TextField select label="Framework Version" fullWidth value={frameworkVersionId} onChange={(e) => setFrameworkVersionId(e.target.value)}>
              {versionOptions.map((v) => (
                <MenuItem key={v.id} value={v.id}>
                  {v.label}
                </MenuItem>
              ))}
            </TextField>
            <TextField label="Name" fullWidth value={name} onChange={(e) => setName(e.target.value)} />
            <TextField label="Description" fullWidth multiline rows={2} value={description} onChange={(e) => setDescription(e.target.value)} />
            <TextField
              label="Due date"
              type="date"
              fullWidth
              slotProps={{ inputLabel: { shrink: true } }}
              value={dueDate}
              onChange={(e) => setDueDate(e.target.value)}
            />
          </Stack>
        )}

        {activeStep === 1 && (
          <Stack spacing={2}>
            <FormControlLabel
              control={<Checkbox checked={includeScope} onChange={(e) => setIncludeScope(e.target.checked)} />}
              label="Limit this assessment to a specific business unit or department"
            />
            {includeScope && (
              <>
                <TextField
                  select
                  label="Business Unit"
                  fullWidth
                  value={businessUnitId}
                  onChange={(e) => {
                    setBusinessUnitId(e.target.value);
                    setDepartmentId("");
                  }}
                >
                  <MenuItem value="">Whole organisation</MenuItem>
                  {businessUnits?.items.map((bu) => (
                    <MenuItem key={bu.id} value={bu.id}>
                      {bu.name}
                    </MenuItem>
                  ))}
                </TextField>
                <TextField select label="Department (optional)" fullWidth value={departmentId} onChange={(e) => setDepartmentId(e.target.value)}>
                  <MenuItem value="">All departments</MenuItem>
                  {departments?.items.map((d) => (
                    <MenuItem key={d.id} value={d.id}>
                      {d.name}
                    </MenuItem>
                  ))}
                </TextField>
                <TextField label="Scope notes" fullWidth value={scopeNotes} onChange={(e) => setScopeNotes(e.target.value)} />
              </>
            )}
          </Stack>
        )}

        {activeStep === 2 && (
          <Stack spacing={1}>
            <Typography variant="subtitle2">Framework</Typography>
            <Typography variant="body2">{selectedVersion?.label ?? "—"}</Typography>
            <Typography variant="subtitle2" sx={{ mt: 1 }}>
              Name
            </Typography>
            <Typography variant="body2">{name || "—"}</Typography>
            <Typography variant="subtitle2" sx={{ mt: 1 }}>
              Scope
            </Typography>
            <Typography variant="body2">
              {includeScope && (businessUnitId || departmentId)
                ? [businessUnits?.items.find((b) => b.id === businessUnitId)?.name, departments?.items.find((d) => d.id === departmentId)?.name]
                    .filter(Boolean)
                    .join(" / ")
                : "Whole organisation"}
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
              Every ACTIVE control mapped to this framework version will be added to the assessment automatically.
            </Typography>
          </Stack>
        )}

        <Box sx={{ display: "flex", justifyContent: "space-between", mt: 3 }}>
          <Button onClick={() => (activeStep === 0 ? navigate("/assessments") : setActiveStep((s) => s - 1))}>
            {activeStep === 0 ? "Cancel" : "Back"}
          </Button>
          {activeStep < steps.length - 1 ? (
            <Button variant="contained" disabled={activeStep === 0 && !canProceedFromStep0} onClick={() => setActiveStep((s) => s + 1)}>
              Next
            </Button>
          ) : (
            <Button variant="contained" onClick={handleCreate} disabled={createMutation.isPending}>
              {createMutation.isPending ? "Creating…" : "Create Assessment"}
            </Button>
          )}
        </Box>
      </Paper>
    </Stack>
  );
}
