using Microsoft.Extensions.Logging;
using QdtCqts.Application;
using QdtCqts.Calculation.Abstractions;
using QdtCqts.Calculation.Cqts;
using QdtCqts.Domain;
using QdtCqts.Infrastructure.Observability;
using Xunit;

namespace QdtCqts.Tests.Domain;

public sealed class ObservabilityAndTraceTests
{
    [Fact]
    public void Test01_LoggerConfiguration_ProfilesAreConfigurable()
    {
        var devConfig = LoggingConfiguration.ForProfile(LoggingProfile.Development);
        var prodConfig = LoggingConfiguration.ForProfile(LoggingProfile.Production);
        var diagConfig = LoggingConfiguration.ForProfile(LoggingProfile.Diagnostic);
        var testConfig = LoggingConfiguration.ForProfile(LoggingProfile.Test);

        Assert.Equal(Serilog.Events.LogEventLevel.Debug, devConfig.MinimumLevel);
        Assert.Equal(Serilog.Events.LogEventLevel.Information, prodConfig.MinimumLevel);
        Assert.Equal(Serilog.Events.LogEventLevel.Verbose, diagConfig.MinimumLevel);
        Assert.False(testConfig.EnableFileRolling);

        var factory = LoggingBootstrapper.CreateLoggerFactory(testConfig);
        Assert.NotNull(factory);
        var logger = factory.CreateLogger("TestCategory");
        Assert.NotNull(logger);
    }

    [Fact]
    public void Test02_EventTaxonomy_IdsAreDistinctAndWellFormatted()
    {
        var eventIds = new[]
        {
            CalculationEventIds.AppStartup,
            CalculationEventIds.AppShutdown,
            CalculationEventIds.AppUnhandledException,
            CalculationEventIds.DbOpened,
            CalculationEventIds.DbMigrationStarted,
            CalculationEventIds.DbMigrationCompleted,
            CalculationEventIds.DbMigrationFailed,
            CalculationEventIds.ImportStarted,
            CalculationEventIds.ImportCompleted,
            CalculationEventIds.ImportWarning,
            CalculationEventIds.ImportFailed,
            CalculationEventIds.CalcStarted,
            CalculationEventIds.CalcRuleExecuted,
            CalculationEventIds.CalcCompleted,
            CalculationEventIds.CalcBlocked,
            CalculationEventIds.RuleExecuted,
            CalculationEventIds.RuleFailed,
            CalculationEventIds.RuleCandidateWarning,
            CalculationEventIds.TopoNodeCreated,
            CalculationEventIds.TopoEdgeCreated,
            CalculationEventIds.TopoParentAssigned,
            CalculationEventIds.TopoValidated,
            CalculationEventIds.TopoInvariantViolation,
            CalculationEventIds.ParityMatch,
            CalculationEventIds.ParityMismatch,
            CalculationEventIds.EvidCaptured,
            CalculationEventIds.EvidMissingOrIncomplete,
            CalculationEventIds.GoldenPass,
            CalculationEventIds.GoldenMismatch
        };

        // All IDs must be unique
        var distinctInts = eventIds.Select(e => e.Id).Distinct().Count();
        Assert.Equal(eventIds.Length, distinctInts);

        // All Names must follow TAXONOMY-XXX pattern
        foreach (var ev in eventIds)
        {
            Assert.Contains("-", ev.Name);
            Assert.True(ev.Id >= 1000);
        }
    }

    [Fact]
    public void Test03_CorrelationId_PropagatesViaAmbientScope()
    {
        var testCorrId = "CORR-FLOW-TEST-001";
        Assert.NotNull(CorrelationContext.Current.CorrelationId);

        using (CorrelationContext.BeginScope(testCorrId))
        {
            Assert.Equal(testCorrId, CorrelationContext.Current.CorrelationId);

            // Nested scope with CalculationId
            using (CorrelationContext.SetCalculationId("CALC-TEST-001"))
            {
                Assert.Equal(testCorrId, CorrelationContext.Current.CorrelationId);
                Assert.Equal("CALC-TEST-001", CorrelationContext.Current.CalculationId);
            }

            // CalculationId is reverted
            Assert.Null(CorrelationContext.Current.CalculationId);
            Assert.Equal(testCorrId, CorrelationContext.Current.CorrelationId);
        }
    }

    [Fact]
    public void Test04_CalculationId_IsUniqueAndStructured()
    {
        var calcId1 = CorrelationContext.GenerateCalculationId();
        var calcId2 = CorrelationContext.GenerateCalculationId();

        Assert.StartsWith("CALC-", calcId1);
        Assert.StartsWith("CALC-", calcId2);
        Assert.NotEqual(calcId1, calcId2);
    }

