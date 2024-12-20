# CHANGELOG

## v2.5.0

* Updated to support .NET 9.0 and optimized certain methods to use .NET 9 specific API's wherever possible.
* Added `BufferWrapper<T>` which can be used to append items to a `Span<T>` without managing indexes and capacity. This buffer also implement `IBufferWriter<T>`, and as a `ref struct implementing an interface` it is only available on .NET 9.0 and above.
* `Utils.String.FormatBytes` now uses a much larger buffer size of 512 chars by default, to handle the edge case of `double.MaxValue` which would previously cause an `ArgumentOutOfRangeException` to be thrown or similarly any number of bytes that would be bigger than 1024 petabytes. The result will now also include thousands separators to improve readability.
  * The inner implementation that uses this buffer size is pooled so this should not have any impact on performance.

## v2.4.0

* All derived types of `PersistentDictionary` now implement `IDisposable` interface.
* Main concurrent processing methods are now `ICollection<T>.ForAll()` and `ICollection<T>.ForAllAsync()` from many, many benchmarks it became clear that for short duration tasks not involving heavy compute, it has by far the best compromise of speed and memory-allocation. If you use it with a non `async function` all the tasks will yield immediately and require virtually no allocations at all. Which is as good as `ValueTask` from benchmarks. This method has 2 overloads, one which accepts an `IAsyncAction` which enables users with long code and many captured variables to maintain a better structured codebase, and a `Func` alternative for quick and easy usage. The difference in memory allocation / execution time between time is nearly non-existent, this mainly for maintainability.
* For heavier compute tasks, please revert to using `Parallel.For` or `Parallel.ForEachAsync` and their overloads, they are excellent in load balancing.
* Due to the changes above, all other concurrent processing methods, such as `ForEachAsync`, `InvokeAsync` and all the related functionality from the `Concurrent` class have been removed. `AsAsyncLocal` entry is also removed, and users will be able to access the new `ForAll` and `ForAllAsync` methods directly from the `ICollection<T>` interface static extensions.
* `ForAll` and `ForAllAsync` methods have identical parameters, the only difference is that the implementation for `ForAll` is optimized for synchronous lambdas that don't need to allocate an AsyncStateMachine, while the `ForAllAsync` is optimized for increased concurrency for asynchronous lambdas. Choosing `ForAll` for synchronous lambdas massively decreases memory allocation and execution time.
* `IAsyncAction<T>`'s `InvokeAsync` method now has a `CancellationToken` parameter.
* Changes to `TimeSpan` related functions:
  * `Format`, `FormatNonAllocated`, `ToRemainingDuration`, `ToRemainingDurationNonAllocated`, `ToTimeStamp`, `ToTimeStampNonAllocated`, were all removed due to duplication and suboptimal implementations.
  * The new methods replacing these functionalities are now in `Utils.DateAndTime` namespace.
  * `FormatTimeSpan` is now replacing `Format` and `FormatNonAllocated`, `FormatTimeSpan` is hyper optimized. The first overload requires a `Span{char}` buffer of at least 30 characters, and returns a `ReadOnlySpan{char}` of the written portion. The second doesn't require a buffer, and allocated a new `string` which is returned. `FormatTimeSpan` outputs a different format than the predecessor, as the time was formatted in decimal and is rather confusing, now it is formatted as `00:00unit` for the largest 2 units. So a minute and a half would be `01:30m` and a day and a half would be `02:30d` etc... this seems more intuitive to me.
  * `FormatTimeStamp` is now replacing `ToTimeStamp` and `ToTimeStampNonAllocated`, it is also optimized and the overloads work the same way as `FormatTimeSpan`.
* The `StringBuffer` which previously rented arrays from the shared array pool, then used the same API's to write to it as `AllocatedStringBuffer` was removed. The previous `AllocatedStringBuffer` was now renamed to `StringBuffer` and it requires a pre-allocated `Span{char}`. You can get the same functionality by renting any buffer, and simply supplying to `StringBuffer.Create`. This allowed removal of a lot of duplicated code and made the API more consistent. `StringBuffer` now doesn't have an implicit converter to `ReadOnlySpan{char}` anymore, use `StringBuffer.WrittenSpan` instead.
* `IModifier{T}` was removed, use `Func<T, T>` instead.
* `Utils.Strings.FormatBytes` was changed in the same manner as `Utils.DateAndTime.FormatTimeSpan` and `Utils.DateAndTime.FormatTimeStamp`, it now returns a `ReadOnlySpan<char>` instead of a `string` and it is optimized to use less memory.
* `ThreadSafe<T>` now implements `IEquatable<T>` and `IEquatable<ThreadSafe<T>>` to allow comparisons.

