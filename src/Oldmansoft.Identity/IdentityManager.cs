using Oldmansoft.ClassicDomain;
using Oldmansoft.Identity.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oldmansoft.Identity
{
    /// <summary>
    /// 身份管理员
    /// </summary>
    public class IdentityManager
    {
        private Infrastructure.IRepositoryFactory Factory { get; set; }

        /// <summary>
        /// 创建身份管理员
        /// </summary>
        /// <param name="factory"></param>
        public IdentityManager(Infrastructure.IRepositoryFactory factory)
        {
            Factory = factory;
        }

        #region 私有方法
        private bool CreateAccount<TOperateResource>(string name, string passwordSHA256Hash, Guid? memberId, int memberType, out Guid accountId)
            where TOperateResource : class, IOperateResource, new()
        {
            var repository = Factory.GetRepository<IAccountRepository>();
            accountId = Guid.Empty;

            if (repository.GetByName(name) != null) return false;

            var domain = Factory.CreateAccountObject(ResourceProvider.GetResource<TOperateResource>().Id, name, memberType);
            domain.SetPasswordHash(passwordSHA256Hash);
            if (memberId.HasValue) domain.Bind(memberId.Value, memberType);
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

        private Data.AccountData FillRoles(Domain.Account domain)
        {
            var data = domain.MapTo(new Data.AccountData());

            var roleIds = domain.GetRoleIds();
            if (roleIds.Count > 0)
            {
                var roles = Factory.GetRepository<IRoleRepository>();
                foreach (var roleId in roleIds)
                {
                    var role = roles.Get(roleId);
                    if (role == null) continue;
                    data.Roles.Add(role.MapTo(new Data.RoleData()));
                }
            }

            return data;
        }

        /// <summary>
        /// 获取同样分区的角色列表
        /// </summary>
        /// <param name="roleIds"></param>
        /// <param name="roleRepository"></param>
        /// <param name="partitionResourceId"></param>
        /// <param name="list"></param>
        private void TakeSamePartitionRole(Guid[] roleIds, IRoleRepository roleRepository, Guid partitionResourceId, List<Guid> list)
        {
            foreach (var roleId in roleIds)
            {
                var role = roleRepository.Get(roleId);
                if (!role.SamePartition(partitionResourceId)) continue;
                list.Add(roleId);
            }
        }

        /// <summary>
        /// 获取其它分区的角色列表
        /// </summary>
        /// <param name="roleRepository"></param>
        /// <param name="partitionResourceId"></param>
        /// <param name="domain"></param>
        /// <param name="list"></param>
        private void TakeOtherPartitionRole(IRoleRepository roleRepository, Guid partitionResourceId, Domain.Account domain, List<Guid> list)
        {
            foreach (var roleId in domain.GetRoleIds())
            {
                var role = roleRepository.Get(roleId);
                if (role.SamePartition(partitionResourceId)) continue;
                list.Add(roleId);
            }
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
            return ResourceProvider.GetResource<TOperateResource>().MapTo(new Data.ResourceData());
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
            
            var allOperators = new Operation[]
            {
                Operation.List,
                Operation.Execute,
                Operation.View,
                Operation.Append,
                Operation.Modify,
                Operation.Remove
            };
            var permissions = new List<Data.PermissionData>();
            foreach(var resource in GetResource<TOperateResource>().Children)
            {
                foreach(var item in allOperators)
                {
                    var permission = new Data.PermissionData
                    {
                        ResourceId = resource.Id,
                        Operator = item
                    };
                    permissions.Add(permission);
                }
            }

            if (!CreateAccount<TOperateResource>(name, passwordSHA256Hash, null, DataDefinition.MemberType.System, out Guid accountId)) return false;
            if (!CreateRole<TOperateResource>(roleName, roleDescription, permissions, out Guid roleId)) return false;

            AccountSetRole<TOperateResource>(accountId, new Guid[] { roleId });
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
            return CreateAccount<TOperateResource>(name, passwordSHA256Hash, null, DataDefinition.MemberType.Unknown, out _);
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
            var repository = Factory.GetRepository<IAccountRepository>();
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
            var repository = Factory.GetRepository<IAccountRepository>();
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
            var domain = Factory.GetRepository<IAccountRepository>().Get(accountId);
            if (domain == null) return null;

            return FillRoles(domain);
        }

        /// <summary>
        /// 获取帐号
        /// </summary>
        /// <param name="name">用户名称</param>
        /// <returns></returns>
        public Data.AccountData GetAccountByName(string name)
        {
            var domain = Factory.GetRepository<IAccountRepository>().GetByName(name);
            if (domain == null) return null;

            return FillRoles(domain);
        }

        /// <summary>
        /// 获取帐号
        /// </summary>
        /// <param name="memberId">会员序号</param>
        /// <returns></returns>
        public Data.AccountData GetAccountByMemberId(Guid memberId)
        {
            var domain = Factory.GetRepository<IAccountRepository>().GetByMemberId(memberId);
            if (domain == null) return null;

            return FillRoles(domain);
        }

        /// <summary>
        /// 获取帐号
        /// </summary>
        /// <param name="memberId">会员序号</param>
        /// <returns></returns>
        public IList<Data.AccountData> GetAccountsByMemberId(Guid memberId)
        {
            var list = Factory.GetRepository<IAccountRepository>().ListByMemberId(memberId);

            var result = new List<Data.AccountData>();
            foreach (var item in list)
            {
                result.Add(FillRoles(item));
            }
            return result;
        }

        /// <summary>
        /// 获取帐号
        /// </summary>
        /// <param name="name">帐号</param>
        /// <param name="passwordSHA256Hash">密码十六进制散列</param>
        /// <returns></returns>
        public Data.AccountData GetAccount(string name, string passwordSHA256Hash)
        {
            var repository = Factory.GetRepository<IAccountRepository>();
            var domain = repository.GetByName(name);
            if (domain == null) return null;
            if (!domain.CheckPasswordHash(passwordSHA256Hash)) return null;

            return FillRoles(domain);
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
            var repository = Factory.GetRepository<IAccountRepository>();
            var domain = repository.GetByName(name);
            if (domain == null) return null;
            if (!domain.CheckPassword(doubleSHA256Hash, seed)) return null;

            return FillRoles(domain);
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
            var list = Factory.GetRepository<IAccountRepository>()
                .Paging(key)
                .Size(size)
                .ToList(index, out totalCount);
            var result = new List<Data.AccountData>();
            foreach(var item in list)
            {
                result.Add(FillRoles(item));
            }
            return result;
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
            var list = Factory.GetRepository<IAccountRepository>()
                .Paging(partitionResourceId, key)
                .Size(size)
                .ToList(index, out totalCount);
            var result = new List<Data.AccountData>();
            foreach (var item in list)
            {
                result.Add(FillRoles(item));
            }
            return result;
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
            var list = Factory.GetRepository<IAccountRepository>()
                .Paging(index, size, out totalCount, roleId, key);

            var result = new List<Data.AccountData>();
            foreach (var item in list)
            {
                result.Add(FillRoles(item));
            }
            return result;
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

            var repository = Factory.GetRepository<IAccountRepository>();

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

            var repository = Factory.GetRepository<IAccountRepository>();

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
            var repository = Factory.GetRepository<IAccountRepository>();
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
            var repository = Factory.GetRepository<IAccountRepository>();
            var roleRepository = Factory.GetRepository<IRoleRepository>();
            var partitionResourceId = ResourceProvider.GetResource<TOperateResource>().Id;

            var domain = repository.Get(accountId);
            if (domain == null) return false;

            var list = new List<Guid>();
            TakeOtherPartitionRole(roleRepository, partitionResourceId, domain, list);
            TakeSamePartitionRole(roleIds, roleRepository, partitionResourceId, list);
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
            var domain = Factory.GetRepository<IRoleRepository>().Get(id);
            if (domain == null) return null;
            if (!domain.SamePartition(ResourceProvider.GetResource<TOperateResource>().Id)) return null;
            return domain.MapTo(new Data.RoleData());
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
            var list = Factory.GetRepository<IRoleRepository>().ListByPartitionResourceId(partitionResourceId);
            return list.MapTo(new List<Data.RoleData>());
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
            var repository = Factory.GetRepository<IRoleRepository>();
            var list = repository.PagingByPartitionResourceId(partitionResourceId, key)
                .Size(size)
                .ToList(index, out totalCount);
            return list.MapTo(new List<Data.RoleData>());
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <typeparam name="TOperateResource">操作资源</typeparam>
        /// <param name="name">名称</param>
        /// <param name="description">注释</param>
        /// <param name="permissions">许可列表</param>
        /// <returns></returns>
        public bool CreateRole<TOperateResource>(string name, string description, List<Data.PermissionData> permissions)
            where TOperateResource : class, IOperateResource, new()
        {
            return CreateRole<TOperateResource>(name, description, permissions, out _);
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <typeparam name="TOperateResource">操作资源</typeparam>
        /// <param name="name">名称</param>
        /// <param name="description">注释</param>
        /// <param name="permissions">许可列表</param>
        /// <param name="roleId">角色序号</param>
        /// <returns></returns>
        public bool CreateRole<TOperateResource>(string name, string description, List<Data.PermissionData> permissions, out Guid roleId)
            where TOperateResource : class, IOperateResource, new()
        {
            var partitionResourceId = ResourceProvider.GetResource<TOperateResource>().Id;
            var repository = Factory.GetRepository<IRoleRepository>();
            var domain = Factory.CreateRoleObject(partitionResourceId, name, description, permissions.MapTo(new List<Domain.Permission>()));
            repository.Add(domain);
            try
            {
                Factory.GetUnitOfWork().Commit();
                roleId = domain.Id;
                return true;
            }
            catch (UniqueException)
            {
                roleId = Guid.Empty;
                return false;
            }
        }

        /// <summary>
        /// 修改角色
        /// </summary>
        /// <typeparam name="TOperateResource">操作资源</typeparam>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public bool ReplaceRole<TOperateResource>(Guid id, string name, string description, IList<Data.PermissionData> permissions)
            where TOperateResource : class, IOperateResource, new()
        {
            var repository = Factory.GetRepository<IRoleRepository>();

            var domain = repository.Get(id);
            if (domain == null) return false;
            if (!domain.SamePartition(ResourceProvider.GetResource<TOperateResource>().Id)) return false;

            domain.Change(name, description, permissions.MapTo(new List<Domain.Permission>()));
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
            var repository = Factory.GetRepository<IRoleRepository>();
            
            var domain = repository.Get(roleId);
            if (domain == null) return false;
            if (domain.HasAccountSetIt(Factory.GetRepository<IAccountRepository>())) return false;
            if (!domain.SamePartition(ResourceProvider.GetResource<TOperateResource>().Id)) return false;

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
            return Factory.GetRepository<IAccountRepository>().ContainsRoleId(roleId);
        }
        #endregion
    }
}
