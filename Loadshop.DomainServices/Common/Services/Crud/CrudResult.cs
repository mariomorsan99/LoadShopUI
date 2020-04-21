using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Loadshop.DomainServices.Common.Services.Crud
{
    public class CrudResult
    {
        public CrudResult()
        {

        }

        public ICollection<CrudWarning> Warnings { get; set; } = new List<CrudWarning>();
        public CrudResultStatus Status { get; set; }
        public ModelStateDictionary ModelState { get; set; } = new ModelStateDictionary();

        public bool Successful => CrudResultStatus.Successful == Status;
        public bool Forbidden => CrudResultStatus.Forbidden == Status;
        public bool IsValid => ModelState.IsValid;

        public ICollection<Exception> Exceptions { get; set; } = new List<Exception>();

        public bool HasWarnings => Warnings.Any();

        public bool HasExceptions => Exceptions.Any();

        public static CrudResult Create()
        {
            return new CrudResult();
        }

        public static CrudResult Create(Exception ex)
        {
            return new CrudResult().AddExceptions(ex);
        }

        public CrudResult AddWarning(string message, string property = null)
        {
            Warnings.Add(new CrudWarning() { Message = message, Property = property });

            return this;
        }

        public CrudResult AddExceptions(params Exception[] exceptions)
        {
            foreach (var ex in exceptions)
                Exceptions.Add(ex);

            return this;
        }
    }
}
