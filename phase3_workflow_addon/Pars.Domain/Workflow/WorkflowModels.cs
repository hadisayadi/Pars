namespace Pars.Domain.Workflow;

public class WorkflowDefinition { public Guid Id {get;set;} public string Code {get;set;} = ""; public string Name {get;set;} = ""; public bool IsActive {get;set;} }
public class WorkflowStep { public Guid Id {get;set;} public Guid WorkflowDefinitionId {get;set;} public string Name {get;set;} = ""; public int Order {get;set;} }
public class WorkflowInstance { public Guid Id {get;set;} public Guid WorkflowDefinitionId {get;set;} public string Status {get;set;} = "Pending"; public DateTime CreatedAt {get;set;} }
public class WorkflowTask { public Guid Id {get;set;} public Guid InstanceId {get;set;} public string Action {get;set;} = ""; public string Status {get;set;} = "Pending"; }
