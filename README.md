# Sharpify

A collection of high performance language extensions for C#

## Features

* ⚡ Fully Native AOT compatible
* 🤷 `Either<T0, T1>` - Discriminated union object that forces handling of both cases
* 🦾 Flexible `Result` type that can encapsulate any other type and adds a massage options and a success or failure status. Flexible as it doesn't require any special handling to use (unlike `Either`)
* 🏄 Wrapper extensions that simplify use of common functions and advanced features from the `CollectionsMarshal` class
* `Routine` and `AsyncRoutine` bring the user easily usable and configurable interval based background job execution.
* `PersistentDictionary` and derived types are super lightweight and efficient serializable dictionaries that are thread-safe and work amazingly for things like configuration files.
* `SortedList{T}` bridges the performance of `List` and order assurance of `SortedSet`
* `SerializableObject` and the `Monitored` variant allow persisting an object to the disk, and elegantly synchronizing modifications.
* 💿 `StringBuffer` enable zero allocation, easy to use appending buffer for creation of string in hot paths.
* `RentedBufferWriter{T}` is an alternative to `ArrayBufferWriter{T}` that requires upfront estimation of the capacity, to use an array rented from the shared array pool, reducing memory allocations and garbage collection.
* A 🚣🏻 boat load of extension functions for all common types, bridging ease of use and performance.
* `Utils.Env`, `Utils.Math`, `Utils.Strings` and `Utils.Unsafe` provide uncanny convenience at maximal performance.
* 🧵 `ThreadSafe{T}` makes any variable type thread-safe
* 🔐 `AesProvider` provides access to industry leading AES-128 encryption with virtually no setup
* 🏋️ High performance optimized alternatives to core language extensions
* 🎁 More added features that are not present in the core language
* ❗ Static inner exception throwers guide the JIT to further optimize the code during runtime.
* 🫴 Focus on giving the user complete control by using flexible and common types, and resulting types that can be further used and just viewed.

## Demos

The main repository contains a folder named demos, with time more and more demos will be added, each demo will be accompanied by a tutorial on YouTube.

## ⬇ Installation

* `Sharpify` [![Nuget](https://img.shields.io/nuget/dt/Sharpify?label=Nuget%20Downloads)](https://www.nuget.org/packages/Sharpify/)
* `Sharpify.Data` [![Nuget](https://img.shields.io/nuget/dt/Sharpify.Data?label=Nuget%20Downloads)](https://www.nuget.org/packages/Sharpify.Data/)
* `Sharpify.CommandLineInterface` [![Nuget](https://img.shields.io/nuget/dt/Sharpify.CommandLineInterface?label=Nuget%20Downloads)](https://www.nuget.org/packages/Sharpify.CommandLineInterface/)

## Sharpify.Data

`Sharpify.Data` is an extension package, that should be installed on-top of `Sharpify` and adds a high performance persistent key-value-pair database, utilizing [MemoryPack](https://github.com/Cysharp/MemoryPack). The database support multiple types in the same file, 2 stage AES encryption (for whole file and per-key). Filtering by type, Single or Array value per key, and more...

For more information check [inner directory](Sharpify.Data/README.md).

## Sharpify.CommandLineInterface

`Sharpify.CommandLineInterface` is another extension package that adds a high performance, reflection free and `AOT-ready` framework for creating command line interfaces

For more information check [inner directory](Sharpify.CommandLineInterface/README.md)

## Methodology

As the name suggests - `Sharpify` is a package mainly intended to extend the core language using high performance implementations. `Sharpify` will never be guaranteed to be backwards compatible, and each release may contain breaking changes as it tries to adapt to the latest language features. `.NET` has a very active community and many features will be added to the core language that will perform at some point better than what `Sharpify` currently offers, at which point these features will be removed from `Sharpify` to encourage user to use the base language instead.

One such example could be the planed addition of union types in C# 14, which if goes as planned will trigger the removal of `Either<T0, T1>` and `Result<T>` from `Sharpify` as they are not needed anymore.

Even though backwards compatibility is not guaranteed, at the latest version, `Sharpify` is made to be very stable and reliable, there are many-many tests made to ensure that. And if something breaks, make sure to report it as it will be considered high priority.

If your packages / libraries use `Sharpify`, I recommend locking the dependency to a specific version which you test.

## Contribution

This packages was made public so that the entire community could benefit from it. If you experience issues, want to suggest new features or improve existing ones, please use the [issues](https://github.com/dusrdev/Sharpify/issues) section.

## Contact

For bug reports, feature requests or offers of support/sponsorship contact <dusrdev@gmail.com>

> This project is proudly made in Israel 🇮🇱 for the benefit of mankind.
