# <a href="#Xunit_Assert">Class Assert</a>

Namespace: [Xunit](Xunit.md)  
Assembly: xunit.v3.assert.dll  

Contains various static methods that are used to verify that conditions are met during the
process of running tests.

```csharp
public class Assert
```

#### Inheritance

[object](https://learn.microsoft.com/dotnet/api/system.object) ← 
[Assert](Xunit.Assert.md)

#### Inherited Members

[object.Equals\(object?\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\)), 
[object.Equals\(object?, object?\)](https://learn.microsoft.com/dotnet/api/system.object.equals\#system\-object\-equals\(system\-object\-system\-object\)), 
[object.GetHashCode\(\)](https://learn.microsoft.com/dotnet/api/system.object.gethashcode), 
[object.GetType\(\)](https://learn.microsoft.com/dotnet/api/system.object.gettype), 
[object.MemberwiseClone\(\)](https://learn.microsoft.com/dotnet/api/system.object.memberwiseclone), 
[object.ReferenceEquals\(object?, object?\)](https://learn.microsoft.com/dotnet/api/system.object.referenceequals), 
[object.ToString\(\)](https://learn.microsoft.com/dotnet/api/system.object.tostring)

## Constructors

### <a href="#Xunit_Assert__ctor">Assert\(\)</a>

Initializes a new instance of the <xref href="Xunit.Assert" data-throw-if-not-resolved="false"></xref> class.

```csharp
protected Assert()
```

## Methods

### <a href="#Xunit_Assert_All__1_System_Collections_Generic_IEnumerable___0__System_Action___0__">All<T\>\(IEnumerable<T\>, Action<T\>\)</a>

Verifies that all items in the collection pass when executed against
action.

```csharp
public static void All<T>(IEnumerable<T> collection, Action<T> action)
```

#### Parameters

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>

The collection

`action` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<T\>

The action to test each item against

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [AllException](Xunit.Sdk.AllException.md)

Thrown when the collection contains at least one non-matching element

### <a href="#Xunit_Assert_All__1_System_Collections_Generic_IEnumerable___0__System_Action___0_System_Int32__">All<T\>\(IEnumerable<T\>, Action<T, int\>\)</a>

Verifies that all items in the collection pass when executed against
action. The item index is provided to the action, in addition to the item.

```csharp
public static void All<T>(IEnumerable<T> collection, Action<T, int> action)
```

#### Parameters

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>

The collection

`action` [Action](https://learn.microsoft.com/dotnet/api/system.action\-2)<T, [int](https://learn.microsoft.com/dotnet/api/system.int32)\>

The action to test each item against

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [AllException](Xunit.Sdk.AllException.md)

Thrown when the collection contains at least one non-matching element

### <a href="#Xunit_Assert_AllAsync__1_System_Collections_Generic_IEnumerable___0__System_Func___0_System_Threading_Tasks_Task__">AllAsync<T\>\(IEnumerable<T\>, Func<T, Task\>\)</a>

Verifies that all items in the collection pass when executed against
action.

```csharp
public static Task AllAsync<T>(IEnumerable<T> collection, Func<T, Task> action)
```

#### Parameters

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>

The collection

`action` [Func](https://learn.microsoft.com/dotnet/api/system.func\-2)<T, [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)\>

The action to test each item against

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [AllException](Xunit.Sdk.AllException.md)

Thrown when the collection contains at least one non-matching element

### <a href="#Xunit_Assert_AllAsync__1_System_Collections_Generic_IEnumerable___0__System_Func___0_System_Int32_System_Threading_Tasks_Task__">AllAsync<T\>\(IEnumerable<T\>, Func<T, int, Task\>\)</a>

Verifies that all items in the collection pass when executed against
action. The item index is provided to the action, in addition to the item.

```csharp
public static Task AllAsync<T>(IEnumerable<T> collection, Func<T, int, Task> action)
```

#### Parameters

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>

The collection

`action` [Func](https://learn.microsoft.com/dotnet/api/system.func\-3)<T, [int](https://learn.microsoft.com/dotnet/api/system.int32), [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)\>

The action to test each item against

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [AllException](Xunit.Sdk.AllException.md)

Thrown when the collection contains at least one non-matching element

### <a href="#Xunit_Assert_Collection__1_System_Collections_Generic_IEnumerable___0__System_Action___0____">Collection<T\>\(IEnumerable<T\>, params Action<T\>\[\]\)</a>

Verifies that a collection contains exactly a given number of elements, which meet
the criteria provided by the element inspectors.

```csharp
public static void Collection<T>(IEnumerable<T> collection, params Action<T>[] elementInspectors)
```

#### Parameters

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>

The collection to be inspected

`elementInspectors` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<T\>\[\]

The element inspectors, which inspect each element in turn. The
    total number of element inspectors must exactly match the number of elements in the collection.

#### Type Parameters

`T` 

The type of the object to be verified

### <a href="#Xunit_Assert_CollectionAsync__1_System_Collections_Generic_IEnumerable___0__System_Func___0_System_Threading_Tasks_Task____">CollectionAsync<T\>\(IEnumerable<T\>, params Func<T, Task\>\[\]\)</a>

Verifies that a collection contains exactly a given number of elements, which meet
the criteria provided by the element inspectors.

```csharp
public static Task CollectionAsync<T>(IEnumerable<T> collection, params Func<T, Task>[] elementInspectors)
```

#### Parameters

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>

The collection to be inspected

`elementInspectors` [Func](https://learn.microsoft.com/dotnet/api/system.func\-2)<T, [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)\>\[\]

The element inspectors, which inspect each element in turn. The
    total number of element inspectors must exactly match the number of elements in the collection.

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

#### Type Parameters

`T` 

The type of the object to be verified

### <a href="#Xunit_Assert_Contains__1___0_System_Collections_Generic_IEnumerable___0__">Contains<T\>\(T, IEnumerable<T\>\)</a>

Verifies that a collection contains a given object.

```csharp
public static void Contains<T>(T expected, IEnumerable<T> collection)
```

#### Parameters

`expected` T

The object expected to be in the collection

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>

The collection to be inspected

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the collection

### <a href="#Xunit_Assert_Contains__1___0_System_Collections_Generic_IEnumerable___0__System_Collections_Generic_IEqualityComparer___0__">Contains<T\>\(T, IEnumerable<T\>, IEqualityComparer<T\>\)</a>

Verifies that a collection contains a given object, using an equality comparer.

```csharp
public static void Contains<T>(T expected, IEnumerable<T> collection, IEqualityComparer<T> comparer)
```

#### Parameters

`expected` T

The object expected to be in the collection

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>

The collection to be inspected

`comparer` [IEqualityComparer](https://learn.microsoft.com/dotnet/api/system.collections.generic.iequalitycomparer\-1)<T\>

The comparer used to equate objects in the collection with the expected object

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the collection

### <a href="#Xunit_Assert_Contains__1_System_Collections_Generic_IEnumerable___0__System_Predicate___0__">Contains<T\>\(IEnumerable<T\>, Predicate<T\>\)</a>

Verifies that a collection contains a given object.

```csharp
public static void Contains<T>(IEnumerable<T> collection, Predicate<T> filter)
```

#### Parameters

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>

The collection to be inspected

`filter` [Predicate](https://learn.microsoft.com/dotnet/api/system.predicate\-1)<T\>

The filter used to find the item you're ensuring the collection contains

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the collection

### <a href="#Xunit_Assert_Contains__2___0_System_Collections_Generic_IDictionary___0___1__">Contains<TKey, TValue\>\(TKey, IDictionary<TKey, TValue\>\)</a>

Verifies that a dictionary contains a given key.

```csharp
public static TValue Contains<TKey, TValue>(TKey expected, IDictionary<TKey, TValue> collection) where TKey : notnull
```

#### Parameters

`expected` TKey

The object expected to be in the collection.

`collection` [IDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary\-2)<TKey, TValue\>

The collection to be inspected.

#### Returns

 TValue

The value associated with <code class="paramref">expected</code>.

#### Type Parameters

`TKey` 

The type of the keys of the object to be verified.

`TValue` 

The type of the values of the object to be verified.

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the collection

### <a href="#Xunit_Assert_Contains__2___0_System_Collections_Generic_IReadOnlyDictionary___0___1__">Contains<TKey, TValue\>\(TKey, IReadOnlyDictionary<TKey, TValue\>\)</a>

Verifies that a read-only dictionary contains a given key.

```csharp
public static TValue Contains<TKey, TValue>(TKey expected, IReadOnlyDictionary<TKey, TValue> collection) where TKey : notnull
```

#### Parameters

`expected` TKey

The object expected to be in the collection.

`collection` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary\-2)<TKey, TValue\>

The collection to be inspected.

#### Returns

 TValue

The value associated with <code class="paramref">expected</code>.

#### Type Parameters

`TKey` 

The type of the keys of the object to be verified.

`TValue` 

The type of the values of the object to be verified.

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the collection

### <a href="#Xunit_Assert_Contains__2___0_System_Collections_Concurrent_ConcurrentDictionary___0___1__">Contains<TKey, TValue\>\(TKey, ConcurrentDictionary<TKey, TValue\>\)</a>

Verifies that a dictionary contains a given key.

```csharp
public static TValue Contains<TKey, TValue>(TKey expected, ConcurrentDictionary<TKey, TValue> collection) where TKey : notnull
```

#### Parameters

`expected` TKey

The object expected to be in the collection.

`collection` [ConcurrentDictionary](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentdictionary\-2)<TKey, TValue\>

The collection to be inspected.

#### Returns

 TValue

The value associated with <code class="paramref">expected</code>.

#### Type Parameters

`TKey` 

The type of the keys of the object to be verified.

`TValue` 

The type of the values of the object to be verified.

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the collection

### <a href="#Xunit_Assert_Contains__2___0_System_Collections_Generic_Dictionary___0___1__">Contains<TKey, TValue\>\(TKey, Dictionary<TKey, TValue\>\)</a>

Verifies that a dictionary contains a given key.

```csharp
public static TValue Contains<TKey, TValue>(TKey expected, Dictionary<TKey, TValue> collection) where TKey : notnull
```

#### Parameters

`expected` TKey

The object expected to be in the collection.

`collection` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<TKey, TValue\>

The collection to be inspected.

#### Returns

 TValue

The value associated with <code class="paramref">expected</code>.

#### Type Parameters

`TKey` 

The type of the keys of the object to be verified.

`TValue` 

The type of the values of the object to be verified.

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the collection

### <a href="#Xunit_Assert_Contains__2___0_System_Collections_ObjectModel_ReadOnlyDictionary___0___1__">Contains<TKey, TValue\>\(TKey, ReadOnlyDictionary<TKey, TValue\>\)</a>

Verifies that a dictionary contains a given key.

```csharp
public static TValue Contains<TKey, TValue>(TKey expected, ReadOnlyDictionary<TKey, TValue> collection) where TKey : notnull
```

#### Parameters

`expected` TKey

The object expected to be in the collection.

`collection` [ReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.objectmodel.readonlydictionary\-2)<TKey, TValue\>

The collection to be inspected.

#### Returns

 TValue

The value associated with <code class="paramref">expected</code>.

#### Type Parameters

`TKey` 

The type of the keys of the object to be verified.

`TValue` 

The type of the values of the object to be verified.

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the collection

### <a href="#Xunit_Assert_Contains__2___0_System_Collections_Immutable_ImmutableDictionary___0___1__">Contains<TKey, TValue\>\(TKey, ImmutableDictionary<TKey, TValue\>\)</a>

Verifies that a dictionary contains a given key.

```csharp
public static TValue Contains<TKey, TValue>(TKey expected, ImmutableDictionary<TKey, TValue> collection) where TKey : notnull
```

#### Parameters

`expected` TKey

The object expected to be in the collection.

`collection` [ImmutableDictionary](https://learn.microsoft.com/dotnet/api/system.collections.immutable.immutabledictionary\-2)<TKey, TValue\>

The collection to be inspected.

#### Returns

 TValue

The value associated with <code class="paramref">expected</code>.

#### Type Parameters

`TKey` 

The type of the keys of the object to be verified.

`TValue` 

The type of the values of the object to be verified.

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the collection

### <a href="#Xunit_Assert_Contains__1_System_Memory___0__System_Memory___0__">Contains<T\>\(Memory<T\>, Memory<T\>\)</a>

Verifies that a Memory contains a given sub-Memory

```csharp
public static void Contains<T>(Memory<T> expectedSubMemory, Memory<T> actualMemory) where T : IEquatable<T>
```

#### Parameters

`expectedSubMemory` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<T\>

The sub-Memory expected to be in the Memory

`actualMemory` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<T\>

The Memory to be inspected

#### Type Parameters

`T` 

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-Memory is not present inside the Memory

### <a href="#Xunit_Assert_Contains__1_System_Memory___0__System_ReadOnlyMemory___0__">Contains<T\>\(Memory<T\>, ReadOnlyMemory<T\>\)</a>

Verifies that a Memory contains a given sub-Memory

```csharp
public static void Contains<T>(Memory<T> expectedSubMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
```

#### Parameters

`expectedSubMemory` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<T\>

The sub-Memory expected to be in the Memory

`actualMemory` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<T\>

The Memory to be inspected

#### Type Parameters

`T` 

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-Memory is not present inside the Memory

### <a href="#Xunit_Assert_Contains__1_System_ReadOnlyMemory___0__System_Memory___0__">Contains<T\>\(ReadOnlyMemory<T\>, Memory<T\>\)</a>

Verifies that a Memory contains a given sub-Memory

```csharp
public static void Contains<T>(ReadOnlyMemory<T> expectedSubMemory, Memory<T> actualMemory) where T : IEquatable<T>
```

#### Parameters

`expectedSubMemory` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<T\>

The sub-Memory expected to be in the Memory

`actualMemory` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<T\>

The Memory to be inspected

#### Type Parameters

`T` 

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-Memory is not present inside the Memory

### <a href="#Xunit_Assert_Contains__1_System_ReadOnlyMemory___0__System_ReadOnlyMemory___0__">Contains<T\>\(ReadOnlyMemory<T\>, ReadOnlyMemory<T\>\)</a>

Verifies that a Memory contains a given sub-Memory

```csharp
public static void Contains<T>(ReadOnlyMemory<T> expectedSubMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
```

#### Parameters

`expectedSubMemory` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<T\>

The sub-Memory expected to be in the Memory

`actualMemory` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<T\>

The Memory to be inspected

#### Type Parameters

`T` 

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-Memory is not present inside the Memory

### <a href="#Xunit_Assert_Contains__1___0_System_Collections_Generic_ISet___0__">Contains<T\>\(T, ISet<T\>\)</a>

Verifies that the set contains the given object.

```csharp
public static void Contains<T>(T expected, ISet<T> set)
```

#### Parameters

`expected` T

The object expected to be in the set

`set` [ISet](https://learn.microsoft.com/dotnet/api/system.collections.generic.iset\-1)<T\>

The set to be inspected

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the set

### <a href="#Xunit_Assert_Contains__1___0_System_Collections_Generic_HashSet___0__">Contains<T\>\(T, HashSet<T\>\)</a>

Verifies that the hashset contains the given object.

```csharp
public static void Contains<T>(T expected, HashSet<T> set)
```

#### Parameters

`expected` T

The object expected to be in the set

`set` [HashSet](https://learn.microsoft.com/dotnet/api/system.collections.generic.hashset\-1)<T\>

The set to be inspected

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the set

### <a href="#Xunit_Assert_Contains__1___0_System_Collections_Generic_SortedSet___0__">Contains<T\>\(T, SortedSet<T\>\)</a>

Verifies that the sorted hashset contains the given object.

```csharp
public static void Contains<T>(T expected, SortedSet<T> set)
```

#### Parameters

`expected` T

The object expected to be in the set

`set` [SortedSet](https://learn.microsoft.com/dotnet/api/system.collections.generic.sortedset\-1)<T\>

The set to be inspected

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the set

### <a href="#Xunit_Assert_Contains__1___0_System_Collections_Immutable_ImmutableHashSet___0__">Contains<T\>\(T, ImmutableHashSet<T\>\)</a>

Verifies that the immutable hashset contains the given object.

```csharp
public static void Contains<T>(T expected, ImmutableHashSet<T> set)
```

#### Parameters

`expected` T

The object expected to be in the set

`set` [ImmutableHashSet](https://learn.microsoft.com/dotnet/api/system.collections.immutable.immutablehashset\-1)<T\>

The set to be inspected

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the set

### <a href="#Xunit_Assert_Contains__1___0_System_Collections_Immutable_ImmutableSortedSet___0__">Contains<T\>\(T, ImmutableSortedSet<T\>\)</a>

Verifies that the immutable sorted hashset contains the given object.

```csharp
public static void Contains<T>(T expected, ImmutableSortedSet<T> set)
```

#### Parameters

`expected` T

The object expected to be in the set

`set` [ImmutableSortedSet](https://learn.microsoft.com/dotnet/api/system.collections.immutable.immutablesortedset\-1)<T\>

The set to be inspected

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the set

### <a href="#Xunit_Assert_Contains__1_System_Span___0__System_Span___0__">Contains<T\>\(Span<T\>, Span<T\>\)</a>

Verifies that a span contains a given sub-span

```csharp
public static void Contains<T>(Span<T> expectedSubSpan, Span<T> actualSpan) where T : IEquatable<T>
```

#### Parameters

`expectedSubSpan` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<T\>

The sub-span expected to be in the span

`actualSpan` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<T\>

The span to be inspected

#### Type Parameters

`T` 

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-span is not present inside the span

### <a href="#Xunit_Assert_Contains__1_System_Span___0__System_ReadOnlySpan___0__">Contains<T\>\(Span<T\>, ReadOnlySpan<T\>\)</a>

Verifies that a span contains a given sub-span

```csharp
public static void Contains<T>(Span<T> expectedSubSpan, ReadOnlySpan<T> actualSpan) where T : IEquatable<T>
```

#### Parameters

`expectedSubSpan` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<T\>

The sub-span expected to be in the span

`actualSpan` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<T\>

The span to be inspected

#### Type Parameters

`T` 

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-span is not present inside the span

### <a href="#Xunit_Assert_Contains__1_System_ReadOnlySpan___0__System_Span___0__">Contains<T\>\(ReadOnlySpan<T\>, Span<T\>\)</a>

Verifies that a span contains a given sub-span

```csharp
public static void Contains<T>(ReadOnlySpan<T> expectedSubSpan, Span<T> actualSpan) where T : IEquatable<T>
```

#### Parameters

`expectedSubSpan` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<T\>

The sub-span expected to be in the span

`actualSpan` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<T\>

The span to be inspected

#### Type Parameters

`T` 

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-span is not present inside the span

### <a href="#Xunit_Assert_Contains__1_System_ReadOnlySpan___0__System_ReadOnlySpan___0__">Contains<T\>\(ReadOnlySpan<T\>, ReadOnlySpan<T\>\)</a>

Verifies that a span contains a given sub-span

```csharp
public static void Contains<T>(ReadOnlySpan<T> expectedSubSpan, ReadOnlySpan<T> actualSpan) where T : IEquatable<T>
```

#### Parameters

`expectedSubSpan` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<T\>

The sub-span expected to be in the span

`actualSpan` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<T\>

The span to be inspected

#### Type Parameters

`T` 

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-span is not present inside the span

### <a href="#Xunit_Assert_Contains_System_String_System_String_">Contains\(string, string?\)</a>

Verifies that a string contains a given sub-string, using the current culture.

```csharp
public static void Contains(string expectedSubstring, string? actualString)
```

#### Parameters

`expectedSubstring` [string](https://learn.microsoft.com/dotnet/api/system.string)

The sub-string expected to be in the string

`actualString` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The string to be inspected

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-string is not present inside the string

### <a href="#Xunit_Assert_Contains_System_String_System_String_System_StringComparison_">Contains\(string, string?, StringComparison\)</a>

Verifies that a string contains a given sub-string, using the given comparison type.

```csharp
public static void Contains(string expectedSubstring, string? actualString, StringComparison comparisonType)
```

#### Parameters

`expectedSubstring` [string](https://learn.microsoft.com/dotnet/api/system.string)

The sub-string expected to be in the string

`actualString` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-string is not present inside the string

### <a href="#Xunit_Assert_Contains_System_Memory_System_Char__System_Memory_System_Char__">Contains\(Memory<char\>, Memory<char\>\)</a>

Verifies that a string contains a given sub-string, using the current culture.

```csharp
public static void Contains(Memory<char> expectedSubstring, Memory<char> actualString)
```

#### Parameters

`expectedSubstring` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be in the string

`actualString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-string is not present inside the string

### <a href="#Xunit_Assert_Contains_System_Memory_System_Char__System_ReadOnlyMemory_System_Char__">Contains\(Memory<char\>, ReadOnlyMemory<char\>\)</a>

Verifies that a string contains a given sub-string, using the current culture.

```csharp
public static void Contains(Memory<char> expectedSubstring, ReadOnlyMemory<char> actualString)
```

#### Parameters

`expectedSubstring` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be in the string

`actualString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-string is not present inside the string

### <a href="#Xunit_Assert_Contains_System_ReadOnlyMemory_System_Char__System_Memory_System_Char__">Contains\(ReadOnlyMemory<char\>, Memory<char\>\)</a>

Verifies that a string contains a given sub-string, using the current culture.

```csharp
public static void Contains(ReadOnlyMemory<char> expectedSubstring, Memory<char> actualString)
```

#### Parameters

`expectedSubstring` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be in the string

`actualString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-string is not present inside the string

### <a href="#Xunit_Assert_Contains_System_ReadOnlyMemory_System_Char__System_ReadOnlyMemory_System_Char__">Contains\(ReadOnlyMemory<char\>, ReadOnlyMemory<char\>\)</a>

Verifies that a string contains a given sub-string, using the current culture.

```csharp
public static void Contains(ReadOnlyMemory<char> expectedSubstring, ReadOnlyMemory<char> actualString)
```

#### Parameters

`expectedSubstring` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be in the string

`actualString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-string is not present inside the string

### <a href="#Xunit_Assert_Contains_System_Memory_System_Char__System_Memory_System_Char__System_StringComparison_">Contains\(Memory<char\>, Memory<char\>, StringComparison\)</a>

Verifies that a string contains a given sub-string, using the given comparison type.

```csharp
public static void Contains(Memory<char> expectedSubstring, Memory<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedSubstring` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be in the string

`actualString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-string is not present inside the string

### <a href="#Xunit_Assert_Contains_System_Memory_System_Char__System_ReadOnlyMemory_System_Char__System_StringComparison_">Contains\(Memory<char\>, ReadOnlyMemory<char\>, StringComparison\)</a>

Verifies that a string contains a given sub-string, using the given comparison type.

```csharp
public static void Contains(Memory<char> expectedSubstring, ReadOnlyMemory<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedSubstring` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be in the string

`actualString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-string is not present inside the string

### <a href="#Xunit_Assert_Contains_System_ReadOnlyMemory_System_Char__System_Memory_System_Char__System_StringComparison_">Contains\(ReadOnlyMemory<char\>, Memory<char\>, StringComparison\)</a>

Verifies that a string contains a given sub-string, using the given comparison type.

```csharp
public static void Contains(ReadOnlyMemory<char> expectedSubstring, Memory<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedSubstring` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be in the string

`actualString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-string is not present inside the string

### <a href="#Xunit_Assert_Contains_System_ReadOnlyMemory_System_Char__System_ReadOnlyMemory_System_Char__System_StringComparison_">Contains\(ReadOnlyMemory<char\>, ReadOnlyMemory<char\>, StringComparison\)</a>

Verifies that a string contains a given sub-string, using the given comparison type.

```csharp
public static void Contains(ReadOnlyMemory<char> expectedSubstring, ReadOnlyMemory<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedSubstring` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be in the string

`actualString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the sub-string is not present inside the string

### <a href="#Xunit_Assert_Contains_System_Span_System_Char__System_Span_System_Char__System_StringComparison_">Contains\(Span<char\>, Span<char\>, StringComparison\)</a>

Verifies that a string contains a given string, using the given comparison type.

```csharp
public static void Contains(Span<char> expectedSubstring, Span<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedSubstring` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string expected to be in the string

`actualString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the string is not present inside the string

### <a href="#Xunit_Assert_Contains_System_Span_System_Char__System_ReadOnlySpan_System_Char__System_StringComparison_">Contains\(Span<char\>, ReadOnlySpan<char\>, StringComparison\)</a>

Verifies that a string contains a given string, using the given comparison type.

```csharp
public static void Contains(Span<char> expectedSubstring, ReadOnlySpan<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedSubstring` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string expected to be in the string

`actualString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the string is not present inside the string

### <a href="#Xunit_Assert_Contains_System_ReadOnlySpan_System_Char__System_Span_System_Char__System_StringComparison_">Contains\(ReadOnlySpan<char\>, Span<char\>, StringComparison\)</a>

Verifies that a string contains a given string, using the given comparison type.

```csharp
public static void Contains(ReadOnlySpan<char> expectedSubstring, Span<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedSubstring` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string expected to be in the string

`actualString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the string is not present inside the string

### <a href="#Xunit_Assert_Contains_System_ReadOnlySpan_System_Char__System_ReadOnlySpan_System_Char__System_StringComparison_">Contains\(ReadOnlySpan<char\>, ReadOnlySpan<char\>, StringComparison\)</a>

Verifies that a string contains a given string, using the given comparison type.

```csharp
public static void Contains(ReadOnlySpan<char> expectedSubstring, ReadOnlySpan<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedSubstring` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string expected to be in the string

`actualString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the string is not present inside the string

### <a href="#Xunit_Assert_Contains_System_Span_System_Char__System_Span_System_Char__">Contains\(Span<char\>, Span<char\>\)</a>

Verifies that a string contains a given string, using the current culture.

```csharp
public static void Contains(Span<char> expectedSubstring, Span<char> actualString)
```

#### Parameters

`expectedSubstring` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string expected to be in the string

`actualString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the string is not present inside the string

### <a href="#Xunit_Assert_Contains_System_Span_System_Char__System_ReadOnlySpan_System_Char__">Contains\(Span<char\>, ReadOnlySpan<char\>\)</a>

Verifies that a string contains a given string, using the current culture.

```csharp
public static void Contains(Span<char> expectedSubstring, ReadOnlySpan<char> actualString)
```

#### Parameters

`expectedSubstring` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string expected to be in the string

`actualString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the string is not present inside the string

### <a href="#Xunit_Assert_Contains_System_ReadOnlySpan_System_Char__System_Span_System_Char__">Contains\(ReadOnlySpan<char\>, Span<char\>\)</a>

Verifies that a string contains a given string, using the current culture.

```csharp
public static void Contains(ReadOnlySpan<char> expectedSubstring, Span<char> actualString)
```

#### Parameters

`expectedSubstring` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string expected to be in the string

`actualString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the string is not present inside the string

### <a href="#Xunit_Assert_Contains_System_ReadOnlySpan_System_Char__System_ReadOnlySpan_System_Char__">Contains\(ReadOnlySpan<char\>, ReadOnlySpan<char\>\)</a>

Verifies that a string contains a given string, using the current culture.

```csharp
public static void Contains(ReadOnlySpan<char> expectedSubstring, ReadOnlySpan<char> actualString)
```

#### Parameters

`expectedSubstring` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string expected to be in the string

`actualString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the string is not present inside the string

### <a href="#Xunit_Assert_Distinct__1_System_Collections_Generic_IEnumerable___0__">Distinct<T\>\(IEnumerable<T\>\)</a>

Verifies that a collection contains each object only once.

```csharp
public static void Distinct<T>(IEnumerable<T> collection)
```

#### Parameters

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>

The collection to be inspected

#### Type Parameters

`T` 

The type of the object to be compared

#### Exceptions

 [DistinctException](Xunit.Sdk.DistinctException.md)

Thrown when an object is present inside the collection more than once

### <a href="#Xunit_Assert_Distinct__1_System_Collections_Generic_IEnumerable___0__System_Collections_Generic_IEqualityComparer___0__">Distinct<T\>\(IEnumerable<T\>, IEqualityComparer<T\>\)</a>

Verifies that a collection contains each object only once.

```csharp
public static void Distinct<T>(IEnumerable<T> collection, IEqualityComparer<T> comparer)
```

#### Parameters

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>

The collection to be inspected

`comparer` [IEqualityComparer](https://learn.microsoft.com/dotnet/api/system.collections.generic.iequalitycomparer\-1)<T\>

The comparer used to equate objects in the collection with the expected object

#### Type Parameters

`T` 

The type of the object to be compared

#### Exceptions

 [DistinctException](Xunit.Sdk.DistinctException.md)

Thrown when an object is present inside the collection more than once

### <a href="#Xunit_Assert_DoesNotContain__1___0_System_Collections_Generic_IEnumerable___0__">DoesNotContain<T\>\(T, IEnumerable<T\>\)</a>

Verifies that a collection does not contain a given object.

```csharp
public static void DoesNotContain<T>(T expected, IEnumerable<T> collection)
```

#### Parameters

`expected` T

The object that is expected not to be in the collection

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>

The collection to be inspected

#### Type Parameters

`T` 

The type of the object to be compared

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the object is present inside the collection

### <a href="#Xunit_Assert_DoesNotContain__1___0_System_Collections_Generic_IEnumerable___0__System_Collections_Generic_IEqualityComparer___0__">DoesNotContain<T\>\(T, IEnumerable<T\>, IEqualityComparer<T\>\)</a>

Verifies that a collection does not contain a given object, using an equality comparer.

```csharp
public static void DoesNotContain<T>(T expected, IEnumerable<T> collection, IEqualityComparer<T> comparer)
```

#### Parameters

`expected` T

The object that is expected not to be in the collection

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>

The collection to be inspected

`comparer` [IEqualityComparer](https://learn.microsoft.com/dotnet/api/system.collections.generic.iequalitycomparer\-1)<T\>

The comparer used to equate objects in the collection with the expected object

#### Type Parameters

`T` 

The type of the object to be compared

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the object is present inside the collection

### <a href="#Xunit_Assert_DoesNotContain__1_System_Collections_Generic_IEnumerable___0__System_Predicate___0__">DoesNotContain<T\>\(IEnumerable<T\>, Predicate<T\>\)</a>

Verifies that a collection does not contain a given object.

```csharp
public static void DoesNotContain<T>(IEnumerable<T> collection, Predicate<T> filter)
```

#### Parameters

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>

The collection to be inspected

`filter` [Predicate](https://learn.microsoft.com/dotnet/api/system.predicate\-1)<T\>

The filter used to find the item you're ensuring the collection does not contain

#### Type Parameters

`T` 

The type of the object to be compared

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the object is present inside the collection

### <a href="#Xunit_Assert_DoesNotContain__2___0_System_Collections_Generic_IDictionary___0___1__">DoesNotContain<TKey, TValue\>\(TKey, IDictionary<TKey, TValue\>\)</a>

Verifies that a dictionary does not contain a given key.

```csharp
public static void DoesNotContain<TKey, TValue>(TKey expected, IDictionary<TKey, TValue> collection) where TKey : notnull
```

#### Parameters

`expected` TKey

The object expected to be in the collection.

`collection` [IDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.idictionary\-2)<TKey, TValue\>

The collection to be inspected.

#### Type Parameters

`TKey` 

The type of the keys of the object to be verified.

`TValue` 

The type of the values of the object to be verified.

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the object is present in the collection

### <a href="#Xunit_Assert_DoesNotContain__2___0_System_Collections_Generic_IReadOnlyDictionary___0___1__">DoesNotContain<TKey, TValue\>\(TKey, IReadOnlyDictionary<TKey, TValue\>\)</a>

Verifies that a dictionary does not contain a given key.

```csharp
public static void DoesNotContain<TKey, TValue>(TKey expected, IReadOnlyDictionary<TKey, TValue> collection) where TKey : notnull
```

#### Parameters

`expected` TKey

The object expected to be in the collection.

`collection` [IReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.ireadonlydictionary\-2)<TKey, TValue\>

The collection to be inspected.

#### Type Parameters

`TKey` 

The type of the keys of the object to be verified.

`TValue` 

The type of the values of the object to be verified.

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the object is present in the collection

### <a href="#Xunit_Assert_DoesNotContain__2___0_System_Collections_Concurrent_ConcurrentDictionary___0___1__">DoesNotContain<TKey, TValue\>\(TKey, ConcurrentDictionary<TKey, TValue\>\)</a>

Verifies that a dictionary does not contain a given key.

```csharp
public static void DoesNotContain<TKey, TValue>(TKey expected, ConcurrentDictionary<TKey, TValue> collection) where TKey : notnull
```

#### Parameters

`expected` TKey

The object expected to be in the collection.

`collection` [ConcurrentDictionary](https://learn.microsoft.com/dotnet/api/system.collections.concurrent.concurrentdictionary\-2)<TKey, TValue\>

The collection to be inspected.

#### Type Parameters

`TKey` 

The type of the keys of the object to be verified.

`TValue` 

The type of the values of the object to be verified.

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the object is present in the collection

### <a href="#Xunit_Assert_DoesNotContain__2___0_System_Collections_Generic_Dictionary___0___1__">DoesNotContain<TKey, TValue\>\(TKey, Dictionary<TKey, TValue\>\)</a>

Verifies that a dictionary does not contain a given key.

```csharp
public static void DoesNotContain<TKey, TValue>(TKey expected, Dictionary<TKey, TValue> collection) where TKey : notnull
```

#### Parameters

`expected` TKey

The object expected to be in the collection.

`collection` [Dictionary](https://learn.microsoft.com/dotnet/api/system.collections.generic.dictionary\-2)<TKey, TValue\>

The collection to be inspected.

#### Type Parameters

`TKey` 

The type of the keys of the object to be verified.

`TValue` 

The type of the values of the object to be verified.

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the object is present in the collection

### <a href="#Xunit_Assert_DoesNotContain__2___0_System_Collections_ObjectModel_ReadOnlyDictionary___0___1__">DoesNotContain<TKey, TValue\>\(TKey, ReadOnlyDictionary<TKey, TValue\>\)</a>

Verifies that a dictionary does not contain a given key.

```csharp
public static void DoesNotContain<TKey, TValue>(TKey expected, ReadOnlyDictionary<TKey, TValue> collection) where TKey : notnull
```

#### Parameters

`expected` TKey

The object expected to be in the collection.

`collection` [ReadOnlyDictionary](https://learn.microsoft.com/dotnet/api/system.collections.objectmodel.readonlydictionary\-2)<TKey, TValue\>

The collection to be inspected.

#### Type Parameters

`TKey` 

The type of the keys of the object to be verified.

`TValue` 

The type of the values of the object to be verified.

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the object is present in the collection

### <a href="#Xunit_Assert_DoesNotContain__2___0_System_Collections_Immutable_ImmutableDictionary___0___1__">DoesNotContain<TKey, TValue\>\(TKey, ImmutableDictionary<TKey, TValue\>\)</a>

Verifies that a dictionary does not contain a given key.

```csharp
public static void DoesNotContain<TKey, TValue>(TKey expected, ImmutableDictionary<TKey, TValue> collection) where TKey : notnull
```

#### Parameters

`expected` TKey

The object expected to be in the collection.

`collection` [ImmutableDictionary](https://learn.microsoft.com/dotnet/api/system.collections.immutable.immutabledictionary\-2)<TKey, TValue\>

The collection to be inspected.

#### Type Parameters

`TKey` 

The type of the keys of the object to be verified.

`TValue` 

The type of the values of the object to be verified.

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the object is present in the collection

### <a href="#Xunit_Assert_DoesNotContain__1_System_Memory___0__System_Memory___0__">DoesNotContain<T\>\(Memory<T\>, Memory<T\>\)</a>

Verifies that a Memory does not contain a given sub-Memory

```csharp
public static void DoesNotContain<T>(Memory<T> expectedSubMemory, Memory<T> actualMemory) where T : IEquatable<T>
```

#### Parameters

`expectedSubMemory` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<T\>

The sub-Memory expected not to be in the Memory

`actualMemory` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<T\>

The Memory to be inspected

#### Type Parameters

`T` 

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-Memory is present inside the Memory

### <a href="#Xunit_Assert_DoesNotContain__1_System_Memory___0__System_ReadOnlyMemory___0__">DoesNotContain<T\>\(Memory<T\>, ReadOnlyMemory<T\>\)</a>

Verifies that a Memory does not contain a given sub-Memory

```csharp
public static void DoesNotContain<T>(Memory<T> expectedSubMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
```

#### Parameters

`expectedSubMemory` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<T\>

The sub-Memory expected not to be in the Memory

`actualMemory` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<T\>

The Memory to be inspected

#### Type Parameters

`T` 

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-Memory is present inside the Memory

### <a href="#Xunit_Assert_DoesNotContain__1_System_ReadOnlyMemory___0__System_Memory___0__">DoesNotContain<T\>\(ReadOnlyMemory<T\>, Memory<T\>\)</a>

Verifies that a Memory does not contain a given sub-Memory

```csharp
public static void DoesNotContain<T>(ReadOnlyMemory<T> expectedSubMemory, Memory<T> actualMemory) where T : IEquatable<T>
```

#### Parameters

`expectedSubMemory` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<T\>

The sub-Memory expected not to be in the Memory

`actualMemory` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<T\>

The Memory to be inspected

#### Type Parameters

`T` 

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-Memory is present inside the Memory

### <a href="#Xunit_Assert_DoesNotContain__1_System_ReadOnlyMemory___0__System_ReadOnlyMemory___0__">DoesNotContain<T\>\(ReadOnlyMemory<T\>, ReadOnlyMemory<T\>\)</a>

Verifies that a Memory does not contain a given sub-Memory

```csharp
public static void DoesNotContain<T>(ReadOnlyMemory<T> expectedSubMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
```

#### Parameters

`expectedSubMemory` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<T\>

The sub-Memory expected not to be in the Memory

`actualMemory` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<T\>

The Memory to be inspected

#### Type Parameters

`T` 

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-Memory is present inside the Memory

### <a href="#Xunit_Assert_DoesNotContain__1___0_System_Collections_Generic_ISet___0__">DoesNotContain<T\>\(T, ISet<T\>\)</a>

Verifies that the set does not contain the given item.

```csharp
public static void DoesNotContain<T>(T expected, ISet<T> set)
```

#### Parameters

`expected` T

The object that is expected not to be in the set

`set` [ISet](https://learn.microsoft.com/dotnet/api/system.collections.generic.iset\-1)<T\>

The set to be inspected

#### Type Parameters

`T` 

The type of the object to be compared

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the object is present inside the set

### <a href="#Xunit_Assert_DoesNotContain__1___0_System_Collections_Generic_HashSet___0__">DoesNotContain<T\>\(T, HashSet<T\>\)</a>

Verifies that the hashset does not contain the given item.

```csharp
public static void DoesNotContain<T>(T expected, HashSet<T> set)
```

#### Parameters

`expected` T

The object expected to be in the set

`set` [HashSet](https://learn.microsoft.com/dotnet/api/system.collections.generic.hashset\-1)<T\>

The set to be inspected

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the set

### <a href="#Xunit_Assert_DoesNotContain__1___0_System_Collections_Generic_SortedSet___0__">DoesNotContain<T\>\(T, SortedSet<T\>\)</a>

Verifies that the sorted hashset does not contain the given item.

```csharp
public static void DoesNotContain<T>(T expected, SortedSet<T> set)
```

#### Parameters

`expected` T

The object expected to be in the set

`set` [SortedSet](https://learn.microsoft.com/dotnet/api/system.collections.generic.sortedset\-1)<T\>

The set to be inspected

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the set

### <a href="#Xunit_Assert_DoesNotContain__1___0_System_Collections_Immutable_ImmutableHashSet___0__">DoesNotContain<T\>\(T, ImmutableHashSet<T\>\)</a>

Verifies that the immutable hashset does not contain the given item.

```csharp
public static void DoesNotContain<T>(T expected, ImmutableHashSet<T> set)
```

#### Parameters

`expected` T

The object expected to be in the set

`set` [ImmutableHashSet](https://learn.microsoft.com/dotnet/api/system.collections.immutable.immutablehashset\-1)<T\>

The set to be inspected

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the set

### <a href="#Xunit_Assert_DoesNotContain__1___0_System_Collections_Immutable_ImmutableSortedSet___0__">DoesNotContain<T\>\(T, ImmutableSortedSet<T\>\)</a>

Verifies that the immutable sorted hashset does not contain the given item.

```csharp
public static void DoesNotContain<T>(T expected, ImmutableSortedSet<T> set)
```

#### Parameters

`expected` T

The object expected to be in the set

`set` [ImmutableSortedSet](https://learn.microsoft.com/dotnet/api/system.collections.immutable.immutablesortedset\-1)<T\>

The set to be inspected

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the object is not present in the set

### <a href="#Xunit_Assert_DoesNotContain__1_System_Span___0__System_Span___0__">DoesNotContain<T\>\(Span<T\>, Span<T\>\)</a>

Verifies that a span does not contain a given sub-span

```csharp
public static void DoesNotContain<T>(Span<T> expectedSubSpan, Span<T> actualSpan) where T : IEquatable<T>
```

#### Parameters

`expectedSubSpan` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<T\>

The sub-span expected not to be in the span

`actualSpan` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<T\>

The span to be inspected

#### Type Parameters

`T` 

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-span is present inside the span

### <a href="#Xunit_Assert_DoesNotContain__1_System_Span___0__System_ReadOnlySpan___0__">DoesNotContain<T\>\(Span<T\>, ReadOnlySpan<T\>\)</a>

Verifies that a span does not contain a given sub-span

```csharp
public static void DoesNotContain<T>(Span<T> expectedSubSpan, ReadOnlySpan<T> actualSpan) where T : IEquatable<T>
```

#### Parameters

`expectedSubSpan` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<T\>

The sub-span expected not to be in the span

`actualSpan` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<T\>

The span to be inspected

#### Type Parameters

`T` 

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-span is present inside the span

### <a href="#Xunit_Assert_DoesNotContain__1_System_ReadOnlySpan___0__System_Span___0__">DoesNotContain<T\>\(ReadOnlySpan<T\>, Span<T\>\)</a>

Verifies that a span does not contain a given sub-span

```csharp
public static void DoesNotContain<T>(ReadOnlySpan<T> expectedSubSpan, Span<T> actualSpan) where T : IEquatable<T>
```

#### Parameters

`expectedSubSpan` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<T\>

The sub-span expected not to be in the span

`actualSpan` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<T\>

The span to be inspected

#### Type Parameters

`T` 

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-span is present inside the span

### <a href="#Xunit_Assert_DoesNotContain__1_System_ReadOnlySpan___0__System_ReadOnlySpan___0__">DoesNotContain<T\>\(ReadOnlySpan<T\>, ReadOnlySpan<T\>\)</a>

Verifies that a span does not contain a given sub-span

```csharp
public static void DoesNotContain<T>(ReadOnlySpan<T> expectedSubSpan, ReadOnlySpan<T> actualSpan) where T : IEquatable<T>
```

#### Parameters

`expectedSubSpan` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<T\>

The sub-span expected not to be in the span

`actualSpan` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<T\>

The span to be inspected

#### Type Parameters

`T` 

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-span is present inside the span

### <a href="#Xunit_Assert_DoesNotContain_System_String_System_String_">DoesNotContain\(string, string?\)</a>

Verifies that a string does not contain a given sub-string, using the current culture.

```csharp
public static void DoesNotContain(string expectedSubstring, string? actualString)
```

#### Parameters

`expectedSubstring` [string](https://learn.microsoft.com/dotnet/api/system.string)

The sub-string expected not to be in the string

`actualString` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The string to be inspected

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotContain_System_String_System_String_System_StringComparison_">DoesNotContain\(string, string?, StringComparison\)</a>

Verifies that a string does not contain a given sub-string, using the current culture.

```csharp
public static void DoesNotContain(string expectedSubstring, string? actualString, StringComparison comparisonType)
```

#### Parameters

`expectedSubstring` [string](https://learn.microsoft.com/dotnet/api/system.string)

The sub-string expected not to be in the string

`actualString` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotContain_System_Memory_System_Char__System_Memory_System_Char__">DoesNotContain\(Memory<char\>, Memory<char\>\)</a>

Verifies that a string does not contain a given sub-string, using the current culture.

```csharp
public static void DoesNotContain(Memory<char> expectedSubstring, Memory<char> actualString)
```

#### Parameters

`expectedSubstring` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected not to be in the string

`actualString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotContain_System_Memory_System_Char__System_ReadOnlyMemory_System_Char__">DoesNotContain\(Memory<char\>, ReadOnlyMemory<char\>\)</a>

Verifies that a string does not contain a given sub-string, using the current culture.

```csharp
public static void DoesNotContain(Memory<char> expectedSubstring, ReadOnlyMemory<char> actualString)
```

#### Parameters

`expectedSubstring` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected not to be in the string

`actualString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotContain_System_ReadOnlyMemory_System_Char__System_Memory_System_Char__">DoesNotContain\(ReadOnlyMemory<char\>, Memory<char\>\)</a>

Verifies that a string does not contain a given sub-string, using the current culture.

```csharp
public static void DoesNotContain(ReadOnlyMemory<char> expectedSubstring, Memory<char> actualString)
```

#### Parameters

`expectedSubstring` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected not to be in the string

`actualString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotContain_System_ReadOnlyMemory_System_Char__System_ReadOnlyMemory_System_Char__">DoesNotContain\(ReadOnlyMemory<char\>, ReadOnlyMemory<char\>\)</a>

Verifies that a string does not contain a given sub-string, using the current culture.

```csharp
public static void DoesNotContain(ReadOnlyMemory<char> expectedSubstring, ReadOnlyMemory<char> actualString)
```

#### Parameters

`expectedSubstring` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected not to be in the string

`actualString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotContain_System_Memory_System_Char__System_Memory_System_Char__System_StringComparison_">DoesNotContain\(Memory<char\>, Memory<char\>, StringComparison\)</a>

Verifies that a string does not contain a given sub-string, using the given comparison type.

```csharp
public static void DoesNotContain(Memory<char> expectedSubstring, Memory<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedSubstring` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected not to be in the string

`actualString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotContain_System_Memory_System_Char__System_ReadOnlyMemory_System_Char__System_StringComparison_">DoesNotContain\(Memory<char\>, ReadOnlyMemory<char\>, StringComparison\)</a>

Verifies that a string does not contain a given sub-string, using the given comparison type.

```csharp
public static void DoesNotContain(Memory<char> expectedSubstring, ReadOnlyMemory<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedSubstring` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected not to be in the string

`actualString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotContain_System_ReadOnlyMemory_System_Char__System_Memory_System_Char__System_StringComparison_">DoesNotContain\(ReadOnlyMemory<char\>, Memory<char\>, StringComparison\)</a>

Verifies that a string does not contain a given sub-string, using the given comparison type.

```csharp
public static void DoesNotContain(ReadOnlyMemory<char> expectedSubstring, Memory<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedSubstring` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected not to be in the string

`actualString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotContain_System_ReadOnlyMemory_System_Char__System_ReadOnlyMemory_System_Char__System_StringComparison_">DoesNotContain\(ReadOnlyMemory<char\>, ReadOnlyMemory<char\>, StringComparison\)</a>

Verifies that a string does not contain a given sub-string, using the given comparison type.

```csharp
public static void DoesNotContain(ReadOnlyMemory<char> expectedSubstring, ReadOnlyMemory<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedSubstring` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected not to be in the string

`actualString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotContain_System_Span_System_Char__System_Span_System_Char__System_StringComparison_">DoesNotContain\(Span<char\>, Span<char\>, StringComparison\)</a>

Verifies that a string does not contain a given sub-string, using the given comparison type.

```csharp
public static void DoesNotContain(Span<char> expectedSubstring, Span<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedSubstring` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected not to be in the string

`actualString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotContain_System_Span_System_Char__System_ReadOnlySpan_System_Char__System_StringComparison_">DoesNotContain\(Span<char\>, ReadOnlySpan<char\>, StringComparison\)</a>

Verifies that a string does not contain a given sub-string, using the given comparison type.

```csharp
public static void DoesNotContain(Span<char> expectedSubstring, ReadOnlySpan<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedSubstring` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected not to be in the string

`actualString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotContain_System_ReadOnlySpan_System_Char__System_Span_System_Char__System_StringComparison_">DoesNotContain\(ReadOnlySpan<char\>, Span<char\>, StringComparison\)</a>

Verifies that a string does not contain a given sub-string, using the given comparison type.

```csharp
public static void DoesNotContain(ReadOnlySpan<char> expectedSubstring, Span<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedSubstring` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected not to be in the string

`actualString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotContain_System_ReadOnlySpan_System_Char__System_ReadOnlySpan_System_Char__System_StringComparison_">DoesNotContain\(ReadOnlySpan<char\>, ReadOnlySpan<char\>, StringComparison\)</a>

Verifies that a string does not contain a given sub-string, using the given comparison type.

```csharp
public static void DoesNotContain(ReadOnlySpan<char> expectedSubstring, ReadOnlySpan<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedSubstring` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected not to be in the string

`actualString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotContain_System_Span_System_Char__System_Span_System_Char__">DoesNotContain\(Span<char\>, Span<char\>\)</a>

Verifies that a string does not contain a given sub-string, using the current culture.

```csharp
public static void DoesNotContain(Span<char> expectedSubstring, Span<char> actualString)
```

#### Parameters

`expectedSubstring` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected not to be in the string

`actualString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotContain_System_Span_System_Char__System_ReadOnlySpan_System_Char__">DoesNotContain\(Span<char\>, ReadOnlySpan<char\>\)</a>

Verifies that a string does not contain a given sub-string, using the current culture.

```csharp
public static void DoesNotContain(Span<char> expectedSubstring, ReadOnlySpan<char> actualString)
```

#### Parameters

`expectedSubstring` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected not to be in the string

`actualString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotContain_System_ReadOnlySpan_System_Char__System_Span_System_Char__">DoesNotContain\(ReadOnlySpan<char\>, Span<char\>\)</a>

Verifies that a string does not contain a given sub-string, using the current culture.

```csharp
public static void DoesNotContain(ReadOnlySpan<char> expectedSubstring, Span<char> actualString)
```

#### Parameters

`expectedSubstring` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected not to be in the string

`actualString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotContain_System_ReadOnlySpan_System_Char__System_ReadOnlySpan_System_Char__">DoesNotContain\(ReadOnlySpan<char\>, ReadOnlySpan<char\>\)</a>

Verifies that a string does not contain a given sub-string, using the current culture.

```csharp
public static void DoesNotContain(ReadOnlySpan<char> expectedSubstring, ReadOnlySpan<char> actualString)
```

#### Parameters

`expectedSubstring` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected not to be in the string

`actualString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [DoesNotContainException](Xunit.Sdk.DoesNotContainException.md)

Thrown when the sub-string is present inside the string

### <a href="#Xunit_Assert_DoesNotMatch_System_String_System_String_">DoesNotMatch\(string, string?\)</a>

Verifies that a string does not match a regular expression.

```csharp
public static void DoesNotMatch(string expectedRegexPattern, string? actualString)
```

#### Parameters

`expectedRegexPattern` [string](https://learn.microsoft.com/dotnet/api/system.string)

The regex pattern expected not to match

`actualString` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The string to be inspected

#### Exceptions

 [DoesNotMatchException](Xunit.Sdk.DoesNotMatchException.md)

Thrown when the string matches the regex pattern

### <a href="#Xunit_Assert_DoesNotMatch_System_Text_RegularExpressions_Regex_System_String_">DoesNotMatch\(Regex, string?\)</a>

Verifies that a string does not match a regular expression.

```csharp
public static void DoesNotMatch(Regex expectedRegex, string? actualString)
```

#### Parameters

`expectedRegex` [Regex](https://learn.microsoft.com/dotnet/api/system.text.regularexpressions.regex)

The regex expected not to match

`actualString` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The string to be inspected

#### Exceptions

 [DoesNotMatchException](Xunit.Sdk.DoesNotMatchException.md)

Thrown when the string matches the regex

### <a href="#Xunit_Assert_Empty_System_Collections_IEnumerable_">Empty\(IEnumerable\)</a>

Verifies that a collection is empty.

```csharp
public static void Empty(IEnumerable collection)
```

#### Parameters

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.ienumerable)

The collection to be inspected

#### Exceptions

 [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)

Thrown when the collection is null

 [EmptyException](Xunit.Sdk.EmptyException.md)

Thrown when the collection is not empty

### <a href="#Xunit_Assert_Empty_System_String_">Empty\(string\)</a>

Verifies that a string is empty.

```csharp
public static void Empty(string value)
```

#### Parameters

`value` [string](https://learn.microsoft.com/dotnet/api/system.string)

The string value to be inspected

#### Exceptions

 [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)

Thrown when the string is null

 [EmptyException](Xunit.Sdk.EmptyException.md)

Thrown when the string is not empty

### <a href="#Xunit_Assert_EndsWith_System_String_System_String_">EndsWith\(string?, string?\)</a>

Verifies that a string ends with a given sub-string, using the current culture.

```csharp
public static void EndsWith(string? expectedEndString, string? actualString)
```

#### Parameters

`expectedEndString` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The sub-string expected to be at the end of the string

`actualString` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The string to be inspected

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_EndsWith_System_String_System_String_System_StringComparison_">EndsWith\(string?, string?, StringComparison\)</a>

Verifies that a string ends with a given sub-string, using the given comparison type.

```csharp
public static void EndsWith(string? expectedEndString, string? actualString, StringComparison comparisonType)
```

#### Parameters

`expectedEndString` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The sub-string expected to be at the end of the string

`actualString` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_EndsWith_System_Memory_System_Char__System_Memory_System_Char__">EndsWith\(Memory<char\>, Memory<char\>\)</a>

Verifies that a string ends with a given sub-string, using the current culture.

```csharp
public static void EndsWith(Memory<char> expectedEndString, Memory<char> actualString)
```

#### Parameters

`expectedEndString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the end of the string

`actualString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [EndsWithException](Xunit.Sdk.EndsWithException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_EndsWith_System_Memory_System_Char__System_ReadOnlyMemory_System_Char__">EndsWith\(Memory<char\>, ReadOnlyMemory<char\>\)</a>

Verifies that a string ends with a given sub-string, using the current culture.

```csharp
public static void EndsWith(Memory<char> expectedEndString, ReadOnlyMemory<char> actualString)
```

#### Parameters

`expectedEndString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the end of the string

`actualString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [EndsWithException](Xunit.Sdk.EndsWithException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_EndsWith_System_ReadOnlyMemory_System_Char__System_Memory_System_Char__">EndsWith\(ReadOnlyMemory<char\>, Memory<char\>\)</a>

Verifies that a string ends with a given sub-string, using the current culture.

```csharp
public static void EndsWith(ReadOnlyMemory<char> expectedEndString, Memory<char> actualString)
```

#### Parameters

`expectedEndString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the end of the string

`actualString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [EndsWithException](Xunit.Sdk.EndsWithException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_EndsWith_System_ReadOnlyMemory_System_Char__System_ReadOnlyMemory_System_Char__">EndsWith\(ReadOnlyMemory<char\>, ReadOnlyMemory<char\>\)</a>

Verifies that a string ends with a given sub-string, using the current culture.

```csharp
public static void EndsWith(ReadOnlyMemory<char> expectedEndString, ReadOnlyMemory<char> actualString)
```

#### Parameters

`expectedEndString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the end of the string

`actualString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [EndsWithException](Xunit.Sdk.EndsWithException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_EndsWith_System_Memory_System_Char__System_Memory_System_Char__System_StringComparison_">EndsWith\(Memory<char\>, Memory<char\>, StringComparison\)</a>

Verifies that a string ends with a given sub-string, using the given comparison type.

```csharp
public static void EndsWith(Memory<char> expectedEndString, Memory<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedEndString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the end of the string

`actualString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [EndsWithException](Xunit.Sdk.EndsWithException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_EndsWith_System_Memory_System_Char__System_ReadOnlyMemory_System_Char__System_StringComparison_">EndsWith\(Memory<char\>, ReadOnlyMemory<char\>, StringComparison\)</a>

Verifies that a string ends with a given sub-string, using the given comparison type.

```csharp
public static void EndsWith(Memory<char> expectedEndString, ReadOnlyMemory<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedEndString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the end of the string

`actualString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [EndsWithException](Xunit.Sdk.EndsWithException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_EndsWith_System_ReadOnlyMemory_System_Char__System_Memory_System_Char__System_StringComparison_">EndsWith\(ReadOnlyMemory<char\>, Memory<char\>, StringComparison\)</a>

Verifies that a string ends with a given sub-string, using the given comparison type.

```csharp
public static void EndsWith(ReadOnlyMemory<char> expectedEndString, Memory<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedEndString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the end of the string

`actualString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [EndsWithException](Xunit.Sdk.EndsWithException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_EndsWith_System_ReadOnlyMemory_System_Char__System_ReadOnlyMemory_System_Char__System_StringComparison_">EndsWith\(ReadOnlyMemory<char\>, ReadOnlyMemory<char\>, StringComparison\)</a>

Verifies that a string ends with a given sub-string, using the given comparison type.

```csharp
public static void EndsWith(ReadOnlyMemory<char> expectedEndString, ReadOnlyMemory<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedEndString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the end of the string

`actualString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [EndsWithException](Xunit.Sdk.EndsWithException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_EndsWith_System_Span_System_Char__System_Span_System_Char__">EndsWith\(Span<char\>, Span<char\>\)</a>

Verifies that a string ends with a given sub-string, using the current culture.

```csharp
public static void EndsWith(Span<char> expectedEndString, Span<char> actualString)
```

#### Parameters

`expectedEndString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the end of the string

`actualString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [EndsWithException](Xunit.Sdk.EndsWithException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_EndsWith_System_Span_System_Char__System_ReadOnlySpan_System_Char__">EndsWith\(Span<char\>, ReadOnlySpan<char\>\)</a>

Verifies that a string ends with a given sub-string, using the current culture.

```csharp
public static void EndsWith(Span<char> expectedEndString, ReadOnlySpan<char> actualString)
```

#### Parameters

`expectedEndString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the end of the string

`actualString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [EndsWithException](Xunit.Sdk.EndsWithException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_EndsWith_System_ReadOnlySpan_System_Char__System_Span_System_Char__">EndsWith\(ReadOnlySpan<char\>, Span<char\>\)</a>

Verifies that a string ends with a given sub-string, using the current culture.

```csharp
public static void EndsWith(ReadOnlySpan<char> expectedEndString, Span<char> actualString)
```

#### Parameters

`expectedEndString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the end of the string

`actualString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [EndsWithException](Xunit.Sdk.EndsWithException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_EndsWith_System_ReadOnlySpan_System_Char__System_ReadOnlySpan_System_Char__">EndsWith\(ReadOnlySpan<char\>, ReadOnlySpan<char\>\)</a>

Verifies that a string ends with a given sub-string, using the current culture.

```csharp
public static void EndsWith(ReadOnlySpan<char> expectedEndString, ReadOnlySpan<char> actualString)
```

#### Parameters

`expectedEndString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the end of the string

`actualString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [EndsWithException](Xunit.Sdk.EndsWithException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_EndsWith_System_Span_System_Char__System_Span_System_Char__System_StringComparison_">EndsWith\(Span<char\>, Span<char\>, StringComparison\)</a>

Verifies that a string ends with a given sub-string, using the given comparison type.

```csharp
public static void EndsWith(Span<char> expectedEndString, Span<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedEndString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the end of the string

`actualString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [EndsWithException](Xunit.Sdk.EndsWithException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_EndsWith_System_Span_System_Char__System_ReadOnlySpan_System_Char__System_StringComparison_">EndsWith\(Span<char\>, ReadOnlySpan<char\>, StringComparison\)</a>

Verifies that a string ends with a given sub-string, using the given comparison type.

```csharp
public static void EndsWith(Span<char> expectedEndString, ReadOnlySpan<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedEndString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the end of the string

`actualString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [EndsWithException](Xunit.Sdk.EndsWithException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_EndsWith_System_ReadOnlySpan_System_Char__System_Span_System_Char__System_StringComparison_">EndsWith\(ReadOnlySpan<char\>, Span<char\>, StringComparison\)</a>

Verifies that a string ends with a given sub-string, using the given comparison type.

```csharp
public static void EndsWith(ReadOnlySpan<char> expectedEndString, Span<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedEndString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the end of the string

`actualString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [EndsWithException](Xunit.Sdk.EndsWithException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_EndsWith_System_ReadOnlySpan_System_Char__System_ReadOnlySpan_System_Char__System_StringComparison_">EndsWith\(ReadOnlySpan<char\>, ReadOnlySpan<char\>, StringComparison\)</a>

Verifies that a string ends with a given sub-string, using the given comparison type.

```csharp
public static void EndsWith(ReadOnlySpan<char> expectedEndString, ReadOnlySpan<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedEndString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the end of the string

`actualString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [EndsWithException](Xunit.Sdk.EndsWithException.md)

Thrown when the string does not end with the expected sub-string

### <a href="#Xunit_Assert_Equal__1_System_Collections_Generic_IEnumerable___0__System_Collections_Generic_IEnumerable___0__">Equal<T\>\(IEnumerable<T\>?, IEnumerable<T\>?\)</a>

Verifies that two sequences are equivalent, using a default comparer.

```csharp
public static void Equal<T>(IEnumerable<T>? expected, IEnumerable<T>? actual)
```

#### Parameters

`expected` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>?

The expected value

`actual` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>?

The value to be compared against

#### Type Parameters

`T` 

The type of the objects to be compared

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the objects are not equal

### <a href="#Xunit_Assert_Equal__1_System_Collections_Generic_IEnumerable___0__System_Collections_Generic_IEnumerable___0__System_Collections_Generic_IEqualityComparer___0__">Equal<T\>\(IEnumerable<T\>?, IEnumerable<T\>?, IEqualityComparer<T\>\)</a>

Verifies that two sequences are equivalent, using a custom equatable comparer.

```csharp
public static void Equal<T>(IEnumerable<T>? expected, IEnumerable<T>? actual, IEqualityComparer<T> comparer)
```

#### Parameters

`expected` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>?

The expected value

`actual` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>?

The value to be compared against

`comparer` [IEqualityComparer](https://learn.microsoft.com/dotnet/api/system.collections.generic.iequalitycomparer\-1)<T\>

The comparer used to compare the two objects

#### Type Parameters

`T` 

The type of the objects to be compared

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the objects are not equal

### <a href="#Xunit_Assert_Equal__1_System_Collections_Generic_IEnumerable___0__System_Collections_Generic_IEnumerable___0__System_Func___0___0_System_Boolean__">Equal<T\>\(IEnumerable<T\>?, IEnumerable<T\>?, Func<T, T, bool\>\)</a>

Verifies that two collections are equal, using a comparer function against
items in the two collections.

```csharp
public static void Equal<T>(IEnumerable<T>? expected, IEnumerable<T>? actual, Func<T, T, bool> comparer)
```

#### Parameters

`expected` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>?

The expected value

`actual` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>?

The value to be compared against

`comparer` [Func](https://learn.microsoft.com/dotnet/api/system.func\-3)<T, T, [bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

The function to compare two items for equality

#### Type Parameters

`T` 

The type of the objects to be compared

### <a href="#Xunit_Assert_Equal__1___0_____0___">Equal<T\>\(T\[\], T\[\]\)</a>

Verifies that two arrays of un-managed type T are equal, using Span&lt;T&gt;.SequenceEqual.
This can be significantly faster than generic enumerables, when the collections are actually
equal, because the system can optimize packed-memory comparisons for value type arrays.

```csharp
public static void Equal<T>(T[] expected, T[] actual) where T : unmanaged, IEquatable<T>
```

#### Parameters

`expected` T\[\]

The expected value

`actual` T\[\]

The value to be compared against

#### Type Parameters

`T` 

The type of items whose arrays are to be compared

#### Remarks

If <xref href="System.MemoryExtensions.SequenceEqual%60%601(System.Span%7b%60%600%7d%2cSystem.ReadOnlySpan%7b%60%600%7d)" data-throw-if-not-resolved="false"></xref> fails, a call
to <xref href="Xunit.Assert.Equal%60%601(%60%600%2c%60%600)" data-throw-if-not-resolved="false"></xref> is made, to provide a more meaningful error message.

### <a href="#Xunit_Assert_Equal__1___0___0_">Equal<T\>\(T, T\)</a>

Verifies that two objects are equal, using a default comparer.

```csharp
public static void Equal<T>(T expected, T actual)
```

#### Parameters

`expected` T

The expected value

`actual` T

The value to be compared against

#### Type Parameters

`T` 

The type of the objects to be compared

### <a href="#Xunit_Assert_Equal__1___0___0_System_Func___0___0_System_Boolean__">Equal<T\>\(T, T, Func<T, T, bool\>\)</a>

Verifies that two objects are equal, using a custom comparer function.

```csharp
public static void Equal<T>(T expected, T actual, Func<T, T, bool> comparer)
```

#### Parameters

`expected` T

The expected value

`actual` T

The value to be compared against

`comparer` [Func](https://learn.microsoft.com/dotnet/api/system.func\-3)<T, T, [bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

The comparer used to compare the two objects

#### Type Parameters

`T` 

The type of the objects to be compared

### <a href="#Xunit_Assert_Equal__1___0___0_System_Collections_Generic_IEqualityComparer___0__">Equal<T\>\(T, T, IEqualityComparer<T\>\)</a>

Verifies that two objects are equal, using a custom equatable comparer.

```csharp
public static void Equal<T>(T expected, T actual, IEqualityComparer<T> comparer)
```

#### Parameters

`expected` T

The expected value

`actual` T

The value to be compared against

`comparer` [IEqualityComparer](https://learn.microsoft.com/dotnet/api/system.collections.generic.iequalitycomparer\-1)<T\>

The comparer used to compare the two objects

#### Type Parameters

`T` 

The type of the objects to be compared

### <a href="#Xunit_Assert_Equal_System_Double_System_Double_System_Int32_">Equal\(double, double, int\)</a>

Verifies that two <xref href="System.Double" data-throw-if-not-resolved="false"></xref> values are equal, within the number of decimal
places given by <code class="paramref">precision</code>. The values are rounded before comparison.

```csharp
public static void Equal(double expected, double actual, int precision)
```

#### Parameters

`expected` [double](https://learn.microsoft.com/dotnet/api/system.double)

The expected value

`actual` [double](https://learn.microsoft.com/dotnet/api/system.double)

The value to be compared against

`precision` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The number of decimal places (valid values: 0-15)

### <a href="#Xunit_Assert_Equal_System_Double_System_Double_System_Int32_System_MidpointRounding_">Equal\(double, double, int, MidpointRounding\)</a>

Verifies that two <xref href="System.Double" data-throw-if-not-resolved="false"></xref> values are equal, within the number of decimal
places given by <code class="paramref">precision</code>. The values are rounded before comparison.
The rounding method to use is given by <code class="paramref">rounding</code>

```csharp
public static void Equal(double expected, double actual, int precision, MidpointRounding rounding)
```

#### Parameters

`expected` [double](https://learn.microsoft.com/dotnet/api/system.double)

The expected value

`actual` [double](https://learn.microsoft.com/dotnet/api/system.double)

The value to be compared against

`precision` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The number of decimal places (valid values: 0-15)

`rounding` [MidpointRounding](https://learn.microsoft.com/dotnet/api/system.midpointrounding)

Rounding method to use to process a number that is midway between two numbers

### <a href="#Xunit_Assert_Equal_System_Double_System_Double_System_Double_">Equal\(double, double, double\)</a>

Verifies that two <xref href="System.Double" data-throw-if-not-resolved="false"></xref> values are equal, within the tolerance given by
<code class="paramref">tolerance</code> (positive or negative).

```csharp
public static void Equal(double expected, double actual, double tolerance)
```

#### Parameters

`expected` [double](https://learn.microsoft.com/dotnet/api/system.double)

The expected value

`actual` [double](https://learn.microsoft.com/dotnet/api/system.double)

The value to be compared against

`tolerance` [double](https://learn.microsoft.com/dotnet/api/system.double)

The allowed difference between values

### <a href="#Xunit_Assert_Equal_System_Single_System_Single_System_Int32_">Equal\(float, float, int\)</a>

Verifies that two <xref href="System.Single" data-throw-if-not-resolved="false"></xref> values are equal, within the number of decimal
places given by <code class="paramref">precision</code>. The values are rounded before comparison.

```csharp
public static void Equal(float expected, float actual, int precision)
```

#### Parameters

`expected` [float](https://learn.microsoft.com/dotnet/api/system.single)

The expected value

`actual` [float](https://learn.microsoft.com/dotnet/api/system.single)

The value to be compared against

`precision` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The number of decimal places (valid values: 0-15)

### <a href="#Xunit_Assert_Equal_System_Single_System_Single_System_Int32_System_MidpointRounding_">Equal\(float, float, int, MidpointRounding\)</a>

Verifies that two <xref href="System.Single" data-throw-if-not-resolved="false"></xref> values are equal, within the number of decimal
places given by <code class="paramref">precision</code>. The values are rounded before comparison.
The rounding method to use is given by <code class="paramref">rounding</code>

```csharp
public static void Equal(float expected, float actual, int precision, MidpointRounding rounding)
```

#### Parameters

`expected` [float](https://learn.microsoft.com/dotnet/api/system.single)

The expected value

`actual` [float](https://learn.microsoft.com/dotnet/api/system.single)

The value to be compared against

`precision` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The number of decimal places (valid values: 0-15)

`rounding` [MidpointRounding](https://learn.microsoft.com/dotnet/api/system.midpointrounding)

Rounding method to use to process a number that is midway between two numbers

### <a href="#Xunit_Assert_Equal_System_Single_System_Single_System_Single_">Equal\(float, float, float\)</a>

Verifies that two <xref href="System.Single" data-throw-if-not-resolved="false"></xref> values are equal, within the tolerance given by
<code class="paramref">tolerance</code> (positive or negative).

```csharp
public static void Equal(float expected, float actual, float tolerance)
```

#### Parameters

`expected` [float](https://learn.microsoft.com/dotnet/api/system.single)

The expected value

`actual` [float](https://learn.microsoft.com/dotnet/api/system.single)

The value to be compared against

`tolerance` [float](https://learn.microsoft.com/dotnet/api/system.single)

The allowed difference between values

### <a href="#Xunit_Assert_Equal_System_Decimal_System_Decimal_System_Int32_">Equal\(decimal, decimal, int\)</a>

Verifies that two <xref href="System.Decimal" data-throw-if-not-resolved="false"></xref> values are equal, within the number of decimal
places given by <code class="paramref">precision</code>. The values are rounded before comparison.

```csharp
public static void Equal(decimal expected, decimal actual, int precision)
```

#### Parameters

`expected` [decimal](https://learn.microsoft.com/dotnet/api/system.decimal)

The expected value

`actual` [decimal](https://learn.microsoft.com/dotnet/api/system.decimal)

The value to be compared against

`precision` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The number of decimal places (valid values: 0-28)

### <a href="#Xunit_Assert_Equal_System_DateTime_System_DateTime_">Equal\(DateTime, DateTime\)</a>

Verifies that two <xref href="System.DateTime" data-throw-if-not-resolved="false"></xref> values are equal.

```csharp
public static void Equal(DateTime expected, DateTime actual)
```

#### Parameters

`expected` [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

The expected value

`actual` [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

The value to be compared against

### <a href="#Xunit_Assert_Equal_System_DateTime_System_DateTime_System_TimeSpan_">Equal\(DateTime, DateTime, TimeSpan\)</a>

Verifies that two <xref href="System.DateTime" data-throw-if-not-resolved="false"></xref> values are equal, within the precision
given by <code class="paramref">precision</code>.

```csharp
public static void Equal(DateTime expected, DateTime actual, TimeSpan precision)
```

#### Parameters

`expected` [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

The expected value

`actual` [DateTime](https://learn.microsoft.com/dotnet/api/system.datetime)

The value to be compared against

`precision` [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

The allowed difference in time where the two dates are considered equal

### <a href="#Xunit_Assert_Equal_System_DateTimeOffset_System_DateTimeOffset_">Equal\(DateTimeOffset, DateTimeOffset\)</a>

Verifies that two <xref href="System.DateTimeOffset" data-throw-if-not-resolved="false"></xref> values are equal.

```csharp
public static void Equal(DateTimeOffset expected, DateTimeOffset actual)
```

#### Parameters

`expected` [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

The expected value

`actual` [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

The value to be compared against

### <a href="#Xunit_Assert_Equal_System_DateTimeOffset_System_DateTimeOffset_System_TimeSpan_">Equal\(DateTimeOffset, DateTimeOffset, TimeSpan\)</a>

Verifies that two <xref href="System.DateTimeOffset" data-throw-if-not-resolved="false"></xref> values are equal, within the precision
given by <code class="paramref">precision</code>.

```csharp
public static void Equal(DateTimeOffset expected, DateTimeOffset actual, TimeSpan precision)
```

#### Parameters

`expected` [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

The expected value

`actual` [DateTimeOffset](https://learn.microsoft.com/dotnet/api/system.datetimeoffset)

The value to be compared against

`precision` [TimeSpan](https://learn.microsoft.com/dotnet/api/system.timespan)

The allowed difference in time where the two dates are considered equal

### <a href="#Xunit_Assert_Equal__1_System_Memory___0__System_Memory___0__">Equal<T\>\(Memory<T\>, Memory<T\>\)</a>

Verifies that two Memory values are equivalent.

```csharp
public static void Equal<T>(Memory<T> expectedMemory, Memory<T> actualMemory) where T : IEquatable<T>
```

#### Parameters

`expectedMemory` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<T\>

The expected Memory value.

`actualMemory` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<T\>

The actual Memory value.

#### Type Parameters

`T` 

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the Memory values are not equivalent.

### <a href="#Xunit_Assert_Equal__1_System_Memory___0__System_ReadOnlyMemory___0__">Equal<T\>\(Memory<T\>, ReadOnlyMemory<T\>\)</a>

Verifies that two Memory values are equivalent.

```csharp
public static void Equal<T>(Memory<T> expectedMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
```

#### Parameters

`expectedMemory` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<T\>

The expected Memory value.

`actualMemory` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<T\>

The actual Memory value.

#### Type Parameters

`T` 

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the Memory values are not equivalent.

### <a href="#Xunit_Assert_Equal__1_System_ReadOnlyMemory___0__System_Memory___0__">Equal<T\>\(ReadOnlyMemory<T\>, Memory<T\>\)</a>

Verifies that two Memory values are equivalent.

```csharp
public static void Equal<T>(ReadOnlyMemory<T> expectedMemory, Memory<T> actualMemory) where T : IEquatable<T>
```

#### Parameters

`expectedMemory` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<T\>

The expected Memory value.

`actualMemory` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<T\>

The actual Memory value.

#### Type Parameters

`T` 

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the Memory values are not equivalent.

### <a href="#Xunit_Assert_Equal__1_System_ReadOnlyMemory___0__System_ReadOnlyMemory___0__">Equal<T\>\(ReadOnlyMemory<T\>, ReadOnlyMemory<T\>\)</a>

Verifies that two Memory values are equivalent.

```csharp
public static void Equal<T>(ReadOnlyMemory<T> expectedMemory, ReadOnlyMemory<T> actualMemory) where T : IEquatable<T>
```

#### Parameters

`expectedMemory` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<T\>

The expected Memory value.

`actualMemory` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<T\>

The actual Memory value.

#### Type Parameters

`T` 

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the Memory values are not equivalent.

### <a href="#Xunit_Assert_Equal__1_System_ReadOnlySpan___0____0___">Equal<T\>\(ReadOnlySpan<T\>, T\[\]\)</a>

Verifies that a span and an array contain the same values in the same order.

```csharp
public static void Equal<T>(ReadOnlySpan<T> expectedSpan, T[] actualArray) where T : IEquatable<T>
```

#### Parameters

`expectedSpan` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<T\>

The expected span value.

`actualArray` T\[\]

The actual array value.

#### Type Parameters

`T` 

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the collections are not equal.

### <a href="#Xunit_Assert_Equal__1_System_Span___0__System_Span___0__">Equal<T\>\(Span<T\>, Span<T\>\)</a>

Verifies that two spans contain the same values in the same order.

```csharp
public static void Equal<T>(Span<T> expectedSpan, Span<T> actualSpan) where T : IEquatable<T>
```

#### Parameters

`expectedSpan` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<T\>

The expected span value.

`actualSpan` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<T\>

The actual span value.

#### Type Parameters

`T` 

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the spans are not equal.

### <a href="#Xunit_Assert_Equal__1_System_Span___0__System_ReadOnlySpan___0__">Equal<T\>\(Span<T\>, ReadOnlySpan<T\>\)</a>

Verifies that two spans contain the same values in the same order.

```csharp
public static void Equal<T>(Span<T> expectedSpan, ReadOnlySpan<T> actualSpan) where T : IEquatable<T>
```

#### Parameters

`expectedSpan` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<T\>

The expected span value.

`actualSpan` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<T\>

The actual span value.

#### Type Parameters

`T` 

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the spans are not equal.

### <a href="#Xunit_Assert_Equal__1_System_ReadOnlySpan___0__System_Span___0__">Equal<T\>\(ReadOnlySpan<T\>, Span<T\>\)</a>

Verifies that two spans contain the same values in the same order.

```csharp
public static void Equal<T>(ReadOnlySpan<T> expectedSpan, Span<T> actualSpan) where T : IEquatable<T>
```

#### Parameters

`expectedSpan` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<T\>

The expected span value.

`actualSpan` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<T\>

The actual span value.

#### Type Parameters

`T` 

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the spans are not equal.

### <a href="#Xunit_Assert_Equal__1_System_ReadOnlySpan___0__System_ReadOnlySpan___0__">Equal<T\>\(ReadOnlySpan<T\>, ReadOnlySpan<T\>\)</a>

Verifies that two spans contain the same values in the same order.

```csharp
public static void Equal<T>(ReadOnlySpan<T> expectedSpan, ReadOnlySpan<T> actualSpan) where T : IEquatable<T>
```

#### Parameters

`expectedSpan` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<T\>

The expected span value.

`actualSpan` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<T\>

The actual span value.

#### Type Parameters

`T` 

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the spans are not equal.

### <a href="#Xunit_Assert_Equal_System_String_System_String_">Equal\(string?, string?\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(string? expected, string? actual)
```

#### Parameters

`expected` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The expected string value.

`actual` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The actual string value.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equal_System_ReadOnlySpan_System_Char__System_ReadOnlySpan_System_Char__System_Boolean_System_Boolean_System_Boolean_System_Boolean_">Equal\(ReadOnlySpan<char\>, ReadOnlySpan<char\>, bool, bool, bool, bool\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(ReadOnlySpan<char> expected, ReadOnlySpan<char> actual, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false, bool ignoreAllWhiteSpace = false)
```

#### Parameters

`expected` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The expected string value.

`actual` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The actual string value.

`ignoreCase` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, ignores cases differences. The invariant culture is used.

`ignoreLineEndingDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats \r\n, \r, and \n as equivalent.

`ignoreWhiteSpaceDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats horizontal white-space (i.e. spaces, tabs, and others; see remarks) in any non-zero quantity as equivalent.

`ignoreAllWhiteSpace` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats horizontal white-space (i.e. spaces, tabs, and others; see remarks), including zero quantities, as equivalent.

#### Remarks

The <code class="paramref">ignoreWhiteSpaceDifferences</code> and <code class="paramref">ignoreAllWhiteSpace</code> flags consider
the following characters to be white-space:
<a href="https://unicode-explorer.com/c/0009">Tab(\t),</a>
<a href="https://unicode-explorer.com/c/0020">Space(\u0020),</a>
<a href="https://unicode-explorer.com/c/00A0">No-Break Space(\u00A0),</a>
<a href="https://unicode-explorer.com/c/1680">Ogham Space Mark(\u1680),</a>
<a href="https://unicode-explorer.com/c/180E">Mongolian Vowel Separator(\u180E),</a>
<a href="https://unicode-explorer.com/c/2000">En Quad(\u2000),</a>
<a href="https://unicode-explorer.com/c/2001">Em Quad(\u2001),</a>
<a href="https://unicode-explorer.com/c/2002">En Space(\u2002),</a>
<a href="https://unicode-explorer.com/c/2003">Em Space(\u2003),</a>
<a href="https://unicode-explorer.com/c/2004">Three-Per-Em Space(\u2004),</a>
<a href="https://unicode-explorer.com/c/2005">Four-Per-Em Space(\u2004),</a>
<a href="https://unicode-explorer.com/c/2006">Six-Per-Em Space(\u2006),</a>
<a href="https://unicode-explorer.com/c/2007">Figure Space(\u2007),</a>
<a href="https://unicode-explorer.com/c/2008">Punctuation Space(\u2008),</a>
<a href="https://unicode-explorer.com/c/2009">Thin Space(\u2009),</a>
<a href="https://unicode-explorer.com/c/200A">Hair Space(\u200A),</a>
<a href="https://unicode-explorer.com/c/200B">Zero Width Space(\u200B),</a>
<a href="https://unicode-explorer.com/c/202F">Narrow No-Break Space(\u202F),</a>
<a href="https://unicode-explorer.com/c/205F">Medium Mathematical Space(\u205F),</a>
<a href="https://unicode-explorer.com/c/3000">Ideographic Space(\u3000),</a>
and <a href="https://unicode-explorer.com/c/FEFF">Zero Width No-Break Space(\uFEFF).</a>
In particular, it does not include carriage return (\r) or line feed (\n), which are covered by
<code class="paramref">ignoreLineEndingDifferences</code>.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equal_System_Memory_System_Char__System_Memory_System_Char__">Equal\(Memory<char\>, Memory<char\>\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(Memory<char> expected, Memory<char> actual)
```

#### Parameters

`expected` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The expected string value.

`actual` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The actual string value.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equal_System_Memory_System_Char__System_ReadOnlyMemory_System_Char__">Equal\(Memory<char\>, ReadOnlyMemory<char\>\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(Memory<char> expected, ReadOnlyMemory<char> actual)
```

#### Parameters

`expected` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The expected string value.

`actual` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The actual string value.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equal_System_ReadOnlyMemory_System_Char__System_Memory_System_Char__">Equal\(ReadOnlyMemory<char\>, Memory<char\>\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(ReadOnlyMemory<char> expected, Memory<char> actual)
```

#### Parameters

`expected` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The expected string value.

`actual` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The actual string value.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equal_System_ReadOnlyMemory_System_Char__System_ReadOnlyMemory_System_Char__">Equal\(ReadOnlyMemory<char\>, ReadOnlyMemory<char\>\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(ReadOnlyMemory<char> expected, ReadOnlyMemory<char> actual)
```

#### Parameters

`expected` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The expected string value.

`actual` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The actual string value.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equal_System_Memory_System_Char__System_Memory_System_Char__System_Boolean_System_Boolean_System_Boolean_System_Boolean_">Equal\(Memory<char\>, Memory<char\>, bool, bool, bool, bool\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(Memory<char> expected, Memory<char> actual, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false, bool ignoreAllWhiteSpace = false)
```

#### Parameters

`expected` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The expected string value.

`actual` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The actual string value.

`ignoreCase` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, ignores cases differences. The invariant culture is used.

`ignoreLineEndingDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats \r\n, \r, and \n as equivalent.

`ignoreWhiteSpaceDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats horizontal white-space (i.e. spaces, tabs, and others; see remarks) in any non-zero quantity as equivalent.

`ignoreAllWhiteSpace` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats horizontal white-space (i.e. spaces, tabs, and others; see remarks), including zero quantities, as equivalent.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equal_System_Memory_System_Char__System_ReadOnlyMemory_System_Char__System_Boolean_System_Boolean_System_Boolean_System_Boolean_">Equal\(Memory<char\>, ReadOnlyMemory<char\>, bool, bool, bool, bool\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(Memory<char> expected, ReadOnlyMemory<char> actual, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false, bool ignoreAllWhiteSpace = false)
```

#### Parameters

`expected` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The expected string value.

`actual` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The actual string value.

`ignoreCase` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, ignores cases differences. The invariant culture is used.

`ignoreLineEndingDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats \r\n, \r, and \n as equivalent.

`ignoreWhiteSpaceDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats horizontal white-space (i.e. spaces, tabs, and others; see remarks) in any non-zero quantity as equivalent.

`ignoreAllWhiteSpace` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats horizontal white-space (i.e. spaces, tabs, and others; see remarks), including zero quantities, as equivalent.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equal_System_ReadOnlyMemory_System_Char__System_Memory_System_Char__System_Boolean_System_Boolean_System_Boolean_System_Boolean_">Equal\(ReadOnlyMemory<char\>, Memory<char\>, bool, bool, bool, bool\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(ReadOnlyMemory<char> expected, Memory<char> actual, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false, bool ignoreAllWhiteSpace = false)
```

#### Parameters

`expected` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The expected string value.

`actual` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The actual string value.

`ignoreCase` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, ignores cases differences. The invariant culture is used.

`ignoreLineEndingDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats \r\n, \r, and \n as equivalent.

`ignoreWhiteSpaceDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats horizontal white-space (i.e. spaces, tabs, and others; see remarks) in any non-zero quantity as equivalent.

`ignoreAllWhiteSpace` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats horizontal white-space (i.e. spaces, tabs, and others; see remarks), including zero quantities, as equivalent.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equal_System_ReadOnlyMemory_System_Char__System_ReadOnlyMemory_System_Char__System_Boolean_System_Boolean_System_Boolean_System_Boolean_">Equal\(ReadOnlyMemory<char\>, ReadOnlyMemory<char\>, bool, bool, bool, bool\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(ReadOnlyMemory<char> expected, ReadOnlyMemory<char> actual, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false, bool ignoreAllWhiteSpace = false)
```

#### Parameters

`expected` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The expected string value.

`actual` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The actual string value.

`ignoreCase` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, ignores cases differences. The invariant culture is used.

`ignoreLineEndingDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats \r\n, \r, and \n as equivalent.

`ignoreWhiteSpaceDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats horizontal white-space (i.e. spaces, tabs, and others; see remarks) in any non-zero quantity as equivalent.

`ignoreAllWhiteSpace` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats horizontal white-space (i.e. spaces, tabs, and others; see remarks), including zero quantities, as equivalent.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equal_System_Span_System_Char__System_Span_System_Char__">Equal\(Span<char\>, Span<char\>\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(Span<char> expected, Span<char> actual)
```

#### Parameters

`expected` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The expected string value.

`actual` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The actual string value.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equal_System_Span_System_Char__System_ReadOnlySpan_System_Char__">Equal\(Span<char\>, ReadOnlySpan<char\>\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(Span<char> expected, ReadOnlySpan<char> actual)
```

#### Parameters

`expected` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The expected string value.

`actual` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The actual string value.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equal_System_ReadOnlySpan_System_Char__System_Span_System_Char__">Equal\(ReadOnlySpan<char\>, Span<char\>\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(ReadOnlySpan<char> expected, Span<char> actual)
```

#### Parameters

`expected` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The expected string value.

`actual` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The actual string value.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equal_System_ReadOnlySpan_System_Char__System_ReadOnlySpan_System_Char__">Equal\(ReadOnlySpan<char\>, ReadOnlySpan<char\>\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(ReadOnlySpan<char> expected, ReadOnlySpan<char> actual)
```

#### Parameters

`expected` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The expected string value.

`actual` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The actual string value.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equal_System_Span_System_Char__System_Span_System_Char__System_Boolean_System_Boolean_System_Boolean_System_Boolean_">Equal\(Span<char\>, Span<char\>, bool, bool, bool, bool\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(Span<char> expected, Span<char> actual, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false, bool ignoreAllWhiteSpace = false)
```

#### Parameters

`expected` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The expected string value.

`actual` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The actual string value.

`ignoreCase` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, ignores cases differences. The invariant culture is used.

`ignoreLineEndingDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats \r\n, \r, and \n as equivalent.

`ignoreWhiteSpaceDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats spaces and tabs (in any non-zero quantity) as equivalent.

`ignoreAllWhiteSpace` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, ignores all white space differences during comparison.

#### Remarks

The <code class="paramref">ignoreWhiteSpaceDifferences</code> and <code class="paramref">ignoreAllWhiteSpace</code> flags consider
the following characters to be white-space:
<a href="https://unicode-explorer.com/c/0009">Tab(\t),</a>
<a href="https://unicode-explorer.com/c/0020">Space(\u0020),</a>
<a href="https://unicode-explorer.com/c/00A0">No-Break Space(\u00A0),</a>
<a href="https://unicode-explorer.com/c/1680">Ogham Space Mark(\u1680),</a>
<a href="https://unicode-explorer.com/c/180E">Mongolian Vowel Separator(\u180E),</a>
<a href="https://unicode-explorer.com/c/2000">En Quad(\u2000),</a>
<a href="https://unicode-explorer.com/c/2001">Em Quad(\u2001),</a>
<a href="https://unicode-explorer.com/c/2002">En Space(\u2002),</a>
<a href="https://unicode-explorer.com/c/2003">Em Space(\u2003),</a>
<a href="https://unicode-explorer.com/c/2004">Three-Per-Em Space(\u2004),</a>
<a href="https://unicode-explorer.com/c/2005">Four-Per-Em Space(\u2004),</a>
<a href="https://unicode-explorer.com/c/2006">Six-Per-Em Space(\u2006),</a>
<a href="https://unicode-explorer.com/c/2007">Figure Space(\u2007),</a>
<a href="https://unicode-explorer.com/c/2008">Punctuation Space(\u2008),</a>
<a href="https://unicode-explorer.com/c/2009">Thin Space(\u2009),</a>
<a href="https://unicode-explorer.com/c/200A">Hair Space(\u200A),</a>
<a href="https://unicode-explorer.com/c/200B">Zero Width Space(\u200B),</a>
<a href="https://unicode-explorer.com/c/202F">Narrow No-Break Space(\u202F),</a>
<a href="https://unicode-explorer.com/c/205F">Medium Mathematical Space(\u205F),</a>
<a href="https://unicode-explorer.com/c/3000">Ideographic Space(\u3000),</a>
and <a href="https://unicode-explorer.com/c/FEFF">Zero Width No-Break Space(\uFEFF).</a>
In particular, it does not include carriage return (\r) or line feed (\n), which are covered by
<code class="paramref">ignoreLineEndingDifferences</code>.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equal_System_Span_System_Char__System_ReadOnlySpan_System_Char__System_Boolean_System_Boolean_System_Boolean_System_Boolean_">Equal\(Span<char\>, ReadOnlySpan<char\>, bool, bool, bool, bool\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(Span<char> expected, ReadOnlySpan<char> actual, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false, bool ignoreAllWhiteSpace = false)
```

#### Parameters

`expected` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The expected string value.

`actual` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The actual string value.

`ignoreCase` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, ignores cases differences. The invariant culture is used.

`ignoreLineEndingDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats \r\n, \r, and \n as equivalent.

`ignoreWhiteSpaceDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats spaces and tabs (in any non-zero quantity) as equivalent.

`ignoreAllWhiteSpace` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, ignores all white space differences during comparison.

#### Remarks

The <code class="paramref">ignoreWhiteSpaceDifferences</code> and <code class="paramref">ignoreAllWhiteSpace</code> flags consider
the following characters to be white-space:
<a href="https://unicode-explorer.com/c/0009">Tab(\t),</a>
<a href="https://unicode-explorer.com/c/0020">Space(\u0020),</a>
<a href="https://unicode-explorer.com/c/00A0">No-Break Space(\u00A0),</a>
<a href="https://unicode-explorer.com/c/1680">Ogham Space Mark(\u1680),</a>
<a href="https://unicode-explorer.com/c/180E">Mongolian Vowel Separator(\u180E),</a>
<a href="https://unicode-explorer.com/c/2000">En Quad(\u2000),</a>
<a href="https://unicode-explorer.com/c/2001">Em Quad(\u2001),</a>
<a href="https://unicode-explorer.com/c/2002">En Space(\u2002),</a>
<a href="https://unicode-explorer.com/c/2003">Em Space(\u2003),</a>
<a href="https://unicode-explorer.com/c/2004">Three-Per-Em Space(\u2004),</a>
<a href="https://unicode-explorer.com/c/2005">Four-Per-Em Space(\u2004),</a>
<a href="https://unicode-explorer.com/c/2006">Six-Per-Em Space(\u2006),</a>
<a href="https://unicode-explorer.com/c/2007">Figure Space(\u2007),</a>
<a href="https://unicode-explorer.com/c/2008">Punctuation Space(\u2008),</a>
<a href="https://unicode-explorer.com/c/2009">Thin Space(\u2009),</a>
<a href="https://unicode-explorer.com/c/200A">Hair Space(\u200A),</a>
<a href="https://unicode-explorer.com/c/200B">Zero Width Space(\u200B),</a>
<a href="https://unicode-explorer.com/c/202F">Narrow No-Break Space(\u202F),</a>
<a href="https://unicode-explorer.com/c/205F">Medium Mathematical Space(\u205F),</a>
<a href="https://unicode-explorer.com/c/3000">Ideographic Space(\u3000),</a>
and <a href="https://unicode-explorer.com/c/FEFF">Zero Width No-Break Space(\uFEFF).</a>
In particular, it does not include carriage return (\r) or line feed (\n), which are covered by
<code class="paramref">ignoreLineEndingDifferences</code>.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equal_System_ReadOnlySpan_System_Char__System_Span_System_Char__System_Boolean_System_Boolean_System_Boolean_System_Boolean_">Equal\(ReadOnlySpan<char\>, Span<char\>, bool, bool, bool, bool\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(ReadOnlySpan<char> expected, Span<char> actual, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false, bool ignoreAllWhiteSpace = false)
```

#### Parameters

`expected` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The expected string value.

`actual` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The actual string value.

`ignoreCase` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, ignores cases differences. The invariant culture is used.

`ignoreLineEndingDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats \r\n, \r, and \n as equivalent.

`ignoreWhiteSpaceDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats spaces and tabs (in any non-zero quantity) as equivalent.

`ignoreAllWhiteSpace` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, removes all whitespaces and tabs before comparing.

#### Remarks

The <code class="paramref">ignoreWhiteSpaceDifferences</code> and <code class="paramref">ignoreAllWhiteSpace</code> flags consider
the following characters to be white-space:
<a href="https://unicode-explorer.com/c/0009">Tab(\t),</a>
<a href="https://unicode-explorer.com/c/0020">Space(\u0020),</a>
<a href="https://unicode-explorer.com/c/00A0">No-Break Space(\u00A0),</a>
<a href="https://unicode-explorer.com/c/1680">Ogham Space Mark(\u1680),</a>
<a href="https://unicode-explorer.com/c/180E">Mongolian Vowel Separator(\u180E),</a>
<a href="https://unicode-explorer.com/c/2000">En Quad(\u2000),</a>
<a href="https://unicode-explorer.com/c/2001">Em Quad(\u2001),</a>
<a href="https://unicode-explorer.com/c/2002">En Space(\u2002),</a>
<a href="https://unicode-explorer.com/c/2003">Em Space(\u2003),</a>
<a href="https://unicode-explorer.com/c/2004">Three-Per-Em Space(\u2004),</a>
<a href="https://unicode-explorer.com/c/2005">Four-Per-Em Space(\u2004),</a>
<a href="https://unicode-explorer.com/c/2006">Six-Per-Em Space(\u2006),</a>
<a href="https://unicode-explorer.com/c/2007">Figure Space(\u2007),</a>
<a href="https://unicode-explorer.com/c/2008">Punctuation Space(\u2008),</a>
<a href="https://unicode-explorer.com/c/2009">Thin Space(\u2009),</a>
<a href="https://unicode-explorer.com/c/200A">Hair Space(\u200A),</a>
<a href="https://unicode-explorer.com/c/200B">Zero Width Space(\u200B),</a>
<a href="https://unicode-explorer.com/c/202F">Narrow No-Break Space(\u202F),</a>
<a href="https://unicode-explorer.com/c/205F">Medium Mathematical Space(\u205F),</a>
<a href="https://unicode-explorer.com/c/3000">Ideographic Space(\u3000),</a>
and <a href="https://unicode-explorer.com/c/FEFF">Zero Width No-Break Space(\uFEFF).</a>
In particular, it does not include carriage return (\r) or line feed (\n), which are covered by
<code class="paramref">ignoreLineEndingDifferences</code>.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equal_System_String_System_String_System_Boolean_System_Boolean_System_Boolean_System_Boolean_">Equal\(string?, string?, bool, bool, bool, bool\)</a>

Verifies that two strings are equivalent.

```csharp
public static void Equal(string? expected, string? actual, bool ignoreCase = false, bool ignoreLineEndingDifferences = false, bool ignoreWhiteSpaceDifferences = false, bool ignoreAllWhiteSpace = false)
```

#### Parameters

`expected` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The expected string value.

`actual` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The actual string value.

`ignoreCase` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, ignores cases differences. The invariant culture is used.

`ignoreLineEndingDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats \r\n, \r, and \n as equivalent.

`ignoreWhiteSpaceDifferences` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats horizontal white-space (i.e. spaces, tabs, and others; see remarks) in any non-zero quantity as equivalent.

`ignoreAllWhiteSpace` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

If set to <code>true</code>, treats horizontal white-space (i.e. spaces, tabs, and others; see remarks), including zero quantities, as equivalent.

#### Remarks

The <code class="paramref">ignoreWhiteSpaceDifferences</code> and <code class="paramref">ignoreAllWhiteSpace</code> flags consider
the following characters to be white-space:
<a href="https://unicode-explorer.com/c/0009">Tab(\t),</a>
<a href="https://unicode-explorer.com/c/0020">Space(\u0020),</a>
<a href="https://unicode-explorer.com/c/00A0">No-Break Space(\u00A0),</a>
<a href="https://unicode-explorer.com/c/1680">Ogham Space Mark(\u1680),</a>
<a href="https://unicode-explorer.com/c/180E">Mongolian Vowel Separator(\u180E),</a>
<a href="https://unicode-explorer.com/c/2000">En Quad(\u2000),</a>
<a href="https://unicode-explorer.com/c/2001">Em Quad(\u2001),</a>
<a href="https://unicode-explorer.com/c/2002">En Space(\u2002),</a>
<a href="https://unicode-explorer.com/c/2003">Em Space(\u2003),</a>
<a href="https://unicode-explorer.com/c/2004">Three-Per-Em Space(\u2004),</a>
<a href="https://unicode-explorer.com/c/2005">Four-Per-Em Space(\u2004),</a>
<a href="https://unicode-explorer.com/c/2006">Six-Per-Em Space(\u2006),</a>
<a href="https://unicode-explorer.com/c/2007">Figure Space(\u2007),</a>
<a href="https://unicode-explorer.com/c/2008">Punctuation Space(\u2008),</a>
<a href="https://unicode-explorer.com/c/2009">Thin Space(\u2009),</a>
<a href="https://unicode-explorer.com/c/200A">Hair Space(\u200A),</a>
<a href="https://unicode-explorer.com/c/200B">Zero Width Space(\u200B),</a>
<a href="https://unicode-explorer.com/c/202F">Narrow No-Break Space(\u202F),</a>
<a href="https://unicode-explorer.com/c/205F">Medium Mathematical Space(\u205F),</a>
<a href="https://unicode-explorer.com/c/3000">Ideographic Space(\u3000),</a>
and <a href="https://unicode-explorer.com/c/FEFF">Zero Width No-Break Space(\uFEFF).</a>
In particular, it does not include carriage return (\r) or line feed (\n), which are covered by
<code class="paramref">ignoreLineEndingDifferences</code>.

#### Exceptions

 [EqualException](Xunit.Sdk.EqualException.md)

Thrown when the strings are not equivalent.

### <a href="#Xunit_Assert_Equivalent_System_Object_System_Object_System_Boolean_">Equivalent\(object?, object?, bool\)</a>

Verifies that two objects are equivalent, using a default comparer. This comparison is done
without regard to type, and only inspects public property and field values for individual
equality. Deep equivalence tests (meaning, property or fields which are themselves complex
types) are supported. With strict mode off, object comparison allows <code class="paramref">actual</code>
to have extra public members that aren't part of <code class="paramref">expected</code>, and collection
comparison allows <code class="paramref">actual</code> to have more data in it than is present in
<code class="paramref">expected</code>; with strict mode on, those rules are tightened to require exact
member list (for objects) or data (for collections).

```csharp
public static void Equivalent(object? expected, object? actual, bool strict = false)
```

#### Parameters

`expected` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The expected value

`actual` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The actual value

`strict` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

A flag which enables strict comparison mode

### <a href="#Xunit_Assert_Fail_System_String_">Fail\(string?\)</a>

Indicates that the test should immediately fail.

```csharp
public static void Fail(string? message = null)
```

#### Parameters

`message` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The optional failure message

### <a href="#Xunit_Assert_False_System_Boolean_">False\(bool\)</a>

Verifies that the condition is false.

```csharp
public static void False(bool condition)
```

#### Parameters

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

The condition to be tested

#### Exceptions

 [FalseException](Xunit.Sdk.FalseException.md)

Thrown if the condition is not false

### <a href="#Xunit_Assert_False_System_Nullable_System_Boolean__">False\(bool?\)</a>

Verifies that the condition is false.

```csharp
public static void False(bool? condition)
```

#### Parameters

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)?

The condition to be tested

#### Exceptions

 [FalseException](Xunit.Sdk.FalseException.md)

Thrown if the condition is not false

### <a href="#Xunit_Assert_False_System_Boolean_System_String_">False\(bool, string?\)</a>

Verifies that the condition is false.

```csharp
public static void False(bool condition, string? userMessage)
```

#### Parameters

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

The condition to be tested

`userMessage` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The message to show when the condition is not false

#### Exceptions

 [FalseException](Xunit.Sdk.FalseException.md)

Thrown if the condition is not false

### <a href="#Xunit_Assert_False_System_Nullable_System_Boolean__System_String_">False\(bool?, string?\)</a>

Verifies that the condition is false.

```csharp
public static void False(bool? condition, string? userMessage)
```

#### Parameters

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)?

The condition to be tested

`userMessage` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The message to show when the condition is not false

#### Exceptions

 [FalseException](Xunit.Sdk.FalseException.md)

Thrown if the condition is not false

### <a href="#Xunit_Assert_InRange__1___0___0___0_">InRange<T\>\(T, T, T\)</a>

Verifies that a value is within a given range.

```csharp
public static void InRange<T>(T actual, T low, T high) where T : IComparable
```

#### Parameters

`actual` T

The actual value to be evaluated

`low` T

The (inclusive) low value of the range

`high` T

The (inclusive) high value of the range

#### Type Parameters

`T` 

The type of the value to be compared

#### Exceptions

 [InRangeException](Xunit.Sdk.InRangeException.md)

Thrown when the value is not in the given range

### <a href="#Xunit_Assert_InRange__1___0___0___0_System_Collections_Generic_IComparer___0__">InRange<T\>\(T, T, T, IComparer<T\>\)</a>

Verifies that a value is within a given range, using a comparer.

```csharp
public static void InRange<T>(T actual, T low, T high, IComparer<T> comparer)
```

#### Parameters

`actual` T

The actual value to be evaluated

`low` T

The (inclusive) low value of the range

`high` T

The (inclusive) high value of the range

`comparer` [IComparer](https://learn.microsoft.com/dotnet/api/system.collections.generic.icomparer\-1)<T\>

The comparer used to evaluate the value's range

#### Type Parameters

`T` 

The type of the value to be compared

#### Exceptions

 [InRangeException](Xunit.Sdk.InRangeException.md)

Thrown when the value is not in the given range

### <a href="#Xunit_Assert_IsAssignableFrom__1_System_Object_">IsAssignableFrom<T\>\(object?\)</a>

Verifies that an object is of the given type or a derived type.

```csharp
public static T IsAssignableFrom<T>(object? @object)
```

#### Parameters

`object` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The object to be evaluated

#### Returns

 T

The object, casted to type T when successful

#### Type Parameters

`T` 

The type the object should be

#### Exceptions

 [IsAssignableFromException](Xunit.Sdk.IsAssignableFromException.md)

Thrown when the object is not the given type

### <a href="#Xunit_Assert_IsAssignableFrom_System_Type_System_Object_">IsAssignableFrom\(Type, object?\)</a>

Verifies that an object is of the given type or a derived type.

```csharp
public static void IsAssignableFrom(Type expectedType, object? @object)
```

#### Parameters

`expectedType` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The type the object should be

`object` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The object to be evaluated

#### Exceptions

 [IsAssignableFromException](Xunit.Sdk.IsAssignableFromException.md)

Thrown when the object is not the given type

### <a href="#Xunit_Assert_IsNotAssignableFrom__1_System_Object_">IsNotAssignableFrom<T\>\(object?\)</a>

Verifies that an object is not of the given type or a derived type.

```csharp
public static void IsNotAssignableFrom<T>(object? @object)
```

#### Parameters

`object` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The object to be evaluated

#### Type Parameters

`T` 

The type the object should not be

#### Exceptions

 [IsNotAssignableFromException](Xunit.Sdk.IsNotAssignableFromException.md)

Thrown when the object is of the given type

### <a href="#Xunit_Assert_IsNotAssignableFrom_System_Type_System_Object_">IsNotAssignableFrom\(Type, object?\)</a>

Verifies that an object is not of the given type or a derived type.

```csharp
public static void IsNotAssignableFrom(Type expectedType, object? @object)
```

#### Parameters

`expectedType` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The type the object should not be

`object` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The object to be evaluated

#### Exceptions

 [IsNotAssignableFromException](Xunit.Sdk.IsNotAssignableFromException.md)

Thrown when the object is of the given type

### <a href="#Xunit_Assert_IsNotType__1_System_Object_">IsNotType<T\>\(object?\)</a>

Verifies that an object is not exactly the given type.

```csharp
public static void IsNotType<T>(object? @object)
```

#### Parameters

`object` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The object to be evaluated

#### Type Parameters

`T` 

The type the object should not be

#### Exceptions

 [IsNotTypeException](Xunit.Sdk.IsNotTypeException.md)

Thrown when the object is the given type

### <a href="#Xunit_Assert_IsNotType_System_Type_System_Object_">IsNotType\(Type, object?\)</a>

Verifies that an object is not exactly the given type.

```csharp
public static void IsNotType(Type expectedType, object? @object)
```

#### Parameters

`expectedType` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The type the object should not be

`object` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The object to be evaluated

#### Exceptions

 [IsNotTypeException](Xunit.Sdk.IsNotTypeException.md)

Thrown when the object is the given type

### <a href="#Xunit_Assert_IsType__1_System_Object_">IsType<T\>\(object?\)</a>

Verifies that an object is exactly the given type (and not a derived type).

```csharp
public static T IsType<T>(object? @object)
```

#### Parameters

`object` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The object to be evaluated

#### Returns

 T

The object, casted to type T when successful

#### Type Parameters

`T` 

The type the object should be

#### Exceptions

 [IsTypeException](Xunit.Sdk.IsTypeException.md)

Thrown when the object is not the given type

### <a href="#Xunit_Assert_IsType_System_Type_System_Object_">IsType\(Type, object?\)</a>

Verifies that an object is exactly the given type (and not a derived type).

```csharp
public static void IsType(Type expectedType, object? @object)
```

#### Parameters

`expectedType` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The type the object should be

`object` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The object to be evaluated

#### Exceptions

 [IsTypeException](Xunit.Sdk.IsTypeException.md)

Thrown when the object is not the given type

### <a href="#Xunit_Assert_Matches_System_String_System_String_">Matches\(string, string?\)</a>

Verifies that a string matches a regular expression.

```csharp
public static void Matches(string expectedRegexPattern, string? actualString)
```

#### Parameters

`expectedRegexPattern` [string](https://learn.microsoft.com/dotnet/api/system.string)

The regex pattern expected to match

`actualString` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The string to be inspected

#### Exceptions

 [MatchesException](Xunit.Sdk.MatchesException.md)

Thrown when the string does not match the regex pattern

### <a href="#Xunit_Assert_Matches_System_Text_RegularExpressions_Regex_System_String_">Matches\(Regex, string?\)</a>

Verifies that a string matches a regular expression.

```csharp
public static void Matches(Regex expectedRegex, string? actualString)
```

#### Parameters

`expectedRegex` [Regex](https://learn.microsoft.com/dotnet/api/system.text.regularexpressions.regex)

The regex expected to match

`actualString` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The string to be inspected

#### Exceptions

 [MatchesException](Xunit.Sdk.MatchesException.md)

Thrown when the string does not match the regex

### <a href="#Xunit_Assert_Multiple_System_Action___">Multiple\(params Action\[\]\)</a>

Runs multiple checks, collecting the exceptions from each one, and then bundles all failures
up into a single assertion failure.

```csharp
public static void Multiple(params Action[] checks)
```

#### Parameters

`checks` [Action](https://learn.microsoft.com/dotnet/api/system.action)\[\]

The individual assertions to run, as actions.

### <a href="#Xunit_Assert_NotEmpty_System_Collections_IEnumerable_">NotEmpty\(IEnumerable\)</a>

Verifies that a collection is not empty.

```csharp
public static void NotEmpty(IEnumerable collection)
```

#### Parameters

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.ienumerable)

The collection to be inspected

#### Exceptions

 [ArgumentNullException](https://learn.microsoft.com/dotnet/api/system.argumentnullexception)

Thrown when a null collection is passed

 [NotEmptyException](Xunit.Sdk.NotEmptyException.md)

Thrown when the collection is empty

### <a href="#Xunit_Assert_NotEqual__1_System_Collections_Generic_IEnumerable___0__System_Collections_Generic_IEnumerable___0__">NotEqual<T\>\(IEnumerable<T\>?, IEnumerable<T\>?\)</a>

Verifies that two sequences are not equivalent, using a default comparer.

```csharp
public static void NotEqual<T>(IEnumerable<T>? expected, IEnumerable<T>? actual)
```

#### Parameters

`expected` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>?

The expected object

`actual` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>?

The actual object

#### Type Parameters

`T` 

The type of the objects to be compared

#### Exceptions

 [NotEqualException](Xunit.Sdk.NotEqualException.md)

Thrown when the objects are equal

### <a href="#Xunit_Assert_NotEqual__1_System_Collections_Generic_IEnumerable___0__System_Collections_Generic_IEnumerable___0__System_Collections_Generic_IEqualityComparer___0__">NotEqual<T\>\(IEnumerable<T\>?, IEnumerable<T\>?, IEqualityComparer<T\>\)</a>

Verifies that two sequences are not equivalent, using a custom equality comparer.

```csharp
public static void NotEqual<T>(IEnumerable<T>? expected, IEnumerable<T>? actual, IEqualityComparer<T> comparer)
```

#### Parameters

`expected` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>?

The expected object

`actual` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>?

The actual object

`comparer` [IEqualityComparer](https://learn.microsoft.com/dotnet/api/system.collections.generic.iequalitycomparer\-1)<T\>

The comparer used to compare the two objects

#### Type Parameters

`T` 

The type of the objects to be compared

#### Exceptions

 [NotEqualException](Xunit.Sdk.NotEqualException.md)

Thrown when the objects are equal

### <a href="#Xunit_Assert_NotEqual__1_System_Collections_Generic_IEnumerable___0__System_Collections_Generic_IEnumerable___0__System_Func___0___0_System_Boolean__">NotEqual<T\>\(IEnumerable<T\>?, IEnumerable<T\>?, Func<T, T, bool\>\)</a>

Verifies that two collections are not equal, using a comparer function against
items in the two collections.

```csharp
public static void NotEqual<T>(IEnumerable<T>? expected, IEnumerable<T>? actual, Func<T, T, bool> comparer)
```

#### Parameters

`expected` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>?

The expected value

`actual` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>?

The value to be compared against

`comparer` [Func](https://learn.microsoft.com/dotnet/api/system.func\-3)<T, T, [bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

The function to compare two items for equality

#### Type Parameters

`T` 

The type of the objects to be compared

### <a href="#Xunit_Assert_NotEqual__1___0_____0___">NotEqual<T\>\(T\[\], T\[\]\)</a>

Verifies that two arrays of un-managed type T are not equal, using Span&lt;T&gt;.SequenceEqual.

```csharp
public static void NotEqual<T>(T[] expected, T[] actual) where T : unmanaged, IEquatable<T>
```

#### Parameters

`expected` T\[\]

The expected value

`actual` T\[\]

The value to be compared against

#### Type Parameters

`T` 

The type of items whose arrays are to be compared

### <a href="#Xunit_Assert_NotEqual__1___0___0_">NotEqual<T\>\(T, T\)</a>

Verifies that two objects are not equal, using a default comparer.

```csharp
public static void NotEqual<T>(T expected, T actual)
```

#### Parameters

`expected` T

The expected object

`actual` T

The actual object

#### Type Parameters

`T` 

The type of the objects to be compared

### <a href="#Xunit_Assert_NotEqual__1___0___0_System_Func___0___0_System_Boolean__">NotEqual<T\>\(T, T, Func<T, T, bool\>\)</a>

Verifies that two objects are not equal, using a custom equality comparer function.

```csharp
public static void NotEqual<T>(T expected, T actual, Func<T, T, bool> comparer)
```

#### Parameters

`expected` T

The expected object

`actual` T

The actual object

`comparer` [Func](https://learn.microsoft.com/dotnet/api/system.func\-3)<T, T, [bool](https://learn.microsoft.com/dotnet/api/system.boolean)\>

The comparer used to examine the objects

#### Type Parameters

`T` 

The type of the objects to be compared

### <a href="#Xunit_Assert_NotEqual__1___0___0_System_Collections_Generic_IEqualityComparer___0__">NotEqual<T\>\(T, T, IEqualityComparer<T\>\)</a>

Verifies that two objects are not equal, using a custom equality comparer.

```csharp
public static void NotEqual<T>(T expected, T actual, IEqualityComparer<T> comparer)
```

#### Parameters

`expected` T

The expected object

`actual` T

The actual object

`comparer` [IEqualityComparer](https://learn.microsoft.com/dotnet/api/system.collections.generic.iequalitycomparer\-1)<T\>

The comparer used to examine the objects

#### Type Parameters

`T` 

The type of the objects to be compared

### <a href="#Xunit_Assert_NotEqual_System_Double_System_Double_System_Int32_">NotEqual\(double, double, int\)</a>

Verifies that two <xref href="System.Double" data-throw-if-not-resolved="false"></xref> values are not equal, within the number of decimal
places given by <code class="paramref">precision</code>.

```csharp
public static void NotEqual(double expected, double actual, int precision)
```

#### Parameters

`expected` [double](https://learn.microsoft.com/dotnet/api/system.double)

The expected value

`actual` [double](https://learn.microsoft.com/dotnet/api/system.double)

The value to be compared against

`precision` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The number of decimal places (valid values: 0-15)

### <a href="#Xunit_Assert_NotEqual_System_Double_System_Double_System_Int32_System_MidpointRounding_">NotEqual\(double, double, int, MidpointRounding\)</a>

Verifies that two <xref href="System.Double" data-throw-if-not-resolved="false"></xref> values are not equal, within the number of decimal
places given by <code class="paramref">precision</code>. The values are rounded before comparison.
The rounding method to use is given by <code class="paramref">rounding</code>

```csharp
public static void NotEqual(double expected, double actual, int precision, MidpointRounding rounding)
```

#### Parameters

`expected` [double](https://learn.microsoft.com/dotnet/api/system.double)

The expected value

`actual` [double](https://learn.microsoft.com/dotnet/api/system.double)

The value to be compared against

`precision` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The number of decimal places (valid values: 0-15)

`rounding` [MidpointRounding](https://learn.microsoft.com/dotnet/api/system.midpointrounding)

Rounding method to use to process a number that is midway between two numbers

### <a href="#Xunit_Assert_NotEqual_System_Double_System_Double_System_Double_">NotEqual\(double, double, double\)</a>

Verifies that two <xref href="System.Double" data-throw-if-not-resolved="false"></xref> values are not equal, within the tolerance given by
<code class="paramref">tolerance</code> (positive or negative).

```csharp
public static void NotEqual(double expected, double actual, double tolerance)
```

#### Parameters

`expected` [double](https://learn.microsoft.com/dotnet/api/system.double)

The expected value

`actual` [double](https://learn.microsoft.com/dotnet/api/system.double)

The value to be compared against

`tolerance` [double](https://learn.microsoft.com/dotnet/api/system.double)

The allowed difference between values

### <a href="#Xunit_Assert_NotEqual_System_Single_System_Single_System_Int32_">NotEqual\(float, float, int\)</a>

Verifies that two <xref href="System.Single" data-throw-if-not-resolved="false"></xref> values are not equal, within the number of decimal
places given by <code class="paramref">precision</code>.

```csharp
public static void NotEqual(float expected, float actual, int precision)
```

#### Parameters

`expected` [float](https://learn.microsoft.com/dotnet/api/system.single)

The expected value

`actual` [float](https://learn.microsoft.com/dotnet/api/system.single)

The value to be compared against

`precision` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The number of decimal places (valid values: 0-15)

### <a href="#Xunit_Assert_NotEqual_System_Single_System_Single_System_Int32_System_MidpointRounding_">NotEqual\(float, float, int, MidpointRounding\)</a>

Verifies that two <xref href="System.Single" data-throw-if-not-resolved="false"></xref> values are not equal, within the number of decimal
places given by <code class="paramref">precision</code>. The values are rounded before comparison.
The rounding method to use is given by <code class="paramref">rounding</code>

```csharp
public static void NotEqual(float expected, float actual, int precision, MidpointRounding rounding)
```

#### Parameters

`expected` [float](https://learn.microsoft.com/dotnet/api/system.single)

The expected value

`actual` [float](https://learn.microsoft.com/dotnet/api/system.single)

The value to be compared against

`precision` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The number of decimal places (valid values: 0-15)

`rounding` [MidpointRounding](https://learn.microsoft.com/dotnet/api/system.midpointrounding)

Rounding method to use to process a number that is midway between two numbers

### <a href="#Xunit_Assert_NotEqual_System_Single_System_Single_System_Single_">NotEqual\(float, float, float\)</a>

Verifies that two <xref href="System.Single" data-throw-if-not-resolved="false"></xref> values are not equal, within the tolerance given by
<code class="paramref">tolerance</code> (positive or negative).

```csharp
public static void NotEqual(float expected, float actual, float tolerance)
```

#### Parameters

`expected` [float](https://learn.microsoft.com/dotnet/api/system.single)

The expected value

`actual` [float](https://learn.microsoft.com/dotnet/api/system.single)

The value to be compared against

`tolerance` [float](https://learn.microsoft.com/dotnet/api/system.single)

The allowed difference between values

### <a href="#Xunit_Assert_NotEqual_System_Decimal_System_Decimal_System_Int32_">NotEqual\(decimal, decimal, int\)</a>

Verifies that two <xref href="System.Decimal" data-throw-if-not-resolved="false"></xref> values are not equal, within the number of decimal
places given by <code class="paramref">precision</code>.

```csharp
public static void NotEqual(decimal expected, decimal actual, int precision)
```

#### Parameters

`expected` [decimal](https://learn.microsoft.com/dotnet/api/system.decimal)

The expected value

`actual` [decimal](https://learn.microsoft.com/dotnet/api/system.decimal)

The value to be compared against

`precision` [int](https://learn.microsoft.com/dotnet/api/system.int32)

The number of decimal places (valid values: 0-28)

### <a href="#Xunit_Assert_NotInRange__1___0___0___0_">NotInRange<T\>\(T, T, T\)</a>

Verifies that a value is not within a given range, using the default comparer.

```csharp
public static void NotInRange<T>(T actual, T low, T high) where T : IComparable
```

#### Parameters

`actual` T

The actual value to be evaluated

`low` T

The (inclusive) low value of the range

`high` T

The (inclusive) high value of the range

#### Type Parameters

`T` 

The type of the value to be compared

#### Exceptions

 [NotInRangeException](Xunit.Sdk.NotInRangeException.md)

Thrown when the value is in the given range

### <a href="#Xunit_Assert_NotInRange__1___0___0___0_System_Collections_Generic_IComparer___0__">NotInRange<T\>\(T, T, T, IComparer<T\>\)</a>

Verifies that a value is not within a given range, using a comparer.

```csharp
public static void NotInRange<T>(T actual, T low, T high, IComparer<T> comparer)
```

#### Parameters

`actual` T

The actual value to be evaluated

`low` T

The (inclusive) low value of the range

`high` T

The (inclusive) high value of the range

`comparer` [IComparer](https://learn.microsoft.com/dotnet/api/system.collections.generic.icomparer\-1)<T\>

The comparer used to evaluate the value's range

#### Type Parameters

`T` 

The type of the value to be compared

#### Exceptions

 [NotInRangeException](Xunit.Sdk.NotInRangeException.md)

Thrown when the value is in the given range

### <a href="#Xunit_Assert_NotNull_System_Object_">NotNull\(object?\)</a>

Verifies that an object reference is not null.

```csharp
public static void NotNull(object? @object)
```

#### Parameters

`object` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The object to be validated

#### Exceptions

 [NotNullException](Xunit.Sdk.NotNullException.md)

Thrown when the object reference is null

### <a href="#Xunit_Assert_NotNull__1_System_Nullable___0__">NotNull<T\>\(T?\)</a>

Verifies that a nullable struct value is not null.

```csharp
public static T NotNull<T>(T? value) where T : struct
```

#### Parameters

`value` T?

The value to e validated

#### Returns

 T

The non-<code>null</code> value

#### Type Parameters

`T` 

The type of the struct

#### Exceptions

 [NotNullException](Xunit.Sdk.NotNullException.md)

Thrown when the value is null

### <a href="#Xunit_Assert_NotSame_System_Object_System_Object_">NotSame\(object?, object?\)</a>

Verifies that two objects are not the same instance.

```csharp
public static void NotSame(object? expected, object? actual)
```

#### Parameters

`expected` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The expected object instance

`actual` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The actual object instance

#### Exceptions

 [NotSameException](Xunit.Sdk.NotSameException.md)

Thrown when the objects are the same instance

### <a href="#Xunit_Assert_NotStrictEqual__1___0___0_">NotStrictEqual<T\>\(T, T\)</a>

Verifies that two objects are strictly not equal, using the type's default comparer.

```csharp
public static void NotStrictEqual<T>(T expected, T actual)
```

#### Parameters

`expected` T

The expected object

`actual` T

The actual object

#### Type Parameters

`T` 

The type of the objects to be compared

### <a href="#Xunit_Assert_Null_System_Object_">Null\(object?\)</a>

Verifies that an object reference is null.

```csharp
public static void Null(object? @object)
```

#### Parameters

`object` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The object to be inspected

#### Exceptions

 [NullException](Xunit.Sdk.NullException.md)

Thrown when the object reference is not null

### <a href="#Xunit_Assert_Null__1_System_Nullable___0__">Null<T\>\(T?\)</a>

Verifies that a nullable struct value is null.

```csharp
public static void Null<T>(T? value) where T : struct
```

#### Parameters

`value` T?

The value to be inspected

#### Type Parameters

`T` 

#### Exceptions

 [NullException](Xunit.Sdk.NullException.md)

Thrown when the value is not null

### <a href="#Xunit_Assert_ProperSubset__1_System_Collections_Generic_ISet___0__System_Collections_Generic_ISet___0__">ProperSubset<T\>\(ISet<T\>, ISet<T\>?\)</a>

Verifies that a set is a proper subset of another set.

```csharp
public static void ProperSubset<T>(ISet<T> expectedSubset, ISet<T>? actual)
```

#### Parameters

`expectedSubset` [ISet](https://learn.microsoft.com/dotnet/api/system.collections.generic.iset\-1)<T\>

The expected subset

`actual` [ISet](https://learn.microsoft.com/dotnet/api/system.collections.generic.iset\-1)<T\>?

The set expected to be a proper subset

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the actual set is not a proper subset of the expected set

### <a href="#Xunit_Assert_ProperSuperset__1_System_Collections_Generic_ISet___0__System_Collections_Generic_ISet___0__">ProperSuperset<T\>\(ISet<T\>, ISet<T\>?\)</a>

Verifies that a set is a proper superset of another set.

```csharp
public static void ProperSuperset<T>(ISet<T> expectedSuperset, ISet<T>? actual)
```

#### Parameters

`expectedSuperset` [ISet](https://learn.microsoft.com/dotnet/api/system.collections.generic.iset\-1)<T\>

The expected superset

`actual` [ISet](https://learn.microsoft.com/dotnet/api/system.collections.generic.iset\-1)<T\>?

The set expected to be a proper superset

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the actual set is not a proper superset of the expected set

### <a href="#Xunit_Assert_PropertyChanged_System_ComponentModel_INotifyPropertyChanged_System_String_System_Action_">PropertyChanged\(INotifyPropertyChanged, string, Action\)</a>

Verifies that the provided object raised <xref href="System.ComponentModel.INotifyPropertyChanged.PropertyChanged" data-throw-if-not-resolved="false"></xref>
as a result of executing the given test code.

```csharp
public static void PropertyChanged(INotifyPropertyChanged @object, string propertyName, Action testCode)
```

#### Parameters

`object` [INotifyPropertyChanged](https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanged)

The object which should raise the notification

`propertyName` [string](https://learn.microsoft.com/dotnet/api/system.string)

The property name for which the notification should be raised

`testCode` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The test code which should cause the notification to be raised

#### Exceptions

 [PropertyChangedException](Xunit.Sdk.PropertyChangedException.md)

Thrown when the notification is not raised

### <a href="#Xunit_Assert_PropertyChangedAsync_System_ComponentModel_INotifyPropertyChanged_System_String_System_Func_System_Threading_Tasks_Task__">PropertyChangedAsync\(INotifyPropertyChanged, string, Func<Task\>\)</a>

Verifies that the provided object raised <xref href="System.ComponentModel.INotifyPropertyChanged.PropertyChanged" data-throw-if-not-resolved="false"></xref>
as a result of executing the given test code.

```csharp
public static Task PropertyChangedAsync(INotifyPropertyChanged @object, string propertyName, Func<Task> testCode)
```

#### Parameters

`object` [INotifyPropertyChanged](https://learn.microsoft.com/dotnet/api/system.componentmodel.inotifypropertychanged)

The object which should raise the notification

`propertyName` [string](https://learn.microsoft.com/dotnet/api/system.string)

The property name for which the notification should be raised

`testCode` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)\>

The test code which should cause the notification to be raised

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

#### Exceptions

 [PropertyChangedException](Xunit.Sdk.PropertyChangedException.md)

Thrown when the notification is not raised

### <a href="#Xunit_Assert_Raises_System_Action_System_Action__System_Action_System_Action__System_Action_">Raises\(Action<Action\>, Action<Action\>, Action\)</a>

Verifies that an event is raised.

```csharp
public static void Raises(Action<Action> attach, Action<Action> detach, Action testCode)
```

#### Parameters

`attach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[Action](https://learn.microsoft.com/dotnet/api/system.action)\>

Code to attach the event handler

`detach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[Action](https://learn.microsoft.com/dotnet/api/system.action)\>

Code to detach the event handler

`testCode` [Action](https://learn.microsoft.com/dotnet/api/system.action)

A delegate to the code to be tested

#### Exceptions

 [RaisesException](Xunit.Sdk.RaisesException.md)

Thrown when the expected event was not raised.

### <a href="#Xunit_Assert_Raises__1_System_Action_System_Action___0___System_Action_System_Action___0___System_Action_">Raises<T\>\(Action<Action<T\>\>, Action<Action<T\>\>, Action\)</a>

Verifies that an event with the exact event args is raised.

```csharp
public static Assert.RaisedEvent<T> Raises<T>(Action<Action<T>> attach, Action<Action<T>> detach, Action testCode)
```

#### Parameters

`attach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<T\>\>

Code to attach the event handler

`detach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<T\>\>

Code to detach the event handler

`testCode` [Action](https://learn.microsoft.com/dotnet/api/system.action)

A delegate to the code to be tested

#### Returns

 [Assert](Xunit.Assert.md).[RaisedEvent](Xunit.Assert.RaisedEvent\-1.md)<T\>

The event sender and arguments wrapped in an object

#### Type Parameters

`T` 

The type of the event arguments to expect

#### Exceptions

 [RaisesException](Xunit.Sdk.RaisesException.md)

Thrown when the expected event was not raised.

### <a href="#Xunit_Assert_Raises__1_System_Action_System_EventHandler___0___System_Action_System_EventHandler___0___System_Action_">Raises<T\>\(Action<EventHandler<T\>\>, Action<EventHandler<T\>\>, Action\)</a>

Verifies that an event with the exact event args is raised.

```csharp
public static Assert.RaisedEvent<T> Raises<T>(Action<EventHandler<T>> attach, Action<EventHandler<T>> detach, Action testCode)
```

#### Parameters

`attach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler\-1)<T\>\>

Code to attach the event handler

`detach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler\-1)<T\>\>

Code to detach the event handler

`testCode` [Action](https://learn.microsoft.com/dotnet/api/system.action)

A delegate to the code to be tested

#### Returns

 [Assert](Xunit.Assert.md).[RaisedEvent](Xunit.Assert.RaisedEvent\-1.md)<T\>

The event sender and arguments wrapped in an object

#### Type Parameters

`T` 

The type of the event arguments to expect

#### Exceptions

 [RaisesException](Xunit.Sdk.RaisesException.md)

Thrown when the expected event was not raised.

### <a href="#Xunit_Assert_Raises__1_System_Func_Xunit_Assert_RaisedEvent___0___System_Action_System_Action_System_Action_">Raises<T\>\(Func<RaisedEvent<T\>?\>, Action, Action, Action\)</a>

Verifies that an event with the exact event args is raised.

```csharp
public static Assert.RaisedEvent<T> Raises<T>(Func<Assert.RaisedEvent<T>?> handler, Action attach, Action detach, Action testCode)
```

#### Parameters

`handler` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[Assert](Xunit.Assert.md).[RaisedEvent](Xunit.Assert.RaisedEvent\-1.md)<T\>?\>

Code returning the raised event

`attach` [Action](https://learn.microsoft.com/dotnet/api/system.action)

Code to attach the event handler

`detach` [Action](https://learn.microsoft.com/dotnet/api/system.action)

Code to detach the event handler

`testCode` [Action](https://learn.microsoft.com/dotnet/api/system.action)

A delegate to the code to be tested

#### Returns

 [Assert](Xunit.Assert.md).[RaisedEvent](Xunit.Assert.RaisedEvent\-1.md)<T\>

The event sender and arguments wrapped in an object

#### Type Parameters

`T` 

The type of the event arguments to expect

#### Exceptions

 [RaisesException](Xunit.Sdk.RaisesException.md)

Thrown when the expected event was not raised.

### <a href="#Xunit_Assert_RaisesAny_System_Action_System_EventHandler__System_Action_System_EventHandler__System_Action_">RaisesAny\(Action<EventHandler\>, Action<EventHandler\>, Action\)</a>

Verifies that an event is raised.

```csharp
public static Assert.RaisedEvent<EventArgs> RaisesAny(Action<EventHandler> attach, Action<EventHandler> detach, Action testCode)
```

#### Parameters

`attach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler)\>

Code to attach the event handler

`detach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler)\>

Code to detach the event handler

`testCode` [Action](https://learn.microsoft.com/dotnet/api/system.action)

A delegate to the code to be tested

#### Returns

 [Assert](Xunit.Assert.md).[RaisedEvent](Xunit.Assert.RaisedEvent\-1.md)<[EventArgs](https://learn.microsoft.com/dotnet/api/system.eventargs)\>

The event sender and arguments wrapped in an object

#### Exceptions

 [RaisesException](Xunit.Sdk.RaisesException.md)

Thrown when the expected event was not raised.

### <a href="#Xunit_Assert_RaisesAny__1_System_Action_System_Action___0___System_Action_System_Action___0___System_Action_">RaisesAny<T\>\(Action<Action<T\>\>, Action<Action<T\>\>, Action\)</a>

Verifies that an event with the exact or a derived event args is raised.

```csharp
public static Assert.RaisedEvent<T> RaisesAny<T>(Action<Action<T>> attach, Action<Action<T>> detach, Action testCode)
```

#### Parameters

`attach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<T\>\>

Code to attach the event handler

`detach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<T\>\>

Code to detach the event handler

`testCode` [Action](https://learn.microsoft.com/dotnet/api/system.action)

A delegate to the code to be tested

#### Returns

 [Assert](Xunit.Assert.md).[RaisedEvent](Xunit.Assert.RaisedEvent\-1.md)<T\>

The event sender and arguments wrapped in an object

#### Type Parameters

`T` 

The type of the event arguments to expect

#### Exceptions

 [RaisesException](Xunit.Sdk.RaisesException.md)

Thrown when the expected event was not raised.

### <a href="#Xunit_Assert_RaisesAny__1_System_Action_System_EventHandler___0___System_Action_System_EventHandler___0___System_Action_">RaisesAny<T\>\(Action<EventHandler<T\>\>, Action<EventHandler<T\>\>, Action\)</a>

Verifies that an event with the exact or a derived event args is raised.

```csharp
public static Assert.RaisedEvent<T> RaisesAny<T>(Action<EventHandler<T>> attach, Action<EventHandler<T>> detach, Action testCode)
```

#### Parameters

`attach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler\-1)<T\>\>

Code to attach the event handler

`detach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler\-1)<T\>\>

Code to detach the event handler

`testCode` [Action](https://learn.microsoft.com/dotnet/api/system.action)

A delegate to the code to be tested

#### Returns

 [Assert](Xunit.Assert.md).[RaisedEvent](Xunit.Assert.RaisedEvent\-1.md)<T\>

The event sender and arguments wrapped in an object

#### Type Parameters

`T` 

The type of the event arguments to expect

#### Exceptions

 [RaisesException](Xunit.Sdk.RaisesException.md)

Thrown when the expected event was not raised.

### <a href="#Xunit_Assert_RaisesAnyAsync_System_Action_System_EventHandler__System_Action_System_EventHandler__System_Func_System_Threading_Tasks_Task__">RaisesAnyAsync\(Action<EventHandler\>, Action<EventHandler\>, Func<Task\>\)</a>

Verifies that an event is raised.

```csharp
public static Task<Assert.RaisedEvent<EventArgs>> RaisesAnyAsync(Action<EventHandler> attach, Action<EventHandler> detach, Func<Task> testCode)
```

#### Parameters

`attach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler)\>

Code to attach the event handler

`detach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler)\>

Code to detach the event handler

`testCode` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)\>

A delegate to the code to be tested

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Assert](Xunit.Assert.md).[RaisedEvent](Xunit.Assert.RaisedEvent\-1.md)<[EventArgs](https://learn.microsoft.com/dotnet/api/system.eventargs)\>\>

The event sender and arguments wrapped in an object

#### Exceptions

 [RaisesException](Xunit.Sdk.RaisesException.md)

Thrown when the expected event was not raised.

### <a href="#Xunit_Assert_RaisesAnyAsync__1_System_Action_System_Action___0___System_Action_System_Action___0___System_Func_System_Threading_Tasks_Task__">RaisesAnyAsync<T\>\(Action<Action<T\>\>, Action<Action<T\>\>, Func<Task\>\)</a>

Verifies that an event with the exact or a derived event args is raised.

```csharp
public static Task<Assert.RaisedEvent<T>> RaisesAnyAsync<T>(Action<Action<T>> attach, Action<Action<T>> detach, Func<Task> testCode)
```

#### Parameters

`attach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<T\>\>

Code to attach the event handler

`detach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<T\>\>

Code to detach the event handler

`testCode` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)\>

A delegate to the code to be tested

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Assert](Xunit.Assert.md).[RaisedEvent](Xunit.Assert.RaisedEvent\-1.md)<T\>\>

The event sender and arguments wrapped in an object

#### Type Parameters

`T` 

The type of the event arguments to expect

#### Exceptions

 [RaisesException](Xunit.Sdk.RaisesException.md)

Thrown when the expected event was not raised.

### <a href="#Xunit_Assert_RaisesAnyAsync__1_System_Action_System_EventHandler___0___System_Action_System_EventHandler___0___System_Func_System_Threading_Tasks_Task__">RaisesAnyAsync<T\>\(Action<EventHandler<T\>\>, Action<EventHandler<T\>\>, Func<Task\>\)</a>

Verifies that an event with the exact or a derived event args is raised.

```csharp
public static Task<Assert.RaisedEvent<T>> RaisesAnyAsync<T>(Action<EventHandler<T>> attach, Action<EventHandler<T>> detach, Func<Task> testCode)
```

#### Parameters

`attach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler\-1)<T\>\>

Code to attach the event handler

`detach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler\-1)<T\>\>

Code to detach the event handler

`testCode` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)\>

A delegate to the code to be tested

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Assert](Xunit.Assert.md).[RaisedEvent](Xunit.Assert.RaisedEvent\-1.md)<T\>\>

The event sender and arguments wrapped in an object

#### Type Parameters

`T` 

The type of the event arguments to expect

#### Exceptions

 [RaisesException](Xunit.Sdk.RaisesException.md)

Thrown when the expected event was not raised.

### <a href="#Xunit_Assert_RaisesAsync_System_Action_System_Action__System_Action_System_Action__System_Func_System_Threading_Tasks_Task__">RaisesAsync\(Action<Action\>, Action<Action\>, Func<Task\>\)</a>

Verifies that an event is raised.

```csharp
public static Task RaisesAsync(Action<Action> attach, Action<Action> detach, Func<Task> testCode)
```

#### Parameters

`attach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[Action](https://learn.microsoft.com/dotnet/api/system.action)\>

Code to attach the event handler

`detach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[Action](https://learn.microsoft.com/dotnet/api/system.action)\>

Code to detach the event handler

`testCode` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)\>

A delegate to the code to be tested

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)

The event sender and arguments wrapped in an object

#### Exceptions

 [RaisesException](Xunit.Sdk.RaisesException.md)

Thrown when the expected event was not raised.

### <a href="#Xunit_Assert_RaisesAsync__1_System_Action_System_Action___0___System_Action_System_Action___0___System_Func_System_Threading_Tasks_Task__">RaisesAsync<T\>\(Action<Action<T\>\>, Action<Action<T\>\>, Func<Task\>\)</a>

Verifies that an event with the exact event args (and not a derived type) is raised.

```csharp
public static Task<Assert.RaisedEvent<T>> RaisesAsync<T>(Action<Action<T>> attach, Action<Action<T>> detach, Func<Task> testCode)
```

#### Parameters

`attach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<T\>\>

Code to attach the event handler

`detach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<T\>\>

Code to detach the event handler

`testCode` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)\>

A delegate to the code to be tested

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Assert](Xunit.Assert.md).[RaisedEvent](Xunit.Assert.RaisedEvent\-1.md)<T\>\>

The event sender and arguments wrapped in an object

#### Type Parameters

`T` 

The type of the event arguments to expect

#### Exceptions

 [RaisesException](Xunit.Sdk.RaisesException.md)

Thrown when the expected event was not raised.

### <a href="#Xunit_Assert_RaisesAsync__1_System_Action_System_EventHandler___0___System_Action_System_EventHandler___0___System_Func_System_Threading_Tasks_Task__">RaisesAsync<T\>\(Action<EventHandler<T\>\>, Action<EventHandler<T\>\>, Func<Task\>\)</a>

Verifies that an event with the exact event args (and not a derived type) is raised.

```csharp
public static Task<Assert.RaisedEvent<T>> RaisesAsync<T>(Action<EventHandler<T>> attach, Action<EventHandler<T>> detach, Func<Task> testCode)
```

#### Parameters

`attach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler\-1)<T\>\>

Code to attach the event handler

`detach` [Action](https://learn.microsoft.com/dotnet/api/system.action\-1)<[EventHandler](https://learn.microsoft.com/dotnet/api/system.eventhandler\-1)<T\>\>

Code to detach the event handler

`testCode` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)\>

A delegate to the code to be tested

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Assert](Xunit.Assert.md).[RaisedEvent](Xunit.Assert.RaisedEvent\-1.md)<T\>\>

The event sender and arguments wrapped in an object

#### Type Parameters

`T` 

The type of the event arguments to expect

#### Exceptions

 [RaisesException](Xunit.Sdk.RaisesException.md)

Thrown when the expected event was not raised.

### <a href="#Xunit_Assert_RecordException_System_Action_">RecordException\(Action\)</a>

Records any exception which is thrown by the given code.

```csharp
protected static Exception? RecordException(Action testCode)
```

#### Parameters

`testCode` [Action](https://learn.microsoft.com/dotnet/api/system.action)

The code which may thrown an exception.

#### Returns

 [Exception](https://learn.microsoft.com/dotnet/api/system.exception)?

Returns the exception that was thrown by the code; null, otherwise.

### <a href="#Xunit_Assert_RecordException_System_Func_System_Object__System_String_">RecordException\(Func<object?\>, string\)</a>

Records any exception which is thrown by the given code that has
a return value. Generally used for testing property accessors.

```csharp
protected static Exception? RecordException(Func<object?> testCode, string asyncMethodName)
```

#### Parameters

`testCode` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[object](https://learn.microsoft.com/dotnet/api/system.object)?\>

The code which may thrown an exception.

`asyncMethodName` [string](https://learn.microsoft.com/dotnet/api/system.string)

The name of the async method the user should've called if they accidentally
    passed in an async function

#### Returns

 [Exception](https://learn.microsoft.com/dotnet/api/system.exception)?

Returns the exception that was thrown by the code; null, otherwise.

### <a href="#Xunit_Assert_RecordExceptionAsync_System_Func_System_Threading_Tasks_Task__">RecordExceptionAsync\(Func<Task\>\)</a>

Records any exception which is thrown by the given task.

```csharp
protected static Task<Exception?> RecordExceptionAsync(Func<Task> testCode)
```

#### Parameters

`testCode` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)\>

The task which may thrown an exception.

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Exception](https://learn.microsoft.com/dotnet/api/system.exception)?\>

Returns the exception that was thrown by the code; null, otherwise.

### <a href="#Xunit_Assert_Same_System_Object_System_Object_">Same\(object?, object?\)</a>

Verifies that two objects are the same instance.

```csharp
public static void Same(object? expected, object? actual)
```

#### Parameters

`expected` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The expected object instance

`actual` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The actual object instance

#### Exceptions

 [SameException](Xunit.Sdk.SameException.md)

Thrown when the objects are not the same instance

### <a href="#Xunit_Assert_Single_System_Collections_IEnumerable_">Single\(IEnumerable\)</a>

Verifies that the given collection contains only a single
element of the given type.

```csharp
public static object? Single(IEnumerable collection)
```

#### Parameters

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.ienumerable)

The collection.

#### Returns

 [object](https://learn.microsoft.com/dotnet/api/system.object)?

The single item in the collection.

#### Exceptions

 [SingleException](Xunit.Sdk.SingleException.md)

Thrown when the collection does not contain
    exactly one element.

### <a href="#Xunit_Assert_Single_System_Collections_IEnumerable_System_Object_">Single\(IEnumerable, object?\)</a>

Verifies that the given collection contains only a single
element of the given value. The collection may or may not
contain other values.

```csharp
public static void Single(IEnumerable collection, object? expected)
```

#### Parameters

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.ienumerable)

The collection.

`expected` [object](https://learn.microsoft.com/dotnet/api/system.object)?

The value to find in the collection.

#### Exceptions

 [SingleException](Xunit.Sdk.SingleException.md)

Thrown when the collection does not contain
    exactly one element.

### <a href="#Xunit_Assert_Single__1_System_Collections_Generic_IEnumerable___0__">Single<T\>\(IEnumerable<T\>\)</a>

Verifies that the given collection contains only a single
element of the given type.

```csharp
public static T Single<T>(IEnumerable<T> collection)
```

#### Parameters

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>

The collection.

#### Returns

 T

The single item in the collection.

#### Type Parameters

`T` 

The collection type.

#### Exceptions

 [SingleException](Xunit.Sdk.SingleException.md)

Thrown when the collection does not contain
    exactly one element.

### <a href="#Xunit_Assert_Single__1_System_Collections_Generic_IEnumerable___0__System_Predicate___0__">Single<T\>\(IEnumerable<T\>, Predicate<T\>\)</a>

Verifies that the given collection contains only a single
element of the given type which matches the given predicate. The
collection may or may not contain other values which do not
match the given predicate.

```csharp
public static T Single<T>(IEnumerable<T> collection, Predicate<T> predicate)
```

#### Parameters

`collection` [IEnumerable](https://learn.microsoft.com/dotnet/api/system.collections.generic.ienumerable\-1)<T\>

The collection.

`predicate` [Predicate](https://learn.microsoft.com/dotnet/api/system.predicate\-1)<T\>

The item matching predicate.

#### Returns

 T

The single item in the filtered collection.

#### Type Parameters

`T` 

The collection type.

#### Exceptions

 [SingleException](Xunit.Sdk.SingleException.md)

Thrown when the filtered collection does
    not contain exactly one element.

### <a href="#Xunit_Assert_Skip_System_String_">Skip\(string\)</a>

Skips the current test. Used when determining whether a test should be skipped
happens at runtime rather than at discovery time.

```csharp
public static void Skip(string reason)
```

#### Parameters

`reason` [string](https://learn.microsoft.com/dotnet/api/system.string)

The message to indicate why the test was skipped

### <a href="#Xunit_Assert_SkipUnless_System_Boolean_System_String_">SkipUnless\(bool, string\)</a>

Will skip the current test unless <code class="paramref">condition</code> evaluates to <code>true</code>.

```csharp
public static void SkipUnless(bool condition, string reason)
```

#### Parameters

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <code>true</code>, the test will continue to run; when <code>false</code>,
    the test will be skipped

`reason` [string](https://learn.microsoft.com/dotnet/api/system.string)

The message to indicate why the test was skipped

### <a href="#Xunit_Assert_SkipWhen_System_Boolean_System_String_">SkipWhen\(bool, string\)</a>

Will skip the current test when <code class="paramref">condition</code> evaluates to <code>true</code>.

```csharp
public static void SkipWhen(bool condition, string reason)
```

#### Parameters

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

When <code>true</code>, the test will be skipped; when <code>false</code>,
    the test will continue to run

`reason` [string](https://learn.microsoft.com/dotnet/api/system.string)

The message to indicate why the test was skipped

### <a href="#Xunit_Assert_StartsWith_System_String_System_String_">StartsWith\(string?, string?\)</a>

Verifies that a string starts with a given string, using the current culture.

```csharp
public static void StartsWith(string? expectedStartString, string? actualString)
```

#### Parameters

`expectedStartString` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The string expected to be at the start of the string

`actualString` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The string to be inspected

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StartsWith_System_String_System_String_System_StringComparison_">StartsWith\(string?, string?, StringComparison\)</a>

Verifies that a string starts with a given sub-string, using the given comparison type.

```csharp
public static void StartsWith(string? expectedStartString, string? actualString, StringComparison comparisonType)
```

#### Parameters

`expectedStartString` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The sub-string expected to be at the start of the string

`actualString` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StartsWith_System_Memory_System_Char__System_Memory_System_Char__">StartsWith\(Memory<char\>, Memory<char\>\)</a>

Verifies that a string starts with a given sub-string, using the current culture.

```csharp
public static void StartsWith(Memory<char> expectedStartString, Memory<char> actualString)
```

#### Parameters

`expectedStartString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the start of the string

`actualString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [StartsWithException](Xunit.Sdk.StartsWithException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StartsWith_System_Memory_System_Char__System_ReadOnlyMemory_System_Char__">StartsWith\(Memory<char\>, ReadOnlyMemory<char\>\)</a>

Verifies that a string starts with a given sub-string, using the current culture.

```csharp
public static void StartsWith(Memory<char> expectedStartString, ReadOnlyMemory<char> actualString)
```

#### Parameters

`expectedStartString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the start of the string

`actualString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [StartsWithException](Xunit.Sdk.StartsWithException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StartsWith_System_ReadOnlyMemory_System_Char__System_Memory_System_Char__">StartsWith\(ReadOnlyMemory<char\>, Memory<char\>\)</a>

Verifies that a string starts with a given sub-string, using the current culture.

```csharp
public static void StartsWith(ReadOnlyMemory<char> expectedStartString, Memory<char> actualString)
```

#### Parameters

`expectedStartString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the start of the string

`actualString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [StartsWithException](Xunit.Sdk.StartsWithException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StartsWith_System_ReadOnlyMemory_System_Char__System_ReadOnlyMemory_System_Char__">StartsWith\(ReadOnlyMemory<char\>, ReadOnlyMemory<char\>\)</a>

Verifies that a string starts with a given sub-string, using the default StringComparison.CurrentCulture comparison type.

```csharp
public static void StartsWith(ReadOnlyMemory<char> expectedStartString, ReadOnlyMemory<char> actualString)
```

#### Parameters

`expectedStartString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the start of the string

`actualString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [StartsWithException](Xunit.Sdk.StartsWithException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StartsWith_System_Memory_System_Char__System_Memory_System_Char__System_StringComparison_">StartsWith\(Memory<char\>, Memory<char\>, StringComparison\)</a>

Verifies that a string starts with a given sub-string, using the given comparison type.

```csharp
public static void StartsWith(Memory<char> expectedStartString, Memory<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedStartString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the start of the string

`actualString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [StartsWithException](Xunit.Sdk.StartsWithException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StartsWith_System_Memory_System_Char__System_ReadOnlyMemory_System_Char__System_StringComparison_">StartsWith\(Memory<char\>, ReadOnlyMemory<char\>, StringComparison\)</a>

Verifies that a string starts with a given sub-string, using the given comparison type.

```csharp
public static void StartsWith(Memory<char> expectedStartString, ReadOnlyMemory<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedStartString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the start of the string

`actualString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [StartsWithException](Xunit.Sdk.StartsWithException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StartsWith_System_ReadOnlyMemory_System_Char__System_Memory_System_Char__System_StringComparison_">StartsWith\(ReadOnlyMemory<char\>, Memory<char\>, StringComparison\)</a>

Verifies that a string starts with a given sub-string, using the given comparison type.

```csharp
public static void StartsWith(ReadOnlyMemory<char> expectedStartString, Memory<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedStartString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the start of the string

`actualString` [Memory](https://learn.microsoft.com/dotnet/api/system.memory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [StartsWithException](Xunit.Sdk.StartsWithException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StartsWith_System_ReadOnlyMemory_System_Char__System_ReadOnlyMemory_System_Char__System_StringComparison_">StartsWith\(ReadOnlyMemory<char\>, ReadOnlyMemory<char\>, StringComparison\)</a>

Verifies that a string starts with a given sub-string, using the given comparison type.

```csharp
public static void StartsWith(ReadOnlyMemory<char> expectedStartString, ReadOnlyMemory<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedStartString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the start of the string

`actualString` [ReadOnlyMemory](https://learn.microsoft.com/dotnet/api/system.readonlymemory\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [StartsWithException](Xunit.Sdk.StartsWithException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StartsWith_System_Span_System_Char__System_Span_System_Char__">StartsWith\(Span<char\>, Span<char\>\)</a>

Verifies that a string starts with a given sub-string, using the current culture.

```csharp
public static void StartsWith(Span<char> expectedStartString, Span<char> actualString)
```

#### Parameters

`expectedStartString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the start of the string

`actualString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [StartsWithException](Xunit.Sdk.StartsWithException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StartsWith_System_Span_System_Char__System_ReadOnlySpan_System_Char__">StartsWith\(Span<char\>, ReadOnlySpan<char\>\)</a>

Verifies that a string starts with a given sub-string, using the current culture.

```csharp
public static void StartsWith(Span<char> expectedStartString, ReadOnlySpan<char> actualString)
```

#### Parameters

`expectedStartString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the start of the string

`actualString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [StartsWithException](Xunit.Sdk.StartsWithException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StartsWith_System_ReadOnlySpan_System_Char__System_Span_System_Char__">StartsWith\(ReadOnlySpan<char\>, Span<char\>\)</a>

Verifies that a string starts with a given sub-string, using the current culture.

```csharp
public static void StartsWith(ReadOnlySpan<char> expectedStartString, Span<char> actualString)
```

#### Parameters

`expectedStartString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the start of the string

`actualString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [StartsWithException](Xunit.Sdk.StartsWithException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StartsWith_System_ReadOnlySpan_System_Char__System_ReadOnlySpan_System_Char__">StartsWith\(ReadOnlySpan<char\>, ReadOnlySpan<char\>\)</a>

Verifies that a string starts with a given sub-string, using the current culture.

```csharp
public static void StartsWith(ReadOnlySpan<char> expectedStartString, ReadOnlySpan<char> actualString)
```

#### Parameters

`expectedStartString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the start of the string

`actualString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

#### Exceptions

 [StartsWithException](Xunit.Sdk.StartsWithException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StartsWith_System_Span_System_Char__System_Span_System_Char__System_StringComparison_">StartsWith\(Span<char\>, Span<char\>, StringComparison\)</a>

Verifies that a string starts with a given sub-string, using the given comparison type.

```csharp
public static void StartsWith(Span<char> expectedStartString, Span<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedStartString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the start of the string

`actualString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [StartsWithException](Xunit.Sdk.StartsWithException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StartsWith_System_Span_System_Char__System_ReadOnlySpan_System_Char__System_StringComparison_">StartsWith\(Span<char\>, ReadOnlySpan<char\>, StringComparison\)</a>

Verifies that a string starts with a given sub-string, using the given comparison type.

```csharp
public static void StartsWith(Span<char> expectedStartString, ReadOnlySpan<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedStartString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the start of the string

`actualString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [StartsWithException](Xunit.Sdk.StartsWithException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StartsWith_System_ReadOnlySpan_System_Char__System_Span_System_Char__System_StringComparison_">StartsWith\(ReadOnlySpan<char\>, Span<char\>, StringComparison\)</a>

Verifies that a string starts with a given sub-string, using the given comparison type.

```csharp
public static void StartsWith(ReadOnlySpan<char> expectedStartString, Span<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedStartString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the start of the string

`actualString` [Span](https://learn.microsoft.com/dotnet/api/system.span\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [StartsWithException](Xunit.Sdk.StartsWithException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StartsWith_System_ReadOnlySpan_System_Char__System_ReadOnlySpan_System_Char__System_StringComparison_">StartsWith\(ReadOnlySpan<char\>, ReadOnlySpan<char\>, StringComparison\)</a>

Verifies that a string starts with a given sub-string, using the given comparison type.

```csharp
public static void StartsWith(ReadOnlySpan<char> expectedStartString, ReadOnlySpan<char> actualString, StringComparison comparisonType = StringComparison.CurrentCulture)
```

#### Parameters

`expectedStartString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The sub-string expected to be at the start of the string

`actualString` [ReadOnlySpan](https://learn.microsoft.com/dotnet/api/system.readonlyspan\-1)<[char](https://learn.microsoft.com/dotnet/api/system.char)\>

The string to be inspected

`comparisonType` [StringComparison](https://learn.microsoft.com/dotnet/api/system.stringcomparison)

The type of string comparison to perform

#### Exceptions

 [StartsWithException](Xunit.Sdk.StartsWithException.md)

Thrown when the string does not start with the expected sub-string

### <a href="#Xunit_Assert_StrictEqual__1___0___0_">StrictEqual<T\>\(T, T\)</a>

Verifies that two objects are strictly equal, using the type's default comparer.

```csharp
public static void StrictEqual<T>(T expected, T actual)
```

#### Parameters

`expected` T

The expected value

`actual` T

The value to be compared against

#### Type Parameters

`T` 

The type of the objects to be compared

### <a href="#Xunit_Assert_Subset__1_System_Collections_Generic_ISet___0__System_Collections_Generic_ISet___0__">Subset<T\>\(ISet<T\>, ISet<T\>?\)</a>

Verifies that a set is a subset of another set.

```csharp
public static void Subset<T>(ISet<T> expectedSubset, ISet<T>? actual)
```

#### Parameters

`expectedSubset` [ISet](https://learn.microsoft.com/dotnet/api/system.collections.generic.iset\-1)<T\>

The expected subset

`actual` [ISet](https://learn.microsoft.com/dotnet/api/system.collections.generic.iset\-1)<T\>?

The set expected to be a subset

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the actual set is not a subset of the expected set

### <a href="#Xunit_Assert_Superset__1_System_Collections_Generic_ISet___0__System_Collections_Generic_ISet___0__">Superset<T\>\(ISet<T\>, ISet<T\>?\)</a>

Verifies that a set is a superset of another set.

```csharp
public static void Superset<T>(ISet<T> expectedSuperset, ISet<T>? actual)
```

#### Parameters

`expectedSuperset` [ISet](https://learn.microsoft.com/dotnet/api/system.collections.generic.iset\-1)<T\>

The expected superset

`actual` [ISet](https://learn.microsoft.com/dotnet/api/system.collections.generic.iset\-1)<T\>?

The set expected to be a superset

#### Type Parameters

`T` 

The type of the object to be verified

#### Exceptions

 [ContainsException](Xunit.Sdk.ContainsException.md)

Thrown when the actual set is not a superset of the expected set

### <a href="#Xunit_Assert_Throws_System_Type_System_Action_">Throws\(Type, Action\)</a>

Verifies that the exact exception is thrown (and not a derived exception type).

```csharp
public static Exception Throws(Type exceptionType, Action testCode)
```

#### Parameters

`exceptionType` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The type of the exception expected to be thrown

`testCode` [Action](https://learn.microsoft.com/dotnet/api/system.action)

A delegate to the code to be tested

#### Returns

 [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

The exception that was thrown, when successful

### <a href="#Xunit_Assert_Throws_System_Type_System_Func_System_Object__">Throws\(Type, Func<object?\>\)</a>

Verifies that the exact exception is thrown (and not a derived exception type).
Generally used to test property accessors.

```csharp
public static Exception Throws(Type exceptionType, Func<object?> testCode)
```

#### Parameters

`exceptionType` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The type of the exception expected to be thrown

`testCode` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[object](https://learn.microsoft.com/dotnet/api/system.object)?\>

A delegate to the code to be tested

#### Returns

 [Exception](https://learn.microsoft.com/dotnet/api/system.exception)

The exception that was thrown, when successful

### <a href="#Xunit_Assert_Throws__1_System_Action_">Throws<T\>\(Action\)</a>

Verifies that the exact exception is thrown (and not a derived exception type).

```csharp
public static T Throws<T>(Action testCode) where T : Exception
```

#### Parameters

`testCode` [Action](https://learn.microsoft.com/dotnet/api/system.action)

A delegate to the code to be tested

#### Returns

 T

The exception that was thrown, when successful

#### Type Parameters

`T` 

The type of the exception expected to be thrown

### <a href="#Xunit_Assert_Throws__1_System_Func_System_Object__">Throws<T\>\(Func<object?\>\)</a>

Verifies that the exact exception is thrown (and not a derived exception type).
Generally used to test property accessors.

```csharp
public static T Throws<T>(Func<object?> testCode) where T : Exception
```

#### Parameters

`testCode` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[object](https://learn.microsoft.com/dotnet/api/system.object)?\>

A delegate to the code to be tested

#### Returns

 T

The exception that was thrown, when successful

#### Type Parameters

`T` 

The type of the exception expected to be thrown

### <a href="#Xunit_Assert_Throws__1_System_String_System_Action_">Throws<T\>\(string?, Action\)</a>

Verifies that the exact exception is thrown (and not a derived exception type), where the exception
derives from <xref href="System.ArgumentException" data-throw-if-not-resolved="false"></xref> and has the given parameter name.

```csharp
public static T Throws<T>(string? paramName, Action testCode) where T : ArgumentException
```

#### Parameters

`paramName` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The parameter name that is expected to be in the exception

`testCode` [Action](https://learn.microsoft.com/dotnet/api/system.action)

A delegate to the code to be tested

#### Returns

 T

The exception that was thrown, when successful

#### Type Parameters

`T` 

### <a href="#Xunit_Assert_Throws__1_System_String_System_Func_System_Object__">Throws<T\>\(string?, Func<object?\>\)</a>

Verifies that the exact exception is thrown (and not a derived exception type), where the exception
derives from <xref href="System.ArgumentException" data-throw-if-not-resolved="false"></xref> and has the given parameter name.

```csharp
public static T Throws<T>(string? paramName, Func<object?> testCode) where T : ArgumentException
```

#### Parameters

`paramName` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The parameter name that is expected to be in the exception

`testCode` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[object](https://learn.microsoft.com/dotnet/api/system.object)?\>

A delegate to the code to be tested

#### Returns

 T

The exception that was thrown, when successful

#### Type Parameters

`T` 

### <a href="#Xunit_Assert_ThrowsAny__1_System_Action_">ThrowsAny<T\>\(Action\)</a>

Verifies that the exact exception or a derived exception type is thrown.

```csharp
public static T ThrowsAny<T>(Action testCode) where T : Exception
```

#### Parameters

`testCode` [Action](https://learn.microsoft.com/dotnet/api/system.action)

A delegate to the code to be tested

#### Returns

 T

The exception that was thrown, when successful

#### Type Parameters

`T` 

The type of the exception expected to be thrown

### <a href="#Xunit_Assert_ThrowsAny__1_System_Func_System_Object__">ThrowsAny<T\>\(Func<object?\>\)</a>

Verifies that the exact exception or a derived exception type is thrown.
Generally used to test property accessors.

```csharp
public static T ThrowsAny<T>(Func<object?> testCode) where T : Exception
```

#### Parameters

`testCode` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[object](https://learn.microsoft.com/dotnet/api/system.object)?\>

A delegate to the code to be tested

#### Returns

 T

The exception that was thrown, when successful

#### Type Parameters

`T` 

The type of the exception expected to be thrown

### <a href="#Xunit_Assert_ThrowsAnyAsync__1_System_Func_System_Threading_Tasks_Task__">ThrowsAnyAsync<T\>\(Func<Task\>\)</a>

Verifies that the exact exception or a derived exception type is thrown.

```csharp
public static Task<T> ThrowsAnyAsync<T>(Func<Task> testCode) where T : Exception
```

#### Parameters

`testCode` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)\>

A delegate to the task to be tested

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<T\>

The exception that was thrown, when successful

#### Type Parameters

`T` 

The type of the exception expected to be thrown

### <a href="#Xunit_Assert_ThrowsAsync_System_Type_System_Func_System_Threading_Tasks_Task__">ThrowsAsync\(Type, Func<Task\>\)</a>

Verifies that the exact exception is thrown (and not a derived exception type).

```csharp
public static Task<Exception> ThrowsAsync(Type exceptionType, Func<Task> testCode)
```

#### Parameters

`exceptionType` [Type](https://learn.microsoft.com/dotnet/api/system.type)

The type of the exception expected to be thrown

`testCode` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)\>

A delegate to the task to be tested

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<[Exception](https://learn.microsoft.com/dotnet/api/system.exception)\>

The exception that was thrown, when successful

### <a href="#Xunit_Assert_ThrowsAsync__1_System_Func_System_Threading_Tasks_Task__">ThrowsAsync<T\>\(Func<Task\>\)</a>

Verifies that the exact exception is thrown (and not a derived exception type).

```csharp
public static Task<T> ThrowsAsync<T>(Func<Task> testCode) where T : Exception
```

#### Parameters

`testCode` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)\>

A delegate to the task to be tested

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<T\>

The exception that was thrown, when successful

#### Type Parameters

`T` 

The type of the exception expected to be thrown

### <a href="#Xunit_Assert_ThrowsAsync__1_System_String_System_Func_System_Threading_Tasks_Task__">ThrowsAsync<T\>\(string?, Func<Task\>\)</a>

Verifies that the exact exception is thrown (and not a derived exception type), where the exception
derives from <xref href="System.ArgumentException" data-throw-if-not-resolved="false"></xref> and has the given parameter name.

```csharp
public static Task<T> ThrowsAsync<T>(string? paramName, Func<Task> testCode) where T : ArgumentException
```

#### Parameters

`paramName` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The parameter name that is expected to be in the exception

`testCode` [Func](https://learn.microsoft.com/dotnet/api/system.func\-1)<[Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task)\>

A delegate to the task to be tested

#### Returns

 [Task](https://learn.microsoft.com/dotnet/api/system.threading.tasks.task\-1)<T\>

The exception that was thrown, when successful

#### Type Parameters

`T` 

### <a href="#Xunit_Assert_True_System_Boolean_">True\(bool\)</a>

Verifies that an expression is true.

```csharp
public static void True(bool condition)
```

#### Parameters

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

The condition to be inspected

#### Exceptions

 [TrueException](Xunit.Sdk.TrueException.md)

Thrown when the condition is false

### <a href="#Xunit_Assert_True_System_Nullable_System_Boolean__">True\(bool?\)</a>

Verifies that an expression is true.

```csharp
public static void True(bool? condition)
```

#### Parameters

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)?

The condition to be inspected

#### Exceptions

 [TrueException](Xunit.Sdk.TrueException.md)

Thrown when the condition is false

### <a href="#Xunit_Assert_True_System_Boolean_System_String_">True\(bool, string?\)</a>

Verifies that an expression is true.

```csharp
public static void True(bool condition, string? userMessage)
```

#### Parameters

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)

The condition to be inspected

`userMessage` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The message to be shown when the condition is false

#### Exceptions

 [TrueException](Xunit.Sdk.TrueException.md)

Thrown when the condition is false

### <a href="#Xunit_Assert_True_System_Nullable_System_Boolean__System_String_">True\(bool?, string?\)</a>

Verifies that an expression is true.

```csharp
public static void True(bool? condition, string? userMessage)
```

#### Parameters

`condition` [bool](https://learn.microsoft.com/dotnet/api/system.boolean)?

The condition to be inspected

`userMessage` [string](https://learn.microsoft.com/dotnet/api/system.string)?

The message to be shown when the condition is false

#### Exceptions

 [TrueException](Xunit.Sdk.TrueException.md)

Thrown when the condition is false

