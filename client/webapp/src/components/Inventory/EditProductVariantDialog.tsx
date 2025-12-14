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
import type { ProductVariant, UpdateProductVariant, Product } from "@/types/inventory";
import { UnitOfMeasure } from "@/types/inventory";

interface EditProductVariantDialogProps {
    open: boolean;
    productVariant: ProductVariant;
    products: Product[];
    onClose: () => void;
    onSubmit: (data: UpdateProductVariant) => void;
}

export default function EditProductVariantDialog({
    open,
    productVariant,
    onClose,
    onSubmit,
}: EditProductVariantDialogProps) {
    const theme = useTheme();
    const fullScreen = useMediaQuery(theme.breakpoints.down("sm"));

    const [formData, setFormData] = useState<UpdateProductVariant>({
        name: productVariant.name,
        description: productVariant.description || "",
        stock: productVariant.stock,
        quantity: productVariant.quantity,
        unitOfMeasure: productVariant.unitOfMeasure,
    });

    useEffect(() => {
        setFormData({
            name: productVariant.name,
            description: productVariant.description || "",
            stock: productVariant.stock,
            quantity: productVariant.quantity,
            unitOfMeasure: productVariant.unitOfMeasure,
        });
    }, [productVariant]);

    const handleChange = (field: keyof UpdateProductVariant) => (event: React.ChangeEvent<HTMLInputElement>) => {
        const value =
            field === "stock" || field === "quantity" ? parseFloat(event.target.value) || 0 : event.target.value;
        setFormData((prev) => ({ ...prev, [field]: value }));
    };

    const handleSubmit = () => {
        onSubmit(formData);
    };

    return (
        <Dialog open={open} onClose={onClose} fullScreen={fullScreen} maxWidth="md" fullWidth>
            <DialogTitle>Edit Product Variant</DialogTitle>
            <DialogContent>
                <Grid container spacing={2} sx={{ mt: 0.5 }}>
                    <Grid size={12}>
                        <TextField
                            fullWidth
                            label="Variant Name"
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
                            label="Stock"
                            type="number"
                            value={formData.stock}
                            onChange={handleChange("stock")}
                            required
                            inputProps={{ min: 0, step: 0.01 }}
                        />
                    </Grid>

                    <Grid size={{ xs: 12, sm: 6 }}>
                        <TextField
                            fullWidth
                            label="Quantity"
                            type="number"
                            value={formData.quantity}
                            onChange={handleChange("quantity")}
                            required
                            inputProps={{ min: 0 }}
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
                </Grid>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose}>Cancel</Button>
                <Button
                    onClick={handleSubmit}
                    variant="contained"
                    disabled={
                        !formData.name ||
                        (formData.stock !== null && formData.stock !== undefined && formData.stock < 0) ||
                        (formData.quantity !== null && formData.quantity !== undefined && formData.quantity < 0)
                    }
                >
                    Save
                </Button>
            </DialogActions>
        </Dialog>
    );
}