    [Fact]
    public void Test05_RuleIdAndVersion_AttachedToExecutionResult()
    {
        var factory = LoggerFactory.Create(b => { });
        var logger = factory.CreateLogger<CalculationAuditService>();
        var recorder = new CalculationTraceRecorder();
        var service = new CalculationAuditService(logger, recorder);

        var rule = new CandidateSegmentVoltageDropRule();
        var inputs = new[]
        {
            new RuleInput("M", 74.448d, UnitCode.Kva),
            new RuleInput("R", 0.088708306113649771d, UnitCode.Ohm),
            new RuleInput("X", 0.0897d, UnitCode.Ohm),
            new RuleInput("L", 4.0d, UnitCode.Meter),
            new RuleInput("AP", 2.0d, UnitCode.Unknown),
            new RuleInput("V", 220.0d, UnitCode.V),
            new RuleInput("H", 3.0d, UnitCode.ConductorKey)
        };

        var result = service.ExecuteRuleWithAudit(rule, inputs);

        Assert.Equal(CalculationStatus.Pass, result.Status);
        Assert.Equal("CQTS.REAL_PROJECT.SEGMENT_VOLTAGE_DROP", result.RuleId);
        Assert.Equal("1.0.0", result.RuleVersion);
        Assert.NotNull(result.InputHash);
        Assert.NotNull(result.OutputHash);

        var traces = recorder.GetTraces(result.OutputHash);
        Assert.NotEmpty(recorder.GetTraces());
    }

    [Fact]
    public void Test06_InputHash_IsDeterministicRegardlessOfOrdering()
    {
        var inputA1 = new RuleInput("M", 74.448d, UnitCode.Kva);
        var inputA2 = new RuleInput("L", 4.0d, UnitCode.Meter);
        var inputA3 = new RuleInput("H", 3.0d, UnitCode.ConductorKey);

        // Ordem 1: M, L, H
        var hash1 = DeterministicHashing.ComputeInputHash(new[] { inputA1, inputA2, inputA3 });

        // Ordem 2: H, M, L
        var hash2 = DeterministicHashing.ComputeInputHash(new[] { inputA3, inputA1, inputA2 });

        Assert.Equal(hash1, hash2);
        Assert.Equal(64, hash1.Length); // 256 bits = 64 hex characters
    }

    [Fact]
    public void Test07_OutputHash_IsDeterministic()
    {
        const double val = 0.038810072181038206d;
        var hash1 = DeterministicHashing.ComputeOutputHash(val, UnitCode.Percent);
        var hash2 = DeterministicHashing.ComputeOutputHash(val, UnitCode.Percent);

        Assert.Equal(hash1, hash2);
        Assert.Equal(64, hash1.Length);

        // Valor diferente gera hash diferente
        var hashDiff = DeterministicHashing.ComputeOutputHash(0.039d, UnitCode.Percent);
        Assert.NotEqual(hash1, hashDiff);
    }

    [Fact]
    public void Test08_ParityEventLogging_RecordsMatchAndMismatch()
    {
        var factory = LoggerFactory.Create(b => { });
        var logger = factory.CreateLogger<CalculationAuditService>();
        var recorder = new CalculationTraceRecorder();
        var service = new CalculationAuditService(logger, recorder);

        service.RecordParity("CQTS.BZ", "PROJ7-L13", "Excel", "Native", 0.03881d, 0.03881d, 0.0, 1e-6, "MATCH");
        service.RecordParity("CQTS.BZ", "PROJ7-L14", "Excel", "Native", 0.86241d, 0.85000d, 0.01241, 1e-6, "MISMATCH");

        var records = recorder.GetParityRecords();
        Assert.Equal(2, records.Count);
        Assert.Equal("MATCH", records[0].Status);
        Assert.Equal("MISMATCH", records[1].Status);
    }

    [Fact]
    public void Test09_GoldenEventLogging_RecordsPassAndMismatch()
    {
        var factory = LoggerFactory.Create(b => { });
        var logger = factory.CreateLogger<CalculationAuditService>();
        var recorder = new CalculationTraceRecorder();
        var service = new CalculationAuditService(logger, recorder);

        service.RecordGolden("GC-BZ13", "1.0", "hash123", 0.03881d, 0.03881d, 0.0, 1e-9, "PASS");
        service.RecordGolden("GC-BZ14", "1.0", "hash456", 0.86241d, 0.80000d, 0.06241, 1e-9, "FAIL");

        var records = recorder.GetGoldenRecords();
        Assert.Equal(2, records.Count);
        Assert.Equal("PASS", records[0].Status);
        Assert.Equal("FAIL", records[1].Status);
    }

