import { useState, useEffect } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    CircularProgress,
    Alert,
    useTheme,
    useMediaQuery,
} from "@mui/material";
import { updateBatchName } from "@/api/v1/batches";
import type { Batch } from "@/types/batch";

interface EditBatchNameDialogProps {
    open: boolean;
    onClose: () => void;
    batch: Batch;
    onSuccess?: () => void;
}

export default function EditBatchNameDialog({ open, onClose, batch, onSuccess }: EditBatchNameDialogProps) {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("sm"));

    const [name, setName] = useState(batch.name);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Reset form when dialog opens or batch changes
    useEffect(() => {
        if (open) {
            setName(batch.name);
            setError(null);
        }
    }, [open, batch.name]);

    const handleClose = () => {
        if (!loading) {
            setName(batch.name);
            setError(null);
            onClose();
        }
    };

    const handleSubmit = async (event: React.FormEvent) => {
        event.preventDefault();

        // Validation
        if (!name || name.trim().length === 0) {
            setError("Batch name is required");
            return;
        }

        if (name.trim().length > 100) {
            setError("Batch name cannot exceed 100 characters");
            return;
        }

        setLoading(true);
        setError(null);

        try {
            await updateBatchName(batch.id, name.trim());

            // Success - close and notify parent
            handleClose();
            if (onSuccess) {
                onSuccess();
            }
        } catch (err: unknown) {
            console.error("Error updating batch name:", err);

            // Handle validation errors
            if (err && typeof err === "object" && "validationErrors" in err) {
                const validationErrors = (err as { validationErrors: Array<{ field: string; error: string }> })
                    .validationErrors;
                const nameError = validationErrors.find((e) => e.field.toLowerCase() === "name");
                setError(nameError?.error || "Please fix the validation errors.");
            } else if (err && typeof err === "object" && "message" in err) {
                setError((err as { message: string }).message);
            } else {
                setError("An unexpected error occurred. Please try again.");
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth fullScreen={isMobile}>
            <form onSubmit={handleSubmit}>
                <DialogTitle>Edit Batch Name</DialogTitle>
                <DialogContent>
                    {error && (
                        <Alert severity="error" sx={{ mb: 2 }}>
                            {error}
                        </Alert>
                    )}

                    <TextField
                        autoFocus
                        margin="dense"
                        id="name"
                        label="Batch Name"
                        type="text"
                        fullWidth
                        variant="outlined"
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                        disabled={loading}
                        required
                        inputProps={{ maxLength: 100 }}
                        helperText={`${name.trim().length}/100 characters`}
                    />
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleClose} disabled={loading}>
                        Cancel
                    </Button>
                    <Button type="submit" variant="contained" disabled={loading || name.trim() === batch.name}>
                        {loading ? <CircularProgress size={24} /> : "Save"}
                    </Button>
                </DialogActions>
            </form>
        </Dialog>
    );
}
