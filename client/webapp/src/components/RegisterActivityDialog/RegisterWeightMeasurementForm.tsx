import { Box, TextField, MenuItem, Typography, FormControl, InputLabel, Select } from "@mui/material";
import type { SelectChangeEvent } from "@mui/material";
import moment from "moment";
import type { NewWeightMeasurement } from "@/types/batchActivity";
import type { Batch } from "@/types/batch";

interface RegisterWeightMeasurementFormProps {
    batch: Batch;
    formData: NewWeightMeasurement;
    onChange: (data: NewWeightMeasurement) => void;
    errors: Record<string, string>;
}

export default function RegisterWeightMeasurementForm({
    batch,
    formData,
    onChange,
    errors,
}: RegisterWeightMeasurementFormProps) {
    const handleAverageWeightChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = parseFloat(e.target.value) || 0;
        onChange({
            ...formData,
            averageWeight: value,
        });
    };

    const handleSampleSizeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = parseInt(e.target.value) || 0;
        onChange({
            ...formData,
            sampleSize: value,
        });
    };

    const handleUnitChange = (e: SelectChangeEvent<string>) => {
        onChange({
            ...formData,
            unitOfMeasure: e.target.value,
        });
    };

    const handleDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const dateValue = e.target.value;
        if (dateValue) {
            const isoString = moment(dateValue).toISOString();
            onChange({
                ...formData,
                dateClientIsoString: isoString,
            });
        }
    };

    const handleNotesChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        onChange({
            ...formData,
            notes: e.target.value || null,
        });
    };

    return (
        <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                Recording weight measurement for batch: <strong>{batch.name}</strong>
            </Typography>

            <TextField
                fullWidth
                type="number"
                label="Average Weight"
                value={formData.averageWeight || ""}
                onChange={handleAverageWeightChange}
                error={!!errors.averageWeight}
                helperText={errors.averageWeight || "Enter the average weight of the sample"}
                required
                inputProps={{ step: "0.01", min: "0" }}
            />

            <TextField
                fullWidth
                type="number"
                label="Sample Size"
                value={formData.sampleSize || ""}
                onChange={handleSampleSizeChange}
                error={!!errors.sampleSize}
                helperText={errors.sampleSize || "Number of birds measured"}
                required
                inputProps={{ min: "1" }}
            />

            <FormControl fullWidth error={!!errors.unitOfMeasure}>
                <InputLabel id="unit-label">Unit of Measure</InputLabel>
                <Select
                    labelId="unit-label"
                    value={formData.unitOfMeasure}
                    onChange={handleUnitChange}
                    label="Unit of Measure"
                    required
                >
                    <MenuItem value="Kilogram">Kilogram (kg)</MenuItem>
                    <MenuItem value="Gram">Gram (g)</MenuItem>
                    <MenuItem value="Pound">Pound (lb)</MenuItem>
                </Select>
                {errors.unitOfMeasure && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                        {errors.unitOfMeasure}
                    </Typography>
                )}
            </FormControl>

            <TextField
                fullWidth
                type="date"
                label="Date"
                value={
                    formData.dateClientIsoString
                        ? moment(formData.dateClientIsoString).format("YYYY-MM-DD")
                        : moment().format("YYYY-MM-DD")
                }
                onChange={handleDateChange}
                required
                error={!!errors.dateClientIsoString}
                helperText={errors.dateClientIsoString}
                InputLabelProps={{
                    shrink: true,
                }}
            />

            <TextField
                fullWidth
                multiline
                rows={3}
                label="Notes (Optional)"
                value={formData.notes || ""}
                onChange={handleNotesChange}
                error={!!errors.notes}
                helperText={errors.notes || "Any additional information about the measurement"}
                inputProps={{ maxLength: 500 }}
            />
        </Box>
    );
}