    [Fact]
    public void Test10_ExceptionLogging_CapturesContextWithoutCrashing()
    {
        var factory = LoggerFactory.Create(b => { });
        var logger = factory.CreateLogger<CalculationAuditService>();
        var recorder = new CalculationTraceRecorder();
        var service = new CalculationAuditService(logger, recorder);

        var rule = new CandidateSegmentVoltageDropRule();
        // Insumos incompletos para forçar resultado de erro/bloqueio
        var invalidInputs = new[] { new RuleInput("M", 50.0d, UnitCode.Kva) };

        var result = service.ExecuteRuleWithAudit(rule, invalidInputs);

        Assert.Equal(CalculationStatus.Blocked, result.Status);
        Assert.NotNull(result.Reason);

        var traces = recorder.GetTraces();
        Assert.NotEmpty(traces);
        Assert.Equal(CalculationStatus.Blocked, traces.Last().Status);
    }

    [Fact]
    public void Test11_SensitiveDataRedactor_MasksSecretsAndPii()
    {
        var sensitive1 = "User authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9";
        var redacted1 = SensitiveDataRedactor.Redact(sensitive1);
        Assert.DoesNotContain("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9", redacted1);
        Assert.Contains("***REDACTED***", redacted1);

        var sensitive2 = "Server=srv;Database=db;User Id=admin;Password=SuperSecretPassword123;";
        var redacted2 = SensitiveDataRedactor.Redact(sensitive2);
        Assert.DoesNotContain("SuperSecretPassword123", redacted2);
        Assert.Contains("Password=***REDACTED***", redacted2);

        var sensitive3 = "Consumidor João da Silva, CPF: 123.456.789-00 residencial";
        var redacted3 = SensitiveDataRedactor.Redact(sensitive3);
        Assert.DoesNotContain("123.456.789-00", redacted3);
        Assert.Contains("***.***.***-**", redacted3);
    }

    [Fact]
    public void Test12_LogRotationConfiguration_HasSensibleDefaults()
    {
        var config = LoggingConfiguration.ForProfile(LoggingProfile.Production);
        Assert.Equal(10 * 1024 * 1024, config.FileSizeLimitBytes);
        Assert.Equal(30, config.RetainedFileCountLimit);
        Assert.Equal("logs", config.LogDirectory);
        Assert.True(config.EnableFileRolling);
    }

    [Fact]
    public void Test13_StartupMetadata_LogsVersionAndRuntimeContext()
    {
        var factory = LoggerFactory.Create(b => { });
        var logger = factory.CreateLogger("StartupTest");
        var config = LoggingConfiguration.ForProfile(LoggingProfile.Test);

        StartupLogger.LogStartup(logger, config, "git-test-commit-hash");

        Assert.NotNull(VersioningMetadata.CodeVersion);
        Assert.NotNull(VersioningMetadata.SchemaVersion);
        Assert.NotNull(VersioningMetadata.RuleSetVersion);
        Assert.NotNull(VersioningMetadata.EvidenceVersion);
    }

    [Fact]
    public void Test14_Integration_CalculationFlow_PreservesCorrelation()
    {
        var factory = LoggerFactory.Create(b => { });
        var logger = factory.CreateLogger<CalculationAuditService>();
        var recorder = new CalculationTraceRecorder();
        var service = new CalculationAuditService(logger, recorder);

        const string correlationId = "CORR-E2E-TEST-42";
        const string calculationId = "CALC-E2E-TEST-42";
        using (CorrelationContext.BeginScope(correlationId, calculationId))
        {
            var rule = new CandidateSegmentVoltageDropRule();
            var inputs = new[]
            {
                new RuleInput("M", 74.448d, UnitCode.Kva),
                new RuleInput("R", 0.088708306113649771d, UnitCode.Ohm),
                new RuleInput("X", 0.0897d, UnitCode.Ohm),
                new RuleInput("L", 4.0d, UnitCode.Meter),
                new RuleInput("AP", 2.0d, UnitCode.Unknown),
                new RuleInput("V", 220.0d, UnitCode.V),
                new RuleInput("H", 3.0d, UnitCode.ConductorKey)
            };

            var result = service.ExecuteRuleWithAudit(rule, inputs);
            Assert.Equal(CalculationStatus.Pass, result.Status);

            service.RecordParity(rule.RuleId, "CASE-01", "Excel", "Native", (double)result.OutputValue!, (double)result.OutputValue!, 0.0, 1e-12, "MATCH");

            var traces = recorder.GetTraces();
            var parities = recorder.GetParityRecords();

            Assert.Single(traces);
            Assert.Single(parities);
            Assert.Equal(correlationId, traces[0].CorrelationId);
            Assert.Equal(correlationId, parities[0].CorrelationId);
            Assert.Equal(traces[0].CalculationId, parities[0].CalculationId);
        }
    }

