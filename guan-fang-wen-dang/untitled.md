---
description: 带您快速了解Ethereal
---

# 快速入门

## 引论

### 介绍

#### Ethereal

> **致力于解决混合编程、管理中心、注册中心、快速部署的SOA（面向服务架构）微服务框架。**

#### 微服务需求

> “ 庞大臃肿的业务，就像塞满逻辑的Main函数。”

假设我们要搭建一款大型游戏，我们是不是先需要简略的分析一下系统类别：用户系统、消息系统、战斗系统、副本系统、活动系统....

从我们的思考行为来佐证，我们也是本能的进行了业务功能需求的分离，如果把这些系统放到不同的服务器，这就叫**分布式**。

那什么是微服务呢？分布式完成了一个系统拥有的一组功能，比如用户系统拥有：登录、注册、查询，业务逻辑那么多，其实也很麻烦，所以我们再进一步的细分，**一个微服务就是一个业务**，登录、注册、查询，就是三个微服务。

我们从面向用户系统，转为了面向登录、注册、查询业务，颗粒度更低，目的性更强，实现起来更加轻松！

> “提倡将单一应用程序划分成一组小的服务，服务之间互相协调、互相配合，为用户提供最终价值。” ——百度百科\[微服务\]

作者表示一下自己的一得之见：

> _分布式：将一辆汽车分解为发动机、底盘、车身、电器；_
>
> _微服务：将发动机、底盘、车身、电器拆分为各种零器件；_
>
> _集群：将发动机、底盘、车身、电器乃至零器件进行批量复制。_

**发动机、底盘、车身、电器还是属于车的部件，但各种零器件已经和车没有了关联。**

大家都知道微服务的概念了，那有关微服务的一些技术问题也就接踵而来....

#### 服务与请求\[RPC\]

每一个微服务是一个零件，程序员通过组装零件，合成服务乃至系统，但零件和零件也会相互组合，一个微服务中，也会调用其他微服务，这样一个凌乱的环境，简化微服务调用逻辑的需求迫在眉睫！

RPC\(Remote Procedure Call\)即远程方法调用，成功的解决了这个技术难点。

图可能看起来有点复杂，其实我们可以字面意思理解，远程方法调用其实就是**调用远程服务器中的方法如同像调用本地方法一样**。

比如得到GetName这个方法，如果请求网络服务器执行这个方法，我们需要实现具体的这个方法的数据传输、数据接收，然后得到结果，并考虑这个结果以何种方式反馈。

如果直接通过Socket通讯，我们可以简单地通过文本操作进行数据分析。

> 请求文本："GetName\|\|参数一\|\|参数二\|\|参数三..."
>
> 返回文本："GetName\|\|结果"

假设有100个方法，你要写100条这样的判断语句嘛？【作者当时真的是这么做的，打开那页的代码时，已经有些卡顿了.....】

好的，幸得贵人指点，作者去研究了RPC框架。

原理就不说了，一些动态代理和反射的知识，动态代理和反射高级语言里面都有实现，我这里重点讲一下使用方法。

```csharp
//IServer接口[部署在客户端]
public interface IServer{
    public string GetName(long id);
}


//IServer实现类[部署在服务器]
public class Server:IServer{
    public string GetName(long id){
        return NameDictionary.Get(id);//从键值表中取出名字并返回
    }
}

//使用[客户端]
public void main(){
    IServer server = RPC.Register(IServer);//向RPC注册接口
    Console.WriteLine("Name:" + server.GetName(id));
}
```



**定义、实现、使用！**

开发者不需要关心数据发送和反馈的各种细节，你只需在客户端定义一个接口，在服务端进行接口的对应实现，便可以在客户端进行直接调用，当采用同步方法调用时，更是保证了代码逻辑上的**顺序执行**。

这种针对函数级别的调用，极大的简化了网络请求的复杂度，为后续微服务架构奠定了强有力的技术基础。

#### 服务注册与发现\[注册中心\]

我们既然讨论了微服务调用问题，接下来我们再进一步考虑一件事情，微服务就像各种零器件，如果只有七八个零器件，倒也简单，但一辆汽车，包含的零器件数以万计，这些零器件散落在地，凌乱混杂，怎么才能找到自己想要的那个零器件呢？

这时候就需要一个贴心的表单了，将每一个零器件进行注册，标记坐标，当需要时我们通过表单快速查找对应零器件信息，从而确定坐标，Get。

