import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import type { Batch, NewBatch } from "@/types/batch";
import BatchList from "@/components/BatchList";
import CreateBatchForm from "@/components/CreateBatchForm";
import { useBatchesContext } from "@/hooks/useBatchesContext";
import useBatches from "@/hooks/useBatches";

export default function BatchListPage() {
    const navigate = useNavigate();
    const { batches, loading, refreshBatches } = useBatchesContext();
    const { createBatch, loading: createLoading } = useBatches();
    const [createModalOpen, setCreateModalOpen] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Fetch batches on component mount
    useEffect(() => {
        refreshBatches();
    }, [refreshBatches]);

    const handleBatchClick = (batch: Batch) => {
        navigate(`/batches/${batch.id}`);
    };

    const handleCreateBatch = () => {
        setCreateModalOpen(true);
        setError(null); // Reset error when opening modal
    };

    const handleCloseModal = () => {
        setCreateModalOpen(false);
        setError(null);
    };

    const handleSubmitBatch = async (batchData: NewBatch) => {
        setError(null);
        const result = await createBatch(batchData);
        if (result) {
            setCreateModalOpen(false);
            // Refresh the batch list to show the new batch
            refreshBatches();
        } else {
            setError("Failed to create batch. Please try again.");
        }
    };

    return (
        <>
            <BatchList
                batches={batches}
                loading={loading}
                onBatchClick={handleBatchClick}
                onRefresh={refreshBatches}
                onCreateBatch={handleCreateBatch}
            />

            <CreateBatchForm
                open={createModalOpen}
                onSubmit={handleSubmitBatch}
                onClose={handleCloseModal}
                loading={createLoading}
                error={error}
            />
        </>
    );
}
