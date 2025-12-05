import { useParams, useNavigate } from "react-router-dom";
import { useState, useEffect, useCallback } from "react";
import { Container, Box, Button, CircularProgress, Alert } from "@mui/material";
import { ArrowBack as BackIcon } from "@mui/icons-material";
import BatchDetail from "../components/BatchDetail";
import useBatches from "../hooks/useBatches";
import type { Batch } from "../types/batch";
import type { BatchActivity } from "../types/batchActivity";

export default function BatchDetailPage() {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    const [batch, setBatch] = useState<Batch | null>(null);
    const [activities, setActivities] = useState<BatchActivity[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const { fetchBatchById } = useBatches();

    // Fetch batch data when component mounts or ID changes
    const loadBatch = useCallback(async () => {
        if (!id) {
            setError("No batch ID provided");
            return;
        }

        try {
            setIsLoading(true);
            setError(null);
            const { batch: batchData, activities: batchActivities } = await fetchBatchById(id);
            setBatch(batchData);
            setActivities(batchActivities);
            if (!batchData) {
                setError("Batch not found");
            }
        } catch (err) {
            setError("Failed to load batch details");
            console.error("Error loading batch:", err);
        } finally {
            setIsLoading(false);
        }
    }, [id, fetchBatchById]);

    useEffect(() => {
        loadBatch();
    }, [loadBatch]);

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
    return <BatchDetail batch={batch} activities={activities} onRefresh={loadBatch} />;
}
