## Language

[English](README.md) | 简体中文





## Koubot.Tool

辅助开发Koubot项目相关的工具包，提供了大量扩展方法，以及一些工具类，也可以用于开发其他项目（懒人专用），也已经上传到nuget上，可以使用nuget搜索Koubot进行引用，该工具类项目没有也不会有其他项目的依赖。基于.NET Standard 2.0

> 里面有部分方法使用了ReSharper的特性，使用ReSharper的插件可以获得良好的提示和代码检查：
>
> ```C#
> 		//能够教会ReSharper空判断(传入的是null，返回true)		
> 		[ContractAnnotation("null => true")] 
>         public static bool IsNullOrEmpty([CanBeNull] this string s)
>             => string.IsNullOrEmpty(s);
> //https://www.jetbrains.com/help/resharper/Contract_Annotations.html#syntax
> ```
>





## General 一般工具

### KouTaskDelayer

定时器，定时执行一个任务，如果使用需要耗费一个long-running线程一直判断任务列表中是否有任务需要执行。支持重复的执行时间。

```c#
KouTaskDelayer.AddTask(executeDate, new Task(() => { ... }));
```





### SortTool

其中的扩展方法能够快速完成Comparison、Compare等相关方法的实现，且支持链式（实现权重排序）、null。

```c#
		public int CompareTo(SystemAliasList? other)
        {
            return this.CompareToObjAsc(IsGlobalAlias, other?.IsGlobalAlias, out int result)
                ?.CompareToObjAsc(IsGroupAlias, other?.IsGroupAlias, out result)
                ?.CompareToObjAsc(Advanced, other?.Advanced, out result)
                ?.CompareToObjAsc(AliasId, other?.AliasId, out result) == null ? result : 0;
        }
```

比如实现一个比较alias列表的比较器，该效果是先按照IsGlobalAlias字段升序、再按照IsGroupAlias字段升序、再按照Advanced字段升序、最后按照AliasID字段升序，且空的都会放在最后（体现支持null排序）





### KouWatch 单元测试计时器/效率比较器

需要计时测试执行时间的时候，总需要在前面写上

```
Stopwatch watch = new Stopwatch();
watch.Start();
...
watch.Stop();
Debug.WriteLine(watch.ElapsedMilliseconds);
```

显然十分繁琐，如果使用KouWatch，则可以使用以下代替，且会自动输出执行完毕的时间。

```
			KouWatch.Start("testName", () =>
            {
                ...
            });
```

另外如果要比较优劣，还有比较的方法，比如测试GetCustomAttributeCached与自带方法的效率比较：

```c#
			KouWatch.Start("cache", () =>
            {
                userBlacklist.FieldInfo(p => p.Reason);//二次封装GetCustomAttributeCached
            }, "default", () =>
            {
                property.GetCustomAttribute<KouAutoModelField>(); ;
            }, 100000);
```

```
//输出结果：
“cache”动作执行100000次中...
cache执行时间：253ms
“default”动作执行100000中...
default执行时间：596ms
效率比较：“cache”动作比default快135.573%倍
```









## Web 网络类

#### 漏桶算法限流器LeakyBucketRateLimiter

专门用于**客户端**调用API的漏桶算法限流器，目前网络上一般的漏桶算法实现不会计算请求到达服务器所需时间，造成超过服务器所规定的QPS。该限流器实现是请求完毕之后才会从桶中取出。

QPS支持浮点数，比如0.5，即2秒钟才可调用一次（另外QPS很大的话测试出来最多间隔50ms调用一次，因此目前最大有效支持的QPS为20左右，后期再进行优化）误差在0.5ms-3ms左右

另外限流器支持多种API同时限流，不会开启新线程用于自动漏桶，是用使用的线程来自动控制桶中的请求（但这也造成了对大QPS支持不好的缺点）

支持设定超时时间

使用示例：

```c#
using (var limiter = new LeakyBucketRateLimiter("test", TestQPS, TestLimitedSize))
{
    if (!limiter.CanRequest())
    {
        Console.WriteLine($"请求失败：{limiter.ErrorMsg}");
        return;
    }
    Console.WriteLine($"发出了一个API请求");
}
```

> 利用了IDisposable机制，当出using范围，即自动认为请求结束，可以从桶中取出元素



#### Encoder/Decoder

目前提供base64加密解密（`WebTool.EncodeBase64`），以及MD5加密（`WebTool.EncryptStringMD5`）。



## Random

### RandomTool

> 仅初始化一次并一直使用同一个随机数种子。
>
> 以`XXX.`开头的是对应类型的扩展方法
>

