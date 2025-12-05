import { useState, useEffect } from "react";
import { Box, TextField, MenuItem, Typography, Alert, FormControl, InputLabel, Select } from "@mui/material";
import type { SelectChangeEvent } from "@mui/material";
import moment from "moment";
import type { NewStatusSwitch, BatchStatus } from "../../types/batchActivity";
import type { Batch } from "../../types/batch";

interface StatusSwitchFormProps {
    batch: Batch;
    formData: NewStatusSwitch;
    onChange: (data: NewStatusSwitch) => void;
    errors: Record<string, string>;
}

const STATUS_TRANSITIONS: Record<string, BatchStatus[]> = {
    Active: ["Processed", "ForSale", "Canceled"],
    Processed: ["ForSale"],
    ForSale: ["Sold"],
    Sold: [],
    Canceled: [],
};

export default function StatusSwitchForm({ batch, formData, onChange, errors }: StatusSwitchFormProps) {
    const [availableStatuses, setAvailableStatuses] = useState<BatchStatus[]>([]);

    useEffect(() => {
        const currentStatus = batch.status as BatchStatus;
        setAvailableStatuses(STATUS_TRANSITIONS[currentStatus] || []);
    }, [batch.status]);

    const handleStatusChange = (e: SelectChangeEvent<BatchStatus>) => {
        onChange({
            ...formData,
            newStatus: e.target.value as BatchStatus,
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

    // Get today's date for default
    const today = moment().format("YYYY-MM-DD");

    return (
        <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                Switching status for batch: <strong>{batch.name}</strong>
            </Typography>

            <Alert severity="info" sx={{ mb: 1 }}>
                Current Status: <strong>{batch.status}</strong>
            </Alert>

            {availableStatuses.length === 0 ? (
                <Alert severity="warning">No status transitions available from the current status.</Alert>
            ) : (
                <>
                    <FormControl fullWidth error={!!errors.newStatus}>
                        <InputLabel id="status-label">New Status</InputLabel>
                        <Select
                            labelId="status-label"
                            value={formData.newStatus}
                            onChange={handleStatusChange}
                            label="New Status"
                            required
                        >
                            {availableStatuses.map((status) => (
                                <MenuItem key={status} value={status}>
                                    {status}
                                </MenuItem>
                            ))}
                        </Select>
                        {errors.newStatus && (
                            <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                                {errors.newStatus}
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
                                : today
                        }
                        onChange={handleDateChange}
                        required
                        error={!!errors.dateClientIsoString}
                        helperText={errors.dateClientIsoString || "Date when the status change occurred"}
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
                        helperText={errors.notes || "Additional information about the status change"}
                        inputProps={{
                            maxLength: 500,
                        }}
                    />
                </>
            )}
        </Box>
    );
}
