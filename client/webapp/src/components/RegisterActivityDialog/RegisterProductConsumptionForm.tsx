import { useState, useEffect } from "react";
import {
    Box,
    TextField,
    MenuItem,
    Typography,
    FormControl,
    InputLabel,
    Select,
    CircularProgress,
    Alert,
} from "@mui/material";
import type { SelectChangeEvent } from "@mui/material";
import moment from "moment";
import type { NewProductConsumption } from "@/types/batchActivity";
import type { Batch } from "@/types/batch";
import type { Product } from "@/types/inventory";
import { getProducts } from "@/api/v1/products";

interface RegisterProductConsumptionFormProps {
    batch: Batch;
    formData: NewProductConsumption;
    onChange: (data: NewProductConsumption) => void;
    errors: Record<string, string>;
}

const unitOfMeasureOptions = [
    { value: "Kilogram", label: "Kilogram (kg)" },
    { value: "Gram", label: "Gram (g)" },
    { value: "Pound", label: "Pound (lb)" },
    { value: "Liter", label: "Liter (L)" },
    { value: "Milliliter", label: "Milliliter (mL)" },
    { value: "Gallon", label: "Gallon (gal)" },
    { value: "Unit", label: "Unit" },
    { value: "Piece", label: "Piece" },
];

export default function RegisterProductConsumptionForm({
    batch,
    formData,
    onChange,
    errors,
}: RegisterProductConsumptionFormProps) {
    const [products, setProducts] = useState<Product[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);

    useEffect(() => {
        const loadProducts = async () => {
            try {
                setLoading(true);
                setError(null);
                const response = await getProducts();
                setProducts(response.products || []);
            } catch (err) {
                console.error("Error loading products:", err);
                setError("Failed to load products. Please refresh the page.");
            } finally {
                setLoading(false);
            }
        };

        loadProducts();
    }, []);

    useEffect(() => {
        if (formData.productId && products.length > 0) {
            const product = products.find((p) => p.id === formData.productId);
            setSelectedProduct(product || null);
        } else {
            setSelectedProduct(null);
        }
    }, [formData.productId, products]);

    const handleProductChange = (e: SelectChangeEvent<string>) => {
        const productId = e.target.value;
        const product = products.find((p) => p.id === productId);
        onChange({
            ...formData,
            productId,
            unitOfMeasure: product?.unitOfMeasure || "Kilogram",
        });
    };

    const handleStockChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const value = parseFloat(e.target.value) || 0;
        onChange({
            ...formData,
            stock: value,
        });
    };

    const handleUnitChange = (e: SelectChangeEvent<string>) => {
        onChange({
            ...formData,
            unitOfMeasure: e.target.value,
        });
    };

    const handleDateChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const dateValue = e.target.value;
        if (dateValue) {
            const isoString = moment(dateValue).toISOString();
            onChange({
                ...formData,
                dateClientIsoString: isoString,
            });
        }
    };

    const handleNotesChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        onChange({
            ...formData,
            notes: e.target.value || null,
        });
    };

    if (loading) {
        return (
            <Box sx={{ display: "flex", justifyContent: "center", py: 4 }}>
                <CircularProgress />
            </Box>
        );
    }

    if (error) {
        return <Alert severity="error">{error}</Alert>;
    }

    return (
        <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                Recording product consumption for batch: <strong>{batch.name}</strong>
            </Typography>

            <FormControl fullWidth error={!!errors.productId} required>
                <InputLabel id="product-label">Product</InputLabel>
                <Select
                    labelId="product-label"
                    value={formData.productId || ""}
                    onChange={handleProductChange}
                    label="Product"
                    disabled={products.length === 0}
                >
                    {products.length === 0 ? (
                        <MenuItem value="" disabled>
                            No products available
                        </MenuItem>
                    ) : (
                        products.map((product) => (
                            <MenuItem key={product.id} value={product.id}>
                                {product.name} ({product.manufacturer}) - {product.stock.toFixed(2)}{" "}
                                {product.unitOfMeasure} available
                            </MenuItem>
                        ))
                    )}
                </Select>
                {errors.productId && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                        {errors.productId}
                    </Typography>
                )}
            </FormControl>

            <TextField
                fullWidth
                type="number"
                label="Stock Consumed"
                value={formData.stock || ""}
                onChange={handleStockChange}
                required
                error={!!errors.stock}
                helperText={
                    errors.stock ||
                    (selectedProduct
                        ? `Available: ${selectedProduct.stock.toFixed(2)} ${selectedProduct.unitOfMeasure}`
                        : "Select a product first")
                }
                inputProps={{
                    min: 0.01,
                    step: "0.01",
                }}
            />

            <FormControl fullWidth error={!!errors.unitOfMeasure} required>
                <InputLabel id="unit-label">Unit of Measure</InputLabel>
                <Select
                    labelId="unit-label"
                    value={formData.unitOfMeasure}
                    onChange={handleUnitChange}
                    label="Unit of Measure"
                >
                    {unitOfMeasureOptions.map((option) => (
                        <MenuItem key={option.value} value={option.value}>
                            {option.label}
                        </MenuItem>
                    ))}
                </Select>
                {errors.unitOfMeasure && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                        {errors.unitOfMeasure}
                    </Typography>
                )}
            </FormControl>

            <TextField
                fullWidth
                type="date"
                label="Date"
                value={
                    formData.dateClientIsoString
                        ? moment(formData.dateClientIsoString).format("YYYY-MM-DD")
                        : moment().format("YYYY-MM-DD")
                }
                onChange={handleDateChange}
                required
                error={!!errors.dateClientIsoString}
                helperText={errors.dateClientIsoString}
                InputLabelProps={{
                    shrink: true,
                }}
            />

            <TextField
                fullWidth
                multiline
                rows={3}
                label="Notes (Optional)"
                value={formData.notes || ""}
                onChange={handleNotesChange}
                error={!!errors.notes}
                helperText={errors.notes || `${formData.notes?.length || 0}/500 characters`}
                inputProps={{
                    maxLength: 500,
                }}
            />
        </Box>
    );
}