## v2.2.0

* `SortedList`
  * `GetIndex` now is not simplified, instead returns pure result of performing a binary search:
    * if the item exists, a zero based index
    * if not, a negative number that is the bitwise complement of the index of the next element that is larger than the item or, if there is no larger element, the bitwise complement of `Count` property.
    * With this you can still find out if something exists by checking for negative, but you can now also use the negative bitwise complement of the index to get section of the list in relation to the item, best used with the span.
  * Added `AddRange` functions for `ReadOnlySpan{T}` and `IEnumerable{T}`
* Collection extensions `ToArrayFast` and `ToListFast` were removed after being marked as `Obsolete` in the previous version, the `LINQ` alternatives perform only marginally slower, but will improve passively under the hood, consider using them instead.

## v2.1.0

* Changes to `RentedBufferWriter{T}`:
  * `RenterBufferWriter{T}` no longer throws an exception when the initial capacity is set to 0, instead it will just be disabled, this can be checked with the `.IsDisabled` property. Setting the initial capacity to a negative number will still throw an exception. This change was made to accommodate use cases where a `RentedBufferWriter` could be used as a return type, before an "invalid" operation, could not be attained, as it would've been required to give a valid capacity in any case, lest you risk a runtime exception. Now you could actually return a "Disabled" `RentedBufferWriter` if you set the capacity to 0, which intuitively means that the buffer doesn't actually have a backing array, and all operations would throw an exception.
  * To increase it's usability, a method `WriteAndAdvance` was also added that accepts either a single `T` or a `ReadOnlySpan{T}`, it checks if there is enough capacity to write the data to the buffer, if so it writes it and advances the position automatically.
  * A secondary access `ref T[] GetReferenceUnsafe` was added, to allow passing the inner buffer to methods that write to a `ref T[]` which previously required using unsafe code to manipulate the pointers. As implied by the name, this uses `Unsafe` to acquire the reference, and should only be used if you are carful and know what you are doing.
* Collection extension methods such as `ToArrayFast()` and `ToListFast()` were made deprecated, use the `ToArray()` and `ToList()` LINQ methods instead, with time they become the fastest possible implementation and will continue to improve, the performance gap is already minimal, and only improves speed, not memory allocations, which makes it negligible.

## v2.0.0

* Performance improvements to parallel extensions that use `AsyncLocal`
  * The changes are *BREAKING* as now the call sites should use newer convention, behavior will be the same.
  * Instead of the previous `.AsAsyncLocal`, there are now 2 overloads, both use nested generics which is the reason for the update, instead of `IList<T>` they use `<TList, TItem>` where `TList : IList<TItem>`. The first overload with no additional parameters can be used but it will require to specify both generic types as the compiler cannot infer `TItem` for some reason. To partially compensate for the verbosity it creates when using complex types, a second overload accepts a `TItem? @default` argument. Specifying `default(TItem)` there will enable the compiler to infer the generic.

  ```csharp
  // EXAMPLE
  var items = List<int>() { 1, 2, 3 };
  _ = items.AsAsyncLocal<List<int>, int>(); // Overload 1 with no arguments
  _ = items.AsAsyncLocal(default(int)); // Overload 2 with TItem? @default
  ```

  * This might seem like a step backwards but using a nested generic here, can improve performance by not virtualizing interface calls to `IList<T>`, and also enable the compiler to more accurately trim the code when compiling to `NativeAOT`.
* Both overloads of `DecryptBytes` in `AesProvider` now have an optional parameter `throwOnError` which is set to `false` by default, retaining the previous behavior. Setting it to `true` will make sure the exception is thrown in case of a `CryptographicException` instead of just returning an empty array or 0 (respective of the overloads). This might help catch certain issues.
  * Note: Previously and still now, exceptions other than `CryptographicException` would and will be thrown. They are not caught in the methods.
