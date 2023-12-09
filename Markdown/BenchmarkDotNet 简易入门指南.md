## **BenchmarkDotNet简易入门指南**

---

### **Class**

- `MemoryDiagnoser`：查看内存分配情况（有一个bool参数，表示是否显示GC的情况）
- `SimpleJob`：可以设置如 RuntimeMoniker.Net60
- `Orderer(SummaryOrderPolicy.SlowestToFastest)`：输出结果的排序
- `RankColumn`：为结果表格添加一列 Rank，表示当前行的方法的排名

### **Method**

- `Benchmark`：表示这个方法需要被测试（另有一个 `Baseline` 参数，同时会给结果添加一列 Ratio，表示和 Baseline 的比率）
- `Arguments`：类似于 `Params`，表示该方法的传参，可以有多个，并且会和 `Params` 联动，充分考虑各种组合
- `GlobalSetup`：全局初始化，常用于初始化一个要用来测试的变量、集合等。可以和 Params 联动，比如数组的容量由某个字段决定
- `IterationSetup`：用于在每次迭代前的初始化，每次迭代都会调用一次

### **Field**

- `Params`：某个字段可能有不同的值(如果多个字段被标记该特性，则会充分考虑所有参数的组合)

 

### **注意事项：**

1、要使用有编译器优化的 Release 模式

2、被测试的类、使用了特性的方法与字段均需要为 public

3、在要测试的方法中尽量避免会被 JIT 优化掉的情况，比如有一个不会被使用的变量等

4、除非还想要测试内存读取的速度等，否则一般没有必要创建过大的数组