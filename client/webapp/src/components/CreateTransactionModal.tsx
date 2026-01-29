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
    Typography,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    Box,
    useTheme,
    useMediaQuery,
} from "@mui/material";
import moment from "moment";
import useTransactions from "@/hooks/useTransactions";
import { getVendors } from "@/api/v1/vendors";
import { getPersons } from "@/api/v1/persons";
import type { TransactionType } from "@/types/transaction";
import type { Vendor } from "@/types/vendor";
import type { Person } from "@/types/person";

interface CreateTransactionModalProps {
    open: boolean;
    onClose: () => void;
    onSuccess: () => void;
    batchId: string;
    transactionType: TransactionType;
}

export default function CreateTransactionModal({
    open,
    onClose,
    onSuccess,
    batchId,
    transactionType,
}: CreateTransactionModalProps) {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("sm"));

    const [formData, setFormData] = useState({
        title: "",
        date: moment().format("YYYY-MM-DD"),
        unitPrice: "",
        quantity: "",
        notes: "",
        vendorId: "",
        customerId: "",
    });

    const [vendors, setVendors] = useState<Vendor[]>([]);
    const [vendorsLoading, setVendorsLoading] = useState(false);
    const [persons, setPersons] = useState<Person[]>([]);
    const [personsLoading, setPersonsLoading] = useState(false);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const { createTransaction } = useTransactions();

    // Load vendors when modal opens (for expenses)
    useEffect(() => {
        if (open && transactionType === "Expense") {
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
    }, [open, transactionType]);

    // Load persons/customers when modal opens (for income)
    useEffect(() => {
        if (open && transactionType === "Income") {
            const loadPersons = async () => {
                try {
                    setPersonsLoading(true);
                    const response = await getPersons();
                    setPersons(response.persons);
                } catch (err) {
                    console.error("Failed to load persons:", err);
                } finally {
                    setPersonsLoading(false);
                }
            };
            loadPersons();
        }
    }, [open, transactionType]);

    // Reset form when modal opens/closes or type changes
    useEffect(() => {
        if (open) {
            setFormData({
                title: "",
                date: moment().format("YYYY-MM-DD"),
                unitPrice: "",
                quantity: "",
                notes: "",
                vendorId: "",
                customerId: "",
            });
            setError(null);
        }
    }, [open, transactionType]);

    const handleInputChange = (field: string) => (event: React.ChangeEvent<HTMLInputElement>) => {
        setFormData((prev) => ({ ...prev, [field]: event.target.value }));
    };

    const handleSubmit = async (event: React.FormEvent) => {
        event.preventDefault();

        if (!formData.title || !formData.unitPrice || !formData.date) {
            setError("Please fill in all required fields");
            return;
        }

        const unitPrice = parseFloat(formData.unitPrice);
        const quantity = formData.quantity ? parseInt(formData.quantity) : null;

        if (isNaN(unitPrice) || unitPrice <= 0) {
            setError("Unit price must be a positive number");
            return;
        }

        if (formData.quantity && (isNaN(quantity!) || quantity! <= 0)) {
            setError("Quantity must be a positive number");
            return;
        }

        try {
            setLoading(true);
            setError(null);

            const transactionAmount = quantity ? unitPrice * quantity : unitPrice;

            await createTransaction({
                title: formData.title,
                dateClientIsoString: moment(formData.date).format(),
                type: transactionType,
                unitPrice,
                quantity,
                transactionAmount,
                notes: formData.notes || null,
                productVariantId: null,
                batchId,
                vendorId: formData.vendorId || null,
                customerId: formData.customerId || null,
            });

            onSuccess();
        } catch (err) {
            const apiError = err as { response?: { data?: { message?: string } } };
            setError(apiError.response?.data?.message || "Failed to create transaction");
            console.error("Error creating transaction:", err);
        } finally {
            setLoading(false);
        }
    };

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth fullScreen={isMobile}>
            <DialogTitle>
                <Typography variant="h5" component="div" sx={{ fontWeight: "bold" }}>
                    Add {transactionType}
                </Typography>
            </DialogTitle>

            <DialogContent>
                {error && (
                    <Alert severity="error" sx={{ mb: 2 }}>
                        {error}
                    </Alert>
                )}

                <Box component="form" onSubmit={handleSubmit} sx={{ mt: 2 }}>
                    <Stack spacing={3}>
                        <TextField
                            fullWidth
                            label="Title"
                            value={formData.title}
                            onChange={handleInputChange("title")}
                            required
                            disabled={loading}
                        />

                        <TextField
                            fullWidth
                            label="Date"
                            type="date"
                            value={formData.date}
                            onChange={handleInputChange("date")}
                            required
                            disabled={loading}
                            InputLabelProps={{
                                shrink: true,
                            }}
                        />

                        <TextField
                            fullWidth
                            label="Unit Price"
                            type="number"
                            value={formData.unitPrice}
                            onChange={handleInputChange("unitPrice")}
                            required
                            disabled={loading}
                            inputProps={{ min: 0.01, step: 0.01 }}
                        />

                        <TextField
                            fullWidth
                            label="Quantity (Optional)"
                            type="number"
                            value={formData.quantity}
                            onChange={handleInputChange("quantity")}
                            disabled={loading}
                            inputProps={{ min: 1, step: 1 }}
                            helperText="Leave empty if not applicable"
                        />

                        {transactionType === "Expense" && (
                            <FormControl fullWidth disabled={loading || vendorsLoading}>
                                <InputLabel>Vendor (Optional)</InputLabel>
                                <Select
                                    value={formData.vendorId}
                                    onChange={(e) => setFormData((prev) => ({ ...prev, vendorId: e.target.value }))}
                                    label="Vendor (Optional)"
                                >
                                    <MenuItem value="">
                                        <em>None</em>
                                    </MenuItem>
                                    {vendors.map((vendor) => (
                                        <MenuItem key={vendor.id} value={vendor.id}>
                                            {vendor.name}
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        )}

                        {transactionType === "Income" && (
                            <FormControl fullWidth required disabled={loading || personsLoading}>
                                <InputLabel>Customer *</InputLabel>
                                <Select
                                    value={formData.customerId}
                                    onChange={(e) => setFormData((prev) => ({ ...prev, customerId: e.target.value }))}
                                    label="Customer *"
                                >
                                    <MenuItem value="">
                                        <em>Select a customer</em>
                                    </MenuItem>
                                    {persons.map((person) => (
                                        <MenuItem key={person.id} value={person.id}>
                                            {person.firstName} {person.lastName}
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        )}

                        <TextField
                            fullWidth
                            label="Notes (Optional)"
                            value={formData.notes}
                            onChange={handleInputChange("notes")}
                            disabled={loading}
                            multiline
                            rows={3}
                        />

                        {formData.unitPrice && (
                            <Box sx={{ bgcolor: "grey.100", p: 2, borderRadius: 1 }}>
                                <Typography variant="body2" color="text.secondary">
                                    Total Amount
                                </Typography>
                                <Typography variant="h6" sx={{ fontWeight: "bold" }}>
                                    $
                                    {(
                                        parseFloat(formData.unitPrice || "0") *
                                        (formData.quantity ? parseInt(formData.quantity) : 1)
                                    ).toFixed(2)}
                                </Typography>
                            </Box>
                        )}
                    </Stack>
                </Box>
            </DialogContent>

            <DialogActions sx={{ px: 3, py: 2 }}>
                <Button onClick={onClose} disabled={loading} color="inherit">
                    Cancel
                </Button>
                <Button
                    onClick={handleSubmit}
                    variant="contained"
                    disabled={
                        loading ||
                        !formData.title ||
                        !formData.unitPrice ||
                        (transactionType === "Income" && !formData.customerId)
                    }
                    startIcon={loading ? <CircularProgress size={20} /> : null}
                    color={transactionType === "Income" ? "success" : "error"}
                >
                    {loading ? "Creating..." : `Add ${transactionType}`}
                </Button>
            </DialogActions>
        </Dialog>
    );
}
