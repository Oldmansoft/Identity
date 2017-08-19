using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebManApplication.Areas.BusinessManage.Models
{
    public class RoleManageListModel
    {
        public Guid Id { get; set; }

        [Display(Name = "名称")]
        public string Name { get; set; }

        [Display(Name = "说明")]
        public string Description { get; set; }
    }

    public class RoleManageListMoreModel : RoleManageListModel
    {
        public bool HasAccount { get; set; }
    }

    public class RoleManageEditModel
    {
        [Display(Name = "序号")]
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "名称")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "说明")]
        public string Description { get; set; }
    }
}