import { useContext } from "react";
import { BatchesContext, type BatchesContextValue } from "@/contexts/batches";

export function useBatchesContext(): BatchesContextValue {
    const context = useContext(BatchesContext);
    if (context === undefined) {
        throw new Error("useBatchesContext must be used within a BatchesProvider");
    }
    return context;
}
