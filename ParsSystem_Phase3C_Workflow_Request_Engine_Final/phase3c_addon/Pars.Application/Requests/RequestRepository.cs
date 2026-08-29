namespace Pars.Application.Requests;
public interface IRequestRepository { Task<object?> GetAsync(int id); Task<IEnumerable<object>> GetMyRequestsAsync(int userId); }
public class RequestService { }
