import React, { useState } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    Box,
    Typography,
    IconButton,
    MenuItem,
    CircularProgress,
    Divider,
} from "@mui/material";
import { Add as AddIcon, Delete as DeleteIcon } from "@mui/icons-material";
import type { Asset, UpdateAsset, AssetState } from "@/types/inventory";

interface AssetDetailModalProps {
    open: boolean;
    onClose: () => void;
    asset: Asset;
    onUpdate: (id: string, data: UpdateAsset) => Promise<void>;
}

const ASSET_STATUSES = [
    { value: "Available", label: "Available" },
    { value: "InUse", label: "In Use" },
    { value: "Damaged", label: "Damaged" },
    { value: "UnderMaintenance", label: "Under Maintenance" },
    { value: "Retired", label: "Retired" },
];

const AssetDetailModal: React.FC<AssetDetailModalProps> = ({ open, onClose, asset, onUpdate }) => {
    const [editedName, setEditedName] = useState("");
    const [editedDescription, setEditedDescription] = useState("");
    const [editedNotes, setEditedNotes] = useState("");
    const [editedStates, setEditedStates] = useState<AssetState[]>([]);
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Initialize form when modal opens
    React.useEffect(() => {
        if (open && asset) {
            setEditedName(asset.name);
            setEditedDescription(asset.description || "");
            setEditedNotes(asset.notes || "");
            setEditedStates(
                asset.states?.map((s) => ({
                    id: s.id,
                    status: s.status,
                    quantity: s.quantity,
                    location: s.location,
                })) || [],
            );
        }
    }, [open, asset]);

    const handleAddState = () => {
        // Find first available status that's not already used
        const usedStatuses = editedStates.map((s) => s.status);
        const availableStatus = ASSET_STATUSES.find((s) => !usedStatuses.includes(s.value))?.value || "Available";
        setEditedStates([
            ...editedStates,
            { id: "00000000-0000-0000-0000-000000000000", status: availableStatus, quantity: 1, location: null },
        ]);
    };

    const handleRemoveState = (index: number) => {
        setEditedStates(editedStates.filter((_, i) => i !== index));
    };

    const handleStateChange = (index: number, field: "status" | "quantity" | "location", value: string | number) => {
        const newStates = [...editedStates];
        if (field === "status") {
            newStates[index].status = value as string;
        } else if (field === "quantity") {
            newStates[index].quantity = value as number;
        } else {
            newStates[index].location = (value as string) || null;
        }
        setEditedStates(newStates);
    };

    const handleSave = async () => {
        if (!asset) return;

        setIsSubmitting(true);
        try {
            const updates: UpdateAsset = {};

            if (editedName !== asset.name) {
                updates.name = editedName;
            }
            if (editedDescription !== (asset.description || "")) {
                updates.description = editedDescription || null;
            }
            if (editedNotes !== (asset.notes || "")) {
                updates.notes = editedNotes || null;
            }

            // Always include states if they exist (since they're managed as a whole)
            if (editedStates.length > 0) {
                updates.states = editedStates;
            }

            if (Object.keys(updates).length > 0) {
                await onUpdate(asset.id, updates);
            }

            onClose();
        } catch (error) {
            console.error("Failed to update asset:", error);
        } finally {
            setIsSubmitting(false);
        }
    };

    if (!asset) return null;

    return (
        <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
            <DialogTitle>Edit Asset</DialogTitle>
            <DialogContent>
                <Box sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 1 }}>
                    <TextField
                        label="Asset Name"
                        value={editedName}
                        onChange={(e) => setEditedName(e.target.value)}
                        fullWidth
                        required
                    />

                    <TextField
                        label="Description"
                        value={editedDescription}
                        onChange={(e) => setEditedDescription(e.target.value)}
                        multiline
                        rows={2}
                        fullWidth
                    />

                    <TextField
                        label="Notes"
                        value={editedNotes}
                        onChange={(e) => setEditedNotes(e.target.value)}
                        multiline
                        rows={2}
                        fullWidth
                    />

                    <Divider sx={{ my: 1 }} />

                    <Box>
                        <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 2 }}>
                            <Typography variant="h6" fontWeight={600}>
                                Asset States
                            </Typography>
                            <Button
                                startIcon={<AddIcon />}
                                onClick={handleAddState}
                                variant="outlined"
                                size="small"
                                color="secondary"
                                disabled={editedStates.length >= ASSET_STATUSES.length}
                            >
                                Add State
                            </Button>
                        </Box>

                        {editedStates.length === 0 ? (
                            <Typography variant="body2" color="text.secondary" sx={{ textAlign: "center", py: 2 }}>
                                No states defined. Click "Add State" to add one.
                            </Typography>
                        ) : (
                            <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
                                {editedStates.map((state, index) => (
                                    <Box
                                        key={index}
                                        sx={{
                                            display: "flex",
                                            flexDirection: "column",
                                            gap: 1.5,
                                            p: 2,
                                            pr: 6,
                                            bgcolor: "action.hover",
                                            borderRadius: 1,
                                            position: "relative",
                                        }}
                                    >
                                        <IconButton
                                            onClick={() => handleRemoveState(index)}
                                            color="error"
                                            size="small"
                                            sx={{ position: "absolute", top: 8, right: 8 }}
                                        >
                                            <DeleteIcon />
                                        </IconButton>
                                        <Box sx={{ display: "flex", gap: 2 }}>
                                            <TextField
                                                select
                                                label="Status"
                                                value={state.status}
                                                onChange={(e) => handleStateChange(index, "status", e.target.value)}
                                                sx={{ flex: 1 }}
                                                size="small"
                                            >
                                                {ASSET_STATUSES.map((status) => {
                                                    const isUsed = editedStates.some(
                                                        (s, i) => i !== index && s.status === status.value,
                                                    );
                                                    return (
                                                        <MenuItem
                                                            key={status.value}
                                                            value={status.value}
                                                            disabled={isUsed}
                                                        >
                                                            {status.label}
                                                            {isUsed && " (already used)"}
                                                        </MenuItem>
                                                    );
                                                })}
                                            </TextField>
                                            <TextField
                                                label="Quantity"
                                                type="number"
                                                value={state.quantity}
                                                onChange={(e) =>
                                                    handleStateChange(index, "quantity", parseInt(e.target.value))
                                                }
                                                sx={{ flex: 1 }}
                                                size="small"
                                                inputProps={{ min: 1 }}
                                            />
                                        </Box>
                                        <TextField
                                            label="Location"
                                            value={state.location || ""}
                                            onChange={(e) => handleStateChange(index, "location", e.target.value)}
                                            fullWidth
                                            size="small"
                                            placeholder="e.g., Warehouse A, Shelf 3"
                                        />
                                    </Box>
                                ))}
                            </Box>
                        )}
                    </Box>
                </Box>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose} disabled={isSubmitting}>
                    Cancel
                </Button>
                <Button
                    onClick={handleSave}
                    variant="contained"
                    color="primary"
                    disabled={isSubmitting || editedStates.length === 0}
                >
                    {isSubmitting ? <CircularProgress size={24} /> : "Save"}
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export default AssetDetailModal;