* Implement a trick in `RemoveDuplicatesFromSorted`, `RemoveDuplicates`, `RemoveDuplicatesSorted` and `TryConvertToInt32` to ensure the compiler knows to remove the bounds checking from the loops.
* `IComparer<T>` parameter type in `SortedList<T>` was changed to `Comparer<T>`, the interface virtualization seemed to make custom comparers produce unexpected results.
* `StringBuffer` and `AllocatedStringBuffer` can now use the builder pattern to chain its construction, same as `StringBuilder`.

  ```csharp
  var buffer = StringBuffer.Create(stackalloc char[20]).Append("Hello").AppendLine().Append("World");
  ```

  * This change required each method to return the actual reference of the struct, Use this with caution as this means you can change the actual reference instead of a copy if you manually assign it a new value.
  * From my testing, with correct usage, without manually assigning values to the original variable, previous and builder pattern usage passes my tests. If you encounter any issue with this, make sure to submit it in the GitHub repo.
* Fixed edge case where pausing an `AsyncRoutine` would stop it permanently, regardless of calls to `Resume`.

## v1.8.1

* Small performance improvement to `Array.ToListFast()`

## v1.8.0

* `AllocatedStringBuffer` now has variation that accepts a `char[]` as input buffer, this version which also has a corresponding overload in `StringBuffer.Create` supports an implicit converter to `ReadOnlyMemory<char>`, intellisense will allow you to use this converter even you used `Span<char>` as a buffer, but doing so will cause an exception to be thrown, as a `Span<char>` can be `stack allocated` and won't be able to be referenced.
* The method above paved the way to creating a `FormatNonAllocated(TimeSpan)`, `ToRemainingDurationNonAllocated(TimeSpan)`, `ToTimeStampNonAllocated(TimeSpan)`, and `FormatBytesNonAllocated(double)`, these overloads would format directly to span, and return a slice, this would completely bypass the `string` allocation, which would've otherwise caused large amounts of GC overhead in frequent calls.
* Added `ToArrayFast(List)` and `ToListFast(Array)` extension methods, that efficiently create an array from list and the other way around.
* `SerializableObject<T>` and `MonitoredSerializableObject<T>` now require a `JsonTypeInfo<T>` instead of a `JsonSerializerContext` to improve type safety, as the `JsonSerializerContext` overloads cannot verify that the context has any implementation for the type, potentially leading to exceptions at runtime.

## v1.7.3

* Made improvements to all `IDisposable` implementing types making their `Dispose` implementations idempotent (consecutive calls will just be ignored, eliminating exceptions that can occur when misused).
* Added a new `AsyncLocal<IList<T>>` extension `ForEach` overload that uses a synchronous `IAction<T>` implementation, it should be the more modern approach to executing synchronous but parallel actions, it also has parameters for `degreeOfParallelism` and `cancellationToken`, and it also returns a `ValueTask`, which means you should await it, and you can attempt to cancel it if you want. It should also perform a bit better as now it is using a partitioner internally. In the case of performing synchronous operations on parallel, there is less room for speed optimization that can occur by jumping between tasks when a task is just waiting like it is possible in async, here your main tool to optimize will be the figure out the right amount of `degreeOfParallelism`, taking into account the capabilities of the system and complexity of each action. Nevertheless, the default (-1) is not a bad baseline.
  * If you want the call site to be sync as well, use the "older" `ForEach` `Concurrent` overload, as calling the new `ValueTask` overload in a non-async context, will degrade performance.

## v1.7.2

