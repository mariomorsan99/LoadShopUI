namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class GenericResponse<T> : BaseServiceResponse
    {
        public T Data { get; set; }
    }
}
