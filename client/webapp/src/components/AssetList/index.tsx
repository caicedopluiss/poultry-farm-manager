import { Box, Container, Typography, useTheme, useMediaQuery, CircularProgress, Button } from "@mui/material";
import { Refresh as RefreshIcon, Add as AddIcon } from "@mui/icons-material";
import AssetCard from "@/components/AssetCard";
import type { Asset } from "@/types/inventory";

interface Props {
    assets?: Asset[];
    loading?: boolean;
    onAssetClick?: (asset: Asset) => void;
    onRefresh?: () => void;
    onCreateAsset?: () => void;
}

export default function AssetList({ assets = [], loading = false, onAssetClick, onRefresh, onCreateAsset }: Props) {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("sm"));
    const isExtraSmall = useMediaQuery("(max-width:400px)");

    const handleAssetClick = (asset: Asset) => {
        if (onAssetClick) {
            onAssetClick(asset);
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
                        color: theme.palette.primary.main,
                        flexShrink: 0,
                    }}
                >
                    Assets
                </Typography>

                <Box
                    sx={{
                        display: "flex",
                        flexDirection: { xs: "column", sm: "row" },
                        gap: { xs: 1.5, sm: 2 },
                        width: { xs: "100%", sm: "auto" },
                    }}
                >
                    {onCreateAsset && (
                        <Button
                            variant="contained"
                            startIcon={<AddIcon />}
                            onClick={onCreateAsset}
                            disabled={loading}
                            size={isMobile ? "medium" : "medium"}
                            sx={{
                                minWidth: { xs: "100%", sm: "auto" },
                                whiteSpace: "nowrap",
                                px: { xs: 2, sm: 2 },
                            }}
                        >
                            {isExtraSmall ? "Create" : isMobile ? "Create Asset" : "Create New Asset"}
                        </Button>
                    )}
                    {onRefresh && (
                        <Button
                            variant="outlined"
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
                    <CircularProgress />
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
                        {assets.map((asset) => (
                            <AssetCard key={asset.id} asset={asset} onClick={handleAssetClick} />
                        ))}
                    </Box>

                    {assets.length === 0 && !loading && (
                        <Box
                            sx={{
                                textAlign: "center",
                                py: 8,
                                color: theme.palette.text.secondary,
                            }}
                        >
                            <Typography variant="h6" gutterBottom>
                                No assets found
                            </Typography>
                            <Typography variant="body2">Start by creating your first asset</Typography>
                        </Box>
                    )}
                </>
            )}
        </Container>
    );
}
