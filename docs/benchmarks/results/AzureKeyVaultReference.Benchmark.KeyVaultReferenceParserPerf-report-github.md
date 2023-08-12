```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.1992/22H2/2022Update/SunValley2)
AMD Ryzen 5 1600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 7.0.307
  [Host] : .NET 6.0.21 (6.0.2123.36311), X64 RyuJIT AVX2
  2.0.4  : .NET 6.0.21 (6.0.2123.36311), X64 RyuJIT AVX2
  dev    : .NET 6.0.21 (6.0.2123.36311), X64 RyuJIT AVX2

IterationCount=15  LaunchCount=2  WarmupCount=10  

```
|                         Method |   Job |        Mean |     Error |    StdDev | Ratio |   Gen0 | Allocated | Alloc Ratio |
|------------------------------- |------ |------------:|----------:|----------:|------:|-------:|----------:|------------:|
|    IsKeyVaultReferenceWhenItIs | 2.0.4 |    95.99 ns |  0.894 ns |  1.283 ns |  1.00 |      - |         - |          NA |
|    IsKeyVaultReferenceWhenItIs |   dev |    93.37 ns |  0.463 ns |  0.693 ns |  0.97 |      - |         - |          NA |
|                                |       |             |           |           |       |        |           |             |
| IsKeyVaultReferenceWhenItIsNot | 2.0.4 |    51.56 ns |  0.426 ns |  0.625 ns |  1.00 |      - |         - |          NA |
| IsKeyVaultReferenceWhenItIsNot |   dev |    42.69 ns |  0.320 ns |  0.479 ns |  0.83 |      - |         - |          NA |
|                                |       |             |           |           |       |        |           |             |
|               TryParseWithName | 2.0.4 | 1,644.09 ns | 12.014 ns | 17.610 ns |  1.00 | 0.5684 |    2384 B |        1.00 |
|               TryParseWithName |   dev | 1,624.76 ns |  6.684 ns |  9.150 ns |  0.99 | 0.5684 |    2384 B |        1.00 |
|                                |       |             |           |           |       |        |           |             |
|                TryParseWithUri | 2.0.4 | 2,025.15 ns | 15.213 ns | 22.299 ns |  1.00 | 0.5112 |    2152 B |        1.00 |
|                TryParseWithUri |   dev | 2,030.39 ns |  6.614 ns |  9.899 ns |  1.00 | 0.5112 |    2152 B |        1.00 |
