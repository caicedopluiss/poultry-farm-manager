const KG_ROUNDING_STEP = 0.05;

/**
 * Converts a mass value from any supported unit to kilograms.
 *
 * Supported units: "Kilogram", "Gram", "Pound"
 *
 * @param round - When true, rounds the result to the nearest 0.05 kg. This
 *   matches the physical precision of the weighing scale used to measure batch
 *   feed portions, so the displayed value reflects what can actually be set on
 *   the scale. Intended only for batch feeding calculations; defaults to false.
 */
export function convertToKilograms(value: number, unitOfMeasure: string, round = false): number {
    let raw: number;
    if (unitOfMeasure === "Gram") {
        raw = value / 1000;
    } else if (unitOfMeasure === "Pound") {
        raw = value * 0.453592;
    } else if (unitOfMeasure === "Kilogram") {
        raw = value;
    } else {
        console.warn(`Unsupported unit of measure passed to convertToKilograms: "${unitOfMeasure}"`);
        return Number.NaN;
    }
    if (!round) return raw;
    // Rounding strategy: snap to the nearest KG_ROUNDING_STEP (0.05 kg).
    //
    // Naive approach:  Math.round(raw / 0.05) * 0.05
    // Problem:         0.05 is not exactly representable in IEEE-754, so the
    //                  intermediate division/multiplication often produces
    //                  results like 1.1500000000000001 instead of 1.15.
    //
    // Fix:             Work in integer space by multiplying by SCALE = 1 / 0.05 = 20,
    //                  rounding to the nearest integer, then dividing back.
    //                  e.g. raw=1.137 → 1.137*20=22.74 → round→23 → 23/20=1.15
    const SCALE = Math.round(1 / KG_ROUNDING_STEP); // 20 — exact integer, no FP error
    return Math.round(raw * SCALE) / SCALE;
}
