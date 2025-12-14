import { useState } from "react";
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
import type { NewAsset } from "@/types/inventory";

interface CreateAssetDialogProps {
    open: boolean;
    onClose: () => void;
    onSubmit: (data: NewAsset) => void;
}

export default function CreateAssetDialog({ open, onClose, onSubmit }: CreateAssetDialogProps) {
    const theme = useTheme();
    const fullScreen = useMediaQuery(theme.breakpoints.down("sm"));

    const [formData, setFormData] = useState<NewAsset>({
        name: "",
        description: "",
        initialQuantity: 1,
        notes: "",
    });

    const handleChange = (field: keyof NewAsset) => (event: React.ChangeEvent<HTMLInputElement>) => {
        setFormData((prev) => ({ ...prev, [field]: event.target.value }));
    };

    const handleSubmit = () => {
        onSubmit(formData);
        // Reset form
        setFormData({
            name: "",
            description: "",
            initialQuantity: 1,
            notes: "",
        });
    };

    const handleClose = () => {
        onClose();
        // Reset form
        setFormData({
            name: "",
            description: "",
            initialQuantity: 1,
            notes: "",
        });
    };

    return (
        <Dialog open={open} onClose={handleClose} fullScreen={fullScreen} maxWidth="md" fullWidth>
            <DialogTitle>Create New Asset</DialogTitle>
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
                            label="Initial Quantity"
                            type="number"
                            value={formData.initialQuantity}
                            onChange={handleChange("initialQuantity")}
                            required
                            inputProps={{ min: 0 }}
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
                <Button onClick={handleClose}>Cancel</Button>
                <Button onClick={handleSubmit} variant="contained" disabled={!formData.name}>
                    Create
                </Button>
            </DialogActions>
        </Dialog>
    );
}
