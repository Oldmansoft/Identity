using Oldmansoft.ClassicDomain;
using Oldmansoft.Html;
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
            var result = new List<Oldmansoft.Identity.Data.ResourceData>
            {
                CreateIdentity().GetResource<Resource.System>(),
                CreateIdentity().GetResource<Resource.Business>()
            };
            return result;
        }

        [Auth(Operation.List)]
        [Location("系统角色", Icon = FontAwesome.User)]
        public ActionResult Index(string key)
        {
            var table = DataTable.Define<Models.RoleManageListModel>(o => o.Id).Create(Url.Location(IndexDataSource).Set("key", key));
            table.AddActionTable(Url.Location(Create), Account);
            table.AddActionItem(Url.Location<SystemRoleAccountController>(o => o.Index));
            table.AddActionItem(Url.Location(Edit), Account);
            table.AddActionItem(Url.Location(Delete), Account).Confirm("是否删除角色").OnClientCondition(ItemActionClient.Disable, "data.HasAccount == '是'");

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
            var identityManager = CreateIdentity();
            var source = identityManager.GetRoles<Resource.System>(request.PageIndex, request.PageSize, out int totalCount, key);
            var list = new List<Models.RoleManageListMoreModel>();
            foreach (var item in source)
            {
                var model = item.MapTo(new Models.RoleManageListMoreModel());
                model.HasAccount = identityManager.RoleHasAccount(item.Id);
                list.Add(model);
            }
            return Json(DataTable.Source(list, request, totalCount));
        }

        private void SetResourceInput(FormHorizontal form, Oldmansoft.Identity.Data.ResourceData resource, Dictionary<Guid, List<string>> permissions)
        {
            var div = new HtmlElement(HtmlTag.Div);
            foreach (var item in resource.Children)
            {
                List<string> value = null;
                if (permissions != null) permissions.TryGetValue(item.Id, out value);
                if (value == null) value = new List<string>();

                var input = CreateCheckBoxList("Operation" + item.Id, value);
                input.Prepend(new HtmlElement(HtmlTag.Label).Text(item.Name).AddClass("checkbox-list-head"));
                div.Append(input);
            }
            form.Add(resource.Name, div.CreateGrid(Column.Sm9 | Column.Md10));
        }

        private static HtmlElement CreateCheckBoxList(string name, List<string> value)
        {
            var result = new HtmlElement(HtmlTag.Div);
            foreach (var option in GetDataSourceList())
            {
                var lable = new HtmlElement(HtmlTag.Label);
                result.Append(lable);
                lable.AddClass("checkbox-inline");

                var input = new HtmlElement(HtmlTag.Input);
                lable.Append(input);
                input.Attribute(HtmlAttribute.Type, "checkbox");
                input.Attribute(HtmlAttribute.Name, name);
                input.Attribute(HtmlAttribute.Value, option.Value);
                if (value.Contains(option.Value))
                {
                    input.Attribute(HtmlAttribute.Checked, "checked");
                }
                lable.Append(new HtmlRaw(option.Text.HtmlEncode()));
            }
            return result;
        }

        private static List<ListDataItem> GetDataSourceList()
        {
            var cn = new Dictionary<string, string>
            {
                { "List", "列表" },
                { "Execute", "执行" },
                { "View", "查看" },
                { "Append", "添加" },
                { "Modify", "修改" },
                { "Remove", "移除" }
            };

            var list = new List<ListDataItem>();
            foreach (var item in Enum.GetValues(typeof(Operation)))
            {
                list.Add(new ListDataItem(cn[Enum.GetName(typeof(Operation), item)], ((int)item).ToString()));
            }

            return list;
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
                        var permission = new Oldmansoft.Identity.Data.PermissionData
                        {
                            ResourceId = child.Id,
                            Operator = (Operation)int.Parse(item)
                        };
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
            if (CreateIdentity().CreateRole<Resource.System>(model.Name, model.Description, GetPermissions()))
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

            var model = data.MapTo(new Models.RoleManageEditModel());
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

            CreateIdentity().ReplaceRole<Resource.System>(model.Id, model.Name, model.Description, GetPermissions());
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