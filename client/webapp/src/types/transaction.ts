export type TransactionType = "Income" | "Expense";

export interface Transaction {
    id: string;
    title: string;
    date: string;
    type: TransactionType;
    unitPrice: number;
    quantity: number | null;
    transactionAmount: number;
    totalAmount: number;
    notes: string | null;
    productVariantId: string | null;
    productVariantName: string | null;
    batchId: string | null;
    batchName: string | null;
    vendorId: string | null;
    vendorName: string | null;
    customerId: string | null;
    customerName: string | null;
}

export interface NewTransaction {
    title: string;
    dateClientIsoString: string;
    type: string;
    unitPrice: number;
    quantity: number | null;
    transactionAmount: number;
    notes: string | null;
    productVariantId: string | null;
    batchId: string | null;
    vendorId: string | null;
    customerId: string | null;
}
