import { Box, Container, Typography, useTheme, useMediaQuery, CircularProgress, Button } from "@mui/material";
import { Refresh as RefreshIcon, Add as AddIcon } from "@mui/icons-material";
import ProductCard from "@/components/ProductCard";
import type { Product } from "@/types/inventory";

interface Props {
    products?: Product[];
    loading?: boolean;
    onProductClick?: (product: Product) => void;
    onRefresh?: () => void;
    onCreateProduct?: () => void;
}

export default function ProductList({
    products = [],
    loading = false,
    onProductClick,
    onRefresh,
    onCreateProduct,
}: Props) {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("sm"));
    const isExtraSmall = useMediaQuery("(max-width:400px)");

    const handleProductClick = (product: Product) => {
        if (onProductClick) {
            onProductClick(product);
        }
    };

    return (
        <Container maxWidth="lg" sx={{ py: 3 }}>
            <Box
                sx={{
                    display: "flex",
                    flexDirection: { xs: "column", sm: "row" },
                    justifyContent: "space-between",
                    alignItems: { xs: "flex-start", sm: "center" },
                    gap: { xs: 2, sm: 0 },
                    mb: 4,
                }}
            >
                <Typography
                    variant={isMobile ? "h4" : "h3"}
                    component="h1"
                    sx={{
                        fontWeight: "bold",
                        color: theme.palette.secondary.main,
                        flexShrink: 0,
                    }}
                >
                    Products
                </Typography>

                <Box
                    sx={{
                        display: "flex",
                        flexDirection: { xs: "column", sm: "row" },
                        gap: { xs: 1.5, sm: 2 },
                        width: { xs: "100%", sm: "auto" },
                    }}
                >
                    {onCreateProduct && (
                        <Button
                            variant="contained"
                            color="secondary"
                            startIcon={<AddIcon />}
                            onClick={onCreateProduct}
                            disabled={loading}
                            size={isMobile ? "medium" : "medium"}
                            sx={{
                                minWidth: { xs: "100%", sm: "auto" },
                                whiteSpace: "nowrap",
                                px: { xs: 2, sm: 2 },
                            }}
                        >
                            {isExtraSmall ? "Create" : isMobile ? "Create Product" : "Create New Product"}
                        </Button>
                    )}
                    {onRefresh && (
                        <Button
                            variant="outlined"
                            color="secondary"
                            startIcon={<RefreshIcon />}
                            onClick={onRefresh}
                            disabled={loading}
                            size={isMobile ? "medium" : "medium"}
                            sx={{
                                minWidth: { xs: "100%", sm: "auto" },
                                whiteSpace: "nowrap",
                            }}
                        >
                            Refresh
                        </Button>
                    )}
                </Box>
            </Box>
            {loading ? (
                <Box
                    sx={{
                        display: "flex",
                        justifyContent: "center",
                        alignItems: "center",
                        py: 8,
                    }}
                >
                    <CircularProgress color="secondary" />
                </Box>
            ) : (
                <>
                    <Box
                        sx={{
                            display: "grid",
                            gridTemplateColumns: {
                                xs: "1fr",
                                sm: "repeat(2, 1fr)",
                                md: "repeat(3, 1fr)",
                                lg: "repeat(4, 1fr)",
                                xl: "repeat(5, 1fr)",
                            },
                            gap: 3,
                            mb: 4,
                            justifyItems: "center",
                            justifyContent: "center",
                        }}
                    >
                        {products.map((product) => (
                            <ProductCard key={product.id} product={product} onClick={handleProductClick} />
                        ))}
                    </Box>

                    {products.length === 0 && !loading && (
                        <Box
                            sx={{
                                textAlign: "center",
                                py: 8,
                                color: theme.palette.text.secondary,
                            }}
                        >
                            <Typography variant="h6" gutterBottom>
                                No products found
                            </Typography>
                            <Typography variant="body2">Start by creating your first product</Typography>
                        </Box>
                    )}
                </>
            )}
        </Container>
    );
}
