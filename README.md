# Koubot.Tool

辅助开发Koubot项目相关的工具包。也可以用于开发其他项目，封装了一些常用的小工具。

目前只上传了 System 相关的扩展方法。

里面有部分方法使用了ReSharper的特性，使用ReSharper的插件可以获得良好的提示：

```C#
[ContractAnnotation("null => true")] //能够教会ReSharper空判断(传入的是null，返回true)https://www.jetbrains.com/help/resharper/Contract_Annotations.html#syntax
        public static bool IsNullOrEmpty([CanBeNull] this string s)
        {
            return string.IsNullOrEmpty(s);
        }
```





#### System 相关拓展方法

扩展一些常用的方法，加速开发，缩减代码

- 时间相关扩展：字符串、DateTime格式转特定类型（Unix、Javascript）时间戳

- 特性相关扩展：获取一个类所实现的Interface类上的Custom Attribute（暂不支持接口方法和属性）

- Enum类扩展：读取Enum枚举元素上标记了DescriptionAttribute特性的值；判断任一、全部给定的枚举是否满足...

- Object类扩展

  使用BeNullOr（如果给定object为null则返回null，否则返回给定字符串）快速格式化：

  ```C#
  var result = $"{Group.Name.BeNullOr($"{Group.Name}")}[组 {Group.PlatformGroupId}]" +
                  $"{Plugin.BeNullOr($"【{Plugin?.PluginZhName}】")}" +
                  $" {(IsGlobal ? "【全局】" : null)} 结束于 {EndAt}" +
                  $"{Reason.BeNullOr($"\n原因：{Reason}")}",
  ```

  EqualsAny、EqualsAll（比较多个的时候很好用，比如Blog.Post.Name == "a" || Blog.Post.Name == "b" || Blog.Post.Name == "c"， 可以写成 Blog.Post.Name.EqualsAny("a","b","c")）

  等等

- IDictionary类扩展：ContainsAny、ContainsAll、GetValueOrCustom（自带的GetValueOrDefault不好用，不能传null）...

- int类扩展：LimitInRange...

  ...

  

