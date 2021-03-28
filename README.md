## Language

English | [简体中文](README.zh_CN.md)





## Koubot.Tool

A toolkit to assist in the development of Koubot related projects, providing a large number of extension methods, as well as some tool classes, can also be used to develop other projects (for lazy people lol), has also been uploaded to Nuget, you can use Nuget search Koubot to reference. The toolkit project does not have and will not have other project dependencies. 

bases on .NET Standard 2.0



> Some of the methods inside use features of  VS plug-in ReSharper, and good hints and code checking are available when using ReSharper
>
> ```C#
> 		//Ability to teach ReSharper null judgment (passed in null, returns true)		
> 		[ContractAnnotation("null => true")] 
>         public static bool IsNullOrEmpty([CanBeNull] this string s)
>             => string.IsNullOrEmpty(s);
> //https://www.jetbrains.com/help/resharper/Contract_Annotations.html#syntax
> ```
>





## General

### KouTaskDelayer

Timer that executes a task at particular time. if used it, it will takes a long-running thread to determine whether there is a task in the task list that needs to be executed. Supports same execution times.

```c#
KouTaskDelayer.AddTask(executeDate, new Task(() => { ... }));
```





### SortTool

Extension methods in it can help to quickly complete the implementation of Comparison, Compare and other related methods, and it support chaining (to achieve weight sorting) and compare with null.

```c#
		public int CompareTo(SystemAliasList? other)
        {
            return this.CompareToObjAsc(IsGlobalAlias, other?.IsGlobalAlias, out int result)
                ?.CompareToObjAsc(IsGroupAlias, other?.IsGroupAlias, out result)
                ?.CompareToObjAsc(Advanced, other?.Advanced, out result)
                ?.CompareToObjAsc(AliasId, other?.AliasId, out result) == null ? result : 0;
        }
```

For example, to implement a CompareTo method for alias list, the effect is that the list will first sorted by the IsGlobalAlias field in ascending order, then by the IsGroupAlias field ascending, then by the Advanced field ascending , and finally by the AliasID field ascending, and the empty ones will be placed at the end (represent the null comparison support)





### KouWatch

Unit test timer/efficiency comparator

When you need to time the execution time, you always need to write：

```
Stopwatch watch = new Stopwatch();
watch.Start();
...
watch.Stop();
Debug.WriteLine(watch.ElapsedMilliseconds);
```

Obviously very tedious, if you use KouWatch, you can use the following instead, and it will automatically output the time when the execution is finished.

```
			KouWatch.Start("testName", () =>
            {
                ...
            });
```

