using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oldmansoft.Html;
using Oldmansoft.Html.WebMan;
using Oldmansoft.Identity;
using System;
using System.Collections.Generic;

namespace WebManApplicationCore.Controllers
{
    public class HomeController : AuthController
    {
        public override Guid OperateResource
        {
            get
            {
                return Guid.Empty;
            }
        }

        public ActionResult Index()
        {
            if (Account == null) return RedirectToAction("Login");

            var document = new ManageDocument(Url.Location(Welcome));
            document.Resources.Select2.Enabled = true;
            document.Resources.AddScript(new Oldmansoft.Html.Element.ScriptResource(Url.Content("~/js/oldmansoft-webman.cn.js")));
            document.Resources.AddScript(new Oldmansoft.Html.Element.ScriptResource("//cdn.bootcss.com/bootstrap-validator/0.5.3/js/language/zh_CN.min.js"));
            document.Resources.WebApp.Link.Href = Url.Content("~/css/oldmansoft-webapp.css");
            document.Resources.WebMan.Link.Href = Url.Content("~/css/oldmansoft-webman.css");
            document.Resources.WebApp.Script.Src = Url.Content("~/js/oldmansoft-webapp.js");
            document.Resources.WebMan.Script.Src = Url.Content("~/js/oldmansoft-webman.js");
            document.Resources.PluginFix.Script.Src = Url.Content("~/js/oldmansoft-plugin-fix.js");

            document.Resources.AddLink(new Oldmansoft.Html.Element.Link("//cdn.bootcss.com/lity/2.3.0/lity.min.css"));
            document.Resources.AddScript(new Oldmansoft.Html.Element.ScriptResource("//cdn.bootcss.com/lity/2.3.0/lity.min.js"));

            document.Resources.Bootstrap.Link = new Oldmansoft.Html.Element.Link("//cdn.bootcss.com/bootstrap/3.3.7/css/bootstrap.min.css");
            document.Resources.Bootstrap.Script = new Oldmansoft.Html.Element.ScriptResource("//cdn.bootcss.com/bootstrap/3.3.7/js/bootstrap.min.js");
            document.Resources.JQuery.Script = new Oldmansoft.Html.Element.ScriptResource("//cdn.bootcss.com/jquery/1.12.4/jquery.min.js");
            document.Resources.Sha256.Script = new Oldmansoft.Html.Element.ScriptResource("//cdn.bootcss.com/js-sha256/0.5.0/sha256.min.js");
            document.Resources.JQueryForm.Script = new Oldmansoft.Html.Element.ScriptResource("//cdn.bootcss.com/jquery.form/4.2.1/jquery.form.min.js");
            document.Title = "示例";

            document.Menu.Add(
                new TreeListItem("业务", FontAwesome.Suitcase)
                    .Add(new TreeListItem(Url.Location<Areas.BusinessManage.Controllers.BusinessAccountManageController>(o => o.Index)), Account)
                    .Add(new TreeListItem(Url.Location<Areas.BusinessManage.Controllers.BusinessRoleManageController>(o => o.Index)), Account)
            );
            document.Menu.Add(
                new TreeListItem("系统管理", FontAwesome.Shield)
                    .Add(new TreeListItem(Url.Location<Areas.SystemManage.Controllers.SystemAccountManageController>(o => o.Index)), Account)
                    .Add(new TreeListItem(Url.Location<Areas.SystemManage.Controllers.SystemRoleManageController>(o => o.Index)), Account)
            );

            document.Quick.Avatar.Display = Account.Name;
            document.Quick.Add(Url.Location(Logoff));
            return Content(document);
        }

        public ActionResult Welcome()
        {
            return Content("Welcome");
        }

        [Location("退出", Icon = FontAwesome.Sign_Out, Behave = LinkBehave.Self)]
        public ActionResult Logoff()
        {
            HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Home");
        }

