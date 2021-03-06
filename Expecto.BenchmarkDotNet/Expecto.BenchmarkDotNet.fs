namespace Expecto

open BenchmarkDotNet
open BenchmarkDotNet.Running
open BenchmarkDotNet.Configs
open BenchmarkDotNet.Analysers
open BenchmarkDotNet.Columns
open BenchmarkDotNet.Diagnosers
open BenchmarkDotNet.Exporters
open BenchmarkDotNet.Filters
open BenchmarkDotNet.Jobs
open BenchmarkDotNet.Loggers
open BenchmarkDotNet.Order
open BenchmarkDotNet.Validators

[<AutoOpen>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module BenchmarkDotNet =

  type BenchmarkAttribute = BenchmarkDotNet.Attributes.BenchmarkAttribute
  type CleanupAttribute = BenchmarkDotNet.Attributes.GlobalCleanupAttribute
  type SetupAttribute = BenchmarkDotNet.Attributes.GlobalSetupAttribute

  type BenchmarkConfig =
    { columnProviders: IColumnProvider list
      hardwareCounters: HardwareCounter list
      summaryStyle: Reports.ISummaryStyle
      exporters : IExporter list
      loggers : ILogger list
      diagnosers : IDiagnoser list
      analysers : IAnalyser list
      jobs : Job list
      validators : IValidator list
      orderProvider : IOrderProvider
      unionRule : ConfigUnionRule
      keepFiles : bool
      filters : IFilter list
    }

    interface IConfig with
      member x.GetColumnProviders() = x.columnProviders :> seq<IColumnProvider>
      member x.GetHardwareCounters() = x.hardwareCounters :> seq<_>
      member x.GetSummaryStyle()     = x.summaryStyle
      member x.GetExporters()       = x.exporters  :> seq<IExporter>
      member x.GetLoggers()         = x.loggers    :> seq<ILogger>
      member x.GetDiagnosers()      = x.diagnosers :> seq<IDiagnoser>
      member x.GetAnalysers()       = x.analysers  :> seq<IAnalyser>
      member x.GetJobs()            = x.jobs       :> seq<Job>
      member x.GetValidators()      = x.validators :> seq<IValidator>
      member x.GetOrderProvider()   = x.orderProvider : IOrderProvider
      member x.UnionRule            = x.unionRule : ConfigUnionRule
      /// Determines if all auto-generated files should be kept or removed after running benchmarks
      member x.KeepBenchmarkFiles = x.keepFiles
      member x.GetFilters()         = x.filters :> seq<IFilter>

  let private synchronisedLogger =
    let cl = ConsoleLogger.Default
    { new ILogger with
        member __.Write(kind, text) =
          cl.Write(kind, text)
        member __.WriteLine(kind, text) =
          cl.WriteLine (kind, text)
        member __.WriteLine () =
          cl.WriteLine()
    }

  let benchmarkConfig =
    let def = DefaultConfig.Instance
    { columnProviders = def.GetColumnProviders() |> List.ofSeq
      hardwareCounters = def.GetHardwareCounters() |> List.ofSeq
      summaryStyle = def.GetSummaryStyle()
      exporters = def.GetExporters() |> List.ofSeq
      loggers = [ synchronisedLogger ]
      diagnosers = def.GetDiagnosers() |> List.ofSeq
      analysers = def.GetAnalysers() |> List.ofSeq
      jobs = def.GetJobs() |> List.ofSeq
      validators = def.GetValidators() |> List.ofSeq
      orderProvider = def.GetOrderProvider()
      unionRule = def.UnionRule
      keepFiles = true
      filters = def.GetFilters() |> List.ofSeq
    }

  /// Run a performance test: pass the annotated type as a type param
  /// to this function call. Pass 'benchmarkConfig' as the config parameter –
  /// because this is a record, you can change it to suit your liking.
  /// NOTE: Now needs to be manually put in a testCase.
  let benchmark<'typ> config onSummary =
    BenchmarkRunner.Run<'typ>(config) |> onSummary