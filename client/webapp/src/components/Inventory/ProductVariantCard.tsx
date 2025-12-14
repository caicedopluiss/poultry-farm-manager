import { Card, CardContent, CardActions, Typography, IconButton, Box, Tooltip, Chip } from "@mui/material";
import { Edit as EditIcon, Inventory as StockIcon } from "@mui/icons-material";
import type { ProductVariant, Product } from "@/types/inventory";

interface ProductVariantCardProps {
    variant: ProductVariant;
    products: Product[];
    onEdit: (variant: ProductVariant) => void;
}

export default function ProductVariantCard({ variant, products, onEdit }: ProductVariantCardProps) {
    const product = products.find((p) => p.id === variant.productId);
    const productName = product?.name || "Unknown Product";

    const getStockColor = () => {
        if (variant.stock <= 0) return "error";
        if (variant.stock < 10) return "warning";
        return "success";
    };

    return (
        <Card
            sx={{
                width: 280,
                height: "100%",
                display: "flex",
                flexDirection: "column",
                transition: "transform 0.2s, box-shadow 0.2s",
                "&:hover": {
                    transform: "translateY(-4px)",
                    boxShadow: 4,
                },
            }}
        >
            <CardContent sx={{ flexGrow: 1 }}>
                <Tooltip title={variant.name} arrow>
                    <Typography
                        variant="subtitle1"
                        component="div"
                        sx={{
                            fontWeight: "bold",
                            mb: 1,
                            overflow: "hidden",
                            textOverflow: "ellipsis",
                            whiteSpace: "nowrap",
                        }}
                    >
                        {variant.name}
                    </Typography>
                </Tooltip>

                <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                    Product: {productName}
                </Typography>

                {variant.description && (
                    <Typography
                        variant="body2"
                        color="text.secondary"
                        sx={{
                            mb: 2,
                            overflow: "hidden",
                            textOverflow: "ellipsis",
                            display: "-webkit-box",
                            WebkitLineClamp: 2,
                            WebkitBoxOrient: "vertical",
                        }}
                    >
                        {variant.description}
                    </Typography>
                )}

                <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 2 }}>
                    <StockIcon sx={{ fontSize: 20, color: "text.secondary" }} />
                    <Chip label={`${variant.stock} ${variant.unitOfMeasure}`} size="small" color={getStockColor()} />
                </Box>
            </CardContent>

            <CardActions sx={{ justifyContent: "flex-end", px: 2, pb: 2 }}>
                <Tooltip title="Edit Variant">
                    <IconButton size="small" color="primary" onClick={() => onEdit(variant)}>
                        <EditIcon />
                    </IconButton>
                </Tooltip>
            </CardActions>
        </Card>
    );
}
