using System;
using System.ComponentModel.DataAnnotations;

namespace Loadshop.DomainServices.Loadshop.Services.Data
{
    public class SecurityAccessRoleListData : BaseData
    {
        public Guid AccessRoleId { get; set; }
        [Required]
        [StringLength(50)]
        public string AccessRoleName { get; set; }
        public int AccessRoleLevel { get; set; }
    }
}
