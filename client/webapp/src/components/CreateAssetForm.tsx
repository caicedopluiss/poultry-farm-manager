import React, { useState, useEffect } from "react";
import {
    Box,
    Button,
    TextField,
    Typography,
    Alert,
    CircularProgress,
    Stack,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    useTheme,
    useMediaQuery,
    MenuItem,
} from "@mui/material";
import { getVendors } from "@/api/v1/vendors";
import type { NewAsset } from "@/types/inventory";
import type { Vendor } from "@/types/vendor";

interface CreateAssetFormProps {
    open: boolean;
    onSubmit: (assetData: NewAsset) => Promise<void>;
    onClose: () => void;
    loading: boolean;
    error: string | null;
}

export default function CreateAssetForm({ open, onSubmit, onClose, loading, error }: CreateAssetFormProps) {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));
    const isSmallMobile = useMediaQuery(theme.breakpoints.down("sm"));

    const [formData, setFormData] = useState({
        name: "",
        description: "",
        initialQuantity: 1,
        notes: "",
        vendorId: "",
        unitPrice: "",
    });

    const [vendors, setVendors] = useState<Vendor[]>([]);
    const [vendorsLoading, setVendorsLoading] = useState(false);

    useEffect(() => {
        if (open) {
            setFormData({
                name: "",
                description: "",
                initialQuantity: 1,
                notes: "",
                vendorId: "",
                unitPrice: "",
            });
            loadVendors();
        }
    }, [open]);

    const loadVendors = async () => {
        try {
            setVendorsLoading(true);
            const response = await getVendors();
            setVendors(response.vendors);
        } catch (error) {
            console.error("Failed to load vendors:", error);
        } finally {
            setVendorsLoading(false);
        }
    };

    const handleInputChange = (field: string) => (event: React.ChangeEvent<HTMLInputElement>) => {
        let value: string | number = event.target.value;

        if (event.target.type === "number") {
            value =
                field === "unitPrice"
                    ? event.target.value
                        ? parseFloat(event.target.value)
                        : ""
                    : event.target.value
                      ? parseInt(event.target.value)
                      : 0;
        }

        setFormData((prev) => ({
            ...prev,
            [field]: value,
        }));
    };

    const handleSubmit = async (event: React.FormEvent) => {
        event.preventDefault();

        const newAsset: NewAsset = {
            name: formData.name,
            description: formData.description || null,
            initialQuantity: formData.initialQuantity,
            notes: formData.notes || null,
            vendorId: formData.vendorId || null,
            unitPrice: formData.unitPrice
                ? typeof formData.unitPrice === "string"
                    ? parseFloat(formData.unitPrice)
                    : formData.unitPrice
                : null,
        };

        await onSubmit(newAsset);
    };

    return (
        <Dialog
            open={open}
            onClose={onClose}
            maxWidth="md"
            fullWidth
            fullScreen={isSmallMobile}
            PaperProps={{
                sx: {
                    minHeight: isMobile ? "90vh" : "auto",
                    margin: isMobile ? 1 : 3,
                },
            }}
        >
            <DialogTitle sx={{ pb: 1 }}>
                <Typography variant={isSmallMobile ? "h5" : "h4"} component="div" sx={{ fontWeight: "bold" }}>
                    Create New Asset
                </Typography>
            </DialogTitle>

            <DialogContent sx={{ px: { xs: 2, sm: 3 }, pb: 1 }}>
                {error && (
                    <Alert severity="error" sx={{ mb: 3 }}>
                        {error}
                    </Alert>
                )}

                <Box component="form" onSubmit={handleSubmit} id="create-asset-form" sx={{ mt: 2 }}>
                    <Stack spacing={3}>
                        <TextField
                            fullWidth
                            label="Asset Name"
                            value={formData.name}
                            onChange={handleInputChange("name")}
                            required
                            disabled={loading}
                        />

                        <TextField
                            fullWidth
                            label="Description"
                            value={formData.description}
                            onChange={handleInputChange("description")}
                            disabled={loading}
                            multiline
                            rows={3}
                        />

                        <TextField
                            fullWidth
                            label="Initial Quantity"
                            type="number"
                            value={formData.initialQuantity}
                            onChange={handleInputChange("initialQuantity")}
                            required
                            disabled={loading}
                            inputProps={{ min: 1 }}
                        />

                        <TextField
                            select
                            fullWidth
                            label="Vendor (Optional)"
                            value={formData.vendorId}
                            onChange={handleInputChange("vendorId")}
                            disabled={loading || vendorsLoading}
                        >
                            <MenuItem value="">
                                <em>None</em>
                            </MenuItem>
                            {vendors.map((vendor) => (
                                <MenuItem key={vendor.id} value={vendor.id}>
                                    {vendor.name}
                                </MenuItem>
                            ))}
                        </TextField>

                        <TextField
                            fullWidth
                            label="Unit Price (Optional)"
                            type="number"
                            value={formData.unitPrice}
                            onChange={handleInputChange("unitPrice")}
                            disabled={loading}
                            inputProps={{ min: 0, step: 0.001 }}
                        />

                        <TextField
                            fullWidth
                            label="Notes"
                            value={formData.notes}
                            onChange={handleInputChange("notes")}
                            disabled={loading}
                            multiline
                            rows={2}
                        />
                    </Stack>
                </Box>
            </DialogContent>

            <DialogActions sx={{ px: { xs: 2, sm: 3 }, pb: 3, pt: 2 }}>
                <Button onClick={onClose} disabled={loading} size="large" sx={{ minWidth: 100 }}>
                    Cancel
                </Button>
                <Button
                    type="submit"
                    form="create-asset-form"
                    variant="contained"
                    disabled={loading}
                    size="large"
                    sx={{ minWidth: 100 }}
                >
                    {loading ? <CircularProgress size={24} /> : "Create"}
                </Button>
            </DialogActions>
        </Dialog>
    );
}
