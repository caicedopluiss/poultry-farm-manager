import { useState, useEffect } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    MenuItem,
    Grid,
    useTheme,
    useMediaQuery,
} from "@mui/material";
import type { Product, UpdateProduct } from "@/types/inventory";
import { UnitOfMeasure } from "@/types/inventory";

interface EditProductDialogProps {
    open: boolean;
    product: Product;
    onClose: () => void;
    onSubmit: (data: UpdateProduct) => void;
}

export default function EditProductDialog({ open, product, onClose, onSubmit }: EditProductDialogProps) {
    const theme = useTheme();
    const fullScreen = useMediaQuery(theme.breakpoints.down("sm"));

    const [formData, setFormData] = useState<UpdateProduct>({
        name: product.name,
        description: product.description || "",
        manufacturer: product.manufacturer,
        unitOfMeasure: product.unitOfMeasure,
        stock: product.stock,
    });

    useEffect(() => {
        setFormData({
            name: product.name,
            description: product.description || "",
            manufacturer: product.manufacturer,
            unitOfMeasure: product.unitOfMeasure,
            stock: product.stock,
        });
    }, [product]);

    const handleChange = (field: keyof UpdateProduct) => (event: React.ChangeEvent<HTMLInputElement>) => {
        setFormData((prev) => ({ ...prev, [field]: event.target.value }));
    };

    const handleSubmit = () => {
        onSubmit(formData);
    };

    return (
        <Dialog open={open} onClose={onClose} fullScreen={fullScreen} maxWidth="md" fullWidth>
            <DialogTitle>Edit Product</DialogTitle>
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

                    <Grid size={{ xs: 12, sm: 6 }}>
                        <TextField
                            fullWidth
                            label="Manufacturer"
                            value={formData.manufacturer}
                            onChange={handleChange("manufacturer")}
                            required
                            inputProps={{ maxLength: 100 }}
                        />
                    </Grid>

                    <Grid size={{ xs: 12, sm: 6 }}>
                        <TextField
                            fullWidth
                            select
                            label="Unit of Measure"
                            value={formData.unitOfMeasure}
                            onChange={handleChange("unitOfMeasure")}
                            required
                        >
                            <MenuItem value={UnitOfMeasure.Kilogram}>Kilogram</MenuItem>
                            <MenuItem value={UnitOfMeasure.Gram}>Gram</MenuItem>
                            <MenuItem value={UnitOfMeasure.Pound}>Pound</MenuItem>
                            <MenuItem value={UnitOfMeasure.Liter}>Liter</MenuItem>
                            <MenuItem value={UnitOfMeasure.Milliliter}>Milliliter</MenuItem>
                            <MenuItem value={UnitOfMeasure.Gallon}>Gallon</MenuItem>
                            <MenuItem value={UnitOfMeasure.Unit}>Unit</MenuItem>
                            <MenuItem value={UnitOfMeasure.Piece}>Piece</MenuItem>
                        </TextField>
                    </Grid>

                    <Grid size={12}>
                        <TextField
                            fullWidth
                            label="Stock"
                            type="number"
                            value={formData.stock}
                            onChange={handleChange("stock")}
                            required
                            inputProps={{ min: 0, step: 0.01 }}
                        />
                    </Grid>
                </Grid>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose}>Cancel</Button>
                <Button onClick={handleSubmit} variant="contained" disabled={!formData.name || !formData.manufacturer}>
                    Save
                </Button>
            </DialogActions>
        </Dialog>
    );
}
