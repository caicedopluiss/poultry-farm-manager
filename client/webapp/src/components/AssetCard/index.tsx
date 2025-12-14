import { Card, CardContent, CardActionArea, Box, Typography, Chip, useTheme } from "@mui/material";
import { Inventory2 as AssetIcon } from "@mui/icons-material";
import type { Asset } from "@/types/inventory";

interface Props {
    asset: Asset;
    onClick?: (asset: Asset) => void;
}

export default function AssetCard({ asset, onClick }: Props) {
    const theme = useTheme();

    // Calculate total quantity from all states
    const totalQuantity = asset.states?.reduce((sum, state) => sum + state.quantity, 0) ?? 0;
    const availableQuantity = asset.states?.find((s) => s.status === "Available")?.quantity ?? 0;

    const handleClick = () => {
        onClick?.(asset);
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
                    borderColor: theme.palette.primary.main,
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
                        <AssetIcon sx={{ color: theme.palette.primary.main, fontSize: 24 }} />
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
                                {asset.name}
                            </Typography>
                        </Box>
                    </Box>

                    {/* Description */}
                    {asset.description && (
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
                            {asset.description}
                        </Typography>
                    )}

                    {/* Quantities */}
                    <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mt: "auto" }}>
                        <Box>
                            <Typography variant="caption" color="text.secondary" display="block">
                                Available
                            </Typography>
                            <Typography variant="h6" fontWeight={600} color="primary">
                                {availableQuantity}
                            </Typography>
                        </Box>
                        <Box sx={{ textAlign: "right" }}>
                            <Typography variant="caption" color="text.secondary" display="block">
                                Total
                            </Typography>
                            <Typography variant="h6" fontWeight={600}>
                                {totalQuantity}
                            </Typography>
                        </Box>
                    </Box>

                    {/* States Count */}
                    {asset.states && asset.states.length > 0 && (
                        <Chip
                            label={`${asset.states.length} state${asset.states.length !== 1 ? "s" : ""}`}
                            size="small"
                            sx={{ alignSelf: "flex-start" }}
                        />
                    )}
                </CardContent>
            </CardActionArea>
        </Card>
    );
}
