using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oldmansoft.ClassicDomain;
using Oldmansoft.ClassicDomain.Util;

namespace Oldmansoft.Identity
{
    /// <summary>
    /// 身份管理员
    /// </summary>
    public class IdentityManager
    {
        private IRepositoryFactory Factory { get; set; }

        /// <summary>
        /// 创建身份管理员
        /// </summary>
        /// <param name="factory"></param>
        public IdentityManager(IRepositoryFactory factory)
        {
            Factory = factory;
        }

        #region 私有方法
        private bool CreateAccount<TOperateResource>(string name, string passwordSHA256Hash, Guid? memberId, int memberType, out Guid accountId)
            where TOperateResource : class, IOperateResource, new()
        {
            var repository = Factory.CreateAccountRepository();
            accountId = Guid.Empty;

            if (repository.GetByName(name) != null) return false;

            var domain = Factory.CreateAccountObject();
            domain.PartitionResourceId = ResourceProvider.GetResource<TOperateResource>().Id;
            domain.Name = name;
            domain.SetPasswordHash(passwordSHA256Hash);
            domain.MemberId = memberId;
            domain.MemberType = memberType;
            repository.Add(domain);

            try
            {
                Factory.GetUnitOfWork().Commit();
                accountId = domain.Id;
            }
            catch (UniqueException)
            {
                return false;
            }
            return true;
        }

        private Data.AccountData FillRoles(IRepositoryFactory factory, Domain.Account domain)
        {
            var data = domain.CopyTo(new Data.AccountData());

            var roleIds = domain.GetRoleIds();
            if (roleIds.Count > 0)
            {
                var roles = Factory.CreateRoleRepository();
                foreach (var roleId in roleIds)
                {
                    var role = roles.Get(roleId);
                    if (role == null) continue;
                    data.Roles.Add(role.CopyTo(new Data.RoleData()));
                }
            }

            return data;
        }
        #endregion

        #region 资源方法
        /// <summary>
        /// 获取资源内容
        /// </summary>
        /// <typeparam name="TOperateResource"></typeparam>
        /// <returns></returns>
        public Data.ResourceData GetResource<TOperateResource>()
            where TOperateResource : class, IOperateResource, new()
        {
            return ResourceProvider.GetResource<TOperateResource>().CopyTo(new Data.ResourceData());
        }
        #endregion

        #region 帐号方法
        /// <summary>
        /// 初始化超级管理员帐号
        /// </summary>
        /// <typeparam name="TOperateResource">操作资源</typeparam>
        /// <param name="name">帐号</param>
        /// <param name="passwordSHA256Hash">密码十六进制散列</param>
        /// <param name="roleName">角色名称</param>
        /// <param name="roleDescription">角色描述</param>
        /// <returns></returns>
        public bool InitAdminAccount<TOperateResource>(string name, string passwordSHA256Hash, string roleName = "Admin", string roleDescription = "Administrator")
            where TOperateResource : class, IOperateResource, new()
        {
            var roles = GetRoles<TOperateResource>();
            if (roles.Count > 0) return false;

            var adminResource = GetResource<TOperateResource>();

            var role = new Data.RoleData();
            role.Name = roleName;
            role.Description = roleDescription;

            var allOperators = new Operation[]
            {
                Operation.List,
                Operation.Execute,
                Operation.View,
                Operation.Append,
                Operation.Modify,
                Operation.Remove
            };
            role.Permissions = new List<Data.PermissionData>();
            foreach(var resource in adminResource.Children)
            {
                foreach(var item in allOperators)
                {
                    var permission = new Data.PermissionData();
                    permission.ResourceId = resource.Id;
                    permission.Operator = item;
                    role.Permissions.Add(permission);
                }
            }

            Guid accountId;
            if (!CreateAccount<TOperateResource>(name, passwordSHA256Hash, null, DataDefinition.MemberType.System, out accountId)) return false;
            if (!CreateRole<TOperateResource>(role)) return false;

            AccountSetRole<TOperateResource>(accountId, new Guid[] { role.Id });
            return true;
        }

