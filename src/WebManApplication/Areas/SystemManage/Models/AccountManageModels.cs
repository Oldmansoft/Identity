using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebManApplication.Areas.SystemManage.Models
{
    public class AccountManageListModel
    {
        public Guid Id { get; set; }

        [Display(Name = "区名")]
        public string Partition { get; set; }

        [Display(Name = "帐号")]
        public string Name { get; set; }

        [Display(Name = "角色")]
        public List<string> Roles { get; set; }
    }

    public class AccountManageCreateModel
    {
        public Guid Id { get; set; }

        [Required]
        [Display(Name = "帐号")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "密码")]
        [DataType(DataType.Password)]
        [StringLength(30, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "确认")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }
    }

    public class AccountManageEditModel
    {
        public Guid Id { get; set; }

        [Display(Name = "帐号")]
        [System.ComponentModel.ReadOnly(true)]
        public string Name { get; set; }

        [Required]
        [Display(Name = "密码")]
        [DataType(DataType.Password)]
        [StringLength(30, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "确认")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "密码和确认密码不匹配。")]
        public string ConfirmPassword { get; set; }
    }

    public class AccountManageSetRoleModel
    {
        public Guid Id { get; set; }

        [Display(Name = "帐号")]
        [System.ComponentModel.ReadOnly(true)]
        public string Name { get; set; }

        [Display(Name = "系统角色")]
        [Oldmansoft.Html.WebMan.Annotations.CustomInput(typeof(Oldmansoft.Html.WebMan.Input.Select2))]
        public List<Guid> RoleId { get; set; }
    }
}