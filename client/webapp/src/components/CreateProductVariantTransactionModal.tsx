import { useState, useEffect } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    Stack,
    Alert,
    CircularProgress,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    Box,
    Typography,
    useTheme,
    useMediaQuery,
} from "@mui/material";
import moment from "moment";
import { getVendors } from "@/api/v1/vendors";
import { createTransaction } from "@/api/v1/transactions";
import type { Vendor } from "@/types/vendor";

interface CreateProductVariantTransactionModalProps {
    open: boolean;
    onClose: () => void;
    onSuccess: () => void;
    productVariantId: string;
    productVariantName: string;
}

export default function CreateProductVariantTransactionModal({
    open,
    onClose,
    onSuccess,
    productVariantId,
    productVariantName,
}: CreateProductVariantTransactionModalProps) {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("sm"));

    const [formData, setFormData] = useState({
        title: "",
        date: moment().format("YYYY-MM-DD"),
        unitPrice: "",
        quantity: "",
        notes: "",
        vendorId: "",
    });

    const [vendors, setVendors] = useState<Vendor[]>([]);
    const [vendorsLoading, setVendorsLoading] = useState(false);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Load vendors when modal opens
    useEffect(() => {
        if (open) {
            const loadVendors = async () => {
                try {
                    setVendorsLoading(true);
                    const response = await getVendors();
                    setVendors(response.vendors);
                } catch (err) {
                    console.error("Failed to load vendors:", err);
                } finally {
                    setVendorsLoading(false);
                }
            };
            loadVendors();
        }
    }, [open]);

    // Reset form when modal opens
    useEffect(() => {
        if (open) {
            setFormData({
                title: `Purchase: ${productVariantName}`,
                date: moment().format("YYYY-MM-DD"),
                unitPrice: "",
                quantity: "",
                notes: "",
                vendorId: "",
            });
            setError(null);
        }
    }, [open, productVariantName]);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setError(null);

        // Validation
        if (!formData.title.trim()) {
            setError("Title is required");
            return;
        }

        if (!formData.vendorId) {
            setError("Vendor is required");
            return;
        }

        const unitPrice = parseFloat(formData.unitPrice);
        const quantity = parseFloat(formData.quantity);

        if (isNaN(unitPrice) || unitPrice <= 0) {
            setError("Unit price must be a positive number");
            return;
        }

        if (isNaN(quantity) || quantity <= 0) {
            setError("Quantity must be a positive number");
            return;
        }

        try {
            setLoading(true);

            const transactionAmount = unitPrice * quantity;

            await createTransaction({
                title: formData.title.trim(),
                dateClientIsoString: moment(formData.date).format(),
                type: "Expense",
                unitPrice,
                quantity,
                transactionAmount,
                notes: formData.notes.trim() || null,
                vendorId: formData.vendorId,
                productVariantId,
                batchId: null,
                customerId: null,
            });

            onSuccess();
        } catch (err: any) {
            console.error("Failed to create transaction:", err);
            setError(err?.response?.detail || "Failed to create transaction. Please try again.");
        } finally {
            setLoading(false);
        }
    };

    const handleClose = () => {
        if (!loading) {
            onClose();
        }
    };

    const totalAmount = parseFloat(formData.unitPrice) * parseFloat(formData.quantity);
    const isValidAmount = !isNaN(totalAmount) && totalAmount > 0;

    return (
        <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth fullScreen={isMobile}>
            <form onSubmit={handleSubmit}>
                <DialogTitle>
                    <Typography variant="h6" component="div">
                        Add Purchase Transaction
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                        {productVariantName}
                    </Typography>
                </DialogTitle>

                <DialogContent>
                    <Stack spacing={3} sx={{ mt: 1 }}>
                        {error && (
                            <Alert severity="error" onClose={() => setError(null)}>
                                {error}
                            </Alert>
                        )}

                        <TextField
                            label="Title"
                            value={formData.title}
                            onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                            required
                            fullWidth
                            autoFocus
                        />

                        <TextField
                            label="Date"
                            type="date"
                            value={formData.date}
                            onChange={(e) => setFormData({ ...formData, date: e.target.value })}
                            required
                            fullWidth
                            InputLabelProps={{ shrink: true }}
                        />

                        <FormControl fullWidth required>
                            <InputLabel>Vendor</InputLabel>
                            <Select
                                value={formData.vendorId}
                                onChange={(e) => setFormData({ ...formData, vendorId: e.target.value })}
                                label="Vendor"
                                disabled={vendorsLoading}
                            >
                                {vendorsLoading ? (
                                    <MenuItem disabled>
                                        <CircularProgress size={20} sx={{ mr: 1 }} />
                                        Loading vendors...
                                    </MenuItem>
                                ) : (
                                    vendors.map((vendor) => (
                                        <MenuItem key={vendor.id} value={vendor.id}>
                                            {vendor.name}
                                        </MenuItem>
                                    ))
                                )}
                            </Select>
                        </FormControl>

                        <TextField
                            label="Unit Price"
                            type="number"
                            value={formData.unitPrice}
                            onChange={(e) => setFormData({ ...formData, unitPrice: e.target.value })}
                            required
                            fullWidth
                            inputProps={{ min: 0, step: 0.01 }}
                        />

                        <TextField
                            label="Quantity"
                            type="number"
                            value={formData.quantity}
                            onChange={(e) => setFormData({ ...formData, quantity: e.target.value })}
                            required
                            fullWidth
                            inputProps={{ min: 0, step: 0.01 }}
                        />

                        {isValidAmount && (
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
                                    Total Amount
                                </Typography>
                                <Typography variant="h5" sx={{ fontWeight: "bold", color: "primary.main" }}>
                                    ${totalAmount.toFixed(2)}
                                </Typography>
                            </Box>
                        )}

                        <TextField
                            label="Notes (Optional)"
                            value={formData.notes}
                            onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                            multiline
                            rows={3}
                            fullWidth
                        />
                    </Stack>
                </DialogContent>

                <DialogActions sx={{ px: 3, py: 2 }}>
                    <Button onClick={handleClose} disabled={loading}>
                        Cancel
                    </Button>
                    <Button type="submit" variant="contained" disabled={loading}>
                        {loading ? <CircularProgress size={24} /> : "Create Transaction"}
                    </Button>
                </DialogActions>
            </form>
        </Dialog>
    );
}
