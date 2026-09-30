namespace HRAttendanceMVC.Domain.Enities;

public class ServiceResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, string> Errors { get; set; } = new();

    public static ServiceResult Ok(string? message = null)
        => new() { Success = true, Message = message };

    public static ServiceResult Failed(string error, string? propertyName = null)
    {
        var result = new ServiceResult { Success = false, Message = error };
        if (!string.IsNullOrWhiteSpace(propertyName))
        {
            result.Errors[propertyName] = error;
        }
        return result;
    }
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; set; }

    public static ServiceResult<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public new static ServiceResult<T> Failed(string error, string? propertyName = null)
    {
        var result = new ServiceResult<T> { Success = false, Message = error };
        if (!string.IsNullOrWhiteSpace(propertyName))
        {
            result.Errors[propertyName] = error;
        }
        return result;
    }
}