        [AllowAnonymous]
        public ActionResult Seed()
        {
            string seed = Guid.NewGuid().ToString().Substring(0, 4);
            TempData["HashSeed"] = seed;
            return Content(seed);
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            var document = new LoginDocument(Url.Location(Seed), Url.Location(new Func<string, string, JsonResult>(Login)));
            document.Resources.AddScript(new Oldmansoft.Html.Element.ScriptResource(Url.Content("~/js/oldmansoft-webman.cn.js")));
            document.Resources.WebApp.Link.Href = Url.Content("~/css/oldmansoft-webapp.css");
            document.Resources.WebMan.Link.Href = Url.Content("~/css/oldmansoft-webman.css");
            document.Resources.WebApp.Script.Src = Url.Content("~/js/oldmansoft-webapp.js");
            document.Resources.WebMan.Script.Src = Url.Content("~/js/oldmansoft-webman.js");
            document.Resources.PluginFix.Script.Src = Url.Content("~/js/oldmansoft-plugin-fix.js");
            document.Resources.Bootstrap.Link = new Oldmansoft.Html.Element.Link("//cdn.bootcss.com/bootstrap/3.3.7/css/bootstrap.min.css");
            document.Resources.Bootstrap.Script = new Oldmansoft.Html.Element.ScriptResource("//cdn.bootcss.com/bootstrap/3.3.7/js/bootstrap.min.js");
            document.Resources.JQuery.Script = new Oldmansoft.Html.Element.ScriptResource("//cdn.bootcss.com/jquery/1.12.4/jquery.min.js");
            document.Resources.Sha256.Script = new Oldmansoft.Html.Element.ScriptResource("//cdn.bootcss.com/js-sha256/0.5.0/sha256.min.js");
            document.Resources.JQueryForm.Script = new Oldmansoft.Html.Element.ScriptResource("//cdn.bootcss.com/jquery.form/4.2.1/jquery.form.min.js");
            document.Title = "登录示例";
            return Content(document);
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult Login(string account, string hash)
        {
            if (!(TempData["HashSeed"] is string hashSeed))
            {
                return Json(DealResult.Wrong("脚本运行不正确"));
            }
            if (PasswordGuard.Instance.IsLockedOut(account))
            {
                return Json(DealResult.Wrong("请稍候尝试登录"));
            }
            var data = CreateIdentity().GetAccount(account, hash, hashSeed);
            if (data != null)
            {
                var claims = new List<System.Security.Claims.Claim>()
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, data.Name),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, data.Id.ToString("N")),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin"),
                };
                var identity = new System.Security.Claims.ClaimsIdentity(claims, "ApplicationCookie");
                HttpContext.SignInAsync(new System.Security.Claims.ClaimsPrincipal(new[] { identity }));
                return Json(DealResult.Location(Url.Location<HomeController>(o => o.Index)));
            }
            else
            {
                PasswordGuard.Instance.SetCount(account);
                return Json(DealResult.Wrong("帐号或密码错误"));
            }
        }

        /// <summary>
        /// 初始化数据库的系统管理员帐号
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Init()
        {
            var identity = CreateIdentity();
            if (identity.GetRoles<Resource.System>().Count > 0) return NotFound();

            var document = new Oldmansoft.Html.Element.Document();

            var form = new HtmlElement(HtmlTag.Form);
            document.Body.Append(form);
            form.Attribute(HtmlAttribute.Method, "post");

            var label = new HtmlElement(HtmlTag.Label);
            form.Append(label);
            label.Text("帐号");
            var input = new HtmlElement(HtmlTag.Input);
            label.Append(input);
            input.Attribute(HtmlAttribute.Name, "Account");

            label = new HtmlElement(HtmlTag.Label);
            form.Append(label);
            label.Text("密码");
            input = new HtmlElement(HtmlTag.Input);
            label.Append(input);
            input.Attribute(HtmlAttribute.Name, "Password");
            input.Attribute(HtmlAttribute.Type, "password");

            input = new HtmlElement(HtmlTag.Input);
            form.Append(input);
            input.Attribute(HtmlAttribute.Type, "submit");

            return Content(document);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Init(string account, string password)
        {
            if (string.IsNullOrWhiteSpace(account)) return Init();
            if (string.IsNullOrEmpty(password)) return Init();
            account = account.Trim();

            if (CreateIdentity().InitAdminAccount<Resource.System>(account, account.CreatePasswordSHA256Hash(password)))
            {
                return Redirect("/");
            }
            else
            {
                return NotFound();
            }
        }
    }
}
