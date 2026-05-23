namespace events_api.Responses;

public class ServiceResponse<T>
{
    public bool Success { get; private set; }

    public string? Message { get; private set; }

    public T? Data { get; private set; }

    public List<string>? Errors { get; private set; }

    private ServiceResponse()
    {
    }

    // =========================
    // RESPUESTAS EXITOSAS
    // =========================

    public static ServiceResponse<T> Ok(
        T data,
        string? message = null)
    {
        return new ServiceResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ServiceResponse<T> Ok(
        string message)
    {
        return new ServiceResponse<T>
        {
            Success = true,
            Message = message
        };
    }

    public static ServiceResponse<T> Ok()
    {
        return new ServiceResponse<T>
        {
            Success = true
        };
    }

    // =========================
    // RESPUESTAS DE ERROR
    // =========================

    public static ServiceResponse<T> Fail(
        string error)
    {
        return new ServiceResponse<T>
        {
            Success = false,
            Errors = new List<string>
            {
                error
            }
        };
    }

    public static ServiceResponse<T> Fail(
        List<string> errors)
    {
        return new ServiceResponse<T>
        {
            Success = false,
            Errors = errors
        };
    }

    public static ServiceResponse<T> Fail(
        string message,
        List<string> errors)
    {
        return new ServiceResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}