import { useCallback, useState, type ReactNode } from "react";
import type { Batch } from "@/types/batch";
import type { BatchesContextValue } from "./context";
import { BatchesContext } from "./context";
import useBatches from "@/hooks/useBatches";

interface BatchesProviderProps {
    children: ReactNode;
}

export function BatchesProvider({ children }: BatchesProviderProps) {
    const [batches, setBatches] = useState<Batch[]>([]);
    const { loading, fetchBatches } = useBatches();

    const refreshBatches = useCallback(async () => {
        try {
            const newBatches = await fetchBatches();
            setBatches(newBatches);
        } catch (err) {
            console.error("Error refreshing batches:", err);
        }
    }, [fetchBatches]);

    const contextValue: BatchesContextValue = {
        batches,
        loading,
        refreshBatches,
    };

    return <BatchesContext.Provider value={contextValue}>{children}</BatchesContext.Provider>;
}
