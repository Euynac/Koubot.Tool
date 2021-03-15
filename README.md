# Koubot.Tool

辅助开发Koubot项目相关的工具包。也可以用于开发其他项目，封装了一些常用的小工具。

里面有部分方法使用了ReSharper的特性，使用ReSharper的插件可以获得良好的提示：

```C#
		[ContractAnnotation("null => true")] //能够教会ReSharper空判断(传入的是null，返回true)https://www.jetbrains.com/help/resharper/Contract_Annotations.html#syntax
        public static bool IsNullOrEmpty([CanBeNull] this string s)
            => string.IsNullOrEmpty(s);
        
```



## 工具类

### KouWatch 单元测试计时器/效率比较器

比如测试GetCustomAttributeCached与自带方法的效率比较：

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

比如实现一个比较alias列表的比较器，该效果是空的都会放在最后（支持null），且先按照IsGlobalAlias字段升序、再按照IsGroupAlias字段升序、再按照Advanced字段升序、最后按照AliasID字段升序。







## 服务类

#### 漏桶算法限流器LeakyBucketRateLimiter

专门用于客户端调用API的漏桶算法限流器，一般的漏桶算法实现不会计算请求到达服务器所需时间，造成超过服务器所规定的QPS。该限流器实现是请求完毕之后才会从桶中取出。

QPS支持浮点数，比如0.5，即2秒钟才可调用一次（另外QPS很大的话测试出来最多间隔50ms调用一次，因此目前最大有效支持的QPS为20左右，后期再进行优化）误差在0.5ms-3ms左右

另外限流器支持多种API同时限流，不会开启新线程用于自动漏桶，是用使用的线程来自动控制桶中的请求（但这也造成了对大QPS支持不好的缺点）

支持设定超时时间

使用示例：

```c#
using (var limiter = new LeakyBucketRateLimiter("test", TestQPS, TestLimitedSize))//利用了IDisposable机制，当出using范围，即自动认为请求结束，可以从桶中取出元素
{
    if (!limiter.CanRequest())
    {
        Console.WriteLine($"请求失败：{limiter.ErrorMsg}");
        return;
    }
    Console.WriteLine($"发出了一个API请求");
}
```



## 接口类

#### IKouErrorMsg

某些服务类中若实现该接口，可以获取返回错误的原因。



## 其他扩展方法

#### Reflection 方法

T.CloneParameters(T copyObj)：克隆某个对象中所有属性值到对象（EFCore会追踪修改，因为做的是Action操作）（可设定忽略克隆的属性名）



#### Random 方法

IList.RandomGetOne：从IList中随机获取一个item，失败返回default(T)

IList.RandomGetItems：随机获取规定数量的items（不会重复），返回list，失败返回null

IList.RandomList：打乱IList顺序返回，失败则返回原来的list

IList.EnumRandomGetOne：从Enum中随机选取一个（需要Enum类是0-n连续的）

IntervalDoublePair.GenerateRandomDouble：产生区间范围中的随机浮点数（与Tool中的IntervalDoublePair类连用）

T.ProbablyDo：有x%可能性不返回null，就是有x%可能性会做（链式上?截断用于概率执行）

T.ProbablyBe：有x%可能性会成为给定的对象

double.ProbablyTrue：有x%可能性返回true

#### Attribute类

CustomAttributeExtensions中封装一些关于CustomAttribute的扩展方法，使用了Dict做cache，能够高效的得到用户对类上使用的自定义标签（Attribute特性）中的值。

其中GetCustomAttributeCached方法能够快速获取对应类或指定属性或方法上的自定义标签，较自带的GetCustomAttribute而言，不需要反射得到类中指定属性、或方法的Type即可得到自定义标签，且效率高2倍左右。

#### String 类

支持string转int、double、TimeSpan、bool等（和Koubot中的支持类型一致，其实就是KouType类型转换的实现开源）

int以及double：支持sin(pi/2)*(壹佰+14)\*1000+五百一十四等字符串变成114514

TimeSpan：支持明天早上八点过五分、5:00、一炷香、一个月等转化为对应当前时间的TimeSpan

支持string转以上对应的IList\<\>类，比如List\<int\>：19，19，810等输入可以获得{19,19,810}这样的int List。



#### System 相关拓展方法

扩展一些常用的方法，加速开发，缩减代码

- 时间相关扩展：字符串、DateTime格式转特定类型（Unix、Javascript）时间戳

- 特性相关扩展：获取一个类所实现的Interface类上的Custom Attribute（暂不支持接口方法和属性）

- Enum类扩展：读取Enum枚举元素上标记了DescriptionAttribute特性的值；判断任一、全部给定的枚举是否满足...

- Object类扩展

  使用BeNullOr（如果给定object为null则返回null，否则返回给定字符串）快速格式化：

  ```C#
  var result = $"{Group.Name?.BeNullOr(Group.Name)}")}" +
      			$"[组 {Group.PlatformGroupId}]" +
                  $"{Plugin?.BeNullOr(Plugin.PluginZhName)}")}" +
                  $"{Reason?.BeNullOr($"\n原因：{Reason}")}",
  ```

  EqualsAny、EqualsAll（比较多个的时候很好用，比如Blog.Post.Name == "a" || Blog.Post.Name == "b" || Blog.Post.Name == "c"， 可以写成 Blog.Post.Name.EqualsAny("a","b","c")）

  等等

- IDictionary类扩展：ContainsAny、ContainsAll、GetValueOrCustom（自带的GetValueOrDefault不好用，不能传null）...

- int类扩展：LimitInRange...

  ...

  
  
  