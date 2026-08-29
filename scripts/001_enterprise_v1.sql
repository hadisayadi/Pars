/* Pars System v1.0 - Enterprise HR & Workflow
   Run against the Pars SQL Server database before launching v1.0.
   This script only creates application-owned schemas/tables when absent.
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF SCHEMA_ID('auth') IS NULL EXEC('CREATE SCHEMA auth');
IF SCHEMA_ID('workflow') IS NULL EXEC('CREATE SCHEMA workflow');
IF SCHEMA_ID('hr') IS NULL EXEC('CREATE SCHEMA hr');
IF SCHEMA_ID('audit') IS NULL EXEC('CREATE SCHEMA audit');

IF OBJECT_ID('auth.Users','U') IS NULL
CREATE TABLE auth.Users(
 Id uniqueidentifier NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
 Username nvarchar(50) NOT NULL,
 PasswordHash nvarchar(max) NOT NULL,
 FirstName nvarchar(50) NULL, LastName nvarchar(50) NULL, Email nvarchar(100) NULL,
 IsActive bit NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT(1),
 CreatedAt datetime2 NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
 CONSTRAINT UQ_Users_Username UNIQUE(Username)
);

IF OBJECT_ID('auth.Roles','U') IS NULL
CREATE TABLE auth.Roles(
 Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
 Name nvarchar(50) NOT NULL, Description nvarchar(100) NULL,
 CONSTRAINT UQ_Roles_Name UNIQUE(Name)
);

IF OBJECT_ID('auth.UserRoles','U') IS NULL
CREATE TABLE auth.UserRoles(
 UserId uniqueidentifier NOT NULL, RoleId int NOT NULL,
 CONSTRAINT PK_UserRoles PRIMARY KEY(UserId,RoleId),
 CONSTRAINT FK_UserRoles_User FOREIGN KEY(UserId) REFERENCES auth.Users(Id) ON DELETE CASCADE,
 CONSTRAINT FK_UserRoles_Role FOREIGN KEY(RoleId) REFERENCES auth.Roles(Id) ON DELETE CASCADE
);

IF OBJECT_ID('workflow.Requests','U') IS NULL
CREATE TABLE workflow.Requests(
 Id uniqueidentifier NOT NULL CONSTRAINT PK_WorkflowRequests PRIMARY KEY,
 Kind int NOT NULL, PersonId nvarchar(50) NOT NULL, RequestedByUserId uniqueidentifier NOT NULL,
 Title nvarchar(200) NOT NULL, Description nvarchar(1000) NULL,
 StartDate datetime2 NOT NULL, EndDate datetime2 NOT NULL, StartTime time NULL, EndTime time NULL,
 IsHourly bit NOT NULL CONSTRAINT DF_Requests_IsHourly DEFAULT(0), Destination nvarchar(200) NULL,
 HasVehicle bit NOT NULL CONSTRAINT DF_Requests_HasVehicle DEFAULT(0), SubstitutePersonId nvarchar(50) NULL,
 Status int NOT NULL CONSTRAINT DF_Requests_Status DEFAULT(0), CurrentStep int NOT NULL CONSTRAINT DF_Requests_CurrentStep DEFAULT(0),
 CreatedAt datetime2 NOT NULL CONSTRAINT DF_Requests_CreatedAt DEFAULT SYSUTCDATETIME(), SubmittedAt datetime2 NULL, ClosedAt datetime2 NULL
);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Requests_UserCreated' AND object_id=OBJECT_ID('workflow.Requests'))
 CREATE INDEX IX_Requests_UserCreated ON workflow.Requests(RequestedByUserId,CreatedAt DESC);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Requests_StatusKind' AND object_id=OBJECT_ID('workflow.Requests'))
 CREATE INDEX IX_Requests_StatusKind ON workflow.Requests(Status,Kind);

IF OBJECT_ID('workflow.Approvals','U') IS NULL
CREATE TABLE workflow.Approvals(
 Id uniqueidentifier NOT NULL CONSTRAINT PK_Approvals PRIMARY KEY,
 RequestId uniqueidentifier NOT NULL, Step int NOT NULL, StepName nvarchar(100) NOT NULL,
 ApproverUserId uniqueidentifier NULL, ApproverRole nvarchar(50) NOT NULL,
 Status nvarchar(20) NOT NULL CONSTRAINT DF_Approvals_Status DEFAULT('Pending'), Comment nvarchar(1000) NULL,
 CreatedAt datetime2 NOT NULL CONSTRAINT DF_Approvals_CreatedAt DEFAULT SYSUTCDATETIME(), ActionAt datetime2 NULL,
 CONSTRAINT FK_Approvals_Request FOREIGN KEY(RequestId) REFERENCES workflow.Requests(Id) ON DELETE CASCADE
);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_Approvals_StatusRole' AND object_id=OBJECT_ID('workflow.Approvals'))
 CREATE INDEX IX_Approvals_StatusRole ON workflow.Approvals(Status,ApproverRole);

IF OBJECT_ID('hr.LeaveBalances','U') IS NULL
CREATE TABLE hr.LeaveBalances(
 Id uniqueidentifier NOT NULL CONSTRAINT PK_LeaveBalances PRIMARY KEY,
 PersonId nvarchar(50) NOT NULL, [Year] int NOT NULL,
 AnnualEntitlementDays decimal(8,2) NOT NULL CONSTRAINT DF_LeaveBalance_Annual DEFAULT(26),
 CarriedDays decimal(8,2) NOT NULL CONSTRAINT DF_LeaveBalance_Carried DEFAULT(0),
 UsedDays decimal(8,2) NOT NULL CONSTRAINT DF_LeaveBalance_Used DEFAULT(0),
 UpdatedAt datetime2 NOT NULL CONSTRAINT DF_LeaveBalance_Updated DEFAULT SYSUTCDATETIME(),
 CONSTRAINT UQ_LeaveBalance_PersonYear UNIQUE(PersonId,[Year])
);

IF OBJECT_ID('audit.AuditLogs','U') IS NULL
CREATE TABLE audit.AuditLogs(
 Id bigint IDENTITY(1,1) NOT NULL CONSTRAINT PK_AuditLogs PRIMARY KEY,
 UserId uniqueidentifier NULL, Action nvarchar(100) NOT NULL, Entity nvarchar(100) NOT NULL,
 EntityId nvarchar(100) NULL, Details nvarchar(2000) NULL,
 CreatedAt datetime2 NOT NULL CONSTRAINT DF_AuditLogs_Created DEFAULT SYSUTCDATETIME()
);
IF NOT EXISTS(SELECT 1 FROM sys.indexes WHERE name='IX_AuditLogs_CreatedAt' AND object_id=OBJECT_ID('audit.AuditLogs'))
 CREATE INDEX IX_AuditLogs_CreatedAt ON audit.AuditLogs(CreatedAt DESC);

IF NOT EXISTS(SELECT 1 FROM auth.Roles WHERE Name='User') INSERT auth.Roles(Name,Description) VALUES(N'User',N'کاربر عادی');
IF NOT EXISTS(SELECT 1 FROM auth.Roles WHERE Name='Manager') INSERT auth.Roles(Name,Description) VALUES(N'Manager',N'مدیر مستقیم و تأییدکننده مرحله اول');
IF NOT EXISTS(SELECT 1 FROM auth.Roles WHERE Name='HR') INSERT auth.Roles(Name,Description) VALUES(N'HR',N'منابع انسانی و تأییدکننده نهایی');
IF NOT EXISTS(SELECT 1 FROM auth.Roles WHERE Name='Admin') INSERT auth.Roles(Name,Description) VALUES(N'Admin',N'مدیر سامانه');

COMMIT TRANSACTION;
GO
