import { useState, useCallback } from "react";
import { getBatchTransactions, createTransaction as apiCreateTransaction } from "@/api/v1/transactions";
import type { Transaction, NewTransaction } from "@/types/transaction";
import type { ApiClientError } from "@/api/client";

export default function useTransactions() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const fetchBatchTransactions = useCallback(async (batchId: string): Promise<Transaction[]> => {
        try {
            setLoading(true);
            setError(null);
            const transactions = await getBatchTransactions(batchId);
            return transactions;
        } catch (err) {
            const apiError = (err as ApiClientError) || {};
            const errorMessage = apiError.response?.message || "Failed to fetch transactions";
            setError(errorMessage);
            throw err;
        } finally {
            setLoading(false);
        }
    }, []);

    const createTransaction = useCallback(async (transaction: NewTransaction): Promise<Transaction> => {
        try {
            setLoading(true);
            setError(null);
            const createdTransaction = await apiCreateTransaction(transaction);
            return createdTransaction;
        } catch (err) {
            const apiError = (err as ApiClientError) || {};
            const errorMessage = apiError.response?.message || "Failed to create transaction";
            setError(errorMessage);
            throw err;
        } finally {
            setLoading(false);
        }
    }, []);

    return {
        loading,
        error,
        fetchBatchTransactions,
        createTransaction,
    };
}
