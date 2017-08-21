# Identity
MVC.net 身份权限管理系统

### 示例

示例项目 WebManApplication 和它的支持库 WebManApplication.WebDefinition

#### 数据库
请安装 Mongodb 数据库，并修改 web.config 指向相应的地址
```xml
<connectionStrings>
    <add name="Oldmansoft.Identity.Driver.Mongo.Mapping" connectionString="Data Source=localhost;Initial Catalog=WebManApplication;" />
</connectionStrings>
```

#### 初始化
第一次运行需要初始化数据，请进入 /Home/Init 初始化您的超级管理员帐号。