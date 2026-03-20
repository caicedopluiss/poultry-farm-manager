import { useState, useEffect } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    Grid,
    MenuItem,
    IconButton,
    Typography,
    Box,
    CircularProgress,
    Alert,
    Divider,
} from "@mui/material";
import { Add as AddIcon, Delete as DeleteIcon } from "@mui/icons-material";
import moment from "moment";
import type { NewSaleOrder, NewSaleOrderItem } from "@/types/saleOrder";
import type { Person } from "@/types/person";
import { getPersons } from "@/api/v1/persons";
import useSaleOrders from "@/hooks/useSaleOrders";

const UNIT_OPTIONS = ["Kilogram", "Gram", "Pound"] as const;

interface CreateSaleOrderModalProps {
    open: boolean;
    onClose: () => void;
    onSuccess: () => void;
    batchId: string;
}

interface ItemForm {
    weight: string;
    unitOfMeasure: string;
    processedDate: string;
}

const emptyItem = (): ItemForm => ({
    weight: "",
    unitOfMeasure: "Kilogram",
    processedDate: moment().format("YYYY-MM-DD"),
});

export default function CreateSaleOrderModal({ open, onClose, onSuccess, batchId }: CreateSaleOrderModalProps) {
    const { loading, createSaleOrder } = useSaleOrders();

    const [customerId, setCustomerId] = useState("");
    const [date, setDate] = useState(moment().format("YYYY-MM-DD"));
    const [pricePerUnit, setPricePerUnit] = useState("");
    const [notes, setNotes] = useState("");
    const [items, setItems] = useState<ItemForm[]>([emptyItem()]);
    const [submitError, setSubmitError] = useState<string | null>(null);

    const [persons, setPersons] = useState<Person[]>([]);
    const [personsLoading, setPersonsLoading] = useState(false);

    useEffect(() => {
        if (!open) return;
        setPersonsLoading(true);
        getPersons()
            .then((r) => setPersons(r.persons))
            .finally(() => setPersonsLoading(false));
    }, [open]);

    function resetForm() {
        setCustomerId("");
        setDate(moment().format("YYYY-MM-DD"));
        setPricePerUnit("");
        setNotes("");
        setItems([emptyItem()]);
        setSubmitError(null);
    }

    function handleClose() {
        resetForm();
        onClose();
    }

    function updateItem(index: number, field: keyof ItemForm, value: string) {
        setItems((prev) => prev.map((item, i) => (i === index ? { ...item, [field]: value } : item)));
    }

    function addItem() {
        setItems((prev) => [...prev, emptyItem()]);
    }

    function removeItem(index: number) {
        setItems((prev) => prev.filter((_, i) => i !== index));
    }

    async function handleSubmit() {
        setSubmitError(null);

        const parsedPrice = parseFloat(pricePerUnit);
        if (!customerId || !date || isNaN(parsedPrice) || parsedPrice <= 0) {
            setSubmitError("Please fill in all required fields with valid values.");
            return;
        }

        for (const item of items) {
            const w = parseFloat(item.weight);
            if (isNaN(w) || w <= 0 || !item.unitOfMeasure || !item.processedDate) {
                setSubmitError("Each item must have a valid weight, unit of measure, and processed date.");
                return;
            }
        }

        const payload: NewSaleOrder = {
            batchId,
            customerId,
            dateClientIsoString: moment(date).format(),
            pricePerUnit: parsedPrice,
            notes: notes.trim() || null,
            items: items.map<NewSaleOrderItem>((item) => ({
                weight: parseFloat(item.weight),
                unitOfMeasure: item.unitOfMeasure,
                processedDateClientIsoString: moment(item.processedDate).format(),
            })),
        };

        try {
            await createSaleOrder(payload);
            resetForm();
            onSuccess();
        } catch (err: unknown) {
            const message = err instanceof Error ? err.message : "An error occurred.";
            setSubmitError(message);
        }
    }

    return (
        <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
            <DialogTitle>New Sale Order</DialogTitle>
            <DialogContent>
                <Grid container spacing={2} sx={{ mt: 0.5 }}>
                    <Grid size={{ xs: 12, sm: 6 }}>
                        <TextField
                            select
                            label="Customer *"
                            value={customerId}
                            onChange={(e) => setCustomerId(e.target.value)}
                            fullWidth
                            disabled={personsLoading}
                            InputProps={{
                                endAdornment: personsLoading ? <CircularProgress size={18} /> : undefined,
                            }}
                        >
                            {persons.map((p) => (
                                <MenuItem key={p.id} value={p.id}>
                                    {p.firstName} {p.lastName}
                                </MenuItem>
                            ))}
                        </TextField>
                    </Grid>

                    <Grid size={{ xs: 12, sm: 6 }}>
                        <TextField
                            label="Order Date *"
                            type="date"
                            value={date}
                            onChange={(e) => setDate(e.target.value)}
                            fullWidth
                            InputLabelProps={{ shrink: true }}
                        />
                    </Grid>

                    <Grid size={{ xs: 12, sm: 6 }}>
                        <TextField
                            label="Price per Kg *"
                            type="number"
                            value={pricePerUnit}
                            onChange={(e) => setPricePerUnit(e.target.value)}
                            fullWidth
                            inputProps={{ min: 0, step: "0.01" }}
                        />
                    </Grid>

                    <Grid size={{ xs: 12, sm: 6 }}>
                        <TextField
                            label="Notes"
                            value={notes}
                            onChange={(e) => setNotes(e.target.value)}
                            fullWidth
                            multiline
                            rows={1}
                        />
                    </Grid>
                </Grid>

                <Box sx={{ mt: 3, mb: 1, display: "flex", alignItems: "center", justifyContent: "space-between" }}>
                    <Typography variant="subtitle1" fontWeight={600}>
                        Items
                    </Typography>
                    <Button startIcon={<AddIcon />} size="small" onClick={addItem}>
                        Add Item
                    </Button>
                </Box>
                <Divider sx={{ mb: 2 }} />

                {items.map((item, i) => (
                    <Grid container spacing={2} key={i} sx={{ mb: 1.5, alignItems: "center" }}>
                        <Grid size={{ xs: 12, sm: 3 }}>
                            <TextField
                                label="Weight *"
                                type="number"
                                value={item.weight}
                                onChange={(e) => updateItem(i, "weight", e.target.value)}
                                fullWidth
                                size="small"
                                inputProps={{ min: 0, step: "0.01" }}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 3 }}>
                            <TextField
                                select
                                label="Unit of Measure *"
                                value={item.unitOfMeasure}
                                onChange={(e) => updateItem(i, "unitOfMeasure", e.target.value)}
                                fullWidth
                                size="small"
                            >
                                {UNIT_OPTIONS.map((u) => (
                                    <MenuItem key={u} value={u}>
                                        {u}
                                    </MenuItem>
                                ))}
                            </TextField>
                        </Grid>
                        <Grid size={{ xs: 12, sm: 4 }}>
                            <TextField
                                label="Processed Date *"
                                type="date"
                                value={item.processedDate}
                                onChange={(e) => updateItem(i, "processedDate", e.target.value)}
                                fullWidth
                                size="small"
                                InputLabelProps={{ shrink: true }}
                            />
                        </Grid>
                        <Grid size={{ xs: 12, sm: 2 }} sx={{ display: "flex", justifyContent: "center" }}>
                            <IconButton
                                size="small"
                                color="error"
                                onClick={() => removeItem(i)}
                                disabled={items.length === 1}
                            >
                                <DeleteIcon fontSize="small" />
                            </IconButton>
                        </Grid>
                    </Grid>
                ))}

                {submitError && (
                    <Alert severity="error" sx={{ mt: 2 }}>
                        {submitError}
                    </Alert>
                )}
            </DialogContent>
            <DialogActions>
                <Button onClick={handleClose} disabled={loading}>
                    Cancel
                </Button>
                <Button onClick={handleSubmit} variant="contained" disabled={loading}>
                    {loading ? <CircularProgress size={18} /> : "Create Sale Order"}
                </Button>
            </DialogActions>
        </Dialog>
    );
}