* Moved `_lock` acquiring statements into the `try-finally` blocks to handle `out-of-band` exceptions. Thanks [TheGenbox](https://www.reddit.com/user/TheGenbox/).
* Modified most of the internal `await` statements to use `ConfigureAwait(false)` wherever it was possible, also thanks to [TheGenbox](https://www.reddit.com/user/TheGenbox/).

## v1.7.0

* Added `AppendLine` overloads to all `Append` methods of the `StringBuffer` variants that append the platform specific new line sequence at after the `Append`, there is also an empty overload which just appends the new line sequence. This was made to reduce code when using newlines, and to make the api even more similar to `StringBuilder`.
* Added `RentedBufferWriter{T}` to `Sharpify.Collections`, it is an implementation of the `IBufferWriter{T}` that uses an array rented from the shared array pool as the backing buffer, it provides an allocation free alternative to `ArrayBufferWriter{T}`.
* Heavily implement array pooling in `AesProvider`, increasing performance and reducing memory allocations in virtually all APIs.
* Added another `CopyToArray` extension for `HashSet{T}` which enables usage of pre-existing buffer, which in turn enables usage of pooling.
* Improved performance of `LazyPersistentDictionary` reads using pooling.
* Reduced memory usage in initialization of `SerializableObject{T}`
* `AsyncRoutine` with the option `ExecuteOnParallel` now also uses pooling, virtually eliminating memory allocations (which used to happen regularly per the execution interval)
* Optimized `FibonacciApproximation` in `Utils.Mathematics`

## v1.6.0

* `Sharpify` is now fully AOT-Compatible!!
* Performed IO optimizations to `LocalPersistentDictionary` and `LazyLocalPersistentDictionary` and configured to use compile-time JSON.
* **BREAKING** `ReturnRentedBuffer` was renamed to `ReturnBufferToSharedArrayPool` to better explain what it does,
Also added a method `ReturnToArrayPool` which takes an `ArrayPool` in case anyone wanted an extension method to be used with custom `ArrayPool`. The main reason for both these extensions is because the `ArrayPool`s generic type is on the class and not the method, it usually can't be inferred, resulting in longer and more difficult code to write. The extension methods hide all of the generic types because they are made in a format which the compiler can infer from.
* **BREAKING** `SerializableObject` and `MonitoredSerializableObject` now require a `JsonSerializationContext` parameter, that makes them use compile time AOT compatible serialization. This was required in order to make the entire library AOT compatible.

## v1.5.0

* **BREAKING** `StringBuffer`s and `AllocatedStringBuffer`s constructor have been made internal, to enforce usage of the factory methods. The factory methods of both are now under `StringBuffer` and vary by name to indicate the type you are getting back. `StringBuffer.Rent(capacity)` will return a `StringBuffer` which rents memory from the array pool. And `StringBuffer.Create(Span{char})` will return an `AllocatedStringBuffer` which works on pre-allocated buffer. `StringBuffer` still implements `IDisposable` so should be used together with a `using` statement or keyword. Also, the implicit converters should now be prioritized to be inlined by the compiler.
  * It should be noted that in some cases **JetBrains Rider** marks it an error when the implicit operator to string is used in return statements of a method, this is not an actual error, it compiles and works fine, rather seems as an intellisense failure. If this bothers you, use `.Allocate(true)` instead, it does the same thing.
* Added newer faster and memory memory efficient concurrent APIs whose entry point is `AsyncLocal<IList<T>>`, for more information check the updated `Parallel` section in the repo Wiki.
* Added `Dictionary{K,V}.CopyTo((K,V)[], index)` extension (as built-in one isn't available without casting.)
* Added `RentBufferAndCopyEntries(Dictionary<K,V>)` extension method that returns a tuple of the reference to the rented buffer and array segment over the buffer with the correct range, simplifying renting buffers for a copy of the dictionary contents to perform parallel operations on it.
* Added `ReturnRentedBuffer(T[])` extension to supplement the entry above, or with any `ArrayPool<T>.Shared.Rent` call.

## v1.4.2

* Updated synchronization aspect of `SerializableObject{T}` and `MonitoredSerializableObject{T}`, they now both implement `IDisposable` and finalizers in case you forget to dispose of them, or their context makes it inconvenient.

## v1.4.0 - v1.4.1

* Introduced new `SerializableObject{T}` class that will serialize an object to a file and expose an event that will fire on once the object has changed, also a variant `MonitoredSerializableObject` that has the same functionality and in addition it will monitor for external changes within the file system and synchronize them as well.
* Added implicit converters to `ReadOnlySpan{Char}` for `StringBuffer` and `AllocatedStringBuffer`, which can enable usage of the buffer without any allocation in api's that accept `ReadOnlySpan{Char}`.
* Added `[Flags]` attribute to `RoutineOptions` to calm down some IDEs.
* Updated `AesProvider.EncryptBytes` and `AesProvider.DecryptBytes` to use `ReadOnlySpan{byte}` parameters
* Added `AesProvider.EncryptBytes` and `AesProvider.DecryptBytes` overloads that encrypt into a destination span, with guides to length requirements in the summary.
* Added implicit converter to `ReadOnlyMemory{char}` for `StringBuffer` that might help usage in some cases.

## v1.3.1

* Fixed issue where assemblies inside the nuget package were older than the package version

## v1.3.0

* Addressed issue which resulted in some parts of the library having older implementations when downloading the package using nuget.
* Added new `StringBuffer` collection, which rents a buffer of specified length, and allows efficient appending of elements without using any low level apis, indexing or slice management. And with zero costs to performance (tested in benchmarks), for smaller lengths it is more recommended to use `AllocatedStringBuffer` with `stackalloc`, for larger than about 1024 characters it would be better to use `StringBuffer` as the it would create less pressure on the system, at those scales `stackalloc` can become slow and sometimes may even fail.
* Added new `AllocateStringBuffer` which is similar to `StringBuffer` but requires a preallocated buffer - allowing usage of `stackalloc`.
* `StringBuffer` and `AllocatedStringBuffer` were internally integrated to replace almost all low level buffer manipulations
* `AesProvider.EncryptUrl` and `AesProvider.DecryptUrl` in **.NET8 or later** were optimized to minimize allocations and using hardware intrinsics api's for character replacement.
* **BREAKING** Removed `String.Suffix` as the abstraction is the same as `String.Concat` which already uses a very good implementation
* Updated project properties for better end user support via nuget.

## v1.2.0

* Modifications to `PersistentDictionary`:
  * `Upsert` has been renamed to `UpsertAsync` to make its nature more obvious (Possible **BREAKING** change)
  * `Upsert` now handles a special case in which the key exists and value is the same as new value, it will completely forgo the operation, requiring no `Task` creation and no serialization.
  * `GetOrCreateAsync(key, val)` and `UpsertAsync(key, val)` now return a `ValueTask` reducing resource usage
  * `PersistentDictionary` now uses a regular `Dictionary` as the internal data structure to be lighter and handle reads even faster. This is the ***BREAKING** change as custom inherited types will need to be updated to also serialize and deserialize to a regular `Dictionary`.
  * To allow concurrent writes, a very efficient and robust concurrency model using a `ConcurrentQueue` and a `SemaphoreSlim` is used. It perfect conditions it will even reduce serialization counts.
  * The base `Dictionary` is also not nullable anymore, which reduces null checks.
  * More methods of `PersistentDictionary` that had a base implementation were marked as `virtual` for more customization options with inheritance.
  * Overloads for `T` types were added to both `GetOrCreateAsync(key, T val)` and `UpsertAsync(key, T val)` to make usage even easier for primitive types, and they both rely on the `string` overloads so that inherited types would'nt need to implement both.
  * `LocalPersistentDictionary` and `LazyLocalPersistentDictionary` were both updated to support this new structure and also now utilize a single internal instance of the `JsonOptions` for serialization, thus reducing resource usage in some scenarios.
  * `LazyLocalPersistentDictionary` get key implementation was revised and improved to reduce memory allocations.
  * Edge cases of concurrent writing with `PersistentDictionary` are very hard to detect in unit tests due to inconsistencies in executing upserts in parallel, if you encounter any issues, please post the issue in the repo or email me.
* Added `OpenLink(string url)` function to `Utils.Env` that supports opening a link on Windows, Mac, and Linux
* `Result.Message` and `Result<T>.Message` is no longer nullable, and instead will default to an empty string.
* `string.GetReference` extension
* Added `Result.Fail` overloads that support a value, to allow usage of static defaults or empty collections
* Added `HashSet.ToArrayFast()` method which converts a hash set to an array more efficiently than Linq.
* Further optimized `AesProvider.GeneratePassword`
* **BREAKING**, The `FormatBytes` function for `long` and `double` was moved to `Utils.Strings` class and is no longer an extension function, this will make the usage clearer.
* Further optimized `TimeSpan.Format`
* Multiple string creating functions which used to stack allocate the buffers, now rent them instead, potentially reducing overall application memory usage.
* Added another class `Utils.Unsafe` that has "hacky" utilities that allow you to reuse existing code in other high performance apis
* New exceptions were added to validate function input in places where the JIT could you use this information to optimize the code by removing bound checks and such.
* **BREAKING** all of the `ConvertToInt32` methods were removed, an in place a method `TryConvertToInt32(ReadOnlySpan{char}, out int result)` was added, it is more efficient and generic as it can work for signed and unsigned integers by relaying on the bool as the operation status.
* `SortedList<T>`'s fields were changed to be protected and not private, this will make inheritance if you so choose, also added an implicit operator that will return the inner list for places which require a list input.

## v1.1.0

* Changed nullability of return type of `PersistentDictionary.GetOrCreateAsync(key, val)`
* Finalized features, seems it is a good place to end the patches and start the next minor release

## v1.0.9

* Updated to support .NET 8.0
* Added `GetOrCreateAsync(key, val)` method to `PersistentDictionary`
* Further performance improvement to the `FormatBytes` extension
* `Either`s default empty constructor now throws an exception instead of simply warning during usage.
* `AesProvider.IsPasswordValid` was further optimized using spans (**Only applies when running > .NET8**)
* Updated outdates summary documentations

## v1.0.8

* Added 2 new persistent dictionary types: `LocalPersistentDictionary` and `LazyLocalPersistentDictionary`
  * Both of them Inherit from `PersistentDictionary`, they are essentially a `ConcurrentDictionary<string, string>` data store, which is optimized for maximal performance.
  * `LocalPersistentDictionary` requires a local path and utilizes Json to serialize and deserialize the dictionary, requiring minimal setup.
  * `LazyLocalPersistentDictionary` is similar to `LocalPersistentDictionary` but doesn't keep a permanent copy in-memory. Instead it loads and unloads the dictionary per operation.
  * Do not be mistaken by the simplicity of the `ConcurrentDictionary<string, string>` base type, as the string value allows you as much complexity as you want. You can create entire types for the value and just pass their to the dictionary.
  * `PersistentDictionary` is an abstract class which lays the ground work for creating such dictionaries with efficiency and thread-safety. You can create your own implementation easily by inheriting the class, you will need at the very least to override `SerializeAsync` and `Deserialize` and create your own constructors for setup. It is also possible to override `GetValueByKey` and `SetKeyAndValue` which allows you to implement lazy loading for example. The flexibility of the serialization is what gives you the option to persist the dictionary to where ever you choose, even an online database. For examples just look how `LocalPersistentDictionary` and `LazyLocalPersistentDictionary` are implemented in the source code.
  * Both types support a `StringComparer` parameter allowing you to customize the dictionary key management protocol, perhaps you want to ignore case, this is how you configure it.
* Added new extension method `ICollection<T>.IsNullOrEmpty` that check if it is null or empty using pattern matching.
* Added new function `Utils.Env.PathInBaseDirectory(filename)` that returns the combined path of the base directory of the executable and the filename.

## v1.0.7

* Performance increase in `RollingAverage` and `FibonacciApproximation`
* changes to `List.RemoveDuplicates`:
  * api change: parameter `isSorted` was moved to be after the `comparer` override, since it usually is used less frequently.
  * Another overload is available which accepts an `out HashSet<T>` parameter that can return the already allocated `HashSet` that was used to check the collection. Using it with `isSorted = true` is discouraged as the algorithm doesn't use n `HashSet` in that case, and it would be more efficient to just `new HashSet(list)` in that case.
* Small performance and stability enhancement in `DateTime.ToTimeStamp`
* `Concurrent.InvokeAsync` memory usage further optimized when using large collections by using an array with exact item count.
* Added new `Routines` namespace that includes two types: `Routine` and `AsyncRoutine`
  * Both types allow you to create a routine/background job that will execute a series of functions on a requested interval. And both support configuration with the `Builder` pattern.
  * `Routine` is the simplest and lightest that works best with simple actions.
  * `AsyncRoutine` is more complex and made specifically to accommodate async functions, it manages an async timer that will execute a collection of async functions. It has a `CancellationTokenSource` that will manage the cancellation of the timer itself and each of the functions. If you want more control you can provide it yourself. Despite the fact that `AsyncRoutine` can be configured using the `Builder` pattern, unlike `Routine`, the `Start` method returns a task, so to avoid loosing track of the routine, **DO NOT** call `Start` in the same call to the configuration.
  * `RoutineOptions` is an enum that is accepted to configure an `AsyncRoutine` and currently has 2 options:
    1. `ExecuteInParallel` this will create execute the functions provided in parallel in every tick, this may increase memory allocation since parallel execution requires a collection of tasks to be re-created upon every execution. But, it might provide a speed benefit when using long-running background functions in the routine.
    2. `ThrowOnCancellation`, stopping a task using a `cancellationToken` inevitably throws a `TaskCancelledException`. By default to make the routine easier to use it ignores this exception as it should only occur by design. If you toggle this option, it will re-throw the exception and you will be required to handle it. If you want to ensure that the routine finishes when you want to without controlling the token, simply call `Dispose` on the routine.
* New collection type `SortedList<T>` in `Sharpify.Collections`, it is a List that maintains a sorted order, unlike the original `SortedList<K,V>` which is based on a sorted dictionary and a tree. This isn't, it is lighter, more customizable. And enables super efficient iteration and even internal `Span<T>` access
  * Performance stats:
  * Initialization from collection: O(nlogn)
  * Add and remove item: O(logn)
  * Get by index: O(1)

## v1.0.6

* New `RemoveDuplicates` extensions for `List<T>` was added, it is implemented using a more efficient algorithm, and has an optional parameter `isSorted` that allows further optimization. There is also an optional `IEqualityComparer<T>` parameter for types that their default comparer doesn't provide accurate results
* Performance enhancements to `AesProvider`
* New `ChunkToSegments` extension method for arrays that returns `List<ArraySegment>>` that can be used to segment array and process them concurrently, which is the most efficient way to do this as `Enumerable.Chunk` will actually create new arrays (much more memory allocation), and `span`s are very limited when it comes to concurrent processing with `Task`s.
* Optimized `base64` related methods in `AesProvider`
* Several methods that relied on exception throwing for invalid input parameters, no instead use `Debug.Assert` to improve performance on Release builds.

### Breaking Changes

* `List` extensions: `RemovedDuplicatesSorted`, `SortAndRemoveDuplicates` have been removed. Their functionality is replaces with the new `RemoveDuplicates` extension

## v1.0.5

THIS UPDATE MAY INTRODUCES THE FOLLOWING BREAKING CHANGES BUT THEY ARE REQUIRED FOR FURTHER STABILITY

* Updated `Result` and `Result<T>` to disallow default constructors, thus enforcing use of factory methods such as `Result.OK(message)` and `Result.Fail(message)` and their overloads.
* Updated `Concurrent` to also disallow default constructor, enforcing use of `ICollection.Concurrent()` extension method.

## v1.0.4

* Added url encryption and decryption functions to `AesProvider`
* Added more safeguards that prevent exceptions when `plain` text input is passed as `encrypted` to decryption functions. They now return an empty output, either `byte[]` or `string.Empty`, depends on the method.

## v1.0.3

* Fixed implementation of `IModifier<T>` to better fit the requirements of `Func<T, T>`

## v1.0.2

* Introduces a new `ThreadSafe<T>` wrapper which makes any type thread-safe
* Introduces a new `AesProvider` class with enables no-setup access to encryption

### `ThreadSafe<T>` Notes

* Access the value by using `ThreadSafe<T>.Value`
* Modify the value by using `ThreadSafe<T>.Modify(Func<T, T>)` or `ThreadSafe<T>.Modify(IModifier<T>)`
* `IModifier<T>` allows you to create an operation that will be better optimized than a `Func<T, T>` and possibly avoid the memory allocation penalty associated with `delegates`
* Unlike the `Interlocked` api's, which require using `ref` thus making them unusable in `async` methods, `ThreadSafe<T>` doesn't, which makes it much more usable

### `AesProvider` Notes

* Has `string` key based constructor that takes care of the headache for padding and key size for you
* Has methods for encrypting and decrypting both `string`s and `byte[]`s
* Provides an option to generate an `ICryptoTransform` for either encryption or decryption to fit special needs
* Properly implements `IDisposable` with api notices to prevent memory leaks
* Has static methods for generating hashed passwords and validating them

## v1.0.1

* `Utils` class was made upper class of `Env`, `Mathematics` and `DateAndTime` to allow better categorization and maintenance
* Fixed invalid options in the .csproj file

### `Utils.Env`

* Added `GetBaseFolder` which returns the base path of the application directory
* Added `IsRunningAsAdmin` and `IsRunningOnWindows` which are self-explanatory
* Added `IsInternetAvailable` which checks for internet connection

### `Utils.Mathematics`

* Added `FibonacciApproximation` and `Factorial` functions
