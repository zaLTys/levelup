namespace Duende.IDP.Models
{
    public class ServiceResponse
    {
        public ServiceResponse()
        {
        }

        public bool Success { get; set; }
        public string Message { get; set; }

        public static ServiceResponse Error(string message)
            => new ServiceResponse { Success = false, Message = message };
    }

    public class ServiceResponse<T> : ServiceResponse
    {
        public ServiceResponse()
        {
        }

        public T Data { get; set; }


        public static ServiceResponse<T> Error(string message)
            => new ServiceResponse<T> { Success = false, Message = message };
        public static ServiceResponse<T> FromData(T data)
            => new ServiceResponse<T> { Success = true, Data = data };
        public static ServiceResponse<T> FromData(T data, string message)
        => new ServiceResponse<T> { Success = true, Data = data, Message = message };
    }
}
