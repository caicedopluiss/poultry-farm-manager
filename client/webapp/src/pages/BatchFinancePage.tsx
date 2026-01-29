import { useParams, useNavigate } from "react-router-dom";
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
    Add as AddIcon,
} from "@mui/icons-material";
import TransactionsTable from "@/components/TransactionsTable";
import CreateTransactionModal from "@/components/CreateTransactionModal";
import useBatches from "@/hooks/useBatches";
import useTransactions from "@/hooks/useTransactions";
import type { Batch } from "@/types/batch";
import type { Transaction } from "@/types/transaction";

export default function BatchFinancePage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [batch, setBatch] = useState<Batch | null>(null);
    const [transactions, setTransactions] = useState<Transaction[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [currentTab, setCurrentTab] = useState(0);
    const [createModalOpen, setCreateModalOpen] = useState(false);
    const [transactionType, setTransactionType] = useState<"Income" | "Expense">("Expense");

    const { fetchBatchById } = useBatches();
    const { fetchBatchTransactions } = useTransactions();

    // Load batch and transactions
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

            const transactionsData = await fetchBatchTransactions(id);
            setTransactions(transactionsData);
        } catch (err) {
            setError("Failed to load batch finance data");
            console.error("Error loading batch finance:", err);
        } finally {
            setIsLoading(false);
        }
    }, [id, fetchBatchById, fetchBatchTransactions]);

    useEffect(() => {
        loadData();
    }, [loadData]);

    const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
        setCurrentTab(newValue);
    };

    const handleOpenCreateModal = (type: "Income" | "Expense") => {
        setTransactionType(type);
        setCreateModalOpen(true);
    };

    const handleTransactionCreated = () => {
        setCreateModalOpen(false);
        loadData();
    };

    // Filter transactions by type
    const incomeTransactions = transactions.filter((t) => t.type === "Income");
    const expenseTransactions = transactions.filter((t) => t.type === "Expense");

    // Calculate totals
    const totalIncome = incomeTransactions.reduce((sum, t) => sum + t.transactionAmount, 0);
    const totalExpense = expenseTransactions.reduce((sum, t) => sum + t.transactionAmount, 0);
    const netProfit = totalIncome - totalExpense;

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
            <Grid container spacing={3} sx={{ mb: 4 }}>
                <Grid size={{ xs: 12, md: 4 }}>
                    <Card sx={{ bgcolor: "success.50", borderLeft: 6, borderColor: "success.main" }}>
                        <CardContent>
                            <Box sx={{ display: "flex", alignItems: "center", mb: 1 }}>
                                <IncomeIcon sx={{ mr: 1, color: "success.main" }} />
                                <Typography variant="h6" color="success.dark">
                                    Total Income
                                </Typography>
                            </Box>
                            <Typography variant="h4" sx={{ fontWeight: "bold", color: "success.dark" }}>
                                ${totalIncome.toFixed(2)}
                            </Typography>
                            <Typography variant="body2" color="text.secondary">
                                {incomeTransactions.length} transaction(s)
                            </Typography>
                        </CardContent>
                    </Card>
                </Grid>

                <Grid size={{ xs: 12, md: 4 }}>
                    <Card sx={{ bgcolor: "error.50", borderLeft: 6, borderColor: "error.main" }}>
                        <CardContent>
                            <Box sx={{ display: "flex", alignItems: "center", mb: 1 }}>
                                <ExpenseIcon sx={{ mr: 1, color: "error.main" }} />
                                <Typography variant="h6" color="error.dark">
                                    Total Expense
                                </Typography>
                            </Box>
                            <Typography variant="h4" sx={{ fontWeight: "bold", color: "error.dark" }}>
                                ${totalExpense.toFixed(2)}
                            </Typography>
                            <Typography variant="body2" color="text.secondary">
                                {expenseTransactions.length} transaction(s)
                            </Typography>
                        </CardContent>
                    </Card>
                </Grid>

                <Grid size={{ xs: 12, md: 4 }}>
                    <Card
                        sx={{
                            bgcolor: netProfit >= 0 ? "primary.50" : "warning.50",
                            borderLeft: 6,
                            borderColor: netProfit >= 0 ? "primary.main" : "warning.main",
                        }}
                    >
                        <CardContent>
                            <Typography
                                variant="h6"
                                sx={{ mb: 1, color: netProfit >= 0 ? "primary.dark" : "warning.dark" }}
                            >
                                Net Profit/Loss
                            </Typography>
                            <Typography
                                variant="h4"
                                sx={{ fontWeight: "bold", color: netProfit >= 0 ? "primary.dark" : "warning.dark" }}
                            >
                                ${netProfit.toFixed(2)}
                            </Typography>
                            <Typography variant="body2" color="text.secondary">
                                {transactions.length} total transaction(s)
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
                        <Tab label={`All Transactions (${transactions.length})`} />
                        <Tab label={`Income (${incomeTransactions.length})`} />
                        <Tab label={`Expenses (${expenseTransactions.length})`} />
                    </Tabs>
                </Box>

                {/* All Transactions */}
                {currentTab === 0 && (
                    <Box sx={{ p: 3 }}>
                        <Box sx={{ display: "flex", gap: 2, mb: 3 }}>
                            <Button
                                variant="contained"
                                color="success"
                                startIcon={<AddIcon />}
                                onClick={() => handleOpenCreateModal("Income")}
                            >
                                Add Income
                            </Button>
                            <Button
                                variant="contained"
                                color="error"
                                startIcon={<AddIcon />}
                                onClick={() => handleOpenCreateModal("Expense")}
                            >
                                Add Expense
                            </Button>
                        </Box>
                        <TransactionsTable transactions={transactions} />
                    </Box>
                )}

                {/* Income Tab */}
                {currentTab === 1 && (
                    <Box sx={{ p: 3 }}>
                        <Button
                            variant="contained"
                            color="success"
                            startIcon={<AddIcon />}
                            onClick={() => handleOpenCreateModal("Income")}
                            sx={{ mb: 3 }}
                        >
                            Add Income
                        </Button>
                        <TransactionsTable transactions={incomeTransactions} />
                    </Box>
                )}

                {/* Expense Tab */}
                {currentTab === 2 && (
                    <Box sx={{ p: 3 }}>
                        <Button
                            variant="contained"
                            color="error"
                            startIcon={<AddIcon />}
                            onClick={() => handleOpenCreateModal("Expense")}
                            sx={{ mb: 3 }}
                        >
                            Add Expense
                        </Button>
                        <TransactionsTable transactions={expenseTransactions} />
                    </Box>
                )}
            </Paper>

            {/* Create Transaction Modal */}
            <CreateTransactionModal
                open={createModalOpen}
                onClose={() => setCreateModalOpen(false)}
                onSuccess={handleTransactionCreated}
                batchId={id!}
                transactionType={transactionType}
            />
        </Container>
    );
}
