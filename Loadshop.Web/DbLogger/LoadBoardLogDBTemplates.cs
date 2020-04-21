using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Loadshop.Web.DbLogger
{
    [Table("dbo.ErrorLog")]
    public class LoadBoardLogDbTemplate
    {
        public LoadBoardLogDbTemplate(string programId, string appCode = null)
        {
            ApplCd = appCode ?? programId;
            ProgramId = programId;
            ServerName = Environment.MachineName;
            UserID = Environment.UserDomainName + @"\" + Environment.UserName;
        }

        public Guid ErrorGuid { get; set; }

        [Column("ErrorMessage")]
        public string Message { get; set; }

        [Column("ErrorStack")]
        public string StackTrace { get; set; }

        public string ErrorType { get; set; } = "SYSTEM";
        public string Description { get; set; } = "";
        public string ApplCd { get; set; }
        public string ProgramId { get; set; }
        public string ServerName { get; set; }
        public string UserID { get; set; }
        public string Severity { get; set; } = "Medium";
        public bool IsAlert { get; set; } = false;
        public string RequestURL { get; set; }
        public string RequestBody { get; set; }
        public string ModelState { get; set; }
        public string RouteData { get; set; }
        public Guid? RequestUserId { get; set; }

        //public string UserResolved { get; set; } = "";
        //public string ResolutionComment { get; set; } = "";
        //public DateTime? DateResolved { get; set; }
    }
}
