﻿using Oldmansoft.ClassicDomain;
using Oldmansoft.Html.WebMan;
using Oldmansoft.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace WebManApplication.Areas.BusinessManage.Controllers
{
    public class BusinessRoleAccountController : AuthController
    {
        public override Guid OperateResource
        {
            get
            {
                return ResourceProvider.Get<Resource.Business>().Account;
            }
        }

        [Auth(Operation.List)]
        [Location("帐号列表", Icon = FontAwesome.User)]
        public ActionResult Index(Guid SelectedId)
        {
            return Search(SelectedId, null);
        }

        [Auth(Operation.List)]
        [Location("帐号列表", Icon = FontAwesome.User)]
        public ActionResult Search(Guid roleId, string key)
        {
            var role = CreateIdentity().GetRole<Resource.Business>(roleId);
            if (role == null) return HttpNotFound();

            var sourceLocation = Url.Location(new Func<Guid, string, DataTable.Request, JsonResult>(IndexDataSource));
            var table = DataTable.Define<Models.AccountManageListModel>(o => o.Id).Create(sourceLocation.Set("roleId", roleId).Set("key", key));
            table.AddActionItem(Url.Location(new Func<Guid, Guid[], JsonResult>(Remove)).Set("roleId", roleId), Account);
            var searchLocation = Url.Location(new Func<Guid, string, ActionResult>(Search));

            var panel = new Panel();
            panel.ConfigLocation();
            panel.Caption = string.Format("{0} 的帐号列表", role.Name);
            panel.Append(table);

            var result = Content(panel.CreateGrid());
            result.SetQuickSearch(Url.Location(Index));
            return result;
        }

        [Auth(Operation.List)]
        public JsonResult IndexDataSource(Guid roleId, string key, DataTable.Request request)
        {
            var source = CreateIdentity().GetAccounts(request.PageIndex, request.PageSize, out int totalCount, roleId, key);
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

        [Auth(Operation.Modify)]
        [Location("移除")]
        public JsonResult Remove(Guid roleId, Guid[] selectedId)
        {
            if (selectedId == null) return Json(DealResult.Wrong("不允许操作"));

            foreach (var id in selectedId)
            {
                var account = CreateIdentity().GetAccount(id);
                if (account == null) continue;
                var roleIds = new List<Guid>();
                foreach (var role in account.Roles)
                {
                    if (role.Id == roleId) continue;
                    roleIds.Add(role.Id);
                }
                CreateIdentity().AccountSetRole<Resource.Business>(account.Id, roleIds.ToArray());
            }
            return Json(DealResult.Refresh());
        }
    }
}