using Oldmansoft.ClassicDomain.Util;
using Oldmansoft.Html.WebMan;
using Oldmansoft.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebManApplication.Areas.SystemManage.Controllers
{
    public class SystemRoleAccountController : AuthController
    {
        public override Guid OperateResource
        {
            get
            {
                return ResourceProvider.Get<Resource.System>().Account;
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
            var role = CreateIdentity().GetRole<Resource.System>(roleId);
            if (role == null) return HttpNotFound();

            var sourceLocation = Url.Location(new Func<Guid, string, DataTableRequest, JsonResult>(IndexDataSource));
            var table = DataTable.Definition<Models.AccountManageListModel>(o => o.Id).Create(sourceLocation.Set("roleId", roleId).Set("key", key));
            table.AddActionItem(Url.Location(new Func<Guid, Guid[], JsonResult>(Remove)).Set("roleId", roleId), Account);
            var searchLocation = Url.Location(new Func<Guid, string, ActionResult>(Search));
            table.AddSearchPanel(searchLocation.Set("roleId", roleId), "key", key, "帐号");

            var panel = new Panel();
            panel.ConfigLocation();
            panel.Caption = string.Format("{0} 的帐号列表", role.Name);
            panel.Append(table);
            return Content(panel.CreateGrid());
        }

        [Auth(Operation.List)]
        public JsonResult IndexDataSource(Guid roleId, string key, DataTableRequest request)
        {
            int totalCount;
            var list = CreateIdentity().GetAccounts(request.PageIndex, request.PageSize, out totalCount, roleId, key);
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
                CreateIdentity().AccountSetRole(account.Id, roleIds.ToArray());
            }
            return Json(DealResult.Refresh());
        }
    }
}