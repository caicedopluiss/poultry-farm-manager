import { createContext } from "react";
import type { Batch } from "../../types/batch";

export interface BatchesContextValue {
    batches: Batch[];
    loading: boolean;
    refreshBatches: () => Promise<void>;
}

export const BatchesContext = createContext<BatchesContextValue | undefined>(undefined);
