/*
默认全局 using
实际上微软已经自动把一些常用的 using 在编译后的代码中自动补上了，路径在 项目/obj/Debug/net6.0/项目.GlobalUsings.cs 文件中
也就是以上的 using 无需写在你创建的 GlobalUsings.cs 中了，微软会在编译时自动合并。
*/
global using System.Collections.Concurrent;
global using System.Threading.Channels;
