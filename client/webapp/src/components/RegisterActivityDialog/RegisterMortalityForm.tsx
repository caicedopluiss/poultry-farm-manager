import { useState, useEffect } from "react";
import {
    Box,
    TextField,
    MenuItem,
    Typography,
    Alert,
    FormControl,
    InputLabel,
    Select,
} from "@mui/material";
import type { SelectChangeEvent } from "@mui/material";
import moment from "moment";
import type { NewMortalityRegistration, Sex } from "../../types/batchActivity";
import type { Batch } from "../../types/batch";

interface RegisterMortalityFormProps {
    batch: Batch;
    formData: NewMortalityRegistration;
    onChange: (data: NewMortalityRegistration) => void;
    errors: Record<string, string>;
}

export default function RegisterMortalityForm({
    batch,
    formData,
    onChange,
    errors,
}: RegisterMortalityFormProps) {
    const [availableCounts, setAvailableCounts] = useState({
        male: batch.maleCount,
        female: batch.femaleCount,
        unsexed: batch.unsexedCount,
    });

    useEffect(() => {
        setAvailableCounts({
            male: batch.maleCount,
            female: batch.femaleCount,
            unsexed: batch.unsexedCount,
        });
    }, [batch]);

    const handleNumberChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = parseInt(e.target.value) || 0;
        onChange({
            ...formData,
            numberOfDeaths: value,
        });
    };

    const handleSexChange = (e: SelectChangeEvent<Sex>) => {
        onChange({
            ...formData,
            sex: e.target.value as Sex,
        });
    };

    const handleDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        // Convert local date to ISO string
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

    const getAvailableCount = (): number => {
        switch (formData.sex) {
            case "Male":
                return availableCounts.male;
            case "Female":
                return availableCounts.female;
            case "Unsexed":
                return availableCounts.unsexed;
            default:
                return 0;
        }
    };

    const showWarning = formData.numberOfDeaths > getAvailableCount();

    return (
        <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                Recording mortality for batch: <strong>{batch.name}</strong>
            </Typography>

            <FormControl fullWidth error={!!errors.sex}>
                <InputLabel id="sex-label">Sex</InputLabel>
                <Select
                    labelId="sex-label"
                    value={formData.sex}
                    onChange={handleSexChange}
                    label="Sex"
                    required
                >
                    <MenuItem value="Unsexed">
                        Unsexed ({availableCounts.unsexed} available)
                    </MenuItem>
                    <MenuItem value="Male">
                        Male ({availableCounts.male} available)
                    </MenuItem>
                    <MenuItem value="Female">
                        Female ({availableCounts.female} available)
                    </MenuItem>
                </Select>
                {errors.sex && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                        {errors.sex}
                    </Typography>
                )}
            </FormControl>

            <TextField
                fullWidth
                type="number"
                label="Number of Deaths"
                value={formData.numberOfDeaths || ""}
                onChange={handleNumberChange}
                required
                error={!!errors.numberOfDeaths || showWarning}
                helperText={
                    errors.numberOfDeaths ||
                    (showWarning
                        ? `⚠️ Only ${getAvailableCount()} ${formData.sex.toLowerCase()} birds available`
                        : `Available: ${getAvailableCount()} ${formData.sex.toLowerCase()} birds`)
                }
                inputProps={{
                    min: 1,
                    max: getAvailableCount(),
                }}
            />

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
                helperText={
                    errors.notes ||
                    `${(formData.notes?.length || 0)}/500 characters`
                }
                inputProps={{
                    maxLength: 500,
                }}
            />

            {showWarning && (
                <Alert severity="warning">
                    The number of deaths exceeds the available {formData.sex.toLowerCase()} population.
                    Please verify the count.
                </Alert>
            )}
        </Box>
    );
}
