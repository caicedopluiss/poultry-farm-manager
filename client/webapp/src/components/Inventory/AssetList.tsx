import { Grid, Typography, Box } from "@mui/material";
import { Inventory2 as EmptyIcon } from "@mui/icons-material";
import type { Asset } from "@/types/inventory";
import AssetCard from "./AssetCard";

interface AssetListProps {
    assets: Asset[];
    onEdit: (asset: Asset) => void;
}

export default function AssetList({ assets, onEdit }: AssetListProps) {
    if (assets.length === 0) {
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
                    No assets found
                </Typography>
                <Typography variant="body2" color="text.secondary">
                    Click "Add Asset" to create your first asset
                </Typography>
            </Box>
        );
    }

    return (
        <Grid container spacing={3}>
            {assets.map((asset) => (
                <Grid size={{ xs: 12, sm: 6, md: 4 }} key={asset.id}>
                    <AssetCard asset={asset} onEdit={onEdit} />
                </Grid>
            ))}
        </Grid>
    );
}
