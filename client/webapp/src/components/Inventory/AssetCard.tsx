import { Card, CardContent, CardActions, Typography, Chip, IconButton, Box, Tooltip } from "@mui/material";
import { Edit as EditIcon } from "@mui/icons-material";
import type { Asset } from "@/types/inventory";
import { AssetStatus } from "@/types/inventory";

interface AssetCardProps {
    asset: Asset;
    onEdit: (asset: Asset) => void;
}

export default function AssetCard({ asset, onEdit }: AssetCardProps) {
    // Get the latest state (most recent)
    const latestState = asset.states && asset.states.length > 0 ? asset.states[asset.states.length - 1] : null;

    const getStatusColor = (status: string) => {
        switch (status) {
            case AssetStatus.Available:
                return "success";
            case AssetStatus.InUse:
                return "primary";
            case AssetStatus.Damaged:
                return "error";
            case AssetStatus.UnderMaintenance:
                return "warning";
            case AssetStatus.Obsolete:
            case AssetStatus.Disposed:
            case AssetStatus.Sold:
            case AssetStatus.Leased:
            case AssetStatus.Lost:
                return "default";
            default:
                return "default";
        }
    };

    if (!latestState) {
        return null;
    }

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
                <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", mb: 2 }}>
                    <Tooltip title={asset.name} arrow>
                        <Typography
                            variant="subtitle1"
                            component="div"
                            sx={{
                                fontWeight: "bold",
                                overflow: "hidden",
                                textOverflow: "ellipsis",
                                whiteSpace: "nowrap",
                                flexGrow: 1,
                                mr: 1,
                            }}
                        >
                            {asset.name}
                        </Typography>
                    </Tooltip>
                    <Chip
                        label={latestState.status}
                        color={getStatusColor(latestState.status)}
                        size="small"
                        sx={{ flexShrink: 0 }}
                    />
                </Box>

                {asset.description && (
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
                        {asset.description}
                    </Typography>
                )}

                {latestState.location && (
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                        <strong>Location:</strong> {latestState.location}
                    </Typography>
                )}

                <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                    <strong>Quantity:</strong> {latestState.quantity}
                </Typography>
            </CardContent>

            <CardActions sx={{ justifyContent: "flex-end", px: 2, pb: 2 }}>
                <Tooltip title="Edit Asset">
                    <IconButton size="small" color="primary" onClick={() => onEdit(asset)}>
                        <EditIcon />
                    </IconButton>
                </Tooltip>
            </CardActions>
        </Card>
    );
}
