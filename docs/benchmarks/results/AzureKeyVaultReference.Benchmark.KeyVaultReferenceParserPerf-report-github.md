```

BenchmarkDotNet v0.13.7, Windows 11 (10.0.22621.1992/22H2/2022Update/SunValley2)
AMD Ryzen 5 1600, 1 CPU, 12 logical and 6 physical cores
.NET SDK 7.0.307
  [Host] : .NET 6.0.21 (6.0.2123.36311), X64 RyuJIT AVX2
  2.0.4  : .NET 6.0.21 (6.0.2123.36311), X64 RyuJIT AVX2
  dev    : .NET 6.0.21 (6.0.2123.36311), X64 RyuJIT AVX2

IterationCount=15  LaunchCount=2  WarmupCount=10  

```
|                         Method |   Job |        Mean |     Error |    StdDev | Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|------------------------------- |------ |------------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
|    IsKeyVaultReferenceWhenItIs | 2.0.4 |   101.50 ns |  0.839 ns |  1.120 ns |  1.00 |    0.00 |      - |         - |          NA |
|    IsKeyVaultReferenceWhenItIs |   dev |    90.30 ns |  0.250 ns |  0.343 ns |  0.89 |    0.01 |      - |         - |          NA |
|                                |       |             |           |           |       |         |        |           |             |
| IsKeyVaultReferenceWhenItIsNot | 2.0.4 |    43.40 ns |  0.217 ns |  0.305 ns |  1.00 |    0.00 |      - |         - |          NA |
| IsKeyVaultReferenceWhenItIsNot |   dev |    47.45 ns |  0.171 ns |  0.240 ns |  1.09 |    0.01 |      - |         - |          NA |
|                                |       |             |           |           |       |         |        |           |             |
|               TryParseWithName | 2.0.4 | 1,681.31 ns | 14.770 ns | 21.183 ns |  1.00 |    0.00 | 0.5684 |    2384 B |        1.00 |
|               TryParseWithName |   dev | 1,673.04 ns |  9.158 ns | 12.838 ns |  1.00 |    0.02 | 0.5684 |    2384 B |        1.00 |
|                                |       |             |           |           |       |         |        |           |             |
|                TryParseWithUri | 2.0.4 | 2,069.75 ns | 11.353 ns | 15.916 ns |  1.00 |    0.00 | 0.5112 |    2152 B |        1.00 |
|                TryParseWithUri |   dev | 2,080.43 ns | 18.672 ns | 26.779 ns |  1.01 |    0.01 | 0.5112 |    2152 B |        1.00 |
