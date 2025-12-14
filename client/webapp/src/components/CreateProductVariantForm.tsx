import React, { useState, useEffect } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    MenuItem,
    Box,
    Alert,
    CircularProgress,
} from "@mui/material";
import { getProducts } from "@/api/v1/products";
import type { NewProductVariant, Product } from "@/types/inventory";

interface CreateProductVariantFormProps {
    open: boolean;
    onClose: () => void;
    onSubmit: (variantData: NewProductVariant) => Promise<void>;
    productId?: string; // Make optional
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

const CreateProductVariantForm: React.FC<CreateProductVariantFormProps> = ({ open, onClose, onSubmit, productId }) => {
    const [products, setProducts] = useState<Product[]>([]);
    const [loadingProducts, setLoadingProducts] = useState(false);
    const [selectedProductId, setSelectedProductId] = useState<string>(productId || "");
    const [name, setName] = useState("");
    const [quantity, setQuantity] = useState<number>(0);
    const [stock, setStock] = useState<number>(0);
    const [unitOfMeasure, setUnitOfMeasure] = useState<string>("Kilogram");
    const [description, setDescription] = useState("");
    const [error, setError] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Load products when productId is not provided
    useEffect(() => {
        if (open && !productId) {
            loadProducts();
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [open, productId]);

    // Update selectedProductId when productId prop changes
    useEffect(() => {
        if (productId) {
            setSelectedProductId(productId);
        }
    }, [productId]);

    const loadProducts = async () => {
        try {
            setLoadingProducts(true);
            const response = await getProducts();
            setProducts(response.products);
            if (response.products.length > 0 && !selectedProductId) {
                setSelectedProductId(response.products[0].id);
            }
        } catch (err) {
            setError("Failed to load products");
            console.error(err);
        } finally {
            setLoadingProducts(false);
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);
        setIsSubmitting(true);

        try {
            const variantData: NewProductVariant = {
                productId: selectedProductId,
                name: name.trim(),
                quantity,
                stock,
                unitOfMeasure,
                description: description.trim() || null,
            };

            await onSubmit(variantData);
            handleClose();
        } catch (err) {
            setError(err instanceof Error ? err.message : "Failed to create variant");
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleClose = () => {
        if (!isSubmitting) {
            setSelectedProductId(productId || "");
            setName("");
            setQuantity(0);
            setStock(0);
            setUnitOfMeasure("Kilogram");
            setDescription("");
            setError(null);
            onClose();
        }
    };

    const isFormValid = selectedProductId !== "" && name.trim() !== "" && quantity >= 0 && stock >= 0;

    return (
        <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
            <form onSubmit={handleSubmit}>
                <DialogTitle>Create New Product Variant</DialogTitle>
                <DialogContent>
                    <Box sx={{ display: "flex", flexDirection: "column", gap: 2, mt: 1 }}>
                        {error && (
                            <Alert severity="error" onClose={() => setError(null)}>
                                {error}
                            </Alert>
                        )}

                        {!productId &&
                            (loadingProducts ? (
                                <Box sx={{ display: "flex", justifyContent: "center", py: 2 }}>
                                    <CircularProgress size={24} />
                                </Box>
                            ) : (
                                <TextField
                                    label="Product"
                                    select
                                    value={selectedProductId}
                                    onChange={(e) => setSelectedProductId(e.target.value)}
                                    required
                                    fullWidth
                                    helperText="Select the product for this variant"
                                >
                                    {products.map((product) => (
                                        <MenuItem key={product.id} value={product.id}>
                                            {product.name}
                                        </MenuItem>
                                    ))}
                                </TextField>
                            ))}

                        <TextField
                            label="Variant Name"
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                            required
                            fullWidth
                            autoFocus
                            placeholder="e.g., Premium Feed, Standard Mix"
                        />

                        <TextField
                            label="Quantity"
                            type="number"
                            value={quantity}
                            onChange={(e) => setQuantity(Number(e.target.value))}
                            required
                            fullWidth
                            inputProps={{ min: 0, step: 0.01 }}
                        />

                        <TextField
                            label="Stock"
                            type="number"
                            value={stock}
                            onChange={(e) => setStock(Number(e.target.value))}
                            required
                            fullWidth
                            inputProps={{ min: 0, step: 0.01 }}
                        />

                        <TextField
                            label="Unit of Measure"
                            select
                            value={unitOfMeasure}
                            onChange={(e) => setUnitOfMeasure(e.target.value)}
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
                            value={description}
                            onChange={(e) => setDescription(e.target.value)}
                            multiline
                            rows={3}
                            fullWidth
                            placeholder="Optional description..."
                        />
                    </Box>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleClose} disabled={isSubmitting}>
                        Cancel
                    </Button>
                    <Button type="submit" variant="contained" disabled={!isFormValid || isSubmitting}>
                        {isSubmitting ? "Creating..." : "Create Variant"}
                    </Button>
                </DialogActions>
            </form>
        </Dialog>
    );
};

export default CreateProductVariantForm;
