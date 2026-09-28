using System;
using System.Collections.Generic;
using System.Linq;
using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Cqts;

/// <summary>
/// Regra isolada e determinística para avaliação de proteção e suportabilidade térmica de condutores em curto-circuito.
/// Fórmula de Onderdonk / IEC 60364-5-54 comprovada nas linhas 34-48 da planilha oficial CQT/CQTS (LADO 1).
/// </summary>
public sealed class CandidateProtectionRule : IIsolatedRule
{
    public const string FormulaId = "CQTS.REAL_PROJECT.PROTECTION_AND_THERMAL_WITHSTAND";
    public const string EvidenceId = "CQTS.REAL_PROJECT.LADO1_ROWS_34_48";
    public const string ConductorSectionEvidenceId = "CQTS.REAL_PROJECT.LADO1_CT38_CU40";
    public string RuleId => "CQTS.PROTECTION_ASSESSMENT";

    public RuleExecutionResult Execute(IReadOnlyList<RuleInput> inputs, RuleExecutionContext context)
    {
        var rootLoadKvaInput = inputs.SingleOrDefault(i => i.Name == "RootLoadKva");
        var vNomInput = inputs.SingleOrDefault(i => i.Name == "Vnom");
        var minIcc1phInput = inputs.SingleOrDefault(i => i.Name == "MinIcc1Phase");
        var maxIcc3phInput = inputs.SingleOrDefault(i => i.Name == "MaxIcc3Phase");
        var criticalTempInput = inputs.SingleOrDefault(i => i.Name == "CriticalTemperature");
        var criticalTempEvidenceInput = inputs.SingleOrDefault(i => i.Name == "CriticalTemperatureEvidenceId");
        var criticalConductorInput = inputs.SingleOrDefault(i => i.Name == "CriticalConductorKey");
        var deviceEvidenceInput = inputs.SingleOrDefault(i => i.Name == "DeviceEvidence");

        if (rootLoadKvaInput is null || vNomInput is null || minIcc1phInput is null ||
            maxIcc3phInput is null || criticalTempInput is null || criticalConductorInput is null ||
            rootLoadKvaInput.Value is null || vNomInput.Value is null || minIcc1phInput.Value is null ||
            maxIcc3phInput.Value is null || criticalTempInput.Value is null)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Parâmetros obrigatórios de proteção ausentes.");
        }

        double rootLoadKva = Convert.ToDouble(rootLoadKvaInput.Value);
        double vNom = Convert.ToDouble(vNomInput.Value);
        double minIcc1ph = Convert.ToDouble(minIcc1phInput.Value);
        double maxIcc3ph = Convert.ToDouble(maxIcc3phInput.Value);
        double criticalTemp = Convert.ToDouble(criticalTempInput.Value);
        string conductorKey = criticalConductorInput.Value?.ToString() ?? string.Empty;

