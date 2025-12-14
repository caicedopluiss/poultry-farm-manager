import { Card, CardContent, CardActions, Typography, IconButton, Box, Tooltip, Chip } from "@mui/material";
import { Edit as EditIcon } from "@mui/icons-material";
import type { Product } from "@/types/inventory";

interface ProductCardProps {
    product: Product;
    onEdit: (product: Product) => void;
}

export default function ProductCard({ product, onEdit }: ProductCardProps) {
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
                <Tooltip title={product.name} arrow>
                    <Typography
                        variant="subtitle1"
                        component="div"
                        sx={{
                            fontWeight: "bold",
                            mb: 2,
                            overflow: "hidden",
                            textOverflow: "ellipsis",
                            whiteSpace: "nowrap",
                        }}
                    >
                        {product.name}
                    </Typography>
                </Tooltip>

                {product.description && (
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
                        {product.description}
                    </Typography>
                )}

                <Box sx={{ display: "flex", flexWrap: "wrap", gap: 1, mb: 2 }}>
                    <Chip label={product.manufacturer} size="small" variant="outlined" />
                    <Chip label={product.unitOfMeasure} size="small" color="primary" variant="outlined" />
                    <Chip label={`Stock: ${product.stock}`} size="small" color="secondary" variant="outlined" />
                </Box>
            </CardContent>

            <CardActions sx={{ justifyContent: "flex-end", px: 2, pb: 2 }}>
                <Tooltip title="Edit Product">
                    <IconButton size="small" color="primary" onClick={() => onEdit(product)}>
                        <EditIcon />
                    </IconButton>
                </Tooltip>
            </CardActions>
        </Card>
    );
}
