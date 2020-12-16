# Dapper

基于[Dapper](https://github.com/StackExchange/Dapper)的、轻量级的、高性能的、简单的、灵活的ORM框架

1. 高性能（与Dapper一致），以热启动后计算（第一次启动有缓存过程）
2. 像EF一样使用简单，也可像Dapper一样灵活使用原生SQL

## 准备工作

### 首先定义一个Poco类

```csharp
//表示文章表里的一条记录
public class SYS_USER
{
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CREATE_TIME { get; set; }
    /// <summary>
    /// 用户代码
    /// </summary>
    [Column(IsPrimaryKey = true)]
    public string USER_CODE { get; set; }
    /// <summary>
    /// 用户名称
    /// </summary>
    public string USER_NAME { get; set; }
}
```

### 创建DbContext

```csharp
class MesDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseConnectionString("连接字符串");
		//使用SQL Server数据库
        optionsBuilder.UseSqlAdapter(new SqlServerAdapter(SqlClientFactory.Instance));
    }
    
    public Table<SYS_USER> SYS_USER { get; set; }
    public Table<SYS_MENU> SYS_MENU { get; set; }
}
```

## 使用示例

### 插入数据

```csharp
var masterDb = new MesDbContext();

//插入一个对象
var a = new SYS_USER 
{
	CREATE_TIME = DateTime.Now,
	USER_CODE = "A1111110"，
	USER_NAME = "harris.zhang"
};
masterDb.SYS_USER.Insert(a);

List<SYS_USER> list = new List<SYS_USER>{a,a}
//插入了多条记录
masterDb.SYS_USER.InsertBatch(list);

//也可以显式指定表名
masterDb.SYS_USER.Insert(a, "SYS_USER");

//原生SQL插入
this.Execute("insert SYS_USER(CREATE_TIME, USER_CODE, USER_NAME) values ($CREATE_TIME, $USER_CODE, $USER_NAME)", a);

//插入了2条记录
this.Execute("insert SYS_USER(CREATE_TIME, USER_CODE, USER_NAME) values ($CREATE_TIME, $USER_CODE, $USER_NAME)", a, a);

//插入了2条记录
this.Execute("insert SYS_USER(CREATE_TIME, USER_CODE, USER_NAME) values ($CREATE_TIME, $USER_CODE, $USER_NAME)", new Article[] { a, a });

//也可以直接写参数值
this.Execute("insert SYS_USER(CREATE_TIME, USER_CODE, USER_NAME) values ($CREATE_TIME, $USER_CODE, $USER_NAME)", DateTime.Now, "A1111110", "harris.zhang");
```

### 更新数据

```csharp
var masterDb = new MesDbContext();

//先查出来准备更新
var user = masterDb.SYS_USER.GetById(new SYS_USER { USER_CODE = "A11110" });

//更新除主键外的所有列
user.USER_NAME = "HARRIS.ZHANG2";
masterDb.SYS_USER.Update(user);

//仅更新指定列，指定表列名
user.USER_NAME = "HARRIS.ZHANG2";
masterDb.SYS_USER.Update(user,new string[]{"USER_NAME"});

```

### 保存数据

```csharp
var masterDb = new MesDbContext();

var a = new SYS_USER 
{
	CREATE_TIME = DateTime.Now,
	USER_CODE = "A1111110"，
	USER_NAME = "harris.zhang"
};

//如果记录存在则更新，不存在则插入
masterDb.Save(a);

//保存并指定列名
masterDb.Save(a, new string[] { "Title" });
```

### 删除数据

```csharp
var masterDb = new MesDbContext();

var user = masterDb.SYS_USER.GetById(new SYS_USER { USER_CODE = "A11110" });

//删除实体记录
masterDb.SYS_USER.Delete(article);

//删除实体记录，显式指定主键名
masterDb.SYS_USER.Delete(article, new string[] {"USER_CODE"});
```

### 查询数据（单表）

```csharp
var masterDb = new MesDbContext();

//查询SYS_USER表所有记录
var list = masterDb.SYS_USER.GetList();

//指定条件查询，直接写参数值
var articles = masterDb.SYS_USER.GetLis(new SYS_USER{USER_CODE = "A11110"}, new string[] {"USER_CODE"});

//查询单条记录
masterDb.SYS_USER.GetById(new SYS_USER{USER_CODE = "A11110"});

//查询分页的结果
public TablePagerList GetList(GridPager pager, string userId, string userName)
{
    string where = "";
    var args = new List<KeyValuePair<string, object>>();

    if (!ValidateHelper.IsNullOrEmpty(userId))
    {
        where += " AND USER_CODE = $USER_CODE";
        args.Add(new KeyValuePair<string, object>("USER_CODE", userId));
    }
    if (!ValidateHelper.IsNullOrEmpty(userName))
    {
        where += " AND USER_NAME = $USER_NAME";
        args.Add(new KeyValuePair<string, object>("USER_NAME", userName));
    }

    TablePagerList list = new TablePagerList();
    list.Items = _db.SYS_USER.Paged(out long total, pager.page, pager.rows, pager.sort, where, args);
    list.totalRows = total;
    return list;
}

```

### 查询数据（立即执行）

延迟查询使用Query，与Fetch不同的是Query返回的结果只有在使用时才会真正查询数据库

```csharp
var masterDb = new MesDbContext();

//正常sql查询
var user = masterDb.Fetch<SYS_USER>("select * from SYS_USER where USER_CODE=$USER_CODE", new { USER_CODE = "A11110" });

//查询一条数据
var user = masterDb.FirstOrDefault<SYS_USER>("select * from SYS_USER where USER_CODE=$USER_CODE", new { USER_CODE = "A11110" });

//查询单列
var count = masterDb.ExecuteScalar<long>("select count(*) from SYS_USER");

//查询分页的结果（第1页，每页20条）
Paged<Article> paged = masterDb.Paged<SYS_USER>(1, 20, "select * from SYS_USER where USER_CODE=$USER_CODE", new { USER_CODE = "A11110" });
```

### 

### 查询数据（延迟执行）

延迟查询使用Query，与Fetch不同的是Query返回的结果只有在使用时才会真正查询数据库

```csharp
var masterDb = new MesDbContext();

//延迟查询
var user = masterDb.Query<SYS_USER>("select * from SYS_USER where USER_CODE=$USER_CODE", new { USER_CODE = "A11110" });
```

### 动态查询条件

```csharp
var masterDb = new MesDbContext();

var roleId = "此变量来自用户输入";

StringBuilder sql = new StringBuilder();
sql.Append("SELECT USER_CODE, USER_NAME, USER_DESC");
sql.Append("  FROM SYS_USER ");
sql.Append(" WHERE IS_ENABLE = 1 ");
sql.Append("   AND USER_CODE IN (SELECT USER_CODE FROM SYS_USER_ROLE WHERE ROLE_ID = $ROLE_ID)");
List<SYS_USER> list = masterDb.Fetch<SYS_USER>(sql.ToString(), new { ROLE_ID = roleId });
```

### 事务支持

```csharp
var masterDb = new MasterDbContext();

 using (var trans = _db.GetTransaction())
 {
     _db.R_MACH_PART_LIFE.Delete(entity_life);
     _db.R_MACH_PART_LIFE.Insert(newPart);
    //业务sql
    trans.Complete();
 }


```