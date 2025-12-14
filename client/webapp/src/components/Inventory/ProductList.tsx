import { Grid, Typography, Box } from "@mui/material";
import { Category as EmptyIcon } from "@mui/icons-material";
import type { Product } from "@/types/inventory";
import ProductCard from "./ProductCard";

interface ProductListProps {
    products: Product[];
    onEdit: (product: Product) => void;
}

export default function ProductList({ products, onEdit }: ProductListProps) {
    if (products.length === 0) {
        return (
            <Box
                sx={{
                    display: "flex",
                    flexDirection: "column",
                    alignItems: "center",
                    justifyContent: "center",
                    py: 8,
                    gap: 2,
                }}
            >
                <EmptyIcon sx={{ fontSize: 64, color: "text.secondary", opacity: 0.3 }} />
                <Typography variant="h6" color="text.secondary">
                    No products found
                </Typography>
                <Typography variant="body2" color="text.secondary">
                    Click "Add Product" to create your first product
                </Typography>
            </Box>
        );
    }

    return (
        <Grid container spacing={3}>
            {products.map((product) => (
                <Grid size={{ xs: 12, sm: 6, md: 4 }} key={product.id}>
                    <ProductCard product={product} onEdit={onEdit} />
                </Grid>
            ))}
        </Grid>
    );
}
