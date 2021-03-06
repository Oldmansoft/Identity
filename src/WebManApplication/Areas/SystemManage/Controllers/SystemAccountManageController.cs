﻿using Oldmansoft.ClassicDomain;
using Oldmansoft.Html.WebMan;
using Oldmansoft.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace WebManApplication.Areas.SystemManage.Controllers
{
    public class SystemAccountManageController : AuthController
    {
        public override Guid OperateResource
        {
            get
            {
                return ResourceProvider.Get<Resource.System>().Account;
            }
        }

        [Auth(Operation.List)]
        [Location("帐号管理", Icon = FontAwesome.User)]
        public ActionResult Index(string key)
        {
            var table = DataTable.Define<Models.AccountManageListModel>(o => o.Id)
                .Create(Url.Location(IndexDataSource).Set("key", key));
            table.AddActionTable(Url.Location(Create), Account);
            table.AddActionItem(Url.Location(SetPassword), Account).OnClientCondition(ItemActionClient.Disable, string.Format("data.Partition != '{0}'", ResourceProvider.GetResourceData<Resource.System>().Name));
            table.AddActionItem(Url.Location(SetRole), Account).OnClientCondition(ItemActionClient.Disable, string.Format("data.Partition != '{0}'", ResourceProvider.GetResourceData<Resource.System>().Name));
            table.AddActionItem(Url.Location(Delete), Account)
                .Confirm("是否删除帐号")
                .OnClientCondition(ItemActionClient.Hide, "data.MemberType == -1")
                .OnClientCondition(ItemActionClient.Disable, "data.MemberId != ''")
                .OnClientCondition(ItemActionClient.Disable, string.Format("data.Partition != '{0}'", ResourceProvider.GetResourceData<Resource.System>().Name));
            table.SetRowClassNameWhenClientCondition("text-warning", string.Format("data.Partition != '{0}'", ResourceProvider.GetResourceData<Resource.System>().Name));

            var panel = new Panel();
            panel.ConfigLocation();
            panel.Append(table);

            var result = Content(panel.CreateGrid());
            result.SetQuickSearch(Url.Location(Index));
            return result;
        }

        [Auth(Operation.List)]
        public JsonResult IndexDataSource(string key, DataTable.Request request)
        {
            var source = CreateIdentity().GetAccounts(request.PageIndex, request.PageSize, out int totalCount, key);
            var list = new List<Models.AccountManageListModel>();
            foreach (var item in source)
            {
                var model = item.MapTo(new Models.AccountManageListModel());
                model.Partition = ResourceProvider.GetResourceData<Resource>(item.PartitionResourceId).Name;
                model.Roles = item.Roles.Select(o => o.Name).ToList();
                list.Add(model);
            }
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
            if (CreateIdentity().CreateAccount<Resource.System>(model.Name, model.Name.CreatePasswordSHA256Hash(model.Password)))
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

            var model = data.MapTo(new Models.AccountManageEditModel());
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

            var model = data.MapTo(new Models.AccountManageSetRoleModel());
            model.RoleId = new List<Guid>();
            foreach (var item in data.Roles)
            {
                model.RoleId.Add(item.Id);
            }
            var dataSource = new ListDataSource();
            var list = dataSource["RoleId"];
            foreach (var item in CreateIdentity().GetRoles<Resource.System>())
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
            if (model.RoleId == null) model.RoleId = new List<Guid>();
            CreateIdentity().AccountSetRole<Resource.System>(model.Id, model.RoleId.ToArray());
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