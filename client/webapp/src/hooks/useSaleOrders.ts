import { useState, useCallback } from "react";
import {
    getBatchSaleOrders,
    createSaleOrder as apiCreateSaleOrder,
    addSaleOrderPayment as apiAddPayment,
    cancelSaleOrder as apiCancelSaleOrder,
} from "@/api/v1/saleOrders";
import type { SaleOrder, NewSaleOrder, NewSaleOrderPayment } from "@/types/saleOrder";
import type { ApiClientError } from "@/api/client";

export default function useSaleOrders() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const fetchBatchSaleOrders = useCallback(async (batchId: string): Promise<SaleOrder[]> => {
        try {
            setLoading(true);
            setError(null);
            return await getBatchSaleOrders(batchId);
        } catch (err) {
            const apiError = (err as ApiClientError) || {};
            const message = apiError.response?.message || "Failed to fetch sale orders";
            setError(message);
            throw err;
        } finally {
            setLoading(false);
        }
    }, []);

    const createSaleOrder = useCallback(async (newSaleOrder: NewSaleOrder): Promise<SaleOrder> => {
        try {
            setLoading(true);
            setError(null);
            return await apiCreateSaleOrder(newSaleOrder);
        } catch (err) {
            const apiError = (err as ApiClientError) || {};
            const message = apiError.response?.message || "Failed to create sale order";
            setError(message);
            throw err;
        } finally {
            setLoading(false);
        }
    }, []);

    const addPayment = useCallback(async (id: string, payment: NewSaleOrderPayment): Promise<SaleOrder> => {
        try {
            setLoading(true);
            setError(null);
            return await apiAddPayment(id, payment);
        } catch (err) {
            const apiError = (err as ApiClientError) || {};
            const message = apiError.response?.message || "Failed to add payment";
            setError(message);
            throw err;
        } finally {
            setLoading(false);
        }
    }, []);

    const cancelOrder = useCallback(async (id: string): Promise<SaleOrder> => {
        try {
            setLoading(true);
            setError(null);
            return await apiCancelSaleOrder(id);
        } catch (err) {
            const apiError = (err as ApiClientError) || {};
            const message = apiError.response?.message || "Failed to cancel sale order";
            setError(message);
            throw err;
        } finally {
            setLoading(false);
        }
    }, []);

    return { loading, error, fetchBatchSaleOrders, createSaleOrder, addPayment, cancelOrder };
}
