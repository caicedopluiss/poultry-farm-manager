import { Box, Container, Typography, useTheme, useMediaQuery, CircularProgress, Button } from "@mui/material";
import { Refresh as RefreshIcon, Add as AddIcon } from "@mui/icons-material";
import BatchCard from "@/components/BatchCard";
import type { Batch } from "@/types/batch";

interface Props {
    batches?: Batch[];
    loading?: boolean;
    onBatchClick?: (batch: Batch) => void;
    onRefresh?: () => void;
    onCreateBatch?: () => void;
}

export default function BatchList({ batches = [], loading = false, onBatchClick, onRefresh, onCreateBatch }: Props) {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("sm"));
    const isExtraSmall = useMediaQuery("(max-width:400px)");

    const handleBatchClick = (batch: Batch) => {
        if (onBatchClick) {
            onBatchClick(batch);
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
                    Batches
                </Typography>

                <Box
                    sx={{
                        display: "flex",
                        flexDirection: { xs: "column", sm: "row" },
                        gap: { xs: 1.5, sm: 2 },
                        width: { xs: "100%", sm: "auto" },
                    }}
                >
                    {onCreateBatch && (
                        <Button
                            variant="contained"
                            startIcon={<AddIcon />}
                            onClick={onCreateBatch}
                            disabled={loading}
                            size={isMobile ? "medium" : "medium"}
                            sx={{
                                minWidth: { xs: "100%", sm: "auto" },
                                whiteSpace: "nowrap",
                                px: { xs: 2, sm: 2 },
                            }}
                        >
                            {isExtraSmall ? "Create" : isMobile ? "Create Batch" : "Create New Batch"}
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
                                xs: "1fr", // 1 column on mobile
                                sm: "repeat(2, 1fr)", // 2 columns on small screens
                                md: "repeat(3, 1fr)", // 3 columns on medium screens
                                lg: "repeat(4, 1fr)", // 4 columns on large screens
                                xl: "repeat(5, 1fr)", // 5 columns on extra large screens
                            },
                            gap: 3,
                            mb: 4,
                            justifyItems: "center", // Center the cards horizontally
                            justifyContent: "center", // Center the entire grid
                        }}
                    >
                        {batches.map((batch) => (
                            <BatchCard key={batch.id} batch={batch} onClick={handleBatchClick} />
                        ))}
                    </Box>

                    {batches.length === 0 && !loading && (
                        <Box
                            sx={{
                                textAlign: "center",
                                py: 8,
                                color: theme.palette.text.secondary,
                            }}
                        >
                            <Typography variant="h6" gutterBottom>
                                No batches found
                            </Typography>
                            <Typography variant="body2">Start by creating your first batch</Typography>
                        </Box>
                    )}
                </>
            )}
        </Container>
    );
}
