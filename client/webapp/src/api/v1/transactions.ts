import apiClient from "../client";
import type { Transaction, NewTransaction } from "../../types/transaction";

interface GetBatchTransactionsResponse {
    transactions: Transaction[];
}

export async function getBatchTransactions(batchId: string): Promise<Transaction[]> {
    const response: GetBatchTransactionsResponse = await apiClient.get(`/v1/batches/${batchId}/transactions`);
    return response.transactions;
}

interface CreateTransactionResponse {
    transaction: Transaction;
}

export async function createTransaction(transaction: NewTransaction): Promise<Transaction> {
    const response: CreateTransactionResponse = await apiClient.post("/v1/transactions", {
        transaction,
    });
    return response.transaction;
}