In addition, if you want to compare the execution time, there are methods for comparing, such as testing the efficiency of GetCustomAttributeCached compared to the system method.

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
//Output results.
“cache”动作执行100000次中...
cache执行时间：253ms
“default”动作执行100000中...
default执行时间：596ms
效率比较：“cache”动作比default快135.573%倍
```









## Web

#### LeakyBucketRateLimiter

A leaky bucket algorithm flow limiter specifically for **client-side** API calls. The common implementation of the Leaky Bucket algorithm on the web does not calculate the time it takes for a request to reach the server, causing it easily to exceed the QPS specified by the server. This flow limiter implementation takes the request out of the bucket only after it is complete.

QPS support floating point number, such as 0.5, that is, 2 seconds before a call (in addition, if the QPS is very large, the interval call once will be at most 50ms , so the current maximum effective support for QPS for about 20 (later to optimize)) error in 0.5ms - 3ms or so

In addition, the flow limiter supports multiple APIs to limit the flow simultaneously and does not open new threads for automatic bucket leakage, but uses these threads to automatically control the requests in the bucket (but this also creates the disadvantage of poor support for large QPS)

support setting timeout time

Usage example:

```c#
using (var limiter = new LeakyBucketRateLimiter("test", TestQPS, TestLimitedSize))
{
    if (!limiter.CanRequest())
    {
        Console.WriteLine($"Request Failed: {limiter.ErrorMsg}");
        return;
    }
    Console.WriteLine($"An API request is sent");
}
```

> The IDisposable feature is utilized so that when the request is out of the using range, the request is automatically considered finished and the request can be removed from the bucket
>



#### Encoder/Decoder

Currently provides base64 encryption and decryption (`WebTool.EncodeBase64`), and MD5 encryption (`WebTool.EncryptStringMD5`).



## Random

### RandomTool

> Initialize only once and always use the same random number seed.
>
> Those starting with `XXX.` are extension methods of the corresponding type

| Method                                  | Description                                                  |
| --------------------------------------- | ------------------------------------------------------------ |
| IList.RandomGetOne                      | Get a random item from IList and return default(T) if it fails |
| Array.RandomGetOne                      | Get a random item from the Array and return default(T) if it fails |
| IList.RandomGetItems                    | Randomly get a specified number of items (will not repeat), return list, failure returns null |
| IList.RandomList                        | Disrupt the IList order and return it, or return the original list if it fails |
| IList.EnumRandomGetOne                  | Randomly select one from Enum (need Enum class to be 0-n continuous) |
| IntervalDoublePair.GenerateRandomDouble | Generate random double in an interval range (used with the IntervalDoublePair class in Tool) |
| GenerateRandomDouble                    | Generate random double in an interval range                  |
| T.ProbablyDo                            | There is an x% probability of not returning null, that is, there is an x% probability that it will do it (use `?` on the chain truncation, for probabilistic execution) |
| T.ProbablyBe                            | There is x% chance that the given object will be giving T    |
| double.ProbablyTrue                     | Returns true with x% probability                             |
| GetSecurityRandomByte                   | Get a strong random byte array                               |
| GetRandomString                         | Generate a random string                                     |
| ...                                     | ...                                                          |



## Math

### ExpressionCalculator

Expression calculator that can be used to calculate expressions like `sin(pi/2)^e-1*floor(3.5)`.

### IntervalDoublePair

Based on the IntervalDouble class, it can convert strings such as `(1,3]`, `(-1,)` into intervals and determine the inclusion relationship between intervals.



## Interfaces

#### IKouErrorMsg

Some service classes that implement this interface can get the reason for the error returned.



## String

### KouStringTool

string into the corresponding type , mostly based on the regular implementation . (actually a partial open source implementation of the KouType type system)

Hope to evolve to use NLP (?)

| Method Or Class                      | Description                                                  |
| ------------------------------------ | ------------------------------------------------------------ |
| ZhNumber                             | Tools related to the conversion of Chinese numeric strings to Arabic numbers, such as the ability to replace all Chinese numbers in a string with Arabic numbers |
| KouStringTool.TryToBool              | Support all kinds of words that express affirmative or negative to bool |
| KouStringTool.TryToDouble            | Support Chinese as well as units and even expressions, such as: 壹佰；500；1k、1w；一万；(sin(pi/2)^e\*100+14)*1000+五百一十肆, etc. |
| KouStringTool.TryToInt               | Similar to Double, but truncates the decimal part            |
| KouStringTool.TryToEnum              | Converts a string to its corresponding enum type. Use with the KouEnumName feature tag for best results. |
| KouStringTool.TryGetTimeSpanInterval | String to time interval, for example 13:41-18:07:22, you can get TimeSpan of 13:41 and TimeSpan of 18:07:22 |
| KouStringTool.TryToTimeSpan          | 一炷香(30 minutes)，一小时，1h5m(one hour and five minutes)，1:00(one minute),50(50 seconds)，后天早上八点(automatically calculates the current distance to 8:00 a.m. the day after), DateTime format, etc. |



### Format

Use

```c#
var result = $"{Reason?.Be($"\nReason:{Reason}")}";
```

instead of

```
var result = $"{(Reason == null ? null : $"\nReason:{Reason}" )}";
```

There are also `obj.BeIfNotEmpty`，`obj.BeIfNotWhiteSpace`，`obj.BeIfTrue`，`obj.BeIfNotDefault`



## SystemExpand

### GenericExpand

| Method       | Help                                                         |
| ------------ | ------------------------------------------------------------ |
| T.EqualsAny  | Determines if there exists an element given as equal to it.  |
| T.EqualsAll  | Those two work well when comparing more than one. <br>for example, `Blog.Post.Name == "a" || Blog.Post.Name == "b" || Blog.Post.Name == "c"` can be replaced by `Blog.Post.Name.EqualsAny("a","b","c")` |
| T.SatisfyAny | delegate version                                             |
| ...          | ...                                                          |

### IDictionary Expand

ContainsAny、ContainsAll、GetValueOrCustom... (The official GetValueOrDefault isn't convenient, because it cannot pass null)...





### Reflection

T.CloneParameters(T copyObj)：Clone all property values in an object to the object (EFCore will track the changes because the Action operation is done) (can be set to ignore the cloned property names)





### Attribute

CustomAttributeExtensions encapsulates some extension methods about CustomAttribute, using Dictionary as cache, to efficiently get the value of the custom tag (Attribute) used by the user on the class.

The GetCustomAttributeCached method can quickly get the custom attribute on the corresponding class or the specified attribute or method, compared with the self-contained GetCustomAttribute, no need to use reflection to get the Type of the specified attribute or method in the class to get the custom attribute, and about two times more efficient.



### Enum

GetDescription

GetFlagsDescription

Remove

Add

HasAnyFlag

HasTheFlag (make good use of IDE hint)

HasAllFlag

### Int/Double

LimitInRange



### Type

Type.IsNullableValueType...



