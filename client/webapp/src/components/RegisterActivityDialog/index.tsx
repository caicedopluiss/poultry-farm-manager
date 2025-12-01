import { useState } from "react";
import { Dialog, DialogTitle, DialogContent, DialogActions, Button, CircularProgress, Alert } from "@mui/material";
import moment from "moment";
import type { Batch } from "../../types/batch";
import type { NewMortalityRegistration, BatchActivityType } from "../../types/batchActivity";
import RegisterMortalityForm from "./RegisterMortalityForm";
import { registerMortality } from "../../api/v1/batches";

interface RegisterActivityDialogProps {
    open: boolean;
    onClose: () => void;
    batch: Batch;
    activityType: BatchActivityType;
    onSuccess?: () => void;
}

export default function RegisterActivityDialog({
    open,
    onClose,
    batch,
    activityType,
    onSuccess,
}: RegisterActivityDialogProps) {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});

    const [mortalityFormData, setMortalityFormData] = useState<NewMortalityRegistration>({
        numberOfDeaths: 1,
        dateClientIsoString: moment().toISOString(),
        sex: "Unsexed",
        notes: null,
    });

    const handleClose = () => {
        if (!loading) {
            // Reset form
            setMortalityFormData({
                numberOfDeaths: 1,
                dateClientIsoString: moment().toISOString(),
                sex: "Unsexed",
                notes: null,
            });
            setError(null);
            setFieldErrors({});
            onClose();
        }
    };

    const handleSubmit = async () => {
        setLoading(true);
        setError(null);
        setFieldErrors({});

        try {
            if (activityType === "MortalityRecording") {
                await registerMortality(batch.id, {
                    ...mortalityFormData,
                    dateClientIsoString: moment(mortalityFormData.dateClientIsoString).format(),
                });

                // Success - close and notify parent
                handleClose();
                if (onSuccess) {
                    onSuccess();
                }
            }
            // Future activity types can be handled here
        } catch (err: unknown) {
            console.error("Error registering activity:", err);

            // Handle validation errors
            if (err && typeof err === "object" && "validationErrors" in err) {
                const validationErrors = (err as { validationErrors: Array<{ field: string; error: string }> })
                    .validationErrors;

                const errors: Record<string, string> = {};
                validationErrors.forEach((e) => {
                    errors[e.field] = e.error;
                });
                setFieldErrors(errors);
                setError("Please fix the validation errors below.");
            } else if (err && typeof err === "object" && "message" in err) {
                setError((err as { message: string }).message);
            } else {
                setError("An unexpected error occurred. Please try again.");
            }
        } finally {
            setLoading(false);
        }
    };

    const getDialogTitle = (): string => {
        switch (activityType) {
            case "MortalityRecording":
                return "Register Mortality";
            case "Feeding":
                return "Register Feeding";
            default:
                return "Register Activity";
        }
    };

    const renderForm = () => {
        switch (activityType) {
            case "MortalityRecording":
                return (
                    <RegisterMortalityForm
                        batch={batch}
                        formData={mortalityFormData}
                        onChange={setMortalityFormData}
                        errors={fieldErrors}
                    />
                );
            // Future activity forms can be added here
            // case "Feeding":
            //     return <RegisterFeedingForm ... />;
            default:
                return null;
        }
    };

    return (
        <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
            <DialogTitle>{getDialogTitle()}</DialogTitle>
            <DialogContent>
                {error && (
                    <Alert severity="error" sx={{ mb: 2 }}>
                        {error}
                    </Alert>
                )}
                {renderForm()}
            </DialogContent>
            <DialogActions sx={{ px: 3, pb: 2 }}>
                <Button onClick={handleClose} disabled={loading}>
                    Cancel
                </Button>
                <Button
                    onClick={handleSubmit}
                    variant="contained"
                    disabled={loading}
                    startIcon={loading ? <CircularProgress size={20} /> : null}
                >
                    {loading ? "Submitting..." : "Submit"}
                </Button>
            </DialogActions>
        </Dialog>
    );
}
