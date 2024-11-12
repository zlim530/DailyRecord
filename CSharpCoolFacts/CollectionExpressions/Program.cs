// See https://aka.ms/new-console-template for more information

// C# 12新增的集合表达式的用法及底层原理

using System.Collections;
using System.Collections.ObjectModel;

Console.WriteLine("Collection Expressions!");

List<int> list = [1, 2, 3, 4, 5, 6, 7, 8, 9];// 编译使用 CollectionsMarshal 与 Span 初始化
int[] array = [ 1, 2, 3, 4, 5, 6, 7, 8, 9 ];
// var col = [1,2,3,4]; error:集合表达式没有目标类型

// 使用以下语法初始化空集合：
List<int> emptyList = [];// List<int> emptyList = new List<int>(); 和之前一样
int[] emptyArray = []; // 反编译为：Array.Empty<int>(); 性能更好 


HashSet<int> set = [1,2,3,4]; // 编译使用 new HashSet<int>() 和 Add() 方法
IEnumerable<int> enumerable = [1, 2, 3, 4, 5, 6, 7, 8, 9]; // 编译使用 z__ReadOnlyArray
IReadOnlyList<string> readOnlyList = ["one", "two", "three"]; // 编译使用 z__ReadOnlyArray
ICollection<char> collection = ['a', 'b','c'];// 编译使用 List<T> 初始化
ObservableCollection<object> observableCollection = [new object(), new object()];// 编译使用 new ObservableCollection<object>() 和 Add() 方法

Span<int> span = [1]; // stackalloc int[10];
ReadOnlySpan<char> span2 = "hello";

ArrayList arrayList = [1, true, "hello", null]; // Dictionary 目前还不支持:https://github.com/dotnet/csharplang/issues/7822

# region 元素展开
// JavaScript
// const arr1 = [1,2,3]
// const arr2 = [4,5,6]
// const arr3 = [...arr1,...arr2]

// Python
// arr1 = [1,2,3]
// arr2 = [4,5,6]
// arr3 = [*arr1, *arr2]

// C#
int[] arr1 = [1, 2,3 ];
int[] arr2 = [4, 5, 6];

int[] arr3 = [..arr1, .. from c in "nihao" select c,0,0, .. from l in "hello, world!" where char.IsLetter(l) select l,..arr2];
Console.WriteLine(arr3.Length);
// foreach (var item in arr3)
// {
//     Console.WriteLine(item);
// }

// 自定义集合类型的适配
MyCollection<int> myCollection = [1,2,3,4];

// Before:
public static class StringExtensions
{
    public static List<Query> QueryStringToList(this string queryString)
    {
        List<Query> queryList = (
            from queryPart in queryString.Split('&')
            let keyValue = queryPart.Split('=')
            where keyValue.Length is 2
            select new Query(keyValue[0], keyValue[1])
            ).ToList();
        
        return queryList;
    }
}

// After：
public static class MyStringExtensions
{
    public static List<Query> QueryStringToList(this string queryString) =>
    [
        .. from queryPart in queryString.Split('&')
            let keyValue = queryPart.Split('=')
            where keyValue.Length is 2
            select new Query(keyValue[0], keyValue[1])
    ];
}

public record class Query(string Name, string Value);
#endregion

#region 自定义集合类型的适配
class MyCollection<T> : IEnumerable
{
    private readonly List<T> _items = new();

    public IEnumerator GetEnumerator() => _items.GetEnumerator();
 
    public void Add(T item) => _items.Add(item);
}

#endregion