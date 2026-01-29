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
} from "@mui/material";
import { Edit as EditIcon } from "@mui/icons-material";
import type { Person, UpdatePerson } from "@/types/person";

interface PersonModalProps {
    open: boolean;
    onClose: () => void;
    person: Person;
    onUpdate: (id: string, data: UpdatePerson) => Promise<void>;
}

const PersonModal: React.FC<PersonModalProps> = ({ open, onClose, person, onUpdate }) => {
    const [isEditMode, setIsEditMode] = useState(false);
    const [editedFirstName, setEditedFirstName] = useState("");
    const [editedLastName, setEditedLastName] = useState("");
    const [editedEmail, setEditedEmail] = useState("");
    const [editedPhoneNumber, setEditedPhoneNumber] = useState("");
    const [editedLocation, setEditedLocation] = useState("");
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Initialize form when modal opens
    React.useEffect(() => {
        if (open && person) {
            setEditedFirstName(person.firstName);
            setEditedLastName(person.lastName);
            setEditedEmail(person.email || "");
            setEditedPhoneNumber(person.phoneNumber || "");
            setEditedLocation(person.location || "");
            setIsEditMode(false);
        }
    }, [open, person]);

    const handleEdit = () => {
        setIsEditMode(true);
    };

    const handleCancel = () => {
        setIsEditMode(false);
        setEditedFirstName(person.firstName);
        setEditedLastName(person.lastName);
        setEditedEmail(person.email || "");
        setEditedPhoneNumber(person.phoneNumber || "");
        setEditedLocation(person.location || "");
    };

    const handleSave = async () => {
        if (!person) return;

        setIsSubmitting(true);
        try {
            const updates: UpdatePerson = {};

            if (editedFirstName !== person.firstName) {
                updates.firstName = editedFirstName;
            }
            if (editedLastName !== person.lastName) {
                updates.lastName = editedLastName;
            }
            if (editedEmail !== (person.email || "")) {
                updates.email = editedEmail || undefined;
            }
            if (editedPhoneNumber !== (person.phoneNumber || "")) {
                updates.phoneNumber = editedPhoneNumber || undefined;
            }
            if (editedLocation !== (person.location || "")) {
                updates.location = editedLocation || undefined;
            }

            if (Object.keys(updates).length > 0) {
                await onUpdate(person.id, updates);
            }

            setIsEditMode(false);
            onClose();
        } catch (error) {
            console.error("Failed to update person:", error);
        } finally {
            setIsSubmitting(false);
        }
    };

    if (!person) return null;

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>
                {isEditMode ? "Edit Person" : "Person Details"}
                {!isEditMode && (
                    <IconButton aria-label="edit" onClick={handleEdit} sx={{ position: "absolute", right: 8, top: 8 }}>
                        <EditIcon />
                    </IconButton>
                )}
            </DialogTitle>
            <DialogContent>
                <Box sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 1 }}>
                    <TextField
                        label="First Name"
                        value={editedFirstName}
                        onChange={(e) => setEditedFirstName(e.target.value)}
                        fullWidth
                        required
                        disabled={!isEditMode}
                        InputProps={{
                            readOnly: !isEditMode,
                        }}
                    />

                    <TextField
                        label="Last Name"
                        value={editedLastName}
                        onChange={(e) => setEditedLastName(e.target.value)}
                        fullWidth
                        required
                        disabled={!isEditMode}
                        InputProps={{
                            readOnly: !isEditMode,
                        }}
                    />

                    <TextField
                        label="Email"
                        value={editedEmail}
                        onChange={(e) => setEditedEmail(e.target.value)}
                        fullWidth
                        disabled={!isEditMode}
                        InputProps={{
                            readOnly: !isEditMode,
                        }}
                    />

                    <TextField
                        label="Phone Number"
                        value={editedPhoneNumber}
                        onChange={(e) => setEditedPhoneNumber(e.target.value)}
                        fullWidth
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

export default PersonModal;