![img](https://upload-images.jianshu.io/upload_images/22310097-35c569edaad10183.png?imageMogr2/auto-orient/strip|imageView2/2/w/858/format/webp)

通过注册中心，能够将所有微服务进行注册，并通过一定的负载均衡算法，返回一个最优的目标地址。

> CAP理论
>
> 一致性\(Consistency\) : 所有节点在同一时间具有相同的数据
>
> 可用性\(Availability\) : 保证每个请求不管成功或者失败都有响应
>
> 分区容错\(Partition tolerance\) : 系统中任意信息的丢失或失败不会影响系统的继续运作

我们常听的_Eurake_是AP原则，去中心化；Consul 、Zookeeper是CP原则，唯一Leader。

Ethereal采用AP原则，去中心化处理，但Ethereal仍有意争取最大可能性的A原则，对于Ethereal的开发战略中，我们也将区块链作为了研究方向之一。

区块链的特性，可以完美的解决去中心化信息不一致的问题，但作者学业沉重，也仅仅是表态后续会尝试这一方向的进行实现（权限本就安全，极大的简化了区块链的实现难度，根据作者的分析，似乎只需要实现数据一致问题就可以了，希望这个点可以对感兴趣的人有帮助）。

#### 服务管理\[管理中心\]

微服务其实是SOA的一种变体，所以我们可以绕回来讲讲管理中心。

从单机而言，管理中心负责管理本机所有微服务，无关其他网络节点的微服务，单纯的开启、关闭控制自己本机的微服务，属于微服务框架中的管理模块；

但如果管理中心的定义放大到网络上，是对所有微服务的管理，对特定微服务进行远程控制。

Ethereal采取网络管理中心的方案，在Ethereal架构中，每一个Net都是一个网络节点，网络节点下有Service服务，每一个Service服务都包含了多个微服务（函数）。

Ethereal正积极开发网络管理中心前端，未来用户可以通过可视化的管理中心，对所有网络节点进行实时监控（Ethereal采用 WebSocket协议），用户可以轻松的管理每一个网络节点，并持久化在网站的实时配置。

#### 特性服务\[混合编程\]

作者遇到了太多需要通过混合编程来解决问题的需求了。

例如，一个硬件开发商想要硬件产品化，采用C++客户端采集硬件数据，而核心算法的处理却又必须放在Python（算法）所架构的服务端。

每一款语言的流行，必有其所耀眼的特点，C/C++提供了对硬件的强大编程能力，Python提供了对数据的强大处理能力，鱼和熊掌可否兼得也？

以C++作为客户端，向Python服务端发起请求的混合编程需求，作者认为比较好的途径就是通过网络通讯协议解决，这也与RPC相吻合。

Ethereal也对混合编程进行了支持，而且是强有力的支持，Ethereal采用了注解式声明，不需要第三方代码生成工具、不需要学习额外的语法。

同时Ethereal也支持**任意数据类型**的传输，Ethereal采用中间抽象类型的思想，支持参数级的处理，任意参数的序列化方式与逆序列化方式，都可以进行自定义，我们可以默认采用_Json_序列化，您也可以使用ProtoBuf、Xml亦或者个人设计的序列化语法。

#### 责任说明

1. Ethereal并非所有语言都会实现一套C\S，我们理性的认为，用C++搭建服务器是一个糟糕的决定，所以我们长期不会对C++的服务器版本进行支持，且短期并无意于C++客户端版本。我们深知C++客户端的迫切，所以我们采用WebSocket协议，同时也支持了HTTP协议，这两种协议无论在何种流行语言，都有完整的框架支持，所以依旧可以与Ethereal进行交互，确保了无Ethereal版本支持下的最低交互保证！
2. Ethereal热衷于支持流行语言，无论是C\#、Java还是Python都有了可靠的支持，但也并非局限于这几种语言，我们仍在招募着志同道合的道友，同我们一起维护与拓展。
3. Ethereal采用LGPL开源协议，我们希望Ethereal在社区帮助下持续健康的成长，更好的为社区做贡献。
4. Ethereal长期支持，我们欢迎开发者对Ethereal进行尝鲜。

### 入门

接下来我们以C\#和Java版本来快速了解三步曲：类型、网关、服务\请求。

#### Server\[C\#\]

```csharp
public class ServerService
{
    [Service]
    public int Add(int a,int b)
    {
        return a + b;
    }
}

//注册数据类型
AbstractTypes types = new AbstractTypes();
types.Add<int>("Int");
types.Add<long>("Long");
types.Add<string>("String");
types.Add<bool>("Bool");
types.Add<User>("User");
Net net = NetCore.Register("name", Net.NetType.WebSocket); //注册网关
Server server = ServerCore.Register(net,"127.0.0.1:28015/NetDemo/");//注册服务端
Service service = ServiceCore.Register<ServerService>(net, "Server", types);//注册服务
net.Publish();//启动
```

#### Client\[Java\]

```java
public interface ServerService
{
    @Request
    public Integer Add(Integer a,Integer b);
}
//注册数据类型
AbstractTypes types = new AbstractTypes();
types.add(Integer.class,"Int");
types.add(Long,"Long");
types.add(String,"String");
types.add(Boolean,"Bool");
types.add(User.class,"User");
Net net = NetCore.register("name", Net.NetType.WebSocket); //注册网关
Client client = ClientCore.Register(net,"127.0.0.1:28015/NetDemo/");//注册客户端
Request request = RequestCore.register(ServerRequest.class,net, "Server", types);//注册请求
net.publish();//启动
```

### 架构

#### Core

> 维护特有类，作为全局静态类，唯一对外公开接口，确保了对应实体注册/销毁/访问时安全性。

Core一般含有Register、UnRegister、Get三大公开方法，Ethereal拥有四个Core，分别为：

* **NetCore**：Net网络节点的管理
* **ClientCore/ServerCore**：Client客户端或Server服务端的管理
* **ServiceCore**：Service请求体的管理
* **RequestCore**：Request请求体的管理

  Core并非实质保存着对该实体的实例，实际上，Request、Service、Client/Server都归于Net，Net作为一个网络节点，与其他网络节点交互（管理中心、注册中心）。

  Core的目标是屏蔽注册细节，也是为了保证访问安全，Core是用户交互操作的唯一入口。

#### Config

Config含有各式各样的配置项，以此满足用户的个性化配置。

* **NetConfig**：Net网络节点配置项
* **ClientConfig/ServerConfig**：Client/Server配置项
* **ServiceConfig**：Service配置项
* **RequestConfig**：Request配置项

  同时Config可作为蓝本，在多个实体间共享。

#### Object

Core根据Config配置产生具体的Object（实体），实体完成具体的工作。

* **Net**：对内作为管理中心，管理实体，对外负责作为注册中心向外暴露服务。
* **Client/Server**：通讯框架，Java使用Netty框架，Python使用Twisted。
* **Service**：服务实现类，负责请求的具体实现。
* **Request**：服务请求类，负责向远程具体的服务实现发起请求。

## 技术文档

### 中心配置

Ethereal中心配置涵盖了注册中心、管理中心的功能。

中心配置十分轻效，在Net的Config中开启集群配置，并提供集群地址和对应集群配置项即可。

**Server\[C\#\]**

```csharp
Net net = NetCore.Register("name", Net.NetType.WebSocket); //注册网关
//开启集群模式
net.Config.NetNodeMode = true;
List<Tuple<string, ClientConfig>> ips = new();
//添加集群地址
ips.Add(new Tuple<string,ClientConfig>($"{ip}:{28015}/NetDemo/", new ClientConfig()));
ips.Add(new Tuple<string,ClientConfig>($"{ip}:{28016}/NetDemo/", new ClientConfig()));
ips.Add(new Tuple<string,ClientConfig>($"{ip}:{28017}/NetDemo/", new ClientConfig()));
ips.Add(new Tuple<string,ClientConfig>($"{ip}:{28018}/NetDemo/", new ClientConfig()));
net.Config.NetNodeIps = ips;
```

**Client\[Java\]**

```java
Net net = NetCore.register("name", Net.NetType.WebSocket); //注册网关
//开启集群模式
net.getConfig().setNetNodeMode(true);
ArrayList<Pair<String, ClientConfig>> ips = new ArrayList<>();
//添加集群地址
ips.add("127.0.0.1:28015/NetDemo/",new ClientConfig());
ips.add("127.0.0.1:28016/NetDemo/",new ClientConfig());
ips.add("127.0.0.1:28017/NetDemo/",new ClientConfig());
ips.add("127.0.0.1:28018/NetDemo/",new ClientConfig());
net.getConfig().setNetNodeIps(ips);
```

> Ethereal的中心服务部署在Net，与正常Service属于同一层级，这也意味着不需要额外的端口，一个Net节点，就是一个中心，不需要关心集群部署时的端口配置问题，您在**部署服务的同时，也是在部署集群！**

### Token理念

还是简单分析一下场景需求：

一个用户发起登录请求，服务器执行登录逻辑之后，有时会需要将用户信息暂存内存，需要时再查询该用户信息。

比如用户登录，服务器生成一个User类的实体，内部包涵了该用户的临时信息，比如登录时间、用户等级、用户凭证.....当用户断开连接时，再将这个实体销毁。

并且服务时，往往是多个客户端对标一个服务端，如果遇到聊天系统的功能需求，客户端之间也往往会通过服务端进行通讯，这样服务端也需要区别不同的客户端。

基于上述需求，Ethereal开放了Token类别，Token相当于一个客户端连接体，用户控制BaseToken即控制客户端连接体。

BaseToken内含有唯一Key值属性，Ethereal通过用户给予的Key值属性，对Token进行生命周期处理。

```csharp
[Service]
public bool Login(BaseToken token, string username,string password)
{
    token.Key = username;//为该token设置键值属性
    BaseToken.Register();//将token注册，受Ethereal管理其生命周期
}
```

通过上面的函数，我们似乎发现了一个特殊之处，token放在了服务类的**首参**，其实刚刚的加法函数也可以改写为：

```java
public class ServerService
{
    [Service]
    public int Add(BaseToken token,int a,int b)
    {
        return a + b;
    }
}
```

Ethereal会根据用户的首参情况，来决定是否为首参注入token实体。

在Request中，不必提供token参数的定义，对于Request，仍保持基本接口规范即可。

`public Integer Add(Integer a,Integer b);`

```csharp
[Service]
public bool Login(User user, string username,string password)
{
    user.Key = username;//为该token设置键值属性
    user.Register();//将token注册，受Ethereal管理其生命周期
}
```

BaseToken是可继承的，那就代表了用户可以通过自定义一个User类，并继承BaseToken，这进一步的转换了设计理念，从面向连接体，变为了面向用户。

### 双工通讯

Ethereal致力于服务尽可能多的需求业务，虽然现今单工请求占据了大量业务需求，但绝不是所有，作者本人也是经常需要用到双工通讯的，尤其是在游戏业务这一块。

所以就拿游戏业务这一块进行阐明，假设一个游戏角色拥有移动、攻击、聊天等行为，服务端可以通过执行一套请求逻辑，从而达到控制目标角色的需求，可以极大简化服务端的编程逻辑。

```java
public interface ServerService
{
    //Player继承BaseToken
    @Request
    public void Move(Player player);
    @Request
    public void Attack(Player player);
    @Request
    public void Chat(Player player);
}
```

这里的请求，是服务端发起，客户端接收，请求的注册方式与客户端的注册方式相一致，也是通过RequestCore进行注册。

> 与客户端的请求不同点在于：
>
> 1. 服务端的首参必为BaseToken，前文说道BaseToken实际就是连接体，所以传递BaseToken实际代表了将请求发送到目标客户端。
> 2. 服务端的请求函数必定没有返回值，我们不认为服务端等待客户端的结果返回是一个明智的选择。\[当然，这是基于目前的需求来看，需求也在不断地变动，后续也许Ethereal会开放返回值\]

我们这里有一套完整向某用户发送消息Demo：

```csharp
public class ServerService
{
    /// <summary>
    /// 向服务端注册用户
    /// </summary>
    /// <param name="user">客户端用户</param>
    /// <param name="username">用户名</param>
    /// <param name="id">用户ID</param>
    [Service]
    public bool Register(User user, string username, long id)
    {
        user.Username = username;
        user.Key = id;
        return user.Register();//BaseToken方法，向Ethereal注册Token。
    } 
    /// <summary>
    /// 接受客户端发送来的发送消息给某个用户的命令请求
    /// </summary>
    /// <param name="sender">客户端用户</param>
    /// <param name="recevier_key">目标接收用户的唯一Key值</param>
    /// <param name="message">消息内容</param>
    [Service]
    public bool SendSay(User user, long recevier_key, string message)
    {
        //从Ethereal的Net节点中查找目的用户（经过Register注册的）
        User reciver = user.GetToken(recevier_key);
        if (reciver != null)
        {
            //向listener用户发送Hello请求
            request.Say(reciver,user.Name + "说:" + message);
            return true;
        }
        else return false;
    }
}
```

### **日志系统**

Ethereal的日志系统（TrackLog）力图最大化的信息输出，TrackLog实体中，包含了从该点向上一层不断抛出时的抛出实体信息。

TrackLog中含有Net、Request\Service、Client\Server实体，输出日志时，Log会根据事件发生点进行注入抛出，比如一个Service日志，将包含Service、Client、Net三个实体，同时应注意，事件输出之后，应保证这些核心实体不应该被外部保存，避免造成内存泄漏。

每一个核心实体，都包含了日志事件，您可以通过注册事件，实现日志输出事件的捕获，并且可以根据选择，捕获不同层级的事件。

通常捕获Net事件，代表了该Net节点的所有日志输出。

```csharp
net.ExceptionEvent += ExceptionEventFunction;
private static void ExceptionEventFunction(TrackException exception)
{
    Console.WriteLine(exception.Message);
}
```

### **异常系统**

Ethereal的日志系统（TrackException）力图最大化的信息输出，TrackException实体中，包含了从该点向上一层不断抛出时的抛出实体信息。

TrackException中含有Net、Request\Service、Client\Server实体，抛出异常时，TrackException会根据事件发生点进行注入抛出，比如一个Service异常，将包含Service、Client、Net三个实体，同时应注意，事件输出之后，应保证这些核心实体不应该被外部保存，避免造成内存泄漏。

每一个核心实体，都包含了异常事件，您可以通过注册事件，实现日志输出事件的捕获，并且可以根据选择，捕获不同层级的事件。

通常捕获Net事件，代表了该Net节点的所有异常输出。

**与Log不同的是，TrackException内部包含了一个Exception字段，该字段是真正的异常事件，有时为TrackException本身，但也有时是一些其他异常，Ethereal捕获所有异常并封装在其内部。**

> ```csharp
> net.ExceptionEvent += ExceptionEventFunction;
> private static void ExceptionEventFunction(TrackException exception)
> {
>     Console.WriteLine(exception.Message);
> }
> ```

### **服务拦截**

Ethereal的服务拦截分为Net层拦截，以及Service层拦截，且两层拦截均含有Net、Service、Method、Token信息，用户可以充分的获取有用信息来进行判断。

在拦截委托中，如果您返回`True`将进行下一个拦截事件检测，而返回`False`，则消息立即拦截，后续的拦截策略不会执行。

```csharp
service.InterceptorEvent += Interceptor;
private static bool Interceptor(Net net, Service service, MethodInfo method, Token token)
{
    if (token.Key == "123")
    {
        return false;
    }
}
```

**同时，基于拦截器，Ethereal开发了权限拦截的功能拓展。**

```csharp
[Service(authority = 3)]
public bool SendSay(User user, long recevier_key, string message)
{
    //从Ethereal的Net节点中查找目的用户（经过Register注册的）
    User reciver = user.GetToken(recevier_key);
    if (reciver != null)
    {
        //向listener用户发送Hello请求
        request.Say(reciver,user.Name + "说:" + message);
        return true;
    }
    else return false;
}
public class User:BaseToken,IAuthorityCheck
{
    public bool Check(IAuthoritable authoritable){
        if(this.Authority >= authoritable.Authority)return true;
        else return false;
    }
}
```

1. `BaseToken`类实现`IAuthorityCheck`接口，实现权限检查函数
2. 在方法注解中设置添加authority参数：`[Service(authority = 3)]`，这里3就是提供的权限信息
3. 在拦截器中添加Ethereal权限检查函数

   `service.InterceptorEvent += Extension.Authority.AuthorityCheck.ServiceCheck;`

   等待收到请求到达该方法，Ethereal会主动调用`BaseToken`类实现`IAuthorityCheck`接口中的Check函数，具体权限判断逻辑，用户可以根据自己的情况自行设计，最简单的就是大于该等级，即可通过。

## 关于我们

### 团队成员

#### 项目分配

| 板块 | 语言\|框架 | 开发人员 |
| :---: | :---: | :---: |
| Server | C\# | 白阳 |
| Client | C\# | 白阳 |
| Server | Java | 007 |
| Client | Java | anmmMa |
| Server | Python | Ckay |
| Client | Python | 青山 |
| Center | Vue | Laity |
| Document | Jekyll | 白阳 |

