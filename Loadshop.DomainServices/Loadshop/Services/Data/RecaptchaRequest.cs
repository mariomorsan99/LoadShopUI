namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class RecaptchaRequest<T>
    {
        public string Token { get; set; }
        public T Data { get; set; }
    }
}
