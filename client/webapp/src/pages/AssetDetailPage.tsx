import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
    Container,
    Box,
    Typography,
    Paper,
    Button,
    CircularProgress,
    Alert,
    Chip,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    useTheme,
    useMediaQuery,
} from "@mui/material";
import { ArrowBack, Edit as EditIcon, Inventory2 as AssetIcon } from "@mui/icons-material";
import { getAssetById, updateAsset } from "@/api/v1/assets";
import type { Asset, UpdateAsset } from "@/types/inventory";
import AssetDetailModal from "@/components/AssetDetailModal";

export default function AssetDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("sm"));

    const [asset, setAsset] = useState<Asset | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [assetModalOpen, setAssetModalOpen] = useState(false);

    useEffect(() => {
        if (id) {
            loadAsset(id);
        }
    }, [id]);

    const loadAsset = async (assetId: string) => {
        try {
            setLoading(true);
            setError(null);
            const response = await getAssetById(assetId);
            setAsset(response.asset);
        } catch (err) {
            setError("Failed to load asset details");
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const handleBack = () => {
        navigate("/inventory");
    };

    const handleEditAsset = () => {
        setAssetModalOpen(true);
    };

    const handleCloseAssetModal = () => {
        setAssetModalOpen(false);
    };

    const handleUpdateAsset = async (id: string, data: UpdateAsset) => {
        try {
            await updateAsset(id, data);
            // Reload asset after update
            if (asset?.id) {
                await loadAsset(asset.id);
            }
            handleCloseAssetModal();
        } catch (err) {
            console.error("Failed to update asset:", err);
            throw err;
        }
    };

    const totalQuantity = asset?.states?.reduce((sum, state) => sum + state.quantity, 0) ?? 0;

    if (loading) {
        return (
            <Container maxWidth="lg" sx={{ py: 4, display: "flex", justifyContent: "center" }}>
                <CircularProgress />
            </Container>
        );
    }

    if (error || !asset) {
        return (
            <Container maxWidth="lg" sx={{ py: 4 }}>
                <Alert severity="error" sx={{ mb: 2 }}>
                    {error || "Asset not found"}
                </Alert>
                <Button startIcon={<ArrowBack />} onClick={handleBack}>
                    Back to Inventory
                </Button>
            </Container>
        );
    }

    return (
        <Container maxWidth="lg" sx={{ py: 4 }}>
            {/* Header */}
            <Box sx={{ display: "flex", alignItems: "center", gap: 2, mb: 3 }}>
                <Button startIcon={<ArrowBack />} onClick={handleBack} variant="outlined">
                    Back
                </Button>
                <Box sx={{ flex: 1 }}>
                    <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 0.5 }}>
                        <AssetIcon color="primary" />
                        <Typography variant={isMobile ? "h5" : "h4"} fontWeight={600}>
                            {asset.name}
                        </Typography>
                    </Box>
                </Box>
                <Button startIcon={<EditIcon />} variant="contained" onClick={handleEditAsset}>
                    Edit
                </Button>
            </Box>

            {/* Basic Information */}
            <Paper sx={{ p: 3, mb: 3 }}>
                <Typography variant="h6" gutterBottom fontWeight={600}>
                    Basic Information
                </Typography>
                <Box sx={{ display: "grid", gridTemplateColumns: { xs: "1fr", md: "1fr 1fr" }, gap: 2, mt: 2 }}>
                    <Box>
                        <Typography variant="caption" color="text.secondary">
                            Name
                        </Typography>
                        <Typography variant="body1" fontWeight={500}>
                            {asset.name}
                        </Typography>
                    </Box>
                    {asset.description && (
                        <Box sx={{ gridColumn: { xs: "1", md: "1 / -1" } }}>
                            <Typography variant="caption" color="text.secondary">
                                Description
                            </Typography>
                            <Typography variant="body1">{asset.description}</Typography>
                        </Box>
                    )}
                    {asset.notes && (
                        <Box sx={{ gridColumn: { xs: "1", md: "1 / -1" } }}>
                            <Typography variant="caption" color="text.secondary">
                                Notes
                            </Typography>
                            <Typography variant="body1">{asset.notes}</Typography>
                        </Box>
                    )}
                </Box>
            </Paper>

            {/* Quantity Summary */}
            <Paper sx={{ p: 3, mb: 3 }}>
                <Typography variant="h6" gutterBottom fontWeight={600}>
                    Quantity Summary
                </Typography>
                <Box sx={{ display: "flex", gap: 4, mt: 2 }}>
                    <Box>
                        <Typography variant="caption" color="text.secondary">
                            Total Quantity
                        </Typography>
                        <Typography variant="h4" fontWeight={600} color="primary">
                            {totalQuantity}
                        </Typography>
                    </Box>
                    <Box>
                        <Typography variant="caption" color="text.secondary">
                            States
                        </Typography>
                        <Typography variant="h4" fontWeight={600}>
                            {asset.states?.length ?? 0}
                        </Typography>
                    </Box>
                </Box>
            </Paper>

            {/* Asset States */}
            <Paper sx={{ p: 3 }}>
                <Typography variant="h6" gutterBottom fontWeight={600}>
                    Asset States
                </Typography>
                {asset.states && asset.states.length > 0 ? (
                    <TableContainer>
                        <Table>
                            <TableHead>
                                <TableRow>
                                    <TableCell>
                                        <strong>Status</strong>
                                    </TableCell>
                                    <TableCell>
                                        <strong>Location</strong>
                                    </TableCell>
                                    <TableCell align="right">
                                        <strong>Quantity</strong>
                                    </TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {asset.states.map((state) => (
                                    <TableRow key={state.id}>
                                        <TableCell>
                                            <Chip
                                                label={state.status}
                                                size="small"
                                                color="primary"
                                                variant="outlined"
                                            />
                                        </TableCell>
                                        <TableCell>
                                            <Typography variant="body2" color="text.secondary">
                                                {state.location || "—"}
                                            </Typography>
                                        </TableCell>
                                        <TableCell align="right">
                                            <Typography variant="body1" fontWeight={600}>
                                                {state.quantity}
                                            </Typography>
                                        </TableCell>
                                    </TableRow>
                                ))}
                            </TableBody>
                        </Table>
                    </TableContainer>
                ) : (
                    <Alert severity="info" sx={{ mt: 2 }}>
                        No states recorded for this asset
                    </Alert>
                )}
            </Paper>

            {/* Asset Detail Modal */}
            {asset && (
                <AssetDetailModal
                    open={assetModalOpen}
                    onClose={handleCloseAssetModal}
                    asset={asset}
                    onUpdate={handleUpdateAsset}
                />
            )}
        </Container>
    );
}
