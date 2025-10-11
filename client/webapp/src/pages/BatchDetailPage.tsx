import { useParams, useNavigate } from "react-router-dom";
import { useState, useEffect } from "react";
import { Container, Box, Button, CircularProgress, Alert } from "@mui/material";
import { ArrowBack as BackIcon } from "@mui/icons-material";
import BatchDetail from "../components/BatchDetail";
import useBatches from "../hooks/useBatches";
import type { Batch } from "../types/batch";

export default function BatchDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [batch, setBatch] = useState<Batch | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const { fetchBatchById } = useBatches();

    // Fetch batch data when component mounts or ID changes
    useEffect(() => {
        if (!id) {
            setError("No batch ID provided");
            return;
        }

        const loadBatch = async () => {
            try {
                setIsLoading(true);
                setError(null);
                const batchData = await fetchBatchById(id);
                setBatch(batchData);
                if (!batchData) {
                    setError("Batch not found");
                }
            } catch (err) {
                setError("Failed to load batch details");
                console.error("Error loading batch:", err);
            } finally {
                setIsLoading(false);
            }
        };

        loadBatch();
    }, [id, fetchBatchById]);

    // Loading state
    if (isLoading) {
        return (
            <Container maxWidth="lg" sx={{ py: 3 }}>
                <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", py: 8 }}>
                    <CircularProgress />
                </Box>
            </Container>
        );
    }

    // Error state
    if (error || !batch) {
        return (
            <Container maxWidth="lg" sx={{ py: 3 }}>
                <Button variant="outlined" startIcon={<BackIcon />} onClick={() => navigate("/")} sx={{ mb: 2 }}>
                    Back to Batches
                </Button>

                <Alert severity="error" sx={{ mb: 3 }}>
                    {error || "Batch not found"}
                </Alert>

                <Button variant="contained" startIcon={<BackIcon />} onClick={() => navigate("/")}>
                    Back to Batches
                </Button>
            </Container>
        );
    }

    // Render the BatchDetail component with the loaded batch
    return <BatchDetail batch={batch} />;
}
