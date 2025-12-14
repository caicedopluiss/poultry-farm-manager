import React, { useState } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    Box,
    MenuItem,
    CircularProgress,
} from "@mui/material";
import type { Product, UpdateProduct } from "@/types/inventory";

interface ProductDetailModalProps {
    open: boolean;
    onClose: () => void;
    product: Product;
    onUpdate: (id: string, data: UpdateProduct) => Promise<void>;
}

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

const ProductDetailModal: React.FC<ProductDetailModalProps> = ({ open, onClose, product, onUpdate }) => {
    const [editedName, setEditedName] = useState("");
    const [editedManufacturer, setEditedManufacturer] = useState("");
    const [editedStock, setEditedStock] = useState<number>(0);
    const [editedUnitOfMeasure, setEditedUnitOfMeasure] = useState("");
    const [editedDescription, setEditedDescription] = useState("");
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Initialize form when modal opens
    React.useEffect(() => {
        if (open && product) {
            setEditedName(product.name);
            setEditedManufacturer(product.manufacturer);
            setEditedStock(product.stock);
            setEditedUnitOfMeasure(product.unitOfMeasure);
            setEditedDescription(product.description || "");
        }
    }, [open, product]);

    const handleSave = async () => {
        if (!product) return;

        setIsSubmitting(true);
        try {
            const updates: UpdateProduct = {};

            if (editedName !== product.name) {
                updates.name = editedName;
            }
            if (editedManufacturer !== product.manufacturer) {
                updates.manufacturer = editedManufacturer;
            }
            if (editedStock !== product.stock) {
                updates.stock = editedStock;
            }
            if (editedUnitOfMeasure !== product.unitOfMeasure) {
                updates.unitOfMeasure = editedUnitOfMeasure;
            }
            if (editedDescription !== (product.description || "")) {
                updates.description = editedDescription || null;
            }

            if (Object.keys(updates).length > 0) {
                await onUpdate(product.id, updates);
            }

            onClose();
        } catch (error) {
            console.error("Failed to update product:", error);
        } finally {
            setIsSubmitting(false);
        }
    };

    if (!product) return null;

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>Edit Product</DialogTitle>
            <DialogContent>
                <Box sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 1 }}>
                    <TextField
                        label="Product Name"
                        value={editedName}
                        onChange={(e) => setEditedName(e.target.value)}
                        fullWidth
                        required
                    />

                    <TextField
                        label="Manufacturer"
                        value={editedManufacturer}
                        onChange={(e) => setEditedManufacturer(e.target.value)}
                        fullWidth
                        required
                    />

                    <TextField
                        label="Stock"
                        type="number"
                        value={editedStock}
                        onChange={(e) => setEditedStock(parseFloat(e.target.value))}
                        fullWidth
                        inputProps={{ step: "0.01", min: "0" }}
                    />

                    <TextField
                        select
                        label="Unit of Measure"
                        value={editedUnitOfMeasure}
                        onChange={(e) => setEditedUnitOfMeasure(e.target.value)}
                        fullWidth
                        required
                    >
                        {UNITS_OF_MEASURE.map((unit) => (
                            <MenuItem key={unit.value} value={unit.value}>
                                {unit.label}
                            </MenuItem>
                        ))}
                    </TextField>

                    <TextField
                        label="Description"
                        value={editedDescription}
                        onChange={(e) => setEditedDescription(e.target.value)}
                        multiline
                        rows={3}
                        fullWidth
                    />
                </Box>
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose} disabled={isSubmitting}>
                    Cancel
                </Button>
                <Button onClick={handleSave} variant="contained" color="primary" disabled={isSubmitting}>
                    {isSubmitting ? <CircularProgress size={24} /> : "Save"}
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export default ProductDetailModal;
