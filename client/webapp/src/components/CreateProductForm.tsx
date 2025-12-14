import React, { useState } from "react";
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
import type { NewProduct } from "@/types/inventory";

const UNITS_OF_MEASURE = [
    { value: "Kilogram", label: "Kilogram (kg)" },
    { value: "Gram", label: "Gram (g)" },
    { value: "Pound", label: "Pound (lb)" },
    { value: "Liter", label: "Liter (L)" },
    { value: "Milliliter", label: "Milliliter (mL)" },
    { value: "Gallon", label: "Gallon (gal)" },
    { value: "Unit", label: "Unit" },
    { value: "Piece", label: "Piece" },
];

interface CreateProductFormProps {
    open: boolean;
    onSubmit: (productData: NewProduct) => Promise<void>;
    onClose: () => void;
    loading: boolean;
    error: string | null;
}

export default function CreateProductForm({ open, onSubmit, onClose, loading, error }: CreateProductFormProps) {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));
    const isSmallMobile = useMediaQuery(theme.breakpoints.down("sm"));

    const [formData, setFormData] = useState({
        name: "",
        manufacturer: "",
        unitOfMeasure: "Kilogram",
        stock: 0,
        description: "",
    });

    React.useEffect(() => {
        if (open) {
            setFormData({
                name: "",
                manufacturer: "",
                unitOfMeasure: "Kilogram",
                stock: 0,
                description: "",
            });
        }
    }, [open]);

    const handleInputChange = (field: string) => (event: React.ChangeEvent<HTMLInputElement>) => {
        const value = event.target.type === "number" ? parseFloat(event.target.value) || 0 : event.target.value;
        setFormData((prev) => ({
            ...prev,
            [field]: value,
        }));
    };

    const handleSubmit = async (event: React.FormEvent) => {
        event.preventDefault();

        const newProduct: NewProduct = {
            name: formData.name,
            manufacturer: formData.manufacturer,
            unitOfMeasure: formData.unitOfMeasure,
            stock: formData.stock,
            description: formData.description || null,
        };

        await onSubmit(newProduct);
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
                    Create New Product
                </Typography>
            </DialogTitle>

            <DialogContent sx={{ px: { xs: 2, sm: 3 }, pb: 1 }}>
                {error && (
                    <Alert severity="error" sx={{ mb: 3 }}>
                        {error}
                    </Alert>
                )}

                <Box component="form" onSubmit={handleSubmit} id="create-product-form" sx={{ mt: 2 }}>
                    <Stack spacing={3}>
                        <TextField
                            fullWidth
                            label="Product Name"
                            value={formData.name}
                            onChange={handleInputChange("name")}
                            required
                            disabled={loading}
                        />

                        <TextField
                            fullWidth
                            label="Manufacturer"
                            value={formData.manufacturer}
                            onChange={handleInputChange("manufacturer")}
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
                            select
                            label="Unit of Measure"
                            value={formData.unitOfMeasure}
                            onChange={handleInputChange("unitOfMeasure")}
                            required
                            disabled={loading}
                        >
                            {UNITS_OF_MEASURE.map((unit) => (
                                <MenuItem key={unit.value} value={unit.value}>
                                    {unit.label}
                                </MenuItem>
                            ))}
                        </TextField>

                        <TextField
                            fullWidth
                            label="Initial Stock"
                            type="number"
                            value={formData.stock}
                            onChange={handleInputChange("stock")}
                            required
                            disabled={loading}
                            inputProps={{ min: 0, step: 0.01 }}
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
                    form="create-product-form"
                    variant="contained"
                    color="secondary"
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