    [Fact]
    public void Test15_AuditScenario_Proj7_TrToLid_ReconstructsFullChain()
    {
        // Reconstrução de ponta a ponta do trecho TR -> LID do Projeto 7:
        // Quem: CalculationAuditService
        // Versão: VersioningMetadata.CodeVersion
        // Regra: CQTS.REAL_PROJECT.SEGMENT_VOLTAGE_DROP (v1.0.0)
        // Insumos: M=74.448 kVA, R=0.088708 Ohm/km, X=0.0897 Ohm/km, L=4m, AP=2, V=220V, H=3
        // Evidência: F22-CQTS-PROJ7-LADO1-BZ13 (CQT PROJ 7 REV2.xlsm, LADO 1!BZ13)
        // Fórmula: CQTS.REAL_PROJECT.SEGMENT_VOLTAGE_DROP.FORMULA (BW * BJ * AR * k_fase)
        // Resultado: 0.038810072181038206 %
        // Golden: GC-PROJ7-L13-BZ13
        // Comparação: MATCH com tolerância 0.0
        var factory = LoggerFactory.Create(b => { });
        var logger = factory.CreateLogger<CalculationAuditService>();
        var recorder = new CalculationTraceRecorder();
        var service = new CalculationAuditService(logger, recorder);

        const string goldenCaseId = "GC-PROJ7-L13-BZ13";
        const string correlationId = "CORR-AUDIT-SCENARIO-PROJ7";

        using (CorrelationContext.BeginScope(correlationId))
        {
            var rule = new CandidateSegmentVoltageDropRule();
            var inputs = new[]
            {
                new RuleInput("M", 74.448d, UnitCode.Kva),
                new RuleInput("R", 0.088708306113649771d, UnitCode.Ohm),
                new RuleInput("X", 0.0897d, UnitCode.Ohm),
                new RuleInput("L", 4.0d, UnitCode.Meter),
                new RuleInput("AP", 2.0d, UnitCode.Unknown),
                new RuleInput("V", 220.0d, UnitCode.V),
                new RuleInput("H", 3.0d, UnitCode.ConductorKey)
            };

            // Registra proveniência da evidência no audit trail
            recorder.RecordEvidence(new EvidenceTrailRecord(
                EvidenceId: "F22-CQTS-PROJ7-LADO1-BZ13",
                SourceFile: "CQT PROJ 7 - CLANDESTINO - AV PADRE DECAMINADA.xlsm",
                SourceHash: "9315e8ae152c225613988a04568c62749c3d586b569ff900b71584f237ff7762",
                Sheet: "LADO 1",
                Cell: "BZ13",
                Formula: "=IF(OR(C13=\"\",D13=\"\",H13=\"\",E13=\"\",G13=\"\",I13=\"Erro !\",AM13=\"\",AR13=\"\"),\"\",IF(H13=3,BW13*BJ13*AR13,IF(H13=2,BW13*BJ13*AR13*2,IF(H13=1,BW13*BJ13*AR13*6,0))))",
                CachedValue: 0.038810072181038206d,
                ExtractionVersion: "FASE22.1",
                Timestamp: DateTimeOffset.UtcNow));

            var result = service.ExecuteRuleWithAudit(rule, inputs, goldenCaseId, TraceMode.Diagnostic);

            Assert.Equal(CalculationStatus.Pass, result.Status);
            Assert.Equal(0.038810072181038206d, (double)result.OutputValue!, 12);

            service.RecordGolden(goldenCaseId, "1.0", "hash-proj7-evidence", 0.038810072181038206d, (double)result.OutputValue!, 0.0, 1e-12, "PASS");
            service.RecordParity(rule.RuleId, goldenCaseId, "Excel", "NativeEngine", 0.038810072181038206d, (double)result.OutputValue!, 0.0, 1e-12, "MATCH");

            var traces = recorder.GetTraces();
            var evidenceRecords = recorder.GetEvidenceRecords();
            var goldenRecords = recorder.GetGoldenRecords();
            var parityRecords = recorder.GetParityRecords();

            Assert.Single(traces);
            Assert.Single(evidenceRecords);
            Assert.Single(goldenRecords);
            Assert.Single(parityRecords);

            var trace = traces[0];
            Assert.Equal(correlationId, trace.CorrelationId);
            Assert.Equal(rule.RuleId, trace.RuleId);
            Assert.Equal("1.0.0", trace.RuleVersion);
            Assert.Equal(TraceMode.Diagnostic, trace.Mode);
            Assert.Equal(7, trace.Inputs.Count);
            Assert.NotEmpty(trace.Steps);
            Assert.Equal("PASS", goldenRecords[0].Status);
            Assert.Equal("MATCH", parityRecords[0].Status);
            Assert.Equal("LADO 1", evidenceRecords[0].Sheet);
            Assert.Equal("BZ13", evidenceRecords[0].Cell);
        }
    }
}