        /// <summary>
        /// 创建帐号
        /// </summary>
        /// <typeparam name="TOperateResource">操作资源</typeparam>
        /// <param name="name">名称</param>
        /// <param name="passwordSHA256Hash">密码十六进制散列</param>
        /// <returns></returns>
        public bool CreateAccount<TOperateResource>(string name, string passwordSHA256Hash)
            where TOperateResource : class, IOperateResource, new()
        {
            Guid accountId;
            return CreateAccount<TOperateResource>(name, passwordSHA256Hash, null, DataDefinition.MemberType.Unknown, out accountId);
        }

        /// <summary>
        /// 创建帐号
        /// </summary>
        /// <typeparam name="TOperateResource">操作资源</typeparam>
        /// <param name="name">名称</param>
        /// <param name="passwordSHA256Hash">密码十六进制散列</param>
        /// <param name="accountId">返回帐号序号</param>
        /// <returns></returns>
        public bool CreateAccount<TOperateResource>(string name, string passwordSHA256Hash, out Guid accountId)
            where TOperateResource : class, IOperateResource, new()
        {
            return CreateAccount<TOperateResource>(name, passwordSHA256Hash, null, DataDefinition.MemberType.Unknown, out accountId);
        }

        /// <summary>
        /// 创建帐号并绑定会员序号
        /// </summary>
        /// <typeparam name="TOperateResource">操作资源</typeparam>
        /// <param name="name">名称</param>
        /// <param name="passwordSHA256Hash">密码十六进制散列</param>
        /// <param name="memberId">会员序号</param>
        /// <param name="memberType">会员类型</param>
        /// <param name="accountId">返回帐号序号</param>
        /// <returns></returns>
        public bool CreateAccount<TOperateResource>(string name, string passwordSHA256Hash, Guid memberId, uint memberType, out Guid accountId)
            where TOperateResource : class, IOperateResource, new()
        {
            return CreateAccount<TOperateResource>(name, passwordSHA256Hash, memberId, (int)memberType, out accountId);
        }

        /// <summary>
        /// 帐号绑定会员
        /// </summary>
        /// <param name="accountId">帐号序号</param>
        /// <param name="memberId">会员序号</param>
        /// <param name="memberType">会员类型</param>
        /// <returns></returns>
        public bool AccountBindMember(Guid accountId, Guid memberId, uint memberType)
        {
            var repository = Factory.CreateAccountRepository();
            var domain = repository.Get(accountId);
            if (domain == null) return false;

            if (!domain.Bind(memberId, (int)memberType)) return false;

            repository.Replace(domain);
            Factory.GetUnitOfWork().Commit();
            return true;
        }

        /// <summary>
        /// 帐号移除会员
        /// </summary>
        /// <param name="memberId">会员序号</param>
        /// <returns></returns>
        public bool AccountRemoveMember(Guid memberId)
        {
            var repository = Factory.CreateAccountRepository();
            var domain = repository.GetByMemberId(memberId);
            if (domain == null) return false;

            if (!domain.Unbind(memberId)) return false;

            repository.Replace(domain);
            Factory.GetUnitOfWork().Commit();
            return true;
        }

        /// <summary>
        /// 获取帐号
        /// </summary>
        /// <param name="accountId">帐号序号</param>
        /// <returns></returns>
        public Data.AccountData GetAccount(Guid accountId)
        {
            var domain = Factory.CreateAccountRepository().Get(accountId);
            if (domain == null) return null;

            return FillRoles(Factory, domain);
        }

        /// <summary>
        /// 获取帐号
        /// </summary>
        /// <param name="name">用户名称</param>
        /// <returns></returns>
        public Data.AccountData GetAccountByName(string name)
        {
            var domain = Factory.CreateAccountRepository().GetByName(name);
            if (domain == null) return null;

            return FillRoles(Factory, domain);
        }

