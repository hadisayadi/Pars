namespace Pars.Application.Workflow;
public record CreateRequestDto(string Type,string Title,string Description);
public record ApprovalActionDto(Guid RequestId,string Action,string Comment);
public interface IWorkflowService { Task<Guid> StartAsync(CreateRequestDto request); Task ApproveAsync(ApprovalActionDto action); Task RejectAsync(ApprovalActionDto action); }
