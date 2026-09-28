using System;
using System.Collections.Generic;
using System.Linq;
using QdtCqts.Calculation.Abstractions;
using QdtCqts.Domain;

namespace QdtCqts.Calculation.Cqts;

/// <summary>
/// Regra isolada e determinística para cálculo de curto-circuito trifásico (Icc 3φ) e monofásico (Icc 1φ).
/// Fórmula reconstruída e comprovada das colunas CB e CC da planilha oficial CQT/CQTS (LADO 1, linhas 8-15).
/// </summary>
public sealed class CandidateShortCircuitRule : IIsolatedRule
{
    public const string FormulaId = "CQTS.REAL_PROJECT.SHORT_CIRCUIT_CHAIN";
    public const string EvidenceId = "CQTS.REAL_PROJECT.LADO1_COLS_CB_CC";
    public string RuleId => "CQTS.SHORT_CIRCUIT";

    public RuleExecutionResult Execute(IReadOnlyList<RuleInput> inputs, RuleExecutionContext context)
    {
        var vNomInput = inputs.SingleOrDefault(i => i.Name == "Vnom");
        var zUpstreamRealInput = inputs.SingleOrDefault(i => i.Name == "Z_Upstream_Real");
        var zUpstreamImagInput = inputs.SingleOrDefault(i => i.Name == "Z_Upstream_Imag");
        var zBtPriorRealInput = inputs.SingleOrDefault(i => i.Name == "Z_BtPrior_Real");
        var zBtPriorImagInput = inputs.SingleOrDefault(i => i.Name == "Z_BtPrior_Imag");
        var rcaPhaseInput = inputs.SingleOrDefault(i => i.Name == "Rca_Phase");
        var rcaNeutralInput = inputs.SingleOrDefault(i => i.Name == "Rca_Neutral");
        var xReactanceInput = inputs.SingleOrDefault(i => i.Name == "X_Phase");
        var lengthEquivInput = inputs.SingleOrDefault(i => i.Name == "Length_Equiv_Meters");
        var rfnPriorInput = inputs.SingleOrDefault(i => i.Name == "Rfn_Prior");

        if (vNomInput is null || zUpstreamRealInput is null || zUpstreamImagInput is null ||
            rcaPhaseInput is null || xReactanceInput is null || lengthEquivInput is null)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Parâmetros obrigatórios de curto-circuito ausentes.");
        }

        double vNom = Convert.ToDouble(vNomInput.Value);
        double zUpstreamR = Convert.ToDouble(zUpstreamRealInput.Value);
        double zUpstreamX = Convert.ToDouble(zUpstreamImagInput.Value);
        double zBtPriorR = zBtPriorRealInput != null ? Convert.ToDouble(zBtPriorRealInput.Value) : 0.0;
        double zBtPriorX = zBtPriorImagInput != null ? Convert.ToDouble(zBtPriorImagInput.Value) : 0.0;
        double rcaPhase = Convert.ToDouble(rcaPhaseInput.Value);
        double rcaNeutral = rcaNeutralInput != null ? Convert.ToDouble(rcaNeutralInput.Value) : rcaPhase;
        double xPhase = Convert.ToDouble(xReactanceInput.Value);
        double lEquiv = Convert.ToDouble(lengthEquivInput.Value);
        double rfnPrior = rfnPriorInput != null ? Convert.ToDouble(rfnPriorInput.Value) : 0.0;

        if (vNom <= 0 || lEquiv < 0)
        {
            return RuleExecutionResult.Blocked(RuleId, FormulaId, EvidenceId, context, "Tensão nominal ou comprimento inválido.");
        }

        // Tensão de fase secundária Vfn = Vnom / sqrt(3)
        double vPhase = vNom / Math.Sqrt(3.0);

        // Impedância do trecho atual BT
        // BL = (Rca + j X) * L / 1000 [Ohm]
        double segR = rcaPhase * (lEquiv / 1000.0);
        double segX = xPhase * (lEquiv / 1000.0);

        // Resistência de laço Fase + Neutro do trecho atual BT
        // BO = (Rca_fase + Rca_neutro) * L / 1000 [Ohm]
        double segRfn = (rcaPhase + rcaNeutral) * (lEquiv / 1000.0);

        // 1. Curto-Circuito Trifásico (Icc 3φ)
        // Z_total_3ph = Z_upstream + Z_bt_prior + Z_seg
        double total3phR = zUpstreamR + zBtPriorR + segR;
        double total3phX = zUpstreamX + zBtPriorX + segX;
        double zMagnitude3ph = Math.Sqrt((total3phR * total3phR) + (total3phX * total3phX));
        double icc3ph = zMagnitude3ph > 0.0 ? vPhase / zMagnitude3ph : 0.0;

        // 2. Curto-Circuito Monofásico (Icc 1φ)
        // Re(Z_total_1ph) = Re(Z_upstream) + Rfn_prior + segRfn
        // Im(Z_total_1ph) = Im(Z_upstream)
        double total1phR = zUpstreamR + rfnPrior + segRfn;
        double total1phX = zUpstreamX;
        double zMagnitude1ph = Math.Sqrt((total1phR * total1phR) + (total1phX * total1phX));
        double icc1ph = zMagnitude1ph > 0.0 ? vPhase / zMagnitude1ph : 0.0;

        var trace = new[]
        {
            new RuleTrace("Vphase", vPhase, UnitCode.V, "Vnom / sqrt(3)", "VOLTAGE", vPhase, UnitCode.V),
            new RuleTrace("Z_total_3ph_Mag", zMagnitude3ph, UnitCode.Ohm, "sqrt(R^2 + X^2)", "IMPEDANCE", zMagnitude3ph, UnitCode.Ohm),
            new RuleTrace("Icc_3ph", icc3ph, UnitCode.Ampere, "Vphase / |Z_3ph|", "SHORT_CIRCUIT", icc3ph, UnitCode.Ampere),
            new RuleTrace("Z_total_1ph_Mag", zMagnitude1ph, UnitCode.Ohm, "sqrt((R_upstream + R_FN)^2 + X_upstream^2)", "IMPEDANCE", zMagnitude1ph, UnitCode.Ohm),
            new RuleTrace("Icc_1ph", icc1ph, UnitCode.Ampere, "Vphase / |Z_1ph|", "SHORT_CIRCUIT", icc1ph, UnitCode.Ampere)
        };

        return new RuleExecutionResult(
            RuleId,
            FormulaId,
            EvidenceId,
            context.GoldenCaseId,
            (Icc3Phase: icc3ph, Icc1Phase: icc1ph, SegR: segR, SegX: segX, SegRfn: segRfn, Total3phR: total3phR, Total3phX: total3phX),
            UnitCode.Ampere,
            CalculationStatus.Pass,
            trace,
            null);
    }
}
