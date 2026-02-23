import { useState, useEffect } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    Alert,
    CircularProgress,
    Typography,
    Box,
} from "@mui/material";
import type { ProductVariant } from "@/types/inventory";

interface AddProductStockModalProps {
    open: boolean;
    onClose: () => void;
    onSubmit: (productVariantId: string, quantity: number) => Promise<void>;
    variants: ProductVariant[];
    productName: string;
}

export default function AddProductStockModal({
    open,
    onClose,
    onSubmit,
    variants,
    productName,
}: AddProductStockModalProps) {
    const [selectedVariantId, setSelectedVariantId] = useState("");
    const [quantity, setQuantity] = useState(1);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (open) {
            setSelectedVariantId("");
            setQuantity(1);
            setError(null);
        }
    }, [open]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);

        if (!selectedVariantId) {
            setError("Please select a product variant");
            return;
        }

        if (quantity <= 0) {
            setError("Quantity must be greater than 0");
            return;
        }

        try {
            setLoading(true);
            await onSubmit(selectedVariantId, quantity);
            onClose();
        } catch (err: any) {
            console.error("Failed to add stock:", err);
            setError(err?.response?.detail || "Failed to add stock. Please try again.");
        } finally {
            setLoading(false);
        }
    };

    const selectedVariant = variants.find((v) => v.id === selectedVariantId);
    const stockToAdd = selectedVariant ? selectedVariant.stock * quantity : 0;

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
            <form onSubmit={handleSubmit}>
                <DialogTitle>Add Stock to {productName}</DialogTitle>

                <DialogContent>
                    <Box sx={{ display: "flex", flexDirection: "column", gap: 3, mt: 1 }}>
                        {error && (
                            <Alert severity="error" onClose={() => setError(null)}>
                                {error}
                            </Alert>
                        )}

                        <FormControl fullWidth required>
                            <InputLabel>Product Variant</InputLabel>
                            <Select
                                value={selectedVariantId}
                                onChange={(e) => setSelectedVariantId(e.target.value)}
                                label="Product Variant"
                            >
                                {variants.length === 0 ? (
                                    <MenuItem disabled>No variants available</MenuItem>
                                ) : (
                                    variants.map((variant) => (
                                        <MenuItem key={variant.id} value={variant.id}>
                                            {variant.name} - {variant.stock} {variant.unitOfMeasure}
                                        </MenuItem>
                                    ))
                                )}
                            </Select>
                        </FormControl>

                        <TextField
                            label="Quantity"
                            type="number"
                            value={quantity}
                            onChange={(e) => setQuantity(parseInt(e.target.value) || 0)}
                            required
                            fullWidth
                            inputProps={{ min: 1, step: 1 }}
                            helperText="Number of variant units to add"
                        />

                        {selectedVariant && quantity > 0 && (
                            <Box
                                sx={{
                                    p: 2,
                                    bgcolor: "primary.50",
                                    borderRadius: 1,
                                    border: 1,
                                    borderColor: "primary.main",
                                }}
                            >
                                <Typography variant="body2" color="text.secondary">
                                    Stock to be added
                                </Typography>
                                <Typography variant="h5" sx={{ fontWeight: "bold", color: "primary.main" }}>
                                    {stockToAdd} {selectedVariant.unitOfMeasure}
                                </Typography>
                                <Typography variant="caption" color="text.secondary">
                                    {quantity} × {selectedVariant.stock} {selectedVariant.unitOfMeasure}
                                </Typography>
                            </Box>
                        )}
                    </Box>
                </DialogContent>

                <DialogActions sx={{ px: 3, py: 2 }}>
                    <Button onClick={onClose} disabled={loading}>
                        Cancel
                    </Button>
                    <Button type="submit" variant="contained" disabled={loading || variants.length === 0}>
                        {loading ? <CircularProgress size={24} /> : "Add Stock"}
                    </Button>
                </DialogActions>
            </form>
        </Dialog>
    );
}