| Method                                  | Description                                                  |
| --------------------------------------- | ------------------------------------------------------------ |
| IList.RandomGetOne                      | 从IList中随机获取一个item，失败返回default(T)                |
| Array.RandomGetOne                      | 从Array中随机获取一个item，失败返回default(T)                |
| IList.RandomGetItems                    | 随机获取规定数量的items（不会重复），返回list，失败返回null  |
| IList.RandomList                        | 打乱IList顺序返回，失败则返回原来的list                      |
| IList.EnumRandomGetOne                  | 从Enum中随机选取一个（需要Enum类是0-n连续的）                |
| IntervalDoublePair.GenerateRandomDouble | 产生区间范围中的随机浮点数（与Tool中的IntervalDoublePair类连用） |
| GenerateRandomDouble                    | 产生区间范围中的随机浮点数                                   |
| T.ProbablyDo                            | 有x%可能性不返回null，就是有x%可能性会做（链式上?截断用于概率执行） |
| T.ProbablyBe                            | 有x%可能性会成为给定的对象                                   |
| double.ProbablyTrue                     | 有x%可能性返回true                                           |
| GetSecurityRandomByte                   | 获取强随机byte数组                                           |
| GetRandomString                         | 生成随机字符串                                               |



## Math

### ExpressionCalculator

表达式计算器，可以用于计算`sin(pi/2)^e-1*floor(3.5)`之类的表达式。

### IntervalDoublePair

基于IntervalDouble类，能够将字符串诸如`(1,3]`,`(-1,)`转换为区间，以及区间间判断包含关系。



## Interfaces

#### IKouErrorMsg

某些服务类中若实现该接口，可以获取返回错误的原因。



## String

### KouStringTool

将字符串转换成对应类型，大部分基于正则实现。（实际上是KouType类型系统的部分开源实现）

希望能进化到使用NLP（？

| Method Or Class                      | Description                                                  |
| ------------------------------------ | ------------------------------------------------------------ |
| ZhNumber                             | 关于中文数字字符串转换成阿拉伯数字的相关工具类，比如可以将字符串中所有中文数字替换成阿拉伯数字 |
| KouStringTool.TryToBool              | 支持各种表示肯定或否定的词转成bool                           |
| KouStringTool.TryToDouble            | 支持中文以及单位甚至表达式，比如：壹佰；500；1k、1w；一万；(sin(pi/2)^e\*100+14)*1000+五百一十肆 等等。 |
| KouStringTool.TryToInt               | 与Double相似，不过截断小数部分                               |
| KouStringTool.TryToEnum              | 将string转换成对应的enum类型。与KouEnumName特性标签同时使用获得最佳效果。 |
| KouStringTool.TryGetTimeSpanInterval | 字符串转时间区间，比如13:41-18:07:22，可以得到13:41的TimeSpan和18:07:22的TimeSpan |
| KouStringTool.TryToTimeSpan          | 一炷香（30分钟），一小时，1h5m（一小时五分钟），1:00（1分钟）,50（50秒），后天早上八点（自动计算当前到后天早上八点距离）、DateTime格式等等 |



### Format

使用

```c#
var result = $"{Reason?.Be($"\nReason:{Reason}")}";
```

替代

```
var result = $"{(Reason == null ? null : $"\nReason:{Reason}" )}";
```

另外还有`obj.BeIfNotEmpty`，`obj.BeIfNotWhiteSpace`，`obj.BeIfTrue`，`obj.BeIfNotDefault`



## SystemExpand

### GenericExpand

| Method       | Help                                                         |
| ------------ | ------------------------------------------------------------ |
| T.EqualsAny  | 判断是否存在一个元素给定的元素与之相等。                     |
| T.EqualsAll  | 这两个比较多个的时候很好用，比如Blog.Post.Name == "a" \|\| Blog.Post.Name == "b" \|\| Blog.Post.Name == "c"， 可以写成 Blog.Post.Name.EqualsAny("a","b","c") |
| T.SatisfyAny | 委托版                                                       |
| ...          | ...                                                          |

### IDictionary Expand

ContainsAny、ContainsAll、GetValueOrCustom...（自带的GetValueOrDefault不好用，不能传null）...





### Reflection

T.CloneParameters(T copyObj)：克隆某个对象中所有属性值到对象（EFCore会追踪修改，因为做的是Action操作）（可设定忽略克隆的属性名）







### Attribute类

CustomAttributeExtensions中封装一些关于CustomAttribute的扩展方法，使用了Dict做cache，能够高效的得到用户对类上使用的自定义标签（Attribute特性）中的值。

其中GetCustomAttributeCached方法能够快速获取对应类或指定属性或方法上的自定义标签，较自带的GetCustomAttribute而言，不需要手写反射得到类中指定属性、或方法的Type就可得到自定义标签，且效率高2倍左右。

### Enum

GetDescription

GetFlagsDescription

Remove

Add

HasAnyFlag

HasTheFlag（拥有IDE提示）

HasAllFlag

### Int/Double

LimitInRange



### Type

Type.IsNullableValueType...



