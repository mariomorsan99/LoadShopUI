using System;

namespace Loadshop.DomainServices.Common.Services.Crud
{
    public class CrudResult<T> : CrudResult
    {
        public CrudResult()
        {

        }

        public T Data { get; set; }


        public static CrudResult<T> Create(T data)
        {
            return new CrudResult<T>()
            {
                Data = data
            };
        }

        public new static CrudResult<T> Create()
        {
            return new CrudResult<T>();
        }

        public new static CrudResult<T> Create(Exception ex)
        {
            return new CrudResult<T>() { Status = CrudResultStatus.Error }.AddExceptions(ex);
        }

        public new CrudResult<T> AddWarning(string message, string property = null)
        {
            Warnings.Add(new CrudWarning() { Message = message, Property = property });

            return this;
        }

        public new CrudResult<T> AddExceptions(params Exception[] exceptions)
        {
            foreach (var ex in exceptions)
                Exceptions.Add(ex);

            return this;
        }
    }
}
