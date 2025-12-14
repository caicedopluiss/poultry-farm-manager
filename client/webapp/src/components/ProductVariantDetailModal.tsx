import React, { useState } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    MenuItem,
    Box,
    Typography,
    Divider,
    IconButton,
} from "@mui/material";
import { Close as CloseIcon, Edit as EditIcon, Save as SaveIcon, Cancel as CancelIcon } from "@mui/icons-material";
import type { ProductVariant, UpdateProductVariant } from "@/types/inventory";

interface ProductVariantDetailModalProps {
    open: boolean;
    onClose: () => void;
    variant: ProductVariant | null;
    onUpdate: (variantId: string, variantData: UpdateProductVariant) => Promise<void>;
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

const ProductVariantDetailModal: React.FC<ProductVariantDetailModalProps> = ({ open, onClose, variant, onUpdate }) => {
    const [isEditing, setIsEditing] = useState(false);
    const [editedName, setEditedName] = useState("");
    const [editedQuantity, setEditedQuantity] = useState<number>(0);
    const [editedUnitOfMeasure, setEditedUnitOfMeasure] = useState("");
    const [editedDescription, setEditedDescription] = useState("");
    const [editedStock, setEditedStock] = useState<number>(0);
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleEdit = () => {
        if (variant) {
            setEditedName(variant.name);
            setEditedQuantity(variant.quantity);
            setEditedUnitOfMeasure(variant.unitOfMeasure);
            setEditedDescription(variant.description || "");
            setEditedStock(variant.stock);
            setIsEditing(true);
        }
    };

    const handleCancelEdit = () => {
        setIsEditing(false);
    };

    const handleSave = async () => {
        if (!variant) return;

        setIsSubmitting(true);
        try {
            const updates: UpdateProductVariant = {};

            if (editedName !== variant.name) {
                updates.name = editedName;
            }
            if (editedQuantity !== variant.quantity) {
                updates.quantity = editedQuantity;
            }
            if (editedUnitOfMeasure !== variant.unitOfMeasure) {
                updates.unitOfMeasure = editedUnitOfMeasure;
            }
            if (editedDescription !== (variant.description || "")) {
                updates.description = editedDescription || null;
            }
            if (editedStock !== variant.stock) {
                updates.stock = editedStock;
            }

            if (Object.keys(updates).length > 0) {
                await onUpdate(variant.id, updates);
            }

            setIsEditing(false);
            onClose();
        } catch (error) {
            console.error("Failed to update variant:", error);
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleClose = () => {
        setIsEditing(false);
        onClose();
    };

    if (!variant) return null;

    return (
        <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
            <DialogTitle>
                <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                    <Typography variant="h6" fontWeight={600}>
                        Variant Details
                    </Typography>
                    <IconButton onClick={handleClose} size="small">
                        <CloseIcon />
                    </IconButton>
                </Box>
            </DialogTitle>
            <Divider />
            <DialogContent>
                <Box sx={{ display: "flex", flexDirection: "column", gap: 3, mt: 2 }}>
                    {isEditing ? (
                        <>
                            <TextField
                                label="Variant Name"
                                value={editedName}
                                onChange={(e) => setEditedName(e.target.value)}
                                required
                                fullWidth
                            />

                            <TextField
                                label="Quantity"
                                type="number"
                                value={editedQuantity}
                                onChange={(e) => setEditedQuantity(Number(e.target.value))}
                                required
                                fullWidth
                                inputProps={{ min: 1, step: 1 }}
                            />

                            <TextField
                                label="Unit of Measure"
                                select
                                value={editedUnitOfMeasure}
                                onChange={(e) => setEditedUnitOfMeasure(e.target.value)}
                                required
                                fullWidth
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

                            <TextField
                                label="Stock"
                                type="number"
                                value={editedStock}
                                onChange={(e) => setEditedStock(parseFloat(e.target.value))}
                                fullWidth
                                inputProps={{ step: "0.01", min: "0" }}
                            />
                        </>
                    ) : (
                        <>
                            <Box>
                                <Typography variant="caption" color="text.secondary">
                                    Variant Name
                                </Typography>
                                <Typography variant="body1" fontWeight={500}>
                                    {variant.name}
                                </Typography>
                            </Box>

                            <Box>
                                <Typography variant="caption" color="text.secondary">
                                    Quantity
                                </Typography>
                                <Typography variant="body1" fontWeight={500}>
                                    {variant.quantity}
                                </Typography>
                            </Box>

                            <Box>
                                <Typography variant="caption" color="text.secondary">
                                    Stock
                                </Typography>
                                <Typography variant="body1" fontWeight={500}>
                                    {variant.stock} {variant.unitOfMeasure}
                                </Typography>
                            </Box>

                            <Box>
                                <Typography variant="caption" color="text.secondary">
                                    Unit of Measure
                                </Typography>
                                <Typography variant="body1" fontWeight={500}>
                                    {variant.unitOfMeasure}
                                </Typography>
                            </Box>

                            {variant.description && (
                                <Box>
                                    <Typography variant="caption" color="text.secondary">
                                        Description
                                    </Typography>
                                    <Typography variant="body1">{variant.description}</Typography>
                                </Box>
                            )}

                            {variant.product && (
                                <Box>
                                    <Typography variant="caption" color="text.secondary">
                                        Product
                                    </Typography>
                                    <Typography variant="body1" fontWeight={500}>
                                        {variant.product.name}
                                    </Typography>
                                </Box>
                            )}
                        </>
                    )}
                </Box>
            </DialogContent>
            <Divider />
            <DialogActions sx={{ px: 3, py: 2 }}>
                {isEditing ? (
                    <>
                        <Button onClick={handleCancelEdit} disabled={isSubmitting} startIcon={<CancelIcon />}>
                            Cancel
                        </Button>
                        <Button
                            onClick={handleSave}
                            variant="contained"
                            disabled={isSubmitting}
                            startIcon={<SaveIcon />}
                        >
                            {isSubmitting ? "Saving..." : "Save Changes"}
                        </Button>
                    </>
                ) : (
                    <>
                        <Button onClick={handleClose}>Close</Button>
                        <Button onClick={handleEdit} variant="contained" startIcon={<EditIcon />}>
                            Edit
                        </Button>
                    </>
                )}
            </DialogActions>
        </Dialog>
    );
};

export default ProductVariantDetailModal;
