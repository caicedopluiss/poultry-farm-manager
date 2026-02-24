import { useState, useEffect } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    Alert,
    CircularProgress,
} from "@mui/material";
import { updateBatchNotes } from "@/api/v1/batches";
import type { ApiClientError } from "@/api/client";

interface EditBatchNotesDialogProps {
    open: boolean;
    batchId: string;
    currentNotes: string | null;
    onClose: () => void;
    onSuccess: () => void;
}

export default function EditBatchNotesDialog({
    open,
    batchId,
    currentNotes,
    onClose,
    onSuccess,
}: EditBatchNotesDialogProps) {
    const [notes, setNotes] = useState(currentNotes || "");
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (open) {
            setNotes(currentNotes || "");
            setError(null);
        }
    }, [open, currentNotes]);

    const handleSubmit = async () => {
        // Validation
        const trimmedNotes = notes.trim();
        if (trimmedNotes.length > 500) {
            setError("Notes cannot exceed 500 characters");
            return;
        }

        try {
            setLoading(true);
            setError(null);

            await updateBatchNotes(batchId, trimmedNotes || null);

            onSuccess();
            onClose();
        } catch (err) {
            console.error("Failed to update batch notes:", err);
            const apiError = (err as ApiClientError) || {};
            setError(apiError?.response?.message || "Failed to update notes. Please try again.");
        } finally {
            setLoading(false);
        }
    };

    const handleClose = () => {
        if (!loading) {
            onClose();
        }
    };

    return (
        <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
            <DialogTitle>Edit Batch Notes</DialogTitle>
            <DialogContent>
                {error && (
                    <Alert severity="error" sx={{ mb: 2 }}>
                        {error}
                    </Alert>
                )}
                <TextField
                    autoFocus
                    margin="dense"
                    label="Notes"
                    type="text"
                    fullWidth
                    multiline
                    rows={6}
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                    disabled={loading}
                    placeholder="Enter batch notes..."
                    helperText={`${notes.length}/500 characters`}
                />
            </DialogContent>
            <DialogActions>
                <Button onClick={handleClose} disabled={loading}>
                    Cancel
                </Button>
                <Button onClick={handleSubmit} variant="contained" disabled={loading}>
                    {loading ? <CircularProgress size={24} /> : "Save"}
                </Button>
            </DialogActions>
        </Dialog>
    );
}
