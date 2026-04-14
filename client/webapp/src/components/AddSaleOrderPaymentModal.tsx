import { useState } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    Grid,
    Typography,
    Box,
    CircularProgress,
    Alert,
    Divider,
} from "@mui/material";
import moment from "moment";
import type { SaleOrder, NewSaleOrderPayment } from "@/types/saleOrder";
import useSaleOrders from "@/hooks/useSaleOrders";

interface AddSaleOrderPaymentModalProps {
    open: boolean;
    onClose: () => void;
    onSuccess: () => void;
    saleOrder: SaleOrder | null;
}

export default function AddSaleOrderPaymentModal({
    open,
    onClose,
    onSuccess,
    saleOrder,
}: AddSaleOrderPaymentModalProps) {
    const { loading, addPayment } = useSaleOrders();

    const [amount, setAmount] = useState("");
    const [date, setDate] = useState(moment().format("YYYY-MM-DD"));
    const [notes, setNotes] = useState("");
    const [submitError, setSubmitError] = useState<string | null>(null);

    function resetForm() {
        setAmount("");
        setDate(moment().format("YYYY-MM-DD"));
        setNotes("");
        setSubmitError(null);
    }

    function handleClose() {
        resetForm();
        onClose();
    }

    async function handleSubmit() {
        if (!saleOrder) return;
        setSubmitError(null);

        const parsedAmount = parseFloat(amount);
        if (isNaN(parsedAmount) || parsedAmount <= 0) {
            setSubmitError("Please enter a valid payment amount greater than zero.");
            return;
        }

        if (parsedAmount > saleOrder.pendingAmount) {
            setSubmitError(`Amount cannot exceed the pending amount ($${saleOrder.pendingAmount.toFixed(3)}).`);
            return;
        }

        if (!date) {
            setSubmitError("Please select a payment date.");
            return;
        }

        const payload: NewSaleOrderPayment = {
            amount: parsedAmount,
            dateClientIsoString: moment(date).format(),
            notes: notes.trim() || null,
        };

        try {
            await addPayment(saleOrder.id, payload);
            resetForm();
            onSuccess();
        } catch (err: unknown) {
            const message = err instanceof Error ? err.message : "An error occurred.";
            setSubmitError(message);
        }
    }

    if (!saleOrder) return null;

    return (
        <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
            <DialogTitle>Add Payment</DialogTitle>
            <DialogContent>
                {/* Order summary */}
                <Box
                    sx={{
                        bgcolor: "grey.50",
                        border: "1px solid",
                        borderColor: "grey.200",
                        borderRadius: 1,
                        p: 2,
                        mb: 2,
                        mt: 0.5,
                    }}
                >
                    <Typography variant="subtitle2" fontWeight={600} sx={{ mb: 0.5 }}>
                        {saleOrder.customerFullName}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                        Order date: {moment(saleOrder.date).format("MMM DD, YYYY")}
                    </Typography>
                    <Divider sx={{ my: 1 }} />
                    <Box sx={{ display: "flex", justifyContent: "space-between" }}>
                        <Typography variant="body2">Total value:</Typography>
                        <Typography variant="body2" fontWeight={500}>
                            ${saleOrder.totalAmount.toFixed(3)}
                        </Typography>
                    </Box>
                    <Box sx={{ display: "flex", justifyContent: "space-between" }}>
                        <Typography variant="body2">Already paid:</Typography>
                        <Typography variant="body2" color="success.main" fontWeight={500}>
                            ${saleOrder.totalPaid.toFixed(3)}
                        </Typography>
                    </Box>
                    <Box sx={{ display: "flex", justifyContent: "space-between" }}>
                        <Typography variant="body2" fontWeight={600}>
                            Pending:
                        </Typography>
                        <Typography variant="body2" color="error.main" fontWeight={600}>
                            ${saleOrder.pendingAmount.toFixed(3)}
                        </Typography>
                    </Box>
                </Box>

                <Grid container spacing={2}>
                    <Grid size={{ xs: 12, sm: 6 }}>
                        <TextField
                            label="Amount *"
                            type="number"
                            value={amount}
                            onChange={(e) => setAmount(e.target.value)}
                            fullWidth
                            inputProps={{ min: 0.001, max: saleOrder.pendingAmount, step: "0.001" }}
                            helperText={`Max: $${saleOrder.pendingAmount.toFixed(3)}`}
                        />
                    </Grid>
                    <Grid size={{ xs: 12, sm: 6 }}>
                        <TextField
                            label="Payment Date *"
                            type="date"
                            value={date}
                            onChange={(e) => setDate(e.target.value)}
                            fullWidth
                            InputLabelProps={{ shrink: true }}
                        />
                    </Grid>
                    <Grid size={{ xs: 12 }}>
                        <TextField
                            label="Notes"
                            value={notes}
                            onChange={(e) => setNotes(e.target.value)}
                            fullWidth
                            multiline
                            rows={2}
                        />
                    </Grid>
                </Grid>

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
                    {loading ? <CircularProgress size={18} /> : "Add Payment"}
                </Button>
            </DialogActions>
        </Dialog>
    );
}