        if (!double.IsFinite(rootLoadKva) || rootLoadKva < 0.0 ||
            !double.IsFinite(vNom) || vNom <= 0.0 ||
            !double.IsFinite(minIcc1ph) || minIcc1ph <= 0.0 ||
            !double.IsFinite(maxIcc3ph) || maxIcc3ph <= 0.0 ||
            !double.IsFinite(criticalTemp) || criticalTemp < 0.0)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Entradas elétricas inválidas para avaliação de proteção.");
        }

        // Corrente de projeto para seleção do fusível:
        // I_projeto = S_raiz / (sqrt(3) * Vnom / 1000)
        double projectCurrent = rootLoadKva / (Math.Sqrt(3.0) * vNom / 1000.0);

        // Seção do condutor catalogada nos dados reconstruídos de CT38 a CU40.
        double resolvedSectionMm2 = ResolveRealConductorSection(conductorKey);
        double? realSectionMm2 = resolvedSectionMm2 > 0.0 ? resolvedSectionMm2 : null;
        string? conductorSectionEvidenceId = realSectionMm2.HasValue ? ConductorSectionEvidenceId : null;
        bool isAluminum = IsAluminumConductor(conductorKey);

        // Suportabilidade térmica pelo critério adiabático de curto-circuito (Equação de Onderdonk):
        // Alumínio: t_adm = 48686 * ln((250 + 228) / (T + 228)) * (S / Icc1ph)^2
        // Cobre:    t_adm = 115679 * ln((250 + 234) / (T + 234)) * (S / Icc1ph)^2
        string? thermalBlockedReason = null;
        double? maxAdmissibleTime = null;
        bool temperatureEvidenceAvailable = criticalTempEvidenceInput?.Value is string temperatureEvidenceId &&
            string.Equals(temperatureEvidenceId, CandidateCableTemperatureRule.EvidenceId, StringComparison.Ordinal);
        if (!realSectionMm2.HasValue)
        {
            thermalBlockedReason = "EVIDENCE_BLOCKED: seção do condutor não consta no catálogo de proteção.";
        }
        else if (!temperatureEvidenceAvailable)
        {
            thermalBlockedReason = "EVIDENCE_BLOCKED: temperatura do condutor não possui proveniência de cálculo validada.";
        }
        else
        {
            double ratio = realSectionMm2.Value / minIcc1ph;
            if (isAluminum)
            {
                maxAdmissibleTime = 48686.0 * Math.Log((250.0 + 228.0) / (criticalTemp + 228.0)) * (ratio * ratio);
            }
            else
            {
                maxAdmissibleTime = 115679.0 * Math.Log((250.0 + 234.0) / (criticalTemp + 234.0)) * (ratio * ratio);
            }
        }

        ProtectionEvidenceStatus evidenceStatus;
        ProtectionDeviceEvidence? deviceEvidence = null;
        string blockedReason;
        if (deviceEvidenceInput is null || deviceEvidenceInput.Value is null)
        {
            evidenceStatus = ProtectionEvidenceStatus.Missing;
            blockedReason = "EVIDENCE_BLOCKED: curva e dados do dispositivo de proteção não foram fornecidos.";
        }
        else if (deviceEvidenceInput.Value is not ProtectionDeviceEvidence suppliedEvidence || !IsValidEvidence(suppliedEvidence, minIcc1ph))
        {
            evidenceStatus = ProtectionEvidenceStatus.Invalid;
            blockedReason = "EVIDENCE_BLOCKED: proveniência ou ponto da curva de proteção inválido.";
        }
        else
        {
            evidenceStatus = ProtectionEvidenceStatus.Available;
            deviceEvidence = suppliedEvidence;
            blockedReason = string.Empty;
        }

        bool? isRatedCurrentAdequate = null;
        bool? isThermalWithstandAdequate = null;
        bool? isInterruptingCapacityAdequate = null;
        ProtectionAssessmentStatus assessmentStatus;
        string statusMessage;

        if (evidenceStatus != ProtectionEvidenceStatus.Available || !maxAdmissibleTime.HasValue)
        {
            assessmentStatus = ProtectionAssessmentStatus.EvidenceBlocked;
            string thermalStatus = maxAdmissibleTime.HasValue
                ? $"Suportabilidade térmica calculada: t_adm = {maxAdmissibleTime.Value:F6} s."
                : thermalBlockedReason ?? "EVIDENCE_BLOCKED: suportabilidade térmica indisponível por falta de proveniência.";
            statusMessage = string.Join(" ", new[] { blockedReason, thermalStatus }.Where(message => !string.IsNullOrWhiteSpace(message)));
        }
        else
        {
            isRatedCurrentAdequate = deviceEvidence!.RatedCurrentAmperes >= projectCurrent;
            isThermalWithstandAdequate = deviceEvidence.TotalClearingTimeSeconds <= maxAdmissibleTime.Value;
            isInterruptingCapacityAdequate = deviceEvidence.InterruptingCapacityAmperes >= maxIcc3ph;
            assessmentStatus = isRatedCurrentAdequate.Value && isThermalWithstandAdequate.Value && isInterruptingCapacityAdequate.Value
                ? ProtectionAssessmentStatus.Pass
                : ProtectionAssessmentStatus.Fail;
            statusMessage = assessmentStatus == ProtectionAssessmentStatus.Pass
                ? "Avaliação do dispositivo aprovada para os critérios de corrente, tempo total de interrupção e capacidade de interrupção informados."
                : "Avaliação do dispositivo reprovada em pelo menos um critério elétrico informado.";
        }

        var trace = new[]
        {
            new RuleTrace("I_projeto", projectCurrent, UnitCode.Ampere, "S_raiz / (sqrt(3) * V / 1000)", "CURRENT", projectCurrent, UnitCode.Ampere),
            new RuleTrace("Icc1ph_min", minIcc1ph, UnitCode.Ampere, "Min Icc monofasico", "SHORT_CIRCUIT", minIcc1ph, UnitCode.Ampere),
            new RuleTrace("Icc3ph_max", maxIcc3ph, UnitCode.Ampere, "Max Icc trifasico", "SHORT_CIRCUIT", maxIcc3ph, UnitCode.Ampere),
            new RuleTrace("Secao_mm2", realSectionMm2, UnitCode.SquareMillimeter, "Secao catalogada do condutor", "CONDUCTOR", realSectionMm2, UnitCode.SquareMillimeter),
            new RuleTrace("t_admissivel", maxAdmissibleTime, UnitCode.Second, "Equacao de Onderdonk", "THERMAL_WITHSTAND", maxAdmissibleTime, UnitCode.Second)
        };

        var assessment = new ProtectionAssessment(
            conductorKey,
            realSectionMm2,
            conductorSectionEvidenceId,
            minIcc1ph,
            maxIcc3ph,
            criticalTemp,
            temperatureEvidenceAvailable ? CandidateCableTemperatureRule.EvidenceId : null,
            maxAdmissibleTime,
            projectCurrent,
            evidenceStatus,
            assessmentStatus,
            deviceEvidence,
            isRatedCurrentAdequate,
            isThermalWithstandAdequate,
            isInterruptingCapacityAdequate,
            statusMessage);

        var calculationStatus = assessmentStatus switch
        {
            ProtectionAssessmentStatus.Pass => CalculationStatus.Pass,
            ProtectionAssessmentStatus.Fail => CalculationStatus.Fail,
            _ => CalculationStatus.Blocked
        };
        var ruleStatus = assessmentStatus == ProtectionAssessmentStatus.EvidenceBlocked ? RuleStatus.Blocked : RuleStatus.Candidate;

        return new RuleExecutionResult(
            RuleId,
            FormulaId,
            EvidenceId,
            context.GoldenCaseId,
            assessment,
            UnitCode.Unknown,
            calculationStatus,
            trace,
            assessmentStatus == ProtectionAssessmentStatus.EvidenceBlocked ? statusMessage : null,
            RuleStatus: ruleStatus);
    }

    private static bool IsValidEvidence(ProtectionDeviceEvidence evidence, double minIcc1ph)
    {
        bool hasProvenance = !string.IsNullOrWhiteSpace(evidence.EvidenceId) &&
            !string.IsNullOrWhiteSpace(evidence.SourceReference) &&
            !string.IsNullOrWhiteSpace(evidence.Manufacturer) &&
            !string.IsNullOrWhiteSpace(evidence.Model) &&
            !string.IsNullOrWhiteSpace(evidence.CurveId) &&
            !string.IsNullOrWhiteSpace(evidence.Sha256) &&
            evidence.Sha256.Length == 64 && evidence.Sha256.All(Uri.IsHexDigit);
        bool hasValidValues = double.IsFinite(evidence.RatedCurrentAmperes) && evidence.RatedCurrentAmperes > 0.0 &&
            double.IsFinite(evidence.EvaluationCurrentAmperes) && evidence.EvaluationCurrentAmperes > 0.0 &&
            double.IsFinite(evidence.TotalClearingTimeSeconds) && evidence.TotalClearingTimeSeconds > 0.0 &&
            double.IsFinite(evidence.InterruptingCapacityAmperes) && evidence.InterruptingCapacityAmperes > 0.0;
        double currentTolerance = Math.Max(1e-9, minIcc1ph * 1e-9);

        return hasProvenance && hasValidValues &&
            Math.Abs(evidence.EvaluationCurrentAmperes - minIcc1ph) <= currentTolerance;
    }

    public static double ResolveRealConductorSection(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return 0.0;
        string norm = key.Trim();

        if (norm.Equals("240 Al - Arm", StringComparison.OrdinalIgnoreCase) || norm.Equals("240 Al", StringComparison.OrdinalIgnoreCase)) return 226.112;
        if (norm.Equals("240 Cu", StringComparison.OrdinalIgnoreCase)) return 226.2598;
        if (norm.Equals("185 Al - MX", StringComparison.OrdinalIgnoreCase)) return 172.3415;
        if (norm.Equals("120 Cu", StringComparison.OrdinalIgnoreCase)) return 111.6545;
        if (norm.Equals("95 Al - Arm", StringComparison.OrdinalIgnoreCase) || norm.Equals("95 Al", StringComparison.OrdinalIgnoreCase)) return 88.325;
        if (norm.Equals("70 Cu", StringComparison.OrdinalIgnoreCase)) return 63.8556;
        if (norm.Equals("70 Al - MX", StringComparison.OrdinalIgnoreCase)) return 63.8014;
        if (norm.Equals("53 Al - QX", StringComparison.OrdinalIgnoreCase)) return 53.0;
        if (norm.Equals("50 Al - Arm", StringComparison.OrdinalIgnoreCase)) return 44.0936;
        if (norm.Equals("35 Cu", StringComparison.OrdinalIgnoreCase)) return 32.9027;
        if (norm.Equals("25 Al - Arm", StringComparison.OrdinalIgnoreCase) || norm.Equals("25 Al", StringComparison.OrdinalIgnoreCase)) return 23.5533;
        if (norm.Equals("21 Al - QX", StringComparison.OrdinalIgnoreCase)) return 21.0;
        if (norm.StartsWith("16", StringComparison.OrdinalIgnoreCase)) return 16.0;
        if (norm.StartsWith("13", StringComparison.OrdinalIgnoreCase)) return 13.0;
        if (norm.StartsWith("10_CONC", StringComparison.OrdinalIgnoreCase)) return 9.4213;

        return 0.0;
    }

    public static bool IsAluminumConductor(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return true;
        return key.Contains("Al", StringComparison.OrdinalIgnoreCase) || key.Contains("CONC", StringComparison.OrdinalIgnoreCase);
    }
}
