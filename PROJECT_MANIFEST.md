# Pars System v1.0 Project Manifest

Version: **1.0 – Enterprise HR & Workflow**  
Build date: **2026-08-28**

## v1.0 additions
- Domain: EnterpriseRequest, EnterpriseApproval, LeaveBalance, AuditLog
- Application: Enterprise workflow contracts and dashboard DTOs
- Infrastructure: EnterpriseRequestService + EF configurations
- API: EnterpriseRequestsController + HrDashboardController
- Web: Login, HR Dashboard, Leave, Missions, My Requests, Approval Inbox, History
- Database: `scripts/001_enterprise_v1.sql`
- Windows dev launcher: `start-dev.bat`
- Architecture and release documentation

## Validation note
The current build environment does not contain the .NET SDK, so a real `dotnet build` could not be executed here. XML/JSON and basic source structure checks are recorded in `STATIC_VALIDATION.txt`.
