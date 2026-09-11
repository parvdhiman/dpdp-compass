import { QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import CssBaseline from "@mui/material/CssBaseline";
import { ThemeProvider } from "@mui/material/styles";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import { queryClient } from "./app/queryClient";
import { theme } from "./app/theme";
import { AuthenticatedLayout } from "./components/layout/AuthenticatedLayout";
import { AuthProvider } from "./features/auth/AuthProvider";
import { ForgotPasswordPage } from "./features/auth/ForgotPasswordPage";
import { LoginPage } from "./features/auth/LoginPage";
import { ProtectedRoute } from "./features/auth/ProtectedRoute";
import { AssessmentApprovalPage } from "./features/assessments/AssessmentApprovalPage";
import { AssessmentListPage } from "./features/assessments/AssessmentListPage";
import { AssessmentQuestionnairePage } from "./features/assessments/AssessmentQuestionnairePage";
import { AssessmentReviewerPage } from "./features/assessments/AssessmentReviewerPage";
import { AssessmentSummaryPage } from "./features/assessments/AssessmentSummaryPage";
import { AssessmentWizardPage } from "./features/assessments/AssessmentWizardPage";
import { CreateAssessmentPage } from "./features/assessments/CreateAssessmentPage";
import { BusinessUnitsPage } from "./features/business-units/BusinessUnitsPage";
import { ControlDetailPage } from "./features/compliance/ControlDetailPage";
import { ControlLibraryPage } from "./features/compliance/ControlLibraryPage";
import { EvidenceRequirementsPage } from "./features/compliance/EvidenceRequirementsPage";
import { FrameworksPage } from "./features/compliance/FrameworksPage";
import { FrameworkVersionPage } from "./features/compliance/FrameworkVersionPage";
import { QuestionLibraryPage } from "./features/compliance/QuestionLibraryPage";
import { DepartmentsPage } from "./features/departments/DepartmentsPage";
import { ConsentPrivacyDashboardPage } from "./features/consent-privacy/ConsentPrivacyDashboardPage";
import { DataDiscoveryDashboardPage } from "./features/data-discovery/DataDiscoveryDashboardPage";
import { DataInventoryDashboardPage } from "./features/data-inventory/DataInventoryDashboardPage";
import { EvidenceDashboardPage } from "./features/evidence/EvidenceDashboardPage";
import { EvidenceDetailPage } from "./features/evidence/EvidenceDetailPage";
import { FindingDashboardPage } from "./features/findings/FindingDashboardPage";
import { FindingDetailsPage } from "./features/findings/FindingDetailsPage";
import { OrganisationDashboardPage } from "./features/organisation/OrganisationDashboardPage";
import { OrganisationProfilePage } from "./features/organisation/OrganisationProfilePage";
import { PermissionsPage } from "./features/permissions/PermissionsPage";
import { ProfilePage } from "./features/profile/ProfilePage";
import { CreateRemediationTaskPage } from "./features/remediation/CreateRemediationTaskPage";
import { OverdueTasksPage } from "./features/remediation/OverdueTasksPage";
import { RemediationDashboardPage } from "./features/remediation/RemediationDashboardPage";
import { RemediationTaskDetailPage } from "./features/remediation/RemediationTaskDetailPage";
import { RiskRegisterPage } from "./features/risks/RiskRegisterPage";
import { RolesPage } from "./features/roles/RolesPage";
import { SystemStatusPage } from "./features/system/SystemStatusPage";
import { CreateUserPage } from "./features/users/CreateUserPage";
import { EditUserPage } from "./features/users/EditUserPage";
import { UserListPage } from "./features/users/UserListPage";

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <AuthProvider>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/forgot-password" element={<ForgotPasswordPage />} />

              <Route
                path="/"
                element={
                  <ProtectedRoute>
                    <AuthenticatedLayout>
                      <OrganisationDashboardPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              {/* Diagnostic page, deliberately not linked from the nav — see docs/ARCHITECTURE.md section 14. */}
              <Route
                path="/system-status"
                element={
                  <ProtectedRoute>
                    <AuthenticatedLayout>
                      <SystemStatusPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/organisation"
                element={
                  <ProtectedRoute requiredPermission="organisation.read">
                    <AuthenticatedLayout>
                      <OrganisationProfilePage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/business-units"
                element={
                  <ProtectedRoute requiredPermission="businessunits.read">
                    <AuthenticatedLayout>
                      <BusinessUnitsPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/departments"
                element={
                  <ProtectedRoute requiredPermission="departments.read">
                    <AuthenticatedLayout>
                      <DepartmentsPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/users"
                element={
                  <ProtectedRoute requiredPermission="users.read">
                    <AuthenticatedLayout>
                      <UserListPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/users/new"
                element={
                  <ProtectedRoute requiredPermission="users.create">
                    <AuthenticatedLayout>
                      <CreateUserPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/users/:id/edit"
                element={
                  <ProtectedRoute requiredPermission="users.update">
                    <AuthenticatedLayout>
                      <EditUserPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/roles"
                element={
                  <ProtectedRoute requiredPermission="roles.read">
                    <AuthenticatedLayout>
                      <RolesPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/permissions"
                element={
                  <ProtectedRoute requiredPermission="roles.read">
                    <AuthenticatedLayout>
                      <PermissionsPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/compliance/frameworks"
                element={
                  <ProtectedRoute requiredPermission="controls.read">
                    <AuthenticatedLayout>
                      <FrameworksPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/compliance/framework-versions/:id"
                element={
                  <ProtectedRoute requiredPermission="controls.read">
                    <AuthenticatedLayout>
                      <FrameworkVersionPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/compliance/controls"
                element={
                  <ProtectedRoute requiredPermission="controls.read">
                    <AuthenticatedLayout>
                      <ControlLibraryPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/compliance/controls/:id"
                element={
                  <ProtectedRoute requiredPermission="controls.read">
                    <AuthenticatedLayout>
                      <ControlDetailPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/compliance/questions"
                element={
                  <ProtectedRoute requiredPermission="controls.read">
                    <AuthenticatedLayout>
                      <QuestionLibraryPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/compliance/evidence-requirements"
                element={
                  <ProtectedRoute requiredPermission="controls.read">
                    <AuthenticatedLayout>
                      <EvidenceRequirementsPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/assessments"
                element={
                  <ProtectedRoute requiredPermission="assessments.read">
                    <AuthenticatedLayout>
                      <AssessmentListPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/assessments/new"
                element={
                  <ProtectedRoute requiredPermission="assessments.create">
                    <AuthenticatedLayout>
                      <CreateAssessmentPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/assessments/wizard"
                element={
                  <ProtectedRoute requiredPermission="assessments.create">
                    <AuthenticatedLayout>
                      <AssessmentWizardPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/assessments/:id"
                element={
                  <ProtectedRoute requiredPermission="assessments.read">
                    <AuthenticatedLayout>
                      <AssessmentSummaryPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/assessments/:id/questionnaire"
                element={
                  <ProtectedRoute requiredPermission="assessments.read">
                    <AuthenticatedLayout>
                      <AssessmentQuestionnairePage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/assessments/:id/review"
                element={
                  <ProtectedRoute requiredPermission="assessments.review">
                    <AuthenticatedLayout>
                      <AssessmentReviewerPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/assessments/:id/approve"
                element={
                  <ProtectedRoute requiredPermission="assessments.approve">
                    <AuthenticatedLayout>
                      <AssessmentApprovalPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/findings"
                element={
                  <ProtectedRoute requiredPermission="findings.read">
                    <AuthenticatedLayout>
                      <FindingDashboardPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/findings/:id"
                element={
                  <ProtectedRoute requiredPermission="findings.read">
                    <AuthenticatedLayout>
                      <FindingDetailsPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/risks"
                element={
                  <ProtectedRoute requiredPermission="risks.read">
                    <AuthenticatedLayout>
                      <RiskRegisterPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/remediation-tasks"
                element={
                  <ProtectedRoute requiredPermission="remediation.read">
                    <AuthenticatedLayout>
                      <RemediationDashboardPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/remediation-tasks/overdue"
                element={
                  <ProtectedRoute requiredPermission="remediation.read">
                    <AuthenticatedLayout>
                      <OverdueTasksPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/remediation-tasks/new"
                element={
                  <ProtectedRoute requiredPermission="remediation.manage">
                    <AuthenticatedLayout>
                      <CreateRemediationTaskPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/remediation-tasks/:id"
                element={
                  <ProtectedRoute requiredPermission="remediation.read">
                    <AuthenticatedLayout>
                      <RemediationTaskDetailPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/evidence"
                element={
                  <ProtectedRoute requiredPermission="evidence.read">
                    <AuthenticatedLayout>
                      <EvidenceDashboardPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/evidence/:id"
                element={
                  <ProtectedRoute requiredPermission="evidence.read">
                    <AuthenticatedLayout>
                      <EvidenceDetailPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/data-discovery"
                element={
                  <ProtectedRoute requiredPermission="datasources.read">
                    <AuthenticatedLayout>
                      <DataDiscoveryDashboardPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/data-inventory"
                element={
                  <ProtectedRoute requiredPermission="datainventory.read">
                    <AuthenticatedLayout>
                      <DataInventoryDashboardPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/consent-privacy"
                element={
                  <ProtectedRoute requiredPermission="privacynotices.read">
                    <AuthenticatedLayout>
                      <ConsentPrivacyDashboardPage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
              <Route
                path="/profile"
                element={
                  <ProtectedRoute>
                    <AuthenticatedLayout>
                      <ProfilePage />
                    </AuthenticatedLayout>
                  </ProtectedRoute>
                }
              />
            </Routes>
          </AuthProvider>
        </BrowserRouter>
        <ReactQueryDevtools initialIsOpen={false} />
      </QueryClientProvider>
    </ThemeProvider>
  );
}

export default App;
