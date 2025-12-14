import { Card, CardContent, CardActionArea, Box, Typography, Chip, useTheme } from "@mui/material";
import { Category as ProductIcon } from "@mui/icons-material";
import type { Product } from "@/types/inventory";

interface Props {
    product: Product;
    onClick?: (product: Product) => void;
}

export default function ProductCard({ product, onClick }: Props) {
    const theme = useTheme();

    const variantCount = product.variants?.length ?? 0;

    const handleClick = () => {
        onClick?.(product);
    };

    return (
        <Card
            sx={{
                height: "100%",
                minWidth: 250,
                maxWidth: 280,
                border: "1px solid",
                borderColor: theme.palette.divider,
                transition: "all 0.2s ease-in-out",
                "&:hover": {
                    transform: "translateY(-2px)",
                    boxShadow: theme.shadows[4],
                    borderColor: theme.palette.secondary.main,
                },
            }}
        >
            <CardActionArea onClick={handleClick} sx={{ height: "100%", p: 0 }}>
                <CardContent
                    sx={{
                        height: "100%",
                        display: "flex",
                        flexDirection: "column",
                        gap: 1.5,
                        p: 2,
                    }}
                >
                    {/* Header with Icon and Name */}
                    <Box sx={{ display: "flex", alignItems: "flex-start", gap: 1 }}>
                        <ProductIcon sx={{ color: theme.palette.secondary.main, fontSize: 24 }} />
                        <Box sx={{ flex: 1, minWidth: 0 }}>
                            <Typography
                                variant="h6"
                                sx={{
                                    fontWeight: 600,
                                    fontSize: "1.1rem",
                                    lineHeight: 1.3,
                                    overflow: "hidden",
                                    textOverflow: "ellipsis",
                                    display: "-webkit-box",
                                    WebkitLineClamp: 2,
                                    WebkitBoxOrient: "vertical",
                                }}
                            >
                                {product.name}
                            </Typography>
                        </Box>
                    </Box>

                    {/* Manufacturer */}
                    <Box sx={{ display: "flex", alignItems: "center", gap: 0.5 }}>
                        <Typography variant="caption" color="text.secondary">
                            Manufacturer:
                        </Typography>
                        <Typography variant="body2" fontWeight={500}>
                            {product.manufacturer}
                        </Typography>
                    </Box>

                    {/* Description */}
                    {product.description && (
                        <Typography
                            variant="body2"
                            color="text.secondary"
                            sx={{
                                overflow: "hidden",
                                textOverflow: "ellipsis",
                                display: "-webkit-box",
                                WebkitLineClamp: 2,
                                WebkitBoxOrient: "vertical",
                                minHeight: "2.5em",
                            }}
                        >
                            {product.description}
                        </Typography>
                    )}

                    {/* Stock and Unit */}
                    <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "flex-end", mt: "auto" }}>
                        <Box>
                            <Typography variant="caption" color="text.secondary" display="block">
                                Stock
                            </Typography>
                            <Typography variant="h6" fontWeight={600} color="secondary">
                                {product.stock}
                            </Typography>
                        </Box>
                        <Typography variant="body2" color="text.secondary" fontWeight={500}>
                            {product.unitOfMeasure}
                        </Typography>
                    </Box>

                    {/* Variants Count */}
                    {variantCount > 0 && (
                        <Chip
                            label={`${variantCount} variant${variantCount !== 1 ? "s" : ""}`}
                            size="small"
                            color="secondary"
                            variant="outlined"
                            sx={{ alignSelf: "flex-start" }}
                        />
                    )}
                </CardContent>
            </CardActionArea>
        </Card>
    );
}
