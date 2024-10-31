## .NET 技巧：接口（interface）特性及用法汇总

虽然 ``interface`` 与 ``class``里面属性写的都是 ``get; set;``，但是意义是不同的：

- 接口中表示有一个名为 Name 的属性，它拥有 public 的 getter 与 setter，但并不知道它们是或者将会被怎样实现
- 类中表示有一个名为 Name 的属性，它拥有 public 的 getter 与 setter，并且这是一个语法糖，会自动生成一个后台对应的私有字符安，然后将 getter 与 setter 对应到这个字段上