        /// <summary>
        /// 获取帐号
        /// </summary>
        /// <param name="memberId">会员序号</param>
        /// <returns></returns>
        public Data.AccountData GetAccountByMemberId(Guid memberId)
        {
            var domain = Factory.CreateAccountRepository().GetByMemberId(memberId);
            if (domain == null) return null;

            return FillRoles(Factory, domain);
        }

        /// <summary>
        /// 获取帐号
        /// </summary>
        /// <param name="name">帐号</param>
        /// <param name="passwordSHA256Hash">密码十六进制散列</param>
        /// <returns></returns>
        public Data.AccountData GetAccount(string name, string passwordSHA256Hash)
        {
            var repository = Factory.CreateAccountRepository();
            var domain = repository.GetByName(name);
            if (domain == null) return null;
            if (!domain.CheckPasswordHash(passwordSHA256Hash)) return null;

            return FillRoles(Factory, domain);
        }

        /// <summary>
        /// 获取帐号
        /// </summary>
        /// <param name="name">帐号</param>
        /// <param name="doubleSHA256Hash">密码十六进制双散列</param>
        /// <param name="seed">哈希盐</param>
        /// <returns></returns>
        public Data.AccountData GetAccount(string name, string doubleSHA256Hash, string seed)
        {
            var repository = Factory.CreateAccountRepository();
            var domain = repository.GetByName(name);
            if (domain == null) return null;
            if (!domain.CheckPassword(doubleSHA256Hash, seed)) return null;

            return FillRoles(Factory, domain);
        }

        /// <summary>
        /// 获取帐号分页列表
        /// </summary>
        /// <param name="index">页码</param>
        /// <param name="size">页大小</param>
        /// <param name="totalCount">总记录数</param>
        /// <param name="key">查询内容</param>
        /// <returns></returns>
        public IList<Data.AccountData> GetAccounts(int index, int size, out int totalCount, string key = null)
        {
            return Factory.CreateAccountRepository()
                .Paging(key)
                .Size(size)
                .ToList(index, out totalCount)
                .CopyTo(new List<Data.AccountData>());
        }

        /// <summary>
        /// 获取资源区的帐号分页列表
        /// </summary>
        /// <typeparam name="TOperateResource">操作资源</typeparam>
        /// <param name="index">页码</param>
        /// <param name="size">页大小</param>
        /// <param name="totalCount">总记录数</param>
        /// <param name="key">查询内容</param>
        /// <returns></returns>
        public IList<Data.AccountData> GetAccounts<TOperateResource>(int index, int size, out int totalCount, string key = null)
            where TOperateResource : class, IOperateResource, new()
        {
            var partitionResourceId = ResourceProvider.GetResource<TOperateResource>().Id;
            return Factory.CreateAccountRepository()
                .Paging(partitionResourceId, key)
                .Size(size)
                .ToList(index, out totalCount)
                .CopyTo(new List<Data.AccountData>());
        }

