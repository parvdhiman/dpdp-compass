import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Chip from "@mui/material/Chip";
import CircularProgress from "@mui/material/CircularProgress";
import Divider from "@mui/material/Divider";
import List from "@mui/material/List";
import ListItem from "@mui/material/ListItem";
import ListItemText from "@mui/material/ListItemText";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { useNavigate, useParams } from "react-router-dom";
import { useAuth } from "../auth/AuthProvider";
import * as complianceApi from "./api";
import { ControlDialog } from "./ControlDialog";

export function ControlDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { hasPermission } = useAuth();
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [editOpen, setEditOpen] = useState(false);

  const canManage = hasPermission("controls.manage");

  const { data, isPending, isError } = useQuery({
    queryKey: ["compliance", "controls", id],
    queryFn: () => complianceApi.getControlById(id!),
    enabled: !!id,
  });

  const invalidate = () => queryClient.invalidateQueries({ queryKey: ["compliance", "controls"] });

  const activateMutation = useMutation({ mutationFn: () => complianceApi.activateControl(id!), onSuccess: invalidate });
  const retireMutation = useMutation({ mutationFn: () => complianceApi.retireControl(id!), onSuccess: invalidate });

  if (isPending) return <CircularProgress />;
  if (isError || !data) {
    return <Alert severity="error">Could not load this control. It may not be active for your organisation.</Alert>;
  }

  return (
    <Stack spacing={3}>
      <Button size="small" onClick={() => navigate("/compliance/controls")} sx={{ alignSelf: "flex-start" }}>
        ← Back to Control Library
      </Button>

      <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "flex-start" }}>
        <Stack spacing={0.5}>
          <Typography variant="h4" component="h1">
            {data.controlId} — {data.name}
          </Typography>
          <Stack direction="row" spacing={1}>
            <Chip label={data.riskLevel} size="small" />
            <Chip label={data.categoryName} size="small" variant="outlined" />
            {canManage && <Chip label={data.status} size="small" color={data.status === "ACTIVE" ? "success" : "default"} />}
            <Chip label={`v${data.version}`} size="small" variant="outlined" />
          </Stack>
        </Stack>
        {canManage && (
          <Stack direction="row" spacing={1}>
            <Button variant="outlined" onClick={() => setEditOpen(true)}>
              Edit
            </Button>
            {data.status !== "ACTIVE" && (
              <Button variant="outlined" color="success" onClick={() => activateMutation.mutate()} disabled={activateMutation.isPending}>
                Activate
              </Button>
            )}
            {data.status !== "RETIRED" && (
              <Button variant="outlined" color="warning" onClick={() => retireMutation.mutate()} disabled={retireMutation.isPending}>
                Retire
              </Button>
            )}
          </Stack>
        )}
      </Stack>

      <Paper variant="outlined" sx={{ p: 2 }}>
        <Stack spacing={1.5}>
          <Typography variant="body1">{data.description}</Typography>
          <Typography variant="subtitle2">Objective</Typography>
          <Typography variant="body2">{data.objective}</Typography>
          {data.applicableConditions && (
            <>
              <Typography variant="subtitle2">Applicable conditions</Typography>
              <Typography variant="body2">{data.applicableConditions}</Typography>
            </>
          )}
          {data.guidance && (
            <>
              <Typography variant="subtitle2">Guidance</Typography>
              <Typography variant="body2">{data.guidance}</Typography>
            </>
          )}
          <Divider />
          <Typography variant="caption" color="text.secondary">
            Source: {data.sourceReference}
          </Typography>
          {canManage && (
            <Typography variant="caption" color="text.secondary">
              Legal review status: {data.reviewStatus}
            </Typography>
          )}
        </Stack>
      </Paper>

      <Typography variant="h5" component="h2">
        Mapped Legal Requirements
      </Typography>
      <List component={Paper} variant="outlined" disablePadding>
        {data.mappedRequirements.map((req) => (
          <ListItem key={req.requirementId} divider>
            <ListItemText
              primary={`${req.requirementCode} — ${req.requirementTitle}`}
              secondary={`${req.legalCitation}${req.mappingNotes ? ` — ${req.mappingNotes}` : ""}`}
            />
          </ListItem>
        ))}
        {data.mappedRequirements.length === 0 && (
          <ListItem>
            <ListItemText primary="No mapped requirements." />
          </ListItem>
        )}
      </List>

      <Typography variant="h5" component="h2">
        Assessment Questions
      </Typography>
      <Stack spacing={2}>
        {data.questions.map((q) => (
          <Paper key={q.id} variant="outlined" sx={{ p: 2 }}>
            <Stack spacing={1}>
              <Stack direction="row" spacing={1} sx={{ alignItems: "center" }}>
                <Typography variant="subtitle1">{q.text}</Typography>
                <Chip label={q.questionType} size="small" variant="outlined" />
                {q.isRequired && <Chip label="Required" size="small" color="primary" />}
              </Stack>
              {q.helpText && (
                <Typography variant="body2" color="text.secondary">
                  {q.helpText}
                </Typography>
              )}
              {q.options && q.options.length > 0 && (
                <Typography variant="body2">Options: {q.options.join(", ")}</Typography>
              )}
              {q.evidenceRequirements.length > 0 && (
                <>
                  <Typography variant="caption" color="text.secondary">
                    Evidence required:
                  </Typography>
                  <List dense disablePadding>
                    {q.evidenceRequirements.map((e) => (
                      <ListItem key={e.id} disableGutters>
                        <ListItemText
                          primary={`${e.name}${e.isMandatory ? " (mandatory)" : " (optional)"}`}
                          secondary={e.description ?? undefined}
                        />
                      </ListItem>
                    ))}
                  </List>
                </>
              )}
            </Stack>
          </Paper>
        ))}
        {data.questions.length === 0 && (
          <Typography variant="body2" color="text.secondary">
            No assessment questions defined for this control yet.
          </Typography>
        )}
      </Stack>

      {editOpen && (
        <ControlDialog
          control={data}
          onClose={() => setEditOpen(false)}
          onSaved={() => {
            setEditOpen(false);
            queryClient.invalidateQueries({ queryKey: ["compliance", "controls", id] });
            invalidate();
          }}
        />
      )}
    </Stack>
  );
}
