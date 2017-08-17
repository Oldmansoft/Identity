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
    public class SystemRoleManageController : AuthController
    {
        public override Guid OperateResource
        {
            get
            {
                return ResourceProvider.Get<Resource.System>().Role;
            }
        }

        private List<Oldmansoft.Identity.Data.ResourceData> ResourceSupport()
        {
            var result = new List<Oldmansoft.Identity.Data.ResourceData>();
            result.Add(CreateIdentity().GetResource<Resource.System>());
            result.Add(CreateIdentity().GetResource<Resource.Business>());
            return result;
        }

        [Auth(Operation.List)]
        [Location("系统角色", Icon = FontAwesome.User)]
        public ActionResult Index(string key)
        {
            var table = DataTable.Definition<Models.RoleManageListModel>(o => o.Id).Create(Url.Location(IndexDataSource).Set("key", key));
            table.AddActionTable(Url.Location(Create), Account);
            table.AddActionItem(Url.Location<SystemRoleAccountController>(o => o.Index));
            table.AddActionItem(Url.Location(Edit), Account);
            table.AddActionItem(Url.Location(Delete), Account).Confirm("是否删除角色");
            table.AddSearchPanel(Url.Location(Index), "key", key, "名称");

            var panel = new Panel();
            panel.ConfigLocation();
            panel.Append(table);
            return Content(panel.CreateGrid());
        }

        [Auth(Operation.List)]
        public JsonResult IndexDataSource(string key, DataTableRequest request)
        {
            int totalCount;
            var list = CreateIdentity().GetRoles<Resource.System>(request.PageIndex, request.PageSize, out totalCount, key);
            return Json(DataTable.Source(list, request, totalCount));
        }

        private void SetResourceInput(FormHorizontal form, Oldmansoft.Identity.Data.ResourceData resource, Dictionary<Guid, List<string>> permissions)
        {
            var div = new Oldmansoft.Html.HtmlElement(Oldmansoft.Html.HtmlTag.Div);
            foreach (var item in resource.Children)
            {
                List<string> value = null;
                if (permissions != null) permissions.TryGetValue(item.Id, out value);

                var input = new Oldmansoft.Html.WebMan.FormInputCreator.Inputs.CheckBoxList();
                input.Init("Operation" + item.Id, typeof(string), value, Extends.GetOperationDataSourceList());
                input.SetInputMode(false, false, null);

                var inputHead = new Oldmansoft.Html.HtmlElement(Oldmansoft.Html.HtmlTag.Label);
                inputHead.Text(item.Name);
                inputHead.Css("margin-bottom", "2px").Css("vertical-align", "middle").Css("padding-top", "7px").Css("padding-right", "7px");
                input.Prepend(inputHead);
                div.Append(input);
            }
            form.Add(resource.Name, div.CreateGrid(Column.Sm9 | Column.Md10));
        }
        
        private void GetPermissionsFromResource(List<Oldmansoft.Identity.Data.PermissionData> result, Oldmansoft.Identity.Data.ResourceData resource)
        {
            foreach (var child in resource.Children)
            {
                string[] operators = Request.Form.GetValues("Operation" + child.Id.ToString());
                if (operators != null)
                {
                    foreach (var item in operators)
                    {
                        var permission = new Oldmansoft.Identity.Data.PermissionData();
                        permission.ResourceId = child.Id;
                        permission.Operator = (Operation)int.Parse(item);
                        result.Add(permission);
                    }
                }
            }
        }

        private List<Oldmansoft.Identity.Data.PermissionData> GetPermissions()
        {
            var result = new List<Oldmansoft.Identity.Data.PermissionData>();
            foreach (var item in ResourceSupport())
            {
                GetPermissionsFromResource(result, item);
            }
            return result;
        }

        [Auth(Operation.Append)]
        [Location("添加", Behave = LinkBehave.Open)]
        public ActionResult Create()
        {
            var model = new Models.RoleManageEditModel();
            var form = FormHorizontal.Create(model, Url.Location(new Func<Models.RoleManageEditModel, JsonResult>(Create)));
            foreach (var item in ResourceSupport())
            {
                SetResourceInput(form, item, null);
            }
            var panel = new Panel();
            panel.ConfigLocation();
            panel.Append(form);
            return Content(panel);
        }

        [Auth(Operation.Append)]
        [HttpPost]
        public JsonResult Create(Models.RoleManageEditModel model)
        {
            if (ModelState.ValidateFail())
            {
                return Json(DealResult.Wrong(ModelState.ValidateMessage()));
            }
            var data = model.CopyTo(new Oldmansoft.Identity.Data.RoleData());
            data.Permissions = GetPermissions();
            if (CreateIdentity().CreateRole<Resource.System>(data))
            {
                return Json(DealResult.Refresh());
            }
            else
            {
                return Json(DealResult.Wrong("已经存在 " + model.Name));
            }
        }

        [Auth(Operation.Modify)]
        [Location("修改", Behave = LinkBehave.Open)]
        public ActionResult Edit(Guid selectedId)
        {
            var data = CreateIdentity().GetRole<Resource.System>(selectedId);
            if (data == null) return HttpNotFound();

            var permissions = new Dictionary<Guid, List<string>>();
            foreach (var item in data.Permissions)
            {
                if (!permissions.ContainsKey(item.ResourceId)) permissions.Add(item.ResourceId, new List<string>());
                permissions[item.ResourceId].Add(((int)item.Operator).ToString());
            }

            var model = data.CopyTo(new Models.RoleManageEditModel());
            var form = FormHorizontal.Create(model, Url.Location(new Func<Models.RoleManageEditModel, JsonResult>(EditResult)));
            foreach (var item in ResourceSupport())
            {
                SetResourceInput(form, item, permissions);
            }
            var panel = new Panel();
            panel.ConfigLocation();
            panel.Append(form);
            return Content(panel);
        }

        [Auth(Operation.Modify)]
        public JsonResult EditResult(Models.RoleManageEditModel model)
        {
            if (ModelState.ValidateFail())
            {
                return Json(DealResult.Wrong(ModelState.ValidateMessage()));
            }
            var data = CreateIdentity().GetRole<Resource.System>(model.Id);
            if (data == null) return Json(DealResult.WrongRefresh("内容不存在"));
            model.CopyTo(data);
            data.Permissions = GetPermissions();
            CreateIdentity().ReplaceRole<Resource.System>(data);
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
                if (!identity.RemoveRole<Resource.System>(id))
                {
                    return Json(DealResult.WrongRefresh("不允许删除"));
                }
            }
            return Json(DealResult.Refresh());
        }
    }
}