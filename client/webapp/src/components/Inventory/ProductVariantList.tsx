import { Grid, Typography, Box } from "@mui/material";
import { LocalOffer as EmptyIcon } from "@mui/icons-material";
import type { ProductVariant, Product } from "@/types/inventory";
import ProductVariantCard from "./ProductVariantCard";

interface ProductVariantListProps {
    productVariants: ProductVariant[];
    products: Product[];
    onEdit: (variant: ProductVariant) => void;
}

export default function ProductVariantList({ productVariants, products, onEdit }: ProductVariantListProps) {
    if (productVariants.length === 0) {
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
                    No product variants found
                </Typography>
                <Typography variant="body2" color="text.secondary">
                    Click "Add Variant" to create your first product variant
                </Typography>
            </Box>
        );
    }

    return (
        <Grid container spacing={3}>
            {productVariants.map((variant) => (
                <Grid size={{ xs: 12, sm: 6, md: 4 }} key={variant.id}>
                    <ProductVariantCard variant={variant} products={products} onEdit={onEdit} />
                </Grid>
            ))}
        </Grid>
    );
}
