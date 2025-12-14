import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Box, Container, Tabs, Tab } from "@mui/material";
import { Inventory2 as AssetIcon, Category as ProductIcon, ListAlt as VariantIcon } from "@mui/icons-material";
import AssetList from "@/components/AssetList";
import ProductList from "@/components/ProductList";
import ProductVariantsList from "@/components/ProductVariantsList";
import CreateAssetForm from "@/components/CreateAssetForm";
import CreateProductForm from "@/components/CreateProductForm";
import CreateProductVariantForm from "@/components/CreateProductVariantForm";
import { getAssets, createAsset } from "@/api/v1/assets";
import { getProducts, createProduct } from "@/api/v1/products";
import { getProductVariants, createProductVariant } from "@/api/v1/productVariants";
import type { Asset, Product, ProductVariant, NewAsset, NewProduct, NewProductVariant } from "@/types/inventory";

export default function InventoryPage() {
    const navigate = useNavigate();

    const [currentTab, setCurrentTab] = useState(0);

    // Assets state
    const [assets, setAssets] = useState<Asset[]>([]);
    const [assetsLoading, setAssetsLoading] = useState(false);
    const [createAssetOpen, setCreateAssetOpen] = useState(false);
    const [createAssetLoading, setCreateAssetLoading] = useState(false);
    const [createAssetError, setCreateAssetError] = useState<string | null>(null);

    // Products state
    const [products, setProducts] = useState<Product[]>([]);
    const [productsLoading, setProductsLoading] = useState(false);
    const [createProductOpen, setCreateProductOpen] = useState(false);
    const [createProductLoading, setCreateProductLoading] = useState(false);
    const [createProductError, setCreateProductError] = useState<string | null>(null);

    // Product Variants state
    const [variants, setVariants] = useState<ProductVariant[]>([]);
    const [variantsLoading, setVariantsLoading] = useState(false);
    const [createVariantOpen, setCreateVariantOpen] = useState(false);

    // Load assets on mount
    useEffect(() => {
        loadAssets();
    }, []);

    // Load products when switching to products tab
    useEffect(() => {
        if (currentTab === 1 && products.length === 0) {
            loadProducts();
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [currentTab]);

    // Load variants when switching to variants tab
    useEffect(() => {
        if (currentTab === 2 && variants.length === 0) {
            loadVariants();
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [currentTab]);

    const loadAssets = async () => {
        try {
            setAssetsLoading(true);
            const response = await getAssets();
            setAssets(response.assets);
        } catch (error) {
            console.error("Failed to load assets", error);
        } finally {
            setAssetsLoading(false);
        }
    };

    const loadProducts = async () => {
        try {
            setProductsLoading(true);
            const response = await getProducts();
            setProducts(response.products);
        } catch (error) {
            console.error("Failed to load products", error);
        } finally {
            setProductsLoading(false);
        }
    };

    const loadVariants = async () => {
        try {
            setVariantsLoading(true);
            const response = await getProductVariants();
            setVariants(response.productVariants);
        } catch (error) {
            console.error("Failed to load product variants", error);
        } finally {
            setVariantsLoading(false);
        }
    };

    const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
        setCurrentTab(newValue);
    };

    const handleAssetClick = (asset: Asset) => {
        navigate(`/inventory/assets/${asset.id}`);
    };

    const handleProductClick = (product: Product) => {
        navigate(`/inventory/products/${product.id}`);
    };

    const handleCreateAsset = () => {
        setCreateAssetOpen(true);
        setCreateAssetError(null);
    };

    const handleCreateProduct = () => {
        setCreateProductOpen(true);
        setCreateProductError(null);
    };

    const handleSubmitAsset = async (assetData: NewAsset) => {
        try {
            setCreateAssetLoading(true);
            setCreateAssetError(null);
            await createAsset(assetData);
            setCreateAssetOpen(false);
            loadAssets(); // Refresh the list
        } catch (error) {
            setCreateAssetError("Failed to create asset. Please try again.");
            console.error(error);
        } finally {
            setCreateAssetLoading(false);
        }
    };

    const handleSubmitProduct = async (productData: NewProduct) => {
        try {
            setCreateProductLoading(true);
            setCreateProductError(null);
            await createProduct(productData);
            setCreateProductOpen(false);
            loadProducts(); // Refresh the list
        } catch (error) {
            setCreateProductError("Failed to create product. Please try again.");
            console.error(error);
        } finally {
            setCreateProductLoading(false);
        }
    };

    const handleCreateVariant = () => {
        setCreateVariantOpen(true);
    };

    const handleSubmitVariant = async (variantData: NewProductVariant) => {
        try {
            await createProductVariant(variantData);
            setCreateVariantOpen(false);
            loadVariants(); // Refresh the list
        } catch (error) {
            console.error("Failed to create variant:", error);
            throw error;
        }
    };

    return (
        <Box sx={{ width: "100%", bgcolor: "background.default", minHeight: "100vh" }}>
            {/* Tabs */}
            <Box
                sx={{
                    bgcolor: "background.paper",
                    borderBottom: 1,
                    borderColor: "divider",
                    position: "sticky",
                    top: 0,
                    zIndex: 10,
                }}
            >
                <Container maxWidth="lg">
                    <Tabs
                        value={currentTab}
                        onChange={handleTabChange}
                        aria-label="inventory tabs"
                        sx={{
                            "& .MuiTab-root": {
                                minHeight: 64,
                                fontSize: "1rem",
                                fontWeight: 500,
                            },
                        }}
                    >
                        <Tab icon={<AssetIcon />} iconPosition="start" label="Assets" />
                        <Tab icon={<ProductIcon />} iconPosition="start" label="Products" />
                        <Tab icon={<VariantIcon />} iconPosition="start" label="Product Variants" />
                    </Tabs>
                </Container>
            </Box>

            {/* Tab Panels */}
            {currentTab === 0 && (
                <AssetList
                    assets={assets}
                    loading={assetsLoading}
                    onAssetClick={handleAssetClick}
                    onRefresh={loadAssets}
                    onCreateAsset={handleCreateAsset}
                />
            )}

            {currentTab === 1 && (
                <ProductList
                    products={products}
                    loading={productsLoading}
                    onProductClick={handleProductClick}
                    onRefresh={loadProducts}
                    onCreateProduct={handleCreateProduct}
                />
            )}

            {currentTab === 2 && (
                <ProductVariantsList
                    variants={variants}
                    loading={variantsLoading}
                    onRefresh={loadVariants}
                    onCreate={handleCreateVariant}
                />
            )}

            {/* Create Asset Form */}
            <CreateAssetForm
                open={createAssetOpen}
                onSubmit={handleSubmitAsset}
                onClose={() => setCreateAssetOpen(false)}
                loading={createAssetLoading}
                error={createAssetError}
            />

            {/* Create Product Form */}
            <CreateProductForm
                open={createProductOpen}
                onSubmit={handleSubmitProduct}
                onClose={() => setCreateProductOpen(false)}
                loading={createProductLoading}
                error={createProductError}
            />

            {/* Create Product Variant Form */}
            <CreateProductVariantForm
                open={createVariantOpen}
                onSubmit={handleSubmitVariant}
                onClose={() => setCreateVariantOpen(false)}
            />
        </Box>
    );
}
