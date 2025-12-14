import React from "react";
import {
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Paper,
    IconButton,
    Typography,
    Box,
    Tooltip,
} from "@mui/material";
import { Visibility as ViewIcon, Delete as DeleteIcon } from "@mui/icons-material";
import type { ProductVariant } from "@/types/inventory";

interface ProductVariantTableProps {
    variants: ProductVariant[];
    onView?: (variant: ProductVariant) => void;
    onDelete?: (variantId: string) => void;
}

const ProductVariantTable: React.FC<ProductVariantTableProps> = ({ variants, onView, onDelete }) => {
    if (variants.length === 0) {
        return (
            <Box sx={{ textAlign: "center", py: 4 }}>
                <Typography variant="body1" color="text.secondary">
                    No variants found. Create your first variant to get started.
                </Typography>
            </Box>
        );
    }

    return (
        <TableContainer component={Paper} elevation={2}>
            <Table>
                <TableHead>
                    <TableRow sx={{ bgcolor: "primary.main" }}>
                        <TableCell sx={{ color: "white", fontWeight: "bold" }}>Name</TableCell>
                        <TableCell sx={{ color: "white", fontWeight: "bold" }} align="right">
                            Quantity
                        </TableCell>
                        <TableCell sx={{ color: "white", fontWeight: "bold" }} align="right">
                            Stock
                        </TableCell>
                        <TableCell sx={{ color: "white", fontWeight: "bold" }}>Unit</TableCell>
                        <TableCell sx={{ color: "white", fontWeight: "bold" }}>Description</TableCell>
                        <TableCell sx={{ color: "white", fontWeight: "bold" }} align="center">
                            Actions
                        </TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {variants.map((variant) => (
                        <TableRow
                            key={variant.id}
                            sx={{
                                "&:hover": { bgcolor: "action.hover" },
                                "&:last-child td, &:last-child th": { border: 0 },
                            }}
                        >
                            <TableCell component="th" scope="row">
                                <Typography variant="body2" fontWeight={500}>
                                    {variant.name}
                                </Typography>
                            </TableCell>
                            <TableCell align="right">
                                <Typography variant="body2">{variant.quantity}</Typography>
                            </TableCell>
                            <TableCell align="right">
                                <Typography variant="body2">{variant.stock}</Typography>
                            </TableCell>
                            <TableCell>
                                <Typography variant="body2">{variant.unitOfMeasure}</Typography>
                            </TableCell>
                            <TableCell>
                                <Typography variant="body2" color="text.secondary">
                                    {variant.description || "-"}
                                </Typography>
                            </TableCell>
                            <TableCell align="center">
                                <Box sx={{ display: "flex", gap: 0.5, justifyContent: "center" }}>
                                    {onView && (
                                        <Tooltip title="View details">
                                            <IconButton
                                                size="small"
                                                color="primary"
                                                onClick={() => onView(variant)}
                                                aria-label={`View ${variant.name}`}
                                            >
                                                <ViewIcon fontSize="small" />
                                            </IconButton>
                                        </Tooltip>
                                    )}
                                    {onDelete && (
                                        <Tooltip title="Delete variant">
                                            <IconButton
                                                size="small"
                                                color="error"
                                                onClick={() => onDelete(variant.id)}
                                                aria-label={`Delete ${variant.name}`}
                                            >
                                                <DeleteIcon fontSize="small" />
                                            </IconButton>
                                        </Tooltip>
                                    )}
                                </Box>
                            </TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
        </TableContainer>
    );
};

export default ProductVariantTable;
