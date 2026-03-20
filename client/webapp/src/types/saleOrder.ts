export type SaleOrderStatus = "Pending" | "PartiallyPaid" | "Paid" | "Cancelled";

export interface SaleOrderItem {
    id: string;
    weight: number;
    unitOfMeasure: string;
    processedDate: string;
}

export interface SaleOrderPayment {
    transactionId: string;
    date: string;
    amount: number;
    notes: string | null;
}

export interface SaleOrder {
    id: string;
    batchId: string;
    batchName: string | null;
    customerId: string;
    customerFullName: string;
    date: string;
    status: SaleOrderStatus;
    notes: string | null;
    pricePerUnit: number;
    items: SaleOrderItem[];
    payments: SaleOrderPayment[];
    totalWeight: number;
    totalAmount: number;
    totalPaid: number;
    pendingAmount: number;
}

export interface NewSaleOrderItem {
    weight: number;
    unitOfMeasure: string;
    processedDateClientIsoString: string;
}

export interface NewSaleOrder {
    batchId: string;
    customerId: string;
    dateClientIsoString: string;
    pricePerUnit: number;
    items: NewSaleOrderItem[];
    notes: string | null;
}

export interface NewSaleOrderPayment {
    dateClientIsoString: string;
    amount: number;
    notes: string | null;
}
