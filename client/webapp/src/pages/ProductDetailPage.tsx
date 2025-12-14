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
    useTheme,
    useMediaQuery,
} from "@mui/material";
import { ArrowBack, Edit as EditIcon, Category as ProductIcon, Add as AddIcon } from "@mui/icons-material";
import { getProductById, updateProduct } from "@/api/v1/products";
import { getProductVariantsByProductId, createProductVariant, updateProductVariant } from "@/api/v1/productVariants";
import type {
    Product,
    ProductVariant,
    NewProductVariant,
    UpdateProductVariant,
    UpdateProduct,
} from "@/types/inventory";
import ProductVariantTable from "@/components/ProductVariantTable";
import CreateProductVariantForm from "@/components/CreateProductVariantForm";
import ProductVariantDetailModal from "@/components/ProductVariantDetailModal";
import ProductDetailModal from "@/components/ProductDetailModal";

export default function ProductDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("sm"));

    const [product, setProduct] = useState<Product | null>(null);
    const [variants, setVariants] = useState<ProductVariant[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [createFormOpen, setCreateFormOpen] = useState(false);
    const [selectedVariant, setSelectedVariant] = useState<ProductVariant | null>(null);
    const [detailModalOpen, setDetailModalOpen] = useState(false);
    const [productModalOpen, setProductModalOpen] = useState(false);
    useEffect(() => {
        if (id) {
            loadProduct(id);
            loadVariants(id);
        }
    }, [id]);

    const loadProduct = async (productId: string) => {
        try {
            setLoading(true);
            setError(null);
            const response = await getProductById(productId);
            setProduct(response.product);
        } catch (err) {
            setError("Failed to load product details");
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const loadVariants = async (productId: string) => {
        try {
            const response = await getProductVariantsByProductId(productId);
            setVariants(response.productVariants || []);
        } catch (err) {
            console.error("Failed to load variants:", err);
        }
    };

    const handleCreateVariant = async (variantData: NewProductVariant) => {
        await createProductVariant(variantData);
        if (id) {
            await Promise.all([loadProduct(id), loadVariants(id)]);
        }
    };

    const handleRefresh = async () => {
        if (id) {
            await Promise.all([loadProduct(id), loadVariants(id)]);
        }
    };

    const handleBack = () => {
        navigate("/inventory");
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
            // Reload product and variants after update
            if (product?.id) {
                await Promise.all([loadProduct(product.id), loadVariants(product.id)]);
            }
            handleCloseDetailModal();
        } catch (err) {
            console.error("Failed to update variant:", err);
            throw err; // Let modal handle error display
        }
    };

    const handleEditProduct = () => {
        setProductModalOpen(true);
    };

    const handleCloseProductModal = () => {
        setProductModalOpen(false);
    };

    const handleUpdateProduct = async (id: string, data: UpdateProduct) => {
        try {
            await updateProduct(id, data);
            // Reload product after update
            if (product?.id) {
                await loadProduct(product.id);
            }
            handleCloseProductModal();
        } catch (err) {
            console.error("Failed to update product:", err);
            throw err;
        }
    };

    if (loading) {
        return (
            <Container maxWidth="lg" sx={{ py: 4, display: "flex", justifyContent: "center" }}>
                <CircularProgress color="secondary" />
            </Container>
        );
    }

    if (error || !product) {
        return (
            <Container maxWidth="lg" sx={{ py: 4 }}>
                <Alert severity="error" sx={{ mb: 2 }}>
                    {error || "Product not found"}
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
                <Button startIcon={<ArrowBack />} onClick={handleBack} variant="outlined" color="secondary">
                    Back
                </Button>
                <Box sx={{ flex: 1 }}>
                    <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 0.5 }}>
                        <ProductIcon color="secondary" />
                        <Typography variant={isMobile ? "h5" : "h4"} fontWeight={600}>
                            {product.name}
                        </Typography>
                    </Box>
                </Box>
                <Button startIcon={<EditIcon />} variant="contained" color="secondary" onClick={handleEditProduct}>
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
                            {product.name}
                        </Typography>
                    </Box>
                    <Box>
                        <Typography variant="caption" color="text.secondary">
                            Manufacturer
                        </Typography>
                        <Typography variant="body1" fontWeight={500}>
                            {product.manufacturer}
                        </Typography>
                    </Box>
                    <Box>
                        <Typography variant="caption" color="text.secondary">
                            Stock
                        </Typography>
                        <Typography variant="h5" fontWeight={600} color="secondary.main">
                            {product.stock} {product.unitOfMeasure}
                        </Typography>
                    </Box>
                    <Box>
                        <Typography variant="caption" color="text.secondary">
                            Unit of Measure
                        </Typography>
                        <Typography variant="body1" fontWeight={500}>
                            {product.unitOfMeasure}
                        </Typography>
                    </Box>
                    {product.description && (
                        <Box sx={{ gridColumn: { xs: "1", md: "1 / -1" } }}>
                            <Typography variant="caption" color="text.secondary">
                                Description
                            </Typography>
                            <Typography variant="body1">{product.description}</Typography>
                        </Box>
                    )}
                </Box>
            </Paper>

            {/* Product Variants */}
            <Paper sx={{ p: 3 }}>
                <Box sx={{ display: "flex", alignItems: "center", justifyContent: "space-between", mb: 2 }}>
                    <Typography variant="h6" fontWeight={600}>
                        Product Variants ({variants.length})
                    </Typography>
                    <Box sx={{ display: "flex", gap: 1 }}>
                        <Button variant="outlined" onClick={handleRefresh} size="small">
                            Refresh
                        </Button>
                        <Button
                            variant="contained"
                            color="secondary"
                            startIcon={<AddIcon />}
                            onClick={() => setCreateFormOpen(true)}
                            size="small"
                        >
                            Add Variant
                        </Button>
                    </Box>
                </Box>
                <ProductVariantTable variants={variants} onView={handleViewVariant} />
            </Paper>

            {/* Create Variant Form */}
            {id && (
                <CreateProductVariantForm
                    open={createFormOpen}
                    onClose={() => setCreateFormOpen(false)}
                    onSubmit={handleCreateVariant}
                    productId={id}
                />
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

            {/* Product Detail Modal */}
            {product && (
                <ProductDetailModal
                    open={productModalOpen}
                    onClose={handleCloseProductModal}
                    product={product}
                    onUpdate={handleUpdateProduct}
                />
            )}
        </Container>
    );
}
