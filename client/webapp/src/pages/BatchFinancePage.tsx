import { useParams, useNavigate } from "react-router-dom";
import { alpha } from "@mui/material/styles";
import { useState, useEffect, useCallback } from "react";
import {
    Container,
    Box,
    Button,
    CircularProgress,
    Alert,
    Typography,
    Card,
    CardContent,
    Grid,
    Tabs,
    Tab,
    Paper,
} from "@mui/material";
import {
    ArrowBack as BackIcon,
    TrendingUp as IncomeIcon,
    TrendingDown as ExpenseIcon,
    AccountBalance as CollectedIcon,
    HourglassEmpty as PendingIcon,
    Add as AddIcon,
} from "@mui/icons-material";
import TransactionsTable from "@/components/TransactionsTable";
import CreateTransactionModal from "@/components/CreateTransactionModal";
import SaleOrdersList from "@/components/SaleOrdersList";
import CreateSaleOrderModal from "@/components/CreateSaleOrderModal";
import AddSaleOrderPaymentModal from "@/components/AddSaleOrderPaymentModal";
import useBatches from "@/hooks/useBatches";
import useTransactions from "@/hooks/useTransactions";
import useSaleOrders from "@/hooks/useSaleOrders";
import type { Batch } from "@/types/batch";
import type { Transaction } from "@/types/transaction";
import type { SaleOrder } from "@/types/saleOrder";

