using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

var job = Job.Default.WithArguments([new MsBuildArgument("/m:1"),
    new MsBuildArgument("/p:BuildInParallel=false"), new MsBuildArgument("/p:GeneratePackageOnBuild=false")]).DontEnforcePowerPlan().AsMutator();
var config = ManualConfig.Create(DefaultConfig.Instance).AddJob(job);
var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config).ToArray();
if (args.Any(argument => argument is "--list" or "--help" or "--info" or "--version")) return 0;
return summaries.Length > 0 && summaries.All(summary => summary.Reports.Length > 0 && summary.Reports.All(report => report.Success)) ? 0 : 1;
