import React, { useState, useMemo } from "react";
import {
    Box,
    Container,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    TableSortLabel,
    Typography,
    CircularProgress,
    Button,
    Chip,
    IconButton,
    Tooltip,
} from "@mui/material";
import { Refresh as RefreshIcon, Add as AddIcon, Visibility as ViewIcon } from "@mui/icons-material";
import type { ProductVariant, UpdateProductVariant } from "@/types/inventory";
import { updateProductVariant } from "@/api/v1/productVariants";
import ProductVariantDetailModal from "@/components/ProductVariantDetailModal";

interface ProductVariantsListProps {
    variants: ProductVariant[];
    loading: boolean;
    onRefresh: () => void;
    onCreate: () => void;
}

type Order = "asc" | "desc";
type OrderBy = keyof ProductVariant;

const ProductVariantsList: React.FC<ProductVariantsListProps> = ({ variants, loading, onRefresh, onCreate }) => {
    const [order, setOrder] = useState<Order>("asc");
    const [orderBy, setOrderBy] = useState<OrderBy>("name");
    const [selectedVariant, setSelectedVariant] = useState<ProductVariant | null>(null);
    const [detailModalOpen, setDetailModalOpen] = useState(false);

    const handleRequestSort = (property: OrderBy) => {
        const isAsc = orderBy === property && order === "asc";
        setOrder(isAsc ? "desc" : "asc");
        setOrderBy(property);
    };

    const handleViewVariant = (variant: ProductVariant) => {
        setSelectedVariant(variant);
        setDetailModalOpen(true);
    };

    const handleCloseDetailModal = () => {
        setDetailModalOpen(false);
        setSelectedVariant(null);
    };

    const handleUpdateVariant = async (id: string, data: UpdateProductVariant) => {
        try {
            await updateProductVariant(id, data);
            await onRefresh();
            handleCloseDetailModal();
        } catch (err) {
            console.error("Failed to update variant:", err);
            throw err;
        }
    };

    const sortedVariants = useMemo(() => {
        const comparator = (a: ProductVariant, b: ProductVariant) => {
            const aValue = a[orderBy];
            const bValue = b[orderBy];

            if (aValue === null && bValue === null) return 0;
            if (aValue === null) return 1;
            if (bValue === null) return -1;

            if (typeof aValue === "string" && typeof bValue === "string") {
                return order === "asc" ? aValue.localeCompare(bValue) : bValue.localeCompare(aValue);
            }

            if (typeof aValue === "number" && typeof bValue === "number") {
                return order === "asc" ? aValue - bValue : bValue - aValue;
            }

            return 0;
        };

        return [...variants].sort(comparator);
    }, [variants, order, orderBy]);

    if (loading) {
        return (
            <Container maxWidth="lg" sx={{ py: 4, display: "flex", justifyContent: "center" }}>
                <CircularProgress />
            </Container>
        );
    }

    return (
        <Container maxWidth="lg" sx={{ py: 4 }}>
            <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 3 }}>
                <Box>
                    <Typography variant="h4" fontWeight={600}>
                        Product Variants
                    </Typography>
                    <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                        {variants.length} {variants.length === 1 ? "variant" : "variants"} across all products
                    </Typography>
                </Box>
                <Box sx={{ display: "flex", gap: 1 }}>
                    <Button variant="outlined" startIcon={<RefreshIcon />} onClick={onRefresh}>
                        Refresh
                    </Button>
                    <Button variant="contained" color="secondary" startIcon={<AddIcon />} onClick={onCreate}>
                        Create Variant
                    </Button>
                </Box>
            </Box>

            {variants.length === 0 ? (
                <Paper sx={{ p: 4, textAlign: "center" }}>
                    <Typography variant="body1" color="text.secondary">
                        No product variants found. Create products with variants to see them here.
                    </Typography>
                </Paper>
            ) : (
                <TableContainer component={Paper} elevation={2}>
                    <Table>
                        <TableHead>
                            <TableRow sx={{ bgcolor: "secondary.main" }}>
                                <TableCell sx={{ color: "white" }}>
                                    <TableSortLabel
                                        active={orderBy === "name"}
                                        direction={orderBy === "name" ? order : "asc"}
                                        onClick={() => handleRequestSort("name")}
                                        sx={{
                                            color: "white !important",
                                            "&:hover": { color: "white !important" },
                                            "& .MuiTableSortLabel-icon": { color: "white !important" },
                                        }}
                                    >
                                        <Typography fontWeight="bold">Variant Name</Typography>
                                    </TableSortLabel>
                                </TableCell>
                                <TableCell sx={{ color: "white" }}>
                                    <Typography fontWeight="bold">Product</Typography>
                                </TableCell>
                                <TableCell sx={{ color: "white" }} align="right">
                                    <TableSortLabel
                                        active={orderBy === "quantity"}
                                        direction={orderBy === "quantity" ? order : "asc"}
                                        onClick={() => handleRequestSort("quantity")}
                                        sx={{
                                            color: "white !important",
                                            "&:hover": { color: "white !important" },
                                            "& .MuiTableSortLabel-icon": { color: "white !important" },
                                            flexDirection: "row-reverse",
                                        }}
                                    >
                                        <Typography fontWeight="bold">Quantity</Typography>
                                    </TableSortLabel>
                                </TableCell>
                                <TableCell sx={{ color: "white" }} align="right">
                                    <TableSortLabel
                                        active={orderBy === "stock"}
                                        direction={orderBy === "stock" ? order : "asc"}
                                        onClick={() => handleRequestSort("stock")}
                                        sx={{
                                            color: "white !important",
                                            "&:hover": { color: "white !important" },
                                            "& .MuiTableSortLabel-icon": { color: "white !important" },
                                            flexDirection: "row-reverse",
                                        }}
                                    >
                                        <Typography fontWeight="bold">Stock</Typography>
                                    </TableSortLabel>
                                </TableCell>
                                <TableCell sx={{ color: "white" }}>
                                    <TableSortLabel
                                        active={orderBy === "unitOfMeasure"}
                                        direction={orderBy === "unitOfMeasure" ? order : "asc"}
                                        onClick={() => handleRequestSort("unitOfMeasure")}
                                        sx={{
                                            color: "white !important",
                                            "&:hover": { color: "white !important" },
                                            "& .MuiTableSortLabel-icon": { color: "white !important" },
                                        }}
                                    >
                                        <Typography fontWeight="bold">Unit</Typography>
                                    </TableSortLabel>
                                </TableCell>
                                <TableCell sx={{ color: "white" }}>
                                    <Typography fontWeight="bold">Description</Typography>
                                </TableCell>
                                <TableCell sx={{ color: "white" }} align="center">
                                    <Typography fontWeight="bold">Actions</Typography>
                                </TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {sortedVariants.map((variant) => (
                                <TableRow
                                    key={variant.id}
                                    sx={{
                                        "&:hover": { bgcolor: "action.hover" },
                                        "&:last-child td, &:last-child th": { border: 0 },
                                    }}
                                >
                                    <TableCell>
                                        <Typography variant="body2" fontWeight={500}>
                                            {variant.name}
                                        </Typography>
                                    </TableCell>
                                    <TableCell>
                                        {variant.product ? (
                                            <Chip
                                                label={variant.product.name}
                                                size="small"
                                                color="secondary"
                                                variant="outlined"
                                            />
                                        ) : (
                                            <Typography variant="body2" color="text.secondary">
                                                -
                                            </Typography>
                                        )}
                                    </TableCell>
                                    <TableCell align="right">
                                        <Typography variant="body2">{variant.quantity}</Typography>
                                    </TableCell>
                                    <TableCell align="right">
                                        <Typography variant="body2" fontWeight={500}>
                                            {variant.stock}
                                        </Typography>
                                    </TableCell>
                                    <TableCell>
                                        <Typography variant="body2">{variant.unitOfMeasure}</Typography>
                                    </TableCell>
                                    <TableCell>
                                        <Typography variant="body2" color="text.secondary" sx={{ maxWidth: 300 }}>
                                            {variant.description || "-"}
                                        </Typography>
                                    </TableCell>
                                    <TableCell align="center">
                                        <Tooltip title="View details">
                                            <IconButton
                                                size="small"
                                                color="primary"
                                                onClick={() => handleViewVariant(variant)}
                                                aria-label={`View ${variant.name}`}
                                            >
                                                <ViewIcon fontSize="small" />
                                            </IconButton>
                                        </Tooltip>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>
            )}

            {/* Variant Detail Modal */}
            {selectedVariant && (
                <ProductVariantDetailModal
                    open={detailModalOpen}
                    onClose={handleCloseDetailModal}
                    variant={selectedVariant}
                    onUpdate={handleUpdateVariant}
                />
            )}
        </Container>
    );
};

export default ProductVariantsList;
