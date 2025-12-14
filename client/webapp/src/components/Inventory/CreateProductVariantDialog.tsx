import { useState } from "react";
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
import type { NewProductVariant, Product } from "@/types/inventory";
import { UnitOfMeasure } from "@/types/inventory";

interface CreateProductVariantDialogProps {
    open: boolean;
    onClose: () => void;
    onSubmit: (data: NewProductVariant) => void;
    products: Product[];
}

export default function CreateProductVariantDialog({
    open,
    onClose,
    onSubmit,
    products,
}: CreateProductVariantDialogProps) {
    const theme = useTheme();
    const fullScreen = useMediaQuery(theme.breakpoints.down("sm"));

    const [formData, setFormData] = useState<NewProductVariant>({
        productId: "",
        name: "",
        description: "",
        stock: 0,
        quantity: 0,
        unitOfMeasure: UnitOfMeasure.Unit,
    });

    const handleChange = (field: keyof NewProductVariant) => (event: React.ChangeEvent<HTMLInputElement>) => {
        const value =
            field === "stock" || field === "quantity" ? parseFloat(event.target.value) || 0 : event.target.value;
        setFormData((prev) => ({ ...prev, [field]: value }));
    };

    const handleSubmit = () => {
        onSubmit(formData);
        // Reset form
        setFormData({
            productId: "",
            name: "",
            description: "",
            stock: 0,
            quantity: 0,
            unitOfMeasure: UnitOfMeasure.Unit,
        });
    };

    const handleClose = () => {
        onClose();
        // Reset form
        setFormData({
            productId: "",
            name: "",
            description: "",
            stock: 0,
            quantity: 0,
            unitOfMeasure: UnitOfMeasure.Unit,
        });
    };

    return (
        <Dialog open={open} onClose={handleClose} fullScreen={fullScreen} maxWidth="md" fullWidth>
            <DialogTitle>Create New Product Variant</DialogTitle>
            <DialogContent>
                <Grid container spacing={2} sx={{ mt: 0.5 }}>
                    <Grid size={12}>
                        <TextField
                            fullWidth
                            select
                            label="Product"
                            value={formData.productId}
                            onChange={handleChange("productId")}
                            required
                        >
                            {products.length === 0 && (
                                <MenuItem disabled value="">
                                    No products available
                                </MenuItem>
                            )}
                            {products.map((product) => (
                                <MenuItem key={product.id} value={product.id}>
                                    {product.name}
                                </MenuItem>
                            ))}
                        </TextField>
                    </Grid>

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
                <Button onClick={handleClose}>Cancel</Button>
                <Button
                    onClick={handleSubmit}
                    variant="contained"
                    disabled={!formData.productId || !formData.name || formData.stock < 0 || formData.quantity < 0}
                >
                    Create
                </Button>
            </DialogActions>
        </Dialog>
    );
}
