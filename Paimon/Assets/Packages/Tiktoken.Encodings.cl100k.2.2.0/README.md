# Tiktoken

[![Nuget package](https://img.shields.io/nuget/vpre/Tiktoken)](https://www.nuget.org/packages/Tiktoken/)
[![dotnet](https://github.com/tryAGI/Tiktoken/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/tryAGI/Tiktoken/actions/workflows/dotnet.yml)
[![License: MIT](https://img.shields.io/github/license/tryAGI/Tiktoken)](https://github.com/tryAGI/Tiktoken/blob/main/LICENSE.txt)
[![Discord](https://img.shields.io/discord/1115206893015662663?label=Discord&logo=discord&logoColor=white&color=d82679)](https://discord.gg/Ca2xhfBf3v)

This implementation aims for maximum performance, especially in the token count operation.  
There's also a benchmark console app here for easy tracking of this.  
We will be happy to accept any PR.  

### Implemented encodings
- `o200k_base`
- `cl100k_base`
- `r50k_base`
- `p50k_base`
- `p50k_edit`

### Usage
```csharp
using Tiktoken;

var encoder = ModelToEncoder.For("gpt-4o"); // or explicitly using new Encoder(new O200KBase())
var tokens = encoder.Encode("hello world"); // [15339, 1917]
var text = encoder.Decode(tokens); // hello world
var numberOfTokens = encoder.CountTokens(text); // 2
var stringTokens = encoder.Explore(text); // ["hello", " world"]
```

### Benchmarks
You can view the reports for each version [here](benchmarks)

<!--BENCHMARKS_START-->
```

BenchmarkDotNet v0.14.0, macOS Sequoia 15.1 (24B83) [Darwin 24.1.0]
Apple M1 Pro, 1 CPU, 10 logical and 10 physical cores
.NET SDK 9.0.100
  [Host]     : .NET 9.0.0 (9.0.24.52809), Arm64 RyuJIT AdvSIMD
  DefaultJob : .NET 9.0.0 (9.0.24.52809), Arm64 RyuJIT AdvSIMD


```
| Method                            | Categories  | Data                | Mean         | Ratio | Gen0     | Gen1    | Allocated | Alloc Ratio |
|---------------------------------- |------------ |-------------------- |-------------:|------:|---------:|--------:|----------:|------------:|
| **SharpTokenV2_0_3_**                 | **CountTokens** | **1. (...)57. [19866]** | **567,130.0 ns** |  **1.00** |   **2.9297** |       **-** |   **20115 B** |        **1.00** |
| TiktokenSharpV1_1_5_              | CountTokens | 1. (...)57. [19866] | 483,976.7 ns |  0.85 |  64.4531 |  5.8594 |  404648 B |       20.12 |
| MicrosoftMLTokenizerV1_0_0_       | CountTokens | 1. (...)57. [19866] | 427,733.2 ns |  0.75 |        - |       - |     297 B |        0.01 |
| TokenizerLibV1_3_3_               | CountTokens | 1. (...)57. [19866] | 773,467.5 ns |  1.36 | 246.0938 | 83.9844 | 1547675 B |       76.94 |
| Tiktoken_                         | CountTokens | 1. (...)57. [19866] | 271,564.3 ns |  0.48 |  23.4375 |       - |  148313 B |        7.37 |
|                                   |             |                     |              |       |          |         |           |             |
| **SharpTokenV2_0_3_**                 | **CountTokens** | **Hello, World!**       |     **380.0 ns** |  **1.00** |   **0.0405** |       **-** |     **256 B** |        **1.00** |
| TiktokenSharpV1_1_5_              | CountTokens | Hello, World!       |     263.8 ns |  0.69 |   0.0505 |       - |     320 B |        1.25 |
| MicrosoftMLTokenizerV1_0_0_       | CountTokens | Hello, World!       |     305.7 ns |  0.80 |   0.0153 |       - |      96 B |        0.38 |
| TokenizerLibV1_3_3_               | CountTokens | Hello, World!       |     509.6 ns |  1.34 |   0.2356 |  0.0010 |    1480 B |        5.78 |
| Tiktoken_                         | CountTokens | Hello, World!       |     175.7 ns |  0.46 |   0.0191 |       - |     120 B |        0.47 |
|                                   |             |                     |              |       |          |         |           |             |
| **SharpTokenV2_0_3_**                 | **CountTokens** | **King(...)edy. [275]** |   **5,990.7 ns** |  **1.00** |   **0.0763** |       **-** |     **520 B** |        **1.00** |
| TiktokenSharpV1_1_5_              | CountTokens | King(...)edy. [275] |   4,516.5 ns |  0.75 |   0.8011 |       - |    5064 B |        9.74 |
| MicrosoftMLTokenizerV1_0_0_       | CountTokens | King(...)edy. [275] |   3,871.2 ns |  0.65 |   0.0153 |       - |      96 B |        0.18 |
| TokenizerLibV1_3_3_               | CountTokens | King(...)edy. [275] |   7,465.8 ns |  1.25 |   3.0823 |  0.1373 |   19344 B |       37.20 |
| Tiktoken_                         | CountTokens | King(...)edy. [275] |   2,744.5 ns |  0.46 |   0.3128 |       - |    1976 B |        3.80 |
|                                   |             |                     |              |       |          |         |           |             |
| **SharpTokenV2_0_3_Encode**           | **Encode**      | **1. (...)57. [19866]** | **568,150.3 ns** |  **1.00** |   **2.9297** |       **-** |   **20115 B** |        **1.00** |
| TiktokenSharpV1_1_5_Encode        | Encode      | 1. (...)57. [19866] | 444,972.1 ns |  0.78 |  64.4531 |  5.8594 |  404649 B |       20.12 |
| MicrosoftMLTokenizerV1_0_0_Encode | Encode      | 1. (...)57. [19866] | 410,970.9 ns |  0.72 |  10.2539 |  0.4883 |   66137 B |        3.29 |
| TokenizerLibV1_3_3_Encode         | Encode      | 1. (...)57. [19866] | 770,068.9 ns |  1.36 | 246.0938 | 90.8203 | 1547675 B |       76.94 |
| Tiktoken_Encode                   | Encode      | 1. (...)57. [19866] | 290,030.9 ns |  0.51 |  33.6914 |  1.4648 |  214465 B |       10.66 |
|                                   |             |                     |              |       |          |         |           |             |
| **SharpTokenV2_0_3_Encode**           | **Encode**      | **Hello, World!**       |     **381.2 ns** |  **1.00** |   **0.0405** |       **-** |     **256 B** |        **1.00** |
| TiktokenSharpV1_1_5_Encode        | Encode      | Hello, World!       |     260.2 ns |  0.68 |   0.0505 |       - |     320 B |        1.25 |
| MicrosoftMLTokenizerV1_0_0_Encode | Encode      | Hello, World!       |     325.1 ns |  0.85 |   0.0267 |       - |     168 B |        0.66 |
| TokenizerLibV1_3_3_Encode         | Encode      | Hello, World!       |     511.6 ns |  1.34 |   0.2356 |       - |    1480 B |        5.78 |
| Tiktoken_Encode                   | Encode      | Hello, World!       |     241.4 ns |  0.63 |   0.0801 |       - |     504 B |        1.97 |
|                                   |             |                     |              |       |          |         |           |             |
| **SharpTokenV2_0_3_Encode**           | **Encode**      | **King(...)edy. [275]** |   **5,957.3 ns** |  **1.00** |   **0.0763** |       **-** |     **520 B** |        **1.00** |
| TiktokenSharpV1_1_5_Encode        | Encode      | King(...)edy. [275] |   4,523.8 ns |  0.76 |   0.8011 |       - |    5064 B |        9.74 |
| MicrosoftMLTokenizerV1_0_0_Encode | Encode      | King(...)edy. [275] |   4,069.8 ns |  0.68 |   0.1144 |       - |     744 B |        1.43 |
| TokenizerLibV1_3_3_Encode         | Encode      | King(...)edy. [275] |   7,207.8 ns |  1.21 |   3.0823 |  0.1373 |   19344 B |       37.20 |
| Tiktoken_Encode                   | Encode      | King(...)edy. [275] |   2,945.7 ns |  0.49 |   0.4654 |       - |    2936 B |        5.65 |

<!--BENCHMARKS_END-->

## Support

Priority place for bugs: https://github.com/tryAGI/LangChain/issues  
Priority place for ideas and general questions: https://github.com/tryAGI/LangChain/discussions  
Discord: https://discord.gg/Ca2xhfBf3v  