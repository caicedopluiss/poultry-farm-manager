import { Box, Container, Typography, useTheme, useMediaQuery } from "@mui/material";
import BatchCard from "../BatchCard";
import type { Batch } from "../../types/batch";

// Static mock data based on your example
const mockBatches: Batch[] = [
    {
        id: "2dfeee93-b074-438f-affc-5395eab0a4f0",
        name: "2025001",
        breed: null,
        status: "Active",
        startDate: "2025-09-25T02:18:42Z",
        initialPopulation: 10,
        maleCount: 0,
        femaleCount: 0,
        unsexedCount: 10,
        population: 10,
        shed: "Shed A-1",
    },
    {
        id: "e29fc5f0-07f7-4292-a77d-dd20fd93aa2c",
        name: "2025002",
        breed: null,
        status: "Active",
        startDate: "2025-09-25T02:18:42Z",
        initialPopulation: 10,
        maleCount: 0,
        femaleCount: 0,
        unsexedCount: 10,
        population: 10,
        shed: "Shed B-2",
    },
    // Add some additional mock data for variety
    {
        id: "3ef8b123-c185-549e-bg04-6496fab1b5f1",
        name: "2025003",
        breed: "Rhode Island Red",
        status: "Planned",
        startDate: "2025-10-15T08:00:00Z",
        initialPopulation: 50,
        maleCount: 25,
        femaleCount: 25,
        unsexedCount: 0,
        population: 50,
        shed: "Shed C-1",
    },
    {
        id: "4fg9c234-d296-65af-ch15-7507gbc2c6g2",
        name: "2024050",
        breed: "Leghorn",
        status: "Completed",
        startDate: "2024-05-10T06:30:00Z",
        initialPopulation: 100,
        maleCount: 45,
        femaleCount: 50,
        unsexedCount: 5,
        population: 85,
        shed: "Shed A-3",
    },
    // Add examples for ForSale and Canceled statuses
    {
        id: "5gh0d345-e3a7-76bg-di26-8618hcd3d7h3",
        name: "2025004",
        breed: "Sussex",
        status: "ForSale",
        startDate: "2025-08-01T07:00:00Z",
        initialPopulation: 75,
        maleCount: 35,
        femaleCount: 40,
        unsexedCount: 0,
        population: 72,
        shed: "Shed D-1",
    },
    {
        id: "6hi1e456-f4b8-87ch-ej37-9729ide4e8i4",
        name: "2025005",
        breed: "Bantam",
        status: "Canceled",
        startDate: "2025-11-01T08:30:00Z",
        initialPopulation: 25,
        maleCount: 12,
        femaleCount: 13,
        unsexedCount: 0,
        population: 0,
        shed: null, // Some batches might not have shed assigned
    },
];

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