        /// <summary>
        /// 获取帐号分页列表
        /// </summary>
        /// <param name="index">页码</param>
        /// <param name="size">页大小</param>
        /// <param name="totalCount">总记录数</param>
        /// <param name="roleId">角色序号</param>
        /// <param name="key">查询内容</param>
        /// <returns></returns>
        public IList<Data.AccountData> GetAccounts(int index, int size, out int totalCount, Guid roleId, string key = null)
        {
            return Factory.CreateAccountRepository()
                .Paging(index, size, out totalCount, roleId, key)
                .CopyTo(new List<Data.AccountData>());
        }
        
        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="accountId">帐号序号</param>
        /// <param name="passwordSHA256Hash">密码十六进制散列</param>
        /// <returns></returns>
        public bool SetPassword(Guid accountId, string passwordSHA256Hash)
        {
            if (string.IsNullOrEmpty(passwordSHA256Hash)) throw new ArgumentNullException("passwordHash");

            var repository = Factory.CreateAccountRepository();

            var domain = repository.Get(accountId);
            if (domain == null) return false;

            domain.SetPasswordHash(passwordSHA256Hash);
            repository.Replace(domain);
            Factory.GetUnitOfWork().Commit();
            return true;
        }

        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="accountId">帐号序号</param>
        /// <param name="passwordSHA256Hash">密码十六进制散列</param>
        /// <param name="oldPasswordSHA256Hash">旧密码十六进制散列</param>
        /// <returns></returns>
        public bool SetPassword(Guid accountId, string passwordSHA256Hash, string oldPasswordSHA256Hash)
        {
            if (string.IsNullOrEmpty(passwordSHA256Hash)) throw new ArgumentNullException("passwordHash");

            var repository = Factory.CreateAccountRepository();

            var domain = repository.Get(accountId);
            if (domain == null) return false;
            if (!domain.CheckPasswordHash(oldPasswordSHA256Hash)) return false;

            domain.SetPasswordHash(passwordSHA256Hash);
            repository.Replace(domain);
            Factory.GetUnitOfWork().Commit();
            return true;
        }

        /// <summary>
        /// 移除帐号
        /// </summary>
        /// <param name="accountId">帐号序号</param>
        /// <returns></returns>
        public bool RemoveAccount(Guid accountId)
        {
            var repository = Factory.CreateAccountRepository();
            var domain = repository.Get(accountId);
            if (domain == null) return false;
            if (domain.IsBound()) return false;
            if (domain.IsSystem()) return false;

            repository.Remove(domain);
            Factory.GetUnitOfWork().Commit();
            return true;
        }

        /// <summary>
        /// 帐号设置角色
        /// </summary>
        /// <typeparam name="TOperateResource">操作资源</typeparam>
        /// <param name="accountId">帐号序号</param>
        /// <param name="roleIds">角色序号组</param>
        /// <returns></returns>
        public bool AccountSetRole<TOperateResource>(Guid accountId, Guid[] roleIds)
            where TOperateResource : class, IOperateResource, new()
        {
            var repository = Factory.CreateAccountRepository();
            var roleRepository = Factory.CreateRoleRepository();

            var partitionResourceId = ResourceProvider.GetResource<TOperateResource>().Id;

            var domain = repository.Get(accountId);
            if (domain == null) return false;

            var list = new List<Guid>();
            foreach (var roleId in domain.GetRoleIds())
            {
                var role = roleRepository.Get(roleId);
                if (role.PartitionResourceId == partitionResourceId) continue;
                list.Add(roleId);
            }
            foreach (var roleId in roleIds)
            {
                var role = roleRepository.Get(roleId);
                if (role.PartitionResourceId != partitionResourceId) continue;
                list.Add(roleId);
            }

            domain.SetRoleIds(list.ToArray());
            
            repository.Replace(domain);
            Factory.GetUnitOfWork().Commit();
            return true;
        }
        #endregion

        #region 角色方法
        /// <summary>
        /// 获取角色
        /// </summary>
        /// <typeparam name="TOperateResource">操作资源</typeparam>
        /// <param name="id">角色序号</param>
        /// <returns></returns>
        public Data.RoleData GetRole<TOperateResource>(Guid id)
            where TOperateResource : class, IOperateResource, new()
        {
            var domain = Factory.CreateRoleRepository().Get(id);
            if (domain == null) return null;
            if (domain.PartitionResourceId != ResourceProvider.GetResource<TOperateResource>().Id) return null;
            return domain.CopyTo(new Data.RoleData());
        }

