import React, { useState } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    Box,
    CircularProgress,
    IconButton,
    MenuItem,
    Select,
    FormControl,
    InputLabel,
} from "@mui/material";
import { Edit as EditIcon } from "@mui/icons-material";
import type { Vendor, UpdateVendor } from "@/types/vendor";
import type { Person } from "@/types/person";

interface VendorModalProps {
    open: boolean;
    onClose: () => void;
    vendor: Vendor;
    persons: Person[];
    onUpdate: (id: string, data: UpdateVendor) => Promise<void>;
}

const VendorModal: React.FC<VendorModalProps> = ({ open, onClose, vendor, persons, onUpdate }) => {
    const [isEditMode, setIsEditMode] = useState(false);
    const [editedName, setEditedName] = useState("");
    const [editedLocation, setEditedLocation] = useState("");
    const [editedContactPersonId, setEditedContactPersonId] = useState("");
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Initialize form when modal opens
    React.useEffect(() => {
        if (open && vendor) {
            setEditedName(vendor.name);
            setEditedLocation(vendor.location || "");
            setEditedContactPersonId(vendor.contactPersonId);
            setIsEditMode(false);
        }
    }, [open, vendor]);

    const handleEdit = () => {
        setIsEditMode(true);
    };

    const handleCancel = () => {
        setIsEditMode(false);
        setEditedName(vendor.name);
        setEditedLocation(vendor.location || "");
        setEditedContactPersonId(vendor.contactPersonId);
    };

    const handleSave = async () => {
        if (!vendor) return;

        setIsSubmitting(true);
        try {
            const updates: UpdateVendor = {};

            if (editedName !== vendor.name) {
                updates.name = editedName;
            }
            if (editedLocation !== (vendor.location || "")) {
                updates.location = editedLocation || undefined;
            }
            if (editedContactPersonId !== vendor.contactPersonId) {
                updates.contactPersonId = editedContactPersonId;
            }

            if (Object.keys(updates).length > 0) {
                await onUpdate(vendor.id, updates);
            }

            setIsEditMode(false);
            onClose();
        } catch (error) {
            console.error("Failed to update vendor:", error);
        } finally {
            setIsSubmitting(false);
        }
    };

    if (!vendor) return null;

    const getContactPersonName = (personId: string) => {
        const person = persons.find((p) => p.id === personId);
        return person ? `${person.firstName} ${person.lastName}` : "Unknown";
    };

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>
                {isEditMode ? "Edit Vendor" : "Vendor Details"}
                {!isEditMode && (
                    <IconButton aria-label="edit" onClick={handleEdit} sx={{ position: "absolute", right: 8, top: 8 }}>
                        <EditIcon />
                    </IconButton>
                )}
            </DialogTitle>
            <DialogContent>
                <Box sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 1 }}>
                    <TextField
                        label="Vendor Name"
                        value={editedName}
                        onChange={(e) => setEditedName(e.target.value)}
                        fullWidth
                        required
                        disabled={!isEditMode}
                        InputProps={{
                            readOnly: !isEditMode,
                        }}
                    />

                    <TextField
                        label="Location"
                        value={editedLocation}
                        onChange={(e) => setEditedLocation(e.target.value)}
                        fullWidth
                        disabled={!isEditMode}
                        InputProps={{
                            readOnly: !isEditMode,
                        }}
                    />

                    {isEditMode ? (
                        <FormControl fullWidth>
                            <InputLabel>Contact Person</InputLabel>
                            <Select
                                value={editedContactPersonId}
                                onChange={(e) => setEditedContactPersonId(e.target.value)}
                                label="Contact Person"
                                required
                            >
                                {persons.map((person) => (
                                    <MenuItem key={person.id} value={person.id}>
                                        {person.firstName} {person.lastName}
                                    </MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                    ) : (
                        <TextField
                            label="Contact Person"
                            value={getContactPersonName(editedContactPersonId)}
                            fullWidth
                            disabled
                            InputProps={{
                                readOnly: true,
                            }}
                        />
                    )}
                </Box>
            </DialogContent>
            <DialogActions>
                {isEditMode ? (
                    <>
                        <Button onClick={handleCancel} disabled={isSubmitting}>
                            Cancel
                        </Button>
                        <Button onClick={handleSave} variant="contained" disabled={isSubmitting}>
                            {isSubmitting ? <CircularProgress size={24} /> : "Save"}
                        </Button>
                    </>
                ) : (
                    <Button onClick={onClose}>Close</Button>
                )}
            </DialogActions>
        </Dialog>
    );
};

export default VendorModal;
