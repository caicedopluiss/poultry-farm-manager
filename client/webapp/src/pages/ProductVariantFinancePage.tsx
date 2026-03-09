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
    Tabs,
    Tab,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Chip,
} from "@mui/material";
import {
    ArrowBack as BackIcon,
    ShoppingCart as PurchaseIcon,
    Store as VendorIcon,
    Add as AddIcon,
} from "@mui/icons-material";
import TransactionsTable from "@/components/TransactionsTable";
import CreateProductVariantTransactionModal from "@/components/CreateProductVariantTransactionModal";
import {
    getProductVariantById,
    getProductVariantTransactions,
    getProductVariantPricingByVendor,
    type VendorPricing,
} from "@/api/v1/productVariants";
import type { ProductVariant } from "@/types/inventory";
import type { Transaction } from "@/types/transaction";

export default function ProductVariantFinancePage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [variant, setVariant] = useState<ProductVariant | null>(null);
    const [transactions, setTransactions] = useState<Transaction[]>([]);
    const [vendorPricings, setVendorPricings] = useState<VendorPricing[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [currentTab, setCurrentTab] = useState(0);
    const [createModalOpen, setCreateModalOpen] = useState(false);

    // Load variant, transactions, and vendor pricing
    const loadData = useCallback(async () => {
        if (!id) {
            setError("No product variant ID provided");
            return;
        }

        try {
            setIsLoading(true);
            setError(null);

            const { productVariant } = await getProductVariantById(id);
            setVariant(productVariant);

            if (!productVariant) {
                setError("Product variant not found");
                return;
            }

            const { transactions: transactionsData } = await getProductVariantTransactions(id);
            setTransactions(transactionsData);

            const { vendorPricings: pricingData } = await getProductVariantPricingByVendor(id);
            setVendorPricings(pricingData);
        } catch (err) {
            setError("Failed to load product variant finance data");
            console.error("Error loading variant finance:", err);
        } finally {
            setIsLoading(false);
        }
    }, [id]);

    useEffect(() => {
        loadData();
    }, [loadData]);

    const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
        setCurrentTab(newValue);
    };

    const handleOpenCreateModal = () => {
        setCreateModalOpen(true);
    };

    const handleTransactionCreated = () => {
        setCreateModalOpen(false);
        loadData();
    };

    // Calculate totals
    const totalPurchases = transactions.length;
    const averagePrice =
        totalPurchases > 0 ? transactions.reduce((sum, t) => sum + t.unitPrice, 0) / totalPurchases : 0;

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
    if (error || !variant) {
        return (
            <Container maxWidth="lg" sx={{ py: 3 }}>
                <Button
                    variant="outlined"
                    startIcon={<BackIcon />}
                    onClick={() => navigate("/inventory")}
                    sx={{ mb: 2 }}
                >
                    Back to Inventory
                </Button>

                <Alert severity="error" sx={{ mb: 3 }}>
                    {error || "Product variant not found"}
                </Alert>

                <Button variant="contained" onClick={() => navigate("/inventory")}>
                    Back to Inventory
                </Button>
            </Container>
        );
    }

    return (
        <Container maxWidth="lg" sx={{ py: 3 }}>
            <Button variant="outlined" startIcon={<BackIcon />} onClick={() => navigate("/inventory")} sx={{ mb: 3 }}>
                Back to Inventory
            </Button>

            <Typography variant="h4" gutterBottom sx={{ fontWeight: "bold" }}>
                {variant.name} - Purchase History & Pricing
            </Typography>

            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                {variant.product?.name || "Product"} - Track purchases and vendor pricing
            </Typography>

            {/* Summary Cards */}
            <Box sx={{ display: "flex", gap: 3, mb: 4, flexWrap: "wrap" }}>
                <Box sx={{ flex: "1 1 300px", minWidth: 250 }}>
                    <Card>
                        <CardContent>
                            <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
                                <PurchaseIcon color="primary" />
                                <Typography variant="body2" color="text.secondary">
                                    Total Purchases
                                </Typography>
                            </Box>
                            <Typography variant="h5" sx={{ fontWeight: "bold" }}>
                                {totalPurchases}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                                Purchase transactions
                            </Typography>
                        </CardContent>
                    </Card>
                </Box>

                <Box sx={{ flex: "1 1 300px", minWidth: 250 }}>
                    <Card>
                        <CardContent>
                            <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
                                <VendorIcon color="success" />
                                <Typography variant="body2" color="text.secondary">
                                    Average Unit Price
                                </Typography>
                            </Box>
                            <Typography variant="h5" sx={{ fontWeight: "bold" }}>
                                ${averagePrice.toFixed(2)}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                                Per unit across all purchases
                            </Typography>
                        </CardContent>
                    </Card>
                </Box>
            </Box>

            {/* Tabs */}
            <Paper sx={{ mb: 3 }}>
                <Tabs value={currentTab} onChange={handleTabChange}>
                    <Tab label="Purchase History" />
                    <Tab label={`Vendor Pricing (${vendorPricings.length})`} />
                </Tabs>
            </Paper>

            {/* Tab Content */}
            <Box sx={{ mt: 3 }}>
                {currentTab === 0 && (
                    <Box>
                        <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 2 }}>
                            <Typography variant="h6" sx={{ fontWeight: 600 }}>
                                Purchase History
                            </Typography>
                            <Button
                                variant="contained"
                                color="primary"
                                startIcon={<AddIcon />}
                                onClick={handleOpenCreateModal}
                            >
                                Add Purchase
                            </Button>
                        </Box>
                        {transactions.length === 0 ? (
                            <Alert severity="info">No purchase transactions found for this product variant.</Alert>
                        ) : (
                            <TransactionsTable transactions={transactions} />
                        )}
                    </Box>
                )}

                {currentTab === 1 && (
                    <Box>
                        <Typography variant="h6" gutterBottom sx={{ fontWeight: 600, mb: 2 }}>
                            Vendor Pricing Comparison
                        </Typography>
                        {vendorPricings.length === 0 ? (
                            <Alert severity="info">No vendor pricing data available yet.</Alert>
                        ) : (
                            <TableContainer component={Paper}>
                                <Table>
                                    <TableHead>
                                        <TableRow>
                                            <TableCell sx={{ fontWeight: "bold" }}>Vendor</TableCell>
                                            <TableCell sx={{ fontWeight: "bold" }}>Contact</TableCell>
                                            <TableCell sx={{ fontWeight: "bold" }}>Last Price</TableCell>
                                            <TableCell sx={{ fontWeight: "bold" }}>Last Purchase</TableCell>
                                            <TableCell sx={{ fontWeight: "bold" }}>Total Purchases</TableCell>
                                        </TableRow>
                                    </TableHead>
                                    <TableBody>
                                        {vendorPricings.map((pricing) => (
                                            <TableRow key={pricing.vendor.id}>
                                                <TableCell>
                                                    <Typography variant="body2" sx={{ fontWeight: 500 }}>
                                                        {pricing.vendor.name}
                                                    </Typography>
                                                    {pricing.vendor.location && (
                                                        <Typography variant="caption" color="text.secondary">
                                                            {pricing.vendor.location}
                                                        </Typography>
                                                    )}
                                                </TableCell>
                                                <TableCell>
                                                    {pricing.vendor.contactPerson && (
                                                        <Box>
                                                            <Typography variant="body2">
                                                                {pricing.vendor.contactPerson.firstName}{" "}
                                                                {pricing.vendor.contactPerson.lastName}
                                                            </Typography>
                                                            {pricing.vendor.contactPerson.email && (
                                                                <Typography variant="caption" color="text.secondary">
                                                                    {pricing.vendor.contactPerson.email}
                                                                </Typography>
                                                            )}
                                                        </Box>
                                                    )}
                                                </TableCell>
                                                <TableCell>
                                                    <Chip
                                                        label={`$${pricing.lastUnitPrice.toFixed(2)}`}
                                                        color="primary"
                                                        size="small"
                                                    />
                                                </TableCell>
                                                <TableCell>
                                                    <Typography variant="body2">
                                                        {new Date(pricing.lastPurchaseDate).toLocaleDateString()}
                                                    </Typography>
                                                </TableCell>
                                                <TableCell>
                                                    <Typography variant="body2" sx={{ fontWeight: 500 }}>
                                                        {pricing.totalPurchases}
                                                    </Typography>
                                                </TableCell>
                                            </TableRow>
                                        ))}
                                    </TableBody>
                                </Table>
                            </TableContainer>
                        )}
                    </Box>
                )}
            </Box>

            {/* Create Transaction Modal */}
            {variant && (
                <CreateProductVariantTransactionModal
                    open={createModalOpen}
                    onClose={() => setCreateModalOpen(false)}
                    onSuccess={handleTransactionCreated}
                    productVariantId={id!}
                    productVariantName={variant.name}
                />
            )}
        </Container>
    );
}
