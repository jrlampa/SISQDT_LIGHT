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
    public string RuleId => "CQTS.PROTECTION_ASSESSMENT";

    public RuleExecutionResult Execute(IReadOnlyList<RuleInput> inputs, RuleExecutionContext context)
    {
        var rootLoadKvaInput = inputs.SingleOrDefault(i => i.Name == "RootLoadKva");
        var vNomInput = inputs.SingleOrDefault(i => i.Name == "Vnom");
        var minIcc1phInput = inputs.SingleOrDefault(i => i.Name == "MinIcc1Phase");
        var criticalTempInput = inputs.SingleOrDefault(i => i.Name == "CriticalTemperature");
        var criticalConductorInput = inputs.SingleOrDefault(i => i.Name == "CriticalConductorKey");
        var fuseRatedInput = inputs.SingleOrDefault(i => i.Name == "FuseRatedCurrent");
        var fuseMeltingTimeInput = inputs.SingleOrDefault(i => i.Name == "FuseMeltingTime");

        if (rootLoadKvaInput is null || vNomInput is null || minIcc1phInput is null ||
            criticalTempInput is null || criticalConductorInput is null)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Parâmetros obrigatórios de proteção ausentes.");
        }

        double rootLoadKva = Convert.ToDouble(rootLoadKvaInput.Value);
        double vNom = Convert.ToDouble(vNomInput.Value);
        double minIcc1ph = Convert.ToDouble(minIcc1phInput.Value);
        double criticalTemp = Convert.ToDouble(criticalTempInput.Value);
        string conductorKey = criticalConductorInput.Value?.ToString() ?? string.Empty;

        // Fusível default comercial Light (ex: NH-1600A ou In apropriado) e tempo de fusão default (0.1s)
        double fuseRated = fuseRatedInput != null ? Convert.ToDouble(fuseRatedInput.Value) : 1600.0;
        double fuseMeltingTime = fuseMeltingTimeInput != null ? Convert.ToDouble(fuseMeltingTimeInput.Value) : 0.1;

        if (vNom <= 0)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Tensão nominal inválida.");
        }

        // Corrente de projeto para seleção do fusível:
        // I_projeto = S_raiz / (sqrt(3) * Vnom / 1000)
        double projectCurrent = rootLoadKva / (Math.Sqrt(3.0) * vNom / 1000.0);

        // Seção real do condutor em mm² do catálogo Light (CT38 a CU40)
        double realSectionMm2 = ResolveRealConductorSection(conductorKey);
        bool isAluminum = IsAluminumConductor(conductorKey);

        // Suportabilidade térmica pelo critério adiabático de curto-circuito (Equação de Onderdonk):
        // Alumínio: t_adm = 48686 * ln((250 + 228) / (T + 228)) * (S / Icc1ph)^2
        // Cobre:    t_adm = 115679 * ln((250 + 234) / (T + 234)) * (S / Icc1ph)^2
        double maxAdmissibleTime = 0.0;
        if (minIcc1ph > 0.0 && realSectionMm2 > 0.0)
        {
            double ratio = realSectionMm2 / minIcc1ph;
            if (isAluminum)
            {
                maxAdmissibleTime = 48686.0 * Math.Log((250.0 + 228.0) / (criticalTemp + 228.0)) * (ratio * ratio);
            }
            else
            {
                maxAdmissibleTime = 115679.0 * Math.Log((250.0 + 234.0) / (criticalTemp + 234.0)) * (ratio * ratio);
            }
        }

        // Validação da corrente do fusível (Erro 06)
        bool isCurrentAdequate = fuseRated > projectCurrent;

        // Validação do tempo de atuação (Erro 07)
        bool isThermalAdequate = maxAdmissibleTime > 0.0 && fuseMeltingTime <= maxAdmissibleTime;

        string statusMessage;
        if (!isCurrentAdequate)
        {
            statusMessage = "Erro 06: Fusível subdimensionado em relação à corrente de carga do circuito.";
        }
        else if (!isThermalAdequate && maxAdmissibleTime > 0.0)
        {
            statusMessage = "Erro 07: Tempo de fusão superior à suportabilidade térmica do condutor perante o curto-circuito.";
        }
        else
        {
            statusMessage = "OK: Dimensionamento de proteção contra sobrecorrente e curto-circuito conforme.";
        }

        var trace = new[]
        {
            new RuleTrace("I_projeto", projectCurrent, UnitCode.Ampere, "S_raiz / (sqrt(3) * V / 1000)", "CURRENT", projectCurrent, UnitCode.Ampere),
            new RuleTrace("In_fusivel", fuseRated, UnitCode.Ampere, "Fusivel nominal", "FUSE", fuseRated, UnitCode.Ampere),
            new RuleTrace("Icc1ph_min", minIcc1ph, UnitCode.Ampere, "Min Icc monofasico", "SHORT_CIRCUIT", minIcc1ph, UnitCode.Ampere),
            new RuleTrace("Secao_mm2", realSectionMm2, UnitCode.SquareMillimeter, "Secao real do condutor", "CONDUCTOR", realSectionMm2, UnitCode.SquareMillimeter),
            new RuleTrace("t_admissivel", maxAdmissibleTime, UnitCode.Second, "Equacao de Onderdonk", "THERMAL_WITHSTAND", maxAdmissibleTime, UnitCode.Second),
            new RuleTrace("t_fusao", fuseMeltingTime, UnitCode.Second, "Tempo de atuacao informado", "FUSE", fuseMeltingTime, UnitCode.Second)
        };

        var assessment = new ProtectionAssessment(
            conductorKey,
            realSectionMm2,
            minIcc1ph,
            criticalTemp,
            maxAdmissibleTime,
            projectCurrent,
            fuseRated,
            fuseMeltingTime,
            isCurrentAdequate,
            isThermalAdequate,
            statusMessage);

        return new RuleExecutionResult(
            RuleId,
            FormulaId,
            EvidenceId,
            context.GoldenCaseId,
            assessment,
            UnitCode.Unknown,
            CalculationStatus.Pass,
            trace,
            null);
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

        // Fallback genérico baseado em número no nome
        return 16.0;
    }

    public static bool IsAluminumConductor(string? key)
    {
        if (string.IsNullOrWhiteSpace(key)) return true;
        return key.Contains("Al", StringComparison.OrdinalIgnoreCase) || key.Contains("CONC", StringComparison.OrdinalIgnoreCase);
    }
}
