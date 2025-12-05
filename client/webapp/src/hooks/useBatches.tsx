import { useCallback, useState } from "react";
import { getBatchById, getBatches, postBatch } from "../api/v1/batches";
import type { Batch, NewBatch } from "../types/batch";
import type { BatchActivity } from "../types/batchActivity";
import { API_RESULT_CODE_BAD_REQUEST, API_RESULT_CODE_NOT_FOUND, type ApiClientError } from "../api/client";

interface UseBatches {
    loading: boolean;
    fetchBatches: () => Promise<Batch[]>;
    fetchBatchById: (id: string) => Promise<{ batch: Batch | null; activities: BatchActivity[] }>;
    createBatch: (batchData: NewBatch) => Promise<Batch | null>;
}

export default function useBatches(): UseBatches {
    const [loading, setLoading] = useState<boolean>(false);

    const fetchBatches = useCallback(async (): Promise<Batch[]> => {
        setLoading(true);
        try {
            const response = await getBatches();
            return response.batches;
        } catch (err) {
            console.error("Failed to fetch batches:", err);
            return [];
        } finally {
            setLoading(false);
        }
    }, []);

    const fetchBatchById = useCallback(
        async (
            id: string
        ): Promise<{
            batch: Batch | null;
            activities: BatchActivity[];
        }> => {
            setLoading(true);
            try {
                const response = await getBatchById(id);
                return {
                    batch: response.batch,
                    activities: response.activities,
                };
            } catch (err) {
                console.error("Failed to fetch batch by ID:", err);
                const apiError = (err as ApiClientError) || {};
                if (apiError.code === API_RESULT_CODE_NOT_FOUND) {
                    console.warn(apiError.response);
                }
                return { batch: null, activities: [] };
            } finally {
                setLoading(false);
            }
        },
        []
    );

    const createBatch = useCallback(async (batchData: NewBatch): Promise<Batch | null> => {
        setLoading(true);
        try {
            const response = await postBatch(batchData);
            return response.batch;
        } catch (err) {
            console.error("Failed to create batch:", err);
            const apiError = (err as ApiClientError) || {};
            if (apiError.code === API_RESULT_CODE_BAD_REQUEST) {
                console.warn("Bad request:", apiError.response);
            }
            return null;
        } finally {
            setLoading(false);
        }
    }, []);

    return {
        loading,
        fetchBatches,
        fetchBatchById,
        createBatch,
    };
}
