using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oldmansoft.ClassicDomain.Util;
using Oldmansoft.Html.WebMan;
using Oldmansoft.Identity;

namespace WebManApplication.Areas.BusinessManage.Controllers
{
    public class BusinessAccountManageController : AuthController
    {
        public override Guid OperateResource
        {
            get
            {
                return ResourceProvider.Get<Resource.Business>().Account;
            }
        }

        [Auth(Operation.List)]
        [Location("帐号管理", Icon = FontAwesome.User)]
        public ActionResult Index(string key)
        {
            var table = DataTable.Definition<Models.AccountManageListModel>(o => o.Id)
                .Create(Url.Location(IndexDataSource).Set("key", key));
            table.AddActionTable(Url.Location(Create), Account);
            table.AddActionItem(Url.Location(SetPassword), Account);
            table.AddActionItem(Url.Location(SetRole), Account);
            table.AddActionItem(Url.Location(Delete), Account)
                .Confirm("是否删除帐号")
                .OnClientCondition(ItemActionClient.Disable, "data.MemberType == -1")
                .OnClientCondition(ItemActionClient.Disable, "data.MemberId != ''");
            table.AddSearchPanel(Url.Location(Index), "key", key, "帐号");
            
            var panel = new Panel();
            panel.ConfigLocation();
            panel.Append(table);
            return Content(panel.CreateGrid());
        }

        [Auth(Operation.List)]
        public JsonResult IndexDataSource(string key, DataTableRequest request)
        {
            int totalCount;
            var list = CreateIdentity().GetAccounts<Resource.Business>(request.PageIndex, request.PageSize, out totalCount, key);
            return Json(DataTable.Source(list, request, totalCount));
        }

        [Auth(Operation.Append)]
        [Location("添加", Behave = LinkBehave.Open)]
        public ActionResult Create()
        {
            var form = FormHorizontal.Create(new Models.AccountManageCreateModel(), Url.Location(new Func<Models.AccountManageCreateModel, JsonResult>(Create)));
            var panel = new Panel();
            panel.ConfigLocation();
            panel.Append(form);
            return Content(panel);
        }

        [Auth(Operation.Append)]
        [HttpPost]
        public JsonResult Create(Models.AccountManageCreateModel model)
        {
            if (ModelState.ValidateFail())
            {
                return Json(DealResult.Wrong(ModelState.ValidateMessage()));
            }

            model.Name = model.Name.Trim();
            if (CreateIdentity().CreateAccount<Resource.Business>(model.Name, model.Name.CreatePasswordSHA256Hash(model.Password)))
            {
                return Json(DealResult.Refresh());
            }
            return Json(DealResult.Wrong(string.Format("已经存在 {0}", model.Name)));
        }

        [Auth(Operation.Modify)]
        [Location("修改密码", Behave = LinkBehave.Open)]
        public ActionResult SetPassword(Guid selectedId)
        {
            var data = CreateIdentity().GetAccount(selectedId);
            if (data == null) return HttpNotFound();

            var model = data.CopyTo(new Models.AccountManageEditModel());
            var form = FormHorizontal.Create(model, Url.Location(new Func<Models.AccountManageEditModel, JsonResult>(SetPasswordResult)));
            var panel = new Panel();
            panel.ConfigLocation();
            panel.Append(form);
            return Content(panel);
        }

        [Auth(Operation.Modify)]
        public JsonResult SetPasswordResult(Models.AccountManageEditModel model)
        {
            if (ModelState.ValidateFail())
            {
                return Json(DealResult.Wrong(ModelState.ValidateMessage()));
            }
            var data = CreateIdentity().GetAccount(model.Id);
            if (data == null) return Json(DealResult.Wrong("没有数据"));

            CreateIdentity().SetPassword(model.Id, data.Name.CreatePasswordSHA256Hash(model.Password));
            return Json(DealResult.Refresh());
        }

        [Auth(Operation.Modify)]
        [Location("设置角色", Behave = LinkBehave.Open)]
        public ActionResult SetRole(Guid selectedId)
        {
            var data = CreateIdentity().GetAccount(selectedId);
            if (data == null) return HttpNotFound();

            var model = data.CopyTo(new Models.AccountManageSetRoleModel());
            model.RoleId = new List<Guid>();
            foreach (var item in data.Roles)
            {
                model.RoleId.Add(item.Id);
            }
            var dataSource = new ListDataSource();
            var list = dataSource["RoleId"];
            foreach (var item in CreateIdentity().GetRoles<Resource.Business>())
            {
                dataSource["RoleId"].Add(new ListDataItem(item.Name, item.Id.ToString()));
            }
            var form = FormHorizontal.Create(model, Url.Location(new Func<Models.AccountManageSetRoleModel, JsonResult>(SetRoleResult)), dataSource);
            var panel = new Panel();
            panel.ConfigLocation();
            panel.Append(form);
            return Content(panel);
        }

        [Auth(Operation.Modify)]
        public JsonResult SetRoleResult(Models.AccountManageSetRoleModel model)
        {
            CreateIdentity().AccountSetRole<Resource.Business>(model.Id, model.RoleId.ToArray());
            return Json(DealResult.Refresh());
        }

        [Auth(Operation.Remove)]
        [Location("删除")]
        public JsonResult Delete(Guid[] selectedId)
        {
            if (selectedId == null) return Json(DealResult.Wrong("不允许操作"));

            var identity = CreateIdentity();
            foreach (var id in selectedId)
            {
                if (!identity.RemoveAccount(id))
                {
                    return Json(DealResult.WrongRefresh("不允许删除"));
                }
            }
            return Json(DealResult.Refresh());
        }
    }
}