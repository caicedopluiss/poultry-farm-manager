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
    TrendingDown as ExpenseIcon,
    ShoppingCart as PurchaseIcon,
    Store as VendorIcon,
    Add as AddIcon,
} from "@mui/icons-material";
import TransactionsTable from "@/components/TransactionsTable";
import CreateAssetTransactionModal from "@/components/CreateAssetTransactionModal";
import { getAssetById, getAssetTransactions, getAssetPricingByVendor } from "@/api/v1/assets";
import type { Asset } from "@/types/inventory";
import type { Transaction } from "@/types/transaction";

interface VendorPricing {
    vendor: {
        id: string;
        name: string;
        location?: string | null;
        contactPerson?: {
            id: string;
            firstName: string;
            lastName: string;
            email?: string | null;
            phoneNumber?: string | null;
            location?: string | null;
        } | null;
    };
    lastUnitPrice: number;
    lastPurchaseDate: string;
    totalPurchases: number;
}

export default function AssetFinancePage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [asset, setAsset] = useState<Asset | null>(null);
    const [transactions, setTransactions] = useState<Transaction[]>([]);
    const [vendorPricings, setVendorPricings] = useState<VendorPricing[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [currentTab, setCurrentTab] = useState(0);
    const [createModalOpen, setCreateModalOpen] = useState(false);

    // Load asset, transactions, and vendor pricing
    const loadData = useCallback(async () => {
        if (!id) {
            setError("No asset ID provided");
            return;
        }

        try {
            setIsLoading(true);
            setError(null);

            const { asset: assetData } = await getAssetById(id);
            setAsset(assetData);

            if (!assetData) {
                setError("Asset not found");
                return;
            }

            const { transactions: transactionsData } = await getAssetTransactions(id);
            setTransactions(transactionsData);

            const { vendorPricings: pricingData } = await getAssetPricingByVendor(id);
            setVendorPricings(pricingData);
        } catch (err) {
            setError("Failed to load asset finance data");
            console.error("Error loading asset finance:", err);
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

    const handleTransactionCreated = () => {
        setCreateModalOpen(false);
        loadData();
    };

    // Calculate totals
    const totalExpense = transactions.reduce((sum, t) => sum + t.transactionAmount, 0);
    const totalPurchases = transactions.length;
    const averagePrice =
        totalPurchases > 0 ? totalExpense / transactions.reduce((sum, t) => sum + (t.quantity || 0), 0) : 0;

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
    if (error || !asset) {
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
                    {error || "Asset not found"}
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
                {asset.name} - Purchase History & Pricing
            </Typography>

            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                Track asset purchases and vendor pricing
            </Typography>

            {/* Summary Cards */}
            <Box sx={{ display: "flex", gap: 3, mb: 4, flexWrap: "wrap" }}>
                <Box sx={{ flex: "1 1 300px", minWidth: 250 }}>
                    <Card>
                        <CardContent>
                            <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
                                <ExpenseIcon color="error" />
                                <Typography variant="body2" color="text.secondary">
                                    Total Spent
                                </Typography>
                            </Box>
                            <Typography variant="h5" sx={{ fontWeight: "bold" }}>
                                ${totalExpense.toFixed(2)}
                            </Typography>
                            <Typography variant="caption" color="text.secondary">
                                All-time purchases
                            </Typography>
                        </CardContent>
                    </Card>
                </Box>

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
            <Box sx={{ borderBottom: 1, borderColor: "divider", mb: 3 }}>
                <Tabs value={currentTab} onChange={handleTabChange}>
                    <Tab label="Purchase History" />
                    <Tab label="Vendor Pricing Comparison" />
                </Tabs>
            </Box>

            {/* Tab Panels */}
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
                            onClick={() => setCreateModalOpen(true)}
                        >
                            Add Purchase
                        </Button>
                    </Box>
                    {transactions.length === 0 ? (
                        <Alert severity="info">No purchase history found for this asset.</Alert>
                    ) : (
                        <TransactionsTable transactions={transactions} />
                    )}
                </Box>
            )}

            {currentTab === 1 && (
                <TableContainer component={Paper}>
                    <Table>
                        <TableHead>
                            <TableRow>
                                <TableCell sx={{ fontWeight: "bold" }}>Vendor</TableCell>
                                <TableCell sx={{ fontWeight: "bold" }}>Contact Person</TableCell>
                                <TableCell sx={{ fontWeight: "bold" }}>Contact Info</TableCell>
                                <TableCell sx={{ fontWeight: "bold" }} align="right">
                                    Last Unit Price
                                </TableCell>
                                <TableCell sx={{ fontWeight: "bold" }}>Last Purchase</TableCell>
                                <TableCell sx={{ fontWeight: "bold" }} align="center">
                                    Total Purchases
                                </TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {vendorPricings.map((vp) => (
                                <TableRow key={vp.vendor.id} hover>
                                    <TableCell>{vp.vendor.name}</TableCell>
                                    <TableCell>
                                        {vp.vendor.contactPerson ? (
                                            `${vp.vendor.contactPerson.firstName} ${vp.vendor.contactPerson.lastName}`
                                        ) : (
                                            <Typography variant="body2" color="text.secondary">
                                                N/A
                                            </Typography>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        <Box>
                                            {vp.vendor.contactPerson?.email && (
                                                <Typography variant="body2">{vp.vendor.contactPerson.email}</Typography>
                                            )}
                                            {vp.vendor.contactPerson?.phoneNumber && (
                                                <Typography variant="body2" color="text.secondary">
                                                    {vp.vendor.contactPerson.phoneNumber}
                                                </Typography>
                                            )}
                                        </Box>
                                    </TableCell>
                                    <TableCell align="right">
                                        <Chip label={`$${vp.lastUnitPrice.toFixed(2)}`} color="primary" size="small" />
                                    </TableCell>
                                    <TableCell>
                                        <Typography variant="body2">
                                            {new Date(vp.lastPurchaseDate).toLocaleDateString()}
                                        </Typography>
                                    </TableCell>
                                    <TableCell align="center">
                                        <Chip label={vp.totalPurchases} variant="outlined" size="small" />
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                    {vendorPricings.length === 0 && (
                        <Alert severity="info" sx={{ m: 2 }}>
                            No vendor pricing data available for this asset.
                        </Alert>
                    )}
                </TableContainer>
            )}

            {/* Create Transaction Modal */}
            {asset && (
                <CreateAssetTransactionModal
                    open={createModalOpen}
                    onClose={() => setCreateModalOpen(false)}
                    onSuccess={handleTransactionCreated}
                    assetId={asset.id}
                    assetName={asset.name}
                />
            )}
        </Container>
    );
}
