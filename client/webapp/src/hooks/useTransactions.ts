import { useState } from "react";
import { getBatchTransactions, createTransaction as apiCreateTransaction } from "@/api/v1/transactions";
import type { Transaction, NewTransaction } from "@/types/transaction";

export default function useTransactions() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const fetchBatchTransactions = async (batchId: string): Promise<Transaction[]> => {
        try {
            setLoading(true);
            setError(null);
            const transactions = await getBatchTransactions(batchId);
            return transactions;
        } catch (err: any) {
            const errorMessage = err.response?.data?.message || "Failed to fetch transactions";
            setError(errorMessage);
            throw err;
        } finally {
            setLoading(false);
        }
    };

    const createTransaction = async (transaction: NewTransaction): Promise<Transaction> => {
        try {
            setLoading(true);
            setError(null);
            const createdTransaction = await apiCreateTransaction(transaction);
            return createdTransaction;
        } catch (err: any) {
            const errorMessage = err.response?.data?.message || "Failed to create transaction";
            setError(errorMessage);
            throw err;
        } finally {
            setLoading(false);
        }
    };

    return {
        loading,
        error,
        fetchBatchTransactions,
        createTransaction,
    };
}