        /// <summary>
        /// 获取角色列表
        /// </summary>
        /// <typeparam name="TOperateResource">操作资源</typeparam>
        /// <returns></returns>
        public IList<Data.RoleData> GetRoles<TOperateResource>()
            where TOperateResource : class, IOperateResource, new()
        {
            var partitionResourceId = ResourceProvider.GetResource<TOperateResource>().Id;
            var list = Factory.CreateRoleRepository().ListByPartitionResourceId(partitionResourceId);
            return list.CopyTo(new List<Data.RoleData>());
        }

        /// <summary>
        /// 获取角色分页列表
        /// </summary>
        /// <typeparam name="TOperateResource">操作资源</typeparam>
        /// <param name="index">页码</param>
        /// <param name="size">页大小</param>
        /// <param name="totalCount">总记录数</param>
        /// <param name="key">查询内容</param>
        /// <returns></returns>
        public IList<Data.RoleData> GetRoles<TOperateResource>(int index, int size, out int totalCount, string key = null)
            where TOperateResource : class, IOperateResource, new()
        {
            var partitionResourceId = ResourceProvider.GetResource<TOperateResource>().Id;
            var repository = Factory.CreateRoleRepository();
            var list = repository.PagingByPartitionResourceId(partitionResourceId, key)
                .Size(size)
                .ToList(index, out totalCount);
            return list.CopyTo(new List<Data.RoleData>());
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <typeparam name="TOperateResource">操作资源</typeparam>
        /// <param name="data">角色数据</param>
        /// <returns></returns>
        public bool CreateRole<TOperateResource>(Data.RoleData data)
            where TOperateResource : class, IOperateResource, new()
        {
            var partitionResourceId = ResourceProvider.GetResource<TOperateResource>().Id;
            var repository = Factory.CreateRoleRepository();
            var domain = data.CopyTo(Factory.CreateRoleObject());
            domain.PartitionResourceId = partitionResourceId;
            repository.Add(domain);
            try
            {
                Factory.GetUnitOfWork().Commit();
                data.Id = domain.Id;
                return true;
            }
            catch (UniqueException)
            {
                return false;
            }
        }

        /// <summary>
        /// 修改角色
        /// </summary>
        /// <typeparam name="TOperateResource">操作资源</typeparam>
        /// <param name="data">角色数据</param>
        /// <returns></returns>
        public bool ReplaceRole<TOperateResource>(Data.RoleData data)
            where TOperateResource : class, IOperateResource, new()
        {
            var partitionResourceId = ResourceProvider.GetResource<TOperateResource>().Id;
            var repository = Factory.CreateRoleRepository();

            var domain = repository.Get(data.Id);
            if (domain == null) return false;
            if (domain.PartitionResourceId != partitionResourceId) return false;

            data.CopyTo(domain);
            repository.Replace(domain);
            Factory.GetUnitOfWork().Commit();
            return true;
        }

        /// <summary>
        /// 移除角色
        /// </summary>
        /// <typeparam name="TOperateResource">操作资源</typeparam>
        /// <param name="roleId">角色序号</param>
        /// <returns></returns>
        public bool RemoveRole<TOperateResource>(Guid roleId)
            where TOperateResource : class, IOperateResource, new()
        {
            var partitionResourceId = ResourceProvider.GetResource<TOperateResource>().Id;
            var repository = Factory.CreateRoleRepository();
            
            var domain = repository.Get(roleId);
            if (domain == null) return false;
            if (domain.HasAccountSetIt(Factory.CreateAccountRepository())) return false;
            if (domain.PartitionResourceId != partitionResourceId) return false;

            repository.Remove(domain);
            Factory.GetUnitOfWork().Commit();
            return true;
        }

        /// <summary>
        /// 角色拥有帐号
        /// </summary>
        /// <param name="roleId">角色序号</param>
        /// <returns></returns>
        public bool RoleHasAccount(Guid roleId)
        {
            return Factory.CreateAccountRepository().ContainsRoleId(roleId);
        }
        #endregion
    }
}
