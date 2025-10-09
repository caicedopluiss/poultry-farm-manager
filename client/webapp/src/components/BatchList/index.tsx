import { Box, Container, Typography, useTheme, useMediaQuery } from "@mui/material";
import BatchCard from "../BatchCard";
import type { Batch } from "../../types/batch";
import { mockBatches } from "../../data/mockBatches";

interface Props {
    batches?: Batch[];
    onBatchClick?: (batch: Batch) => void;
}

export default function BatchList({ batches = mockBatches, onBatchClick }: Props) {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("sm"));

    const handleBatchClick = (batch: Batch) => {
        console.log("Batch clicked:", batch.name, batch.id);
        if (onBatchClick) {
            onBatchClick(batch);
        }
    };

    return (
        <Container maxWidth="lg" sx={{ py: 3 }}>
            <Typography
                variant={isMobile ? "h4" : "h3"}
                component="h1"
                gutterBottom
                sx={{
                    fontWeight: "bold",
                    color: theme.palette.primary.main,
                    mb: 4,
                }}
            >
                Batches
            </Typography>

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

            {batches.length === 0 && (
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
        </Container>
    );
}
