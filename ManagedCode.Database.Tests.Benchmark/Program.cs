using Benchmark;
using Benchmark.DB;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ManagedCode.ZoneTree.Cluster.DB;

//new ZoneTreeBenchmarks().ZoneTree_Insert_100_000_Batch_250();
//new ZoneTreeBenchmarks().ZoneTree_Insert_100_000_Batch_250();
//new ZoneTreeBenchmarks().ZoneTree_Count();
//new ZoneTreeBenchmarks().ZoneTree_Insert_100_000_Batch_250();
//new ZoneTreeBenchmarks().ZoneTree_Insert_100_000_Batch_250();

var summary = BenchmarkRunner.Run(typeof(Program).Assembly);