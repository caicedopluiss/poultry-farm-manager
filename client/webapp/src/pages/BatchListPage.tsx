import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import type { Batch, NewBatch } from "@/types/batch";
import type { Vendor } from "@/types/vendor";
import BatchList from "@/components/BatchList";
import CreateBatchForm from "@/components/CreateBatchForm";
import { useBatchesContext } from "@/hooks/useBatchesContext";
import useBatches from "@/hooks/useBatches";
import { getVendors } from "@/api/v1/vendors";

export default function BatchListPage() {
    const navigate = useNavigate();
    const { batches, loading, refreshBatches } = useBatchesContext();
    const { createBatch, loading: createLoading } = useBatches();
    const [createModalOpen, setCreateModalOpen] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [vendors, setVendors] = useState<Vendor[]>([]);
    const [vendorsLoading, setVendorsLoading] = useState(false);

    // Fetch batches on component mount
    useEffect(() => {
        refreshBatches();
    }, [refreshBatches]);

    // Fetch vendors for the create form
    useEffect(() => {
        const loadVendors = async () => {
            setVendorsLoading(true);
            try {
                const response = await getVendors();
                setVendors(response.vendors);
            } catch (error) {
                console.error("Failed to load vendors:", error);
            } finally {
                setVendorsLoading(false);
            }
        };
        loadVendors();
    }, []);

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
                vendors={vendors}
                vendorsLoading={vendorsLoading}
            />
        </>
    );
}
