const KG_ROUNDING_STEP = 0.05;

/**
 * Converts a mass value from any supported unit to kilograms,
 * rounded to the nearest 0.05 kg.
 *
 * Supported units: "Kilogram", "Gram", "Pound"
 */
export function convertToKilograms(value: number, unitOfMeasure: string): number {
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
    // Scale by 1/KG_ROUNDING_STEP (20) to avoid floating-point artifacts such as
    // 1.1500000000000001 that arise from multiplying back a non-representable step.
    const SCALE = Math.round(1 / KG_ROUNDING_STEP); // 20
    return Math.round(raw * SCALE) / SCALE;
}