export default function BatchFinancePage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [batch, setBatch] = useState<Batch | null>(null);
    const [transactions, setTransactions] = useState<Transaction[]>([]);
    const [saleOrders, setSaleOrders] = useState<SaleOrder[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [currentTab, setCurrentTab] = useState(0);

    // Modals
    const [createExpenseOpen, setCreateExpenseOpen] = useState(false);
    const [createSaleOrderOpen, setCreateSaleOrderOpen] = useState(false);
    const [addPaymentOrder, setAddPaymentOrder] = useState<SaleOrder | null>(null);

    const { fetchBatchById } = useBatches();
    const { fetchBatchTransactions } = useTransactions();
    const { fetchBatchSaleOrders, cancelOrder } = useSaleOrders();

    const loadData = useCallback(async () => {
        if (!id) {
            setError("No batch ID provided");
            return;
        }

        try {
            setIsLoading(true);
            setError(null);
            const { batch: batchData } = await fetchBatchById(id);
            setBatch(batchData);
            if (!batchData) {
                setError("Batch not found");
                return;
            }

            const [transactionsData, saleOrdersData] = await Promise.all([
                fetchBatchTransactions(id),
                fetchBatchSaleOrders(id),
            ]);
            setTransactions(transactionsData);
            setSaleOrders(saleOrdersData);
        } catch (err) {
            setError("Failed to load batch finance data");
            console.error("Error loading batch finance:", err);
        } finally {
            setIsLoading(false);
        }
    }, [id, fetchBatchById, fetchBatchTransactions, fetchBatchSaleOrders]);

    useEffect(() => {
        loadData();
    }, [loadData]);

    const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
        setCurrentTab(newValue);
    };

    const handleCancelOrder = useCallback(
        async (order: SaleOrder) => {
            try {
                await cancelOrder(order.id);
                await loadData();
            } catch {
                setError("Failed to cancel sale order");
            }
        },
        [cancelOrder, loadData],
    );

    const expenseTransactions = transactions.filter((t) => t.type === "Expense");

    // Totals
    const totalSalesValue = saleOrders
        .filter((o) => o.status !== "Cancelled")
        .reduce((sum, o) => sum + o.totalAmount, 0);
    const totalCollected = saleOrders.reduce((sum, o) => sum + o.totalPaid, 0);
    const totalPending = saleOrders
        .filter((o) => o.status !== "Cancelled")
        .reduce((sum, o) => sum + o.pendingAmount, 0);
    const totalExpense = expenseTransactions.reduce((sum, t) => sum + t.transactionAmount, 0);
    const net = totalCollected - totalExpense;

    // Loading state
    if (isLoading) {
        return (
            <Container maxWidth="lg" sx={{ py: 3 }}>
                <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", py: 8 }}>
                    <CircularProgress />
                </Box>
            </Container>
        );
    }

    // Error state
    if (error || !batch) {
        return (
            <Container maxWidth="lg" sx={{ py: 3 }}>
                <Button
                    variant="outlined"
                    startIcon={<BackIcon />}
                    onClick={() => navigate(`/batches/${id}`)}
                    sx={{ mb: 2 }}
                >
                    Back to Batch
                </Button>

                <Alert severity="error" sx={{ mb: 3 }}>
                    {error || "Batch not found"}
                </Alert>

                <Button variant="contained" onClick={() => navigate(`/batches/${id}`)}>
                    Back to Batch
                </Button>
            </Container>
        );
    }

    return (
        <Container maxWidth="lg" sx={{ py: 3 }}>
            <Button
                variant="outlined"
                startIcon={<BackIcon />}
                onClick={() => navigate(`/batches/${id}`)}
                sx={{ mb: 3 }}
            >
                Back to Batch
            </Button>

            <Typography variant="h4" gutterBottom sx={{ fontWeight: "bold" }}>
                {batch.name} - Finance
            </Typography>

            {/* Summary Cards */}
            <Grid container spacing={2} sx={{ mb: 4 }}>
                <Grid size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }}>
                    <Card
                        sx={{
                            bgcolor: (theme) => alpha(theme.palette.success.main, 0.08),
                            borderLeft: 4,
                            borderColor: "success.main",
                            height: "100%",
                        }}
                    >
                        <CardContent sx={{ py: 1.5, "&:last-child": { pb: 1.5 } }}>
                            <Box sx={{ display: "flex", alignItems: "center", mb: 0.5 }}>
                                <IncomeIcon sx={{ mr: 0.75, color: "success.main", fontSize: 18 }} />
                                <Typography variant="body2" color="success.dark" sx={{ fontWeight: 600 }}>
                                    Total Sales Value
                                </Typography>
                            </Box>
                            <Typography variant="h5" sx={{ fontWeight: "bold", color: "success.dark", mb: 0.25 }}>
                                ${totalSalesValue.toFixed(3)}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                                {saleOrders.filter((o) => o.status !== "Cancelled").length} active order(s)
                            </Typography>
                        </CardContent>
                    </Card>
                </Grid>

                <Grid size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }}>
                    <Card
                        sx={{
                            bgcolor: (theme) => alpha(theme.palette.primary.main, 0.08),
                            borderLeft: 4,
                            borderColor: "primary.main",
                            height: "100%",
                        }}
                    >
                        <CardContent sx={{ py: 1.5, "&:last-child": { pb: 1.5 } }}>
                            <Box sx={{ display: "flex", alignItems: "center", mb: 0.5 }}>
                                <CollectedIcon sx={{ mr: 0.75, color: "primary.main", fontSize: 18 }} />
                                <Typography variant="body2" color="primary.dark" sx={{ fontWeight: 600 }}>
                                    Total Collected
                                </Typography>
                            </Box>
                            <Typography variant="h5" sx={{ fontWeight: "bold", color: "primary.dark", mb: 0.25 }}>
                                ${totalCollected.toFixed(3)}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                                Payments received
                            </Typography>
                        </CardContent>
                    </Card>
                </Grid>

                <Grid size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }}>
                    <Card
                        sx={{
                            bgcolor: (theme) => alpha(theme.palette.warning.main, 0.08),
                            borderLeft: 4,
                            borderColor: "warning.main",
                            height: "100%",
                        }}
                    >
                        <CardContent sx={{ py: 1.5, "&:last-child": { pb: 1.5 } }}>
                            <Box sx={{ display: "flex", alignItems: "center", mb: 0.5 }}>
                                <PendingIcon sx={{ mr: 0.75, color: "warning.main", fontSize: 18 }} />
                                <Typography variant="body2" color="warning.dark" sx={{ fontWeight: 600 }}>
                                    Total Pending
                                </Typography>
                            </Box>
                            <Typography variant="h5" sx={{ fontWeight: "bold", color: "warning.dark", mb: 0.25 }}>
                                ${totalPending.toFixed(3)}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                                Outstanding balance
                            </Typography>
                        </CardContent>
                    </Card>
                </Grid>

                <Grid size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }}>
                    <Card
                        sx={{
                            bgcolor: (theme) => alpha(theme.palette.error.main, 0.08),
                            borderLeft: 4,
                            borderColor: "error.main",
                            height: "100%",
                        }}
                    >
                        <CardContent sx={{ py: 1.5, "&:last-child": { pb: 1.5 } }}>
                            <Box sx={{ display: "flex", alignItems: "center", mb: 0.5 }}>
                                <ExpenseIcon sx={{ mr: 0.75, color: "error.main", fontSize: 18 }} />
                                <Typography variant="body2" color="error.dark" sx={{ fontWeight: 600 }}>
                                    Total Expenses
                                </Typography>
                            </Box>
                            <Typography variant="h5" sx={{ fontWeight: "bold", color: "error.dark", mb: 0.25 }}>
                                ${totalExpense.toFixed(3)}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                                {expenseTransactions.length} transaction(s)
                            </Typography>
                        </CardContent>
                    </Card>
                </Grid>

                <Grid size={{ xs: 12, sm: 6, md: 4, lg: 2.4 }}>
                    <Card
                        sx={{
                            bgcolor: (theme) =>
                                alpha(net >= 0 ? theme.palette.primary.main : theme.palette.warning.main, 0.08),
                            borderLeft: 4,
                            borderColor: net >= 0 ? "primary.main" : "warning.main",
                            height: "100%",
                        }}
                    >
                        <CardContent sx={{ py: 1.5, "&:last-child": { pb: 1.5 } }}>
                            <Typography
                                variant="body2"
                                sx={{ mb: 0.5, fontWeight: 600, color: net >= 0 ? "primary.dark" : "warning.dark" }}
                            >
                                Net Profit/Loss
                            </Typography>
                            <Typography
                                variant="h5"
                                sx={{ fontWeight: "bold", color: net >= 0 ? "primary.dark" : "warning.dark", mb: 0.25 }}
                            >
                                ${net.toFixed(3)}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                                Collected minus expenses
                            </Typography>
                        </CardContent>
                    </Card>
                </Grid>
            </Grid>

            {/* Tabs */}
            <Paper sx={{ mb: 3 }}>
                <Box
                    sx={{
                        borderBottom: 1,
                        borderColor: "divider",
                        display: "flex",
                        justifyContent: "space-between",
                        alignItems: "center",
                        px: 2,
                    }}
                >
                    <Tabs value={currentTab} onChange={handleTabChange}>
                        <Tab label={`Sale Orders (${saleOrders.length})`} />
                        <Tab label={`Expenses (${expenseTransactions.length})`} />
                    </Tabs>
                </Box>

                {/* Sale Orders Tab */}
                {currentTab === 0 && (
                    <Box sx={{ p: 3 }}>
                        <Box sx={{ mb: 3 }}>
                            <Button
                                variant="contained"
                                color="success"
                                startIcon={<AddIcon />}
                                onClick={() => setCreateSaleOrderOpen(true)}
                            >
                                New Sale Order
                            </Button>
                        </Box>
                        <SaleOrdersList
                            saleOrders={saleOrders}
                            onAddPayment={(order) => setAddPaymentOrder(order)}
                            onCancel={handleCancelOrder}
                        />
                    </Box>
                )}

                {/* Expense Tab */}
                {currentTab === 1 && (
                    <Box sx={{ p: 3 }}>
                        <Button
                            variant="contained"
                            color="error"
                            startIcon={<AddIcon />}
                            onClick={() => setCreateExpenseOpen(true)}
                            sx={{ mb: 3 }}
                        >
                            Add Expense
                        </Button>
                        <TransactionsTable transactions={expenseTransactions} />
                    </Box>
                )}
            </Paper>

            {/* Modals */}
            <CreateSaleOrderModal
                open={createSaleOrderOpen}
                onClose={() => setCreateSaleOrderOpen(false)}
                onSuccess={() => {
                    setCreateSaleOrderOpen(false);
                    loadData();
                }}
                batchId={id!}
            />

            <AddSaleOrderPaymentModal
                open={addPaymentOrder !== null}
                onClose={() => setAddPaymentOrder(null)}
                onSuccess={() => {
                    setAddPaymentOrder(null);
                    loadData();
                }}
                saleOrder={addPaymentOrder}
            />

            <CreateTransactionModal
                open={createExpenseOpen}
                onClose={() => setCreateExpenseOpen(false)}
                onSuccess={() => {
                    setCreateExpenseOpen(false);
                    loadData();
                }}
                batchId={id!}
                transactionType="Expense"
            />
        </Container>
    );
}
