import React, { useState, useEffect } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    Grid,
    useTheme,
    useMediaQuery,
} from "@mui/material";
import type { Asset, UpdateAsset } from "@/types/inventory";

interface EditAssetDialogProps {
    open: boolean;
    asset: Asset;
    onClose: () => void;
    onSubmit: (data: UpdateAsset) => void;
}

export default function EditAssetDialog({ open, asset, onClose, onSubmit }: EditAssetDialogProps) {
    const theme = useTheme();
    const fullScreen = useMediaQuery(theme.breakpoints.down("sm"));

    const [formData, setFormData] = useState<UpdateAsset>({
        name: asset.name,
        description: asset.description || "",
        notes: asset.notes || "",
        states: asset.states,
    });

    useEffect(() => {
        setFormData({
            name: asset.name,
            description: asset.description || "",
            notes: asset.notes || "",
            states: asset.states,
        });
    }, [asset]);

    const handleChange = (field: keyof UpdateAsset) => (event: React.ChangeEvent<HTMLInputElement>) => {
        setFormData((prev) => ({ ...prev, [field]: event.target.value }));
    };

    const handleSubmit = () => {
        onSubmit(formData);
    };

    return (
        <Dialog open={open} onClose={onClose} fullScreen={fullScreen} maxWidth="md" fullWidth>
            <DialogTitle>Edit Asset</DialogTitle>
            <DialogContent>
                <Grid container spacing={2} sx={{ mt: 0.5 }}>
                    <Grid size={12}>
                        <TextField
                            fullWidth
                            label="Name"
                            value={formData.name}
                            onChange={handleChange("name")}
                            required
                            inputProps={{ maxLength: 100 }}
                        />
                    </Grid>

                    <Grid size={12}>
                        <TextField
                            fullWidth
                            label="Description"
                            value={formData.description}
                            onChange={handleChange("description")}
                            multiline
                            rows={3}
                            inputProps={{ maxLength: 500 }}
                        />
                    </Grid>

                    <Grid size={12}>
                        <TextField
                            fullWidth
                            label="Notes"
                            value={formData.notes}
                            onChange={handleChange("notes")}
                            multiline
                            rows={3}
                            inputProps={{ maxLength: 500 }}
                        />
                    </Grid>
                </Grid>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose}>Cancel</Button>
                <Button onClick={handleSubmit} variant="contained" disabled={!formData.name}>
                    Save
                </Button>
            </DialogActions>
        </Dialog>
    );
}
